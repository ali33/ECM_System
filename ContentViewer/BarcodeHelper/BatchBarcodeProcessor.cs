using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoftekBarcodeLib3;
using Ecm.ContentViewer.Model;

namespace Ecm.ContentViewer.BarcodeHelper
{
    public class BatchBarcodeProcessor
    {
        private readonly BarcodeReader _barcodeReader;

        public string WorkingFolder { get; private set; }

        public BatchTypeModel BatchType { get; private set; }

        public BarcodeConfigurationModel Configuration { get; private set; }

        public BatchModel Batch { get; private set; }

        public bool IsLogTraceEnabled { get; set; }

        public string LogFolderPath { get; set; }

        public BatchBarcodeProcessor(BatchTypeModel batchType, BarcodeConfigurationModel configuration, string workingFolder, bool x86)
        {
            WorkingFolder = workingFolder;
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            BatchType = batchType;
            Configuration = configuration;
            _barcodeReader = BarcodeInitializers.Initialize(workingFolder, x86);
            ConfigureReader(configuration);
        }

        public BatchModel Process(BatchModel batch)
        {
            WriteLogTrace("BEGIN of Process(Batch batch) method");

            try
            {
                Batch = batch;
                ProcessLoosePages();
                ProcessExistedDocuments();

                return batch;
            }
            catch (Exception ex)
            {
                WriteLogTrace(ex);
                throw;
            }
            finally
            {
                WriteLogTrace("END of Process(Batch batch) method");
            }
        }

        public BatchModel Process(List<string> pageFilePaths, out List<string> loosePages)
        {
            Batch = new BatchModel();
            Batch.FieldValues.AddRange((from p in BatchType.Fields select new FieldValueModel { FieldId = p.Id, Field = BatchType.Fields.SingleOrDefault(q => q.Id == p.Id) }));
            List<BarcodeData> barcodeDatum = new List<BarcodeData>();
            ContentModel currentDocument = null;
            loosePages = new List<string>();
            foreach (string pageFilePath in pageFilePaths)
            {
                currentDocument = Process(Batch, loosePages, currentDocument, pageFilePath, barcodeDatum);
            }

            List<ContentModel> newEmptyDocuments = Batch.Documents.Where(p => p.Pages.Count == 0).ToList();
            foreach (ContentModel newEmptyDocument in newEmptyDocuments)
            {
                Batch.Documents.Remove(newEmptyDocument);
                Batch.DocCount--;
            }

            return Batch;
        }

        public ContentModel Process(BatchModel batch, List<string> loosePages, ContentModel currentDocument, string pageFilePath, List<BarcodeData> barcodeDatum)
        {
            Batch = batch;
            string extension = (Path.GetExtension(pageFilePath) + string.Empty).Replace(".", "");
            if (IsImageFile(extension))
            {
                List<BarcodeData> barcodesOnThisPage = ScanPage(pageFilePath, barcodeDatum);
                if (barcodesOnThisPage.Count > 0)
                {
                    bool hasSeparator;
                    bool removeSeparatorPage;
                    ContentModel outputDocument = RunActions(barcodesOnThisPage, currentDocument,
                                                                new PageModel { FilePath = pageFilePath },
                                                                out hasSeparator, out removeSeparatorPage);
                    if (outputDocument != null && outputDocument != currentDocument)
                    {
                        if (currentDocument != null)
                        {
                            barcodeDatum.Clear();
                        }

                        currentDocument = outputDocument;
                        Batch.Documents.Add(currentDocument);
                        Batch.DocCount++;

                        if (hasSeparator)
                        {
                            if (removeSeparatorPage)
                            {
                                Batch.PageCount--;
                            }
                        }
                    }
                    else if (currentDocument == null)
                    {
                        loosePages.Add(pageFilePath);
                    }
                }
                else if (currentDocument != null)
                {
                    currentDocument.Pages.Add(new PageModel { FilePath = pageFilePath });
                }
                else
                {
                    loosePages.Add(pageFilePath);
                }
            }
            else if (currentDocument != null)
            {
                currentDocument.Pages.Add(new PageModel { FilePath = pageFilePath });
            }
            else
            {
                loosePages.Add(pageFilePath);
            }

            return currentDocument;
        }

