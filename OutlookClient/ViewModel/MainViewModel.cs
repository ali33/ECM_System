using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.Model;
using Ecm.DocViewer.Model;
using Ecm.Model.DataProvider;
using System.ComponentModel;
using Ecm.OutlookClient.ViewModel;
using Ecm.OutlookClient.Model;
using System.IO;
using System.Data;

namespace Ecm.OutlookClient.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        private List<MailItemInfo> _mailItemInfos;
        private LookupProvider _lookupProvider = new LookupProvider();
        #region Public properties

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        private Action CloseView { get; set; }
        #endregion

        #region Public methods

        public MainViewModel(List<MailItemInfo> mailItems, Action closeView)
        {
            CloseView = closeView;
            _documentTypeProvider = new DocumentTypeProvider();
            _documentProvider = new DocumentProvider();
            _mailItemInfos = mailItems;
            Initialize();
        }

        public void Save()
        {
            var saveWorker = new BackgroundWorker();
            saveWorker.DoWork += SaveWorkerDoWork;
            saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
            var documents = new List<DocumentModel>();
            foreach (var docItem in Items[0].Children)
            {
                documents.Add(docItem.DocumentData);
                docItem.DocumentData.Pages = new List<PageModel>();
                foreach (var pageItem in docItem.Children)
                {
                    pageItem.PageData.FileBinaries = pageItem.GetBinary(DocViewerMode.LightCapture);
                    pageItem.PageData.PageNumber = docItem.Children.IndexOf(pageItem);
                    docItem.DocumentData.Pages.Add(pageItem.PageData);
                }
            }

            saveWorker.RunWorkerAsync(documents);
        }

        public DataTable GetLookupData(FieldMetaDataModel fieldMetaData, string fieldValue)
        {
            return _lookupProvider.GetLookupData(fieldMetaData.LookupInfo, fieldValue);
        }
        #endregion

        #region Private methods

        private void Initialize()
        {
            DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetCapturedDocumentTypes().Where(d=>d.IsOutlook));
            BatchTypes = new ObservableCollection<BatchTypeModel>
                             {
                                 new BatchTypeModel
                                     {
                                         Id = Guid.Empty, 
                                         Name = "Default batch", 
                                         DocumentTypes = DocumentTypes
                                     }
                             };
            Items = new ObservableCollection<ContentItem>();

            ContentItem batchItem = new ContentItem(new BatchModel(Guid.Empty,DateTime.Now,LoginViewModel.LoginUser.Username, BatchTypes[0]));

            foreach(var mailItem in _mailItemInfos)
            {
                DocumentModel document = new DocumentModel();
                document.DocumentType = DocumentTypes[0];
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

                    if(document.FieldValues==null)
                        document.FieldValues=new List<FieldValueModel>();

                    document.FieldValues.Add(fieldValue);
                }

                ContentItem item = new ContentItem(document);
                batchItem.Children.Add(item);

                ContentItem pageItem = new ContentItem(mailItem.BodyFileName, null);
                item.Children.Add(pageItem);

                foreach (var attachment in mailItem.Attachments)
                {
                    ContentItem pageAttachItem = new ContentItem(attachment, null);
                    item.Children.Add(pageAttachItem);
                }
            }
            Items.Add(batchItem);
        }


        private void SaveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _documentProvider.InsertDocuments(e.Argument as List<DocumentModel>);
            }
            catch(Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SaveWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }

            foreach (MailItemInfo mailItem in _mailItemInfos)
            {
                if (File.Exists(mailItem.BodyFileName))
                {
                    File.Delete(mailItem.BodyFileName);
                }

                foreach (string attchmentFile in mailItem.Attachments)
                {
                    if (File.Exists(attchmentFile))
                    {
                        File.Delete(attchmentFile);
                    }
                }

                foreach (KeyValuePair<string, byte[]> embeddedFile in mailItem.EmbeddedPictures)
                {
                    if (File.Exists(embeddedFile.Key))
                    {
                        File.Delete(embeddedFile.Key);
                    }
                }
            }

            if (CloseView != null)
            {
                CloseView();
            }
        }
       
        #endregion

        #region Private members

        private readonly DocumentTypeProvider _documentTypeProvider;
        private readonly DocumentProvider _documentProvider;

        #endregion

    }
}
