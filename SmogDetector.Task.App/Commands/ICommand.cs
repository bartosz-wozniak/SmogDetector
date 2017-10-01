using System.Collections.Generic;

namespace SmogDetector.Task.App.Commands
{
    internal interface ICommand
    {
        void Execute(IEnumerable<string> parameters);
    }
}
