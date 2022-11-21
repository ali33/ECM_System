using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using Ecm.CaptureCustomAddIn.ViewModel;
using Ecm.CaptureCustomAddIn.View;
using Ecm.CaptureCustomAddIn;
using System.IO;
using Ecm.AppHelper;
using Ecm.CaptureModel.DataProvider;
using Ecm.Utility;

namespace WordCapture
{
    public partial class ThisAddIn
    {
        private Ribbon _ribbon;
        private const string TEMP_FOLDER = "CaptureAddOn";
        private BatchModel _workItem;
        private DocumentModel _activeDocument;
        private PageModel _activePage;
        private List<BatchModel> _workitems = new List<BatchModel>();
        private List<DocumentModel> _documents = new List<DocumentModel>();
        private List<PageModel> _pages = new List<PageModel>();
        private Ecm.CaptureModel.DataProvider.WorkItemProvider _workitemProvider = new Ecm.CaptureModel.DataProvider.WorkItemProvider();
        private string _filePath = string.Empty;
        private Microsoft.Office.Tools.CustomTaskPane _customTask;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        public bool CanSave
        {
            get
            {
                return _workItem != null;
            }
        }

        public bool CanApprove
        {
            get
            {
                return _workItem != null;
            }
        }

        public bool CanReject
        {
            get
            {
                return _workItem != null;
            }
        }

        protected override object RequestService(Guid serviceGuid)
        {
            if (serviceGuid == typeof(Office.IRibbonExtensibility).GUID)
            {
                if (_ribbon == null)
                {
                    _ribbon = new Ribbon(StartViewer);
                }
                return _ribbon;
            }

            return base.RequestService(serviceGuid);
        }

        private void StartViewer(CaptureAddinAction action)
        {
            Word.Document document = this.Application.ActiveDocument;
            Word._Document doc = this.Application.ActiveDocument;

            DialogService.InitializeDefault();

            if (!string.IsNullOrEmpty(document.Path))
            {
                AutoLogin(document.Path);
            }
            if (LoginViewModel.LoginUser == null)
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Common.WORD_AUTO_SIGNIN_KEY);

                if (key != null)
                {
                    try
                    {
                        string username = key.GetValue("Username").ToString();
                        string password = Ecm.Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(key.GetValue("Password").ToString());

                        LoginViewModel.LoginUser = new SecurityProvider().Login(username, password);

                        if (LoginViewModel.LoginUser == null)
                        {
                            LoginView login = new LoginView("MS Word Capture", AddinType.Word);
                            login.ShowDialog();
                        }
                        else
                        {
                            WorkingFolder.Configure(LoginViewModel.LoginUser.Username);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ProcessHelper.ProcessException(ex);
                    }
                }
                else
                {
                    LoginView login = new LoginView("MS Word Capture", AddinType.Word);
                    login.ShowDialog();
                }
            } 

