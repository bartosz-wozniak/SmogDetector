using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("test-random-forest", "Tests Random Forest")]
    internal class TestRandomForestCommand : ICommand
    {
        private readonly IRandomForest _randomForest;

        public TestRandomForestCommand()
        {
            
        }

        public TestRandomForestCommand(IRandomForest randomForest)
        {
            _randomForest = randomForest;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _randomForest.Test();
        }
    }
}
