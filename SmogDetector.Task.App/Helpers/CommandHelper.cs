using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmogDetector.Task.App.Attributes;
using SmogDetector.Task.App.Commands;

namespace SmogDetector.Task.App.Helpers
{
    internal static class CommandHelper
    {

        public static bool IsCommand(Type type)
        {
            return typeof (ICommand).IsAssignableFrom(type) &&
                   Attribute.GetCustomAttribute(type, typeof (CommandNameAttribute)) != null;
        }

        public static CommandNameAttribute GetCommandName(Type type)
        {
            return (CommandNameAttribute)Attribute.GetCustomAttribute(type, typeof(CommandNameAttribute));
        }

        public static IEnumerable<CommandNameAttribute> GetAvailableCommandNames(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(IsCommand)
                .Select(GetCommandName)
                .OrderBy(c=>c.Name);
        }
    }
}
