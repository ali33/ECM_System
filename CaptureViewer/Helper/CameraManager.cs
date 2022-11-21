using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ecm.CameraLib;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;

namespace Ecm.CaptureViewer.Helper
{
    public class CameraManager
    {
        private BarcodeHelper barcodeHelper;

        public CameraManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            InitializeData();
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public void Replace(ContentItem target, Action unRejectPage)
        {
            EnterContentAction = EnterContentAction.Replace;
            TargetPage = target;
            DisplayCameraDialog();
        }

        public void InsertBefore(ContentItem target)
        {
            EnterContentAction = EnterContentAction.InsertBefore;
            TargetPage = target;
            DisplayCameraDialog();
        }

        public void InsertAfter(ContentItem target)
        {
            EnterContentAction = EnterContentAction.InsertAfter;
            TargetPage = target;
            DisplayCameraDialog();
        }

        public void CaptureOcrTemplate()
        {
            DisplayCameraDialog();
        }

        public bool HasVideoInputDevice
        {
            get { return CameraWrapper.HasVideoInputDevice; }
        }

        public RoutedCommand CaptureContentCommand;

        public RoutedCommand SelectDefaultCameraCommand;

        public RoutedCommand SelectDefaultMicCommand;

        public RoutedCommand CaptureContentToDocumentTypeCommand;

        internal EnterContentAction EnterContentAction { get; private set; }

        internal ContentItem TargetBatch { get; private set; }

        internal ContentItem TargetDocument { get; private set; }

        internal ContentItem TargetPage { get; private set; }

        internal DocTypeModel TargetDocType { get; private set; }

        internal List<DocumentModel> BarcodeExtractorOutputs { get; private set; }

        internal Dictionary<BarcodeTypeModel, int> FoundBarcodes { get; private set; }

        internal int InsertField { get; private set; }

