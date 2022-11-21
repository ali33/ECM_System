using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.CaptureModel;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel.DataProvider;
using System.Collections.Generic;
using Ecm.CaptureDomain;
using System.Data;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Reflection;

namespace Ecm.Capture.ViewModel
{
    public class CaptureViewModel : ComponentViewModel
    {
        #region Private members
        private BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private WorkItemProvider _batchProvider = new WorkItemProvider();
        private AmbiguousDefinitionProvider _ambiguousDefinitionProvider = new AmbiguousDefinitionProvider();
        private LookupProvider _lookupProvider = new LookupProvider();
        private SettingProvider _settingProvider = new SettingProvider();
        private readonly Action _saveCompletedAction;
        private bool _isSaveEnabled = true;
        private BatchTypeModel _selectedBatchType;
        private ObservableCollection<ContentItem> _items = new ObservableCollection<ContentItem>();

        #endregion

        #region Public properties
        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public BatchTypeModel SelectedBatchType
        {
            get { return _selectedBatchType; }
            set
            {
                _selectedBatchType = value;
                OnPropertyChanged("SelectedBatchType");
            }
        }

        public ObservableCollection<DocTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }

        public ObservableCollection<AmbiguousDefinitionModel> AmbiguousDefinitions { get; private set; }

        public SettingModel Setting { get; set; }

        public ObservableCollection<CommentModel> Comments { get; private set; }

        public event EventHandler SaveComplete;

        public bool IsSaveEnabled
        {
            get { return _isSaveEnabled; }
            set
            {
                _isSaveEnabled = value;
                OnPropertyChanged("IsSaveEnabled");
            }
        }
        #endregion

        #region Public methods

        public CaptureViewModel(Action saveCompletedAction)
        {
            try
            {
                Initialize();
                _saveCompletedAction = saveCompletedAction;
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

        public void SaveAll()
        {
            var batchs = new ObservableCollection<BatchModel>((from p in Items
                                                               select GenerateBatch(p)).Where(batchInstance => batchInstance != null).ToList());

            Save(batchs);
        }

        public void Save(ContentItem item)
        {
            Save(new ObservableCollection<BatchModel> { GenerateBatch(item) });
        }

        public void Save(ObservableCollection<BatchModel> batchs)
        {
            IsProcessing = true;
            foreach (BatchModel batch in batchs)
            {
                BackgroundWorker saveWorker = new BackgroundWorker();
                saveWorker.DoWork += DoSave;
                saveWorker.RunWorkerCompleted += DoSaveCompleted;
                saveWorker.RunWorkerAsync(batch);
            }
        }

        #endregion

        #region Private methods

        private BatchModel GenerateBatch(ContentItem item)
        {
            ResourceManager resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());
            //BatchModel batch = item.BatchData;
            if (item.BatchData.Id == Guid.Empty || item.IsChanged)
            {
                int totalPages = 0;
                int totalDocs = 0;

                BatchModel batch = new BatchModel
                {
                    BatchName = LoginViewModel.LoginUser.Username + "_" + DateTime.Now.ToShortDateString(),
                    BatchType = item.BatchData.BatchType,
                    BatchTypeId = item.BatchData.BatchType.Id,
                    CreatedBy = item.BatchData.CreatedBy,
                    CreatedDate = item.BatchData.CreatedDate,
                    IsRejected = item.BatchData.IsRejected,
                    Comments = item.BatchData.Comments
                };

                // HungLe - 2014/07/17 - Adding batch name if user input - Start
                if (!string.IsNullOrWhiteSpace(item.BatchData.BatchName))
                {
                    batch.BatchName = item.BatchData.BatchName;
                }
                // HungLe - 2014/07/17 - Adding batch name if user input - End

                batch.FieldValues = item.BatchData.FieldValues;

                var loosePages = item.Children.Where(p => p.ItemType == ContentItemType.Page).ToList();
                totalPages += loosePages.Count;

                if (loosePages.Count > 0)
                {
                    var looseDoc = new DocumentModel
                    {
                        DocTypeId = Guid.Empty,
                        BatchId = item.BatchData.Id,
                        CreatedDate = item.BatchData.CreatedDate,
                        PageCount = loosePages.Count,
                        CreatedBy = LoginViewModel.LoginUser.Username,
                        DocName = string.Format(resource.GetString("LooseDocNameFormat"), LoginViewModel.LoginUser.Username, DateTime.Now)
                    };

                    totalDocs++;

                    foreach (var loosePage in loosePages)
                    {
                        if (loosePage.Rejected)
                        {
                            continue;
                        }

                        looseDoc.Pages.Add(loosePage.PageData);
                    }

                    if (looseDoc.Pages.Count != 0)
                    {
                        batch.Documents.Add(looseDoc);
                    }
                }

                var documents = item.Children.Where(p => p.ItemType == ContentItemType.Document).ToList();

                foreach (var docItem in documents)
                {
                    if (docItem.Rejected)
                    {
                        continue;
                    }

                    totalDocs++;

                    if (docItem.DocumentData.Id == Guid.Empty || docItem.IsChanged)
                    {
                        var document = new DocumentModel
                        {
                            BatchId = batch.Id,
                            BinaryType = docItem.DocumentData.BinaryType,
                            DocumentType = docItem.DocumentData.DocumentType,
                            CreatedBy = docItem.DocumentData.CreatedBy,
                            CreatedDate = docItem.DocumentData.CreatedDate,
                            DocTypeId = docItem.DocumentData.DocumentType.Id,
                            FieldValues = docItem.DocumentData.FieldValues,
                            IsRejected = docItem.DocumentData.IsRejected,
                            PageCount = docItem.DocumentData.PageCount,
                            DocName = docItem.DocumentData.DocName
                        };

                        // HungLe - 2014/07/17 - Adding doc name - Start
                        //document.DocName = docItem.DocumentData.DocName;
                        // HungLe - 2014/07/17 - Adding doc name - Start

                        document.FieldValues = docItem.DocumentData.FieldValues;

                        batch.Documents.Add(document);

                        foreach (var pageItem in docItem.Children)
                        {
                            //System.IO.File.WriteAllBytes(@"D:\test\test_before." + pageItem.PageData.FileExtension, pageItem.PageData.FileBinary);

                            var page = new PageModel
                            {
                                Annotations = pageItem.PageData.Annotations,
                                DocId = document.Id,
                                FileBinary = pageItem.PageData.FileBinary,//pageItem.GetBinary(),//.PageData.FileBinary,
                                FileExtension = pageItem.PageData.FileExtension,
                                FileFormat = pageItem.PageData.FileFormat,
                                FileHash = pageItem.PageData.FileHash,
                                FilePath = pageItem.PageData.FilePath,
                                FileType = pageItem.PageData.FileType,
                                Height = pageItem.PageData.Height,
                                Id = pageItem.PageData.Id,
                                IsRejected = pageItem.PageData.IsRejected,
                                PageNumber = pageItem.PageData.PageNumber,
                                RotateAngle = pageItem.PageData.RotateAngle,
                                Width = pageItem.PageData.Width,
                                OriginalFileName = pageItem.PageData.OriginalFileName
                            };

                            document.Pages.Add(page);
                            totalPages++;

                            //System.IO.File.WriteAllBytes(@"D:\test\test_after." + page.FileExtension, page.FileBinary);

                        }

                        if (document.Id != Guid.Empty)
                        {
                            document.DeletedPages.AddRange((from p in docItem.DeletedPages select p.PageData.Id).ToList());
                        }

                        if (batch.Id != Guid.Empty)
                        {
                            batch.DeletedDocuments.AddRange((from p in item.DeletedDocuments select p.DocumentData.Id).ToList());
                            batch.DeletedPages.AddRange((from p in item.DeletedPages select p.PageData.Id).ToList());
                        }
                    }
                }

                batch.DocCount = totalDocs;
                batch.PageCount = totalPages;

                return batch;
            }

            return null;
        }

        void DoSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                var contentItem = e.Result as ContentItem;
                SaveComplete(contentItem, new EventArgs());

            }

