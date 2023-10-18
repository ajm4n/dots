using System;
using System.Reflection;
using Dots.Models;

namespace Dots.Commands
{
    public class JitterCommand : DotsCommand
    {
        public override string Name => "jitter";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(TaskRequest task)
        {
            DotsProperty.Jitter = int.Parse(task.Params[0]);
            return $"Succesfully set jitter to {task.Params[0]}";
        }
    }
}
