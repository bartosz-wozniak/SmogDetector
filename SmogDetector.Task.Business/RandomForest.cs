using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmogDetector.DataAccess.IRepositories;
using SmogDetector.DataAccess.Models;
using SmogDetector.Task.Business.Models.RandomForest;
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable UnreachableCode
#pragma warning disable 162

namespace SmogDetector.Task.Business
{
    public class RandomForest : IRandomForest
    {
        #region Private

        private readonly IRepository<NormalizedWeatherData> _weatherRepository;

        private readonly IRepository<ProcessedSmogData> _processedSmogRepository;

        private readonly IRepository<RandomForestResult> _randomForestResultRepository;

        private readonly IRepository<RandomForestStats> _randomForestStatRepository;

        private static Random _random;

        public RandomForest(IRepository<NormalizedWeatherData> weatherRepository, IRepository<ProcessedSmogData> processedRepository, IRepository<RandomForestResult> randomForestResultRepository, IRepository<RandomForestStats> randomForestStatRepository)
        {
            _weatherRepository = weatherRepository;
            _processedSmogRepository = processedRepository;
            _randomForestResultRepository = randomForestResultRepository;
            _randomForestStatRepository = randomForestStatRepository;
            _random = new Random(Seed);
        }

        #endregion

        #region Parameters

        private const int Seed = 1487;

        private const bool DiscretizeContinuousData = false;

        private const bool UseExpotentialMovingAverage = true;

        private const int RepeatBuildingTree = 10;

        private const int EmaDays = 24;

        private const int DiscreteClassesCounter = 4;

        private const double TrainingSetSelectionCount = 2.0 / 3;

        private static readonly int[] DiscreteClassifications = { 459, 131, 129, 127, 123, 241, 736, 764, 766, 430 };

        private static readonly int[] Trees = { 1, 5, 10, 25, 50, 100, 200, 500, 1000 };

        private static readonly Func<int, int> AttributesSelection = (int count) => (int)Math.Floor(Math.Sqrt(count));

        #endregion

        #region InterfaceMethods

        public void Test()
        {
            foreach (var tree in Trees)
            {
                Run(tree);
            }
        }

        public void Run(int trees = 100)
        {
            InitializeData(out List<DataModel> trainingSetData, out List<DataModel> testingSetData, out List<int> attributes, out List<int> classes);
            var root = BuildRandomForest(attributes, trainingSetData, classes, trees, out double oobError);
            foreach (var testingItem in testingSetData)
            {
                testingItem.PredictedSmogClass = ClasifyForest(root, testingItem, classes);
            }
            Save(trees, testingSetData, oobError);
        }

        #endregion

        #region Initialization

        private void InitializeData(out List<DataModel> trainingSetData, out List<DataModel> testingSetData, out List<int> attributes, out List<int> classes)
        {
            var smogData = _processedSmogRepository.GetQueryable().ToList();
            var weatherData = UseExpotentialMovingAverage ? ComputeExpotentialMovingAverage(_weatherRepository.GetQueryable().ToList()) : _weatherRepository.GetQueryable().ToList();
            var trainingSet = weatherData.Where(item => item.Date.Day < 8 || item.Date.Day > 14).GroupBy(item => item.Date).Select(item => new { Date = item.Key, Items = item.ToList() }).ToList();
            var testingSet = weatherData.Where(item => item.Date.Day >= 8 && item.Date.Day <= 14).GroupBy(item => item.Date).Select(item => new { Date = item.Key, Items = item.ToList() }).ToList();
            trainingSetData = trainingSet.Select(trainingItem => new DataModel
            {
                Date = trainingItem.Date,
                Classifications = trainingItem.Items.ToDictionary(item => item.Classification, item => item.Value),
                // ReSharper disable once PossibleInvalidOperationException
                SmogClass = smogData.First(item => item.Date == trainingItem.Date).Class.Value
            }).ToList();
            testingSetData = testingSet.Select(testItem => new DataModel
            {
                Date = testItem.Date,
                Classifications = testItem.Items.ToDictionary(item => item.Classification, item => item.Value),
                // ReSharper disable once PossibleInvalidOperationException
                SmogClass = smogData.First(item => item.Date == testItem.Date).Class.Value
            }).ToList();
            attributes = trainingSetData[0].Classifications.Keys.ToList();
            classes = trainingSetData.Select(item => item.SmogClass).Distinct().ToList();
            foreach (var attribute in attributes.ToList())
            {
                if (trainingSetData.Select(item => item.Classifications[attribute]).Distinct().Count() < 2)
                {
                    attributes.Remove(attribute);
                }
            }
        }

