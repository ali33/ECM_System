using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Ecm.CaptureModel;
using Ecm.Utility;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureBarcodeProcessing;

namespace Ecm.CaptureViewer.Helper
{
    public class ScanningManager
    {
        private int scannedPage = 0;
        private BatchModel _barcodeBatch;
        private BarcodeHelper _barcodeHelper;
        private List<BarcodeData> _barcodeDatum;
        private List<string> _loosePages;
        private List<string> _pageScans;
        private DocumentModel _currentDocument;
        private BatchTypeModel _batchType;

        public RoutedCommand ScanCommand;

        public RoutedCommand SelectDefaultScannerCommand;

        public RoutedCommand ShowHideScannerDialogCommand;

        public RoutedCommand ScanToDocumentTypeCommand;

        public ViewerContainer ViewerContainer { get; private set; }

        public int NumberOfPagesToCreateNewDocumentInNormalScanning { private get; set; }

        internal TwainUtil TwainUtil { get; private set; }

        internal EnterContentAction EnterContentAction { get; private set; }

        internal ContentItem TargetBatch { get; private set; }

        internal ContentItem TargetDocument { get; private set; }

        internal ContentItem TargetPage { get; private set; }

        internal DocTypeModel TargetDocType { get; private set; }

        internal int InsertField { get; private set; }

        internal bool ShowScannerDialog { get; private set; }

        internal List<DocumentModel> BarcodeExtractorOutputs { get; private set; }

        internal Dictionary<BarcodeTypeModel, int> FoundBarcodes { get; private set; }

        public ScanningManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            _barcodeDatum = new List<BarcodeData>();
            _loosePages = new List<string>();
            InitializeData();
        }

        public void ReplaceContent(ContentItem targetPage, Action unRejectPage)
        {
            EnterContentAction = EnterContentAction.Replace;
            TargetPage = targetPage;
            _pageScans = new List<string>();
            TwainUtil.Scan(ShowScannerDialog);

            if (_pageScans != null && _pageScans.Count > 0 && unRejectPage != null)
            {
                unRejectPage();
            }

            ViewerContainer.ContentItemManager.ReplacePages(_pageScans, TargetPage);
            _pageScans = null;
        }

        public void InsertContentBefore(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.InsertBefore;
            TargetPage = targetPage;
            _pageScans = new List<string>();
            TwainUtil.Scan(ShowScannerDialog);

            ViewerContainer.ContentItemManager.InsertPagesBefore(_pageScans, TargetPage);
            _pageScans = null;
        }

        public void InsertContentAfter(ContentItem targetPage)
        {
            EnterContentAction = EnterContentAction.InsertAfter;
            TargetPage = targetPage;
            _pageScans = new List<string>();
            TwainUtil.Scan(ShowScannerDialog);

            ViewerContainer.ContentItemManager.InsertPagesAfter(_pageScans, TargetPage);
            _pageScans = null;
        }

        public string[] ScanOcrTemplate()
        {
            _pageScans = new List<string>();
            TwainUtil.Scan(ShowScannerDialog);
            string[] scanPages = _pageScans.ToArray();
            _pageScans = null;

            return scanPages;
        }

