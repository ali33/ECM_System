using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;
using Ecm.WordImport;
//using Ecm.WordImport.ViewModel;
//using Ecm.WordImport.View;
using System.IO;
using Ecm.CustomAddin.ViewModel;
using Ecm.CustomAddin.View;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.CustomAddin;
using Ecm.AppHelper;
using Ecm.Utility;
using Microsoft.Win32;

namespace WordImport
{
    public partial class ThisAddIn
    {
        Ribbon _ribbon;
        private PageModel _activePage;
        private DocumentModel _activeDocument;
        private List<DocumentModel> _documents = new List<DocumentModel>();
        private List<PageModel> _pages = new List<PageModel>();
        Microsoft.Office.Tools.CustomTaskPane _customTask;
        private string _filePath;
        private const string TEMP_FOLDER = "ArchiveAddOn";

        public bool CanSave
        {
            get { return _activeDocument != null; }
        }

        public bool CanShowDashboard
        {
            get { return _activeDocument != null; }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        protected override object RequestService(Guid serviceGuid)
        {
            if (serviceGuid == typeof(Office.IRibbonExtensibility).GUID)
            {
                if (_ribbon == null)
                    _ribbon = new Ribbon(StartViewer);
                return _ribbon;
            }


            return base.RequestService(serviceGuid);
        }

        private void StartViewer(AddinAction action)
        {
            if (!string.IsNullOrEmpty(Application.ActiveDocument.Path))
            {
                AutoLogin(Application.ActiveDocument.Path);
            }

            if (LoginViewModel.LoginUser == null)
            {
                LoginView loginView = new LoginView(AddinType.Word);
                loginView.ShowDialog();
            }

            if (LoginViewModel.LoginUser != null)
            {
                switch (action)
                {
                    case AddinAction.SendToAcrchive:
                        if (string.IsNullOrEmpty(Application.ActiveDocument.Path))
                        {
                            _filePath = CreateTempFile(Application.ActiveDocument);
                        }
                        else
                        {
                            _filePath = Application.ActiveDocument.FullName;
                        }
                        

                        if (File.Exists(_filePath))
                        {
                            byte[] fileBinary = AddinCommon.GetContents(_filePath);
                            FileInfo info = new FileInfo(_filePath);
                            string fileExtension = info.Extension.Remove(0, 1);
                            MainView mainView = new MainView(info.Name, fileBinary, fileExtension);
                            mainView.ShowDialog();
                        }

                        break;
                    case AddinAction.Open:
                        SearchView content = new SearchView(AddinType.Word);
                        content.ShowDialog();
                        _activePage = (content.DataContext as SearchViewModel).SelectedPage;
                        _activeDocument = (content.DataContext as SearchViewModel).Document;

                        if (_activeDocument != null && _activePage != null)
                        {
                            if (AddinCommon.GetAddinType(_activePage.FileExtension) == AddinType.Word)
                            {
                                if (!_documents.Any(p => p.Id == _activeDocument.Id))
                                {
                                    _documents.Add(_activeDocument);
                                }

                                if (!_pages.Any(p => p.Id == _activePage.Id))
                                {
                                    _pages.Add(_activePage);
                                }

                                InitDashboard();
                            }
                        }

                        break;
                    case AddinAction.Save:
                        Application.ActiveDocument.Save();

                        var page = _activeDocument.Pages.SingleOrDefault(p => p.Id == _activePage.Id);
                        page.FileBinaries = AddinCommon.GetContents(_filePath);

                        new DocumentProvider().UpdateDocuments(new List<DocumentModel> { _activeDocument });

                        object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                        Application.ActiveDocument.Close(ref doNotSaveChanges, ref missing, ref missing);

                        if (File.Exists(_filePath))
                        {
                            File.Delete(_filePath);
                        }

                        _documents.Remove(_activeDocument);
                        _pages.Remove(_activePage);

                        if (_pages.Count == 0)
                        {
                            _activePage = null;
                        }
                        else
                        {
                            _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Application.ActiveDocument.FullName));
                        }

                        if (_documents.Count == 0)
                        {
                            _activeDocument = null;
                        }
                        else
                        {
                            _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);
                        }

                        if (CustomTaskPanes.Count > 0 && _customTask != null)
                        {
                            CustomTaskPanes.Remove(_customTask);

                            if (CustomTaskPanes.Count == 0)
                            {
                                _customTask = null;
                            }
                            else
                            {
                                _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _activeDocument.DocumentType.Name + "_" + _activeDocument.Id);
                            }
                        }
                        break;
                    case AddinAction.SaveAs:
                        if (File.Exists(_filePath))
                        {
                            byte[] fileBinary = AddinCommon.GetContents(_filePath);
                            string fileExtension = new FileInfo(_filePath).Extension.Remove(0, 1);

                            var page1 = new PageModel
                            {
                                Content = _activePage.Content,
                                ContentLanguageCode = _activePage.ContentLanguageCode,
                                FileBinaries = fileBinary,
                                OriginalFileName = _activePage.OriginalFileName,
                                FileExtension = _activePage.FileExtension,
                                PageNumber = 1,
                                FileFormat = FileFormatModel.Xls,
                                FileType = FileTypeModel.Native
                            };
                            MainView mainView = new MainView(_activeDocument, page1);
                            mainView.ShowDialog();
                        }

