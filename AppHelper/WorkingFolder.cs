using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Windows.Media.Imaging;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Ecm.AppHelper
{
    public class WorkingFolder
    {
        private static string _rootDir = "";
        public string FolderName { get; private set; }
        public string Dir { get; private set; }
        public static List<string> UndeletedFiles = new List<string>();

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(ShellAddToRecentDocsFlags flag, IntPtr pidl);

        public static void Configure(string userName)
        {
            _rootDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _rootDir = Path.Combine(_rootDir, "eDocPro\\CloudECM\\" + userName);

            if (!Directory.Exists(_rootDir))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(_rootDir);
                //DirectorySecurity security = dirInfo.GetAccessControl();

                //security.AddAccessRule(new FileSystemAccessRule(userName, FileSystemRights.Modify, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                //security.AddAccessRule(new FileSystemAccessRule(userName, FileSystemRights.Modify, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                //dirInfo.SetAccessControl(security);
            }
        }

        public static string CreateTempFolder()
        {
            if (_rootDir == string.Empty)
            {
                throw new InstanceNotFoundException("Root directory is not found. Please call Configure method before using object of this class.");
            }

            string folder = Path.Combine(_rootDir, "Temp");

            if (!Directory.Exists(folder))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(folder);
                //DirectorySecurity security = dirInfo.GetAccessControl();

                //security.AddAccessRule(new FileSystemAccessRule("Temp", FileSystemRights.Modify, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                //security.AddAccessRule(new FileSystemAccessRule("Temp", FileSystemRights.Modify, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                //dirInfo.SetAccessControl(security);
            }

            return folder;
        }

        public static void GlobalDelete(string fileName)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);

                switch (fileInfo.Extension)
                {
                    case ".xls":
                    case ".xlsx":
                        Process[] excelProcess = Process.GetProcessesByName("EXCEL");
                        foreach (Process p in excelProcess)
                        {
                            p.Kill();
                        }
                        break;
                    case ".doc":
                    case ".docx":
                        Process[] wordProcess = Process.GetProcessesByName("WINWORD");
                        foreach (Process p in wordProcess)
                        {
                            p.Kill();
                        }
                        break;
                    case ".ppt":
                    case ".pptx":
                        Process[] pptProcess = Process.GetProcessesByName("POWERPOINT");
                        foreach (Process p in pptProcess)
                        {
                            p.Kill();
                        }
                        break;
                }

                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fileName));

                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Attributes &= ~FileAttributes.ReadOnly;
                }

                SHAddToRecentDocs(ShellAddToRecentDocsFlags.Pidl, IntPtr.Zero);
                File.Delete(fileName);
            }
            catch(Exception ex)
            {

                UndeletedFiles.Add(fileName);
            }
        }

        public WorkingFolder(string folderName)
        {
            if (_rootDir == string.Empty)
            {
                throw new InstanceNotFoundException("Root directory is not found. Please call Configure method before using object of this class.");
            }

            FolderName = folderName;
            Dir = Path.Combine(_rootDir, folderName);

            if (!Directory.Exists(Dir))
            {
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                //string machineName = System.Environment.MachineName;
                //string loginName = machineName + "\\" + userName;

                DirectoryInfo dirInfo = Directory.CreateDirectory(Dir);
                //DirectorySecurity security = dirInfo.GetAccessControl();

                //security.AddAccessRule(new FileSystemAccessRule(userName, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                //security.AddAccessRule(new FileSystemAccessRule(userName, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                //dirInfo.SetAccessControl(security);
            }
        }

        public string Copy(string sourceFilePath, string destFileName)
        {
            var destFilePath = Path.Combine(Dir, destFileName);
            File.Copy(sourceFilePath, destFilePath);

            return destFilePath;
        }

        public string Save(byte[] binary, string fileName)
        {
            var destFilePath = Path.Combine(Dir, fileName);
            File.WriteAllBytes(destFilePath, binary);
            return destFilePath;
        }

        public string Save(BitmapEncoder encoder)
        {
            string uniqueName = Guid.NewGuid().GetHashCode().ToString() + ".tiff";
            using (FileStream stream = File.Open(Dir + uniqueName, FileMode.Create))
            {
                encoder.Save(stream);
            }

            return Dir + uniqueName;
        }

        public string Save(PdfDocument document, string fileName)
        {
            using (FileStream stream = File.Open(Dir + fileName, FileMode.Create))
            {
                document.Save(stream);
            }

            return Dir + fileName;
        }

        public string Save(string xml, string fileName)
        {
            string filePath = Path.Combine(Dir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(xml);
                }
            }

            return filePath;
        }

        public string ReadText(string fileName)
        {
            string filePath = Path.Combine(Dir, fileName);
            return File.ReadAllText(filePath);
        }

        public string[] GetVisibleFiles()
        {
            FileInfo[] fileInfos = new DirectoryInfo(Dir).GetFiles().Where(p => (p.Attributes & FileAttributes.Hidden) == 0).ToArray();
            return fileInfos.Select(p => p.FullName).ToArray();
        }

        public string CreateDir(string dirName)
        {
            string dirPath = Path.Combine(Dir, dirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            return dirPath;
        }

        public bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(Dir, fileName));
        }

        public void Delete(string fileName)
        {
            try
            {
                if (Exists(fileName))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fileName));

                    foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        file.Attributes &= ~FileAttributes.ReadOnly;
                    }

                    File.Delete(fileName);
                }
            }
            catch
            {

                UndeletedFiles.Add(fileName);
            }
        }

        public static void Delete(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fileName));

                    foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        file.Attributes &= ~FileAttributes.ReadOnly;
                    }

                    File.Delete(fileName);
                }
                catch
                {
                }
            }
        }

        public void Clean()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Dir);

                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Attributes &= ~FileAttributes.ReadOnly;
                }

                Directory.Delete(Dir, true);
            }
            catch
            { }
        }

        public string GetFullFileName(string fileName)
        {
            return Path.Combine(Dir, fileName);
        }
    }
}
