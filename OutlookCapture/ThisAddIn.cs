using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using Ecm.Mvvm;
using System.Resources;
using System.Windows.Forms;
using stdole;
using System.Drawing;
using System.Reflection;
using Ecm.CaptureCustomAddIn.ViewModel;
using Ecm.CaptureCustomAddIn.View;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using System.Configuration;
using Ecm.AppHelper;
using System.IO;
using Ecm.CaptureCustomAddIn;

namespace OutlookCapture
{
    public partial class ThisAddIn
    {
        private Outlook.Selection _selection;
        private const string _tempFolder = @"CaptureAddOn\OutlookTemp";
        private ImageList _newImageList = new ImageList();
        private IPictureDisp _sendToMenuImage;
        private Outlook.MailItem _objMailItem;
        private Ribbon _ribbon;
        private ResourceManager _resource = new ResourceManager("OutlookCapture.Resources", Assembly.GetExecutingAssembly());

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            try
            {
                DialogService.InitializeDefault();
                _newImageList.Images.Add((Icon)_resource.GetObject("logo1"));
                _sendToMenuImage = ImageConverter.Convert(_newImageList.Images[0]);

                Application.ItemContextMenuDisplay += new Outlook.ApplicationEvents_11_ItemContextMenuDisplayEventHandler(Application_ItemContextMenuDisplay);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Application_ItemContextMenuDisplay(Office.CommandBar commandBar, Outlook.Selection selection)
        {
            Office.CommandBarButton btnSendTo = commandBar.Controls.Add(Office.MsoControlType.msoControlButton, Type.Missing, Type.Missing, Type.Missing, Type.Missing) as Office.CommandBarButton;
            btnSendTo.Caption = "Send to Capture";
            btnSendTo.Style = Office.MsoButtonStyle.msoButtonIconAndCaption;

            btnSendTo.Picture = _sendToMenuImage;
            btnSendTo.Visible = true;

            _selection = selection;
            btnSendTo.Click += new Office._CommandBarButtonEvents_ClickEventHandler(btnSendTo_Click);
        }

        void btnSendTo_Click(Office.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            Login();

            if (LoginViewModel.LoginUser != null)
            {
                List<MailItemInfo> mailInfo = this.GetMailInfos();
                OpenViewer(mailInfo);
            }
        }

        private void OpenViewer(List<MailItemInfo> mailInfos)
        {
            try
            {
                if (LoginViewModel.LoginUser != null)
                {
                    BatchTypeSelectionView batchTypeView = new BatchTypeSelectionView(mailInfos);
                    batchTypeView.ShowDialog();

                    foreach (MailItemInfo item in mailInfos)
                    {
                        if (Directory.Exists(item.TempFolderName))
                        {
                            Directory.Delete(item.TempFolderName, true);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<MailItemInfo> GetMailInfos()
        {
            List<MailItemInfo> mailInfos = new List<MailItemInfo>();
            foreach (MailItem mailItem in _selection)
            {
                MailItemInfo mailInfo = GetMailInfo(mailItem);
                mailInfos.Add(mailInfo);
            }

            return mailInfos;
        }

        private List<MailItemInfo> GetMailInfos(Outlook.Selection selection)
        {
            List<MailItemInfo> mailInfos = new List<MailItemInfo>();
            foreach (MailItem mailItem in selection)
            {
                MailItemInfo mailInfo = GetMailInfo(mailItem);
                mailInfos.Add(mailInfo);
            }

            return mailInfos;
        }

        private MailItemInfo GetMailInfo(MailItem mailItem)
        {
            MailItemInfo mailInfo = new MailItemInfo();

            mailInfo.MailBody = mailItem.Body;
            mailInfo.MailFrom = mailItem.SenderName;
            mailInfo.MailTo = mailItem.To;
            mailInfo.ReceivedDate = mailItem.ReceivedTime;
            mailInfo.MailSubject = mailItem.Subject;
            mailInfo.Attachments = new List<string>();
            mailInfo.EmbeddedPictures = new Dictionary<string, byte[]>();

            string htmlStructure = @"<HTML><DIV>{0}</DIV></HTML>";
            string mailBody = string.Empty;
            if (mailItem.BodyFormat != OlBodyFormat.olFormatHTML)
            {
                htmlStructure = string.Format(htmlStructure, mailItem.Body);
                mailBody = htmlStructure;
            }
            else
            {
                mailBody = mailItem.HTMLBody;
            }

            string outlookVersion = Globals.ThisAddIn.Application.Version.Substring(0, 2);

            RegistryKey registry = null;//
            if (outlookVersion == "14")
            {
                registry = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\14.0\Outlook\Security");
            }
            else if (outlookVersion == "12")
            {
                registry = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\12.0\Outlook\Security");
            }

            string outlookTemp = ConfigurationManager.AppSettings["OutlookTempDir"];
            string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            tempPath = Path.Combine(tempPath, outlookTemp);

            StringReader st = new StringReader(mailBody);
            StringBuilder html = new StringBuilder();
            //string line = null;
            Dictionary<string, string> imageNames = new Dictionary<string, string>();
            Dictionary<string, string> cidImageNames = new Dictionary<string, string>();

            List<string> imagePaths = new List<string>();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument {
                OptionWriteEmptyNodes = true,
                OptionAutoCloseOnEnd = true
            };

            doc.LoadHtml(mailBody);

            if (doc.DocumentNode.SelectNodes("//img[@src]") != null)
            {
                foreach (HtmlAgilityPack.HtmlNode link in doc.DocumentNode.SelectNodes("//img[@src]"))
                {
                    HtmlAgilityPack.HtmlAttribute att = link.Attributes["src"];
                    string values = att.Value;
                    
                    if(values.StartsWith("http://") || values.StartsWith("https://"))
                    {
                        continue;
                    }

                    int length = values.IndexOf("@", 0) - 4;
                    string imageName = values.Substring(4, length);
                    string imageNewName = Guid.NewGuid().ToString() + new FileInfo(imageName).Extension;

                    if (!imageNames.ContainsKey(imageName))
                    {
                        imageNames.Add(imageName, imageNewName);
                    }

                    string newSrc = tempPath + imageNewName;
                    att.Value = newSrc;
                }
            }

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            mailInfo.TempFolderName = tempPath;

            foreach (Attachment attachment in mailItem.Attachments)
            {
                FileInfo fileInfo = new FileInfo(attachment.FileName);
                string saveFile = string.Empty;
                string type = attachment.GetType().ToString();
                if (imageNames.Keys.Contains(attachment.FileName))
                {
                    saveFile = Path.Combine(tempPath, imageNames[attachment.FileName]);
                    attachment.SaveAsFile(saveFile);
                    byte[] fileBytes = File.ReadAllBytes(saveFile);

                    if (mailInfo.EmbeddedPictures.ContainsKey(imageNames[attachment.FileName]))
                    {
                        saveFile = Path.Combine(tempPath, Guid.NewGuid().ToString() + fileInfo.Extension);
                        attachment.SaveAsFile(saveFile);
                        mailInfo.Attachments.Add(saveFile);
                    }
                    else
                    {
                        mailInfo.EmbeddedPictures.Add(imageNames[attachment.FileName], fileBytes);
                    }
                }
                else
                {
                    saveFile = Path.Combine(tempPath, Guid.NewGuid().ToString() + fileInfo.Extension);
                    attachment.SaveAsFile(saveFile);
                    mailInfo.Attachments.Add(saveFile);
                }

            }

            string mailBodyFileName = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".html");//PdfHelper.ConvertToPdf(doc.DocumentNode.WriteTo());//
            doc.Save(mailBodyFileName, Encoding.UTF8);
            mailInfo.BodyFileName = mailBodyFileName;

            return mailInfo;
        }

        private int count = 1;

        private string CheckFileName(string file, Dictionary<string, string> files)
        {
            string newFileName = file;

            if (files.ContainsKey(file))
            {
                string extension = new FileInfo(newFileName).Extension;
                newFileName = newFileName.Remove(newFileName.IndexOf("."));
                newFileName += "-copy(" + count + ")";
                newFileName += extension;

                count++;
                CheckFileName(newFileName, files);
            }

            return newFileName;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        protected override object RequestService(Guid serviceGuid)
        {
            if (serviceGuid == typeof(Office.IRibbonExtensibility).GUID)
            {
                if (_ribbon == null)
                {
                    _ribbon = new Ribbon();
                }

                _ribbon.SendToCaptureClick = StartViewer;

                return _ribbon;
            }

            return base.RequestService(serviceGuid);
        }

        private void StartViewer(object sender)
        {
            Login();

            if (LoginViewModel.LoginUser == null)
            {
                LoginView loginView = new LoginView("MS Outlook Capture", AddinType.Outlook);
                loginView.ShowDialog();
            }

            Microsoft.Office.Core.IRibbonControl control = sender as Microsoft.Office.Core.IRibbonControl;

            if (sender != null)
            {
                var item = control.Context as Inspector;

                if (item != null)
                {
                    _objMailItem = (Outlook.MailItem)item.CurrentItem;
                    OpenViewer(new List<MailItemInfo> { GetMailInfo(_objMailItem) });
                }
                else if (Application.ActiveExplorer().Selection != null)
                {
                    OpenViewer(GetMailInfos(Application.ActiveExplorer().Selection));
                }
            }
        }

        private void Login()
        {
            if (LoginViewModel.LoginUser == null)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(Common.OUTLOOK_AUTO_SIGNIN_KEY);

                if (key != null)
                {
                    try
                    {
                        string username = key.GetValue("Username").ToString();
                        string password = Ecm.Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(key.GetValue("Password").ToString());

                        LoginViewModel.LoginUser = new SecurityProvider().Login(username, password);

                        if (LoginViewModel.LoginUser == null)
                        {
                            LoginView loginView = new LoginView("MS Outlook Capture", AddinType.Outlook);
                            loginView.ShowDialog();
                        }
                        else
                        {
                            WorkingFolder.Configure(LoginViewModel.LoginUser.Username);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LoginView loginView = new LoginView("MS Outlook Capture", AddinType.Outlook);
                        loginView.ShowDialog();
                    }
                }
                else
                {
                    LoginView loginView = new LoginView("MS Outlook Capture", AddinType.Outlook);
                    loginView.ShowDialog();
                }
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
