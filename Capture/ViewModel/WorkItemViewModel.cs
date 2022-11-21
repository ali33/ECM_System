using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using Ecm.Capture.ViewModel;
using Ecm.AppHelper;
using System.IO;
using System.Configuration;

namespace Ecm.Capture.ViewModel
{
    public class WorkItemViewModel : ComponentViewModel
    {
        #region Private members

        private bool _isActivated;
        private string _workItemName;
        private RelayCommand _closeCommand;
        private readonly Dictionary<PageModel, byte[]> _backupBinaries = new Dictionary<PageModel, byte[]>();
        private readonly LookupProvider _lookupProvider;
        private readonly WorkItemProvider _workItemProvider;
        private BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private bool _isChanged;
        private bool _readOnly;
        #endregion

        #region Public properties
        public override bool IsChanged
        {
            get
            {
                base.IsChanged = _isChanged;
                return _isChanged;
            }
            set
            {
                _isChanged = value;
                base.IsChanged = value;
                OnPropertyChanged("IsChanged");
            }
        }

        public BatchModel WorkItem { get; private set; }

        public DocumentModel Document { get; private set; }

        public ObservableCollection<CommentModel> Comments { get; private set; }

        public MainViewModel MainViewModel { get; private set; }

        public bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                if (MainViewModel.ContentViewModel != this)
                {
                    _isActivated = value;
                }

