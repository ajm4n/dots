using Dots.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

namespace SystemInformation
{
    public class Program { public static void Main(string[] args) { } }

    public class WhoamiCommand : DotsCommand
    {
        public override string Name => "whoami";

        public static string IsAdministrator()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator) ? "*" : ""; 
        }

        public override void Execute(TaskRequest task)
        {
            SetupResult(task, IsAdministrator() + WindowsIdentity.GetCurrent().Name);
        }
    }

    public class HostnameCommand : DotsCommand
    {
        public override string Name => "hostname";

        public override void Execute(TaskRequest task)
        {
            SetupResult(task, Environment.MachineName);
        }
    }

    public class OsCommand : DotsCommand
    {
        public override string Name => "os";

        public override void Execute(TaskRequest task)
        {
            SetupResult(task, Environment.OSVersion.VersionString);
        }
    }

    public class PidCommand : DotsCommand
    {
        public override string Name => "pid";

        public override void Execute(TaskRequest task)
        {
            SetupResult(task, Process.GetCurrentProcess().Id.ToString());
        }
    }

    public class ArchCommand : DotsCommand
    {
        public override string Name => "arch";

        public override void Execute(TaskRequest task)
        {
            SetupResult(task, IntPtr.Size == 8 ? "x64" : "x86");
        }
    }

    public class IpCommand : DotsCommand
    {
        public override string Name => "ip";

        public override void Execute(TaskRequest task)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(_ip => _ip.AddressFamily == AddressFamily.InterNetwork);
            SetupResult(task, ip != null ? ip.ToString() : "0.0.0.0");
        }
    }
}
