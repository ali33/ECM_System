using Ecm.AppHelper;
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
using System.IO;
using System.Linq;
using System.Text;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class MainViewModel : ComponentViewModel
    {
        #region Private members
        private BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private WorkItemProvider _batchProvider = new WorkItemProvider();
        private AmbiguousDefinitionProvider _ambiguousDefinitionProvider = new AmbiguousDefinitionProvider();
        private LookupProvider _lookupProvider = new LookupProvider();
        private SettingProvider _settingProvider = new SettingProvider();
        private bool _isSaveEnabled = true;
        private BatchTypeModel _selectedBatchType;
        private ObservableCollection<ContentItem> _items = new ObservableCollection<ContentItem>();
        private List<MailItemInfo> _mailItems;

        #endregion
        public MainViewModel(BatchTypeModel batchType, DocTypeModel contentType, string filePath, string extension, FileFormatModel fileFormat)
        {
            try
            {
                InitializeData(batchType, contentType, filePath, extension, fileFormat);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public MainViewModel(BatchTypeModel batchType, DocTypeModel contentType, List<MailItemInfo> mailInfos)
        {
            try
            {
                _mailItems = mailInfos;
                InitializeData(batchType, contentType, mailInfos);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

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

        public BatchModel Item { get; set; }

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
                        CreatedBy = LoginViewModel.LoginUser.Username
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
                            PageCount = docItem.DocumentData.PageCount
                        };

                        document.FieldValues = docItem.DocumentData.FieldValues;
                        document.EmbeddedPictures = docItem.DocumentData.EmbeddedPictures;

                        batch.Documents.Add(document);

                        foreach (var pageItem in docItem.Children)
                        {
                            var page = new PageModel
                            {
                                Annotations = pageItem.PageData.Annotations,
                                DocId = document.Id,
                                FileBinary = pageItem.GetBinary(),//.PageData.FileBinary,
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
                _batchProvider.InsertBatch(batch);

                e.Result = batch;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void InitializeData(BatchTypeModel batchType, DocTypeModel contentType, string filePath, string extension, FileFormatModel fileFormat)
        {
            BatchTypes = _batchTypeProvider.GetCaptureBatchTypes();
            ApplyCurrentDate(BatchTypes);

            Items = new ObservableCollection<ContentItem>();

            ContentItem item = new ContentItem(new BatchModel(Guid.NewGuid(), DateTime.Now, UserName, batchType)
            {
                Permission = new BatchPermissionModel
                {
                    CanAnnotate = batchType.BatchTypePermission.CanCapture,
                    CanChangeDocumentType = batchType.BatchTypePermission.CanCapture,
                    CanDelegateItems = false,
                    CanDelete = batchType.BatchTypePermission.CanCapture,
                    CanEmail = batchType.BatchTypePermission.CanCapture,
                    CanInsertDocument = batchType.BatchTypePermission.CanCapture,
                    CanModifyDocument = batchType.BatchTypePermission.CanCapture,
                    CanModifyIndexes = batchType.BatchTypePermission.CanCapture,
                    CanPrint = batchType.BatchTypePermission.CanCapture,
                    CanReject = false,
                    CanReleaseLoosePage = true,
                    CanSendLink = false,
                    CanSplitDocument = batchType.BatchTypePermission.CanCapture
                }
            });

            DocumentModel document = new DocumentModel
            {
                BinaryType = FileTypeModel.Native,
                DocTypeId = contentType.Id,
                DocumentType = contentType,
                CreatedBy = LoginViewModel.LoginUser.Username,
                CreatedDate = DateTime.Now
            };

            foreach (FieldModel field in document.DocumentType.Fields)
            {
                if (field.IsSystemField)
                {
                    continue;
                }

                FieldValueModel fieldValue = new FieldValueModel { FieldId = field.Id, Field = field };

                if (field.DataType == FieldDataType.Table)
                {
                    fieldValue.TableValues = new ObservableCollection<TableFieldValueModel>();
                }

                document.FieldValues.Add(fieldValue);
            }

            ContentItem docItem = new ContentItem(document);
            item.Children.Add(docItem);

            PageModel page = new PageModel
            {
                FileExtension = extension,
                FileFormat = fileFormat,
                FileType = FileTypeModel.Native
            };

            byte[] fileBinary = AddinCommon.GetContents(filePath);

            ContentItem pageItem = new ContentItem(page, fileBinary);

            docItem.Children.Add(pageItem);

            Items.Add(item);

            SelectedBatchType = batchType;
            AmbiguousDefinitions = new ObservableCollection<AmbiguousDefinitionModel>(_ambiguousDefinitionProvider.GetAllAmbiguousDefinitions());
            Setting = _settingProvider.GetSettings();
            Comments = new ObservableCollection<CommentModel>();
        }

        private void InitializeData(BatchTypeModel batchType, DocTypeModel contentType, List<MailItemInfo> mailInfos)
        {
            BatchTypes = _batchTypeProvider.GetCaptureBatchTypes();
            ApplyCurrentDate(BatchTypes);
            Items = new ObservableCollection<ContentItem>();

            ContentItem batchItem = new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, LoginViewModel.LoginUser.Username, batchType)
                {
                    Permission = new BatchPermissionModel
                        {
                            //CanAnnotate = batchType.BatchTypePermission.CanCapture,
                            //CanChangeDocumentType = batchType.BatchTypePermission.CanCapture,
                            //CanDelegateItems = false,
                            //CanDelete = batchType.BatchTypePermission.CanCapture,
                            //CanEmail = batchType.BatchTypePermission.CanCapture,
                            //CanInsertDocument = batchType.BatchTypePermission.CanCapture,
                            //CanModifyDocument = batchType.BatchTypePermission.CanCapture,
                            //CanModifyIndexes = batchType.BatchTypePermission.CanCapture,
                            //CanPrint = batchType.BatchTypePermission.CanCapture,
                            //CanReject = false,
                            //CanReleaseLoosePage = true,
                            //CanSendLink = false,
                            //CanSplitDocument = batchType.BatchTypePermission.CanCapture,

                            CanAnnotate = batchType.BatchTypePermission.CanCapture,
                            CanChangeDocumentType = batchType.BatchTypePermission.CanCapture,
                            CanDelegateItems = false,
                            CanDelete = batchType.BatchTypePermission.CanCapture,
                            CanEmail = batchType.BatchTypePermission.CanCapture,
                            CanInsertDocument = batchType.BatchTypePermission.CanClassify,
                            CanModifyDocument = batchType.BatchTypePermission.CanCapture,
                            CanModifyIndexes = batchType.BatchTypePermission.CanCapture && batchType.BatchTypePermission.CanIndex,
                            CanPrint = batchType.BatchTypePermission.CanCapture,
                            CanReject = false,
                            CanReleaseLoosePage = true,
                            CanSendLink = false,
                            CanSplitDocument = batchType.BatchTypePermission.CanCapture && batchType.BatchTypePermission.CanClassify,
                        }
                
                });

            foreach (var mailItem in mailInfos)
            {
                DocumentModel document = new DocumentModel();
                document.DocumentType = contentType;
                document.CreatedBy = LoginViewModel.LoginUser.Username;
                document.CreatedDate = DateTime.Now;

                foreach (KeyValuePair<string, byte[]> pic in mailItem.EmbeddedPictures)
                {
                    document.EmbeddedPictures = mailItem.EmbeddedPictures;
                }


                foreach (var field in document.DocumentType.Fields)
                {
                    if (field.IsSystemField)
                    {
                        continue;
                    }

                    FieldValueModel fieldValue = new FieldValueModel();

                    fieldValue.Field = field;

                    switch (field.Name.ToUpper())
                    {
                        case "MAIL FROM":
                            fieldValue.Value = mailItem.MailFrom;
                            break;
                        case "MAIL TO":
                            fieldValue.Value = mailItem.MailTo;
                            break;
                        case "MAIL SUBJECT":
                            fieldValue.Value = mailItem.MailSubject;
                            break;
                        case "RECEIVED DATE":
                            fieldValue.Value = mailItem.ReceivedDate.ToShortDateString();
                            break;
                        case "MAIL BODY":
                            fieldValue.Value = mailItem.MailBody;
                            break;
                    }

                    if (document.FieldValues == null)
                        document.FieldValues = new List<FieldValueModel>();

                    document.FieldValues.Add(fieldValue);
                }

                ContentItem item = new ContentItem(document);
                batchItem.Children.Add(item);

                ContentItem pageItem = new ContentItem(mailItem.BodyFileName);
                item.Children.Add(pageItem);

                foreach (var attachment in mailItem.Attachments)
                {
                    ContentItem pageAttachItem = new ContentItem(attachment);
                    item.Children.Add(pageAttachItem);
                }
            }

            Items.Add(batchItem);
            
            SelectedBatchType = batchType;
            AmbiguousDefinitions = new ObservableCollection<AmbiguousDefinitionModel>(_ambiguousDefinitionProvider.GetAllAmbiguousDefinitions());
            Setting = _settingProvider.GetSettings();
            Comments = new ObservableCollection<CommentModel>();
        }

        private void ApplyCurrentDate(IEnumerable<BatchTypeModel> batchTypes)
        {
            string currentDate = DateTime.Now.ToShortDateString();
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
