using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureBarcodeProcessing;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.BarcodeHelper;

namespace Ecm.ContentViewer.Helper
{
    public class BarcodeHelper
    {
        private BatchBarcodeProcessor _barcodeProcessor;

        public BatchModel BatchData { get; set; }

        public BarcodeHelper(BatchTypeModel batchTypeModel, BarcodeConfigurationModel barcodeConfigurationModel, string workingFolder, bool isX86)
        {
            _barcodeProcessor = new BatchBarcodeProcessor(batchTypeModel, barcodeConfigurationModel, workingFolder, isX86);
        }

        public BatchModel Process(List<string> pages, out List<string> loosePages)
        {
            BatchModel batch = _barcodeProcessor.Process(pages, out loosePages);

            return batch;
        }
        
        public ContentModel Process(BatchModel batchModel, List<string> loosePages, ContentModel currentDocument, string pageFilePath, List<BarcodeData> barcodeDatum)
        {
            ContentModel currentDoc = currentDocument;

            ContentModel document = _barcodeProcessor.Process(batchModel, loosePages, currentDoc, pageFilePath, barcodeDatum);
            ContentModel documentModel = document;
            batchModel = _barcodeProcessor.Batch;
            BatchData = batchModel;
            return documentModel;
        }

        public BatchModel Process(BatchModel batchModel, ContentModel currentDocument, List<string> pageFilePaths)
        {
            ContentModel currentDoc = currentDocument;
            List<string> loosePages = null;
            List<BarcodeData> barcodeData = new List<BarcodeData>();

            foreach (string pageFile in pageFilePaths)
            {
                ContentModel document = _barcodeProcessor.Process(batchModel, loosePages, currentDoc, pageFile, barcodeData);
                batchModel.Documents.Add(document);
            }

            BatchData = batchModel;
            return batchModel;
        }
    }
}
