using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Ecm.CustomAddin.View;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Ecm.Domain;
using System.ComponentModel;
using System.Resources;
using System.Reflection;
using ExcelImport;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using PPT = Microsoft.Office.Interop.PowerPoint;
using Ecm.AppHelper;
using System.IO;
using System.Windows.Controls;

namespace Ecm.CustomAddin.ViewModel
{
    public class SearchViewModel : ComponentViewModel
    {
        #region Private members

        private DocumentTypeModel _selectedDocumentType;
        private RelayCommand _addSearchConditionCommand;
        private RelayCommand _resetSearchConditionCommand;
        private RelayCommand _advanceSearchCommand;
        private RelayCommand _openDocumentCommand;
        private RelayCommand _deleteDocumentCommand;
        private RelayCommand _removeAdditionalField;
        private RelayCommand _closeCommand;
        private readonly SearchProvider _searchProvider = new SearchProvider();
        private readonly SearchQueryProvider _searchQueryProvider = new SearchQueryProvider();
        private readonly DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private readonly DocumentProvider _documentProvider = new DocumentProvider();
        private readonly ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private ObservableCollection<SearchResultModel> _searchResults = new ObservableCollection<SearchResultModel>();
        private bool _isSearchEnabled = true;
        private bool _enableDelete;
        private bool _enablePrint;
        private bool _enableEmail;
        private ObservableCollection<PageModel> _files = new ObservableCollection<PageModel>();
        private ObservableCollection<PageModel> _moreFiles = new ObservableCollection<PageModel>();
        private PageModel _selectedPage;
        //private string _tempFolder;
        private ResourceManager _resource = new ResourceManager("Ecm.CustomAddin.Resources", Assembly.GetExecutingAssembly());
        private AddinType _addinType;
        private const string TEMP_FOLDER = "ArchiveAddOn";

        public SearchViewModel(Action closeAction, AddinType type)
        {
            CloseAction = closeAction;
            _addinType = type;

            //switch (type)
            //{
            //    case AddinType.Excel:
            //        _tempFolder = AddinCommon.EXCEL_ADDIN_TEMP_FOLDER;
            //        break;
            //    case AddinType.Word:
            //        _tempFolder = AddinCommon.WORD_ADDIN_TEMP_FOLDER;
            //        break;
            //    case AddinType.PowerPoint:
            //        _tempFolder = AddinCommon.PPT_ADDIN_TEMP_FOLDER;
            //        break;
            //}

            SearchQueryExpressions = new ObservableCollection<SearchExpressionViewModel>();
            AvailableFields = new ObservableCollection<FieldMetaDataModel>();

            Initialize();
        }

        #endregion

        #region Public properties
        public DataRow SearchDataRow { get; private set; }

        public DocumentModel Document { get; private set; }

        public bool IsSearchEnabled
        {
            get { return _isSearchEnabled; }
            set
            {
                _isSearchEnabled = value;
                OnPropertyChanged("IsSearchEnabled");
            }
        }

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public DocumentTypeModel SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");

