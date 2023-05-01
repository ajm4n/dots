using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Dots.Models;



namespace Filesystem
{
    public class Program { public static void Main(string[] args) { } }

    public class DirCommand : DotsCommand
    {
        public override string Name => "dir";

        public static string IsAdministrator()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator) ? "*" : "";
        }

        public override void Execute(TaskRequest task)
        {
            throw new NotImplementedException();
        }
    }

    public class UploadCommand : DotsCommand
    {
        public override string Name => "upload";

        public override void Execute(TaskRequest task)
        {
            throw new NotImplementedException();
        }
    }

}