        private void InitializeData()
        {
            TwainUtil = new TwainUtil();
            var handle = GetForegroundWindow();
            TwainUtil.Inititlize(handle.ToInt32(), ViewerContainer.WorkingFolder.Dir, ViewerContainer.AppName, PageScaned);

            var gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            ScanCommand = new RoutedCommand("Scan", typeof(ViewerContainer), gesture);
            var commandBinding = new CommandBinding(ScanCommand, ScanContent, CanScanContent);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Control) };
            SelectDefaultScannerCommand = new RoutedCommand("SelectDefaultScanner", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SelectDefaultScannerCommand, SelectDefaultScanner);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control) };
            ShowHideScannerDialogCommand = new RoutedCommand("ShowHideScannerDialog", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ShowHideScannerDialogCommand, ShowHideScannerDialog);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ScanToDocumentTypeCommand = new RoutedCommand("ImportToDocumentType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ScanToDocumentTypeCommand, ScanToDocumentType, CanScanToDocumentType);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void PageScaned(string scanFilePath)
        {
            try
            {
                _pageScans = _pageScans ?? new List<string>();

                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    _pageScans.Add(scanFilePath);
                    return;
                }

                var container = TargetBatch;

                if (TargetPage != null)
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

                        ////List<string> loosePages = null;
                        //DocumentModel preDoc = BarcodeExtractorOutputs.Count > 0 ? BarcodeExtractorOutputs.Last() : null;
                        //DocumentModel outDocument = _barcodeHelper.Process(_barcodeBatch, _loosePages, preDoc, scanFilePath, _barcodeDatum);

                        //if (outDocument == null && _loosePages != null && _loosePages.Count > 0)
                        //{
                        //    ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetBatch, InsertField);
                        //    _loosePages.Clear();
                        //    break;
                        //}

                        //if (outDocument != null && (_loosePages == null || _loosePages.Count == 0))
                        //{
                        //    //DocTypeModel documentTpye = TargetBatch.BatchData.BatchType.DocTypes.FirstOrDefault(p => p.Id == outDocument.DocTypeId);

                        //    if (preDoc != outDocument)
                        //    {
                        //        if (!BarcodeExtractorOutputs.Contains(outDocument))
                        //        {
                        //            BarcodeExtractorOutputs.Add(outDocument);
                        //        }

                        //        if (outDocument.Pages.Count == 1)
                        //        {
                        //            TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, outDocument.DocumentType);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (TargetDocument == null)
                        //        {
                        //            TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, outDocument.DocumentType);
                        //        }
                        //        else
                        //        {
                        //            ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, InsertField);
                        //        }
                        //    }

                        //}

                        ////if (_loosePages.Count <= 0)
                        ////{
                        ////    break;
                        ////}

                        ////if (outDocument != null)
                        ////{
                        ////    if (outDocument.Pages.Count > 0)
                        ////    {
                        ////        TargetDocument = new ContentItem(outDocument);
                        ////        container.Children.Add(TargetDocument);
                        ////        ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, 0);
                        ////    }
                        ////}
                        //if (TargetDocument != null)
                        //{
                        //    foreach (FieldValueModel fieldValue in outDocument.FieldValues)
                        //    {
                        //        FieldValueModel documentField = TargetDocument.DocumentData.FieldValues.FirstOrDefault(p => p.Field.Id == fieldValue.FieldId);
                        //        if (!((documentField == null) || string.IsNullOrEmpty(fieldValue.Value)))
                        //        {
                        //            documentField.Value = fieldValue.Value;
                        //        }
                        //    }
                        //}

                        ////if (_currentDocument != null && TargetDocument != null && outDocument.Pages.Count > this._currentDocument.Pages.Count)
                        ////{
                        ////    ViewerContainer.ContentItemManager.InsertPage(scanFilePath, this.TargetDocument, this.TargetDocument.Children.Count);
                        ////}

                        //foreach (FieldValueModel batchField in _barcodeBatch.FieldValues)
                        //{
                        //    FieldValueModel fieldModel = _barcodeBatch.FieldValues.FirstOrDefault(p => p.Field.Id == batchField.FieldId);
                        //    if (((fieldModel.Field != null) && !fieldModel.Field.IsSystemField) && !string.IsNullOrEmpty(fieldModel.Value))
                        //    {
                        //        if (fieldModel != null)
                        //        {
                        //            fieldModel.Value = batchField.Value;
                        //        }
                        //    }
                        //}
                        //_currentDocument = outDocument;

                        ////ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetBatch, InsertField);
                        //InsertField++;
                        //_loosePages.Clear();

                        if (_pageScans != null)
                        {
                            _pageScans.Add(scanFilePath);
                        }

                        break;
                    case EnterContentAction.PutInNewDoc:

                        //if (NumberOfPagesToCreateNewDocumentInNormalScanning > 0)
                        //{
                        //    if (scannedPage < NumberOfPagesToCreateNewDocumentInNormalScanning)
                        //    {
                        //        if (TargetDocument != null)
                        //        {
                        //            ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, InsertField);

                        //            if (ViewerContainer.EnabledOcrClient)
                        //            {
                        //                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                        //            }
                        //        }
                        //        else
                        //        {
                        //            TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);

                        //            if (ViewerContainer.EnabledOcrClient)
                        //            {
                        //                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        scannedPage = 0;
                        //        InsertField = 0;
                        //        TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);

                        //        if (ViewerContainer.EnabledOcrClient)
                        //        {
                        //            ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                        //        }
                        //    }

                        //    //InsertIndex++;
                        //    scannedPage++;
                        //}
                        //else
                        //{
                        //    if (TargetDocument == null)
                        //    {
                        //        TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);

                        //        if (ViewerContainer.EnabledOcrClient)
                        //        {
                        //            ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, InsertField);

                        //        if (ViewerContainer.EnabledOcrClient)
                        //        {
                        //            ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                        //        }
                        //    }
                        //}

                        if (ViewerContainer.EnabledBarcodeClient && TargetBatch.BatchData.BatchType.BarcodeConfiguration != null)
                        {
                            //List<string> loosePages = new List<string>();
                            //_batchType = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == TargetBatch.BatchData.BatchType.Id)));
                            //_barcodeHelper = new BarcodeHelper(_batchType, TargetBatch.BatchData.BatchType.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
                            //_barcodeBatch = _barcodeHelper.Process(TargetBatch.BatchData, TargetDocument.DocumentData, TargetDocument.Children.Select(p => p.FilePath).ToList());

                            //foreach (FieldValueModel batchField in _barcodeBatch.FieldValues)
                            //{
                            //    if (!batchField.Field.IsSystemField && !string.IsNullOrEmpty(batchField.Value))
                            //    {
                            //        var fieldValue = TargetBatch.BatchData.FieldValues.SingleOrDefault(p => p.Field.Id == batchField.FieldId);
                            //        fieldValue.Value = batchField.Value;
                            //    }
                            //}

                            //foreach (DocumentModel doc in _barcodeBatch.Documents)
                            //{
                            //    foreach (FieldValueModel docField in doc.FieldValues)
                            //    {
                            //        if (!docField.Field.IsSystemField && !string.IsNullOrEmpty(docField.Value))
                            //        {
                            //            var fieldValue = TargetDocument.DocumentData.FieldValues.SingleOrDefault(p => p.Field.Id == docField.FieldId);
                            //            fieldValue.Value = docField.Value;
                            //        }
                            //    }
                            //}
                            //ScanWithBarcode(scanFilePath);
                            _pageScans.Add(scanFilePath);
                            break;
                        }

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

                        InsertField++;
                        break;
                    case EnterContentAction.Replace:
                        _pageScans.Add(scanFilePath);
                        //ViewerContainer.ContentItemManager.ReplacePage(scanFilePath, TargetPage);
                        break;
                    case EnterContentAction.InsertBefore:
                        _pageScans.Add(scanFilePath);
                        //ViewerContainer.ContentItemManager.InsertPageBefore(scanFilePath, TargetPage);
                        break;
                    case EnterContentAction.InsertAfter:
                        _pageScans.Add(scanFilePath);
                        //ViewerContainer.ContentItemManager.InsertPageAfter(scanFilePath, TargetPage);
                        break;
                }

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanScanContent(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0;
        }

        private void ScanContent(object sender, ExecutedRoutedEventArgs e)
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

                _pageScans = _pageScans ?? new List<string>();

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

                        BarcodeExtractorOutputs = new List<DocumentModel>();
                        _barcodeHelper = new BarcodeHelper(_batchType, TargetBatch.BatchData.BatchType.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);

                        TargetDocument = null;
                    }
                    else
                    {
                        _barcodeHelper = null;
                    }
                }

                TwainUtil.Scan(ShowScannerDialog);

                if (_pageScans != null && _pageScans.Count > 0)
                {
                    ProcessBarcodeLoosePage();
                }

                _pageScans = null;
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
                ViewerContainer.ShowScannerDialogMenu.IsChecked = ShowScannerDialog = !ShowScannerDialog;
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

        private void ScanToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                TargetDocument = null;
                TargetDocType = (DocTypeModel)e.Parameter;
                EnterContentAction = EnterContentAction.PutInNewDoc;
                InsertField = 0;
                scannedPage = 0;

                _pageScans = _pageScans ?? new List<string>();

                TwainUtil.Scan(ShowScannerDialog);

                if (ViewerContainer.EnabledBarcodeClient && TargetBatch.BatchData.BatchType.BarcodeConfiguration != null)
                {
                    _batchType = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == TargetBatch.BatchData.BatchType.Id)));
                    _barcodeHelper = new BarcodeHelper(_batchType, TargetBatch.BatchData.BatchType.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
                    BarcodeExtractorOutputs = new List<DocumentModel>();

                    //FoundBarcodes = new Dictionary<BarcodeTypeModel, int>();
                    if (_pageScans != null && _pageScans.Count > 0)
                    {
                        ProcessBarcode(TargetDocType, TargetBatch.BatchData.BatchType.DocTypes.ToList());
                    }
                }

                _pageScans = null;

            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ProcessBarcode(DocTypeModel documentType, List<DocTypeModel> originDoctypes)
        {
            var outBatchModel = _barcodeHelper.Process(TargetBatch.BatchData, _pageScans, documentType);

            foreach (DocumentModel document in outBatchModel.Documents)
            {
                DocTypeModel tempDocTypeModel = originDoctypes.FirstOrDefault(h => h.Id == document.DocTypeId);

                var importDocItem = ViewerContainer.ContentItemManager.CreateDocument(document.Pages.Select(p => p.FilePath).ToList(), TargetBatch, tempDocTypeModel);

                foreach (FieldValueModel fieldValue in document.FieldValues)
                {
                    if (!fieldValue.Field.IsSystemField)
                    {
                        var docItemFieldValue = importDocItem.DocumentData.FieldValues.FirstOrDefault(p => p.Field.Id == fieldValue.Field.Id);

                        if (docItemFieldValue != null)
                        {
                            docItemFieldValue.Value = fieldValue.Value;
                        }
                    }
                }

                if (ViewerContainer.EnabledOcrClient)
                {
                    ViewerContainer.OCRHelper.DoOCR(importDocItem);
                }
            }
            foreach (FieldValueModel fieldValue in outBatchModel.FieldValues)
            {
                if (!fieldValue.Field.IsSystemField)
                {
                    var batchFieldValue = TargetBatch.BatchData.FieldValues.FirstOrDefault(p => p.Field.Id == fieldValue.Field.Id);

                    if (batchFieldValue != null)
                    {
                        batchFieldValue.Value = fieldValue.Value;
                    }
                }
            }
        }

        private void ProcessBarcodeLoosePage()
        {
            List<string> loosePages = new List<string>(_pageScans);
            BatchModel batchModel = new BatchModel();

            batchModel = _barcodeHelper.Process(_pageScans, out loosePages);

            foreach (DocumentModel document in batchModel.Documents)
            {
                var pageFiles = document.Pages.Select(p => p.FilePath).ToList();
                //var docType = batchTypeModel.DocTypes.SingleOrDefault(p => p.Id == document.DocTypeId);

                if (pageFiles.Count > 0)
                {
                    var docItem = ViewerContainer.ContentItemManager.CreateDocument(pageFiles, TargetBatch, document.DocumentType);

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
                int insertIndex = TargetBatch.Children.Count;
                ContentItem firstDoc = Enumerable.FirstOrDefault<ContentItem>(TargetBatch.Children, (Func<ContentItem, bool>)(p => (p.ItemType == ContentItemType.Document)));
                if (firstDoc != null)
                {
                    insertIndex = TargetBatch.Children.IndexOf(firstDoc);
                }
                this.ViewerContainer.ContentItemManager.InsertPages(loosePages, TargetBatch, insertIndex);
            }

        }

        private void ScanWithBarcode(string scanFilePath)
        {
            DocumentModel preDoc = BarcodeExtractorOutputs.Count > 0 ? BarcodeExtractorOutputs.Last() : null;
            DocumentModel postDoc = _barcodeHelper.Process(TargetBatch.BatchData, preDoc, scanFilePath, TargetDocType);
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
                    TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(scanFilePath, TargetBatch, TargetDocType);
                }
            }
            else
            {
                ViewerContainer.ContentItemManager.InsertPage(scanFilePath, TargetDocument, InsertField);
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

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
    }
}
