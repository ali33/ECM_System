using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using System.Resources;
using System.Reflection;

namespace Ecm.Archive.ViewModel
{
    public class CaptureViewModel : ComponentViewModel
    {
        #region Private members
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Archive.Resources", Assembly.GetExecutingAssembly());

        private readonly DocumentTypeProvider _documentTypeProvider;
        private readonly DocumentProvider _documentProvider;
        private readonly LookupProvider _lookupProvider;
        private readonly AmbiguousDefinitionProvider _ambiguousDefinitionProvider;
        //private readonly WorkItemProvider _workItemProvider;
        private bool _isSaveEnabled = true;
        //private readonly Action _saveCompletedAction;
        #endregion

        #region Public properties

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        public ObservableCollection<AmbiguousDefinitionModel> AmbiguousDefinitions { get; private set; }

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

        public CaptureViewModel()
        {
            try
            {
                _documentTypeProvider = new DocumentTypeProvider();
                _documentProvider = new DocumentProvider();
                _lookupProvider = new LookupProvider();
                _ambiguousDefinitionProvider = new AmbiguousDefinitionProvider();

                Initialize();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void Save()
        {
            try
            {
                IsProcessing = true;
                IsSaveEnabled = false;

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

                var saveWorker = new BackgroundWorker();
                saveWorker.DoWork += SaveWorkerDoWork;
                saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
                saveWorker.RunWorkerAsync(documents);
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                IsSaveEnabled = true;
                ProcessHelper.ProcessException(ex);
            }
        }

        public DataTable GetLookupData(FieldMetaDataModel fieldMetaData, string fieldValue)
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

        #endregion

        #region Private methods

        private new void Initialize()
        {
            DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetCapturedDocumentTypes());
            ApplyCurrentDate(DocumentTypes);

            BatchTypes = new ObservableCollection<BatchTypeModel>
                             {
                                 new BatchTypeModel
                                     {
                                         Id = Guid.Empty, 
                                         Name = _resource.GetString("uiDefaultBatch"), 
                                         DocumentTypes = DocumentTypes
                                     }
                             };
            Items = new ObservableCollection<ContentItem>
                        {
                            new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, UserName, BatchTypes[0]))
                        };

            AmbiguousDefinitions = new ObservableCollection<AmbiguousDefinitionModel>(_ambiguousDefinitionProvider.GetAllAmbiguousDefinitions());
        }

        private void ApplyCurrentDate(IEnumerable<DocumentTypeModel> docTypes)
        {
            string currentDate = DateTime.Now.ToShortDateString();
            foreach (var docType in docTypes)
            {
                foreach (var field in docType.Fields)
                {
                    if (field.DataType == FieldDataType.Date && field.UseCurrentDate)
                    {
                        field.DefaultValue = currentDate;
                    }
                }
            }
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
            IsProcessing = false;
            IsSaveEnabled = true;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                SaveComplete(this, EventArgs.Empty);
            }
        }
       
        #endregion

    }
}
