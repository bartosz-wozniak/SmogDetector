using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmogDetector.DataAccess.IRepositories;
using SmogDetector.DataAccess.Models;

namespace SmogDetector.Task.Business
{
    public class KnnComputation : IKnnComputation
    {
        private readonly IRepository<ProcessedSmogData> _processedSmogRepository;
        private readonly IRepository<NormalizedWeatherData> _normalizedWeatherRepository;
        private readonly IRepository<KnnDistancesData> _knnDistancesRepository;
        private readonly IRepository<KnnResultsData> _knnResultsRepository;

        public KnnComputation(IRepository<ProcessedSmogData> processedRepository, IRepository<NormalizedWeatherData> normalizedWeatherRepository, IRepository<KnnDistancesData> knnDistancesRepository, IRepository<KnnResultsData> knnResultsRepository)
        {
            _processedSmogRepository = processedRepository;
            _normalizedWeatherRepository = normalizedWeatherRepository;
            _knnDistancesRepository = knnDistancesRepository;
            _knnResultsRepository = knnResultsRepository;
        }

        public void ComputeDistances()
        {
            var weatherData = _normalizedWeatherRepository.GetQueryable().ToList();
            var trainingSet = weatherData.Where(item => item.Date.Day < 8 || item.Date.Day > 14).GroupBy(item => item.Date).Select(item => new { Date = item.Key, Items = item.ToList() }).ToList();
            var testingSet = weatherData.Where(item => item.Date.Day >= 8 && item.Date.Day <= 14).GroupBy(item => item.Date).Select(item => new { Date = item.Key, Items = item.ToList() }).ToList();
            var averageClassifications = weatherData.GroupBy(item => item.Classification).Select(item => new { Classification = item.Key, Average = item.Average(it => it.Value) }).ToList();
            var existing = _knnDistancesRepository.GetQueryable().ToList();
            foreach (var testingItem in testingSet)
            {
                var ret = new ConcurrentBag<KnnDistancesData>();
                Parallel.ForEach(trainingSet, trainingItem =>
                {
                    if (existing.Any(item => item.TrainingSet == trainingItem.Date && item.TestingSet == testingItem.Date)) return;
                    var distance = .0;
                    foreach (var imgwClassification in testingItem.Items)
                    {
                        var train = trainingItem.Items.First(item => item.Classification == imgwClassification.Classification)?.Value;
                        var test = imgwClassification.Value;
                        if (train == null)
                            train = averageClassifications.First(item => item.Classification == imgwClassification.Classification).Average ?? 0;
                        if (test == null)
                            test = averageClassifications.First(item => item.Classification == imgwClassification.Classification).Average ?? 0;
                        distance += Math.Pow(train.Value - test.Value, 2);
                    }
                    ret.Add(new KnnDistancesData
                    {
                        Distance = distance,
                        TestingSet = testingItem.Date,
                        TrainingSet = trainingItem.Date
                    });
                });
                _knnDistancesRepository.BulkInsert(ret.OrderBy(item => item.TrainingSet).ToList());
                _knnDistancesRepository.Save();
            }
        }

        // This method inserts duplicates
        public void FindClasses()
        {
            var ret = FindClasses(new List<int> { 1, 5, 15 });
            foreach (var item in ret)
            {
                _knnResultsRepository.Insert(new KnnResultsData
                {
                    TestingSet = item.Key,
                    CorrectClass = item.Value[0],
                    KnnClass1 = item.Value[1],
                    KnnClass5 = item.Value[5],
                    KnnClass15 = item.Value[15],
                    Description = string.Empty
                });
                _knnResultsRepository.Save();
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private Dictionary<DateTime, Dictionary<int, int>> FindClasses(IList<int> kList)
        {
            var distances = _knnDistancesRepository.GetQueryable().ToList();
            var testingSet = distances.Select(item => item.TestingSet).Distinct().ToList();
            var smogData = _processedSmogRepository.GetQueryable().ToList();
            var classCounter = smogData.Select(item => item.Class).Distinct().Count();
            var ret = new Dictionary<DateTime, Dictionary<int, int>>();
            var rnd = new Random(1487);
            foreach (var testingItem in testingSet)
            {
                ret.Add(testingItem, new Dictionary<int, int>());
                ret[testingItem] = kList.ToDictionary(item => item, item => 0);
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once PossibleInvalidOperationException
                ret[testingItem].Add(0, smogData.FirstOrDefault(item => item.Date == testingItem.Date).Class.Value);
                var top = distances.Where(item => item.TestingSet == testingItem).OrderBy(item => item.Distance).Take(kList.Max()).ToList();
                var counterClass = new int[classCounter].ToList();
                var counter = 0;
                foreach (var distancesData in top)
                {
                    counter++;
                    var cla = smogData.FirstOrDefault(item => item.Date == distancesData.TrainingSet)?.Class;
                    // ReSharper disable once PossibleInvalidOperationException
                    counterClass[cla.Value - 1]++;
                    if (!kList.Contains(counter)) continue;
                    if (rnd.Next(0, 2) == 1)
                        ret[testingItem][counter] = counterClass.IndexOf(counterClass.Max()) + 1;
                    else
                        ret[testingItem][counter] = counterClass.LastIndexOf(counterClass.Max()) + 1;
                }
            }
            return ret;
        }
    }
}
