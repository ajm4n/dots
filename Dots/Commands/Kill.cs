using System;
using Dots.Models;

namespace Dots.Commands
{ 
    public class KillCommand : DotsCommand
    {
        public override string Name => "kill";

        public override void Execute(TaskRequest task, DotsProperties dotsProperty)
        {
            Environment.Exit(0);
            SetupResult(task, "Success");
        }
    }
}
