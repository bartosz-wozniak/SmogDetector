using System;

namespace SmogDetector.Task.App.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class CommandNameAttribute:Attribute
    {
        public string Name { get; }

        public string Description { get; }

        public CommandNameAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
