using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("import-imgw-stations", "Imports stations data from Imgw json")]
    internal class ImportImgwStationsCommand : ICommand
    {
        private readonly IImgwDataSynchronizer _synchronizer;

        public ImportImgwStationsCommand()
        {
            
        }

        public ImportImgwStationsCommand(IImgwDataSynchronizer synchronizer)
        {
            _synchronizer = synchronizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _synchronizer.ImportStations();
        }
    }
}
