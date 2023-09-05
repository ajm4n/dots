using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Spawn
{
    public class Spawn
    {

        public string Name => "spawn";

        public void Execute(string[] args)
        {
            var process = Process.GetCurrentProcess();
            string process_path = process.MainModule.FileName;
            ExecuteCmd($"{process_path} {args[0]} {args[1]}");
        }

        private void ExecuteCmd(string command)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\cmd.exe",
                    Arguments = $"/c {command}",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }
    }
}
   
