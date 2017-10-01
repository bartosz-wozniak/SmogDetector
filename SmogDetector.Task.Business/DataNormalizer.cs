using System;
using System.Collections.Generic;
using System.Linq;
using SmogDetector.DataAccess.IRepositories;
using SmogDetector.DataAccess.Models;

namespace SmogDetector.Task.Business
{
    public class DataNormalizer : IDataNormalizer
    {
        private readonly IRepository<ImgwClassification> _classificationRepository;
        private readonly IRepository<ImgwWeatherData> _dataRepository;
        private readonly IRepository<SmogData> _smogRepository;
        private readonly IRepository<ProcessedSmogData> _processedSmogRepository;
        private readonly IRepository<ProcessedWeatherData> _processedWeatherRepository;
        private readonly IRepository<NormalizedWeatherData> _normalizedWeatherRepository;

        public DataNormalizer(IRepository<ImgwClassification> classificationRepository, IRepository<ImgwWeatherData> dataRepository, IRepository<SmogData> smogRepository, IRepository<ProcessedSmogData> processedRepository, IRepository<ProcessedWeatherData> processedWeatherRepository, IRepository<NormalizedWeatherData> normalizedWeatherRepository)
        {
            _classificationRepository = classificationRepository;
            _dataRepository = dataRepository;
            _smogRepository = smogRepository;
            _processedSmogRepository = processedRepository;
            _processedWeatherRepository = processedWeatherRepository;
            _normalizedWeatherRepository = normalizedWeatherRepository;
        }

        public void ComputeClassifications()
        {
            var classifications = _classificationRepository.GetQueryable().ToList();
            var weatherData = _dataRepository.GetQueryable().Where(item => item.Value != null).ToList();
            foreach (var imgwClassification in classifications)
            {
                var counter = weatherData.Count(item => item.ClassificationId == imgwClassification.Id);
                var hours = weatherData.Where(item => item.ClassificationId == imgwClassification.Id).GroupBy(item => new { item.Date.Year, item.Date.Month, item.Date.Day, item.Date.Hour }).Select(item => item.Key).Count();
                imgwClassification.Counter = counter;
                imgwClassification.HoursCounter = hours;
                imgwClassification.PerHourCounter = hours == 0 ? 0 : (double)counter / hours;
                _classificationRepository.Update(imgwClassification);
                _classificationRepository.Save();
            }
        }

        public void ProcessSmogData()
        {
            var smogData = _smogRepository.GetQueryable().Where(item => item.Station == "MpKrakAlKras" && item.Type == "PM25").OrderBy(item => item.Date).ToList();
            var processedData = _processedSmogRepository.GetQueryable().ToList();
            for (var date = new DateTime(2015, 1, 1, 0, 0, 0); date <= new DateTime(2015, 2, 28, 23, 0, 0); date = date.AddHours(1))
            {
                var current = smogData.FirstOrDefault(item => item.Date == date);
                var existing = processedData.FirstOrDefault(item => item.Date == date);
                int val;
                if (current == null)
                {
                    var val1 = smogData.FirstOrDefault(item => item.Date < date);
                    var val2 = smogData.FirstOrDefault(item => item.Date > date);
                    val = -1;
                    if (val1 != null && val2 != null)
                        val = (val1.Value + val2.Value) / 2;
                    else if (val1 != null)
                        val = val1.Value;
                    else if (val2 != null)
                        val = val2.Value;
                }
                else val = current.Value;
                var cl = ComputeClass(val);
                if (existing == null)
                    _processedSmogRepository.Insert(new ProcessedSmogData
                    {
                        Date = date,
                        Class = cl,
                        Value = val
                    });
                else
                {
                    existing.Class = cl;
                    existing.Value = val;
                    _processedSmogRepository.Update(existing);
                }
                _processedSmogRepository.Save();
            }
        }

