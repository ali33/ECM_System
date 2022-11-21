using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ecm.Utility
{
    public class FileHelpper
    {
        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            return path.Substring(path.LastIndexOf(@"\") + 1);
        }
        public static string GetPath_WithoutFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            return path.Substring(0, path.LastIndexOf(@"\") + 1);
        }
        public static byte[] CreateFile(string fileName, byte[] value, string fileExtention)
        {
            try
            {
                CreateFolder(GetPath_WithoutFileName(fileName));
                byte[] header = new byte[0];
                int lenghHeader = 0;
                switch (fileExtention.ToLower())
                {
                    case "exe":
                    case "llvm":
                    case "zip":
                        lenghHeader = 2;
                        break;
                    case "tif":
                    case "tiff":
                    case "midi":
                    case "pdf":
                    case "pef":
                        lenghHeader = 4;
                        break;
                    case "gif":
                        lenghHeader = 6;
                        break;
                    case "jpeg":
                    case "jpg":
                        lenghHeader = 10;
                        break;
                    case "png":
                    case "doc":
                    case "docx":
                    case "ppt":
                    case "pptx":
                    case "xls":
                    case "xlsx":
                        lenghHeader = 8;
                        break;
                    case "":
                        lenghHeader = 2;
                        break;
                    default:
                        lenghHeader = 2;
                        break;

                }
                lenghHeader = Math.Min(lenghHeader, value.Length);

                header = new byte[lenghHeader];
                Array.Copy(value, 0, header, 0, lenghHeader);
                byte[] data = new byte[value.Length - lenghHeader > 0 ? value.Length - lenghHeader : 0];
                Array.Copy(value, lenghHeader, data, 0, value.Length - lenghHeader);
                File.WriteAllBytes(fileName, data);
                return header;
            }
            catch
            {
                throw;
            }

        }

        public static byte[] ReadFile(string fileName, byte[] header)
        {
            try
            {
                byte[] value = File.ReadAllBytes(fileName);

                byte[] newArray = new byte[header.Length + value.Length];
                Array.Copy(header, newArray, header.Length);
                Array.Copy(value, 0, newArray, header.Length, value.Length);

                return newArray;
            }
            catch
            {
                throw;
            }

        }

        public static bool Copy(string fileFrom, string fileTo)
        {
            try
            {
                CreateFolder(GetPath_WithoutFileName(fileTo));
                File.Copy(fileFrom, fileTo);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public static bool DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
                return true;
            }
            catch
            {
                throw;
            }

        }

        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
