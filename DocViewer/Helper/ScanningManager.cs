using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Ecm.BarcodeProcessing;
using Ecm.Model;
using Ecm.Utility;
using Ecm.DocViewer.Model;

namespace Ecm.DocViewer.Helper
{
    public class ScanningManager
    {
        public ScanningManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            InitializeData();
        }

        public void ReplaceContent(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.Replace;
            TargetPage = targetPage;
            TwainUtil.Scan(DisplayScannerDialog);
        }

        public void InsertContentBefore(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.InsertBefore;
            TargetPage = targetPage;
            TwainUtil.Scan(DisplayScannerDialog);
        }

        public void InsertContentAfter(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.InsertAfter;
            TargetPage = targetPage;
            TwainUtil.Scan(DisplayScannerDialog);
        }

        public void ScanOcrTemplate()
        {
            TwainUtil.Scan(DisplayScannerDialog);
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public RoutedCommand ScanCommand;

        public RoutedCommand SelectDefaultScannerCommand;

        public RoutedCommand ShowHideScannerDialogCommand;

        public RoutedCommand ScanToDocumentTypeCommand;

        public int NumberOfPagesToCreateNewDocumentInNormalScanning { private get; set; }

        internal TwainUtil TwainUtil { get; private set; }

        internal EnterContentAction EnterContentAction { get; private set; }

        internal ContentItem TargetBatch { get; private set; }

        internal ContentItem TargetDocument { get; private set; }

        internal ContentItem TargetPage { get; private set; }

        internal DocumentTypeModel TargetDocType { get; private set; }

        internal int InsertField { get; private set; }

        internal bool DisplayScannerDialog { get; private set; }

        internal List<DocumentModel> BarcodeExtractorOutputs { get; private set; }

        internal Dictionary<BarcodeTypeModel, int> FoundBarcodes { get; private set; }

        internal BarcodeExtractor BarcodeExtractor { get; private set; }

        private int scannedPage = 0;

        private void InitializeData()
        {
            TwainUtil = new TwainUtil();
            var handle = GetForegroundWindow();
            TwainUtil.Inititlize(handle.ToInt32(), ViewerContainer.WorkingFolder.Dir, ViewerContainer.AppName, PageScaned);

            var gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            ScanCommand = new RoutedCommand("Scan", typeof(ViewerContainer), gesture);
            var commandBinding = new CommandBinding(ScanCommand, Scan, CanScan);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Control) };
            SelectDefaultScannerCommand = new RoutedCommand("ChooseDefaultScanner", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SelectDefaultScannerCommand, SelectDefaultScanner);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control) };
            ShowHideScannerDialogCommand = new RoutedCommand("ShowHideScannerDialog", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ShowHideScannerDialogCommand, ShowHideScannerDialog);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ScanToDocumentTypeCommand = new RoutedCommand("ImportToDocType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ScanToDocumentTypeCommand, ScanToDocumentType, CanScanToDocType);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void PageScaned(string scanFilePath)
        {
            try
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    if (ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Count() > 0)
                    {
                        ViewerContainer.ContentItemManager.InsertPageAfter(scanFilePath, null, ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());
                    }
                    else
                    {
                        ViewerContainer.ImportOCRTemplate(new string[] { scanFilePath });
                    }

                    return;
                }
                switch (EnterContentAction)
                {
                    case EnterContentAction.UnClassify:
                        ViewerContainer.ContentItemManager.InsertPage(scanFilePath, null, TargetBatch, InsertField);
                        InsertField++;
                        break;
                    case EnterContentAction.PutInNewDoc:
                        if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0)
                        {
                            ScanWithBarcode(scanFilePath);
                            break;
                        }

                        if (NumberOfPagesToCreateNewDocumentInNormalScanning > 0)
                        {
                            if (scannedPage < NumberOfPagesToCreateNewDocumentInNormalScanning)
                            {
                                if (TargetDocument != null)
                                {
                                    ViewerContainer.ContentItemManager.InsertPage(scanFilePath, null, TargetDocument, InsertField);
                                    ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                                }
                                else
                                {
                                    TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, null, TargetBatch, TargetDocType);
                                    ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                                }
                            }
                            else
                            {
                                scannedPage = 0;
                                InsertField = 0;
                                TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, null, TargetBatch, TargetDocType);
                                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                            }

                            //InsertIndex++;
                            scannedPage++;
                        }
                        else
                        {
                            if (TargetDocument == null)
                            {
                                TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, null, TargetBatch, TargetDocType);
                                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                            }
                            else
                            {
                                ViewerContainer.ContentItemManager.InsertPage(scanFilePath, null, TargetDocument, InsertField);
                                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                            }
                        }
                        
                        InsertField++;
                        break;
                    case EnterContentAction.Replace:
                        ViewerContainer.ContentItemManager.ReplacePage(scanFilePath, null, TargetPage);
                        break;
                    case EnterContentAction.InsertBefore:
                        ViewerContainer.ContentItemManager.InsertPageBefore(scanFilePath, null, TargetPage);
                        break;
                    case EnterContentAction.InsertAfter:
                        ViewerContainer.ContentItemManager.InsertPageAfter(scanFilePath, null, TargetPage);
                        break;
                }

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ScanWithBarcode(string scanFilePath)
        {
            DocumentModel preDoc = BarcodeExtractorOutputs.Count > 0 ? BarcodeExtractorOutputs.Last() : null;
            DocumentModel postDoc = BarcodeExtractor.ProcessOnePage(scanFilePath, FoundBarcodes, preDoc);
            if (postDoc != preDoc || postDoc.Pages.Count == 1)
            {
                InsertField = 0;
                scannedPage = 0;
                if (!BarcodeExtractorOutputs.Contains(postDoc))
                {
                    BarcodeExtractorOutputs.Add(postDoc);
                }

                if (postDoc.Pages.Count == 1)
                {
                    TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, null, TargetBatch, TargetDocType);
                }
            }
            else
            {
                ViewerContainer.ContentItemManager.InsertPage(scanFilePath, null, TargetDocument, InsertField);
            }

            // Copy index value
            foreach (FieldValueModel fieldValue in postDoc.FieldValues)
            {
                var docItemFieldValue = TargetDocument.DocumentData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                docItemFieldValue.Value = fieldValue.Value;
            }

            ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
            InsertField++;
        }

        private void CanScan(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        }

        private void Scan(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                EnterContentAction = EnterContentAction.UnClassify;
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                InsertField = TargetBatch.Children.Count;
                var firstDoc = TargetBatch.Children.FirstOrDefault(p => p.ItemType == ContentItemType.Document);
                if (firstDoc != null)
                {
                    InsertField = TargetBatch.Children.IndexOf(firstDoc);
                }

                TwainUtil.Scan(DisplayScannerDialog);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void SelectDefaultScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TwainUtil.SetDefaultScanner();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ShowHideScannerDialog(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ShowScannerDialogMenu.IsChecked = DisplayScannerDialog = !DisplayScannerDialog;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanScanToDocType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        }

        private void ScanToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                TargetDocument = null;
                TargetDocType = (DocumentTypeModel)e.Parameter;
                EnterContentAction = EnterContentAction.PutInNewDoc;
                InsertField = 0;
                scannedPage = 0;
                if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0)
                {
                    BarcodeExtractor = new BarcodeExtractor(TargetDocType, ViewerContainer.GetLookupData);
                    BarcodeExtractorOutputs = new List<DocumentModel>();
                    FoundBarcodes = new Dictionary<BarcodeTypeModel, int>();
                }

                TwainUtil.Scan(DisplayScannerDialog);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
    }
}
