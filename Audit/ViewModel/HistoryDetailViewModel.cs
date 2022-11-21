using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Data;
using Ecm.Model;
using System.ComponentModel;
using Ecm.Model.DataProvider;
using Ecm.Domain;
using Ecm.Utility.Exceptions;
using System.Collections.ObjectModel;
using Ecm.DocViewer.Model;

namespace Ecm.Audit.ViewModel
{
    public class HistoryDetailViewModel : ComponentViewModel
    {
        private bool _isActivated;
        private string _documentName;
        private RelayCommand _closeCommand;
        private RelayCommand _showCurrentVersionCommand;
        private RelayCommand _restoreCommand;
        private DocumentVersionProvider _documentVersionProvider = new DocumentVersionProvider();
        private DocumentProvider _documentProvider = new DocumentProvider();
        private ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private Status _status;
        private DocumentVersionModel _documentVersion;
        private DocumentViewModel _viewModel;
        private bool _isCurrentDocumentEnabled = true;
        private ObservableCollection<FieldValueModel> _fieldValues = new ObservableCollection<FieldValueModel>();
        private ObservableCollection<ActionLogModel> _actionLogModel = new ObservableCollection<ActionLogModel>();
        private PermissionProvider _permissionProvider = new PermissionProvider();
        private ObservableCollection<DocumentVersionModel> _docVersions = new ObservableCollection<DocumentVersionModel>();

        public HistoryDetailViewModel(DataRow searchDataRow, MainViewModel mainViewModel)
        {
            SearchDataRow = searchDataRow;
            MainViewModel = mainViewModel;
            LoadDocumentInfo();
        }

        public HistoryDetailViewModel(DataRow searchDataRow, MainViewModel mainViewModel, Status status)
        {
            _status = status;
            SearchDataRow = searchDataRow;
            MainViewModel = mainViewModel;
            LoadDocumentInfo();
        }

        public bool AllowedRestoreDocument { get; set; }