        private void ProcessLoosePages()
        {
            WriteLogTrace("BEGIN of ProcessLoosePages() method");

            ContentModel looseDoc = Batch.Documents.FirstOrDefault(p => p.DocTypeId == Guid.Empty);
            if (looseDoc != null)
            {
                List<BarcodeData> barcodeDatum = new List<BarcodeData>();
                ContentModel currentDocument = null;
                List<PageModel> loosePages = new List<PageModel>(looseDoc.Pages);
                foreach (PageModel page in loosePages)
                {
                    if (IsImageFile(page.FileExtension))
                    {
                        string tempFile = Path.Combine(WorkingFolder, Guid.NewGuid() + "." + page.FileExtension.Replace(".", ""));
                        File.WriteAllBytes(tempFile, page.FileBinary);
                        List<BarcodeData> barcodesOnThisPage = ScanPage(tempFile, barcodeDatum);
                        if (barcodesOnThisPage.Count > 0)
                        {
                            bool hasSeparator;
                            bool removeSeparatorPage;
                            ContentModel outputDocument = RunActions(barcodesOnThisPage, currentDocument, page,
                                                                        out hasSeparator, out removeSeparatorPage);
                            if (outputDocument != null && outputDocument != currentDocument)
                            {
                                if (currentDocument != null)
                                {
                                    barcodeDatum.Clear();
                                }

                                currentDocument = outputDocument;
                                Batch.Documents.Add(currentDocument);
                                Batch.DocCount++;

                                if (hasSeparator)
                                {
                                    looseDoc.Pages.Remove(page);
                                    looseDoc.DeletedPages.Add(page.Id);
                                    if (removeSeparatorPage)
                                    {
                                        Batch.PageCount--;
                                    }
                                }
                            }
                            else if (currentDocument != null)
                            {
                                looseDoc.Pages.Remove(page);
                                looseDoc.DeletedPages.Add(page.Id);
                            }
                        }
                        else if (currentDocument != null)
                        {
                            currentDocument.Pages.Add(page);
                            looseDoc.Pages.Remove(page);
                            looseDoc.DeletedPages.Add(page.Id);
                        }

                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch { }
                    }
                    else if (currentDocument != null)
                    {
                        currentDocument.Pages.Add(page);
                        looseDoc.Pages.Remove(page);
                        looseDoc.DeletedPages.Add(page.Id);
                    }
                }

                // Remove new empty document just created by this method
                List<ContentModel> newEmptyDocuments = Batch.Documents.Where(p => p.Id == Guid.Empty && p.Pages.Count == 0).ToList();
                foreach (ContentModel newEmptyDocument in newEmptyDocuments)
                {
                    Batch.Documents.Remove(newEmptyDocument);
                    Batch.DocCount--;
                }

                if (looseDoc.Pages.Count == 0)
                {
                    Batch.Documents.Remove(looseDoc);
                    Batch.DeletedDocuments.Add(looseDoc.Id);
                }
            }

            WriteLogTrace("END of ProcessLoosePages() method");
        }

        private void ProcessExistedDocuments()
        {
            WriteLogTrace("BEGIN of ProcessExistedDocuments() method");

            List<ContentModel> existedDocuments = Batch.Documents.Where(p => p.DocTypeId != Guid.Empty && p.Id != Guid.Empty).ToList();
            foreach (ContentModel document in existedDocuments)
            {
                List<BarcodeData> barcodeDatum = new List<BarcodeData>();
                foreach (PageModel page in document.Pages)
                {
                    if (IsImageFile(page.FileExtension))
                    {
                        string tempFile = Path.Combine(WorkingFolder, Guid.NewGuid() + "." + page.FileExtension.Replace(".", ""));
                        File.WriteAllBytes(tempFile, page.FileBinary);
                        List<BarcodeData> barcodesOnThisPage = ScanPage(tempFile, barcodeDatum);

                        Guid currentDocTypeId = document.DocTypeId;
                        List<ReadActionModel> readActions = Configuration.ReadActions.Where(p => barcodesOnThisPage.Any(q => q.Type == (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), p.BarcodeType) &&
                                                                                                                        (string.IsNullOrEmpty(p.StartsWith) ||
                                                                                                                         q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                                                        (p.BarcodePositionInDoc == 0 ||
                                                                                                                         p.BarcodePositionInDoc == q.Position) &&
                                                                                                                        (!p.IsDocIndex || p.DocTypeId == currentDocTypeId))).ToList();
                        DoReadAction(barcodesOnThisPage, document, readActions);
                    }
                }
            }

            WriteLogTrace("END of ProcessExistedDocuments() method");
        }

