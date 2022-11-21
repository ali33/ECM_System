using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoftekBarcodeLib3;
using Ecm.CaptureDomain;
using Ecm.BarcodeDomain;

namespace Ecm.CaptureBarcodeProcessing
{
    public class BatchBarcodeProcessor
    {
        private readonly BarcodeReader _barcodeReader;

        public string WorkingFolder { get; private set; }

        public BatchType BatchType { get; private set; }

        public BatchBarcodeConfiguration Configuration { get; private set; }

        public Batch Batch { get; private set; }

        public bool IsLogTraceEnabled { get; set; }

        public string LogFolderPath { get; set; }

        public BatchBarcodeProcessor(BatchType batchType, BatchBarcodeConfiguration configuration, string workingFolder, bool x86)
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

        #region For barcode activity in server
        public Batch Process(Batch batch)
        {
            WriteLogTrace("BEGIN of Process(Batch batch) method");

            try
            {
                Batch = batch;
                ProcessExistedDocuments();
                ProcessLoosePages();

                // Remove new empty document just created by this method
                List<Document> newEmptyDocuments = Batch.Documents.Where(p => p.Id == Guid.Empty && p.Pages.Count == 0).ToList();
                foreach (Document newEmptyDocument in newEmptyDocuments)
                {
                    Batch.Documents.Remove(newEmptyDocument);
                    Batch.DocCount--;
                }

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

        public Batch Process(List<string> pageFilePaths, out List<string> loosePages)
        {
            Batch = new Batch();
            Batch.FieldValues.AddRange((from p in BatchType.Fields select new BatchFieldValue { FieldId = p.Id, FieldMetaData = BatchType.Fields.SingleOrDefault(q => q.Id == p.Id) }));
            List<BarcodeData> barcodeDatum = new List<BarcodeData>();
            Document currentDocument = null;
            loosePages = new List<string>();
            foreach (string pageFilePath in pageFilePaths)
            {
                currentDocument = Process(Batch, loosePages, currentDocument, pageFilePath, barcodeDatum);
            }

            List<Document> newEmptyDocuments = Batch.Documents.Where(p => p.Pages.Count == 0).ToList();
            foreach (Document newEmptyDocument in newEmptyDocuments)
            {
                Batch.Documents.Remove(newEmptyDocument);
                Batch.DocCount--;
            }

            return Batch;
        }

        public Document Process(Batch batch, List<string> loosePages, Document currentDocument, string pageFilePath, List<BarcodeData> barcodeDatum)
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
                    Document outputDocument = RunActions(barcodesOnThisPage, currentDocument,
                                                                new Page { FilePath = pageFilePath },
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
                    currentDocument.Pages.Add(new Page { FilePath = pageFilePath });
                }
                else
                {
                    loosePages.Add(pageFilePath);
                }
            }
            else if (currentDocument != null)
            {
                currentDocument.Pages.Add(new Page { FilePath = pageFilePath });
            }
            else
            {
                loosePages.Add(pageFilePath);
            }

            return currentDocument;
        }

        public Document Process(Batch batch, Document currentDocument, string pageFilePath, List<BarcodeData> barcodeDatum, DocumentType docType)
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
                    Document outputDocument = DoSeparationAction(barcodesOnThisPage, currentDocument, new Page { FilePath = pageFilePath }, out hasSeparator, out removeSeparatorPage);//RunActions(barcodesOnThisPage, currentDocument,new Page { FilePath = pageFilePath },out hasSeparator, out removeSeparatorPage);

                    if (outputDocument != null && outputDocument != currentDocument)
                    {
                        if (currentDocument != null)
                        {
                            barcodeDatum.Clear();
                        }

                        currentDocument = outputDocument;
                        //Batch.Documents.Add(currentDocument);
                        //Batch.DocCount++;

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
                        currentDocument = CreateDocument(pageFilePath, docType);
                    }

                    // 2015/05/06 - HungLe - Start
                    // Fixing bug do barcode on old doc type after do separate new doc type

                    /* Old code
                    List<ReadAction> readActions = Configuration.ReadActions.Where(p => barcodesOnThisPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
                                                                                            (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                            (p.BarcodePositionInDoc == 0 || p.BarcodePositionInDoc == q.Position) &&
                                                                                            (p.DocTypeId == docType.Id || (p.DocTypeId == Guid.Empty && !p.IsDocIndex)))).ToList();
                    */

