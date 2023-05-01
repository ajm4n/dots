using Dots.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dots.Commands
{
    public class WhoamiCommand : DotsCommand
    {
        public override string Name => "whoami";

        public override void Execute(TaskRequest task)
        {
            SetupResult(task, "skyler");
        }
    }
}