        private ContentModel RunActions(IEnumerable<BarcodeData> barcodesOnPage, ContentModel currentDocument, PageModel currentPage, out bool hasSeparator, out bool removeSeparatorPage)
        {
            WriteLogTrace("BEGIN of RunActions(...) method");

            currentDocument = DoSeparationAction(barcodesOnPage, currentDocument, currentPage, out hasSeparator, out removeSeparatorPage);
            Guid currentDocTypeId = (currentDocument == null ? Guid.Empty : currentDocument.DocTypeId);

            if (hasSeparator)
            {
                List<BarcodeTypeModel> barcodeTypes = new List<BarcodeTypeModel>();

                //Reset positions of barcode on this page
                foreach (BarcodeData barcode in barcodesOnPage)
                {
                    barcodeTypes.Add(barcode.Type);
                    barcode.Position = barcodeTypes.Count(p => p == barcode.Type);
                }
            }

            List<ReadActionModel> readActions = Configuration.ReadActions.Where(p => barcodesOnPage.Any(q => q.Type == (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), p.BarcodeType) &&
                                                                                                        (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                                        (p.BarcodePositionInDoc == 0 || p.BarcodePositionInDoc == q.Position) &&
                                                                                                        (p.IsDocIndex || (currentDocument != null && p.DocTypeId == currentDocTypeId)))).ToList();
            DoReadAction(barcodesOnPage, currentDocument, readActions);

            WriteLogTrace("END of RunActions(...) method");
            return currentDocument;
        }

        private ContentModel DoSeparationAction(IEnumerable<BarcodeData> barcodesOnPage, ContentModel currentDocument, PageModel currentPage, out bool hasSeparator,
                                                   out bool removeSeparatorPage)
        {
            WriteLogTrace("BEGIN of DoSeparationAction(...) method");

            List<SeparationActionModel> separationActions = Configuration.SeparationActions.Where(p => barcodesOnPage.Any(q => q.Type == (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), p.BarcodeType) &&
                                                                                                                   (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                                                   (p.BarcodePositionInDoc == 0 || p.BarcodePositionInDoc == q.Position))).ToList();
            hasSeparator = separationActions.Count > 0;
            removeSeparatorPage = separationActions.Any(p => p.RemoveSeparatorPage);
            if (hasSeparator)
            {
                bool breakNewDocument = false;
                if (currentDocument == null)
                {
                    breakNewDocument = true;
                }
                else if (currentDocument.DocTypeId != Guid.Empty || separationActions.Any(p => !p.HasSpecifyDocumentType))
                {
                    breakNewDocument = true;
                }

                if (breakNewDocument)
                {
                    currentDocument = new ContentModel
                    {
                        Id = Batch.Id,
                        CreatedBy = Batch.CreatedBy,
                        CreatedDate = DateTime.Now,
                        PageCount = Batch.Documents.Count,
                        DocTypeId = Guid.Empty,
                        IsUndefinedType = !separationActions.Any(p => p.HasSpecifyDocumentType)
                    };
                }

                if (!removeSeparatorPage)
                {
                    currentDocument.Pages.Add(currentPage);
                }

                if (separationActions.Any(p => p.HasSpecifyDocumentType) || BatchType.DocTypes.Count == 1)
                {
                    ContentTypeModel docType;
                    if (BatchType.DocTypes.Count == 1)
                    {
                        docType = BatchType.DocTypes[0];
                    }
                    else
                    {
                        docType = BatchType.DocTypes.FirstOrDefault(p => p.Id == separationActions.First(q => q.HasSpecifyDocumentType).DocTypeId);
                    }

                    if (docType != null)
                    {
                        currentDocument.DocTypeId = docType.Id;
                        currentDocument.IsUndefinedType = false;
                        foreach (var field in docType.Fields)
                        {
                            currentDocument.FieldValues.Add(new FieldValueModel
                            {
                                FieldId = field.Id,
                                Value = field.DefaultValue,
                                Field = field
                            });
                        }
                    }
                }
            }
            else if (currentDocument != null)
            {
                currentDocument.Pages.Add(currentPage);
            }

            WriteLogTrace("END of DoSeparationAction(...) method");
            return currentDocument;
        }

        private void DoReadAction(IEnumerable<BarcodeData> barcodesOnPage, ContentModel document, IEnumerable<ReadActionModel> readActions)
        {
            WriteLogTrace("BEGIN of DoReadAction(...) method");

            foreach (ReadActionModel readAction in readActions)
            {
                if (Is2DBarcode((BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), readAction.BarcodeType)) && !string.IsNullOrEmpty(readAction.Separator))
                {
                    Read2DBarcode(barcodesOnPage, readAction, document);
                }
                else
                {
                    Read1DBarcode(barcodesOnPage, readAction, document);
                }
            }

            WriteLogTrace("END of DoReadAction(...) method");
        }

