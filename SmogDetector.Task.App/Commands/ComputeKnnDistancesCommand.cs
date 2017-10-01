using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("compute-knn-distances", "Computes KNN Distances between all testing objects and all training objects")]
    internal class ComputeKnnDistancesCommand : ICommand
    {
        private readonly IKnnComputation _computation;

        public ComputeKnnDistancesCommand()
        {
            
        }

        public ComputeKnnDistancesCommand(IKnnComputation computation)
        {
            _computation = computation;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _computation.ComputeDistances();
        }
    }
}
