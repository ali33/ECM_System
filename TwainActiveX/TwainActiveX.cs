using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TwainActiveX
{
    [ClassInterface(ClassInterfaceType.AutoDual), Guid("1EFCF932-9826-4DF5-BB20-4ECBE8A47078"), ProgId("TwainActiveX.TwainActiveX")]
    [ComSourceInterfaces(typeof(ITwainActiveXEvent))]
    [ComVisible(true)]
    public class TwainActiveX : ITwainActiveX
    {
        private string _tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"eDocPro\ActiveXScan");
        private string _uploadUrl;
        private string _scanToken;
        private List<string> _files = new List<string>();

        [ComVisible(true)]
        public delegate void ScanPageEventHandler(string args);

        public event ScanPageEventHandler ScanPageEvent;

        [ComVisible(true)]
        public List<string> Scan(bool isShowDialog, string uploadURL)
        {
            _uploadUrl = uploadURL;
            TwainUtil twainUtil = new TwainUtil();
            IntPtr handle = GetForegroundWindow();
            try
            {
                

                _files.Add(_uploadUrl);
                _files.Add(_tempFolder);

                twainUtil.Inititlize(handle.ToInt32(), _tempFolder, "CloudECM", ScanPage);

                twainUtil.Scan(isShowDialog);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            
            return _files;
        }

        [ComVisible(true)]
        public void ClearScanFile()
        {
            foreach (string file in Directory.GetFiles(_tempFolder))
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private void ScanPage(string filePath)
        {
            try
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("id", "TTR");
                nvc.Add("btn-submit-photo", "Upload");

                _files.Add(FileUpload.HttpUploadFile(_uploadUrl, filePath, "fileUpload", FileUpload.GetMimeType(Path.GetExtension(filePath)), nvc));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        #region Capture system
        /// <summary>
        /// Callback of scan page in Capture system
        /// </summary>
        /// <param name="filePath"></param>
        private void ScanPageCaptureCallback(string filePath)
        {
            try
            {
                // Upload scanned page and get file path in server
                var fileNameServer = FileUpload.HttpUploadFile(_uploadUrl, filePath, _scanToken);
                _files.Add(fileNameServer);

                // Delete file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Scan function use in capture system
        /// </summary>
        /// <param name="isShowDialog"></param>
        /// <param name="uploadURL"></param>
        /// <returns></returns>
        [ComVisible(true)]
        public string ScanCapture(bool isShowDialog, string uploadURL, string scanToken)
        {
            _uploadUrl = uploadURL;
            _scanToken = scanToken;

            TwainUtil twainUtil = new TwainUtil();
            IntPtr handle = GetForegroundWindow();

            try
            {
                twainUtil.Inititlize(handle.ToInt32(), _tempFolder, "CloudECM", ScanPageCaptureCallback);
                twainUtil.Scan(isShowDialog);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string output = jss.Serialize(_files);
            return output;
        }
        #endregion
    }
}
