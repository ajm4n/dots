using System;
using System.Reflection;
using Dots.Models;

namespace Dots.Commands
{
    public class SleepCommand : DotsCommand
    {
        public override string Name => "sleep";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {
            DotsProperty.sleep = int.Parse(args[0]);
            return $"Succesfully set sleep to  {args[0]} module.";
        }
    }
}
