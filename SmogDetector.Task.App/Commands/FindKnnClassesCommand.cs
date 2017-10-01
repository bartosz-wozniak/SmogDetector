using System.Collections.Generic;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("find-knn-classes", "Finds KNN Classes for k = 1, 5, 15")]
    internal class FindKnnClassesCommand : ICommand
    {
        private readonly IKnnComputation _computation;

        public FindKnnClassesCommand()
        {
            
        }

        public FindKnnClassesCommand(IKnnComputation computation)
        {
            _computation = computation;
        }

        public void Execute(IEnumerable<string> parameters)
        {
           _computation.FindClasses();
        }
    }
}
