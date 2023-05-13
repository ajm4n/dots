using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

namespace SystemInformation
{
    public class WhoamiCommand
    {
        public string Name => "whoami";

        public string Execute(string[] args)
        {
            return IsAdministrator() + WindowsIdentity.GetCurrent().Name;
        }

        public static string IsAdministrator()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator) ? "*" : "";
        }
    }

    public class HostnameCommand
    {
        public string Name => "hostname";

        public string Execute(string[] args)
        {
            return Environment.MachineName;
        }
    }

    public class OsCommand
    {
        public string Name => "os";

        public string Execute(string[] args)
        {
            return Environment.OSVersion.VersionString;
        }
    }

    public class PidCommand
    {
        public string Name => "pid";

        public string Execute(string[] args)
        {
            return Process.GetCurrentProcess().Id.ToString();
        }
    }

    public class ArchCommand
    {
        public string Name => "arch";

        public string Execute(string[] args)
        {
            return IntPtr.Size == 8 ? "x64" : "x86";
        }
    }

    public class IpCommand
    {
        public string Name => "ip";

        public string Execute(string[] args)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(_ip => _ip.AddressFamily == AddressFamily.InterNetwork);
            return ip != null ? ip.ToString() : "0.0.0.0";
        }
    }

    public class PwdCommand
    {
        public string Name => "pwd";

        public string Execute(string[] args)
        {
            return Directory.GetCurrentDirectory();
        }
    }

}