                    List<ReadAction> readActions = Configuration.ReadActions.Where(p => barcodesOnThisPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
                                                                        (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                        (p.BarcodePositionInDoc - 1 == q.Position) &&
                                                                        (p.DocTypeId == currentDocument.DocTypeId || (p.DocTypeId == Guid.Empty && !p.IsDocIndex)))).ToList();
                    // 2015/05/06 - HungLe - End

                    DoReadAction(barcodesOnThisPage, currentDocument, readActions);
                }
                else if (currentDocument != null)
                {
                    currentDocument.Pages.Add(new Page { FilePath = pageFilePath });
                }
                else
                {
                    currentDocument = CreateDocument(pageFilePath, docType);
                }
            }
            else if (currentDocument != null)
            {
                currentDocument.Pages.Add(new Page { FilePath = pageFilePath });
            }
            else
            {
                currentDocument = CreateDocument(pageFilePath, docType);
            }

            return currentDocument;
        }

        #endregion

        #region For client barcode in client side

        #endregion

        private void ProcessLoosePages()
        {
            WriteLogTrace("BEGIN of ProcessLoosePages() method");

            Document looseDoc = Batch.Documents.FirstOrDefault(p => p.DocTypeId == Guid.Empty);
            if (looseDoc != null)
            {
                List<BarcodeData> barcodeDatum = new List<BarcodeData>();
                Document currentDocument = null;
                List<Page> loosePages = new List<Page>(looseDoc.Pages);
                foreach (Page page in loosePages)
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
                            Document outputDocument = RunActions(barcodesOnThisPage, currentDocument, page,
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

                if (looseDoc.Pages.Count == 0)
                {
                    Batch.Documents.Remove(looseDoc);
                    Batch.DeletedDocuments.Add(looseDoc.Id);
                }

                //Update document binary type
                foreach (Document newDoc in Batch.Documents.Where(p => p.BinaryType == null))
                {
                    newDoc.BinaryType = GetDocumentBinaryType(newDoc);
                }
            }

            WriteLogTrace("END of ProcessLoosePages() method");
        }

        private void ProcessExistedDocuments()
        {
            WriteLogTrace("BEGIN of ProcessExistedDocuments() method");

            List<Document> existedDocuments = Batch.Documents.Where(p => p.DocTypeId != Guid.Empty && p.Id != Guid.Empty).ToList();
            List<Page> removeRootPage = new List<Page>();

            #region for
            foreach (Document document in existedDocuments)
            {
                List<BarcodeData> barcodeDatum = new List<BarcodeData>();
                List<string> filePaths = new List<string>();

                var currentDocument = document;
                var docType = document.DocumentType;
                Document newDoc = null;
                string binaryType = string.Empty;

                foreach (Page page in document.Pages)
                {
                    if (IsImageFile(page.FileExtension))
                    {
                        string tempFile = Path.Combine(WorkingFolder, Guid.NewGuid() + "." + page.FileExtension.Replace(".", ""));
                        File.WriteAllBytes(tempFile, page.FileBinary);

                        string extension = (Path.GetExtension(tempFile) + string.Empty).Replace(".", "");
                        List<BarcodeData> barcodesOnThisPage = ScanPage(tempFile, barcodeDatum);
                        if (barcodesOnThisPage.Count > 0)
                        {
                            // Do Separation action
                            List<SeparationAction> separationActions = Configuration.SeparationActions.Where
                                (
                                    p => barcodesOnThisPage.Any
                                        (
                                            q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType)
                                                &&
                                                (
                                                    (
                                                        (
                                                            string.IsNullOrEmpty(p.StartsWith)
                                                            || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)
                                                        )
                                                        && p.BarcodePositionInDoc - 1 == q.Position
                                                    )
                                                    || q.Type == BarcodeType.PATCH
                                                )
                                        )
                                ).ToList();
                            bool hasSeparator = separationActions.Count > 0;
                            bool removeSeparatorPage = separationActions.Any(p => p.RemoveSeparatorPage);

                            if (hasSeparator)
                            {
                                //Remove current page from initialized document

                                if (newDoc != null)
                                {
                                    newDoc.BinaryType = GetDocumentBinaryType(newDoc);
                                    Batch.Documents.Add(newDoc);
                                    newDoc = null;
                                }

                                removeRootPage.Add(page);

                                //break new doc
                                newDoc = new Document
                                {
                                    BatchId = Batch.Id,
                                    CreatedBy = Batch.CreatedBy,
                                    CreatedDate = DateTime.Now,
                                    PageCount = Batch.Documents.Count,
                                    DocTypeId = Guid.Empty,
                                    IsUndefinedType = !separationActions.Any(p => p.HasSpecifyDocumentType)
                                };

                                if (!removeSeparatorPage)
                                {
                                    newDoc.Pages.Add(page);
                                }

                                if (separationActions.Any(p => p.HasSpecifyDocumentType) || BatchType.DocTypes.Count == 1)
                                {
                                    DocumentType newDocType;
                                    if (BatchType.DocTypes.Count == 1)
                                    {
                                        newDocType = BatchType.DocTypes[0];
                                    }
                                    else
                                    {
                                        newDocType = BatchType.DocTypes.FirstOrDefault(p => p.Id == separationActions.First(q => q.HasSpecifyDocumentType).DocTypeId);
                                    }

                                    if (newDocType != null)
                                    {
                                        newDoc.DocTypeId = newDocType.Id;
                                        newDoc.DocumentType = newDocType;
                                        newDoc.IsUndefinedType = false;
                                        foreach (var field in newDocType.Fields)
                                        {
                                            newDoc.FieldValues.Add(new DocumentFieldValue
                                            {
                                                FieldId = field.Id,
                                                Value = field.DefaultValue,
                                                FieldMetaData = field
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (newDoc != null)
                                {
                                    newDoc.Pages.Add(CreatePage(page));

                                    removeRootPage.Add(page);
                                }
                            }

                            // Do Read action

                            if (newDoc != null)
                            {
                                List<ReadAction> readActions = Configuration.ReadActions.Where(p => barcodesOnThisPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
                                                                                            (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                            (p.BarcodePositionInDoc - 1 == q.Position) &&
                                                                                            (p.DocTypeId == newDoc.DocumentType.Id || (p.DocTypeId == Guid.Empty && !p.IsDocIndex)))).ToList();
                                DoReadAction(barcodesOnThisPage, newDoc, readActions);
                            }
                            else
                            {
                                List<ReadAction> readActions = Configuration.ReadActions.Where(p => barcodesOnThisPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
                                                                                            (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                            (p.BarcodePositionInDoc - 1 == q.Position) &&
                                                                                            (p.DocTypeId == document.DocumentType.Id || (p.DocTypeId == Guid.Empty && !p.IsDocIndex)))).ToList();
                                DoReadAction(barcodesOnThisPage, document, readActions);

                            }


                        }
                        else
                        {
                            if (newDoc != null)
                            {
                                newDoc.Pages.Add(CreatePage(page));

                                removeRootPage.Add(page);
                            }

                        }
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch { }

                    }
                    else
                    {
                        if (newDoc != null)
                        {
                            newDoc.Pages.Add(CreatePage(page));
                            removeRootPage.Add(page);
                        }
                    }
                }

                if (newDoc != null)
                {
                    newDoc.BinaryType = GetDocumentBinaryType(newDoc);
                    Batch.Documents.Add(newDoc);
                }

            }
            #endregion End for

            foreach (Page removePage in removeRootPage)
            {
                Document removeDoc = Batch.Documents.FirstOrDefault(p => p.Id == removePage.DocId);
                removeDoc.DeletedPages.Add(removePage.Id);
                removeDoc.Pages.Remove(removePage);
            }

            WriteLogTrace("END of ProcessExistedDocuments() method");
        }

        //private void ProcessExistedDocuments()
        //{
        //    WriteLogTrace("BEGIN of ProcessExistedDocuments() method");

        //    List<Document> existedDocuments = Batch.Documents.Where(p => p.DocTypeId != Guid.Empty && p.Id != Guid.Empty).ToList();

        //    foreach (Document document in existedDocuments)
        //    {
        //        List<BarcodeData> barcodeDatum = new List<BarcodeData>();
        //        List<string> filePaths = new List<string>();
        //        var currentDocument = document;
        //        var docType = document.DocumentType;

        //        foreach (Page page in document.Pages)
        //        {
        //            if (IsImageFile(page.FileExtension))
        //            {
        //                string tempFile = Path.Combine(WorkingFolder, Guid.NewGuid() + "." + page.FileExtension.Replace(".", ""));
        //                File.WriteAllBytes(tempFile, page.FileBinary);

        //                string extension = (Path.GetExtension(tempFile) + string.Empty).Replace(".", "");
        //                if (IsImageFile(extension))
        //                {
        //                    List<BarcodeData> barcodesOnThisPage = ScanPage(tempFile, barcodeDatum);
        //                    if (barcodesOnThisPage.Count > 0)
        //                    {
        //                        bool hasSeparator;
        //                        bool removeSeparatorPage;
        //                        Document outputDocument = DoSeparationAction(barcodesOnThisPage, currentDocument, new Page { FilePath = tempFile }, out hasSeparator, out removeSeparatorPage);//RunActions(barcodesOnThisPage, currentDocument,new Page { FilePath = pageFilePath },out hasSeparator, out removeSeparatorPage);

        //                        if (outputDocument != null && outputDocument != currentDocument)
        //                        {
        //                            if (currentDocument != null)
        //                            {
        //                                barcodeDatum.Clear();
        //                            }

        //                            currentDocument = outputDocument;
        //                            Batch.Documents.Add(currentDocument);
        //                            Batch.DocCount++;

        //                            if (hasSeparator)
        //                            {
        //                                if (removeSeparatorPage)
        //                                {
        //                                    Batch.PageCount--;
        //                                }
        //                            }
        //                        }

        //                        List<ReadAction> readActions = Configuration.ReadActions.Where(p => barcodesOnThisPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
        //                                                                                                (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
        //                                                                                                (p.BarcodePositionInDoc == 0 || p.BarcodePositionInDoc == q.Position) &&
        //                                                                                                (p.IsDocIndex || (currentDocument != null && p.DocTypeId == docType.Id)))).ToList();

        //                        DoReadAction(barcodesOnThisPage, currentDocument, readActions);
        //                    }
        //                }
        //                else
        //                {
        //                    currentDocument = CreateDocument(tempFile, docType);
        //                }
        //            }
        //        }

        //    }

        //    WriteLogTrace("END of ProcessExistedDocuments() method");
        //}

        private Document RunActions(IEnumerable<BarcodeData> barcodesOnPage, Document currentDocument, Page currentPage, out bool hasSeparator, out bool removeSeparatorPage)
        {
            WriteLogTrace("BEGIN of RunActions(...) method");

            currentDocument = DoSeparationAction(barcodesOnPage, currentDocument, currentPage, out hasSeparator, out removeSeparatorPage);
            Guid currentDocTypeId = (currentDocument == null ? Guid.Empty : currentDocument.DocTypeId);

            if (hasSeparator)
            {
                List<BarcodeType> barcodeTypes = new List<BarcodeType>();

                //Reset positions of barcode on this page
                foreach (BarcodeData barcode in barcodesOnPage)
                {
                    barcodeTypes.Add(barcode.Type);
                    barcode.Position = barcodeTypes.Count(p => p == barcode.Type) - 1;
                }
            }

            List<ReadAction> readActions = Configuration.ReadActions.Where(p => barcodesOnPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
                                                                                                        (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
                                                                                                        (p.BarcodePositionInDoc - 1 == q.Position) &&
                                                                                                        (p.DocTypeId == currentDocTypeId || (p.DocTypeId == Guid.Empty && !p.IsDocIndex)))).ToList();
            DoReadAction(barcodesOnPage, currentDocument, readActions);

            WriteLogTrace("END of RunActions(...) method");
            return currentDocument;
        }

        private Document DoSeparationAction(IEnumerable<BarcodeData> barcodesOnPage, Document currentDocument, Page currentPage, out bool hasSeparator,
                                                   out bool removeSeparatorPage)
        {
            WriteLogTrace("BEGIN of DoSeparationAction(...) method");

            //List<SeparationAction> separationActions = Configuration.SeparationActions.Where(p => barcodesOnPage.Any(q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType) &&
            //                                                                                                       (string.IsNullOrEmpty(p.StartsWith) || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)) &&
            //                                                                                                       (p.BarcodePositionInDoc - 1 == q.Position))).ToList();

            List<SeparationAction> separationActions = Configuration.SeparationActions.Where
                                (
                                    p => barcodesOnPage.Any
                                        (
                                            q => q.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType)
                                                &&
                                                (
                                                    (
                                                        (
                                                            string.IsNullOrEmpty(p.StartsWith)
                                                            || q.Value.StartsWith(p.StartsWith, StringComparison.OrdinalIgnoreCase)
                                                        )
                                                        && p.BarcodePositionInDoc - 1 == q.Position
                                                    )
                                                    || q.Type == BarcodeType.PATCH
                                                )
                                        )
                                ).ToList();

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
                    currentDocument = new Document
                    {
                        BatchId = Batch.Id,
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
                    DocumentType docType;
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
                        currentDocument.DocumentType = docType;
                        currentDocument.IsUndefinedType = false;
                        foreach (var field in docType.Fields)
                        {
                            currentDocument.FieldValues.Add(new DocumentFieldValue
                            {
                                FieldId = field.Id,
                                Value = field.DefaultValue,
                                FieldMetaData = field
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

        private void DoReadAction(IEnumerable<BarcodeData> barcodesOnPage, Document document, IEnumerable<ReadAction> readActions)
        {
            WriteLogTrace("BEGIN of DoReadAction(...) method");

            foreach (ReadAction readAction in readActions)
            {
                if (Is2DBarcode((BarcodeType)Enum.ToObject(typeof(BarcodeType), readAction.BarcodeType)) && !string.IsNullOrEmpty(readAction.Separator))
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

        private void Read1DBarcode(IEnumerable<BarcodeData> barcodesOnPage, ReadAction readAction, Document document)
        {
            WriteLogTrace("BEGIN of Read1DBarcode(...) method");

            string indexGuid = readAction.CopyValueToFields[0].FieldGuid;
            string value = barcodesOnPage.First(p => p.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), readAction.BarcodeType) &&
                                                     (readAction.BarcodePositionInDoc - 1 == p.Position) &&
                                                     (string.IsNullOrEmpty(readAction.StartsWith) ||
                                                     p.Value.StartsWith(readAction.StartsWith, StringComparison.OrdinalIgnoreCase))).Value;
            if (readAction.IsDocIndex)
            {
                if (document != null && !document.IsUndefinedType)
                {
                    DocumentFieldValue index = document.FieldValues.FirstOrDefault(p => p.FieldMetaData.UniqueId == indexGuid);
                    if (index != null)
                    {
                        // Set value for batch field in runtime workflow
                        if (string.IsNullOrWhiteSpace(index.Value)
                            || (!string.IsNullOrWhiteSpace(value) && readAction.OverwriteFieldValue))
                        {
                            index.Value = value;
                        }

                        // Set value for batch field in first time capture
                        index.BarcodeOverride = readAction.OverwriteFieldValue;
                        index.BarcodeValue = value;
                    }
                }
            }
            else
            {
                BatchFieldValue index = Batch.FieldValues.FirstOrDefault(p => p.FieldMetaData.UniqueId == indexGuid);
                if (index != null)
                {
                    // Set value for batch field in runtime workflow
                    if (string.IsNullOrWhiteSpace(index.Value)
                        || (!string.IsNullOrWhiteSpace(value) && readAction.OverwriteFieldValue))
                    {
                        index.Value = value;
                    }

                    // Set value for batch field in first time capture
                    index.BarcodeOverride = readAction.OverwriteFieldValue;
                    index.BarcodeValue = value;
                }
            }

            WriteLogTrace("END of Read1DBarcode(...) method");
        }

        private void Read2DBarcode(IEnumerable<BarcodeData> barcodesOnPage, ReadAction readAction, Document document)
        {
            WriteLogTrace("BEGIN of Read2DBarcode(...) method");

            string value = barcodesOnPage.First(p => p.Type == (BarcodeType)Enum.ToObject(typeof(BarcodeType), readAction.BarcodeType) &&
                                                     (readAction.BarcodePositionInDoc - 1 == p.Position) &&
                                                     (string.IsNullOrEmpty(readAction.StartsWith) ||
                                                     p.Value.StartsWith(readAction.StartsWith, StringComparison.OrdinalIgnoreCase))).Value;
            string[] valueItems = value.Split(new[] { readAction.Separator }, StringSplitOptions.None);
            foreach (CopyValueToField copyInfo in readAction.CopyValueToFields)
            {
                if (readAction.IsDocIndex)
                {
                    if (document != null && !document.IsUndefinedType)
                    {
                        if (copyInfo.Position < valueItems.Length)
                        {
                            DocumentFieldValue index = document.FieldValues.FirstOrDefault(p => p.FieldMetaData.UniqueId == copyInfo.FieldGuid);
                            if (index != null)
                            {
                                // Set value for batch field in runtime workflow
                                if (string.IsNullOrWhiteSpace(index.Value)
                                    || (!string.IsNullOrWhiteSpace(value) && readAction.OverwriteFieldValue))
                                {
                                    index.Value = valueItems[copyInfo.Position];
                                }

                                // Set value for batch field in first time capture
                                index.BarcodeOverride = readAction.OverwriteFieldValue;
                                index.BarcodeValue = valueItems[copyInfo.Position];
                            }
                        }
                    }
                }
                else
                {
                    if (copyInfo.Position < valueItems.Length)
                    {
                        BatchFieldValue index = Batch.FieldValues.FirstOrDefault(p => p.FieldMetaData.UniqueId == copyInfo.FieldGuid);
                        if (index != null)
                        {
                            // Set value for batch field in runtime workflow
                            if (string.IsNullOrWhiteSpace(index.Value)
                                || (!string.IsNullOrWhiteSpace(value) && readAction.OverwriteFieldValue))
                            {
                                index.Value = valueItems[copyInfo.Position];
                            }

                            // Set value for batch field in first time capture
                            index.BarcodeOverride = readAction.OverwriteFieldValue;
                            index.BarcodeValue = valueItems[copyInfo.Position];
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
                var barcodeType = (BarcodeType)Enum.Parse(typeof(BarcodeType), _barcodeReader.GetBarStringType(i).ToUpper());
                string barcodeValue = _barcodeReader.GetBarString(i) + string.Empty;
                BarcodeData barcodeData = new BarcodeData { Type = barcodeType, Value = barcodeValue, Position = 0 };
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

        private void ConfigureReader(BatchBarcodeConfiguration configuration)
        {
            WriteLogTrace("BEGIN of ConfigureReader(...) method");

            List<BarcodeType> barcodeTypes = new List<BarcodeType>();
            if (configuration != null)
            {
                if (configuration.SeparationActions != null)
                {
                    barcodeTypes.AddRange(from p in configuration.SeparationActions
                                          select (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType));
                }

                if (configuration.ReadActions != null)
                {
                    barcodeTypes.AddRange(from p in configuration.ReadActions
                                          select (BarcodeType)Enum.ToObject(typeof(BarcodeType), p.BarcodeType));
                }
            }

            if (barcodeTypes.Count > 0)
            {
                _barcodeReader.ReadCode128 = barcodeTypes.Contains(BarcodeType.CODE128);
                _barcodeReader.ReadCodabar = barcodeTypes.Contains(BarcodeType.CODABAR);
                _barcodeReader.ReadCode25 = barcodeTypes.Contains(BarcodeType.CODE25);
                _barcodeReader.ReadCode25ni = barcodeTypes.Contains(BarcodeType.CODE25NI);
                _barcodeReader.ReadCode39 = barcodeTypes.Contains(BarcodeType.CODE39);
                _barcodeReader.ReadCode93 = barcodeTypes.Contains(BarcodeType.CODE93);
                _barcodeReader.ReadDatabar = barcodeTypes.Contains(BarcodeType.DATABAR);
                _barcodeReader.ReadDataMatrix = barcodeTypes.Contains(BarcodeType.DATAMATRIX);
                _barcodeReader.ReadEAN13 = barcodeTypes.Contains(BarcodeType.EAN13);
                _barcodeReader.ReadEAN8 = barcodeTypes.Contains(BarcodeType.EAN8);
                _barcodeReader.ReadMicroPDF417 = barcodeTypes.Contains(BarcodeType.MICROPDF417);
                _barcodeReader.ReadPatchCodes = barcodeTypes.Contains(BarcodeType.PATCH);
                _barcodeReader.ReadPDF417 = barcodeTypes.Contains(BarcodeType.PDF417);
                _barcodeReader.ReadShortCode128 = barcodeTypes.Contains(BarcodeType.SHORTCODE128);
                _barcodeReader.ReadUPCA = barcodeTypes.Contains(BarcodeType.UPCA);
                _barcodeReader.ReadUPCE = barcodeTypes.Contains(BarcodeType.UPCE);
                _barcodeReader.ReadQRCode = barcodeTypes.Contains(BarcodeType.QRCODE);
            }

            WriteLogTrace("END of ConfigureReader(...) method");
        }

        private bool IsImageFile(string fileExtension)
        {
            string[] imageExtensions = new[] { "tif", "tiff", "png", "gif", "bmp", "jpg", "jpeg" };
            return imageExtensions.Contains((fileExtension + string.Empty).ToLower());
        }

        private bool IsNativeFile(string fileExtension)
        {
            string[] imageExtensions = new[] { "doc", "docx", "pdf", "xls", "xlsx", "ppt", "pptx", "txt", "csv", "rtf", "htm", "html", "xml", "xaml", "xps" };
            return imageExtensions.Contains((fileExtension + string.Empty).ToLower());
        }

        private bool IsMediaFile(string fileExtension)
        {
            string[] imageExtensions = new[] { "aiff", "asf", "au", "avi", "dvr-ms", "m1v", "mid",
                                          "midi", "mp3", "mp4", "mpe", "mpeg",
                                          "mpg", "rmi", "vob", "wav", "wm", "wma",
                                          "wmv", "dat", "flv",
                                          "m4v", "mov", "3gp", "3g2", "m2v"};

            return imageExtensions.Contains((fileExtension + string.Empty).ToLower());
        }

        private string GetDocumentBinaryType(Document doc)
        {
            var fileTypes = doc.Pages.Where(p => IsImageFile(p.FileExtension) || IsNativeFile(p.FileExtension) || IsMediaFile(p.FileExtension)).Select(q => q.FileExtension).Distinct().ToList();
            var fileTypeCount = fileTypes.Count();
            string binaryType = string.Empty;

            if (fileTypeCount > 1)
            {
                binaryType = "Compount";
            }
            else if (fileTypeCount == 1 && IsImageFile(fileTypes[0]))
            {
                binaryType = "Image";
            }
            else if (fileTypeCount == 1 && IsNativeFile(fileTypes[0]))
            {
                binaryType = "Native";
            }
            else if (fileTypeCount == 1 && IsMediaFile(fileTypes[0]))
            {
                binaryType = "Media";
            }

            return binaryType;
        }

        private bool Is2DBarcode(BarcodeType barcodeType)
        {
            return barcodeType == BarcodeType.DATAMATRIX ||
                   barcodeType == BarcodeType.MICROPDF417 ||
                   barcodeType == BarcodeType.PDF417 ||
                   barcodeType == BarcodeType.QRCODE;
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

        private Document CreateDocument(string pageFilePath, DocumentType docType)
        {
            var currentDocument = new Document
            {
                BatchId = Batch.Id,
                CreatedBy = Batch.CreatedBy,
                CreatedDate = DateTime.Now,
                DocTypeId = docType.Id,
                DocumentType = docType
            };

            foreach (var field in docType.Fields)
            {
                if (!field.IsSystemField)
                {
                    // 2015/05/06 - HungLe - Start
                    // Modify to add default value when do barcode processing

                    /* Old code
                    currentDocument.FieldValues.Add(new DocumentFieldValue { FieldMetaData = field, FieldId = field.Id });
                    */

                    currentDocument.FieldValues.Add(new DocumentFieldValue
                    {
                        FieldMetaData = field,
                        FieldId = field.Id,
                        Value = field.DefaultValue
                    });
                    // 2015/05/06 - HungLe - End
                }
            }

            currentDocument.Pages.Add(new Page { FilePath = pageFilePath });


            return currentDocument;
        }

        private Page CreatePage(Page page)
        {
            return new Page
            {
                Annotations = page.Annotations,
                Content = page.Content,
                ContentLanguageCode = page.ContentLanguageCode,
                DeleteAnnotations = page.DeleteAnnotations,
                FileBinary = page.FileBinary,
                FileExtension = page.FileExtension,
                FileHash = page.FileHash,
                FileHeader = page.FileHeader,
                FilePath = page.FilePath,
                Height = page.Height,
                IsRejected = page.IsRejected,
                OriginalFileName = page.OriginalFileName,
                RotateAngle = page.RotateAngle,
                Width = page.Width
            };
        }
    }
}