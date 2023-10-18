using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spawn
{
    public class Spawn
    {
        public string Name => "spawn";
        public void Execute(string[] args)
        {
            new Thread(() =>
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                AppDomain appDomain = AppDomain.CreateDomain(new Random().Next().ToString());
                string assemblyName = assembly.FullName;
                appDomain.Load(assemblyName);
                appDomain.ExecuteAssemblyByName(assemblyName, args);
            }).Start();
        }
    }
}
   
