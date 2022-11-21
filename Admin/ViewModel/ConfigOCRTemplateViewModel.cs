using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ecm.DocViewer.Model;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;

namespace Ecm.Admin.ViewModel
{
    public class ConfigOCRTemplateViewModel : ComponentViewModel
    {
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        //private RelayCommand _browseCommand;
        private string[] _templateFilePath;
        private bool _isChanged;
        private LanguageModel _selectedLanguage;
        private readonly LanguageProvider _languageProvider = new LanguageProvider();
        private readonly DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private DocumentTypeModel _docType;

        public DocumentTypeModel DocType
        {
            get { return _docType; }
            set
            {
                _docType = value;
                OnPropertyChanged("DocType");
            }
        }

        public Action<bool> SaveOcrTemplateCompleted { get; set; }

        public string UserName { get { return LoginViewModel.LoginUser.Username; } }

        public string[] TemplateFilePath
        {
            get { return _templateFilePath; }
            set
            {
                _templateFilePath = value;
                OnPropertyChanged("TemplateFilePath");
            }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        public ObservableCollection<LanguageModel> Languages { get; private set; }

        public LanguageModel SelectedLanguage
        {
            get 
            {
                return DocType.OCRTemplate.Language;
            }
            set
            {
                DocType.OCRTemplate.Language = value;
                OnPropertyChanged("SelectedLanguage");
                IsChanged = true;
            }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => Save(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        //public ICommand BrowseCommand
        //{
        //    get { return _browseCommand ?? (_browseCommand = new RelayCommand(p => Browse())); }
        //}

        public new bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                OnPropertyChanged("IsChanged");
            }
        }

        public ConfigOCRTemplateViewModel(DocumentTypeModel docType)
        {
            DocType = docType;
            var orgDocType = new DocumentTypeModel
            {
                DocumentTypePermission = DocType.DocumentTypePermission,
                AnnotationPermission = DocType.AnnotationPermission,
                CreateBy = DocType.CreateBy,
                CreatedDate = DocType.CreatedDate,
                Fields = DocType.Fields,
                Id = DocType.Id,
                IsOutlook = DocType.IsOutlook,
                ModifiedBy = DocType.ModifiedBy,
                ModifiedDate = DocType.ModifiedDate,
                Name = DocType.Name,
                OCRTemplate = DocType.OCRTemplate,
                Icon = DocType.Icon,
                IsSelected = true
            };

            BatchTypes = new ObservableCollection<BatchTypeModel> //{ new BatchTypeModel{ DocumentTypes = new ObservableCollection<DocumentTypeModel> { DocType }, Name= "OCR Template"} };
                             {
                                 new BatchTypeModel
                                     {
                                         Id = Guid.Empty, 
                                         Name = "OCR Template", 
                                         DocumentTypes = new ObservableCollection<DocumentTypeModel>{orgDocType}
                                     }
                             };

            Items = new ObservableCollection<ContentItem>
                        {
                            new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, UserName, BatchTypes[0]))
                        };

            if (DocType.OCRTemplate == null)
            {
                DocumentModel document = new DocumentModel(DateTime.Now, UserName, DocType);
                ContentItem docItem = new ContentItem(document);

                Items[0].Children.Add(docItem);

                DocType.OCRTemplate = new OCRTemplateModel { OCRTemplatePages = new List<OCRTemplatePageModel>() };
            }

            Languages = new ObservableCollection<LanguageModel>(_languageProvider.GetLanguages());

            if (SelectedLanguage == null)
            {
                SelectedLanguage = Languages.FirstOrDefault();
            }
        }

