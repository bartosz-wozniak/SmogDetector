using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SmogDetector.DataAccess.IRepositories;
using SmogDetector.DataAccess.Models;
using SmogDetector.Task.Business.Models.ImgwData;

namespace SmogDetector.Task.Business
{
    public class ImgwDataSynchronizer : IImgwDataSynchronizer
    {
        private readonly IRepository<ImgwClassification> _classificationRepository;
        private readonly IRepository<ImgwStation> _stationRepository;
        private readonly IRepository<ImgwWeatherData> _dataRepository;

        public ImgwDataSynchronizer(IRepository<ImgwClassification> classificationRepository, IRepository<ImgwStation> stationRepository, IRepository<ImgwWeatherData> dataRepository)
        {
            _classificationRepository = classificationRepository;
            _stationRepository = stationRepository;
            _dataRepository = dataRepository;
        }

        public void Sync()
        {
            var stations = _stationRepository.GetQueryable().ToList();
            var classifications = _classificationRepository.GetQueryable().ToList();
            var existing = _dataRepository.GetQueryable().ToList();
            foreach (var station in stations)
            {
                for (var date = new DateTime(2015, 1, 1); date < new DateTime(2015, 3, 1); date = date.AddDays(1))
                {
                    foreach (var classification in classifications)
                    {
                        var current = LoadData(station, classification, date);
                        var insert = current.Any(item => !existing.Any(it => it.ClassificationId == item.ClassificationId && it.Date == item.Date && it.StationId == item.StationId && it.CbdoCbdh == item.CbdoCbdh));
                        if (!insert) continue;
                        _dataRepository.BulkInsert(current);
                        _dataRepository.Save();
                    }
                }
            }
        }

        public void ImportClassifications()
        {
            var classifications = JsonConvert.DeserializeObject<Classification[]>(File.ReadAllText("klasyfikacje.json"));
            var existing = _classificationRepository.GetQueryable().ToList();
            foreach (var classification in classifications)
            {
                if (existing.All(item => item.Code != classification.Code) && !classification.Code.StartsWith("pole"))
                {
                    _classificationRepository.Insert(new ImgwClassification
                    {
                        Code = classification.Code,
                        Name = classification.Name,
                        Unit = classification.Unit
                    });
                }
            }
            _classificationRepository.Save();
        }

        public void ImportStations()
        {
            var stations = JsonConvert.DeserializeObject<Station[]>(File.ReadAllText("stacje.json"));
            var existing = _stationRepository.GetQueryable().ToList();
            foreach (var station in stations)
            {
                if (existing.All(item => item.Code != station.Code))
                {
                    _stationRepository.Insert(new ImgwStation
                    {
                        Code = station.Code,
                        Name = station.Name
                    });
                }
            }
            _stationRepository.Save();
        }

        private static IList<ImgwWeatherData> LoadData(ImgwStation station, ImgwClassification classification, DateTime date)
        {
            Thread.Sleep(80);
            const string cbdoCbdh = "cbdh";
            var req = (HttpWebRequest)WebRequest.Create(CreateUrl(station.Code, classification.Code, date, cbdoCbdh));
            req.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("bartosz.wozniak.94@gmail.com" + ":" + "b4a21fc0")));
            var resp = (HttpWebResponse)req.GetResponse();
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var results = sr.ReadToEnd();
                var rows = results.Split('\r').Skip(1);
                var ret = new List<ImgwWeatherData>();
                foreach (var row in rows)
                {
                    var columns = row.Split(';');
                    if (columns[0] == "\n" || string.IsNullOrWhiteSpace(columns[1]) || columns[1] == "/") continue;
                    DateTime parsedDate;
                    double parsedValue;
                    DateTime? dateValue = null;
                    double? value = null;
                    if (double.TryParse(columns[1], out parsedValue))
                    {
                        value = parsedValue;
                    }
                    else if (DateTime.TryParse(columns[1], out parsedDate))
                    {
                        dateValue = parsedDate;
                    }
                    ret.Add(new ImgwWeatherData
                    {
                        ClassificationId = classification.Id,
                        StationId = station.Id,
                        CbdoCbdh = cbdoCbdh,
                        Date = DateTime.Parse(columns[0]),
                        Status = int.Parse(columns[2]),
                        Value = value,
                        ValueDate = dateValue
                    });
                }
                return ret;
            }
        }

        private static string CreateUrl(string station, string classification, DateTime date, string cbdoCbdh)
        {
            return "https://dane.imgw.pl/1.0/pomiary/" + cbdoCbdh + "/" + station + "-" + classification + "/doba/" + date.ToString("yyyy-MM-dd") + "?format=csv";
        }
    }
}