        private void Read1DBarcode(IEnumerable<BarcodeData> barcodesOnPage, ReadActionModel readAction, ContentModel document)
        {
            WriteLogTrace("BEGIN of Read1DBarcode(...) method");

            string indexGuid = readAction.CopyValueToFields[0].FieldGuid;
            string value = barcodesOnPage.First(p => p.Type == (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), readAction.BarcodeType) &&
                                                     (readAction.BarcodePositionInDoc == 0 ||
                                                     readAction.BarcodePositionInDoc == p.Position) &&
                                                     (string.IsNullOrEmpty(readAction.StartsWith) ||
                                                     p.Value.StartsWith(readAction.StartsWith, StringComparison.OrdinalIgnoreCase))).Value;
            if (readAction.IsDocIndex)
            {
                if (document != null && !document.IsUndefinedType)
                {
                    FieldValueModel index = document.FieldValues.FirstOrDefault(p => p.Field.UniqueId == indexGuid);
                    if (index != null && (readAction.OverwriteFieldValue))
                    {
                        index.Value = value;
                    }
                }
            }
            else
            {
                FieldValueModel index = Batch.FieldValues.FirstOrDefault(p => p.Field.UniqueId == indexGuid);
                if (index != null && (readAction.OverwriteFieldValue))
                {
                    index.Value = value;
                }
            }