                if (value != null)
                {
                    LoadDefaultSearchExpression();
                    LoadAvailableFields();
                    SearchResults = null;
                }
            }
        }

        public ObservableCollection<SearchExpressionViewModel> SearchQueryExpressions { get; private set; }

        public ObservableCollection<FieldMetaDataModel> AvailableFields { get; private set; }

        public ObservableCollection<SearchResultModel> SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                OnPropertyChanged("SearchResults");

                if (value != null)
                {
                    foreach(var searchResult in value)
                    {
                        searchResult.PropertyChanged += SearchResultPropertyChanged;
                    }
                }
            }
        }

        private Action CloseAction { get; set; }

        public bool EnableDelete
        {
            get { return _enableDelete; }
            set 
            { 
                _enableDelete = value;
                OnPropertyChanged("EnableDelete");
            }
        }

        public bool EnablePrint
        {
            get { return _enablePrint; }
            set
            {
                _enablePrint = value;
                OnPropertyChanged("EnablePrint");
            }
        }

        public bool EnableEmail
        {
            get { return _enableEmail; }
            set
            {
                _enableEmail = value;
                OnPropertyChanged("EnableEmail");
            }
        }

        public ObservableCollection<PageModel> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }

        public ObservableCollection<PageModel> MoreFiles
        {
            get { return _moreFiles; }
            set
            {
                _moreFiles = value;
                OnPropertyChanged("MoreFiles");
            }
        }

        public PageModel SelectedPage
        {
            get { return _selectedPage; }
            set
            {
                _selectedPage = value;
                OnPropertyChanged("SelectedPage");
            }
        }

        public ICommand AddSearchConditionCommand
        {
            get
            {
                return _addSearchConditionCommand ?? (_addSearchConditionCommand = new RelayCommand(p => AddSearchCondition(), p => CanAddCondition()));
            }
        }

        public ICommand ResetSearchConditionCommand
        {
            get
            {
                return _resetSearchConditionCommand ?? (_resetSearchConditionCommand = new RelayCommand(p => ResetSearchCondition(), p => CanResetSearchCondition()));
            }
        }

        public ICommand AdvanceSearchCommand
        {
            get { return _advanceSearchCommand ?? (_advanceSearchCommand = new RelayCommand(p => RunAdvanceSearch(), p => CanRunAdvanceSearch())); }
        }

        public ICommand OpenDocumentCommand
        {
            get { return _openDocumentCommand ?? (_openDocumentCommand = new RelayCommand(p => OpenOfficeDocument(p))); }
        }

        public ICommand DeleteDocumentCommand
        {
            get { return _deleteDocumentCommand ?? (_deleteDocumentCommand = new RelayCommand(p => DeleteDocument())); }
        }

        public ICommand RemoveAdditionalField
        {
            get
            {
                if (_removeAdditionalField == null)
                {
                    _removeAdditionalField = new RelayCommand(p => RemoveAdditionalCondition(p));
                }

                return _removeAdditionalField;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(p => CloseView());
                }

                return _closeCommand;
            }
        }

        #endregion

        #region Public methods

        public void RunAdvanceSearch()
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += RunAdvanceSearchDoWork;
            worker.RunWorkerCompleted += RunAdvanceSearchRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        public List<DocumentModel> GetSelectedDocuments()
        {
            try
            {
                var docIds = GetSelectedDocumentIds();
                return _documentProvider.GetDocuments(docIds);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;

        }

        public void LoadPageData()
        {
            DataRow selectedRow = null;
            SearchResultModel result = SearchResults.FirstOrDefault();
            selectedRow = result.DataResult.Rows.Cast<DataRow>().FirstOrDefault(s => (bool)s[Common.COLUMN_SELECTED]);
            IsProcessing = true;
            var worker = new BackgroundWorker();
            worker.DoWork += DoLoadPageData;
            worker.RunWorkerCompleted += DoLoadPageDataCompleted;
            worker.RunWorkerAsync(selectedRow);

        }

        #endregion

        #region Private methods

        private new void Initialize()
        {
            DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDocumentTypes());
            if (DocumentTypes.Count > 0)
            {
                SelectedDocumentType = DocumentTypes[0];
            }
        }

        private void LoadDefaultSearchExpression()
        {
            SearchQueryExpressions.Clear();

            int count = SelectedDocumentType.Fields.Count;
            for (int i = 0; i < count; i++)
            {
                if (!SelectedDocumentType.Fields[i].IsSystemField)
                {
                    var searchExpressionViewModel = new SearchExpressionViewModel();
                    var expression = new SearchQueryExpressionModel
                                         {
                                             Condition = i == 0 ? SearchConjunction.None : SearchConjunction.And,
                                             Field = SelectedDocumentType.Fields[i]
                                         };

                    searchExpressionViewModel.SearchQueryExpression = expression;
                    SearchQueryExpressions.Add(searchExpressionViewModel);
                }
            }
        }

        private void LoadSearchExpressionByQuery(SearchQueryModel value)
        {
            if (value != null)
            {
                LoadDefaultSearchExpression();
                foreach (var item in value.SearchQueryExpressions)
                {
                    var viewModel = new SearchExpressionViewModel();

                    if (item.Field == null)
                    {
                        var field = AvailableFields.SingleOrDefault(p => p.FieldUniqueId == item.FieldUniqueId);
                        var expression = new SearchQueryExpressionModel
                        {
                            Condition = item.Condition,
                            Field = field,
                            Id = item.Id,
                            Operator = item.Operator,
                            SearchQueryId = item.SearchQueryId,
                            Value1 = item.Value1,
                            Value2 = item.Value2
                        };

                        viewModel.SearchQueryExpression = expression;
                        SearchQueryExpressions.Add(viewModel);
                    }
                    else
                    {
                        var expression = new SearchQueryExpressionModel
                        {
                            Condition = item.Condition,
                            Field = item.Field,
                            Id = item.Id,
                            Operator = item.Operator,
                            SearchQueryId = item.SearchQueryId,
                            Value1 = item.Value1,
                            Value2 = item.Value2
                        };

                        viewModel.SearchQueryExpression = expression;

                        var existing = SearchQueryExpressions.SingleOrDefault(p => p.SearchQueryExpression.Field.Id == expression.Field.Id);

                        if (existing != null)
                        {
                            existing.SearchQueryExpression = expression;
                        }
                    }
                }
            }
        }

        private void LoadAvailableFields()
        {
            AvailableFields.Clear();
            foreach (var field in SelectedDocumentType.Fields.Where(p => !p.IsSystemField))
            {
                AvailableFields.Add(field);
            }

            FieldMetaDataModel fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Created by",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_CREATED_BY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Created date",
                DataType = FieldDataType.Date,
                FieldUniqueId = Common.DOCUMENT_CREATED_DATE
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Document Id",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_ID
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Modified by",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_MODIFIED_BY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Modified date",
                DataType = FieldDataType.Date,
                FieldUniqueId = Common.DOCUMENT_MODIFIED_DATE
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Page count",
                DataType = FieldDataType.Integer,
                FieldUniqueId = Common.DOCUMENT_PAGE_COUNT
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Binary type",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_FILE_BINARY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Version",
                DataType = FieldDataType.Integer,
                FieldUniqueId = Common.DOCUMENT_VERSION
            };

            AvailableFields.Add(fieldModel);

        }

        private bool CanAddCondition()
        {
            return SelectedDocumentType != null;
        }

        private void AddSearchCondition()
        {
            var viewModel = new SearchExpressionViewModel();
            var expression = new SearchQueryExpressionModel { Condition = SearchConjunction.And, Operator = SearchOperator.Equal };

            //if (SelectedSearchQuery != null)
            //{
            //    expression.SearchQueryId = SelectedSearchQuery.Id;
            //}

            viewModel.IsAdditionCondition = true;
            viewModel.SearchQueryExpression = expression;
            SearchQueryExpressions.Add(viewModel);
        }

        private void RemoveAdditionalCondition(object para)
        {
            var searchExpressionViewModel = para as SearchExpressionViewModel;
            SearchQueryExpressions.Remove(searchExpressionViewModel);
        }

        private bool CanResetSearchCondition()
        {
            return SearchQueryExpressions.Any(p => p.IsAdditionCondition ||
                                                   string.IsNullOrEmpty(p.SearchQueryExpression.Value1) ||
                                                   string.IsNullOrEmpty(p.SearchQueryExpression.Value2));
        }

        private void ResetSearchCondition()
        {
            LoadDefaultSearchExpression();
            //SelectedSearchQuery = null;
        }

        private bool CanSaveQuery()
        {
            return SearchQueryExpressions != null && SearchQueryExpressions.Count > 0 &&
                   SearchQueryExpressions.Any(p => !string.IsNullOrEmpty(p.SearchQueryExpression.Value1) || !string.IsNullOrEmpty(p.SearchQueryExpression.Value2));
        }

        private ObservableCollection<SearchQueryExpressionModel> GetSearchExpressions()
        {
            var expressions = new ObservableCollection<SearchQueryExpressionModel>();
            var valuedExpressions = SearchQueryExpressions.Where(p => !string.IsNullOrEmpty(p.SearchQueryExpression.Value1) &&
                                                                       (p.SearchQueryExpression.Operator != SearchOperator.InBetween ||
                                                                        !string.IsNullOrEmpty(p.SearchQueryExpression.Value2)));
            foreach (var item in valuedExpressions)
            {
                var searchQueryExpression = new SearchQueryExpressionModel
                {
                    Condition = item.SearchQueryExpression.Condition,
                    Field = item.SearchQueryExpression.Field,
                    Id = item.SearchQueryExpression.Id,
                    Operator = item.SearchQueryExpression.Operator,
                    OperatorText = item.SearchQueryExpression.OperatorText,
                    SearchQueryId = item.SearchQueryExpression.SearchQueryId,
                    Value1 = item.SearchQueryExpression.Value1,
                    Value2 = item.SearchQueryExpression.Value2,
                    FieldUniqueId = item.SearchQueryExpression.Field.FieldUniqueId
                };

                expressions.Add(searchQueryExpression);
            }

            if (expressions.Count > 0)
            {
                expressions[0].Condition = SearchConjunction.None;
            }

            return expressions;
        }

        private bool CanRunAdvanceSearch()
        {
            return SelectedDocumentType != null;
        }

        private void RunAdvanceSearchDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SearchResultModel searchResult = _searchProvider.RunAdvanceSearch(0, SelectedDocumentType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });
                if (searchResult != null)
                {
                    var searchResults = new ObservableCollection<SearchResultModel> { searchResult };
                    e.Result = searchResults;
                }
                else
                {
                    e.Result = null;
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void RunAdvanceSearchRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSearchEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    SearchResults = e.Result as ObservableCollection<SearchResultModel>;
                }
                catch(Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }
        }

        private void DeleteDocument()
        {
            var selectedRows = new List<DataRow>();
            foreach (var searchResult in SearchResults)
            {
                selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_CHECKED]));
            }

            if (selectedRows.Count > 0)
            {
                if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDeleteDocument")) == DialogServiceResult.Yes)
                {
                    IsProcessing = true;
                    IsSearchEnabled = false;

                    var deleteWorker = new BackgroundWorker();
                    deleteWorker.DoWork += DeleteWorkerDoWork;
                    deleteWorker.RunWorkerCompleted += DeleteWorkerRunWorkerCompleted;
                    deleteWorker.RunWorkerAsync(selectedRows);
                }
            }
        }

        private void DeleteWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var selectedRows = (List<DataRow>)e.Argument;
                foreach (var selectedRow in selectedRows)
                {
                    _documentProvider.DeleteDocument(Guid.Parse(selectedRow[Common.COLUMN_DOCUMENT_ID].ToString()));
                }

                e.Result = selectedRows;
            }
            catch(Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DeleteWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSearchEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                var selectedRows = (List<DataRow>)e.Result;
                foreach (DataRow row in selectedRows)
                {
                    SearchResultModel result = SearchResults.First(p => p.DocumentType.Id == Guid.Parse(row[Common.COLUMN_DOCUMENT_TYPE_ID].ToString()));
                    result.DataResult.Rows.Remove(row);
                }
            }
        }

        private void SearchResultPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                bool hasCompoundDoc = false;
                bool hasNativeDoc = false;
                bool hasMediaDoc = false;
                EnableDelete = true;
                EnablePrint = true;
                EnableEmail = true;

                foreach (var result in SearchResults)
                {
                    var selectedRows = result.DataResult.Rows.OfType<DataRow>().Where(p => (bool)p[Common.COLUMN_SELECTED]).ToList();
                    if (selectedRows.Count > 0)
                    {
                        hasCompoundDoc = hasCompoundDoc || selectedRows.Any(p => (FileTypeModel) p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Compound);
                        hasNativeDoc = hasNativeDoc || selectedRows.Any(p => (FileTypeModel) p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Native);
                        hasMediaDoc = hasMediaDoc || selectedRows.Any(p => (FileTypeModel) p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Media);

                        EnableDelete = EnableDelete && result.DocumentType.DocumentTypePermission.AllowedDeletePage;
                        EnablePrint = EnablePrint && result.DocumentType.DocumentTypePermission.AlowedPrintDocument;
                        EnableEmail = EnableEmail && result.DocumentType.DocumentTypePermission.AllowedEmailDocument;
                    }
                }

                if (hasCompoundDoc || hasNativeDoc || hasMediaDoc)
                {
                    EnablePrint = false;
                }

                if (hasCompoundDoc)
                {
                    EnableEmail = false;
                }
            }
        }

        private void GetDocumentData()
        {
            Document = SearchDataRow[Common.COLUMN_DOCUMENT] as DocumentModel;
            if (Document == null)
            {
                IsProcessing = true;

                var documentId = Guid.Parse(SearchDataRow[Common.COLUMN_DOCUMENT_ID].ToString());
                Document = _documentProvider.GetDocument(documentId);
            }
        }

        private List<Guid> GetSelectedDocumentIds()
        {
            try
            {
                var selectedRows = new List<DataRow>();
                foreach (var searchResult in SearchResults)
                {
                    selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_CHECKED]));
                }

                var selectedDocIds = new List<Guid>();
                if (selectedRows.Count > 0)
                {
                    selectedDocIds.AddRange(selectedRows.Select(searchRow => Guid.Parse(searchRow[Common.COLUMN_DOCUMENT_ID].ToString())));
                }

                return selectedDocIds;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }

        private void DoLoadPageData(object sender, DoWorkEventArgs e)
        {
            var selectedRow = e.Argument as DataRow;
            Document = selectedRow[Common.COLUMN_DOCUMENT] as DocumentModel;

            if (Document == null)
            {
                var documentId =  Guid.Parse(selectedRow[Common.COLUMN_DOCUMENT_ID].ToString());

                try
                {
                    Document = _documentProvider.GetDocument(documentId);
                    e.Result = Document;
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            }


        }

        private void DoLoadPageDataCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                DialogService.ShowErrorDialog((e.Result as Exception).Message);
            }

            var docModel = e.Result as DocumentModel;
            List<PageModel> files = new List<PageModel>();
            List<PageModel> moreFiles = new List<PageModel>();
            string[] extensions = new string[] { "xls", "xlsx", "doc", "docx", "ppt", "pptx" };
            files = docModel.Pages.Where(p => extensions.Contains(p.FileExtension)).ToList();
            moreFiles = docModel.Pages.Where(p => !extensions.Contains(p.FileExtension)).ToList();

            //switch (_addinType)
            //{
            //    case AddinType.Excel:
            //        files = docModel.Pages.Where(p => p.FileExtension == "xls" || p.FileExtension == "xlsx").ToList();
            //        moreFiles = docModel.Pages.Where(p => p.FileExtension != "xls" && p.FileExtension != "xlsx").ToList();
            //        break;
            //    case AddinType.Word:
            //        files = docModel.Pages.Where(p => p.FileExtension == "doc" || p.FileExtension == "docx").ToList();
            //        moreFiles = docModel.Pages.Where(p => p.FileExtension != "doc" && p.FileExtension != "docx").ToList();
            //        break;
            //    case AddinType.PowerPoint:
            //        files = docModel.Pages.Where(p => p.FileExtension == "ppt" || p.FileExtension == "pptx").ToList();
            //        moreFiles = docModel.Pages.Where(p => p.FileExtension != "ppt" && p.FileExtension != "pptx").ToList();
            //        break;
            //    default:
            //        break;
            //}

            Files = new ObservableCollection<PageModel>(files);
            MoreFiles = new ObservableCollection<PageModel>(moreFiles);

            IsProcessing = false;
        }

        private void OpenOfficeDocument(object p)
        {
            var pageModel = ((ContentControl)p).Content as PageModel;

            if (pageModel != null)
            {
                try
                {
                    WorkingFolder folder = new WorkingFolder(TEMP_FOLDER);
                    string tempPath = folder.Dir;
                    DocumentModel docModel = new DocumentProvider().GetDocument(pageModel.DocId);

                    tempPath = Path.Combine(tempPath, docModel.DocumentType.Name);
                    tempPath = Path.Combine(tempPath, docModel.Id.ToString());

                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    string filePath = Path.Combine(tempPath, pageModel.OriginalFileName);
                    string userPath = Path.Combine(folder.Dir, LoginViewModel.LoginUser.Username);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.WriteAllBytes(filePath, pageModel.FileBinaries);
                    File.WriteAllText(userPath, LoginViewModel.LoginUser.EncryptedPassword);

                    AddinType type = AddinCommon.GetAddinType(pageModel.FileExtension);

                    switch (type)
                    {
                        case AddinType.Excel:
                            Excel.Application app = null;

                            if (type == _addinType)
                            {
                                app = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                            }
                            else
                            {
                                app = new Excel.Application();
                            }

                            app.Workbooks.Open(filePath);
                            app.Visible = true;
                            break;
                        case AddinType.Word:
                            Word.Application appWord = null;

                            if (type == _addinType)
                            {
                                appWord = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
                            }
                            else
                            {
                                appWord = new Word.Application();
                            }

                            appWord.Documents.Open(filePath);
                            appWord.Visible = true;
                            break;
                        case AddinType.PowerPoint:
                            PPT.Application appPPT = null;

                            if (type == _addinType)
                            {
                                appPPT = (PPT.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("PowerPoint.Application");
                            }
                            else
                            {
                                appPPT = new PPT.Application();
                            }

                            appPPT.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                            appPPT.Presentations.Open(filePath, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue);
                            break;
                    }

                    if (CloseAction != null)
                    {
                        CloseAction();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        void app_WorkbookOpen(Excel.Workbook Wb)
        {
            Excel.Worksheet sheet = Wb.Sheets[1];
            Excel.Range range = sheet.Rows;
        }

        private void CloseView()
        {
            if (CloseAction != null)
            {
                CloseAction();
            }
        }

        #endregion

    }
}