            IsProcessing = false;
        }

        void DoSave(object sender, DoWorkEventArgs e)
        {
            try
            {
                BatchModel batch = e.Argument as BatchModel;
                //System.IO.File.WriteAllBytes(@"D:\test." + batch.Documents[0].Pages[0].FileExtension, batch.Documents[0].Pages[0].FileBinary);

                _batchProvider.InsertBatch(batch);

                e.Result = batch;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private new void Initialize()
        {
            BatchTypes = new ObservableCollection<BatchTypeModel>(_batchTypeProvider.GetCaptureBatchTypes());
            ApplyCurrentDate(BatchTypes);

            if (BatchTypes.Count == 1)
            {
                BatchTypeModel batchType = BatchTypes[0];
                BatchModel batch = new BatchModel(Guid.NewGuid(), DateTime.Now, UserName, BatchTypes[0]) { Permission = new BatchPermissionModel() };

                batch.Permission.CanAnnotate = batchType.BatchTypePermission.CanCapture;
                batch.Permission.CanChangeDocumentType = batchType.BatchTypePermission.CanCapture;
                batch.Permission.CanDelegateItems = false;
                batch.Permission.CanDelete = batchType.BatchTypePermission.CanCapture;
                batch.Permission.CanEmail = batchType.BatchTypePermission.CanCapture;
                batch.Permission.CanInsertDocument = batchType.BatchTypePermission.CanClassify;
                batch.Permission.CanModifyDocument = batchType.BatchTypePermission.CanCapture;
                batch.Permission.CanModifyIndexes = batchType.BatchTypePermission.CanCapture && batchType.BatchTypePermission.CanIndex;
                batch.Permission.CanPrint = batchType.BatchTypePermission.CanCapture;
                batch.Permission.CanReject = false;
                batch.Permission.CanReleaseLoosePage = true;
                batch.Permission.CanSendLink = false;
                batch.Permission.CanSplitDocument = batchType.BatchTypePermission.CanCapture && batchType.BatchTypePermission.CanClassify;

                Items = new ObservableCollection<ContentItem>
                            {
                                new ContentItem(batch)
                            };

                SelectedBatchType = BatchTypes[0];
            }

            AmbiguousDefinitions = new ObservableCollection<AmbiguousDefinitionModel>(_ambiguousDefinitionProvider.GetAllAmbiguousDefinitions());
            Setting = _settingProvider.GetSettings();
            Comments = new ObservableCollection<CommentModel>();
        }

        private void ApplyCurrentDate(IEnumerable<BatchTypeModel> batchTypes)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (var batchType in batchTypes)
            {
                foreach (var field in batchType.Fields)
                {
                    if (field.DataType == FieldDataType.Date && field.UseCurrentDate)
                    {
                        field.DefaultValue = currentDate;
                    }
                }
            }
        }

        #endregion
    }
}