        private bool CanSave()
        {
            if (!IsChanged)
            {
                return false;
            }

            //if (DocType.OCRTemplate.Language.Id == Guid.Empty)
            //{
            //    return false;
            //}

            if (Items[0].Children.Count == 0)
            {
                return false;
            }

            bool hasZoneDefined = false;
            var detectDuplicateFieldInZones = new Dictionary<Guid, int>();
            foreach (var pageItem in Items[0].Children[0].Children)
            {
                foreach (var annotationItem in pageItem.PageData.Annotations)
                {
                    if (annotationItem.OCRTemplateZone.FieldMetaData == null || annotationItem.OCRTemplateZone.FieldMetaData.Id == Guid.Empty)
                    {
                        return false;
                    }

                    hasZoneDefined = true;
                    if (detectDuplicateFieldInZones.ContainsKey(annotationItem.OCRTemplateZone.FieldMetaData.Id))
                    {
                        detectDuplicateFieldInZones[annotationItem.OCRTemplateZone.FieldMetaData.Id] += 1;
                    }
                    else
                    {
                        detectDuplicateFieldInZones.Add(annotationItem.OCRTemplateZone.FieldMetaData.Id, 1);
                    }
                }
            }

            bool hasDuplicateFieldInZones = (from p in detectDuplicateFieldInZones.Keys
                                             where detectDuplicateFieldInZones[p] > 1
                                             select p).Any();

            return hasZoneDefined && !hasDuplicateFieldInZones;
        }

        private void Save()
        {
            DocType.OCRTemplate.DocTypeId = DocType.Id;
            DocType.OCRTemplate.OCRTemplatePages.Clear();

            for (int i = 0; i < Items[0].Children[0].Children.Count; i++)
            {
                var pageItem = Items[0].Children[0].Children[i];
                var ocrTemplatePage = DocType.OCRTemplate.OCRTemplatePages.FirstOrDefault(p => p.PageIndex == i);

                if (ocrTemplatePage == null)
                {
                    ocrTemplatePage = new OCRTemplatePageModel
                                          {
                                              Id = Guid.Empty,
                                              PageIndex = i,
                                              OCRTemplateZones = new List<OCRTemplateZoneModel>()
                                          };
                    DocType.OCRTemplate.OCRTemplatePages.Add(ocrTemplatePage);
                }
                else
                {
                    ocrTemplatePage.OCRTemplateZones.Clear();
                }

                ocrTemplatePage.Binary = !string.IsNullOrEmpty(pageItem.FilePath) ? File.ReadAllBytes(pageItem.FilePath) : pageItem.Binary;
                ocrTemplatePage.DPI = pageItem.Image.DpiX;
                ocrTemplatePage.Width = pageItem.PageData.Width;
                ocrTemplatePage.Height = pageItem.PageData.Height;
                ocrTemplatePage.RotateAngle = pageItem.PageData.RotateAngle;
                ocrTemplatePage.FileExtension = pageItem.PageData.FileExtension;

                foreach (var annotationItem in pageItem.PageData.Annotations)
                {
                    ocrTemplatePage.OCRTemplateZones.Add(annotationItem.OCRTemplateZone);
                }
            }

            IsProcessing = true;
            var saveWorker = new BackgroundWorker();
            saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
            saveWorker.DoWork += SaveWorkerDoWork;
            saveWorker.RunWorkerAsync(DocType.OCRTemplate);
        }

        private void SaveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var ocrTemplate = e.Argument as OCRTemplateModel;
                if (ocrTemplate != null)
                {
                    _documentTypeProvider.SaveOCRTemplate(ocrTemplate);
                }
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
                if (SaveOcrTemplateCompleted != null)
                {
                    SaveOcrTemplateCompleted(true);
                }
                //Cancel();
            }
        }

        private void Cancel()
        {
            try
            {
                if (SaveOcrTemplateCompleted != null)
                {
                    SaveOcrTemplateCompleted(false);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        //private void Browse()
        //{
        //    string filePath = DialogService.ShowFileBrowseDialog(string.Empty);
        //    if (!string.IsNullOrEmpty(filePath))
        //    {
        //        string extension = Path.GetExtension(filePath) + string.Empty;
        //        var allowedExtensions = new[] { ".tif", ".tiff", ".png", ".bmp", ".jpg", ".jpeg", ".gif" };
        //        if (allowedExtensions.Contains(extension.ToLower()))
        //        {
        //            TemplateFilePath = filePath;
        //            IsChanged = true;
        //        }
        //        else
        //        {
        //            DialogService.ShowErrorDialog("Only image is supported. (TIFF, PNG, JPG, GIF, BMP)");
        //        }
        //    }
        //}

        private string GetFileExtension(string fileName)
        {
            var lastDotIndex = fileName.LastIndexOf(".");
            return fileName.Substring(lastDotIndex + 1);
        }
    }
}