                        break;
                    case AddinAction.Dashboard:
                        if (_customTask != null)
                        {
                            if (_customTask.Visible)
                            {
                                _customTask.Visible = false;
                            }
                            else
                            {
                                _customTask.Visible = true;
                            }
                        }

                        break;
                }

                _filePath = Application.ActiveDocument.FullName;
                _ribbon.Invalidate();
            }
        }

        private void Login()
        {
            if (LoginViewModel.LoginUser == null)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(Common.WORD_AUTO_SIGNIN_KEY);

                if (key != null)
                {
                    try
                    {
                        string username = key.GetValue("Username").ToString();
                        string password = Ecm.Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(key.GetValue("Password").ToString());

                        LoginViewModel.LoginUser = new SecurityProvider().Login(username, password);

                        if (LoginViewModel.LoginUser == null)
                        {
                            LoginView loginView = new LoginView(AddinType.Word);
                            loginView.ShowDialog();
                        }
                        else
                        {
                            WorkingFolder.Configure(LoginViewModel.LoginUser.Username);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LoginView loginView = new LoginView(AddinType.Word);
                        loginView.ShowDialog();
                    }
                }
                else
                {
                    LoginView loginView = new LoginView(AddinType.Word);
                    loginView.ShowDialog();
                }
            }
        }

        private void InitDashboard()
        {
            BaseActionForm baseForm = new BaseActionForm(new DashboardView(_activeDocument, _activePage));
            baseForm.Width = 450;
            baseForm.MinimumSize = new System.Drawing.Size(400, 800);
            _customTask = this.CustomTaskPanes.Add(baseForm, "Document");
            _customTask.Width = 450;
            _customTask.Control.Name = _activeDocument.DocumentType.Name + "_" + _activeDocument.Id;
        }

        private void AutoLogin(string filePath)
        {
            string username = AddinCommon.GetUserName(filePath, TEMP_FOLDER);
            if (username == null)
            {
                return;
            }

            string encryptedPassword = AddinCommon.GetEncryptedPassword(filePath, TEMP_FOLDER);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(encryptedPassword) && LoginViewModel.LoginUser == null)
            {
                LoginViewModel.LoginUser = new SecurityProvider().Login(username, CryptographyHelper.DecryptUsingSymmetricAlgorithm(encryptedPassword));
            }
        }

        private string CreateTempFile(Word.Document document)
        {
            try
            {
                WorkingFolder folder = new WorkingFolder(TEMP_FOLDER);
                string path = Path.Combine(folder.Dir, Guid.NewGuid().ToString() + ".docx");
                document.SaveAs(path);
                return path;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
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
            //Application.DocumentChange += Application_DocumentChange;
            Application.DocumentOpen += Application_DocumentOpen;
            Application.DocumentBeforeClose += Application_DocumentBeforeClose;
        }

        void Application_DocumentOpen(Word.Document Doc)
        {
            if (!string.IsNullOrEmpty(Doc.Path))
            {
                string filePath = Doc.Path;
                string docId = AddinCommon.GetDocId(filePath);
                AutoLogin(filePath);
                if (LoginViewModel.LoginUser != null && !string.IsNullOrEmpty(docId))
                {
                    _activeDocument = new DocumentProvider().GetDocument(Guid.Parse(docId));
                    _activePage = _activeDocument.Pages.SingleOrDefault(p => p.OriginalFileName == Doc.Name);
                }
            }
            else
            {
                _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Doc.FullName));
                _activeDocument = null;

                if (_activePage != null)
                {
                    _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);
                    _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _activeDocument.DocumentType.Name + "_" + _activeDocument.Id);
                }
            }

            _ribbon.Invalidate();
        }

        //void Application_DocumentChange()
        //{
        //    if (Application.ActiveDocument != null)
        //    {
        //        _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Application.ActiveDocument.FullName));
        //        _activeDocument = null;

        //        if (_activePage != null)
        //        {
        //            _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _activeDocument.DocumentType.Name + "_" + _activeDocument.Id);
        //            _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);
        //        }
        //    }

        //    _ribbon.Invalidate();
        //}

        void Application_DocumentBeforeClose(Word.Document Doc, ref bool Cancel)
        {
            _pages.Remove(_activePage);

            if (!_pages.Any(p => p.DocId == _activeDocument.Id))
            {
                _documents.Remove(_activeDocument);
            }


            if (_customTask != null)
            {
                CustomTaskPanes.Remove(_customTask);
                _customTask = null;
            }

            //_activePage = null;
            //_activeDocument = null;
            _ribbon.Invalidate();
        }

        #endregion
    }
}
