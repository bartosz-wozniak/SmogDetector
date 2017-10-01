using System.Collections.Generic;

namespace SmogDetector.Task.Business.Models.RandomForest
{
    internal sealed class Node
    {
        public int Class { get; set; }

        public Dictionary<double, Node> Children { get; set; }

        public int ClassificationId { get; set; }

        public int TrainingSetCounter { get; set; }
    }
}
