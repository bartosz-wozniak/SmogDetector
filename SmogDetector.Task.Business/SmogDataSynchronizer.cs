using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmogDetector.DataAccess.IRepositories;
using SmogDetector.DataAccess.Models;

namespace SmogDetector.Task.Business
{
    public class SmogDataSynchronizer : ISmogDataSynchronizer
    {
        private readonly IRepository<SmogData> _smogRepository;

        public SmogDataSynchronizer(IRepository<SmogData> smogRepository)
        {
            _smogRepository = smogRepository;
        }

        public void Import()
        {
            Import("PM25");
        }

        public void Import(string type)
        {
            var existing = _smogRepository.GetQueryable().ToList();
            using (var fs = File.OpenRead(type.ToLower() + ".csv"))
            using (var reader = new StreamReader(fs))
            {
                var stations = new Dictionary<int, string>();
                var line = reader.ReadLine();
                var values = line?.Split(',') ?? new string[] { };
                for (var i = 0; i < values.Length; ++i)
                    if (values[i].Contains("MpKrakAlKras"))
                        stations.Add(i, values[i]);
                reader.ReadLine();
                reader.ReadLine();
                var ret = new List<SmogData>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    values = line?.Split(',') ?? new string[] { };
                    var date = DateTime.Parse(values[0]);
                    if ((date.Month != 1 && date.Month != 2) || date.Year != 2015) continue;
                    foreach (var station in stations)
                    {
                        int val;
                        if (!int.TryParse(values[station.Key], out val)) continue;
                        if (existing.Any(item => item.Date == date && item.Station == station.Value && item.Type == type)) continue;
                        ret.Add(new SmogData
                        {
                            Station = station.Value,
                            Type = type,
                            Value = val,
                            Date = date
                        });
                    }
                }
                _smogRepository.BulkInsert(ret);
                _smogRepository.Save();
            }
        }
    }
}
