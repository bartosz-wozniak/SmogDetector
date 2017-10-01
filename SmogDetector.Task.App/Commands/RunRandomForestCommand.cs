using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("run-random-forest", "Runs Random Forest")]
    internal class RunRandomForestCommand : ICommand
    {
        private readonly IRandomForest _randomForest;

        public RunRandomForestCommand()
        {
            
        }

        public RunRandomForestCommand(IRandomForest randomForest)
        {
            _randomForest = randomForest;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _randomForest.Run();
        }
    }
}
