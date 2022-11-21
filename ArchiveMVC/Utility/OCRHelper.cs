using ArchiveMVC.Models;
//##################################################################
//# Copyright (C) 2008-2013, MIA Solution.  All Rights Reserved.  
//# 
//# History:
//#     Date Time       Updater         Comment 
//#     20/09/2013      ThoDinh         Intitally
//#     21/09/2013      ThoDinh         Update DoOCR method
//##################################################################
using Ecm.AppHelper;
using Ecm.Domain;

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
using Tesseract;

namespace ArchiveMVC.Utility
{
    /// <summary>
    /// OCR Documents
    /// </summary>
    public class OCRHelper
    {
        #region Private constant members
        private const string _tessDataDirName = "TessData";
        private const string _languageCodeViet = "vie";
        private const string _languageCodeEng = "eng";
        private const int TIME_CACHE = 24*3600;
        #endregion

        #region Public members
        public WorkingFolder WorkingFolder { get; private set; }
        public string TessDataDir { get; private set; }
        #endregion

        /// <summary>
        /// OCRHelper Default Constructor
        /// </summary>
        public OCRHelper()
        {
            
            //WorkingFolder = new WorkingFolder("OCRData");
            //WorkingFolder = HttpContext.Request.PhysicalApplicationPath;
            TessDataDir = System.Web.Hosting.HostingEnvironment.MapPath(@"~\bin\tessdata");
            //TessDataDir = Path.GetFullPath(_tessDataDirName);
            //TessDataDir = Path.GetFullPath(@"./tessdata");
        }

        /// <summary>
        /// Perform OCR documents
        /// </summary>
        /// <param name="docItem">The DocumentModel needs OCRing</param>
        public Dictionary<Guid, string> DoOCR(DocumentModel docItem)
        {
            Dictionary<Guid, Bitmap> croppedBitmaps = GetCroppedBitmaps(docItem);
            if (croppedBitmaps.Count > 0)
            {
                return OCRWorkerDoWork(croppedBitmaps, docItem);
                //var ambiguousDefinition = ViewerContainer.AmbiguousDefinitions.FirstOrDefault(p => p.Language.Id == docItem.DocumentData.DocumentType.OCRTemplate.Language.Id);
                //var ocrWorker = new BackgroundWorker();
                //ocrWorker.RunWorkerCompleted += OCRWorkerRunWorkerCompleted;
                //ocrWorker.DoWork += OCRWorkerDoWork;
                //var parames = new List<object> { croppedBitmaps, docItem, ambiguousDefinition };
                //ocrWorker.RunWorkerAsync(parames);
            }
            return null;
        }

        /// <summary>
        /// Cropped bitmaps from images of document.
        /// </summary>
        /// <param name="docItem">DocumentModel containing images to cropped</param>
        /// <returns>Dictionary object mapping FieldMetaDataId of OCRTemplateZone with Cropped bitmaps</returns>
        private Dictionary<Guid, Bitmap> GetCroppedBitmaps(DocumentModel docItem)
        {
            try{
            var croppedBitmaps = new Dictionary<Guid, Bitmap>();
            //Check Document has defined OCRTemplate
            if (docItem.DocumentType.HasOCRTemplateDefined)
            {
                //var croppedBitmaps = new Dictionary<Guid, Bitmap>();
                //Check Document has defined OCRTemplate
                if (docItem.DocumentType.HasOCRTemplateDefined)
                {
                    foreach (PageModel pageItem in docItem.Pages)
                    {
                        //Check file type of page is image
                        if (pageItem.FileType == FileTypeModel.Image)
                        {
                            int pageIndex = docItem.Pages.IndexOf(pageItem);
                            //Get OCRTemplatePage Enumeration of DocumentType from docItem
                            //In each Document has contained DocumentType, it defines an Enumeration of OCRTemplatePage
                            OCRTemplatePageModel ocrTemplatePage =
                                docItem.DocumentType.OCRTemplate.OCRTemplatePages.FirstOrDefault(p => p.PageIndex == pageIndex);
                            //Initials Image from byte array
                            Image image = ProcessImages.ByteArrayToImage(pageItem.FileBinaries);
                            if (image != null && ocrTemplatePage != null && ocrTemplatePage.OCRTemplateZones != null)
                            {
                                //For each OCRTemplatePage, it defines an Enumeration of OCRTemplateZone
                                foreach (var ocrTemplateZone in ocrTemplatePage.OCRTemplateZones)
                                {

                                    //Create a cropped area (Retangle) from Left, Top, Width, Height of OCRTemplateZone
                                    var croppedArea = new Rectangle(ProcessImages.ToPixel(ocrTemplateZone.Left, image.HorizontalResolution),
                                                                    ProcessImages.ToPixel(ocrTemplateZone.Top, image.HorizontalResolution),
                                                                    ProcessImages.ToPixel(ocrTemplateZone.Width, image.HorizontalResolution),
                                                                    ProcessImages.ToPixel(ocrTemplateZone.Height, image.HorizontalResolution));
                                    var croppedBitmap = ProcessImages.GenerateCroppedBitmap(image, croppedArea);
                                    croppedBitmaps.Add(ocrTemplateZone.FieldMetaData.Id, croppedBitmap);
                                    var fieldValue = docItem.FieldValues.FirstOrDefault(
                                        p => p.Field.Id == ocrTemplateZone.FieldMetaData.Id);
                                    if (fieldValue != null)
                                        fieldValue.SnippetImage = ProcessImages.ImageToByteArray(croppedBitmap,
                                                                        System.Drawing.Imaging.ImageFormat.Jpeg);
                                }
                            }
                        }
                    }
                }
                return croppedBitmaps;
            }
            }catch(Exception e){
                throw e;
            }
            return new Dictionary<Guid, Bitmap>();
        }


