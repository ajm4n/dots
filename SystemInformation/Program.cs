using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SystemInformation
{
    public class WhoamiCommand
    {
        public string Name => "whoami";

        public string Execute(string[] args)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            
            string[] nameParts = identity.Name.Split('\\');
            string username = nameParts[1];
            string machineName = Dns.GetHostName();
            string domain = (nameParts.Length > 0 && nameParts[0].ToLower() == machineName.ToLower()) ? "" : nameParts[0] + "/";
            return IsAdministrator() + domain + username.ToLower() + "@" + machineName.ToLower();
        }
        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
        }
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(
            IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            int TokenInformationLength,
            out int ReturnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_GROUPS
        {
            public int GroupCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public SID_AND_ATTRIBUTES[] Groups;
        };

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(IntPtr pSID, out IntPtr ptrSid);


        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        private string IsAdministrator()
        {
            // Thanks to SharpUp for the code.

            // Returns all SIDs that the current user is a part of, whether they are disabled or not.

            // adapted almost directly from https://stackoverflow.com/questions/2146153/how-to-get-the-logon-sid-in-c-sharp/2146418#2146418

            int TokenInfLength = 0;

            // first call gets length of TokenInformation
            bool Result = GetTokenInformation(WindowsIdentity.GetCurrent().Token, TOKEN_INFORMATION_CLASS.TokenGroups, IntPtr.Zero, TokenInfLength, out TokenInfLength);
            IntPtr TokenInformation = Marshal.AllocHGlobal(TokenInfLength);
            Result = GetTokenInformation(WindowsIdentity.GetCurrent().Token, TOKEN_INFORMATION_CLASS.TokenGroups, TokenInformation, TokenInfLength, out TokenInfLength);

            if (!Result)
            {
                Marshal.FreeHGlobal(TokenInformation);
                return null;
            }

            TOKEN_GROUPS groups = (TOKEN_GROUPS)Marshal.PtrToStructure(TokenInformation, typeof(TOKEN_GROUPS));
            string[] userSIDS = new string[groups.GroupCount];
            int sidAndAttrSize = Marshal.SizeOf(new SID_AND_ATTRIBUTES());
            for (int i = 0; i < groups.GroupCount; i++)
            {
                SID_AND_ATTRIBUTES sidAndAttributes = (SID_AND_ATTRIBUTES)Marshal.PtrToStructure(
                    new IntPtr(TokenInformation.ToInt64() + i * sidAndAttrSize + IntPtr.Size), typeof(SID_AND_ATTRIBUTES));

                IntPtr pstr = IntPtr.Zero;
                ConvertSidToStringSid(sidAndAttributes.Sid, out pstr);
                userSIDS[i] = Marshal.PtrToStringAuto(pstr);
                LocalFree(pstr);
            }

            Marshal.FreeHGlobal(TokenInformation);
            foreach (string SID in userSIDS)
            {
                if (SID == "S-1-5-32-544")
                {
                    return "*";
                }
            }
            return "";
        }
    }

    public class HostnameCommand
    {
        public string Name => "hostname";

        public string Execute(string[] args)
        {
            return "DNS: " + Dns.GetHostName() + "\nNETBIOS: " + Environment.MachineName;
        }
    }

    public class OsCommand
    {
        public string Name => "os";

        public string Execute(string[] args)
        {
            OperatingSystem os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
            {
                return "Windows " + os.Version.Major + "." + os.Version.Minor + " " + Arch();
            }
            return os.Platform.ToString();
        }

        private string Arch()
        {
            return IntPtr.Size == 8 ? "x64" : "x86";
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

    public class IntegrityCommand
    {
        public string Name => "integrity";
        public string Execute(string[] args)
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator) ? "High" : "Medium";
        }
    }

    public class IpCommand
    {
        public string Name => "ip";

        public string Execute(string[] args)
        {
            List<string> cidrNotations = new List<string>();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.Address.Equals(IPAddress.Parse("127.0.0.1")))
                    {
                        var subnetMask = ip.IPv4Mask.ToString();
                        var subnetMaskBytes = IPAddress.Parse(subnetMask).GetAddressBytes();
                        var subnetMaskBits = subnetMaskBytes.Sum(b => Convert.ToString(b, 2).Count(c => c == '1'));
                        var cidrNotation = $"{ip.Address}/{subnetMaskBits}";
                        cidrNotations.Add(cidrNotation);
                    }
                }
            }
            return cidrNotations.Count > 0 ? string.Join(", ", cidrNotations) : "No Interfaces";
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