        public DocumentViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged("ViewModel");
            }
        }

        public DataRow SearchDataRow { get; private set; }

        public DocumentModel CurrentDocument { get; private set; }

        public DocumentModel Document { get; private set; }

        public bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                if (MainViewModel.ViewModel != this)
                {
                    _isActivated = value;
                }

                OnPropertyChanged("IsActivated");
                if (IsActivated && MainViewModel.ViewModel != this)
                {
                    MainViewModel.ViewModel = this;
                }
            }
        }

        public bool IsCurrentDocumentEnabled
        {
            get { return _isCurrentDocumentEnabled; }
            set
            {
                _isCurrentDocumentEnabled = value;
                OnPropertyChanged("IsCurrentDocumentEnabled");
            }
        }

        public string DocumentName
        {
            get { return _documentName; }
            set
            {
                _documentName = value;
                OnPropertyChanged("DocumentName");
            }
        }

        public ObservableCollection<ActionLogModel> ActionLogs
        {
            get { return _actionLogModel; }
            set
            {
                _actionLogModel = value;
                OnPropertyChanged("ActionLogs");
            }
        }

        public ObservableCollection<FieldValueModel> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                _fieldValues = value;
                OnPropertyChanged("FieldValues");
            }
        }

        public MainViewModel MainViewModel { get; private set; }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => Close())); }
        }

        public RelayCommand ShowCurrentVersionCommand
        {
            get { return _showCurrentVersionCommand ?? (_showCurrentVersionCommand = new RelayCommand(p => ShowCurrentDocument())); }
        }
        public RelayCommand RestoreCommand
        {
            get { return _restoreCommand ?? (_restoreCommand = new RelayCommand(p => RestoreDocument(), p => CanRestore())); }
        }

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        public DocumentTypePermissionModel DocumentTypePermission { get; private set; }

        public ObservableCollection<DocumentVersionModel> DocumentVersions
        {
            get { return _docVersions; }
            set
            {
                _docVersions = value;
                OnPropertyChanged("DocumentVersions");
            }
        }

        public void OpenDocumentVersion(Guid documentVersionId)
        {
            try
            {
                if (_status == Status.Exiting)
                {
                    Document = _documentVersionProvider.GetDocumentVersion(documentVersionId);
                }
                else if (_status == Status.DeleteWirhDocType)
                {
                    Document = _documentVersionProvider.GetLatestDeleteDocumentVersion(documentVersionId);
                }
                else
                {
                    Document = _documentVersionProvider.GetDocumentVersion(documentVersionId);
                }
                ViewModel = new DocumentViewModel(Document);
                IsCurrentDocumentEnabled = true;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public DocumentVersionModel DocumentVersion
        {
            get { return _documentVersion; }
            set
            {
                _documentVersion = value;
                OnPropertyChanged("DocumentVersion");
            }
        }

        public void Close()
        {
            var currentIndex = MainViewModel.HistoryDetailViewModels.IndexOf(this);
            if (MainViewModel.HistoryDetailViewModels.Count > 1)
            {
                if (currentIndex == MainViewModel.HistoryDetailViewModels.Count - 1)
                {
                    currentIndex--;
                }
                else
                {
                    currentIndex++;
                }

                MainViewModel.HistoryDetailViewModels[currentIndex].IsActivated = true;
            }

            MainViewModel.HistoryDetailViewModels.Remove(this);
            CurrentDocument = null;
        }

        private void LoadDocumentInfo()
        {
            AllowedRestoreDocument = _permissionProvider.GetAuditPermissionByUser(LoginViewModel.LoginUser).AllowedRestoreDocument;
            CurrentDocument = SearchDataRow[Common.COLUMN_DOCUMENT] as DocumentModel;

            if (CurrentDocument == null)
            {
                IsProcessing = true;

                Guid documentId = Guid.Empty;

                if (_status == Status.Exiting)
                {
                    documentId = (Guid)SearchDataRow[Common.COLUMN_DOCUMENT_ID];
                }
                else
                {
                    documentId = (Guid)SearchDataRow[Common.COLUMN_DOCUMENT_VERSION_ID];
                }

                var dataRetrieveWorker = new BackgroundWorker();
                dataRetrieveWorker.DoWork += DataRetrieveWorkerDoWork;
                dataRetrieveWorker.RunWorkerCompleted += DataRetrieveWorkerRunWorkerCompleted;
                dataRetrieveWorker.RunWorkerAsync(documentId);

            }
            else
            {
                LoadDocument();
            }

        }

        private void ShowCurrentDocument()
        {
            try
            {
                if (_status == Status.Exiting)
                {
                    Document = _documentProvider.GetDocument(CurrentDocument.Id);
                }
                else if (_status == Status.Deleted)
                {
                    Document = _documentVersionProvider.GetDeletedDocumentVersion(CurrentDocument.DocVersionId);
                }
                else
                {
                    Document = _documentVersionProvider.GetLatestDeleteDocumentVersion(CurrentDocument.Id);
                }
                IList<ActionLogModel> actionLogModels = _actionLogProvider.GetActionLogByDocument(CurrentDocument.Id);
                if (actionLogModels != null)
                {
                    ActionLogs = new ObservableCollection<ActionLogModel>(actionLogModels.ToList());
                }
                else
                {
                    ActionLogs = new ObservableCollection<ActionLogModel>();
                }

                //FieldValues = new ObservableCollection<FieldValueModel>(CurrentDocument.FieldValues);

                //List<DocumentVersionModel> docVersions = _documentVersionProvider.GetDocumentVersions(CurrentDocument.Id);
                //docVersions = docVersions.Where(p => p.Version != CurrentDocument.Version).ToList();

                //DocumentVersions = new ObservableCollection<DocumentVersionModel>(docVersions);

                ViewModel = new DocumentViewModel(Document);

                IsCurrentDocumentEnabled = false;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadDocument()
        {
            DocumentName = GetDocumentName();
            IsActivated = true;
            MainViewModel.HistoryDetailViewModels.Add(this);
            ShowCurrentDocument();
            //DocumentVersions = new ObservableCollection<DocumentVersionModel>(_documentVersionProvider.GetDocumentVersions(CurrentDocument.Id));
        }

        private string GetDocumentName()
        {
            return CurrentDocument.DocumentType.Name + string.Format(" [{0}]", CurrentDocument.Id);
        }

        private void DataRetrieveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (_status == Status.Exiting)
                {
                    CurrentDocument = _documentProvider.GetDocument((Guid)e.Argument);
                }
                else if (_status == Status.Deleted)
                {
                    CurrentDocument = _documentVersionProvider.GetDeletedDocumentVersion((Guid)e.Argument);
                }
                else
                {
                    CurrentDocument = _documentVersionProvider.GetLatestDeleteDocumentVersion((Guid)e.Argument);
                }
                IList<ActionLogModel> actionLogModels = _actionLogProvider.GetActionLogByDocument(CurrentDocument.Id);
                if (actionLogModels != null)
                {
                    ActionLogs = new ObservableCollection<ActionLogModel>(actionLogModels.ToList());
                }
                else
                {
                    ActionLogs = new ObservableCollection<ActionLogModel>();
                }

                FieldValues = new ObservableCollection<FieldValueModel>(CurrentDocument.FieldValues);

                List<DocumentVersionModel> docVersions = _documentVersionProvider.GetDocumentVersions(CurrentDocument.Id);
                docVersions = docVersions.Where(p => p.Version != CurrentDocument.Version).OrderBy(h => h.Version).ToList();

                DocumentVersions = new ObservableCollection<DocumentVersionModel>(docVersions);
                if (CurrentDocument == null)
                {
                    e.Result = new WcfException("Document not found.");
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DataRetrieveWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                SearchDataRow[Common.COLUMN_DOCUMENT] = CurrentDocument;
                LoadDocument();
            }
        }

        private void RestoreDocument()
        {
            DialogService.ShowMessageDialog("Under construction");
        }

        private bool CanRestore()
        {
            return IsCurrentDocumentEnabled && AllowedRestoreDocument;
        }


    }
}
