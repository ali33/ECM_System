using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ecm.CaptureAdmin.Model;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using LanguageModel = Ecm.CaptureModel.LanguageModel;
//using OCRTemplateModel = Ecm.Model.OCRTemplateModel;
//using OCRTemplatePageModel = Ecm.Model.OCRTemplatePageModel;
//using OCRTemplateZoneModel = Ecm.Model.OCRTemplateZoneModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class ConfigOCRTemplateViewModel : ComponentViewModel
    {
        private RelayCommand _saveCommand;
        private RelayCommand _browseCommand;
        private string [] _templateFilePath;
        private bool _isChanged;

        public event SaveOcrTemplateEventHandler SaveOcrTemplate;

        public DocTypeModel DocType { get; private set; }

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

        public ObservableCollection<LanguageModel> Languages { get; set; }

        public LanguageModel SelectedLanguage
        {
            get { return DocType.OCRTemplate.Language; }
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

        public ConfigOCRTemplateViewModel(DocTypeModel docType, List<LanguageModel> languages)
        {
            DocType = docType;
            DocType.DocTypePermission = new DocTypePermissionModel();

            BatchTypes = new ObservableCollection<BatchTypeModel> { DocType.BatchType };

            Items = new ObservableCollection<ContentItem>
                    {
                        new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, UserName, DocType.BatchType){ Permission = BatchPermissionModel.GetAllowAll()} )
                    };

            if (DocType.OCRTemplate == null)
            {
                DocumentModel document = new DocumentModel { DocumentType = DocType, BinaryType = FileTypeModel.Image, CreatedBy = UserName, CreatedDate = DateTime.Now, DocName = "OCR Template for content type: " + DocType.Name };
                ContentItem docItem = new ContentItem(document);

                Items[0].Children.Add(docItem);

                DocType.OCRTemplate = new OCRTemplateModel { Language = new LanguageModel(), OCRTemplatePages = new List<OCRTemplatePageModel>() };
            }

            Languages = new ObservableCollection<LanguageModel>(languages);

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

            if (DocType.OCRTemplate.Language.Id == Guid.Empty)
            {
                return false;
            }

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
                    if (annotationItem.OCRTemplateZone.FieldMetaData.Id == Guid.Empty)
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

            return hasZoneDefined && !hasDuplicateFieldInZones && IsChanged;
        }

        private void Save()
        {
            DocType.OCRTemplate.DocTypeId = DocType.Id;
            DocType.OCRTemplate.OCRTemplatePages.Clear();

            for (int i = 0; i < Items[0].Children[0].Children.Count; i++)
            {
                var pageItem = Items[0].Children[0].Children[i];

                var ocrTemplatePage = new OCRTemplatePageModel
                                        {
                                            Id = Guid.Empty,
                                            PageIndex = i,
                                            OCRTemplateZones = new List<OCRTemplateZoneModel>()
                                        };

                DocType.OCRTemplate.OCRTemplatePages.Add(ocrTemplatePage);

                ocrTemplatePage.Binary = !string.IsNullOrEmpty(pageItem.FilePath) ? File.ReadAllBytes(pageItem.FilePath) : pageItem.Binary;
                ocrTemplatePage.DPI = pageItem.Image.DpiX;
                ocrTemplatePage.Height = pageItem.PageData.Height;
                ocrTemplatePage.RotateAngle = pageItem.PageData.RotateAngle;
                ocrTemplatePage.Width = pageItem.PageData.Width;
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
                    SaveOcrTemplate(DocType.Id, ocrTemplate);
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
                IsChanged = false;
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

    public class OCRTemplateMapper
    {
        public static DocTypeModel GetDocumentTypeWithOcrTemplate(CaptureModel.DocTypeModel docTypeModel)
        {
            DocTypeModel docType = new DocTypeModel
                                            {
                                                Id = docTypeModel.Id,
                                                Name = docTypeModel.Name,
                                                Fields = new ObservableCollection<FieldModel>(GetFieldMetaDatas(docTypeModel.Fields.ToList())),
                                                OCRTemplate = GetOcrTemplate(docTypeModel.OCRTemplate)
                                            };

            return docType;
        }

        public static List<FieldModel> GetFieldMetaDatas(List<CaptureModel.FieldModel> docFieldMetaDataModels)
        {
            List<FieldModel> fields = new List<FieldModel>();

            if (docFieldMetaDataModels != null && docFieldMetaDataModels.Count > 0)
            {
                docFieldMetaDataModels.ForEach(p => fields.Add(GetFieldMetaData(p)));
            }

            return fields;
        }

        public static FieldModel GetFieldMetaData(CaptureModel.FieldModel docFieldMetaDataModel)
        {
            if (docFieldMetaDataModel == null)
            {
                return null;
            }

            return new FieldModel
                       {
                           DocTypeId = docFieldMetaDataModel.DocTypeId,
                           Id = docFieldMetaDataModel.Id,
                           Name = docFieldMetaDataModel.Name
                       };
        }

        public static OCRTemplateModel GetOcrTemplate(OCRTemplateModel ocrTemplateModel)
        {
            if (ocrTemplateModel == null)
            {
                return null;
            }

            CaptureModel.OCRTemplateModel ocrTemplate = new CaptureModel.OCRTemplateModel
                                                            {
                                                                DocTypeId = ocrTemplateModel.DocTypeId,
                                                                Language = GetLanguage(ocrTemplateModel.Language)
                                                            };

            foreach (var ocrTemplatePageModel in ocrTemplateModel.OCRTemplatePages)
            {
                CaptureModel.OCRTemplatePageModel ocrTemplatePage = new CaptureModel.OCRTemplatePageModel
                                                                        {
                                                                            Binary = ocrTemplatePageModel.Binary,
                                                                            DPI = ocrTemplatePageModel.DPI,
                                                                            Id = ocrTemplatePageModel.Id,
                                                                            OCRTemplateId = ocrTemplatePageModel.OCRTemplateId,
                                                                            PageIndex = ocrTemplatePageModel.PageIndex
                                                                        };

                foreach (var ocrTemplateZoneModel in ocrTemplatePageModel.OCRTemplateZones)
                {
                    CaptureModel.OCRTemplateZoneModel ocrTemplateZone = new CaptureModel.OCRTemplateZoneModel
                                                                            {
                                                                                CreatedBy = ocrTemplateZoneModel.CreatedBy,
                                                                                CreatedOn = ocrTemplateZoneModel.CreatedOn,
                                                                                FieldMetaData = new CaptureModel.FieldModel
                                                                                                    {
                                                                                                        DocTypeId = ocrTemplateZoneModel.FieldMetaData.DocTypeId,
                                                                                                        Id = ocrTemplateZoneModel.FieldMetaData.Id,
                                                                                                        Name = ocrTemplateZoneModel.FieldMetaData.Name
                                                                                                    },
                                                                                Height = ocrTemplateZoneModel.Height,
                                                                                Left = ocrTemplateZoneModel.Left,
                                                                                ModifiedBy = ocrTemplateZoneModel.ModifiedBy,
                                                                                ModifiedOn = ocrTemplateZoneModel.ModifiedOn,
                                                                                OCRTemplatePageId = ocrTemplateZoneModel.OCRTemplatePageId,
                                                                                Top = ocrTemplateZoneModel.Top,
                                                                                Width = ocrTemplateZoneModel.Width
                                                                            };

                    ocrTemplatePage.OCRTemplateZones.Add(ocrTemplateZone);
                }

                ocrTemplate.OCRTemplatePages.Add(ocrTemplatePage);
            }

            return ocrTemplate;
        }

        //public static OCRTemplateModel GetOcrTemplate(CaptureModel.OCRTemplateModel ocrTemplateModel)
        //{
        //    if (ocrTemplateModel == null)
        //    {
        //        return null;
        //    }

        //    OCRTemplateModel ocrTemplate = new OCRTemplateModel
        //                                       {
        //                                           DocTypeId = ocrTemplateModel.DocTypeId,
        //                                           FileExtension = ocrTemplateModel.FileExtension,
        //                                           Language = GetLanguage(ocrTemplateModel.Language)
        //                                       };

        //    foreach (var ocrTemplatePageModel in ocrTemplateModel.OCRTemplatePages)
        //    {
        //        OCRTemplatePageModel ocrTemplatePage = new OCRTemplatePageModel
        //                                                   {
        //                                                       Binary = ocrTemplatePageModel.Binary,
        //                                                       DPI = ocrTemplatePageModel.DPI,
        //                                                       Id = ocrTemplatePageModel.Id,
        //                                                       OCRTemplateId = ocrTemplatePageModel.OCRTemplateId,
        //                                                       PageIndex = ocrTemplatePageModel.PageIndex
        //                                                   };

        //        foreach (var ocrTemplateZoneModel in ocrTemplatePageModel.OCRTemplateZones)
        //        {
        //            OCRTemplateZoneModel ocrTemplateZone = new OCRTemplateZoneModel
        //                                                       {
        //                                                           CreatedBy = ocrTemplateZoneModel.CreatedBy,
        //                                                           CreatedOn = GetDateTime(ocrTemplateZoneModel.CreatedOn),
        //                                                           FieldMetaData = GetFieldMetaData(ocrTemplateZoneModel.FieldMetaData),
        //                                                           FieldMetaDataId = ocrTemplateZoneModel.FieldMetaDataId,
        //                                                           Height = ocrTemplateZoneModel.Height,
        //                                                           Left = ocrTemplateZoneModel.Left,
        //                                                           ModifiedBy = ocrTemplateZoneModel.ModifiedBy,
        //                                                           ModifiedOn = GetDateTime(ocrTemplateZoneModel.ModifiedOn),
        //                                                           OCRTemplatePageId = ocrTemplateZoneModel.OCRTemplatePageId,
        //                                                           Top = ocrTemplateZoneModel.Top,
        //                                                           Width = ocrTemplateZoneModel.Width
        //                                                       };

        //            ocrTemplatePage.OCRTemplateZones.Add(ocrTemplateZone);
        //        }

        //        ocrTemplate.OCRTemplatePages.Add(ocrTemplatePage);
        //    }

        //    return ocrTemplate;
        //}

        //public static LanguageModel GetLanguage(LanguageModel languageModel)
        //{
        //    return new LanguageModel
        //               {
        //                   Format = languageModel.Format,
        //                   Id = languageModel.Id,
        //                   Name = languageModel.Name
        //               };
        //}

        public static List<LanguageModel> GetLanguages(List<CaptureModel.LanguageModel> languageModels)
        {
            List<LanguageModel> languages = new List<LanguageModel>();

            if (languageModels != null && languageModels.Count > 0)
            {
                languageModels.ForEach(p => languages.Add(GetLanguage(p)));
            }

            return languages;
        }

        public static LanguageModel GetLanguage(CaptureModel.LanguageModel languageModel)
        {
            return new LanguageModel
            {
                Format = languageModel.Format,
                Id = languageModel.Id,
                Name = languageModel.Name
            };
        }

        private static DateTime GetDateTime(DateTime? dateTime)
        {
            return dateTime == null ? DateTime.Now : dateTime.Value;
        }
    }
}