        private void InitializeData()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            CaptureContentCommand = new RoutedCommand("CaptureContent", typeof(ViewerContainer), gesture);
            var commandBinding = new CommandBinding(CaptureContentCommand, CaptureContent, CanCaptureContent);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) };
            SelectDefaultCameraCommand = new RoutedCommand("SelectDefaultCamera", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SelectDefaultCameraCommand, SelectDefaultCamera, CanSelectDefaultMediaDevice);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
            SelectDefaultMicCommand = new RoutedCommand("SelectDefaultMic", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SelectDefaultMicCommand, SelectDefaultMicroPhone, CanSelectDefaultMediaDevice);
            ViewerContainer.CommandBindings.Add(commandBinding);

            CaptureContentToDocumentTypeCommand = new RoutedCommand("CaptureContentToDocumentType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(CaptureContentToDocumentTypeCommand, CaptureContentToDocumentType, CanCaptureContentToDocumentType);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void CanCaptureContent(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0 && CameraWrapper.HasVideoInputDevice;
        }

        private void CaptureContent(object sender, ExecutedRoutedEventArgs e)
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

                DisplayCameraDialog();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanCaptureContentToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0 && CameraWrapper.HasVideoInputDevice;
        }

        private void CaptureContentToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                TargetDocument = null;
                TargetDocType = (DocTypeModel)e.Parameter;
                EnterContentAction = EnterContentAction.PutInNewDoc;
                InsertField = 0;
                //if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0)
                //{
                //    BarcodeExtractor = new BarcodeExtractor(TargetDocType, ViewerContainer.GetLookupData);
                //    BarcodeExtractorOutputs = new List<DocumentModel>();
                //    FoundBarcodes = new Dictionary<BarcodeTypeModel, int>();
                //}

                DisplayCameraDialog();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanSelectDefaultMediaDevice(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CameraWrapper.HasVideoInputDevice;
        }

        private void SelectDefaultCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                CameraWrapper.SelectCameraDevice();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void SelectDefaultMicroPhone(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                CameraWrapper.SelectMicDevice();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void DisplayCameraDialog(Action unRejectPage = null)
        {
            var cam = new CameraWrapper(ViewerContainer.WorkingFolder.Dir);
            cam.CaptureOutput += Output;
            cam.ShowCamera(unRejectPage);
        }

        private void Output(object obj, CaptureOutputEventArgs e)
        {
            try
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    if (e.FilePath != null)
                    {
                        if (ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Count() > 0)
                        {
                            ViewerContainer.ContentItemManager.InsertPageAfter(e.FilePath, ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());

                        }
                        else
                        {
                            ViewerContainer.ImportOCRTemplate(new string[] { e.FilePath });
                        }
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(e.FilePath))
                {
                    AppHelper.WorkingFolder.Configure(ViewerContainer.UserName);
                    Guid batchTypeId = Guid.Empty;
                    BatchTypeModel batchTypeModel = null;

                    if (TargetBatch != null)
                    {
                        if (TargetBatch.ItemType == ContentItemType.Batch)
                        {
                            batchTypeId = TargetBatch.BatchData.BatchType.Id;
                        }
                        else if (TargetBatch.ItemType == ContentItemType.Document)
                        {
                            batchTypeId = TargetBatch.Parent.BatchData.BatchType.Id;
                        }
                        else
                        {
                            batchTypeId = TargetBatch.Parent.Parent.BatchData.BatchType.Id;
                        }

                        batchTypeModel = Enumerable.First<BatchTypeModel>(ViewerContainer.BatchTypes, (Func<BatchTypeModel, bool>)(p => (p.Id == batchTypeId)));

                        if (batchTypeModel.BarcodeConfiguration != null)
                        {
                            barcodeHelper = new BarcodeHelper(batchTypeModel, batchTypeModel.BarcodeConfiguration, AppHelper.WorkingFolder.CreateTempFolder(), true);
                        }
                    }

                    List<string> loosePages = new List<string>();
                    List<CaptureBarcodeProcessing.BarcodeData> barcodeDatum = new List<CaptureBarcodeProcessing.BarcodeData>();
                    DocumentModel currentDocument = new DocumentModel();

                    switch (EnterContentAction)
                    {
                        case EnterContentAction.UnClassify:
                            ViewerContainer.ContentItemManager.InsertPage(e.FilePath, TargetBatch, InsertField);
                            var output = barcodeHelper.Process(TargetBatch.BatchData, currentDocument, new List<string>{e.FilePath});

                            foreach (FieldValueModel fieldValue in barcodeHelper.BatchData.FieldValues)
                            {
                                if (!fieldValue.Field.IsSystemField && !string.IsNullOrEmpty(fieldValue.Value))
                                {
                                    var batchFieldValue = TargetBatch.BatchData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                    batchFieldValue.Value = fieldValue.Value;
                                    if (batchFieldValue.Field.DataType == CaptureDomain.FieldDataType.String)
                                    {
                                        var tempValue = string.Format("{0}", batchFieldValue.Value).Trim();
                                        if (tempValue.Length > batchFieldValue.Field.MaxLength)
                                        {
                                            batchFieldValue.Value = tempValue.Substring(0, batchFieldValue.Field.MaxLength);
                                        }
                                    }
                                }
                            }

                            InsertField++;
                            break;
                        case EnterContentAction.PutInNewDoc:
                            if (TargetDocument == null)
                            {
                                TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(e.FilePath, TargetBatch, TargetDocType);
                            }
                            else
                            {
                                ViewerContainer.ContentItemManager.InsertPage(e.FilePath, TargetDocument, InsertField);
                            }

                            if (ViewerContainer.EnabledBarcodeClient && barcodeHelper != null)
                            {
                                var outDocument = barcodeHelper.Process(TargetBatch.BatchData, loosePages, currentDocument, e.FilePath, barcodeDatum);

                                foreach (FieldValueModel fieldValue in outDocument.FieldValues)
                                {
                                    if (!fieldValue.Field.IsSystemField)
                                    {
                                        var docItemFieldValue = TargetDocument.DocumentData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                        docItemFieldValue.Value = fieldValue.Value;
                                        if (docItemFieldValue.Field.DataType == CaptureDomain.FieldDataType.String)
                                        {
                                            var tempValue = string.Format("{0}", docItemFieldValue.Value).Trim();
                                            if (tempValue.Length > docItemFieldValue.Field.MaxLength)
                                            {
                                                docItemFieldValue.Value = tempValue.Substring(0, docItemFieldValue.Field.MaxLength);
                                            }
                                        }
                                    }
                                }

                                foreach (FieldValueModel fieldValue in barcodeHelper.BatchData.FieldValues)
                                {
                                    if (!fieldValue.Field.IsSystemField && !string.IsNullOrEmpty(fieldValue.Value))
                                    {
                                        var batchFieldValue = TargetBatch.BatchData.FieldValues.First(p => p.Field.Id == fieldValue.Field.Id);
                                        batchFieldValue.Value = fieldValue.Value;

                                        if (batchFieldValue.Field.DataType == CaptureDomain.FieldDataType.String)
                                        {
                                            var tempValue = string.Format("{0}", batchFieldValue.Value).Trim();
                                            if (tempValue.Length > batchFieldValue.Field.MaxLength)
                                            {
                                                batchFieldValue.Value = tempValue.Substring(0, batchFieldValue.Field.MaxLength);
                                            }
                                        }
                                    }
                                }
                            }


                            if (ViewerContainer.EnabledOcrClient)
                            {
                                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                            }

                            InsertField++;
                            break;
                        case EnterContentAction.Replace:
                            if (e.UnRejectPage != null)
                            {
                                e.UnRejectPage();
                            }
                            ViewerContainer.ContentItemManager.ReplacePage(e.FilePath, TargetPage);
                            break;
                        case EnterContentAction.InsertBefore:
                            ViewerContainer.ContentItemManager.InsertPageBefore(e.FilePath, TargetPage);
                            break;
                        case EnterContentAction.InsertAfter:
                            ViewerContainer.ContentItemManager.InsertPageAfter(e.FilePath, TargetPage);
                            break;
                    }

                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch(Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

    }
}