        private static List<NormalizedWeatherData> ComputeExpotentialMovingAverage(IEnumerable<NormalizedWeatherData> data)
        {
            var ret = new List<NormalizedWeatherData>();
            var dictionary = data.GroupBy(item => item.Classification).ToDictionary(item => item.Key, item => item.OrderBy(it => it.Date).ToList());
            foreach (var weatherData in dictionary)
            {
                var ema = new NormalizedWeatherData
                {
                    Classification = weatherData.Key,
                    Value = weatherData.Value.Take(EmaDays).Where(item => item.Value.HasValue).Average(item => item.Value) ?? weatherData.Value.First(item => item.Value.HasValue).Value ?? 0,
                    Date = weatherData.Value.Skip(EmaDays - 1).First().Date
                };
                ret.Add(ema);
                const double alpha = 2.0 / (EmaDays + 1);
                const double p = 1 - alpha;
                for (var i = EmaDays; i < weatherData.Value.Count; ++i)
                {
                    var value = weatherData.Value[i].Value;
                    if (value != null)
                    {
                        var nextEma = new NormalizedWeatherData
                        {
                            Date = weatherData.Value[i].Date,
                            Value = value.Value * alpha + ema.Value * p,
                            Classification = weatherData.Key
                        };
                        ema = nextEma;
                    }
                    else
                    {
                        var nextEma = new NormalizedWeatherData
                        {
                            Date = weatherData.Value[i].Date,
                            Value = ema.Value * alpha + ema.Value * p,
                            Classification = weatherData.Key
                        };
                        ema = nextEma;
                    }
                    ret.Add(ema);
                }
            }
            return ret;
        }

        private void Save(int trees, IEnumerable<DataModel> testingSetData, double oobError)
        {
            var desc = DateTime.Now.ToString(CultureInfo.InvariantCulture) + ";" + UseExpotentialMovingAverage + ";" + DiscretizeContinuousData + ";" + EmaDays + ";" + RepeatBuildingTree + ";" + trees + ";" + oobError;
            _randomForestResultRepository.BulkInsert(testingSetData.Select(item => new RandomForestResult
            {
                CorrectClass = item.SmogClass,
                PredictedClass = item.PredictedSmogClass,
                TestingSet = item.Date,
                Description = desc
            }));
            _randomForestResultRepository.Save();
            _randomForestStatRepository.Insert(new RandomForestStats
            {
                Oob = oobError,
                Trees = trees,
                Description = desc
            });
            _randomForestStatRepository.Save();
        }

        #endregion

        #region BuildingRandomForest

        private static List<Node> BuildRandomForest(IReadOnlyCollection<int> attributes, IReadOnlyCollection<DataModel> trainingSetData, IReadOnlyList<int> classes, int treesNumber, out double oobErrorSum)
        {
            var forest = new List<Node>();
            oobErrorSum = .0;
            for (var index = 0; index < treesNumber; ++index)
            {
                forest.Add(BuildRandomTree(attributes, trainingSetData, classes, out double oobError));
                oobErrorSum += oobError;
            }
            oobErrorSum /= treesNumber;
            return forest;
        }

        private static Node BuildRandomTree(IReadOnlyCollection<int> attributes, IReadOnlyCollection<DataModel> trainingSetData, IReadOnlyList<int> classes, out double oobError)
        {
            var trainingSet = trainingSetData.OrderBy(item => _random.Next()).Take((int)Math.Floor(trainingSetData.Count * TrainingSetSelectionCount)).ToList();
            var testingSet = trainingSetData.Where(item => !trainingSet.Contains(item)).ToList();
            var minOobError = double.MaxValue;
            var bestAttributes = new List<int>();
            for (var i = 0; i < RepeatBuildingTree; ++i)
            {
                var attr = attributes.OrderBy(item => _random.Next()).Take(AttributesSelection(attributes.Count)).ToList();
                var root = BuildTreeC45(attr, trainingSet, classes);
                oobError = ComputeOobErrorForTree(root, testingSet);
                if (!(oobError < minOobError))
                {
                    continue;
                }
                minOobError = oobError;
                bestAttributes = attr;
            }
            var ret = BuildTreeC45(bestAttributes, trainingSet, classes);
            oobError = ComputeOobErrorForTree(ret, testingSet);
            return ret;
        }

