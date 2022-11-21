using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ecm.BarcodeProcessing;
using Ecm.CameraLib;
using Ecm.DocViewer.Model;
using Ecm.Model;
using System.IO;

namespace Ecm.DocViewer.Helper
{
    public class CameraManager
    {
        public CameraManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            InitializeData();
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public void Replace(ContentItem target)
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

        public RoutedCommand CaptureToDocumentTypeCommand;

        internal EnterContentAction EnterContentAction { get; private set; }

        internal ContentItem TargetBatch { get; private set; }

        internal ContentItem TargetDocument { get; private set; }

        internal ContentItem TargetPage { get; private set; }

        internal DocumentTypeModel TargetDocType { get; private set; }

        internal List<DocumentModel> BarcodeExtractorOutputs { get; private set; }

        internal Dictionary<BarcodeTypeModel, int> FoundBarcodes { get; private set; }

        internal BarcodeExtractor BarcodeExtractor { get; private set; }

        internal int InsertField { get; private set; }

        private void InitializeData()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            CaptureContentCommand = new RoutedCommand("Capture", typeof(ViewerContainer), gesture);
            var commandBinding = new CommandBinding(CaptureContentCommand, Capture, CanCapture);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) };
            SelectDefaultCameraCommand = new RoutedCommand("ChooseDefaultCamera", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SelectDefaultCameraCommand, SelectDefaultCamera, CanSelectDefaultMediaDevice);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
            SelectDefaultMicCommand = new RoutedCommand("ChooseDefaultMic", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SelectDefaultMicCommand, SelectDefaultMic, CanSelectDefaultMediaDevice);
            ViewerContainer.CommandBindings.Add(commandBinding);

            CaptureToDocumentTypeCommand = new RoutedCommand("CaptureToDocumentType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(CaptureToDocumentTypeCommand, CaptureToDocumentType, CanCaptureToDocumentType);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void CanCapture(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0 && CameraWrapper.HasVideoInputDevice;
        }

        private void Capture(object sender, ExecutedRoutedEventArgs e)
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

        private void CanCaptureToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.Items != null && ViewerContainer.Items.Count > 0 && CameraWrapper.HasVideoInputDevice;
        }

        private void CaptureToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TargetBatch = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 ?
                              ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem : ViewerContainer.Items[0];
                TargetDocument = null;
                TargetDocType = (DocumentTypeModel)e.Parameter;
                EnterContentAction = EnterContentAction.PutInNewDoc;
                InsertField = 0;
                if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0)
                {
                    BarcodeExtractor = new BarcodeExtractor(TargetDocType, ViewerContainer.GetLookupData);
                    BarcodeExtractorOutputs = new List<DocumentModel>();
                    FoundBarcodes = new Dictionary<BarcodeTypeModel, int>();
                }

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

        private void SelectDefaultMic(object sender, ExecutedRoutedEventArgs e)
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

        private void DisplayCameraDialog()
        {
            var cam = new CameraWrapper(ViewerContainer.WorkingFolder.Dir);
            cam.CaptureOutput += Output;
            cam.ShowCamera();
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
                            ViewerContainer.ContentItemManager.InsertPageAfter(e.FilePath, null, ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());

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

                    var mediaExtensions = new[] { ".aiff", ".asf", ".au", ".avi", ".dvr-ms", ".m1v", ".mid", 
                                ".midi", ".mp3", ".mp4", ".mpe", ".mpeg", 
                                ".mpg", ".rmi", ".vob", ".wav", ".wm", ".wma", 
                                ".wmv", ".dat", ".flv",
                                ".m4v", ".mov", ".3gp", ".3g2", ".m2v"};
                    var fileType = new FileInfo(e.FilePath).Extension;

                    switch (EnterContentAction)
                    {
                        case EnterContentAction.UnClassify:
                            ViewerContainer.ContentItemManager.InsertPage(e.FilePath, null, TargetBatch, InsertField);
                            InsertField++;
                            break;
                        case EnterContentAction.PutInNewDoc:
                            if (TargetDocType.BarcodeConfigurations != null && TargetDocType.BarcodeConfigurations.Count > 0 && !mediaExtensions.Contains(fileType))
                            {
                                ScanWithBarcode(e.FilePath);
                                break;
                            }

                            if (TargetDocument == null)
                            {
                                TargetDocument = ViewerContainer.ContentItemManager.CreateDocument(e.FilePath, null, TargetBatch, TargetDocType);
                                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[0]);
                            }
                            else
                            {
                                ViewerContainer.ContentItemManager.InsertPage(e.FilePath, null, TargetDocument, InsertField);
                                ViewerContainer.OCRHelper.DoOCROnEachPage(TargetDocument, TargetDocument.Children[InsertField]);
                            }

                            InsertField++;
                            break;
                        case EnterContentAction.Replace:
                            ViewerContainer.ContentItemManager.ReplacePage(e.FilePath, null, TargetPage);
                            break;
                        case EnterContentAction.InsertBefore:
                            ViewerContainer.ContentItemManager.InsertPageBefore(e.FilePath, null, TargetPage);
                            break;
                        case EnterContentAction.InsertAfter:
                            ViewerContainer.ContentItemManager.InsertPageAfter(e.FilePath, null, TargetPage);
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

        private void ScanWithBarcode(string scanFilePath)
        {
            DocumentModel preDoc = BarcodeExtractorOutputs.Count > 0 ? BarcodeExtractorOutputs.Last() : null;
            DocumentModel postDoc = BarcodeExtractor.ProcessOnePage(scanFilePath, FoundBarcodes, preDoc);
            if (postDoc != preDoc || postDoc.Pages.Count == 1)
            {
                InsertField = 0;
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
    }
}
