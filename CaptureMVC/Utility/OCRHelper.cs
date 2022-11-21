using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Net.Mime;
using Ecm.CaptureDomain;
using CaptureMVC.Models;
using CaptureMVC.Utility;
using Ecm.CaptureModel;
using Tesseract;
using System.Text;
using Ecm.Ocr;

namespace CaptureMVC.Utility
{
    public class OCRHelper
    {
        private const string _languageCodeViet = "vie";
        private const string _languageCodeEng = "eng";

        private readonly string[] vieDateFormats = new string[] { "dd/MM/yyyy", "d/M/yyyy" };
        private readonly string[] engDateFormats = new string[] { "MM/dd/yyyy", "M/d/yyyy" };


        public string _tessDataDir;

        public OCRHelper(string ocrFolder)
        {
            this._tessDataDir = System.Web.Hosting.HostingEnvironment.MapPath(@"~\" + ocrFolder);
        }

        public Dictionary<Guid, Bitmap> GetCroppedBitmaps(DocumentType docType, List<ViewPageModel> pageModels)
        {
            var croppedBitmaps = new Dictionary<Guid, Bitmap>();

            if (docType != null
                && docType.OCRTemplate != null
                && docType.OCRTemplate.OCRTemplatePages != null
                && docType.OCRTemplate.OCRTemplatePages.Count > 0
                && pageModels != null
                && pageModels.Count > 0)
            {
                var countPageModels = pageModels.Count;

                // Loop all OCR template pages
                for (int i = 0; i < docType.OCRTemplate.OCRTemplatePages.Count; i++)
                {
                    // Template page index is out of range page models
                    if (i >= countPageModels)
                    {
                        break;
                    }

                    // Get template page
                    var ocrTemplatePage = docType.OCRTemplate.OCRTemplatePages[i];
                    if (ocrTemplatePage.OCRTemplateZones == null || ocrTemplatePage.OCRTemplateZones.Count == 0)
                    {
                        continue;
                    }

                    // Get page model corresponding to template page
                    var pageModel = pageModels[i];
                    if (!pageModel.ContentType.StartsWith(ContentTypeEnumeration.Image.IMAGE_TYPE))
                    {
                        continue;
                    }

                    // Loop all OCR template zones in template page
                    foreach (var ocrTemplateZone in ocrTemplatePage.OCRTemplateZones)
                    {
                        var croppedArea = new Rectangle(ToPixel(ocrTemplateZone.Left, pageModel.Dpi),
                                                        ToPixel(ocrTemplateZone.Top, pageModel.Dpi),
                                                        ToPixel(ocrTemplateZone.Width, pageModel.Dpi),
                                                        ToPixel(ocrTemplateZone.Height, pageModel.Dpi));

                        // Get cropped bitmap
                        var croppedBitmap = GenerateCroppedBitmap(pageModel.OriginPage.FileBinary, croppedArea);
                        if (croppedBitmap == null)
                        {
                            continue;
                        }
                        // Add to list result
                        croppedBitmaps.Add(ocrTemplateZone.FieldMetaDataId, croppedBitmap);
                    }
                }
            }

            return croppedBitmaps;
        }

        public Dictionary<Guid, string> DoOcr(DocumentType docType,
                                              Dictionary<Guid, Bitmap> croppedBitmaps,
                                              string languageName,
                                              List<CaptureAmbiguousDefinitionModel> ambiguousDefinitions)
        {
            var ocredData = new Dictionary<Guid, string>();

            if (croppedBitmaps == null)
            {
                return ocredData;
            }

            var languageCode = GetLanguageCode(languageName);

            using (var ocrEngine = new TesseractEngine(this._tessDataDir, languageCode, EngineMode.Default))
            {
                foreach (var fieldId in croppedBitmaps.Keys)
                {
                    try
                    {
                        var page = ocrEngine.Process(croppedBitmaps[fieldId]);
                        var result = page.GetText();
                        page.Dispose();

                        // Try with another mode
                        if (string.IsNullOrEmpty(result))
                        {
                            page = ocrEngine.Process(croppedBitmaps[fieldId], PageSegMode.SingleLine);
                            result = page.GetText();
                            page.Dispose();
                        }

                        // Correct common spell
                        if (ambiguousDefinitions != null && !string.IsNullOrEmpty(result))
                        {
                            foreach (var item in ambiguousDefinitions)
                            {
                                result = Processor.PostProcess(result, languageCode, item.Dictionary);
                            }
                        }

                        // Correct common type
                        string correctResult = CorrectText(fieldId, result, docType, languageCode);
                        ocredData.Add(fieldId, correctResult);

                    }
                    catch { }
                }
            }

            return ocredData;
        }

        private int ToPixel(double location, double dpi)
        {
            return (int)(location * dpi) / 96;
        }

        public Bitmap GenerateCroppedBitmap(byte[] image, Rectangle croppedArea)
        {
            try
            {
                using (var memoryStream = new MemoryStream(image))
                {
                    var bmpImage = new Bitmap(memoryStream);
                    Bitmap bmpCrop = bmpImage.Clone(croppedArea, bmpImage.PixelFormat);
                    return bmpCrop;
                }
            }
            catch (Exception e) { return null; }
        }

        private string GetLanguageCode(string languageName)
        {
            var lang = string.Format("{0}", languageName).ToLower();
            switch (lang)
            {
                case "vietnamese":
                    return _languageCodeViet;
                default:
                    return _languageCodeEng;
            }
        }

        private string CorrectText(Guid fieldId, string value, DocumentType docType, string languageCode)
        {
            var field = docType.Fields.FirstOrDefault(h => h.Id == fieldId);
            value = string.Format("{0}", value).Trim();

            if (field != null)
            {
                switch (field.DataTypeEnum)
                {
                    case FieldDataType.Decimal:
                        decimal decVal;
                        if (decimal.TryParse(value, out decVal))
                        {
                            value = decVal.ToString();
                        }
                        break;

                    case FieldDataType.Integer:
                        int intVal;
                        if (int.TryParse(value, out intVal))
                        {
                            value = intVal.ToString();
                        }
                        break;

                    case FieldDataType.Date:
                        string[] format = engDateFormats;
                        if (languageCode == _languageCodeViet)
                        {
                            format = vieDateFormats;
                        }

                        DateTime dateValue;
                        if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out dateValue))
                        {
                            value = dateValue.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        }
                        break;
                }

                return value;
            }

            return null;
        }
    }
}