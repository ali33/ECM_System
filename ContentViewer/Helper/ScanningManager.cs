using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Ecm.ContentViewer.Model;
using Ecm.Utility;
using Ecm.CaptureBarcodeProcessing;
using Ecm.ContentViewer.BarcodeHelper;
using Ecm.ContentViewer.ViewModel;

namespace Ecm.ContentViewer.Helper
{
    public class ScanningManager
    {
        private int scannedPage = 0;
        private BatchModel _barcodeBatch;
        private BarcodeHelper _barcodeHelper;
        private List<BarcodeData> _barcodeDatum;
        private List<string> _loosePages;
        private ContentModel _currentDocument;
        private BatchTypeModel _batchType;

        //public RoutedCommand ScanCommand;

        //public RoutedCommand SelectDefaultScannerCommand;

        //public RoutedCommand ShowHideScannerDialogCommand;

        //public RoutedCommand ScanToDocumentTypeCommand;

        public MainViewerViewModel ViewerContainer { get; private set; }

        public int NumberOfPagesToCreateNewDocumentInNormalScanning { private get; set; }

        internal TwainUtil TwainUtil { get; private set; }

        internal EnterContentAction EnterContentAction { get; private set; }

        internal ContentItem TargetBatch { get; private set; }

        internal ContentItem TargetDocument { get; private set; }

        internal ContentItem TargetPage { get; private set; }

        internal ContentTypeModel TargetDocType { get; private set; }

        internal int InsertField { get; private set; }

        internal bool ShowScannerDialog { get; private set; }

        internal List<ContentModel> BarcodeExtractorOutputs { get; private set; }

        internal Dictionary<BarcodeTypeModel, int> FoundBarcodes { get; private set; }

        public ScanningManager(MainViewerViewModel viewerContainer)
        {
            ViewerContainer = viewerContainer;
            _barcodeDatum = new List<BarcodeData>();
            _loosePages = new List<string>();
            InitializeData();
        }

        public void ReplaceContent(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.Replace;
            TargetPage = targetPage;
            TwainUtil.Scan(ShowScannerDialog);
        }

        public void InsertContentBefore(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.InsertBefore;
            TargetPage = targetPage;
            TwainUtil.Scan(ShowScannerDialog);
        }

        public void InsertContentAfter(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.InsertAfter;
            TargetPage = targetPage;
            TwainUtil.Scan(ShowScannerDialog);
        }

        private void InitializeData()
        {
            TwainUtil = new TwainUtil();
            var handle = GetForegroundWindow();
            TwainUtil.Inititlize(handle.ToInt32(), ViewerContainer.WorkingFolder.Dir, ViewerContainer.AppName, PageScaned);

            //var gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            //ScanCommand = new RoutedCommand("Scan", typeof(ViewerContainer), gesture);
            //var commandBinding = new CommandBinding(ScanCommand, ScanContent, CanScanContent);
            //ViewerContainer.CommandBindings.Add(commandBinding);

            //gesture = new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Control) };
            //SelectDefaultScannerCommand = new RoutedCommand("SelectDefaultScanner", typeof(ViewerContainer), gesture);
            //commandBinding = new CommandBinding(SelectDefaultScannerCommand, SelectDefaultScanner);
            //ViewerContainer.CommandBindings.Add(commandBinding);

            //gesture = new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control) };
            //ShowHideScannerDialogCommand = new RoutedCommand("ShowHideScannerDialog", typeof(ViewerContainer), gesture);
            //commandBinding = new CommandBinding(ShowHideScannerDialogCommand, ShowHideScannerDialog);
            //ViewerContainer.CommandBindings.Add(commandBinding);

            //ScanToDocumentTypeCommand = new RoutedCommand("ImportToDocumentType", typeof(ViewerContainer));
            //commandBinding = new CommandBinding(ScanToDocumentTypeCommand, ScanToDocumentType, CanScanToDocumentType);
            //ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void PageScaned(string scanFilePath)
        {
            try
            {
                var container = TargetBatch;

                if(TargetPage !=null)
                {
                    container = TargetPage.BatchItem;
                }

                switch (EnterContentAction)
                {
                    case EnterContentAction.UnClassify:
                        if (_barcodeHelper == null)
                        {
                            ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetBatch, InsertField);
                            InsertField++;
                            break;
                        }

                        ContentModel outDocument  = _barcodeHelper.Process(_barcodeBatch, _loosePages, _currentDocument, scanFilePath, _barcodeDatum);

                        if (_loosePages.Count <= 0)
                        {
                            break;
                        }

                        if (outDocument != null)
                        {
                            if (outDocument.Pages.Count > 0)
                            {
                                TargetDocument = new ContentItem(outDocument);
                                container.Children.Add(TargetDocument);
                                ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, 0);
                            }
                        }
                        else if (TargetDocument != null)
                        {
                            foreach (FieldValueModel fieldValue in outDocument.FieldValues)
                            {
                                FieldValueModel documentField = TargetDocument.DocumentData.FieldValues.FirstOrDefault(p => p.Field.Id == fieldValue.FieldId);
                                if (!((documentField == null) || string.IsNullOrEmpty(fieldValue.Value)))
                                {
                                    documentField.Value = fieldValue.Value;
                                }
                            }
                        }

                        if (_currentDocument != null && TargetDocument != null && outDocument.Pages.Count > this._currentDocument.Pages.Count)
                        {
                            ViewerContainer.ContentItemManager.InsertPage(scanFilePath, this.TargetDocument, this.TargetDocument.Children.Count);
                        }

                        foreach (FieldValueModel batchField in _barcodeBatch.FieldValues)
                        {
                            FieldValueModel fieldModel = _barcodeBatch.FieldValues.FirstOrDefault(p => p.Field.Id == batchField.FieldId);
                            if (((fieldModel.Field != null) && !fieldModel.Field.IsSystemField) && !string.IsNullOrEmpty(fieldModel.Value))
                            {
                                if (fieldModel != null)
                                {
                                    fieldModel.Value = batchField.Value;
                                }
                            }
                        }
                        _currentDocument = outDocument;

                        ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetBatch, InsertField);
                        InsertField++;
                        _loosePages.Clear();

                        break;
                    case EnterContentAction.PutInNewDoc:

                        if (NumberOfPagesToCreateNewDocumentInNormalScanning > 0)
                        {
                            if (scannedPage < NumberOfPagesToCreateNewDocumentInNormalScanning)
                            {
                                if (TargetDocument != null)
                                {
                                    ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, InsertField);

                                    if (ViewerContainer.EnabledOcrClient)
                                    {
                                        ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                                    }
                                }
                                else
                                {
                                    TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);

                                    if (ViewerContainer.EnabledOcrClient)
                                    {
                                        ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                                    }
                                }
                            }
                            else
                            {
                                scannedPage = 0;
                                InsertField = 0;
                                TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);

