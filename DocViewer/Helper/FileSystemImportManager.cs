using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ecm.BarcodeProcessing;
using Ecm.DocViewer.Converter;
using Ecm.DocViewer.Model;
using Ecm.Model;
using Ecm.Utility;
using Microsoft.Win32;

namespace Ecm.DocViewer.Helper
{
    public class FileSystemImportManager
    {
        public RoutedCommand ImportCommand;

        public RoutedCommand ImportToDocumentTypeCommand;

        public event EventHandler ImportCompleted;

        public FileSystemImportManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            RegisterCommands();
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public void DropFilesFromLocalMachine(string[] files, ContentItem target, int insertIndex)
        {
            ImportFileSystem(files, target, insertIndex);
        }

        public void ImportFileSystem(string[] files, ContentItem target, int insertIndex)
        {
            var importWorker = new BackgroundWorker();
            importWorker.DoWork += DoImport;
            importWorker.RunWorkerCompleted += DoImportCompleted;
            var parames = new ImportParameter { SourceFiles = files, Action = EnterContentAction.DropFromDisk, Param1 = target, Param2 = insertIndex };
            importWorker.RunWorkerAsync(parames);
            ViewerContainer.IsProcessing = true;
        }

        public void ReplaceContent(ContentItem target)
        {
            var fileNames = ShowFileDialog();
            if (fileNames != null)
            {
                var importWorker = new BackgroundWorker();
                importWorker.DoWork += DoImport;
                importWorker.RunWorkerCompleted += DoImportCompleted;
                var parames = new ImportParameter { SourceFiles = fileNames, Action = EnterContentAction.Replace, Param1 = target };
                importWorker.RunWorkerAsync(parames);
                ViewerContainer.IsProcessing = true;
            }
        }

        public void InsertContentBefore(ContentItem target)
        {
            var fileNames = ShowFileDialog();
            if (fileNames != null)
            {
                var importWorker = new BackgroundWorker();
                importWorker.DoWork += DoImport;
                importWorker.RunWorkerCompleted += DoImportCompleted;
                var parames = new ImportParameter { SourceFiles = fileNames, Action = EnterContentAction.InsertBefore, Param1 = target };
                importWorker.RunWorkerAsync(parames);
                ViewerContainer.IsProcessing = true;
            }
        }

        public void InsertContentAfter(ContentItem target)
        {
            var fileNames = ShowFileDialog();
            if (fileNames != null)
            {
                var importWorker = new BackgroundWorker();
                importWorker.DoWork += DoImport;
                importWorker.RunWorkerCompleted += DoImportCompleted;
                var parames = new ImportParameter { SourceFiles = fileNames, Action = EnterContentAction.InsertAfter, Param1 = target };
                importWorker.RunWorkerAsync(parames);
                ViewerContainer.IsProcessing = true;
            }
        }

        private void RegisterCommands()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            ImportCommand = new RoutedCommand("Import", typeof(ViewerContainer), gesture);
            var commandBinding = new CommandBinding(ImportCommand, Import, CanImport);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ImportToDocumentTypeCommand = new RoutedCommand("ImportToDocumentType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ImportToDocumentTypeCommand, ImportToDocumentType, CanImportToDocumentType);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        internal Dictionary<string,string> SplitFiles(IEnumerable<string> sourceFiles, out List<FileTypeModel> fileTypes)
        {
            var outFiles = new Dictionary<string, string>();
            var originalFileName = string.Empty;
            fileTypes = new List<FileTypeModel>();

            foreach (var file in sourceFiles)
            {
                var extension = (Path.GetExtension(file) + "").Replace(".", "");
                var fileType = (FileTypeModel)new FileTypeConverter().Convert(extension, null, null, null);
                fileTypes.Add(fileType);

                originalFileName = Path.GetFileName(file);

                if (fileType == FileTypeModel.Image)
                {
                    var imageFormat = (FileFormatModel)new FileFormatConverter().Convert(extension, null, null, null);
                    if (imageFormat == FileFormatModel.Tif)
                    {
                        foreach (string tiff in ImageProcessing.SplitTiffFile(file, ViewerContainer.WorkingFolder.Dir))
                        {
                            outFiles.Add(tiff, originalFileName);
                        }

                        continue;
                    }
                }

                string fileName;
                if (fileType == FileTypeModel.Image)
                {
                    fileName = "singlePage_" + Guid.NewGuid().GetHashCode() + "." + extension;
                }
                else if (fileType == FileTypeModel.Media)
                {
                    fileName = "media_" + Guid.NewGuid().GetHashCode() + "." + extension;
                }
                else
                {
                    fileName = "native_" + Guid.NewGuid().GetHashCode() + "." + extension;
                }

                fileName = fileName.Replace("-", string.Empty);
                outFiles.Add(ViewerContainer.WorkingFolder.Copy(file, fileName), originalFileName);
            }

            return outFiles;
        }

        private string[] ShowFileDialog()
        {
            string filter = "All Files (*.*) | *.*";

            if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
            {
                filter = "All Images|*.BMP;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG";
            }

            var fileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                ValidateNames = true,
                Multiselect = true,
                CheckPathExists = true,
                Filter = filter,
                Title = ViewerContainer.AppName
            };

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileNames;
            }

            return null;
        }

        private void ImportFileSystems(string[] files, DocumentTypeModel documentType)
        {
            var batch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                        ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];

