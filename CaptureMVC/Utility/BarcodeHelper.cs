using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureBarcodeProcessing;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureDomain;
using Ecm.BarcodeDomain;

namespace CaptureMVC.Utility
{
    public class BarcodeHelper
    {
        private BatchBarcodeProcessor _barcodeProcessor;

        //public Batch BatchData { get; set; }

        //public BarcodeHelper(BatchTypeModel batchTypeModel, BarcodeConfigurationModel barcodeConfigurationModel, string workingFolder, bool isX86)
        //{
        //    _barcodeProcessor = new BatchBarcodeProcessor(ObjectMapper.GetBatchType(batchTypeModel), ObjectMapper.GetBarcodeConfiguration(barcodeConfigurationModel), workingFolder, isX86);
        //}

        public BarcodeHelper(BatchType batchTypeModel, BatchBarcodeConfiguration barcodeConfigurationModel, string workingFolder, bool isX86)
        {
            _barcodeProcessor = new BatchBarcodeProcessor(batchTypeModel, barcodeConfigurationModel, workingFolder, isX86);
        }

        public Batch Process(List<string> pages, out List<string> loosePages)
        {
            return _barcodeProcessor.Process(pages, out loosePages);
        }

        //public DocumentModel Process(BatchModel batchModel, List<string> loosePages, DocumentModel currentDocument, string pageFilePath, List<BarcodeData> barcodeDatum)
        //{
        //    Batch batch = ObjectMapper.GetBatch(batchModel);
        //    Document currentDoc = ObjectMapper.GetDocument(currentDocument);

        //    Document document = _barcodeProcessor.Process(batch, loosePages, currentDoc, pageFilePath, barcodeDatum);
        //    DocumentModel documentModel = ObjectMapper.GetDocumentModel(document);
        //    batchModel = ObjectMapper.GetBatchModel(_barcodeProcessor.Batch);
        //    BatchData = batchModel;
        //    return documentModel;
        //}

        //public DocumentModel Process(BatchModel batchModel, DocumentModel currentDocument, string pageFilePath, DocTypeModel documentType)
        //{
        //    Batch batch = ObjectMapper.GetBatch(batchModel);
        //    Document currentDoc = ObjectMapper.GetDocument(currentDocument);
        //    DocumentType docType = ObjectMapper.GetDocType(documentType);
        //    List<BarcodeData> barcodeData = new List<BarcodeData>();

        //    Document document = _barcodeProcessor.Process(batch, currentDoc, pageFilePath, barcodeData, docType);
        //    DocumentModel documentModel = ObjectMapper.GetDocumentModel(document);
        //    currentDocument = ObjectMapper.GetDocumentModel(currentDoc);
        //    batchModel = ObjectMapper.GetBatchModel(_barcodeProcessor.Batch);
        //    BatchData = batchModel;
        //    return documentModel;

        //}

        //public BatchModel Process(BatchModel batchModel, DocumentModel currentDocument, List<string> pageFilePaths)
        //{
        //    Batch batch = ObjectMapper.GetBatch(batchModel);
        //    Document currentDoc = ObjectMapper.GetDocument(currentDocument);
        //    List<string> loosePages = null;
        //    List<BarcodeData> barcodeData = new List<BarcodeData>();

        //    foreach (string pageFile in pageFilePaths)
        //    {
        //        Document document = _barcodeProcessor.Process(batch, loosePages, currentDoc, pageFile, barcodeData);
        //        batch.Documents.Add(document);
        //    }

        //    batchModel = ObjectMapper.GetBatchModel(batch);
        //    BatchData = batchModel;
        //    return batchModel;
        //}

        //public BatchModel Process(BatchModel batchModel, List<string> pageFilePaths, DocTypeModel documentType)
        //{
        //    Batch batch = ObjectMapper.GetBatch(batchModel);
        //    DocumentType docType = ObjectMapper.GetDocType(documentType);
        //    List<Document> outDocs = new List<Document>();
        //    List<BarcodeData> barcodeData = new List<BarcodeData>();

        //    foreach (string pageFile in pageFilePaths)
        //    {
        //        Document preDoc = outDocs.Count > 0 ? outDocs.Last() : null;
        //        Document posDoc = _barcodeProcessor.Process(batch, preDoc, pageFile, barcodeData, docType);

        //        if (preDoc != posDoc)
        //        {
        //            outDocs.Add(posDoc);
        //        }
        //    }
        //    batch.Documents = outDocs;
        //    batchModel = ObjectMapper.GetBatchModel(batch);
        //    BatchData = batchModel;
        //    return batchModel;
        //}

    }
}
