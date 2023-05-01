using Dots.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dots.Commands
{ 
    public class KillCommand : DotsCommand
    {
        public override string Name => "kill";

        public override void Execute(TaskRequest task)
        {
            Environment.Exit(0);
            SetupResult(task, "Success");
        }
    }
}
