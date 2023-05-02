using System;
using Dots.Models;

namespace Dots.Commands
{
    public class KillCommand : DotsCommand
    {
        public override string Name => "kill";
        public override DotsProperties DotsProperty { get ; set; }
        public override string Execute(string[] args)
        {
            Environment.Exit(0);
            return "Success";
        }
    }

}
