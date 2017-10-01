using System;
using System.Collections.Generic;

namespace SmogDetector.Task.Business.Models.RandomForest
{
    internal sealed class DataModel
    {
        public DateTime Date { get; set; }

        public int SmogClass { get; set; }

        public Dictionary<int, double?> Classifications { get; set; }

        public int PredictedSmogClass { get; set; }
    }
}
