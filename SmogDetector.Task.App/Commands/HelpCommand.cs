using System;
using System.Collections.Generic;
using System.Reflection;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.App.Helpers;

namespace SmogDetector.Task.App.Commands
{
    [CommandName("help", "Displays help")]
    internal class HelpCommand : ICommand
    {
        public void Execute(IEnumerable<string> parameters)
        {
            Console.WriteLine("Available commands:");
            foreach (var command in CommandHelper.GetAvailableCommandNames(Assembly.GetExecutingAssembly()))
            {
                Console.WriteLine(command.Name + " - " + command.Description);
            }
        }
    }
}