        private static double ComputeOobErrorForTree(Node root, IReadOnlyCollection<DataModel> testingSet)
        {
            var wrongClassified = (from dataModel in testingSet let cl = ClasifyTree(root, dataModel).Class where cl != dataModel.SmogClass select dataModel).Count();
            return (double)wrongClassified / testingSet.Count;
        }

        private static Node BuildTreeC45(IReadOnlyCollection<int> nonCategoricalAttributes, IReadOnlyList<DataModel> trainingSet, IReadOnlyList<int> classes)
        {
            if (trainingSet == null || trainingSet.Count == 0)
            {
                return new Node
                {
                    Children = null,
                    Class = -1,
                    ClassificationId = -1,
                    TrainingSetCounter = 0
                };
            }
            if (trainingSet.All(item => item.SmogClass == trainingSet[0].SmogClass))
            {
                return new Node
                {
                    Children = null,
                    Class = trainingSet[0].SmogClass,
                    ClassificationId = -1,
                    TrainingSetCounter = trainingSet.Count
                };
            }
            if (nonCategoricalAttributes == null || nonCategoricalAttributes.Count == 0)
            {
                return new Node
                {
                    Children = null,
                    ClassificationId = -1,
                    Class = trainingSet.GroupBy(item => item.SmogClass).OrderByDescending(item => item.ToList().Count).First().Key,
                    TrainingSetCounter = trainingSet.Count
                };
            }
            return DiscretizeContinuousData ? Version1MakeContinuousValuesDiscrete(nonCategoricalAttributes, trainingSet, classes) : Version2ProcessContinuousValues(nonCategoricalAttributes, trainingSet, classes);
        }

        private static Node Version1MakeContinuousValuesDiscrete(IReadOnlyCollection<int> nonCategoricalAttributes, IReadOnlyCollection<DataModel> trainingSet, IReadOnlyList<int> classes)
        {
            var attribute = Gain(nonCategoricalAttributes, trainingSet, classes);
            if (attribute == 0)
            {
                return new Node
                {
                    Children = null,
                    ClassificationId = -1,
                    TrainingSetCounter = trainingSet.Count,
                    Class = trainingSet.GroupBy(item => item.SmogClass).OrderByDescending(item => item.ToList().Count).First().Key
                };
            }
            return DiscreteClassifications.Contains(attribute) ? ProcessDiscreteValues(nonCategoricalAttributes, trainingSet, classes, attribute, false) : ProcessDiscreteValues(nonCategoricalAttributes, trainingSet, classes, attribute);
        }

        private static Node Version2ProcessContinuousValues(IReadOnlyCollection<int> nonCategoricalAttributes, IReadOnlyCollection<DataModel> trainingSet, IReadOnlyList<int> classes)
        {
            var attribute = GainContinuousValues(nonCategoricalAttributes, trainingSet, classes, out double splitPoint);
            if (attribute == 0)
            {
                return new Node
                {
                    Children = null,
                    ClassificationId = -1,
                    TrainingSetCounter = trainingSet.Count,
                    Class = trainingSet.GroupBy(item => item.SmogClass).OrderByDescending(item => item.ToList().Count).First().Key
                };
            }
            if (DiscreteClassifications.Contains(attribute))
            {
                return ProcessDiscreteValues(nonCategoricalAttributes, trainingSet, classes, attribute, false);
            }
            var attributeValues = new Dictionary<double, List<DataModel>>
            {
                {
                    splitPoint, trainingSet.Where(item => item.Classifications[attribute] <= splitPoint).ToList()
                },
                {
                    trainingSet.Max(item => item.Classifications[attribute] ?? double.MinValue), trainingSet.Where(item => item.Classifications[attribute] > splitPoint).ToList()
                }
            };
            var itemsWithNull = trainingSet.Where(item => item.Classifications[attribute] == null).ToList();
            DealWithNullValues(trainingSet.Count - itemsWithNull.Count, itemsWithNull, attributeValues);
            var ret = new Node
            {
                ClassificationId = attribute,
                Class = -1,
                Children = new Dictionary<double, Node>(),
                TrainingSetCounter = trainingSet.Count
            };
            foreach (var attributeValue in attributeValues.Keys.OrderBy(item => item))
            {
                ret.Children.Add(attributeValue, BuildTreeC45(nonCategoricalAttributes.ToList(), attributeValues[attributeValue], classes));
            }
            return ret;
        }

