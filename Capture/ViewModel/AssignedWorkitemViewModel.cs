using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureViewer.Model;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Ecm.Capture.ViewModel
{
    public class AssignedWorkitemViewModel : ComponentViewModel
    {
        private readonly LookupProvider _lookupProvider;
        private readonly WorkItemProvider _workItemProvider;
        private BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private bool _isChanged;

        public AssignedWorkitemViewModel(string workItemId, Action saveCompleted)
        {
            try
            {
                _workItemProvider = new WorkItemProvider();
                _lookupProvider = new LookupProvider();
                BatchModel batch = _workItemProvider.GetWorkItem(Guid.Parse(workItemId));
                SaveCompletedAction = saveCompleted;

                WorkItem = batch;
                Initialize();

                LoadWorkItem();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public BatchModel WorkItem { get; private set; }

        public DocumentModel Document { get; private set; }

        public ObservableCollection<CommentModel> Comments { get; private set; }

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

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<DocTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        public Action SaveCompletedAction { get; set; }

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

        private new void Initialize()
        {
            try
            {
                Items = new ObservableCollection<ContentItem>
                        {
                            new ContentItem(WorkItem)
                        };

                BatchTypes = _batchTypeProvider.GetCaptureBatchTypes();
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
                document.IsRejected = WorkItem.IsRejected;
                ContentItem docItem = new ContentItem(document);
                Items[0].Children.Add(docItem);

                foreach (PageModel page in document.Pages)
                {
                    page.IsRejected = WorkItem.IsRejected;
                    ContentItem pageItem = new ContentItem(page, page.FileBinary);
                    docItem.Children.Add(pageItem);
                }
            }

            foreach (DocumentModel looseDocument in WorkItem.Documents.Where(p => p.DocTypeId == Guid.Empty).ToList())
            {
                foreach (PageModel page in looseDocument.Pages)
                {
                    page.IsRejected = WorkItem.IsRejected;
                    ContentItem pageItem = new ContentItem(page, page.FileBinary);
                    Items[0].Children.Add(pageItem);
                }
            }
        }

        private string GetWorkItemName()
        {
            return WorkItem.BatchType.Name + string.Format(" - {0} [{1}]", WorkItem.BlockingActivityName, WorkItem.Id);
        }

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        private void SaveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BatchModel batch = e.Argument as BatchModel;

                if (batch.IsRejected)
                {
                    _workItemProvider.RejectWorkItems(new List<Guid> { batch.Id }, "Reject workitem by " + LoginViewModel.LoginUser.Username);
                }
                else
                {
                    _workItemProvider.UpdateWorkItem(batch);
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

        private BatchModel GenerateBatch(ContentItem item)
        {
            BatchModel batch = item.BatchData;
            batch.IsRejected = item.Rejected;

            if (batch.Id != Guid.Empty)
            {
                batch.DeletedDocuments.AddRange((from p in item.DeletedDocuments select p.DocumentData.Id).ToList());
                batch.DeletedPages.AddRange((from p in item.DeletedPages select p.DocumentData.Id).ToList());
            }

            return batch;
        }

    }
}
