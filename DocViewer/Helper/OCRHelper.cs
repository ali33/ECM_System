using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Ecm.AppHelper;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;
using Ecm.Ocr;
using ICSharpCode.SharpZipLib.Zip;

namespace Ecm.DocViewer.Helper
{
    public class OCRHelper
    {
        public ViewerContainer ViewerContainer { get; private set; }
        public WorkingFolder WorkingFolder { get; private set; }
        public string TessDataDir { get; private set; }
        
        public OCRHelper(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            WorkingFolder = new WorkingFolder("OCRData");
            TessDataDir = WorkingFolder.CreateDir(_tessDataDirName);
            Stream ocrData = Assembly.GetAssembly(typeof(PanToolHelper)).GetManifestResourceStream("Ecm.DocViewer.OCRData.tessdata.zip");
            var fastZip = new FastZip();
            fastZip.ExtractZip(ocrData, TessDataDir, FastZip.Overwrite.Always, null, null, null, false, false);
        }

        // This method is used for import
        public void DoOCR(ContentItem docItem)
        {
            Dictionary<Guid, Bitmap> croppedBitmaps = GetCroppedBitmaps(docItem);
            if (croppedBitmaps.Count > 0)
            {
                var ambiguousDefinition = ViewerContainer.AmbiguousDefinitions.FirstOrDefault(p => p.Language.Id == docItem.DocumentData.DocumentType.OCRTemplate.Language.Id);
                var ocrWorker = new BackgroundWorker();
                ocrWorker.RunWorkerCompleted += OCRWorkerRunWorkerCompleted;
                ocrWorker.DoWork += OCRWorkerDoWork;
                var parames = new List<object> { croppedBitmaps, docItem, ambiguousDefinition };
                ocrWorker.RunWorkerAsync(parames);
            }
        }

        // This method is used for scan/camera
        public void DoOCROnEachPage(ContentItem docItem, ContentItem pageItem)
        {
            Dictionary<Guid, Bitmap> croppedBitmaps = GetCroppedBitmaps(docItem, pageItem);
            if (croppedBitmaps.Count > 0)
            {
                AmbiguousDefinitionModel ambiguousDefinition = ViewerContainer.AmbiguousDefinitions.FirstOrDefault(p => p.Language.Id == docItem.DocumentData.DocumentType.OCRTemplate.Language.Id);
                var ocrWorker = new BackgroundWorker();
                ocrWorker.RunWorkerCompleted += OCRWorkerRunWorkerCompleted;
                ocrWorker.DoWork += OCRWorkerDoWork;
                var parames = new List<object> { croppedBitmaps, docItem, ambiguousDefinition };
                ocrWorker.RunWorkerAsync(parames);
            }
        }

        private Dictionary<Guid, Bitmap> GetCroppedBitmaps(ContentItem docItem)
        {
            var croppedBitmaps = new Dictionary<Guid, Bitmap>();
            foreach (ContentItem pageItem in docItem.Children)
            {
                var croppedBitmapsInPage = GetCroppedBitmaps(docItem, pageItem);
                foreach (Guid fieldId in croppedBitmapsInPage.Keys)
                {
                    croppedBitmaps.Add(fieldId, croppedBitmapsInPage[fieldId]);
                }
            }

            return croppedBitmaps;
        }

        private Dictionary<Guid, Bitmap> GetCroppedBitmaps(ContentItem docItem, ContentItem pageItem)
        {
            var croppedBitmaps = new Dictionary<Guid, Bitmap>();
            if (docItem.DocumentData.DocumentType.HasOCRTemplateDefined && pageItem.PageData.FileType == FileTypeModel.Image)
            {
                int pageIndex = docItem.Children.IndexOf(pageItem);
                OCRTemplatePageModel ocrTemplatePage = docItem.DocumentData.DocumentType.OCRTemplate.OCRTemplatePages.FirstOrDefault(p => p.PageIndex == pageIndex);
                if (ocrTemplatePage != null && ocrTemplatePage.OCRTemplateZones != null)
                {
                    foreach (var ocrTemplateZone in ocrTemplatePage.OCRTemplateZones)
                    {
                        var croppedArea = new Rectangle(ToPixel(ocrTemplateZone.Left, pageItem.Image.DpiX),
                                                        ToPixel(ocrTemplateZone.Top, pageItem.Image.DpiX),
                                                        ToPixel(ocrTemplateZone.Width, pageItem.Image.DpiX),
                                                        ToPixel(ocrTemplateZone.Height, pageItem.Image.DpiX));
                        var croppedBitmap = GenerateCroppedBitmap(pageItem.FilePath, croppedArea);
                        croppedBitmaps.Add(ocrTemplateZone.FieldMetaData.Id, croppedBitmap);
                        var fieldValue = docItem.DocumentData.FieldValues.FirstOrDefault(p => p.Field.Id == ocrTemplateZone.FieldMetaData.Id);
                        if (fieldValue != null)
                        {
                            var image = new BitmapImage();
                            using (var stream = new MemoryStream())
                            {
                                croppedBitmap.Save(stream, ImageFormat.Bmp);
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                                image.StreamSource = stream;
                                image.EndInit();
                            }

                            fieldValue.SnippetImage = image;
                        }
                    }
                }
            }

            return croppedBitmaps;
        }

