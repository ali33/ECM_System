using System;
using System.Linq;
using System.Collections.Generic;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using Ecm.Workflow.Activities.Contract;
using Ecm.CaptureDomain;
using Ecm.CaptureCore;
using Ecm.Workflow.Activities.BarcodeExecutorDesigner;
using System.Drawing;
using Ecm.CaptureBarcodeProcessing;
using Ecm.Workflow.Activities.CustomActivityDomain;
using Ecm.BarcodeDomain;
using log4net;
using Ecm.AppHelper;
using System.IO;
using Ecm.Workflow.WorkflowExtension;

[assembly: log4net.Config.XmlConfigurator(Watch = true, ConfigFile = @"log4net.xml")]

namespace Ecm.Workflow.Activities.BarcodeExecutor
{
    [Designer(typeof(BarcodeActivityDesigner))]
    [ToolboxBitmap(typeof(Barcode), "barcode.png")]

    public sealed class Barcode : StoppableActivityContract
    {
        private readonly SecurityManager _securityManager = new SecurityManager();

        [Editor(typeof(BarcodeConfigurationDesigner), typeof(DialogPropertyValueEditor))]
        public Guid Configure { get; set; }
        private readonly ILog _log = LogManager.GetLogger(typeof(Barcode));

        protected override void ExecutionBody(NativeActivityContext context)
        {
            try
            {
                var wfSystem = _securityManager.Authorize("WorkflowSystem", "TzmdoMVgNmQ5QMXJDuLBKgKg6CYfx73S/8dPX8Ytva+Eu3hlFNVoAg==");
                wfSystem.ClientHost = string.Empty;

                //Actionlog here
                WorkflowRuntimeData runtimeInfo = GetWorkflowRuntimeData(context);

                ActionLogManager actionLog = new ActionLogManager(wfSystem);

                actionLog.AddLog("Begin barcode processing", wfSystem, ActionName.ProcessBarcode, null, null);

                BatchManager batchMananger = new BatchManager(wfSystem);
                Batch batch = batchMananger.GetBatch(Guid.Parse(runtimeInfo.ObjectID.ToString()));
                BatchType batchType = batch.BatchType;
                CustomActivitySetting customSetting = GetSetting(batch.WorkflowDefinitionId, wfSystem);
                actionLog.AddLog("Get batch Id: " + batch.Id + " to process barcode", wfSystem, ActionName.ProcessBarcode, null, null);

                BatchBarcodeConfiguration barcodeConfiguration = Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(customSetting.Value);

                if (barcodeConfiguration != null)
                {
                    var serverWorkingFolder = batchMananger.GetServerWorkingFolder();
                    BatchBarcodeProcessor barcodeProcessor = new BatchBarcodeProcessor(batchType, barcodeConfiguration, serverWorkingFolder, true);
                    batch = barcodeProcessor.Process(batch);
                    //batch = ProcessBarcode(batch, serverWorkingFolder, barcodeProcessor);
                    batchMananger.UpdateBatchAfterProcessBarcode(batch);
                    actionLog.AddLog("Process barcode on batch Id: " + batch.Id + " completed successfully", wfSystem, ActionName.ProcessBarcode, null, null);
                }
                else
                {
                    actionLog.AddLog("Process barcode on batch Id: " + batch.Id + " fail. Please review barcode configuration",
                                                                wfSystem, ActionName.ProcessBarcode, null, null);
                }
                actionLog.AddLog("End barcode processing", wfSystem, ActionName.ProcessBarcode, null, null);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private CustomActivitySetting GetSetting(Guid wfDefinitionId, User user)
        {
            Guid activityId = this.UniqueID;
            return new CustomActivitySettingManager(user).GetCustomActivitySetting(wfDefinitionId, activityId);
        }
        
        //private Batch ProcessBarcode(Batch b, string workingFolder, BatchBarcodeProcessor barcodeProcessor)
        //{
        //    #region batch
        //    var outBatch = new Batch
        //    {
        //        BatchName = b.BatchName,
        //        Id = b.Id,
        //        BatchPermission = b.BatchPermission,
        //        BatchType = b.BatchType,
        //        BatchTypeId = b.BatchTypeId,
        //        BlockingActivityDescription = b.BlockingActivityDescription,
        //        BlockingActivityName = b.BlockingActivityName,
        //        BlockingBookmark = b.BlockingBookmark,
        //        BlockingDate = b.BlockingDate,
        //        Comments = b.Comments,
        //        CreatedBy = b.CreatedBy,
        //        CreatedDate = b.CreatedDate,
        //        DelegatedBy = b.DelegatedBy,
        //        DelegatedTo = b.DelegatedTo,
        //        DeletedDocuments = b.DeletedDocuments,
        //        DeletedLooseDocuments = b.DeletedLooseDocuments,
        //        DocCount = b.DocCount,
        //        Documents = new System.Collections.Generic.List<Document>(),
        //        FieldValues = b.FieldValues,
        //        HasError = b.HasError,
        //        IsCompleted = b.IsCompleted,
        //        IsProcessing = b.IsProcessing,
        //        IsRejected = b.IsRejected,
        //        LastAccessedBy = b.LastAccessedBy,
        //        LastAccessedDate = b.LastAccessedDate,
        //        LockedBy = b.LockedBy,
        //        ModifiedBy = b.ModifiedBy,
        //        ModifiedDate = b.ModifiedDate,
        //        PageCount = b.PageCount,
        //        StatusMsg = b.StatusMsg,
        //        WorkflowDefinitionId = b.WorkflowDefinitionId,
        //        WorkflowInstanceId = b.WorkflowInstanceId
        //    };

        //    #endregion
        //    List<Document> outDocs = new List<Document>();

        //    foreach (Document doc in b.Documents)
        //    {
        //        List<string> filePaths = new List<string>();

        //        foreach (Page page in doc.Pages)
        //        {
        //            string tempFile = Path.Combine(workingFolder, Guid.NewGuid() + "." + page.FileExtension.Replace(".", ""));
        //            File.WriteAllBytes(tempFile, page.FileBinary);

        //            filePaths.Add(tempFile);

        //            //string extension = (Path.GetExtension(tempFile) + string.Empty).Replace(".", "");
        //        }

        //        outBatch = Process(outBatch, doc, filePaths, barcodeProcessor);
        //    }

        //    return outBatch;
        //}

        //private Batch GetBarcodeProcessBatch(Batch batch, Document currentDoc, List<string> pageFilePaths, DocumentType docType, BatchBarcodeConfiguration barcodeConfiguration, string workingFolder)
        //{
        //    BatchBarcodeProcessor barcodeProcessor = new BatchBarcodeProcessor(batch.BatchType, barcodeConfiguration, workingFolder, true);

        //    List<Document> outDocs = new List<Document>();
        //    List<BarcodeData> barcodeData = new List<BarcodeData>();

        //    foreach (string pageFile in pageFilePaths)
        //    {
        //        Document preDoc = outDocs.Count > 0 ? outDocs.Last() : null;
        //        Document posDoc = barcodeProcessor.Process(batch, preDoc, pageFile, barcodeData, docType);

        //        if (preDoc != posDoc)
        //        {
        //            outDocs.Add(posDoc);
        //        }
        //    }
        //    batch.Documents.AddRange(outDocs);
        //    return batch;

        //}

        //public Batch Process(Batch batch, Document currentDoc, List<string> pageFilePaths, BatchBarcodeProcessor barcodeProcessor)
        //{
        //    List<string> loosePages = null;
        //    List<BarcodeData> barcodeData = new List<BarcodeData>();

        //    foreach (string pageFile in pageFilePaths)
        //    {
        //        Document document = barcodeProcessor.Process(batch, loosePages, currentDoc, pageFile, barcodeData);
        //        //batch.Documents.Add(document);
        //    }

        //    return batch;
        //}

    }
}