                OnPropertyChanged("IsActivated");
                if (IsActivated && MainViewModel.ContentViewModel != this)
                {
                    MainViewModel.ContentViewModel = this;
                }
            }
        }

        public string WorkItemName
        {
            get { return _workItemName; }
            set
            {
                _workItemName = value;
                OnPropertyChanged("WorkItemName");
            }
        }

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => Close(false))); }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<DocTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        public Action SaveCompletedAction { get; set; }

        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                _readOnly = value;
                OnPropertyChanged("ReadOnly");
            }
        }

        #endregion

        public WorkItemViewModel(BatchModel workItem, MainViewModel mainViewModel)
        {
            try
            {
                _workItemProvider = new WorkItemProvider();
                _lookupProvider = new LookupProvider();

                WorkItem = workItem;
                MainViewModel = mainViewModel;
                MainViewModel.ContentViewModel = (ComponentViewModel)this;

                Initialize();

                LoadWorkItem();
                //ReadOnly = !(workItem.LockedBy == LoginViewModel.LoginUser.Username || LoginViewModel.LoginUser.IsAdmin);
                ReadOnly = workItem.LockedBy != LoginViewModel.LoginUser.Username;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void Close(bool forSubmit)
        {
            try
            {
                var currentIndex = MainViewModel.WorkItemViewModels.IndexOf(this);
                if (MainViewModel.WorkItemViewModels.Count > 1)
                {
                    if (currentIndex == MainViewModel.WorkItemViewModels.Count - 1)
                    {
                        currentIndex--;
                    }
                    else
                    {
                        currentIndex++;
                    }

                    MainViewModel.WorkItemViewModels[currentIndex].IsActivated = true;
                }

                MainViewModel.WorkItemViewModels.Remove(this);

                bool canUnlock = WorkItem.LockedBy == LoginViewModel.LoginUser.Username;// || LoginViewModel.LoginUser.IsAdmin;

                if (!forSubmit && canUnlock)
                {
                    _workItemProvider.UnLockWorkItems(new List<Guid>(new[] { WorkItem.Id }));
                }

                MainViewModel.AssignedTaskViewModel.ReloadCommand.Execute(null);

            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public DataTable GetLookupData(FieldModel fieldMetaData, string fieldValue)
        {
            try
            {
                return _lookupProvider.GetLookupData(fieldMetaData.LookupInfo, fieldValue);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }

        public void Submit()
        {
            try
            {
                IsProcessing = true;

                ContentItem item = Items[0];
                BatchModel batch = GenerateBatch(item);
                var saveWorker = new BackgroundWorker();
                saveWorker.DoWork += SubmitWorkerDoWork;
                saveWorker.RunWorkerCompleted += SubmitWorkerRunWorkerCompleted;
                saveWorker.RunWorkerAsync(batch);
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(ex);
            }
        }

        public void Approve()
        {
            try
            {
                IsProcessing = true;

                BatchModel batch = GenerateBatch(Items[0]);
                var saveWorker = new BackgroundWorker();
                saveWorker.DoWork += ApproveWorkerDoWork;
                saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
                saveWorker.RunWorkerAsync(batch);
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(ex);
            }
        }

        public void Save()
        {
            try
            {
                IsProcessing = true;

                ContentItem item = Items[0];
                BatchModel batch = GenerateBatch(item);

                //ResetPageNumber(batch);

                var saveWorker = new BackgroundWorker();
                saveWorker.DoWork += SaveWorkerDoWork;
                saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
                saveWorker.RunWorkerAsync(batch);
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(ex);
            }
        }

        public void InternalSave()
        {
            try
            {
                IsProcessing = true;
                ContentItem item = Items[0];
                BatchModel batch = GenerateBatch(item);
                BackgroundWorker internalSaveWorker = new BackgroundWorker();
                internalSaveWorker.DoWork += InternalSaveDoWork;
                internalSaveWorker.RunWorkerCompleted += InternalSaveCompleted;
                internalSaveWorker.RunWorkerAsync(batch);
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(ex);
            }
        }

        #region Private methods

        private new void Initialize()
        {
            try
            {
                Items = new ObservableCollection<ContentItem>
                        {
                            new ContentItem(WorkItem)
                        };

                BatchTypes = _batchTypeProvider.GetCaptureBatchTypes();
                MainViewModel.WorkItemViewModels.Add(this);
                WorkItemName = GetWorkItemName();
                Comments = WorkItem.Comments;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadWorkItem()
        {

            foreach (DocumentModel document in WorkItem.Documents.Where(p => p.DocTypeId != Guid.Empty).ToList())
            {
                //if(document.DocumentType
                if (WorkItem.BatchType.IsApplyForOutlook && document.EmbeddedPictures != null && document.EmbeddedPictures.Count > 0)
                {
                    string outlookTemp = ConfigurationManager.AppSettings["OutlookTempDir"];
                    string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    tempPath = Path.Combine(tempPath, outlookTemp);

                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    WriteOutlookPictures(document, tempPath);
                }

                //document.IsRejected = WorkItem.IsRejected;
                ContentItem docItem = new ContentItem(document);
                docItem.IsVisible = document.DocumentType.DocTypePermission.CanAccess;

                Items[0].Children.Add(docItem);

                foreach (PageModel page in document.Pages)
                {
                    //page.IsRejected = WorkItem.IsRejected;
                    ContentItem pageItem = new ContentItem(page, page.FileBinary);
                    docItem.Children.Add(pageItem);
                }
            }

            foreach (DocumentModel looseDocument in WorkItem.Documents.Where(p => p.DocTypeId == Guid.Empty).ToList())
            {
                ContentItem looseDocItem = new ContentItem(looseDocument);
                Items[0].Children.Add(looseDocItem);

                foreach (PageModel page in looseDocument.Pages)
                {
                    ContentItem pageItem = new ContentItem(page, page.FileBinary);
                    looseDocItem.Children.Add(pageItem);
                }
            }
        }

        private string GetWorkItemName()
        {
            return WorkItem.BatchType.Name + string.Format(" - {0} [{1}]", WorkItem.BlockingActivityName, WorkItem.BatchName);
        }

        private void SaveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BatchModel batch = e.Argument as BatchModel;
                _workItemProvider.SaveWorkItem(batch);
                IsChanged = false;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SubmitWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BatchModel batch = e.Argument as BatchModel;

                if (batch.IsRejected)
                {
                    _workItemProvider.RejectWorkItems(new List<BatchModel> { batch }, "Reject workitem by " + LoginViewModel.LoginUser.Username);
                }
                else
                {
                    _workItemProvider.ApproveWorkItems(new List<BatchModel> { batch });
                }

                IsChanged = false;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void ApproveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BatchModel batch = e.Argument as BatchModel;
                batch.IsRejected = false;
                _workItemProvider.ApproveWorkItems(new List<BatchModel> { batch });

                IsChanged = false;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SaveWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;

            if (e.Result is Exception)
            {
                // Restore the binary for page
                foreach (PageModel page in _backupBinaries.Keys)
                {
                    page.FileBinary = _backupBinaries[page];
                }

                _backupBinaries.Clear();

                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                Close(true);
            }
        }

        private void SubmitWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;

            if (e.Result is Exception)
            {
                // Restore the binary for page
                foreach (PageModel page in _backupBinaries.Keys)
                {
                    page.FileBinary = _backupBinaries[page];
                }

                _backupBinaries.Clear();

                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                Close(true);
            }
        }

        private void InternalSaveDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _workItemProvider.SaveWorkItem(e.Argument as BatchModel);
                IsChanged = false;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void InternalSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;

            if (e.Result is Exception)
            {
                // Restore the binary for page
                foreach (PageModel page in _backupBinaries.Keys)
                {
                    page.FileBinary = _backupBinaries[page];
                }

                _backupBinaries.Clear();

                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                if (SaveCompletedAction != null)
                {
                    SaveCompletedAction();
                }
            }
        }

        private DocumentModel GetDocumentModel(Batch workItem)
        {
            return null;
            //return new DocumentModel(workItem.CreatedDate, workItem.CreatedBy, workItem.doc)
            //{
            //    CreatedBy = workItem.CreatedBy,
            //    CreatedDate = workItem.CreatedDate,
            //    DocumentType = ObjectMapper.GetDocumentTypeModel(workItem.DocumentType),
            //    FieldValues = ObjectMapper.GetFieldValueModels(workItem.FieldValues),
            //    Id = workItem.Id,
            //    Version = workItem.Version,
            //    ModifiedBy = workItem.ModifiedBy,
            //    ModifiedDate = workItem.ModifiedDate,
            //    PageCount = workItem.PageCount,
            //    Pages = ObjectMapper.GetPageModels(workItem.Pages),
            //    BinaryType = (FileTypeModel)Enum.Parse(typeof(FileTypeModel), workItem.BinaryType)
            //};
        }

        //private BatchModel GenerateBatch(ContentItem item)
        //{
        //    //BatchModel batch = item.BatchData;
        //        int totalPages = 0;
        //        int totalDocs = 0;

        //        BatchModel batch = item.BatchData;
        //        batch.FieldValues = item.BatchData.FieldValues;

        //        var documents = item.Children.Where(p => p.ItemType == ContentItemType.Document).ToList();

        //        foreach (var docItem in documents)
        //        {
        //            //if (docItem.Rejected)
        //            //{
        //            //    continue;
        //            //}

        //            totalDocs++;

        //            var document = new DocumentModel
        //            {
        //                BatchId = batch.Id,
        //                BinaryType = docItem.DocumentData.BinaryType,
        //                DocumentType = docItem.DocumentData.DocumentType,
        //                CreatedBy = docItem.DocumentData.CreatedBy,
        //                CreatedDate = docItem.DocumentData.CreatedDate,
        //                DocTypeId = docItem.DocumentData.DocumentType.Id,
        //                FieldValues = docItem.DocumentData.FieldValues,
        //                IsRejected = docItem.DocumentData.IsRejected,
        //                PageCount = docItem.DocumentData.PageCount
        //            };

        //            document.FieldValues = docItem.DocumentData.FieldValues;

        //            batch.Documents.Add(document);

        //            foreach (var pageItem in docItem.Children)
        //            {
        //                var page = new PageModel
        //                {
        //                    Annotations = pageItem.PageData.Annotations,
        //                    DocId = document.Id,
        //                    FileBinary = pageItem.GetBinary(),//.PageData.FileBinary,
        //                    FileExtension = pageItem.PageData.FileExtension,
        //                    FileFormat = pageItem.PageData.FileFormat,
        //                    FileHash = pageItem.PageData.FileHash,
        //                    FilePath = pageItem.PageData.FilePath,
        //                    FileType = pageItem.PageData.FileType,
        //                    Height = pageItem.PageData.Height,
        //                    Id = pageItem.PageData.Id,
        //                    IsRejected = pageItem.PageData.IsRejected,
        //                    PageNumber = pageItem.PageData.PageNumber,
        //                    RotateAngle = pageItem.PageData.RotateAngle,
        //                    Width = pageItem.PageData.Width,
        //                    OriginalFileName = pageItem.PageData.OriginalFileName
        //                };

        //                document.Pages.Add(page);
        //                totalPages++;
        //            }

        //            if (document.Id != Guid.Empty)
        //            {
        //                document.DeletedPages.AddRange((from p in docItem.DeletedPages select p.PageData.Id).ToList());
        //            }

        //            if (batch.Id != Guid.Empty)
        //            {
        //                batch.DeletedDocuments.AddRange((from p in item.DeletedDocuments select p.DocumentData.Id).ToList());
        //                batch.DeletedPages.AddRange((from p in item.DeletedPages select p.PageData.Id).ToList());
        //            }
        //        }

        //        batch.DocCount = totalDocs;
        //        batch.PageCount = totalPages;

        //        return batch;
        //}

        private BatchModel GenerateBatch(ContentItem item)
        {
            BatchModel batch = item.BatchData;
            batch.IsRejected = item.Rejected;

            batch.PageCount = batch.Documents.Select(p => p.Pages.Count).Sum();
            batch.DocCount = batch.Documents.Count;

            if (batch.Id != Guid.Empty)
            {
                batch.DeletedDocuments.AddRange((from p in item.DeletedDocuments select p.DocumentData.Id).ToList());
                batch.DeletedPages.AddRange((from p in item.DeletedPages select p.DocumentData.Id).ToList());
            }

            return batch;
        }

        private void WriteOutlookPictures(DocumentModel document, string tempPath)
        {
            foreach (KeyValuePair<string, byte[]> file in document.EmbeddedPictures)
            {
                string fileName = Path.Combine(tempPath, file.Key);
                File.WriteAllBytes(fileName, file.Value);
            }
        }

        private void ResetPageNumber(BatchModel batchModel)
        {
            foreach (var doc in batchModel.Documents)
            {
                for (int i = 0; i < doc.Pages.Count; i++)
                {
                    doc.Pages[i].PageNumber = i + 1;
                }
            }
        }

        #endregion

        public bool CheckTransactionId()
        {
            try
            {
                IsProcessing = true;

                ContentItem item = Items[0];

                var transactionId =_workItemProvider.GetTransactionId(item.BatchData.Id);

                return item.BatchData.TransactionId == transactionId;
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(ex);
                return false;
            }
        }

    }
}
