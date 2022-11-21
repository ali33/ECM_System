using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ecm.ImportData
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class ReadIniFile
    {
        private string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }


        [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileStringW",
          SetLastError = true,
          CharSet = CharSet.Unicode, ExactSpelling = true,
          CallingConvention = CallingConvention.StdCall)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
          SetLastError = true,
          CharSet = CharSet.Unicode, ExactSpelling = true,
          CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        //private const int Max_SectionBuffer = 4096;
        //private const int Max_EntryBuffer = 255;

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <param name="INIPath"></param>
        public ReadIniFile(string INIPath)
        {
            path = INIPath;
        }
        public ReadIniFile()
        {
            path = string.Empty;
        }



        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <param name="Section"></param>
        /// Section name
        /// <param name="Key"></param>
        /// Key Name
        /// <param name="Value"></param>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(1000);
            int i = GetPrivateProfileString(Section, Key, "", temp, 1000, path);
            return temp.ToString();
        }



       


    }
}
