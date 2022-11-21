using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ecm.ContentViewer.Converter;
using Ecm.ContentViewer.Model;
using Ecm.Utility;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using Ecm.ContentViewer.ViewModel;

namespace Ecm.ContentViewer.Helper
{
    public class FileSystemImportManager
    {
        private BarcodeHelper barcodeHelper;

        //public RoutedCommand ImportFileSystemCommand;

        //public RoutedCommand ImportFileSystemToDocumentTypeCommand;

        public event EventHandler ImportFileSystemCompleted;

        public FileSystemImportManager(MainViewerViewModel viewerContainer)
        {
            ViewerContainer = viewerContainer;
            RegisterCommands();
        }

        public MainViewerViewModel ViewerContainer { get; private set; }

        public void DropFilesFromLocalMachine(string[] files, ContentItem target, int insertIndex)
        {
            ImportFromFiles(files, target, insertIndex);
        }

        public void ImportFromFiles(string[] files, ContentItem target, int insertIndex)
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
            //var gesture = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            //ImportFileSystemCommand = new RoutedCommand("ImportFile", typeof(ViewerContainer), gesture);
            //var commandBinding = new CommandBinding(ImportFileSystemCommand, ImportFile, CanImportFile);
            //ViewerContainer.CommandBindings.Add(commandBinding);

            //ImportFileSystemToDocumentTypeCommand = new RoutedCommand("ImportFileToDocumentType", typeof(ViewerContainer));
            //commandBinding = new CommandBinding(ImportFileSystemToDocumentTypeCommand, ImportFileToDocumentType, CanImportFileToDocumentType);
            //ViewerContainer.CommandBindings.Add(commandBinding);
        }

        internal List<string> SplitFiles(IEnumerable<string> sourceFiles, out List<FileTypeModel> fileTypes)
        {
            var outFiles = new List<string>();
            fileTypes = new List<FileTypeModel>();
            foreach (var file in sourceFiles)
            {
                var extension = (Path.GetExtension(file) + "").Replace(".", "");
                var fileType = (FileTypeModel)new FileTypeConverter().Convert(extension, null, null, null);
                fileTypes.Add(fileType);

                if (fileType == FileTypeModel.Image)
                {
                    var imageFormat = (FileFormatModel)new FileFormatConverter().Convert(extension, null, null, null);
                    if (imageFormat == FileFormatModel.Tif)
                    {
                        outFiles.AddRange(ImageProcessing.SplitTiffFile(file, ViewerContainer.WorkingFolder.Dir));
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
                outFiles.Add(ViewerContainer.WorkingFolder.Copy(file, fileName));
            }

            return outFiles;
        }

        private string[] ShowFileDialog()
        {
            var fileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                ValidateNames = true,
                Multiselect = true,
                CheckPathExists = true,
                Filter = "All Files (*.*) | *.*",
                Title = ViewerContainer.AppName
            };

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileNames;
            }

            return null;
        }

        private void ImportFiles(string[] files, ContentTypeModel documentType)
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
                    var container = parames.Param1 as ContentItem;
                    AppHelper.WorkingFolder.Configure(ViewerContainer.UserName);
                    BatchTypeModel batchTypeModel = null;
                    Guid batchTypeId = Guid.Empty;

                    if (container.ItemType == ContentItemType.Batch)
                    {
                        batchTypeId = container.BatchData.BatchType.Id;
                    }
                    else if (container.ItemType == ContentItemType.ContentModel)
                    {
                        batchTypeId = container.Parent.BatchData.BatchType.Id;
                    }
                    else
                    {
                        batchTypeId = container.Parent.Parent.BatchData.BatchType.Id;
                    }

