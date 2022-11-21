using Ecm.AppHelper;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using PPT = Microsoft.Office.Interop.PowerPoint;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class AssignedTaskViewModel : ComponentViewModel
    {
        private RelayCommand _closeCommand;
        private RelayCommand _openDocumentCommand;
        private DocumentModel _selectedDocument;
        private PageModel _selectedPage;
        private BatchModel _selectedBatch;
        private BatchTypeModel _selectedBatchType;
        private ObservableCollection<BatchModel> _workitems = new ObservableCollection<BatchModel>();
        private ObservableCollection<DocumentModel> _contents = new ObservableCollection<DocumentModel>();
        private ObservableCollection<PageModel> _pages = new ObservableCollection<PageModel>();
        private WorkItemProvider _workitemProvider = new WorkItemProvider();
        private AddinType _addinType;
        private string[] _extensions;
        private const string TEMP_FOLDER = "CaptureAddOn";

        public AssignedTaskViewModel(Action closeView, AddinType type)
        {
            InitializeData();
            CloseView = closeView;
            _addinType = type;
            _extensions = new string[] { "xlsx", "xls", "docx", "doc", "pptx", "ppt" };

            //switch (type)
            //{
            //    case AddinType.Excel:
            //        _extensions = new string[] { "xlsx", "xls" };
            //        break;
            //    case AddinType.Word:
            //        _extensions = new string[] { "docx", "doc" };
            //        break;
            //    case AddinType.PowerPoint:
            //        _extensions = new string[] { "pptx", "ppt" };
            //        break;
            //}
        }

        public bool DocumentOpened { get; set; }

        private Action CloseView { get; set; }

        public DocumentModel SelectedContent
        {
            get { return _selectedDocument; }
            set
            {
                _selectedDocument = value;
                OnPropertyChanged("SelectedContent");

                if (value != null)
                {
                    value.Pages = value.Pages.Where(p => _extensions.Contains(p.FileExtension)).ToList(); ;
                }
                //Pages.Clear();
                //if (value != null && value.Pages != null)
                //{
                //    Pages = new ObservableCollection<PageModel>(value.Pages.Where(p=> _extensions.Contains(p.FileExtension)));
                //}
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

        public BatchModel Workitem
        {
            get { return _selectedBatch ; }
            set
            {
                _selectedBatch = value;
                OnPropertyChanged("Workitem");
                //Contents.Clear();
                //if (value != null && value.Documents != null)
                //{
                //    Contents = value.Documents;
                //}
            }
        }

        public BatchTypeModel SelectedBatchType
        {
            get { return _selectedBatchType; }
            set
            {
                _selectedBatchType = value;
                OnPropertyChanged("SelectedBatchType");

                if (value != null)
                {
                    Workitems.Clear();
                    try
                    {
                        Workitems = new ObservableCollection<BatchModel>(_workitemProvider.GetBatchs(SelectedBatchType.Id));
                    }
                    catch (Exception ex)
                    {
                        ProcessHelper.ProcessException(ex);
                    }
                }
            }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; set; }

        public ObservableCollection<BatchModel> Workitems
        {
            get { return _workitems; }
            set
            {
                _workitems = value;
                OnPropertyChanged("Workitems");
            }
        }

        public ObservableCollection<DocumentModel> Contents
        {
            get { return _contents; }
            set
            {
                _contents = value;
                OnPropertyChanged("Contents");
            }
        }

        public ObservableCollection<PageModel> Pages
        {
            get { return _pages; }
            set
            {
                _pages = value;
                OnPropertyChanged("Pages");
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(p => Close());
                }

                return _closeCommand;
            }
        }

        public ICommand OpenDocumentCommand
        {
            get
            {
                if (_openDocumentCommand == null)
                {
                    _openDocumentCommand = new RelayCommand(p => OpenOfficeDocument(p));
                }

                return _openDocumentCommand;
            }
        }
        //Private methods

        private void InitializeData()
        {
            try
            {
                BatchTypes = new BatchTypeProvider().GetAssignWorkBatchTypes();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void Close()
        {
            if (CloseView != null)
            {
                CloseView();
            }
        }

        private void OpenOfficeDocument(object p)
        {
            AddinType type = (AddinType)p;

            if (SelectedPage != null)
            {
                try
                {
                    _workitemProvider.LockWorkItems(new List<Guid> { Workitem.Id });
                    WorkingFolder folder = new WorkingFolder(TEMP_FOLDER);
                    string tempPath = folder.Dir;

                    tempPath = Path.Combine(tempPath, SelectedContent.DocumentType.Name);
                    tempPath = Path.Combine(tempPath, Workitem.Id.ToString());
                    tempPath = Path.Combine(tempPath, SelectedContent.Id.ToString());

                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    if (string.IsNullOrEmpty(SelectedPage.OriginalFileName))
                    {
                        SelectedPage.OriginalFileName = Guid.NewGuid().ToString() + "." + SelectedPage.FileExtension;
                    }

                    string filePath = Path.Combine(tempPath, SelectedPage.OriginalFileName);
                    string userPath = Path.Combine(folder.Dir, LoginViewModel.LoginUser.Username);

                    File.WriteAllText(userPath, LoginViewModel.LoginUser.EncryptedPassword);
                    File.WriteAllBytes(filePath, SelectedPage.FileBinary);

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

                    if (CloseView != null)
                    {
                        CloseView();
                    }

                    DocumentOpened = true;
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }

        }

        void app_WorkbookOpen(Excel.Workbook Wb)
        {
            Excel.Worksheet sheet = Wb.Sheets[1];
            Excel.Range range = sheet.Rows;
        }
    }
}
