using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("import-imgw-classifications", "Imports classifications data from Imgw json")]
    internal class ImportImgwClassificationsCommand : ICommand
    {
        private readonly IImgwDataSynchronizer _synchronizer;

        public ImportImgwClassificationsCommand()
        {
            
        }

        public ImportImgwClassificationsCommand(IImgwDataSynchronizer synchronizer)
        {
            _synchronizer = synchronizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _synchronizer.ImportClassifications();
        }
    }
}