                                if (ViewerContainer.EnabledOcrClient)
                                {
                                    ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                                }
                            }

                            //InsertIndex++;
                            scannedPage++;
                        }
                        else
                        {
                            if (TargetDocument == null)
                            {
                                TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);

                                if (ViewerContainer.EnabledOcrClient)
                                {
                                    ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                                }
                            }
                            else
                            {
                                ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, InsertField);

                                if (ViewerContainer.EnabledOcrClient)
                                {
                                    ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                                }
                            }
                        }

                        if (ViewerContainer.EnabledBarcodeClient)
                        {
                            List<string> loosePages = new List<string>();
                            _batchType = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == TargetBatch.BatchData.BatchType.Id)));
                            _barcodeHelper = new BarcodeHelper(_batchType, TargetBatch.BatchData.BatchType.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
                            _barcodeBatch = _barcodeHelper.Process(TargetBatch.BatchData, TargetDocument.DocumentData, TargetDocument.Children.Select(p => p.FilePath).ToList());

                            foreach (FieldValueModel batchField in _barcodeBatch.FieldValues)
                            {
                                if (!batchField.Field.IsSystemField && !string.IsNullOrEmpty(batchField.Value))
                                {
                                    var fieldValue = TargetBatch.BatchData.FieldValues.SingleOrDefault(p => p.Field.Id == batchField.FieldId);
                                    fieldValue.Value = batchField.Value;
                                }
                            }

                            foreach (ContentModel doc in _barcodeBatch.Documents)
                            {
                                foreach (FieldValueModel docField in doc.FieldValues)
                                {
                                    if (!docField.Field.IsSystemField && !string.IsNullOrEmpty(docField.Value))
                                    {
                                        var fieldValue = TargetDocument.DocumentData.FieldValues.SingleOrDefault(p => p.Field.Id == docField.FieldId);
                                        fieldValue.Value = docField.Value;
                                    }
                                }
                            }
                        }

                        InsertField++;
                        break;
                    case EnterContentAction.Replace:
                        ViewerContainer.ContentItemManager.ReplacePage(scanFilePath, TargetPage);
                        break;
                    case EnterContentAction.InsertBefore:
                        ViewerContainer.ContentItemManager.InsertPageBefore(scanFilePath, TargetPage);
                        break;
                    case EnterContentAction.InsertAfter:
                        ViewerContainer.ContentItemManager.InsertPageAfter(scanFilePath, TargetPage);
                        break;
                }

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        public void ScanContent()
        {
            try
            {
                EnterContentAction = EnterContentAction.UnClassify;
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                InsertField = TargetBatch.Children.Count;
                var firstDoc = TargetBatch.Children.FirstOrDefault(p => p.ItemType == ContentItemType.ContentModel);
                if (firstDoc != null)
                {
                    InsertField = TargetBatch.Children.IndexOf(firstDoc);
                }

                if (ViewerContainer.EnabledBarcodeClient)
                {
                    _batchType = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == TargetBatch.BatchData.BatchType.Id)));
                    if (_batchType.BarcodeConfiguration != null && _batchType.BarcodeConfiguration.SeparationActions.Count > 0)
                    {
                        AppHelper.WorkingFolder.Configure(ViewerContainer.UserName);
                        _barcodeBatch = new BatchModel();
                        _barcodeBatch.FieldValues.AddRange((from p in _batchType.Fields select new FieldValueModel { FieldId = p.Id, Field = _batchType.Fields.SingleOrDefault(q => q.Id == p.Id) }));
                        _currentDocument = null;

                        if (_barcodeDatum != null)
                        {
                            _barcodeDatum.Clear();
                        }

                        if (_loosePages != null)
                        {
                            _loosePages.Clear();
                        }

                        _barcodeHelper = new BarcodeHelper(_batchType, TargetBatch.BatchData.BatchType.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
                        TargetDocument = null;
                    }
                    else
                    {
                        _barcodeHelper = null;
                    }
                }

                TwainUtil.Scan(ShowScannerDialog);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        public void SelectDefaultScanner()
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

        public void ShowHideScannerDialog()
        {
            try
            {
                ViewerContainer.IsShowScannerDialogMenu = ShowScannerDialog = !ShowScannerDialog;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanScanToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        }

        public void ScanToDocumentType()
        {
            try
            {
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                TargetDocument = null;
                TargetDocType = (ContentTypeModel)e.Parameter;
                EnterContentAction = EnterContentAction.PutInNewDoc;
                InsertField = 0;
                scannedPage = 0;
                //if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0)
                //{
                //    BarcodeExtractor = new BarcodeExtractor(TargetDocType, ViewerContainer.GetLookupData);
                //    BarcodeExtractorOutputs = new List<DocumentModel>();
                //    FoundBarcodes = new Dictionary<BarcodeTypeModel, int>();
                //}

                TwainUtil.Scan(ShowScannerDialog);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        //private void CanScanContent(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        //}

        //private void ScanContent(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        EnterContentAction = EnterContentAction.UnClassify;
        //        TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
        //                      ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
        //        InsertField = TargetBatch.Children.Count;
        //        var firstDoc = TargetBatch.Children.FirstOrDefault(p => p.ItemType == ContentItemType.ContentModel);
        //        if (firstDoc != null)
        //        {
        //            InsertField = TargetBatch.Children.IndexOf(firstDoc);
        //        }

        //        if (ViewerContainer.EnabledBarcodeClient)
        //        {
        //            _batchType = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == TargetBatch.BatchData.BatchType.Id)));
        //            if (_batchType.BarcodeConfiguration != null && _batchType.BarcodeConfiguration.SeparationActions.Count > 0)
        //            {
        //                AppHelper.WorkingFolder.Configure(ViewerContainer.UserName);
        //                _barcodeBatch = new BatchModel();
        //                _barcodeBatch.FieldValues.AddRange((from p in _batchType.Fields select new FieldValueModel { FieldId = p.Id, Field = _batchType.Fields.SingleOrDefault(q => q.Id == p.Id) }));
        //                _currentDocument = null;

        //                if (_barcodeDatum != null)
        //                {
        //                    _barcodeDatum.Clear();
        //                }

        //                if (_loosePages != null)
        //                {
        //                    _loosePages.Clear();
        //                }

        //                _barcodeHelper = new BarcodeHelper(_batchType, TargetBatch.BatchData.BatchType.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
        //                TargetDocument = null;
        //            }
        //            else
        //            {
        //                _barcodeHelper = null;
        //            }
        //        }

        //        TwainUtil.Scan(ShowScannerDialog);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}

        //private void SelectDefaultScanner(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        TwainUtil.SetDefaultScanner();
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}

        //private void ShowHideScannerDialog(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        ViewerContainer.ShowScannerDialogMenu.IsChecked = ShowScannerDialog = !ShowScannerDialog;
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}

        //private void CanScanToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        //}

        //private void ScanToDocumentType(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
        //                      ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
        //        TargetDocument = null;
        //        TargetDocType = (ContentTypeModel)e.Parameter;
        //        EnterContentAction = EnterContentAction.PutInNewDoc;
        //        InsertField = 0;
        //        scannedPage = 0;
        //        //if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0)
        //        //{
        //        //    BarcodeExtractor = new BarcodeExtractor(TargetDocType, ViewerContainer.GetLookupData);
        //        //    BarcodeExtractorOutputs = new List<DocumentModel>();
        //        //    FoundBarcodes = new Dictionary<BarcodeTypeModel, int>();
        //        //}

        //        TwainUtil.Scan(ShowScannerDialog);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
    }
}
