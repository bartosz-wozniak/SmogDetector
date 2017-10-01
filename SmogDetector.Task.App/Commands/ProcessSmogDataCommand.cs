using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("process-smog-data", "Processes smog data")]
    internal class ProcessSmogDataCommand : ICommand
    {
        private readonly IDataNormalizer _normalizer;

        public ProcessSmogDataCommand()
        {
            
        }

        public ProcessSmogDataCommand(IDataNormalizer normalizer)
        {
            _normalizer = normalizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _normalizer.ProcessSmogData();
        }
    }
}