        public void ProcessWeatherData()
        {
            var data = _dataRepository.GetQueryable().ToList();
            var processedData = _processedWeatherRepository.GetQueryable().ToList();
            var classifications = _classificationRepository.GetQueryable().Where(item => item.Counter > 1);
            var hourly = new[]
            {
                123, 136, 138, 223, 240, 241, 251, 233, 359, 423, 453, 458, 695, 693, 736, 737, 738, 751, 734, 757, 766,
                764, 430, 127, 139, 161, 162, 163, 226, 227, 228, 229, 230, 231, 442, 443, 444, 445, 450, 451, 484, 485,
                486, 487, 488, 739, 689, 683, 686, 142, 716, 429, 131, 129, 431, 432, 436, 730
            };
            var hourly6 = new[] { 469, 667, 670, 658, 664, 428, 459 };
            var hourly12 = new[] { 672, 677, 678 };
            var hourly24 = new[]
            {
                787, 661, 496, 507, 204, 205, 391, 406, 368, 835, 836, 858, 870, 879, 886, 895, 175, 208, 215, 718, 225,
                435, 219, 213, 179, 482, 762, 166, 479, 466, 759, 134, 169, 170, 174, 465, 173, 178, 165
            };
            var daily10 = new[]
            {
                493, 504, 721, 783, 524, 268, 259, 252, 387, 364, 402, 274, 280, 289, 271, 295, 298, 302, 307, 312, 318,
                315, 323, 326, 330, 339, 893, 884, 876, 867, 854, 800, 813, 829, 830, 353
            };
            var daily30 = new[]
            {
                253, 269, 263, 153, 355, 372, 395, 410, 345, 332, 327, 324, 316, 319, 313, 310, 311, 308, 304, 299, 296,
                272, 275, 530, 510, 791, 723, 499, 817, 804, 862, 842, 843, 873, 882, 897
            };
            for (var date = new DateTime(2015, 1, 1, 0, 0, 0); date <= new DateTime(2015, 2, 28, 23, 0, 0); date = date.AddHours(1))
            {
                foreach (var imgwClassification in classifications)
                {
                    double? value = null;
                    if (hourly.Contains(imgwClassification.Id))
                        value =
                            data.FirstOrDefault(
                                item => item.Date == date && item.ClassificationId == imgwClassification.Id)?.Value;
                    else if (hourly6.Contains(imgwClassification.Id))
                        value =
                            data.FirstOrDefault(
                                item => item.Date == date.AddHours(-1 * (date.Hour % 6)) && item.ClassificationId == imgwClassification.Id)?.Value;
                    else if (hourly12.Contains(imgwClassification.Id))
                        value =
                           data.FirstOrDefault(
                               item => item.Date == date.AddHours(-1 * (date.Hour % 12)) && item.ClassificationId == imgwClassification.Id)?.Value;
                    else if (hourly24.Contains(imgwClassification.Id))
                        value =
                           data.FirstOrDefault(
                               item => item.Date == date.AddHours(-1 * (date.Hour % 24)) && item.ClassificationId == imgwClassification.Id)?.Value;
                    else if (daily10.Contains(imgwClassification.Id))
                        if (date.Day < 11)
                            value =
                                data.FirstOrDefault(
                                    item => item.Date == new DateTime(date.Year, date.Month, 1, 0, 0, 0) && item.ClassificationId == imgwClassification.Id)?.Value;
                        else if (date.Day < 21)
                            value =
                                data.FirstOrDefault(
                                    item => item.Date == new DateTime(date.Year, date.Month, 11, 0, 0, 0) && item.ClassificationId == imgwClassification.Id)?.Value;
                        else
                            value =
                                data.FirstOrDefault(
                                    item => item.Date == new DateTime(date.Year, date.Month, 21, 0, 0, 0) && item.ClassificationId == imgwClassification.Id)?.Value;
                    else if (daily30.Contains(imgwClassification.Id))
                        value =
                           data.FirstOrDefault(
                               item => item.Date == new DateTime(date.Year, date.Month, 1, 0, 0, 0) && item.ClassificationId == imgwClassification.Id)?.Value;
                    var existing =
                        processedData.FirstOrDefault(
                            item => item.Date == date && item.Classification == imgwClassification.Id);
                    if (existing == null)
                        _processedWeatherRepository.Insert(new ProcessedWeatherData
                        {
                            Classification = imgwClassification.Id,
                            Date = date,
                            Value = value
                        });
                    else
                    {
                        existing.Value = value;
                        _processedWeatherRepository.Update(existing);
                    }
                    _processedWeatherRepository.Save();
                }
            }
        }

        // This method can insert duplicates
        public void NormalizeWeatherData()
        {
            var input = _processedWeatherRepository.GetQueryable().ToList();
            var classificationsMin = input.Select(item => item.Classification).Distinct().ToDictionary(item => item, item => .0);
            var classificationsMax = classificationsMin.ToDictionary(item => item.Key, item => .0);
            foreach (var i in classificationsMin.Keys.ToList())
            {
                var min = input.Where(item => item.Classification == i).Min(item => item.Value);
                var max = input.Where(item => item.Classification == i).Max(item => item.Value);
                if (min == null || max == null || Math.Abs(min.Value - max.Value) < 0.000001)
                {
                    classificationsMin.Remove(i);
                    classificationsMax.Remove(i);
                    continue;
                }
                classificationsMin[i] = min.Value;
                classificationsMax[i] = max.Value;
            }
            var ret = (from processedWeatherData in input
                where classificationsMin.ContainsKey(processedWeatherData.Classification) && classificationsMax.ContainsKey(processedWeatherData.Classification)
                let value = (processedWeatherData.Value - classificationsMin[processedWeatherData.Classification]) / (classificationsMax[processedWeatherData.Classification] - classificationsMin[processedWeatherData.Classification])
                select new NormalizedWeatherData
                {
                    Date = processedWeatherData.Date,
                    Classification = processedWeatherData.Classification,
                    Value = value
                }).ToList();
            _normalizedWeatherRepository.BulkInsert(ret.OrderBy(item => item.Date).ThenBy(item => item.Classification).ToList());
            _normalizedWeatherRepository.Save();
        }

        private static int ComputeClass(int val)
        {
            var cl = 0;
            if (val >= 0 && val < 33)
                cl = 1;
            else if (val >= 33 && val < 50)
                cl = 2;
            else if (val >= 50 && val < 80)
                cl = 3;
            else if (val >= 80)
                cl = 4;
            return cl;
        }
    }
}
