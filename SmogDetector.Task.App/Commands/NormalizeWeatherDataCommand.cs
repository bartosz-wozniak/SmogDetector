using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("normalize-weather-data", "Normalizes Weather data")]
    internal class NormalizeWeatherDataCommand : ICommand
    {
        private readonly IDataNormalizer _normalizer;

        public NormalizeWeatherDataCommand()
        {
            
        }

        public NormalizeWeatherDataCommand(IDataNormalizer normalizer)
        {
            _normalizer = normalizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _normalizer.NormalizeWeatherData();
        }
    }
}
