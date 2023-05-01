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
        public TaskError Error { get; set; }
        public abstract void Execute(TaskRequest task);
        public void SetupError(TaskRequest task, string message)
        {
            message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            Error = new TaskError
            {
                JSONRPC = task.JSONRPC,
                Error = new TaskErrorDetails
                {
                    Code = -32602,
                    Message = message,
                },
                Id = task.Id
            };
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