            var importWorker = new BackgroundWorker();
            importWorker.DoWork += DoImport;
            importWorker.RunWorkerCompleted += DoImportCompleted;
            var parames = new ImportParameter { SourceFiles = files, Action = EnterContentAction.PutInNewDoc, Param1 = batch, Param2 = documentType };
            importWorker.RunWorkerAsync(parames);
            ViewerContainer.IsProcessing = true;
        }

        private void DoImport(object sender, DoWorkEventArgs e)
        {
            try
            {
                var parames = e.Argument as ImportParameter;
                if (parames != null)
                {
                    List<FileTypeModel> fileTypes;
                    parames.SplitedPageFiles = SplitFiles(parames.SourceFiles, out fileTypes);
                    parames.SplitedPageTypes = fileTypes;
                    e.Result = parames;
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoImportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewerContainer.IsProcessing = false;
            try
            {
                if (e.Result is Exception)
                {
                    ViewerContainer.HandleException(e.Result as Exception);
                }
                else if (e.Result is ImportParameter)
                {
                    var parames = e.Result as ImportParameter;
                    switch (parames.Action)
                    {
                        case EnterContentAction.DropFromDisk:
                            ViewerContainer.ContentItemManager.InsertPages(parames.SplitedPageFiles, parames.Param1 as ContentItem, (int)parames.Param2);
                            break;
                        case EnterContentAction.PutInNewDoc:
                            var documentType = (DocumentTypeModel) parames.Param2;
                            var container = parames.Param1 as ContentItem;
                            if (documentType.BarcodeConfigurations != null && 
                                documentType.BarcodeConfigurations.Count > 0)// && 
                                //parames.SourceFiles.Length == 1 &&
                                //parames.SplitedPageTypes[0] == FileTypeModel.Image)
                            {
                                var barcodeExtractor = new BarcodeExtractor(documentType, ViewerContainer.GetLookupData);

                                var images = parames.SplitedPageFiles.Where(p => (FileTypeModel)new FileTypeConverter().Convert((Path.GetExtension(p.Key) + "").Replace(".", ""), null, null, null) == FileTypeModel.Image);
                                var natives = parames.SplitedPageFiles.Where(p => (FileTypeModel)new FileTypeConverter().Convert((Path.GetExtension(p.Key) + "").Replace(".", ""), null, null, null) != FileTypeModel.Image).ToList();

                                List<DocumentModel> documents = barcodeExtractor.Process(images.ToDictionary(p => p.Key, p => p.Value));

                                if (documents.Count == 0)
                                {
                                    documents.Add(new DocumentModel(DateTime.Now, string.Empty, documentType));
                                    documents[0].Pages = new List<PageModel>();
                                }

                                foreach (var native in natives)
                                {
                                    documents[0].Pages.Add(new PageModel { FilePath = native.Key, OriginalFileName = native.Value});
                                }
                                
                                foreach(var document in documents)
                                {
                                    Dictionary<string, string> pageFiles = new Dictionary<string,string>();
                                    document.Pages.ForEach(p => pageFiles.Add(p.FilePath, p.OriginalFileName));
                                    
                                    if (pageFiles.Count > 0)
                                    {
                                        var docItem = ViewerContainer.ContentItemManager.CreateDocument(pageFiles, container, documentType);
                                        // Copy index value
                                        foreach(FieldValueModel fieldValue in document.FieldValues)
                                        {
                                            var docItemFieldValue = docItem.DocumentData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                            docItemFieldValue.Value = fieldValue.Value;
                                        }

                                        ViewerContainer.OCRHelper.DoOCR(docItem);
                                    }
                                }
                            }
                            else
                            {
                                var docItem = ViewerContainer.ContentItemManager.CreateDocument(parames.SplitedPageFiles, container, documentType);
                                ViewerContainer.OCRHelper.DoOCR(docItem);
                            }
                            break;
                        case EnterContentAction.Replace:
                            ViewerContainer.ContentItemManager.ReplacePages(parames.SplitedPageFiles, parames.Param1 as ContentItem);
                            break;
                        case EnterContentAction.InsertBefore:
                            ViewerContainer.ContentItemManager.InsertPagesBefore(parames.SplitedPageFiles, parames.Param1 as ContentItem);
                            break;
                        case EnterContentAction.InsertAfter:
                            ViewerContainer.ContentItemManager.InsertPagesAfter(parames.SplitedPageFiles, parames.Param1 as ContentItem);
                            break;
                    }

                    CommandManager.InvalidateRequerySuggested();
                    if (ImportCompleted != null)
                    {
                        ImportCompleted(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanImport(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        }

        private void Import(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var fileNames = ShowFileDialog();
                if (fileNames != null)
                {
                    var batch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                                ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                    var insertIndex = batch.Children.Count;
                    var firstDoc = batch.Children.FirstOrDefault(p => p.ItemType == ContentItemType.Document);
                    if (firstDoc != null)
                    {
                        insertIndex = batch.Children.IndexOf(firstDoc);
                    }

                    ImportFileSystem(fileNames, batch, insertIndex);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanImportToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        }

        private void ImportToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var fileNames = ShowFileDialog();
                if (fileNames != null)
                {
                    ImportFileSystems(fileNames, e.Parameter as DocumentTypeModel);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }
    }
}
