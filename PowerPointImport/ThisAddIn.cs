using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Ecm.PowerPointImport;
//using Ecm.PowerPointImport.ViewModel;
//using Ecm.PowerPointImport.View;
using System.IO;
using Ecm.CustomAddin.ViewModel;
using Ecm.CustomAddin.View;
using Ecm.Model;
using Ecm.CustomAddin;
using Ecm.Model.DataProvider;
using Ecm.AppHelper;
using Ecm.Utility;
using Microsoft.Win32;

namespace PowerPointImport
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
            if (!string.IsNullOrEmpty(Application.ActivePresentation.Path))
            {
                AutoLogin(Application.ActivePresentation.Path);
            }

            Login();

            if (LoginViewModel.LoginUser != null)
            {
                switch (action)
                {
                    case AddinAction.SendToAcrchive:

                        if (string.IsNullOrEmpty(Application.ActivePresentation.Path))
                        {
                            _filePath = CreateTempFile(Application.ActivePresentation);
                        }
                        else
                        {
                            _filePath = Application.ActivePresentation.FullName;
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
                        SearchView content = new SearchView(AddinType.PowerPoint);
                        content.ShowDialog();
                        _activePage = (content.DataContext as SearchViewModel).SelectedPage;
                        _activeDocument = (content.DataContext as SearchViewModel).Document;

                        if (_activeDocument != null && _activePage != null)
                        {
                            if (AddinCommon.GetAddinType(_activePage.FileExtension) == AddinType.PowerPoint)
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
                        Application.ActivePresentation.Save();

                        var page = _activeDocument.Pages.SingleOrDefault(p => p.Id == _activePage.Id);
                        page.FileBinaries = AddinCommon.GetContents(_filePath);

                        new DocumentProvider().UpdateDocuments(new List<DocumentModel> { _activeDocument });

                        Application.ActivePresentation.Close();

                        if (File.Exists(_filePath))
                        {
                            File.Delete(_filePath);
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

                _filePath = Application.ActivePresentation.FullName;
                _ribbon.Invalidate();
            }
        }

        private void Login()
        {
            if (LoginViewModel.LoginUser == null)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(Common.POWER_POINT_AUTO_SIGNIN_KEY);

                if (key != null)
                {
                    try
                    {
                        string username = key.GetValue("Username").ToString();
                        string password = Ecm.Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(key.GetValue("Password").ToString());

                        LoginViewModel.LoginUser = new SecurityProvider().Login(username, password);

                        if (LoginViewModel.LoginUser == null)
                        {
                            LoginView loginView = new LoginView(AddinType.PowerPoint);
                            loginView.ShowDialog();
                        }
                        else
                        {
                            WorkingFolder.Configure(LoginViewModel.LoginUser.Username);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LoginView loginView = new LoginView(AddinType.PowerPoint);
                        loginView.ShowDialog();
                    }
                }
                else
                {
                    LoginView loginView = new LoginView(AddinType.PowerPoint);
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

        private string CreateTempFile(PowerPoint.Presentation ppt)
        {
            try
            {
                WorkingFolder folder = new WorkingFolder(TEMP_FOLDER);
                string path = Path.Combine(folder.Dir, Guid.NewGuid().ToString() + ".pptx");
                ppt.SaveAs(path);
                return path;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
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


        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
            Application.PresentationClose += Application_PresentationClose;
            Application.PresentationOpen += Application_PresentationOpen;
        }

        void Application_PresentationOpen(PowerPoint.Presentation Pres)
        {
            if (!string.IsNullOrEmpty(Pres.Path))
            {
                string filePath = Pres.Path;
                string docId = AddinCommon.GetDocId(filePath);
                AutoLogin(filePath);
                if (LoginViewModel.LoginUser != null && !string.IsNullOrEmpty(docId))
                {
                    _activeDocument = new DocumentProvider().GetDocument(Guid.Parse(docId));
                    _activePage = _activeDocument.Pages.SingleOrDefault(p => p.OriginalFileName == Pres.Name);
                }
            }
            else
            {
                _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Pres.FullName));
                _activeDocument = null;

                if (_activePage != null)
                {
                    _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);
                    _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _activeDocument.DocumentType.Name + "_" + _activeDocument.Id);
                }

            }
            _ribbon.Invalidate();            
        }

        void Application_PresentationClose(PowerPoint.Presentation Pres)
        {
            _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Pres.FullName));
            _activeDocument = null;

            if (_activePage != null)
            {
                _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);
                _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _activeDocument.DocumentType.Name + "_" + _activeDocument.Id);
            }

            _ribbon.Invalidate();
        }

        #endregion
    }
}