        private Bitmap GenerateCroppedBitmap(string filePath, Rectangle croppedArea)
        {
            var bmpImage = (Bitmap)Image.FromFile(filePath);
            try
            {
                Bitmap bmpCrop = bmpImage.Clone(croppedArea, bmpImage.PixelFormat);
                return (bmpCrop);
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException)
                {
                    ViewerContainer.LogException(ex);
                }
                else
                {
                    throw ex;
                }
            }

            return new Bitmap(croppedArea.Width, croppedArea.Height);
        }

        private int ToPixel(double location, double dpi)
        {
            return (int)(location * dpi) / 96;
        }

        private void OCRWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var parames = (List<object>)e.Argument;
                var croppedBitmaps = (Dictionary<Guid, Bitmap>)parames[0];
                var docItem = (ContentItem) parames[1];
                var ambiguousDefinition = (AmbiguousDefinitionModel) parames[2];
                var languageCode = GetLanguageCode(docItem.DocumentData.DocumentType.OCRTemplate.Language);

                OCR<Image> ocrEngine = new OCRImages(TessDataDir, languageCode);
                var ocredData = new Dictionary<Guid, string>();
                foreach (var fieldId in croppedBitmaps.Keys)
                {
                    var deskewUtil = new Deskew(croppedBitmaps[fieldId]);
                    var skewedImage = Deskew.RotateImage(croppedBitmaps[fieldId], deskewUtil.GetSkewAngle());
                    string result = ocrEngine.RecognizeText(new List<Image> { skewedImage }, languageCode, Rectangle.Empty, sender as BackgroundWorker, e);
                    //string result = ocrEngine.RecognizeText(new List<Image> { croppedBitmaps[fieldId] }, languageCode, Rectangle.Empty, sender as BackgroundWorker, e);

                    // Try with another mode
                    if (string.IsNullOrEmpty(result))
                    {
                        ocrEngine.PSM = OCR<Image>.PSM_SINGLE_LINE;
                        result = ocrEngine.RecognizeText(new List<Image> { croppedBitmaps[fieldId] }, languageCode, Rectangle.Empty, sender as BackgroundWorker, e);
                    }

                    if (ambiguousDefinition != null && !string.IsNullOrEmpty(result))
                    {
                        result = Processor.PostProcess(result, languageCode, ambiguousDefinition.Dictionary);
                    }

                    //croppedBitmaps[fieldId].Save(string.Format("C:\\test{0}.bmp", fieldId), ImageFormat.Bmp);
                    string correctResult = CopyDataToField(fieldId, result.Replace(" ",string.Empty), docItem, languageCode);
                    ocredData.Add(fieldId, correctResult);
                }

                e.Result = new List<object> { docItem, ocredData };
            }
            catch (Exception ex)
            {
                ViewerContainer.LogException(ex);
                e.Result = null;
            }
        }

        private void OCRWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                try
                {
                    var result = (List<object>) e.Result;
                    var docItem = (ContentItem)result[0];
                    var ocredData = (Dictionary<Guid, string>)result[1];

                    // Update the value for field that is on the index panel
                    if (ViewerContainer.ThumbnailCommandManager.IndexedItems.Any(p => p == docItem) &&
                        ViewerContainer.FieldValues != null)
                    {
                        foreach (Guid fieldId in ocredData.Keys)
                        {
                            var editedFieldValue = ViewerContainer.FieldValues.FirstOrDefault(p => p.Field.Id == fieldId);
                            if (editedFieldValue != null)
                            {
                                editedFieldValue.Value = ocredData[fieldId];
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    ViewerContainer.LogException(ex);
                }
            }
        }

        private string GetLanguageCode(LanguageModel language)
        {
            var lang = language.Name.ToLower();
            switch (lang)
            {
                case "vietnamese":
                    return _languageCodeViet;
                default:
                    return _languageCodeEng;
            }
        }

        private string CopyDataToField(Guid fieldId, string value, ContentItem docItem, string languageCode)
        {
            FieldValueModel fieldValue = docItem.DocumentData.FieldValues.First(p => p.Field.Id == fieldId);
            if (fieldValue != null)
            {
                switch (fieldValue.Field.DataType)
                {
                    case FieldDataType.String:
                    case FieldDataType.Picklist:
                        fieldValue.Value = value;
                        break;
                    case FieldDataType.Decimal:
                        decimal decVal;
                        if (decimal.TryParse(value, out decVal))
                        {
                            fieldValue.Value = decVal.ToString();
                        }

                        break;
                    case FieldDataType.Integer:
                        int intVal;
                        if (int.TryParse(value, out intVal))
                        {
                            fieldValue.Value = intVal.ToString();
                        }
                        break;
                    case FieldDataType.Date:
                        string format = Properties.Resources.ShortDateTimeFormat;
                        if (languageCode == _languageCodeViet)
                        {
                            format = Properties.Resources.ShortDateTimeFormat_Viet;
                        }

                        DateTime dateValue;
                        if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out dateValue))
                        {
                            fieldValue.Value = dateValue.ToString(Properties.Resources.ShortDateTimeFormat);
                        }
                        
                        break;
                }

                return fieldValue.Value;
            }

            return null;
        }

        private const string _tessDataDirName = "TessData";
        private const string _languageCodeViet = "vie";
        private const string _languageCodeEng = "eng";
    }
}
