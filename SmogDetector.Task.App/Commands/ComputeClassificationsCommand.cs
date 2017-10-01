using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("compute-classifications", "Computes how many of each classification exists in weather data table")]
    internal class ComputeClassificationsCommand : ICommand
    {
        private readonly IDataNormalizer _normalizer;

        public ComputeClassificationsCommand()
        {
            
        }

        public ComputeClassificationsCommand(IDataNormalizer normalizer)
        {
            _normalizer = normalizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _normalizer.ComputeClassifications();
        }
    }
}
