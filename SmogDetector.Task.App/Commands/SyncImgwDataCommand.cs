using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("sync-imgw-data", "Syncs weather data from Imgw")]
    internal class SyncImgwDataCommand : ICommand
    {
        private readonly IImgwDataSynchronizer _synchronizer;

        public SyncImgwDataCommand()
        {
            
        }

        public SyncImgwDataCommand(IImgwDataSynchronizer synchronizer)
        {
            _synchronizer = synchronizer;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _synchronizer.Sync();
        }
    }
}