        private static Node ProcessDiscreteValues(IReadOnlyCollection<int> nonCategoricalAttributes, IReadOnlyCollection<DataModel> trainingSet, IReadOnlyList<int> classes, int attribute, bool discretize = true)
        {
            var attributeValues = trainingSet.GroupBy(item => item.Classifications[attribute]).ToDictionary(item => item.Key ?? double.MinValue, item => item.ToList());
            var itemsWithNull = new List<DataModel>();
            if (attributeValues.ContainsKey(double.MinValue))
            {
                itemsWithNull = attributeValues[double.MinValue];
                attributeValues.Remove(double.MinValue);
            }
            if (discretize)
            {
                DiscretizeContinuousValues(attributeValues);
            }
            DealWithNullValues(trainingSet.Count - itemsWithNull.Count, itemsWithNull, attributeValues);
            var ret = new Node
            {
                ClassificationId = attribute,
                Class = -1,
                Children = new Dictionary<double, Node>(),
                TrainingSetCounter = trainingSet.Count
            };
            foreach (var attributeValue in attributeValues.Keys.OrderBy(item => item))
            {
                ret.Children.Add(attributeValue, BuildTreeC45(nonCategoricalAttributes.Where(item => item != attribute).ToList(), attributeValues[attributeValue], classes));
            }
            return ret;
        }

        private static void DealWithNullValues(int trainingSetCount, List<DataModel> itemsWithNull, Dictionary<double, List<DataModel>> attributeValues)
        {
            if (itemsWithNull == null || itemsWithNull.Count == 0)
            {
                return;
            }
            if (attributeValues.Count == 0)
            {
                attributeValues.Add(double.MinValue, itemsWithNull);
                return;
            }
            foreach (var attributeValue in attributeValues.Keys)
            {
                var p = (double)attributeValues[attributeValue].Count / trainingSetCount;
                var itemsToAddCounter = Math.Floor(p * itemsWithNull.Count);
                for (var i = 0; i < itemsToAddCounter; ++i)
                {
                    var index = _random.Next(itemsWithNull.Count);
                    attributeValues[attributeValue].Add(itemsWithNull[index]);
                    itemsWithNull.RemoveAt(index);
                }
            }
            var mostNumerous = attributeValues.OrderByDescending(item => item.Value.Count).FirstOrDefault().Key;
            attributeValues[mostNumerous].AddRange(itemsWithNull);
        }

        private static void DiscretizeContinuousValues(Dictionary<double, List<DataModel>> attributeValues)
        {
            var list = attributeValues.Keys.OrderBy(item => item).ToList();
            if (attributeValues.Count <= DiscreteClassesCounter)
            {
                return;
            }
            var indexes = new List<double>();
            for (var index = 1; index <= DiscreteClassesCounter; ++index)
            {
                indexes.Add(list[index * attributeValues.Count / DiscreteClassesCounter - 1]);
            }
            var start = double.MinValue;
            foreach (var index in indexes)
            {
                var models = new List<DataModel>();
                foreach (var attributeValuesKey in attributeValues.Keys.ToList())
                {
                    if (!(attributeValuesKey > start) || !(attributeValuesKey < index))
                    {
                        continue;
                    }
                    models.AddRange(attributeValues[attributeValuesKey]);
                    attributeValues.Remove(attributeValuesKey);
                }
                attributeValues[index].AddRange(models);
                start = index;
            }
        }

        private static int Gain(IEnumerable<int> nonCategoricalAttributes, IReadOnlyCollection<DataModel> trainingSet, IReadOnlyList<int> classes)
        {
            var bestAttribute = 0;
            var gainMax = double.MinValue;
            var infoT = InfoT(trainingSet, classes);
            foreach (var nonCategoricalAttribute in nonCategoricalAttributes)
            {
                var attributeValues = trainingSet.GroupBy(item => item.Classifications[nonCategoricalAttribute]).ToDictionary(item => item.Key ?? double.MinValue, item => item.ToList());
                attributeValues.Remove(double.MinValue);
                if (!DiscreteClassifications.Contains(nonCategoricalAttribute))
                {
                    DiscretizeContinuousValues(attributeValues);
                }
                var infoAttributeSumT = attributeValues.Sum(attributeValue => InfoT(attributeValue.Value, classes) * attributeValue.Value.Count / trainingSet.Where(item => item.Classifications[nonCategoricalAttribute].HasValue).ToList().Count);
                var gain = infoT - infoAttributeSumT;
                if (!(gain > gainMax) || attributeValues.Count < 2)
                {
                    continue;
                }
                bestAttribute = nonCategoricalAttribute;
                gainMax = gain;
            }
            return bestAttribute;
        }

