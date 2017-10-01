using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("process-weather-data", "Processes Weather data")]
    internal class ProcessWeatherDataCommand : ICommand
    {
        private readonly IDataNormalizer _normalizer;

        public ProcessWeatherDataCommand()
        {
            
        }

        public ProcessWeatherDataCommand(IDataNormalizer normalizer)
        {
            _normalizer = normalizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _normalizer.ProcessWeatherData();
        }
    }
}
