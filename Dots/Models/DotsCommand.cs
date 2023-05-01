using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dots.Models
{
    public abstract class DotsCommand
    {
        public abstract string Name { get; }
        public TaskResult Result { get; set; }
        public abstract void Execute(TaskRequest task);
        public static byte[] Zor(byte[] input, string key)
        {
            int _key = Int32.Parse(key);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ _key);
            }
            return mixed;
        }
        public void SetupResult(TaskRequest task, dynamic result)
        {
            if (result is byte[])
            {
                result = Convert.ToBase64String(result);
            } else
            {
                result = Convert.ToBase64String(Encoding.UTF8.GetBytes(result));
            }

            Result = new TaskResult
            {
                JSONRPC = task.JSONRPC,
                Result = result,
                Id = task.Id
            };
        }
    };
}
