using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using Ecm.CaptureCustomAddIn.View;
using Ecm.CaptureCustomAddIn.ViewModel;
using System.IO;
using Ecm.CaptureCustomAddIn;
using Ecm.AppHelper;
using Ecm.CaptureModel.DataProvider;
using Ecm.Utility;
using Microsoft.Win32;

namespace PowerPointCapture
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
            PowerPoint.Presentation ppt = this.Application.ActivePresentation;


            DialogService.InitializeDefault();
            if (LoginViewModel.LoginUser == null)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(Common.EXCEL_AUTO_SIGNIN_KEY);
                if (key != null)
                {
                    try
                    {
                        string username = key.GetValue("Username").ToString();
                        string password = Ecm.Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(key.GetValue("Password").ToString());

                        LoginViewModel.LoginUser = new SecurityProvider().Login(username, password);

                        if (LoginViewModel.LoginUser == null)
                        {
                            LoginView login = new LoginView("MS Power Point Capture", AddinType.PowerPoint);
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
                    LoginView login = new LoginView("MS Power Point Capture", AddinType.PowerPoint);
                    login.ShowDialog();
                }
            } 
            
            if (LoginViewModel.LoginUser != null)
            {
                switch (action)
                {
                    case CaptureAddinAction.SendToCapture:
                        if (ppt == null)
                        {
                            return;
                        }

                        string fileName = string.Empty;
                        if (string.IsNullOrEmpty(ppt.Path))
                        {
                            fileName = CreateTempFile(ppt);
                        }
                        else
                        {
                            fileName = ppt.FullName;
                        }

                        BatchTypeSelectionView batchTypeView = new BatchTypeSelectionView(fileName, new FileInfo(fileName).Extension.Replace(".", string.Empty), FileFormatModel.Xls);
                        if (batchTypeView.ShowDialog().Value)
                        {
                            Application.ActivePresentation.Close();
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ppt.Path) && File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                        }

                        break;
                    case CaptureAddinAction.Open:
                        AssignedTaskView view = new AssignedTaskView(AddinType.PowerPoint);
                        view.ShowDialog();

                        AssignedTaskViewModel viewModel = view.DataContext as AssignedTaskViewModel;
                        if (viewModel.DocumentOpened)
                        {
                            _workItem = viewModel.Workitem;
                            _activeDocument = viewModel.SelectedContent;
                            _activePage = viewModel.SelectedPage;

                            if (AddinCommon.GetAddinType(_activePage.FileExtension) == AddinType.PowerPoint)
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
                        Application.ActivePresentation.Save();
                        _filePath = Application.ActivePresentation.FullName;

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
                                Application.ActivePresentation.Close();

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
                        Application.ActivePresentation.Save();
                        _filePath = Application.ActivePresentation.FullName;

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
                                Application.ActivePresentation.Close();

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
                                _filePath = Application.ActivePresentation.FullName;
                                _workitemProvider.RejectWorkItems(new List<Guid> { _workItem.Id }, (rejectedNoteView.DataContext as RejectedNoteViewModel).RejectedNotes);
                                Application.ActivePresentation.Close();

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

        private string CreateTempFile(PowerPoint.Presentation pre)
        {
            try
            {
                WorkingFolder folder = new WorkingFolder(TEMP_FOLDER);
                string path = Path.Combine(folder.Dir, Guid.NewGuid().ToString() + ".pptx");
                pre.SaveAs(path);
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
            if (File.Exists(Application.ActivePresentation.Path))
            {
                File.Delete(Path.Combine(Application.ActivePresentation.Path, LoginViewModel.LoginUser.Username));
            }

            LoginViewModel.LoginUser = null;
            foreach (PowerPoint.Presentation pre in Application.Presentations)
            {
                if (_pages.Any(p => p.OriginalFileName == pre.Name))
                {
                    pre.Close();
                }
            }

            _ribbon.Invalidate();
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

        void Application_PresentationOpen(PowerPoint.Presentation Pres)
        {
            if (LoginViewModel.LoginUser == null)
            {
                if (!string.IsNullOrEmpty(Pres.Path))
                {
                    string filePath = Pres.Path;
                    string docId = AddinCommon.GetDocId(filePath);
                    string batchId = AddinCommon.GetBatchId(filePath);
                    string username = AddinCommon.GetUserName(filePath, TEMP_FOLDER);

                    AutoLogin(Pres.Path);

                    if (LoginViewModel.LoginUser != null && !string.IsNullOrEmpty(docId) && !string.IsNullOrEmpty(batchId))
                    {
                        _workItem = new WorkItemProvider().GetWorkItem(Guid.Parse(batchId));
                        _activeDocument = _workItem.Documents.SingleOrDefault(p => p.Id == Guid.Parse(docId));
                        _activePage = _activeDocument.Pages.SingleOrDefault(p => p.OriginalFileName == Pres.Name);
                    }
                }
            }
            else
            {
                _activePage = _pages.SingleOrDefault(p => p.OriginalFileName == Path.GetFileName(Pres.FullName));
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

        void Application_PresentationClose(PowerPoint.Presentation Pres)
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
        #endregion
    }
}