                    batchTypeModel = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == batchTypeId)));

                    if (batchTypeModel.BarcodeConfiguration != null && ViewerContainer.EnabledBarcodeClient)
                    {
                        barcodeHelper = new BarcodeHelper(batchTypeModel, batchTypeModel.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
                    }

                    switch (parames.Action)
                    {
                        case EnterContentAction.DropFromDisk:

                            BatchModel batchModel = new BatchModel();
                            List<string> loosePages = new List<string>(parames.SplitedPageFiles);

                            if (ViewerContainer.EnabledBarcodeClient && barcodeHelper != null)
                            {
                                batchModel = barcodeHelper.Process(parames.SplitedPageFiles, out loosePages);

                                foreach (ContentModel document in batchModel.Documents)
                                {
                                    var pageFiles = document.Pages.Select(p => p.FilePath).ToList();
                                    var docType = batchTypeModel.DocTypes.SingleOrDefault(p => p.Id == document.DocTypeId);

                                    if (pageFiles.Count > 0)
                                    {
                                        var docItem = ViewerContainer.ContentItemManager.CreateDocument(pageFiles, container, docType);

                                        foreach (FieldValueModel fieldValue in document.FieldValues)
                                        {
                                            var docItemFieldValue = docItem.DocumentData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                            docItemFieldValue.Value = fieldValue.Value;
                                        }

                                        if (ViewerContainer.EnabledOcrClient)
                                        {
                                            ViewerContainer.OCRHelper.DoOCR(docItem);
                                        }
                                    }
                                }

                                foreach (FieldValueModel fieldValue in batchModel.FieldValues)
                                {
                                    if (fieldValue.Field != null && !fieldValue.Field.IsSystemField)
                                    {
                                        var batchFieldValue = batchModel.FieldValues.FirstOrDefault(p => p.Field.Name == fieldValue.Field.Name);

                                        if (batchFieldValue != null)
                                        {
                                            batchFieldValue.Value = fieldValue.Value;
                                        }
                                    }
                                }

                                if (loosePages.Count > 0)
                                {
                                    int insertIndex = container.Children.Count;
                                    ContentItem firstDoc = Enumerable.FirstOrDefault<ContentItem>(container.Children, (Func<ContentItem, bool>)(p => (p.ItemType == ContentItemType.ContentModel)));
                                    if (firstDoc != null)
                                    {
                                        insertIndex = container.Children.IndexOf(firstDoc);
                                    }
                                    this.ViewerContainer.ContentItemManager.InsertPages(loosePages, container, insertIndex);
                                }
                            }
                            else
                            {
                                ViewerContainer.ContentItemManager.InsertPages(parames.SplitedPageFiles, parames.Param1 as ContentItem, (int)parames.Param2);
                            }

                            break;
                        case EnterContentAction.PutInNewDoc:
                            var documentType = (ContentTypeModel)parames.Param2;
                            var importFiles = parames.SplitedPageFiles;
                            var importDocItem = ViewerContainer.ContentItemManager.CreateDocument(importFiles, container, documentType);

                            if (ViewerContainer.EnabledBarcodeClient && barcodeHelper != null)
                            {
                                ContentModel currentDocument = importDocItem.DocumentData;
                                currentDocument.DocTypeId = documentType.Id;
                                currentDocument.DocumentType = documentType;

                                var outBatchModel = barcodeHelper.Process(container.BatchData, currentDocument, importFiles);

                                foreach (ContentModel document in outBatchModel.Documents)
                                {
                                    foreach (FieldValueModel fieldValue in document.FieldValues)
                                    {
                                        if (!fieldValue.Field.IsSystemField)
                                        {
                                            var docItemFieldValue = importDocItem.DocumentData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                            docItemFieldValue.Value = fieldValue.Value;
                                        }
                                    }
                                }
                                foreach (FieldValueModel fieldValue in outBatchModel.FieldValues)
                                {
                                    if (!fieldValue.Field.IsSystemField)
                                    {
                                        var batchFieldValue = importDocItem.Parent.BatchData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                        batchFieldValue.Value = fieldValue.Value;
                                    }
                                }
                            }

                            if (ViewerContainer.EnabledOcrClient)
                            {
                                ViewerContainer.OCRHelper.DoOCR(importDocItem);
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
                    if (ImportFileSystemCompleted != null)
                    {
                        ImportFileSystemCompleted(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        public void ImportFile()
        {
            var fileNames = ShowFileDialog();
            if (fileNames != null)
            {
                var batch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                            ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                var insertIndex = batch.Children.Count;
                var firstDoc = batch.Children.FirstOrDefault(p => p.ItemType == ContentItemType.ContentModel);
                if (firstDoc != null)
                {
                    insertIndex = batch.Children.IndexOf(firstDoc);
                }

                ImportFromFiles(fileNames, batch, insertIndex);
            }
        }

        public void ImportFileToDocumentType(ContentTypeModel contentTypeModel)
        {
            var fileNames = ShowFileDialog();
            if (fileNames != null)
            {
                ImportFiles(fileNames, contentTypeModel);
            }
        }

        //private void CanImportFile(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        //}

        //private void ImportFile(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        var fileNames = ShowFileDialog();
        //        if (fileNames != null)
        //        {
        //            var batch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
        //                        ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
        //            var insertIndex = batch.Children.Count;
        //            var firstDoc = batch.Children.FirstOrDefault(p => p.ItemType == ContentItemType.ContentModel);
        //            if (firstDoc != null)
        //            {
        //                insertIndex = batch.Children.IndexOf(firstDoc);
        //            }

        //            ImportFromFiles(fileNames, batch, insertIndex);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}

        //private void CanImportFileToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        //}

        //private void ImportFileToDocumentType(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        var fileNames = ShowFileDialog();
        //        if (fileNames != null)
        //        {
        //            ImportFiles(fileNames, e.Parameter as ContentTypeModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}
    }
}