            WriteLogTrace("END of Read1DBarcode(...) method");
        }

        private void Read2DBarcode(IEnumerable<BarcodeData> barcodesOnPage, ReadActionModel readAction, ContentModel document)
        {
            WriteLogTrace("BEGIN of Read2DBarcode(...) method");

            string value = barcodesOnPage.First(p => p.Type == (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), readAction.BarcodeType) &&
                                                     (readAction.BarcodePositionInDoc == 0 ||
                                                     readAction.BarcodePositionInDoc == p.Position) &&
                                                     (string.IsNullOrEmpty(readAction.StartsWith) ||
                                                     p.Value.StartsWith(readAction.StartsWith, StringComparison.OrdinalIgnoreCase))).Value;
            string[] valueItems = value.Split(new[] { readAction.Separator }, StringSplitOptions.None);
            foreach (CopyValueToFieldModel copyInfo in readAction.CopyValueToFields)
            {
                if (readAction.IsDocIndex)
                {
                    if (document != null && !document.IsUndefinedType)
                    {
                        if (copyInfo.Position < valueItems.Length)
                        {
                            FieldValueModel index = document.FieldValues.FirstOrDefault(p => p.Field.UniqueId == copyInfo.FieldGuid);
                            if (index != null && (readAction.OverwriteFieldValue))
                            {
                                index.Value = valueItems[copyInfo.Position];
                            }
                        }
                    }
                }
                else
                {
                    if (copyInfo.Position < valueItems.Length)
                    {
                        FieldValueModel index = Batch.FieldValues.FirstOrDefault(p => p.Field.UniqueId == copyInfo.FieldGuid);
                        if (index != null && (readAction.OverwriteFieldValue))
                        {
                            index.Value = valueItems[copyInfo.Position];
                        }
                    }
                }
            }

            WriteLogTrace("END of Read2DBarcode(...) method");
        }

        private List<BarcodeData> ScanPage(string pageFile, List<BarcodeData> barcodeDatum)
        {
            WriteLogTrace("BEGIN of ScanPage(...) method");

            int nCode = _barcodeReader.ScanBarCode(pageFile);
            if (nCode <= -6)
            {
                throw new BarcodeException("License key error: either an evaluation key has expired or the license key is not valid for processing pdf documents");
            }

            if (nCode < 0)
            {
                throw new BarcodeException("Error reading barcode");
            }

            if (nCode == 0)
            {
                WriteLogTrace("END of ScanPage(...) method");
                return new List<BarcodeData>();
            }

            List<BarcodeData> barcodesInThisPage = new List<BarcodeData>();
            for (int i = nCode; i >= 1; i--)
            {
                var barcodeType = (BarcodeTypeModel)Enum.Parse(typeof(BarcodeTypeModel), _barcodeReader.GetBarStringType(i).ToUpper());
                string barcodeValue = _barcodeReader.GetBarString(i) + string.Empty;
                BarcodeData barcodeData = new BarcodeData { Type = barcodeType, Value = barcodeValue, Position = 1 };
                if (barcodeDatum.Any(p => p.Type == barcodeType))
                {
                    barcodeData.Position = barcodeDatum.Where(p => p.Type == barcodeType).Max(p => p.Position) + 1;
                }

                barcodeDatum.Add(barcodeData);
                barcodesInThisPage.Add(barcodeData);
            }

            WriteLogTrace("END of ScanPage(...) method");
            return barcodesInThisPage;
        }

        private void ConfigureReader(BarcodeConfigurationModel configuration)
        {
            WriteLogTrace("BEGIN of ConfigureReader(...) method");

            List<BarcodeTypeModel> barcodeTypes = new List<BarcodeTypeModel>();
            if (configuration.SeparationActions != null)
            {
                barcodeTypes.AddRange(from p in configuration.SeparationActions
                                      select (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), p.BarcodeType));
            }

            if (configuration.ReadActions != null)
            {
                barcodeTypes.AddRange(from p in configuration.ReadActions
                                      select (BarcodeTypeModel)Enum.ToObject(typeof(BarcodeTypeModel), p.BarcodeType));
            }

            if (barcodeTypes.Count > 0)
            {
                _barcodeReader.ReadCode128 = barcodeTypes.Contains(BarcodeTypeModel.CODE128);
                _barcodeReader.ReadCodabar = barcodeTypes.Contains(BarcodeTypeModel.CODABAR);
                _barcodeReader.ReadCode25 = barcodeTypes.Contains(BarcodeTypeModel.CODE25);
                _barcodeReader.ReadCode25ni = barcodeTypes.Contains(BarcodeTypeModel.CODE25NI);
                _barcodeReader.ReadCode39 = barcodeTypes.Contains(BarcodeTypeModel.CODE39);
                _barcodeReader.ReadCode93 = barcodeTypes.Contains(BarcodeTypeModel.CODE93);
                _barcodeReader.ReadDatabar = barcodeTypes.Contains(BarcodeTypeModel.DATABAR);
                _barcodeReader.ReadDataMatrix = barcodeTypes.Contains(BarcodeTypeModel.DATAMATRIX);
                _barcodeReader.ReadEAN13 = barcodeTypes.Contains(BarcodeTypeModel.EAN13);
                _barcodeReader.ReadEAN8 = barcodeTypes.Contains(BarcodeTypeModel.EAN8);
                _barcodeReader.ReadMicroPDF417 = barcodeTypes.Contains(BarcodeTypeModel.MICROPDF417);
                _barcodeReader.ReadPatchCodes = barcodeTypes.Contains(BarcodeTypeModel.PATCH);
                _barcodeReader.ReadPDF417 = barcodeTypes.Contains(BarcodeTypeModel.PDF417);
                _barcodeReader.ReadShortCode128 = barcodeTypes.Contains(BarcodeTypeModel.SHORTCODE128);
                _barcodeReader.ReadUPCA = barcodeTypes.Contains(BarcodeTypeModel.UPCA);
                _barcodeReader.ReadUPCE = barcodeTypes.Contains(BarcodeTypeModel.UPCE);
                _barcodeReader.ReadQRCode = barcodeTypes.Contains(BarcodeTypeModel.QRCODE);
            }

            WriteLogTrace("END of ConfigureReader(...) method");
        }

        private bool IsImageFile(string fileExtension)
        {
            string[] imageExtensions = new[] { "tif", "tiff", "png", "gif", "bmp", "jpg", "jpeg" };
            return imageExtensions.Contains((fileExtension + string.Empty).ToLower());
        }

        private bool Is2DBarcode(BarcodeTypeModel barcodeType)
        {
            return barcodeType == BarcodeTypeModel.DATAMATRIX ||
                   barcodeType == BarcodeTypeModel.MICROPDF417 ||
                   barcodeType == BarcodeTypeModel.PDF417 ||
                   barcodeType == BarcodeTypeModel.QRCODE;
        }

        private void WriteLogTrace(string message)
        {
            if (IsLogTraceEnabled && !string.IsNullOrEmpty(LogFolderPath))
            {
                try
                {
                    string filePath = Path.Combine(LogFolderPath, "BarcodeActivity.log");
                    File.AppendAllText(filePath, message + Environment.NewLine);
                }
                catch { }
            }
        }

        private void WriteLogTrace(Exception ex)
        {
            if (IsLogTraceEnabled && !string.IsNullOrEmpty(LogFolderPath))
            {
                try
                {
                    string message = ex.Message;
                    Exception innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        message += Environment.NewLine + innerException.Message;
                        innerException = innerException.InnerException;
                    }

                    message += Environment.NewLine + "Stack trace:";
                    message += Environment.NewLine + ex.StackTrace;

                    string filePath = Path.Combine(LogFolderPath, "BarcodeActivity.log");
                    File.AppendAllText(filePath, message + Environment.NewLine);
                }
                catch { }
            }
        }

    }
}