            if (LoginViewModel.LoginUser != null)
            {
                switch (action)
                {
                    case CaptureAddinAction.SendToCapture:
                        if (document == null)
                        {
                            return;
                        }

                        string fileName = string.Empty;
                        if (string.IsNullOrEmpty(document.Path))
                        {
                            fileName = CreateTempFile(document);
                        }
                        else
                        {
                            fileName = document.FullName;
                        }

                        BatchTypeSelectionView batchTypeView = new BatchTypeSelectionView(fileName, new FileInfo(fileName).Extension.Replace(".", string.Empty), FileFormatModel.Xls);
                        if (batchTypeView.ShowDialog().Value)
                        {
                            object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                            doc.Close(ref doNotSaveChanges, ref missing, ref missing);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(document.Path) && File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                        }

                        break;
                    case CaptureAddinAction.Open:
                        AssignedTaskView view = new AssignedTaskView(AddinType.Word);
                        view.ShowDialog();

                        AssignedTaskViewModel viewModel = view.DataContext as AssignedTaskViewModel;
                        if (viewModel.DocumentOpened)
                        {
                            _workItem = viewModel.Workitem;
                            _activeDocument = viewModel.SelectedContent;
                            _activePage = viewModel.SelectedPage;

                            if (AddinCommon.GetAddinType(_activePage.FileExtension) == AddinType.Word)
                            {
                                if (!_workitems.Any(p => p.Id == _workItem.Id))
                                {
                                    _workitems.Add(_workItem);
                                }

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
                    case CaptureAddinAction.Save:
                        Application.ActiveDocument.Save();
                        _filePath = Application.ActiveDocument.FullName;

                        if (_workItem != null && _activeDocument != null && _activePage != null)
                        {
                            PageModel page = _workItem.Documents.SingleOrDefault(p => p.Id == _activeDocument.Id).Pages.SingleOrDefault(q => q.Id == _activePage.Id);

                            if (page != null)
                            {
                                page.FileBinary = AddinCommon.GetContents(_filePath);
                            }

                            try
                            {
                                _workitemProvider.SaveWorkItem(_workItem);
                                object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;

                                doc.Close(ref doNotSaveChanges, ref missing, ref missing);

                                if (File.Exists(_filePath))
                                {
                                    File.Delete(_filePath);
                                }

                                _pages.Remove(_activePage);

                                if (!_pages.Any(p => p.DocId == _activeDocument.Id))
                                {
                                    _documents.Remove(_activeDocument);

                                    if (!_documents.Any(p => p.BatchId == _workItem.Id))
                                    {
                                        _workitems.Remove(_workItem);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ProcessHelper.ProcessException(ex);
                            }
                        }
                        break;
                    case CaptureAddinAction.Approver:
                        Application.ActiveDocument.Save();
                        _filePath = Application.ActiveDocument.FullName;

                        if (_workItem != null && _activeDocument != null && _activePage != null)
                        {
                            PageModel page = _workItem.Documents.SingleOrDefault(p => p.Id == _activeDocument.Id).Pages.SingleOrDefault(q => q.Id == _activePage.Id);

                            if (page != null)
                            {
                                page.FileBinary = AddinCommon.GetContents(_filePath);
                            }

                            try
                            {
                                _workitemProvider.ApproveWorkItems(new List<BatchModel> { _workItem });
                                object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                                doc.Close(ref doNotSaveChanges, ref missing, ref missing);

                                if (File.Exists(_filePath))
                                {
                                    File.Delete(_filePath);
                                }

                                _pages.Remove(_activePage);

                                if (!_pages.Any(p => p.DocId == _activeDocument.Id))
                                {
                                    _documents.Remove(_activeDocument);

                                    if (!_documents.Any(p => p.BatchId == _workItem.Id))
                                    {
                                        _workitems.Remove(_workItem);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ProcessHelper.ProcessException(ex);
                            }
                        }
                        break;
                    case CaptureAddinAction.Rejected:
                        try
                        {
                            RejectedNoteView rejectedNoteView = new RejectedNoteView();
                            if (rejectedNoteView.ShowDialog().Value)
                            {
                                _filePath = Application.ActiveDocument.FullName;
                                _workitemProvider.RejectWorkItems(new List<Guid> { _workItem.Id }, (rejectedNoteView.DataContext as RejectedNoteViewModel).RejectedNotes);
                                object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                                doc.Close(ref doNotSaveChanges, ref missing, ref missing);

                                if (File.Exists(_filePath))
                                {
                                    File.Delete(_filePath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ProcessHelper.ProcessException(ex);
                        }
                        break;
                    case CaptureAddinAction.Dashboard:
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
                    case CaptureAddinAction.Logout:
                        Logout();
                        break;
                    default:
                        break;
                }
            }

            _ribbon.Invalidate();
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

        private void InitDashboard()
        {
            BaseActionForm baseForm = new BaseActionForm(new DashboardView(_workItem, _activeDocument, _activePage));
            baseForm.Width = 450;
            baseForm.MinimumSize = new System.Drawing.Size(400, 400);
            _customTask = this.CustomTaskPanes.Add(baseForm, "Workitem");
            _customTask.Width = 450;
            _customTask.Control.Name = _workItem.BatchType.Name + "_" + _workItem.Id;
        }

        private void Logout()
        {
            if (File.Exists(Application.ActiveDocument.Path))
            {
                File.Delete(Path.Combine(Application.ActiveDocument.Path, LoginViewModel.LoginUser.Username));
            }

            LoginViewModel.LoginUser = null;
            foreach (Word.Document document in Application.Documents)
            {
                if (_pages.Any(p => p.OriginalFileName == document.Name))
                {
                    object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                    document.Close(ref doNotSaveChanges, ref missing, ref missing);
                }
            }

            _ribbon.Invalidate();
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
            Application.DocumentBeforeClose += Application_DocumentBeforeClose;
            Application.DocumentOpen += Application_DocumentOpen;
            Application.DocumentChange += Application_DocumentChange;
        }

        void Application_DocumentBeforeClose(Word.Document Doc, ref bool Cancel)
        {
            _pages.Remove(_activePage);

            if (!_pages.Any(p => p.DocId == _activeDocument.Id))
            {
                _documents.Remove(_activeDocument);

                if (!_documents.Any(p => p.BatchId == _workItem.Id))
                {
                    _workitems.Remove(_workItem);
                }
            }


            if (_customTask != null)
            {
                CustomTaskPanes.Remove(_customTask);
                _customTask = null;
            }

            _activePage = null;
            _activeDocument = null;
            _workItem = null;
            _ribbon.Invalidate();
        }

        void Application_DocumentOpen(Word.Document Doc)
        {
            if (LoginViewModel.LoginUser == null)
            {
                if (!string.IsNullOrEmpty(Doc.Path))
                {
                    string filePath = Doc.Path;
                    string docId = AddinCommon.GetDocId(filePath);
                    string batchId = AddinCommon.GetBatchId(filePath);
                    string username = AddinCommon.GetUserName(filePath, TEMP_FOLDER);

                    AutoLogin(Doc.Path);

                    if (LoginViewModel.LoginUser != null && !string.IsNullOrEmpty(docId) && !string.IsNullOrEmpty(batchId))
                    {
                        _workItem = new WorkItemProvider().GetWorkItem(Guid.Parse(batchId));
                        _activeDocument = _workItem.Documents.SingleOrDefault(p => p.Id == Guid.Parse(docId));
                        _activePage = _activeDocument.Pages.SingleOrDefault(p => p.OriginalFileName == Doc.Name);
                    }
                }

            }
            else
            {
                _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Doc.FullName));
                _activeDocument = null;

                if (_activePage != null)
                {
                    _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);

                    if (_activeDocument != null)
                    {
                        _workItem = _workitems.SingleOrDefault(p => p.Id == _activeDocument.BatchId);
                    }
                    _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _workItem.BatchType.Name + "_" + _workItem.Id);
                }
            }

            _ribbon.Invalidate();
        }

        void Application_DocumentChange()
        {
            _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Application.ActiveDocument.FullName));
            _activeDocument = null;

            if (_activePage != null)
            {
                _activeDocument = _documents.SingleOrDefault(p => p.Id == _activePage.DocId);

                if (_activeDocument != null)
                {
                    _workItem = _workitems.SingleOrDefault(p => p.Id == _activeDocument.BatchId);
                }
                _customTask = CustomTaskPanes.SingleOrDefault(p => p.Control.Name == _workItem.BatchType.Name + "_" + _workItem.Id);
            }

            _ribbon.Invalidate();
        }
        #endregion
    }
}
