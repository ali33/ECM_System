/**
 * Copyright @ 2011 Quan Nguyen
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
using System.IO;
using OCR.TesseractWrapper;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ecm.Ocr
{
    public class OCRImages : OCR<Image>
    {
        private readonly string _tessDataDir = string.Empty;
        private readonly string _language = "vie";
        string basedir = string.Empty;
        const string TESSDATA = "tessdata/";
        const int oem = 3;
        
        public OCRImages()
        {
            basedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0,6);
            _tessDataDir = Path.Combine(basedir, TESSDATA);
        }

        public OCRImages(string dataPath)
        {
            _tessDataDir = dataPath;
        }

        public OCRImages(string dataPath, string languageCode)
        {
            _tessDataDir = dataPath;
            _language = languageCode;
        }

        public override string RecognizeText(IList<Image> images, string lang)
        {
            try
            {
                var processor = new TesseractProcessor();
                processor.Init(_tessDataDir, _language, oem);
                processor.SetPageSegMode((ePageSegMode)Enum.Parse(typeof(ePageSegMode), PSM));

                var strB = new StringBuilder();
                foreach (Image image in images)
                {
                    string text = processor.Recognize(image, rect);

                    if (text == null) return String.Empty;
                    strB.Append(text);
                }

                return strB.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }
    }
}
