using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("import-smog-data", "Imports smog data from csv file")]
    internal class ImportSmogDataCommand : ICommand
    {
        private readonly ISmogDataSynchronizer _synchronizer;

        public ImportSmogDataCommand()
        {

        }

        public ImportSmogDataCommand(ISmogDataSynchronizer synchronizer)
        {
            _synchronizer = synchronizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
            _synchronizer.Import();
        }
    }
}
