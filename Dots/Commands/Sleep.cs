using System;
using System.Reflection;
using Dots.Models;

namespace Dots.Commands
{
    public class SleepCommand : DotsCommand
    {
        public override string Name => "sleep";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(TaskRequest task)
        {
            DotsProperty.Sleep = int.Parse(task.Params[0]);
            return $"Succesfully set sleep to {task.Params[0]}";
        }
    }
}
