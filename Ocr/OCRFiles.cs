/**
 * Copyright @ 2008 Quan Nguyen
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Ecm.Ocr
{
    public class OCRFiles : OCR<string>
    {
        const string FILE_EXTENSION = ".txt";

        /// <summary>
        /// Recognizes TIFF files.
        /// </summary>
        /// <param name="tiffFiles"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public override string RecognizeText(IList<string> tiffFiles, string lang)
        {
            string tempTessOutputFile = Path.GetTempFileName();
            File.Delete(tempTessOutputFile);
            tempTessOutputFile = Path.ChangeExtension(tempTessOutputFile, FILE_EXTENSION);
            string outputFileName = tempTessOutputFile.Substring(0, tempTessOutputFile.Length - FILE_EXTENSION.Length); // chop the .txt extension

            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "tesseract.exe";

            StringBuilder result = new StringBuilder();

            foreach (string tiffFile in tiffFiles)
            {
                p.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" -l {2} -psm {3}", tiffFile, outputFileName, lang, PSM);
                p.Start();

                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd(); // ignore standard output
                string error = p.StandardError.ReadToEnd(); // 

                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    using (StreamReader sr = new StreamReader(tempTessOutputFile, Encoding.UTF8, true))
                    {
                        result.Append(sr.ReadToEnd());
                    }
                }
                else
                {
                    File.Delete(tempTessOutputFile);
                    if (error.Trim().Length == 0)
                    {
                        error = "Errors occurred.";
                    }
                    throw new ApplicationException(error);
                }
            }

            File.Delete(tempTessOutputFile);
            return result.ToString();
        }
    }
}
