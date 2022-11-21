using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ecm.CustomAddin
{
    public class AddinCommon
    {
        public const string EXCEL_ADDIN_TEMP_FOLDER = "ExcelAddin";
        public const string WORD_ADDIN_TEMP_FOLDER = "WordAddin";
        public const string PPT_ADDIN_TEMP_FOLDER = "PPTAddin";

        public static byte[] GetContents(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            String tempFileCopy = fileName;

            tempFileCopy = tempFileCopy.Replace(".", "_copy.");
            fi.CopyTo(tempFileCopy, true);

            byte[] contents = File.ReadAllBytes(tempFileCopy);
            File.Delete(tempFileCopy);

            return contents;
        }

        public static string GetDocId(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return dir.Name;
        }

        public static string GetUserName(string path, string root)
        {
            string filePath = path;
            string directoryName;
            int i = 0;

            while (filePath != null)
            {
                directoryName = Path.GetDirectoryName(filePath);

                filePath = directoryName;

                if (i == 0)
                {
                    filePath = directoryName + @"\";  // this will preserve the previous path
                }

                if (directoryName == null)
                {
                    return null;
                }

                DirectoryInfo dir = new DirectoryInfo(directoryName);

                if (dir.Name.Equals(root))
                {
                    return dir.Parent.Name;
                }

                i++;
            }

            return null;
        }

        public static string GetEncryptedPassword(string path, string root)
        {
            string filePath = path;
            string directoryName;
            int i = 0;
            string fileName = string.Empty;

            while (filePath != null)
            {
                directoryName = Path.GetDirectoryName(filePath);

                filePath = directoryName;

                if (i == 0)
                {
                    filePath = directoryName + @"\";  // this will preserve the previous path
                }

                DirectoryInfo dir = new DirectoryInfo(directoryName);

                if (dir.Name.Equals(root))
                {
                    string userName = GetUserName(path, root);
                    fileName = Path.Combine(directoryName, userName);

                    break;
                }

                i++;
            }

            if (fileName != string.Empty)
            {
                string password = File.ReadAllText(fileName);

                return password;
            }

            return null;
        }

        public static AddinType GetAddinType(string extension)
        {
            switch (extension)
            {
                case "xls":
                case "xslx":
                    return AddinType.Excel;
                case "doc":
                case "docx":
                    return AddinType.Word;
                case "ppt":
                case "pptx":
                    return AddinType.PowerPoint;
            }

            return AddinType.Excel;
        }

    }

    public enum AddinType
    {
        Excel,
        Word,
        PowerPoint,
        Outlook
    }
}
