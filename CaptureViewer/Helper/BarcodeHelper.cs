using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureBarcodeProcessing;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureDomain;

namespace Ecm.CaptureViewer.Helper
{
    public class BarcodeHelper
    {
        private BatchBarcodeProcessor _barcodeProcessor;

        public BatchModel BatchData { get; set; }

        public BarcodeHelper(BatchTypeModel batchTypeModel, BarcodeConfigurationModel barcodeConfigurationModel, string workingFolder, bool isX86)
        {
            _barcodeProcessor = new BatchBarcodeProcessor(ObjectMapper.GetBatchType(batchTypeModel), ObjectMapper.GetBarcodeConfiguration(barcodeConfigurationModel), workingFolder, isX86);
        }

        public BatchModel Process(List<string> pages, out List<string> loosePages)
        {
            Batch batch = _barcodeProcessor.Process(pages, out loosePages);
            BatchModel batchModel = ObjectMapper.GetBatchModel(batch);

            return batchModel;
        }

        public DocumentModel Process(BatchModel batchModel, List<string> loosePages, DocumentModel currentDocument, string pageFilePath, List<BarcodeData> barcodeDatum)
        {
            Batch batch = ObjectMapper.GetBatch(batchModel);
            Document currentDoc = ObjectMapper.GetDocument(currentDocument);

            Document document = _barcodeProcessor.Process(batch, loosePages, currentDoc, pageFilePath, barcodeDatum);
            DocumentModel documentModel = ObjectMapper.GetDocumentModel(document);
            batchModel = ObjectMapper.GetBatchModel(_barcodeProcessor.Batch);
            BatchData = batchModel;
            return documentModel;
        }

        public DocumentModel Process(BatchModel batchModel, DocumentModel currentDocument, string pageFilePath, DocTypeModel documentType)
        {
            Batch batch = ObjectMapper.GetBatch(batchModel);
            Document currentDoc = ObjectMapper.GetDocument(currentDocument);
            DocumentType docType = ObjectMapper.GetDocType(documentType);
            List<BarcodeData> barcodeData = new List<BarcodeData>();

            Document document = _barcodeProcessor.Process(batch, currentDoc, pageFilePath, barcodeData, docType);
            DocumentModel documentModel = ObjectMapper.GetDocumentModel(document);
            currentDocument = ObjectMapper.GetDocumentModel(currentDoc);
            batchModel = ObjectMapper.GetBatchModel(_barcodeProcessor.Batch);
            BatchData = batchModel;
            return documentModel;

        }

        public BatchModel Process(BatchModel batchModel, DocumentModel currentDocument, List<string> pageFilePaths)
        {
            Batch batch = ObjectMapper.GetBatch(batchModel);
            Document currentDoc = ObjectMapper.GetDocument(currentDocument);
            List<string> loosePages = null;
            List<BarcodeData> barcodeData = new List<BarcodeData>();

            foreach (string pageFile in pageFilePaths)
            {
                Document document = _barcodeProcessor.Process(batch, loosePages, currentDoc, pageFile, barcodeData);
                batch.Documents.Add(document);
            }

            batchModel = ObjectMapper.GetBatchModel(batch);
            BatchData = batchModel;
            return batchModel;
        }

        public BatchModel Process(BatchModel batchModel, List<string> pageFilePaths, DocTypeModel documentType)
        {
            Batch batch = ObjectMapper.GetBatch(batchModel);
            DocumentType docType = ObjectMapper.GetDocType(documentType);
            List<Document> outDocs = new List<Document>();
            List<BarcodeData> barcodeData = new List<BarcodeData>();

            foreach (string pageFile in pageFilePaths)
            {

                Document preDoc = outDocs.Count > 0 ? outDocs.Last() : null;
                Document posDoc = _barcodeProcessor.Process(batch, preDoc, pageFile, barcodeData, docType);

                // 2015/05/06 - HungLe - Start
                // Fixing bug do barcode on old doc type after do separate new doc type

                /* Old code
                if (preDoc != posDoc)
                {
                    outDocs.Add(posDoc);
                }
                */

                if (preDoc != posDoc)
                {
                    outDocs.Add(posDoc);
                    docType = posDoc.DocumentType;
                }
                // 2015/05/06 - HungLe - End

            }
            batch.Documents = outDocs;
            batchModel = ObjectMapper.GetBatchModel(batch);
            BatchData = batchModel;
            return batchModel;
        }

    }
}