        private Dictionary<Guid, string> OCRWorkerDoWork(Dictionary<Guid, Bitmap> croppedBitmaps, DocumentModel docItem)
        {
            var ocredData = new Dictionary<Guid, string>();
            try
            {
                var languageCode = GetLanguageCode(docItem.DocumentType.OCRTemplate.Language);

                foreach (var fieldId in croppedBitmaps.Keys)
                {
                    using (var ocrEngine = new TesseractEngine(TessDataDir, languageCode, EngineMode.Default))
                    {
                        //OCR<Image> ocrEngine = new OCRImages(TessDataDir, languageCode);
                        string result = "";
                        try
                        {
                            result = ocrEngine.Process(croppedBitmaps[fieldId]).GetText();
                        }
                        catch
                        {
                            return new Dictionary<Guid, string>();
                        }
                        if (string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                result = ocrEngine.Process(croppedBitmaps[fieldId], PageSegMode.SingleLine).GetText();
                            }
                            catch
                            {
                                continue;
                            }
                            //result = ocrEngine.RecognizeText(new List<Image> { croppedBitmaps[fieldId] }, languageCode, Rectangle.Empty, sender as BackgroundWorker, e);
                        }
                        //Initial a croppedBitmap cache file object
                        //CacheImage cacheObject = new CacheImage
                        //{
                        //    FileBinaries = ProcessImages.ImageToByteArray(croppedBitmaps[fieldId]),
                        //    ContentType = ContentTypeEnumeration.Image.IMAGE_TYPE
                        //};

                        ////Encrypt key cache
                        //var keyCache = fieldId.ToString();

                        ////Caching cropped image
                        //HttpContext.Current.Cache.Add(
                        //    keyCache, cacheObject, null,
                        //    DateTime.Now.AddMinutes(TIME_CACHE),
                        //    Cache.NoSlidingExpiration,
                        //    CacheItemPriority.Default, null
                        //);

                        //croppedBitmaps[fieldId].Save(string.Format(MapPath("~/Temp/{0}.bmp", fieldId));
                        string correctResult = CopyDataToField(fieldId, result, docItem, languageCode);
                        ocredData.Add(fieldId, correctResult);
                    }
                }
                //e.Result = new List<object> { docItem, ocredData };
            }
            catch
            {
                return new Dictionary<Guid, string>();
                //ViewerContainer.LogException(ex);
                //e.Result = null;
            }
            return ocredData;
        }

        /// <summary>
        /// Get language code
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Coppy data to field value of documents
        /// </summary>
        /// <param name="fieldId">Field value id</param>
        /// <param name="value">Value after OCR</param>
        /// <param name="docItem">The document to fill filed value</param>
        /// <param name="languageCode">Language code</param>
        /// <returns></returns>
        private string CopyDataToField(Guid fieldId, string value, DocumentModel docItem, string languageCode)
        {
            FieldValueModel fieldValue = docItem.FieldValues.First(p => p.Field.Id == fieldId);
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
                        string format = Resources.OCR.ShortDateTimeFormat;
                        if (languageCode == _languageCodeViet)
                        {
                            format = Resources.OCR.ShortDateTimeFormat_Viet;
                        }

                        DateTime dateValue;
                        if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out dateValue))
                        {
                            fieldValue.Value = dateValue.ToString(Resources.OCR.ShortDateTimeFormat);
                        }
                        
                        break;
                }

                return fieldValue.Value;
            }

            return null;
        }

        public Dictionary<string,string> DoOCRFieldId(int pageIndex, DocumentModel docItem)
        {
            Dictionary<string, string> ocredResult = new Dictionary<string, string>();
            Image image = ProcessImages.ByteArrayToImage(docItem.Pages[pageIndex].FileBinaries);
            OCRTemplatePageModel ocrTemplatePage =
                            docItem.DocumentType.OCRTemplate.OCRTemplatePages.FirstOrDefault(p => p.PageIndex == pageIndex);
            //Dictionary<long, Image> croppedBitmaps = new Dictionary<long,Image>();
            if (ocrTemplatePage != null && ocrTemplatePage.OCRTemplateZones != null)
            {
                //For each OCRTemplatePage, it defines an Enumeration of OCRTemplateZone
                foreach (var ocrTemplateZone in ocrTemplatePage.OCRTemplateZones)
                {
                    //Create a cropped area (Retangle) from Left, Top, Width, Height of OCRTemplateZone
                    var croppedArea = new Rectangle(ProcessImages.ToPixel(ocrTemplateZone.Left, image.HorizontalResolution),
                                                    ProcessImages.ToPixel(ocrTemplateZone.Top, image.HorizontalResolution),
                                                    ProcessImages.ToPixel(ocrTemplateZone.Width, image.HorizontalResolution),
                                                    ProcessImages.ToPixel(ocrTemplateZone.Height, image.HorizontalResolution));
                    var croppedBitmap = ProcessImages.GenerateCroppedBitmap(image, croppedArea);
                    var languageCode = GetLanguageCode(docItem.DocumentType.OCRTemplate.Language);
                    using (var ocrEngine = new TesseractEngine(TessDataDir, languageCode, EngineMode.Default))
                    {
                        var ocredData = new Dictionary<long, string>();
                        string value = ocrEngine.Process(croppedBitmap).GetText();
                        ocredResult.Add(ocrTemplateZone.FieldMetaData.Name, value);
                    }
                }
            }
            return ocredResult;
        }
    }
}