        private static int GainContinuousValues(IEnumerable<int> nonCategoricalAttributes, IReadOnlyCollection<DataModel> trainingSet, IReadOnlyList<int> classes, out double divideValue)
        {
            divideValue = double.MinValue;
            var bestAttribute = 0;
            var gainMax = double.MinValue;
            var infoT = InfoT(trainingSet, classes);
            foreach (var nonCategoricalAttribute in nonCategoricalAttributes)
            {
                if (DiscreteClassifications.Contains(nonCategoricalAttribute))
                {
                    var attributeValues = trainingSet.GroupBy(item => item.Classifications[nonCategoricalAttribute]).ToDictionary(item => item.Key ?? double.MinValue, item => item.ToList());
                    attributeValues.Remove(double.MinValue);
                    var infoAttributeSumT = attributeValues.Sum(attributeValue => InfoT(attributeValue.Value, classes) * attributeValue.Value.Count / trainingSet.Where(item => item.Classifications[nonCategoricalAttribute].HasValue).ToList().Count);
                    var gain = infoT - infoAttributeSumT;
                    if (!(gain > gainMax) || attributeValues.Count < 2)
                    {
                        continue;
                    }
                    bestAttribute = nonCategoricalAttribute;
                    gainMax = gain;
                }
                else
                {
                    var attributeValues = trainingSet.GroupBy(item => item.Classifications[nonCategoricalAttribute]).Select(item => item.Key ?? double.MinValue).ToList();
                    attributeValues.Remove(double.MinValue);
                    if (attributeValues.Count < 2)
                    {
                        continue;
                    }
                    attributeValues.Remove(attributeValues.Max());
                    foreach (var attributeValue in attributeValues)
                    {
                        var subsetA = trainingSet.Where(item => item.Classifications[nonCategoricalAttribute] <= attributeValue).ToList();
                        var subsetB = trainingSet.Where(item => item.Classifications[nonCategoricalAttribute] > attributeValue).ToList();
                        var infoAttributeSumT = InfoT(subsetA, classes) * subsetA.Count / trainingSet.Where(item => item.Classifications[nonCategoricalAttribute].HasValue).ToList().Count + InfoT(subsetB, classes) * subsetB.Count / trainingSet.Where(item => item.Classifications[nonCategoricalAttribute].HasValue).ToList().Count;
                        var gain = infoT - infoAttributeSumT;
                        if (!(gain > gainMax))
                        {
                            continue;
                        }
                        bestAttribute = nonCategoricalAttribute;
                        gainMax = gain;
                        divideValue = attributeValue;
                    }
                }
            }
            return bestAttribute;
        }

        private static double InfoT(IReadOnlyCollection<DataModel> trainingSet, IEnumerable<int> classes)
        {
            var result = classes.Select(cl => (double)trainingSet.Count(item => item.SmogClass == cl) / trainingSet.Count).Aggregate(.0, (current, d) => current - (Math.Abs(d) < 0.000001 ? 0 : d * Math.Log(d, 2)));
            return trainingSet.Count == 0 ? 0 : result;
        }

        #endregion

        #region Classification

        private static int ClasifyForest(IEnumerable<Node> forest, DataModel testingItem, IEnumerable<int> classes)
        {
            var dictionary = classes.ToDictionary(item => item, item => 0);
            foreach (var treeNode in forest)
            {
                dictionary[ClasifyTree(treeNode, testingItem).Class]++;
            }
            var max = dictionary.Select(item => item.Value).Max();
            var list = dictionary.Where(item => item.Value == max).Select(item => item.Key).ToList();
            return list[_random.Next(list.Count)];
        }

        private static Node ClasifyTree(Node root, DataModel testingItem)
        {
            if (root.Children == null)
            {
                return root;
            }
            var value = testingItem.Classifications[root.ClassificationId];
            if (value == null)
            {
                var key = root.Children.OrderByDescending(item => item.Value.TrainingSetCounter).First().Key;
                // ReSharper disable once TailRecursiveCall
                return ClasifyTree(root.Children[key], testingItem);
            }
            foreach (var d in root.Children.Keys.OrderBy(item => item).ToList())
            {
                if (value.Value <= d)
                {
                    return ClasifyTree(root.Children[d], testingItem);
                }
            }
            return value.Value > root.Children.Keys.OrderByDescending(item => item).First() ? ClasifyTree(root.Children[root.Children.Keys.OrderByDescending(item => item).First()], testingItem) : null;
        }

        #endregion
    }
}
