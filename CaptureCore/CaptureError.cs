using ChinhDo.Transactions;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.SecurityDao;
using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Ecm.CaptureCore
{
    //public class CaptureError
    //{
    //    public const string DB_BATCH_NULL = "DB_BATCH_NULL";

    //    public const string BATCH_IS_COMPELETED = "BATCH_IS_COMPELETED";
    //    public const string BATCH_LOCKED_BY_OTHER = "BATCH_LOCKED_BY_OTHER";
    //    public const string BATCH_IS_PROCESSING = "BATCH_IS_PROCESSING";
    //    public const string BATCH_HAS_ERROR = "BATCH_HAS_ERROR";

    //    public const string BATCH_NULL_FIELD_VALUES = "BATCH_NULL_FIELD_VALUES";
    //    public const string BATCH_NULL_DB_FIELD_VALUES = "BATCH_NULL_DB_FIELD_VALUES";
    //    public const string BATCH_NULL_PERMISSION = "BATCH_NULL_PERMISSION";
    //    public const string BATCH_NULL_DOCUMENTS = "BATCH_NULL_DOCUMENTS";
    //    public const string BATCH_NULL_DB_DOCUMENTS = "BATCH_NULL_DB_DOCUMENTS";
    //    public const string BATCH_NULL_FIELD_META_DATAS = "BATCH_NULL_FIELD_META_DATAS";

    //    public const string BATCH_INVALID_FIELD_VALUE = "BATCH_INVALID_FIELD_VALUE";
    //    public const string BATCH_INVALID_TRANSACTION = "BATCH_INVALID_TRANSACTION";
    //    public const string BATCH_INVALID_DELETE_DOCUMENT = "BATCH_INVALID_DELETE_DOCUMENT";

    //    public const string BATCH_NO_PERMISSION_MODIFY_INDEX = "BATCH_NO_PERMISSION_MODIFY_INDEX";
    //    public const string BATCH_NO_PERMISSION_MODIFY_DOCUMENT = "BATCH_NO_PERMISSION_MODIFY_DOCUMENT";

    //    public const string BATCH_OVERLAP_SAVE_AND_DELETE_DOCUMENTS = "BATCH_OVERLAP_SAVE_AND_DELETE_DOCUMENTS";

    //    public const string DOC_NULL_FIELD_META_DATAS = "DOC_NULL_FIELD_META_DATAS";
    //    public const string DOC_NULL_PICKLIST_META_DATAS = "DOC_NULL_PICKLIST_META_DATAS";
    //    public const string DOC_NULL_FIELD_CHILDREN_META_DATAS = "DOC_NULL_FIELD_CHILDREN_META_DATAS";
    //}

    public enum CaptureError
    {
        None,
        NullDbBatch,
        BatchIsCompleted,
        BatchIsProcessing,
        BatchHasError,
        BatchLockedByOther,
        InvalidTransaction,
        NullPermmission,
        NullBatchFieldValues,
        NullBatchFieldMetaData,
        NullDbBatchFieldValues,
        DiffDbBatchFieldValues,
        DiffBatchFieldValues,
        NullBatchFieldValue,
        NullDbBatchFieldValue,
        BatchFieldValueMaxLegth,
        InvalidBatchFieldValueDataType,
        InvalidBatchValueInteger,
        InvalidBatchValueDecimal,
        InvalidBatchValueDate,
        InvalidBatchValueBool,
        NullComment,
        NullDbDocuments,
        InvalidDeleteDoc,
        NullDocuments,
        NullDocFieldMetaDatas,
        NullDocFieldMetaPicklists,
        NullDocFieldMetaChildren,
        NullDocFieldValues,
        NullDocFieldValue,
        MissingRequiredValue,
        DocFieldValueMaxLegth,
        InvalidDocFieldValueDataType,
        InvalidDocValueInteger,
        InvalidDocValueDecimal,
        InvalidDocValueDate,
        InvalidDocValueBool,
        InvalidDocValuePicklist,
        InvalidTableValueDataType,
        InvalidTableValueInteger,
        InvalidTableValueDecimal,
        InvalidTableValueDate,
        TableFieldValueMaxLegth,
        NullPages,
        InvalidAnnoType,
        NullContentAnnoText,
        InvalidIdPage,
        InvalidIdDoc,
        InvalidIdDocType,
        DiffDocIdOfPage,
        NullContentPage,
        DiffBatchIdOfDoc,
        DiffDocTypeIdOfFieldValue,

        InvalidBatchData,

        InvalidDataAnno,
        InvalidDataPage,
        InvalidDataDoc,
        InvalidDataDocField,
        InvalidDataTableField,
        InvalidDataBatch,
        InvalidDataBatchField,


        ConflicDataDocAndPage,
        ConflicDataDocAndField,
        ConflicDataBatchAndDoc,
        ConflicDataBatchAndField,


        RequiredDocField,
        RequiredTableField,

        NulDBBatchMetas,
        NulDBDocMetas,
        NulDBPicklistMetas,
        NulDBChildrenMetas,

        NullDatabase,

        NoPermission,

        End
    }

    public partial class BatchManager
    {
        public void AddBatch(Batch batch, Platform platform)
        {
            // Check valid
            if (batch.Documents == null || batch.Documents.Count == 0)
            {
                throw new CaptureException(CaptureError.InvalidDataBatch, "List document null.");
            }

            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                context.BeginTransaction();

                // Get batch type permission
                #region
                var batchTypePermissionDao = new BatchTypePermissionDao(context);
                BatchTypePermission batchTypePermission = null;

                if (LoginUser.IsAdmin)
                {
                    batchTypePermission = BatchTypePermission.GetAllowAll();
                }
                else
                {
                    // Get list group user id
                    List<Guid> groupIds = null;
                    using (Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }
                    // Get list group permission
                    var batchTypePermissions = batchTypePermissionDao.GetByUser(groupIds, batch.BatchTypeId);
                    // Union permission
                    batchTypePermission = new BatchTypePermission();
                    foreach (var batchTypeItem in batchTypePermissions)
                    {
                        batchTypePermission.CanAccess |= batchTypeItem.CanAccess;
                        batchTypePermission.CanCapture |= batchTypeItem.CanCapture;
                        batchTypePermission.CanClassify |= batchTypeItem.CanClassify;
                        batchTypePermission.CanIndex |= batchTypeItem.CanIndex;
                    }
                }
                #endregion

                // Main info batch
                #region
                var wfDao = new WorkflowDefinitionDao(context);
                // Get work flow
                var wfDefinition = wfDao.GetByBatchTypeId(batch.BatchTypeId);
                if (wfDefinition == null)
                {
                    throw new CaptureException(CaptureError.NullDatabase,
                                                "Work-flow definition Id = " + batch.BatchTypeId);
                }
                // Check permission capture
                if (!batchTypePermission.CanCapture)
                {
                    throw new CaptureException(CaptureError.NoPermission,
                                                "Can not capture batch type Id = " + batch.BatchTypeId);
                }

                // Set batch info
                batch.Id = Guid.NewGuid();
                batch.TransactionId = Guid.NewGuid();
                batch.DocCount = batch.Documents.Count;
                batch.IsProcessing = true;
                batch.IsCompleted = false;
                batch.IsRejected = false;
                batch.HasError = false;
                batch.WorkflowDefinitionId = wfDefinition.Id;
                batch.CreatedDate = DateTime.Now;
                batch.CreatedBy = LoginUser.UserName;
                batch.StatusMsg = "Creating batch";
                if (string.IsNullOrWhiteSpace(batch.BatchName))
                {
                    // Generate default batch name
                    batch.BatchName = LoginUser.UserName + "_" + batch.CreatedDate.ToString("yyyy/MM/dd");
                }
                else
                {
                    batch.BatchName = batch.BatchName.Trim();
                }

                // Insert batch to DB
                var batchDao = new BatchDaoHung(context);
                batchDao.Add(batch);
                #endregion

                // Batch index
                #region

                // Get batch field meta data
                var batchMetas = (new BatchFieldMetaDataDao(context)).GetByBatchType(batch.BatchTypeId);
                if (batchMetas == null || batchMetas.Count == 0)
                {
                    throw new CaptureException(CaptureError.NullDatabase,
                                                "List batch field meta data of batch type = " + batch.BatchTypeId);
                }

                // Initialize batch values if not existed
                if (batch.FieldValues == null)
                {
                    batch.FieldValues = new List<BatchFieldValue>();
                }

                var batchValueDao = new BatchFieldValueDao(context);
                BatchFieldValue batchValue;
                foreach (var batchMeta in batchMetas)
                {
                    #region
                    // Get submit batch value
                    batchValue = batch.FieldValues.FirstOrDefault(h => h.FieldId == batchMeta.Id);

                    // Create new if not found
                    if (batchValue == null)
                    {
                        batchValue = new BatchFieldValue();
                        batchValue.Value = null;
                        batchValue.FieldId = batchMeta.Id;
                    }
                    else
                    {
                        // Validate value
                        ValidateBatchFieldValue(batchValue, batchMeta);
                    }

                    // Insert to DB
                    batchValue.BatchId = batch.Id;
                    batchValueDao.Add(batchValue);
                    #endregion
                }

                #endregion

                // Comments
                AddComments(batch.Comments, batch.Id, context);

                // Document
                #region
                var dicDocFieldMeta = new Dictionary<Guid, List<DocumentFieldMetaData>>();
                var workingIndex = 1;

                var pageCount = 0;
                var docDao = new DocumentDao(context);
                var docTypeDao = new DocumentTypeDao(context);
                var docMetaDao = new DocFieldMetaDataDao(context);
                var docValueDao = new DocumentFieldValueDao(context);
                var picklistDao = new PicklistDao(context);
                var tableDao = new TableFieldValueDao(context);
                var pageDao = new PageDao(context);
                var annoDao = new AnnotationDao(context);

                // Get list docType
                var docTypes = docTypeDao.GetDocumentTypeByBatch(batch.BatchTypeId);

                // Loose doc
                var looseDoc = batch.Documents.FirstOrDefault(h => h.DocTypeId == Guid.Empty);
                if (looseDoc != null)
                {
                    looseDoc.BatchId = batch.Id;
                    looseDoc.Order = 0;
                    AddDocument(looseDoc, platform, null, batch.BatchTypeId, docDao,
                                docValueDao, picklistDao, tableDao, pageDao, annoDao);

                    pageCount += looseDoc.PageCount;
                }

                // Normal doc
                foreach (var doc in batch.Documents.OrderBy(h => h.Order))
                {
                    if (doc.DocTypeId == Guid.Empty)
                    {
                        continue;
                    }
                    // Get document field meta data
                    if (!dicDocFieldMeta.ContainsKey(doc.DocTypeId))
                    {
                        if (!docTypes.Any(h => h.Id == doc.DocTypeId))
                        {
                            throw new CaptureException(CaptureError.InvalidDataBatch,
                                                        "Invalid doc type Id = " + doc.DocTypeId.ToString());
                        }

                        #region
                        var docMetas = docMetaDao.GetByDocType(doc.DocTypeId);
                        if (docMetas == null || docMetas.Count == 0)
                        {
                            throw new CaptureException(CaptureError.NullDatabase,
                                                        "List doc field meta data of doc type = " + doc.DocTypeId);
                        }

                        foreach (var docMeta in docMetas)
                        {
                            docMeta.DataType = string.Format("{0}", docMeta.DataType).Trim().ToLower();

                            if (docMeta.DataType == "picklist")
                            {
                                #region
                                var picklists = picklistDao.GetByField(docMeta.Id);
                                if (picklists == null || picklists.Count == 0)
                                {
                                    throw new CaptureException(CaptureError.NullDatabase,
                                                                "Pick-list meta data of field = " + docMeta.Id);
                                }

                                foreach (var picklist in picklists)
                                {
                                    picklist.Value = string.Format("{0}", picklist.Value).Trim();
                                }
                                docMeta.Picklists = picklists;
                                #endregion
                            }
                            // Case table => get children field
                            else if (docMeta.DataType == "table")
                            {
                                #region
                                var children = docMetaDao.GetChildren(docMeta.Id);
                                if (children == null || children.Count == 0)
                                {
                                    throw new CaptureException(CaptureError.NullDatabase,
                                                                "List children field meta data of field = "
                                                                + docMeta.Id);
                                }
                                foreach (var child in children)
                                {
                                    child.DataType = string.Format("{0}", child.DataType).Trim().ToLower();
                                }
                                docMeta.Children = children;
                                #endregion
                            }
                        }

                        dicDocFieldMeta.Add(doc.DocTypeId, docMetas);
                        #endregion
                    }

                    doc.BatchId = batch.Id;
                    doc.Order = workingIndex++;
                    AddDocument(doc, platform, dicDocFieldMeta[doc.DocTypeId], batch.BatchTypeId, docDao,
                                docValueDao, picklistDao, tableDao, pageDao, annoDao);

                    pageCount += doc.PageCount;
                }
                #endregion

                // Update page count for batch
                batchDao.UpdatePageCount(batch.Id, pageCount);

                context.Commit();

                // Call work-flow
                CallWorkflow(batch, true);
            }
        }

        public void UpdateBatch(Batch batch, Platform platform)
        {
            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                context.BeginTransaction();

                var batchDao = new BatchDaoHung(context);
                // Get DB batch
                var dbBatch = batchDao.GetById(batch.Id);
                if (dbBatch == null)
                {
                    throw new CaptureException(CaptureError.NullDatabase, string.Format("Batch '{0}'", batch.Id));
                }

                List<Guid> groupIds = null;
                BatchTypePermission batchTypePermission;
                // Get and check batch type permission
                #region
                if (LoginUser.IsAdmin)
                {
                    batchTypePermission = BatchTypePermission.GetAllowAll();
                }
                else
                {
                    var batchTypePermissionDao = new BatchTypePermissionDao(context);
                    // Get list group user id
                    using (Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }
                    // Get list group permission
                    var batchTypePermissions = batchTypePermissionDao.GetByUser(groupIds, dbBatch.BatchTypeId);
                    // Union permission
                    batchTypePermission = new BatchTypePermission();
                    foreach (var batchTypeItem in batchTypePermissions)
                    {
                        batchTypePermission.CanAccess |= batchTypeItem.CanAccess;
                    }
                }

                // Check permission
                if (!batchTypePermission.CanAccess)
                {
                    throw new CaptureException(CaptureError.NoPermission,
                        string.Format("Cannot access batch '{0}' of batch type '{1}'", batch.Id, batch.BatchTypeId));
                }
                #endregion

                Guid blockingBookmarkId;
                // Check valid batch
                #region
                if (dbBatch.IsCompleted)
                {
                    throw new CaptureException(CaptureError.InvalidDataBatch,
                        string.Format("Batch '{0}' is completed.", dbBatch.Id));
                }
                else if (dbBatch.IsProcessing)
                {
                    throw new CaptureException(CaptureError.InvalidDataBatch,
                        string.Format("Batch '{0}' is processing.", dbBatch.Id));
                }
                else if (dbBatch.HasError
                            || string.IsNullOrWhiteSpace(dbBatch.BlockingBookmark)
                            || Guid.TryParse(dbBatch.BlockingBookmark, out blockingBookmarkId))
                {
                    throw new CaptureException(CaptureError.InvalidDataBatch,
                        string.Format("Batch '{0}' is error.", dbBatch.Id));
                }
                else if (dbBatch.LockedBy != null
                            && !dbBatch.LockedBy.Equals(LoginUser.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new CaptureException(CaptureError.InvalidDataBatch,
                        string.Format("Batch '{0}' is locked by other.", dbBatch.Id));
                }
                else if (dbBatch.TransactionId != batch.TransactionId)
                {
                    throw new CaptureException(CaptureError.InvalidDataBatch,
                        string.Format("Batch '{0}' is invalid transaction.", dbBatch.Id));
                }
                #endregion

                BatchPermission batchPermission = null;
                // Get batch permission
                #region 
                if (LoginUser.IsAdmin)
                {
                    batchPermission = BatchPermission.GetAllowAll();
                }
                else
                {
                    batchPermission = GetWorkItemPermission(LoginUser.Id,
                                                        dbBatch.WorkflowDefinitionId,
                                                        blockingBookmarkId,
                                                        context);
                    if (batchPermission == null)
                    {
                        throw new CaptureException(CaptureError.NullPermmission);
                    }
                }
                #endregion

                var hasModify = false;


                var pageCount = 0;
                var dicDocFieldMeta = new Dictionary<Guid, List<DocumentFieldMetaData>>();
                var docDao = new DocumentDao(context);
                var docTypeDao = new DocumentTypeDao(context);
                var docMetaDao = new DocFieldMetaDataDao(context);
                var docValueDao = new DocumentFieldValueDao(context);
                var picklistDao = new PicklistDao(context);
                var tableDao = new TableFieldValueDao(context);
                var pageDao = new PageDao(context);
                var annoDao = new AnnotationDao(context);

                // Get list DB doc of batch
                var dbDocs = docDao.GetByBatch(dbBatch.Id);
                if (dbDocs == null || dbDocs.Count == 0)
                {
                    throw new CaptureException(CaptureError.NullDatabase,
                        string.Format("List content of batch '{0}'", dbBatch.Id));
                }

                var dicDocTypePermission = new Dictionary<Guid, DocumentTypePermission>();
                var docTypePermissionDao = new DocumentTypePermissionDao(context);
                DocumentTypePermission docTypePermission;

                // Delete document
                #region
                if (batch.DeletedDocuments != null && batch.DeletedDocuments.Count > 0)
                {
                    // Check delete permission
                    if (!batchPermission.CanModifyDocument)
                    {
                        throw new CaptureException(CaptureError.NoPermission,
                            string.Format("Cannot delete content of batch '{0}' with no permission modify content.",
                                           dbBatch.Id));
                    }

                    int deletedDocIndex;
                    Document dbDeletedDoc;

                    foreach (var deletedDocId in batch.DeletedDocuments)
                    {
                        #region
                        // Get deleted doc index
                        deletedDocIndex = dbDocs.FindIndex(h => h != null && h.Id == deletedDocId);
                        // Check valid delete doc id
                        if (deletedDocIndex == -1)
                        {
                            throw new CaptureException(CaptureError.InvalidBatchData,
                                string.Format("Invalid deleted content '{0}' of batch '{1}'",
                                                deletedDocId, dbBatch.Id));
                        }
                        // Get deleted doc
                        dbDeletedDoc = dbDocs[deletedDocIndex];

                        // Get doc type permission
                        #region
                        if (dicDocTypePermission.ContainsKey(dbDeletedDoc.DocTypeId))
                        {
                            docTypePermission = dicDocTypePermission[dbDeletedDoc.DocTypeId];
                        }
                        else
                        {
                            docTypePermission = GetDocTypePermission(dbDeletedDoc.DocTypeId, groupIds,
                                                                        docTypePermissionDao);
                            dicDocTypePermission.Add(dbDeletedDoc.DocTypeId, docTypePermission);
                        }
                        #endregion

                        if (!docTypePermission.CanAccess)
                        {
                            throw new CaptureException(
                                CaptureError.NoPermission,
                                string.Format(
                                    "Cannot delete content '{0}' of batch '{1}' with no permission access content.",
                                    dbBatch.Id));
                        }

                        // Delete doc from DB and it related item
                        docDao.DeleteRecur(deletedDocId);

                        // Remove from list db doc
                        dbDocs[deletedDocIndex] = null;
                        #endregion
                    }

                    // Remove
                    dbDocs = dbDocs.Where(h => h != null).ToList();
                    hasModify = true;
                }
                #endregion

                // Update document
                var docOrder = 0;
                if (batch.Documents == null && batch.Documents.Count == 0)
                {
                    throw new CaptureException(CaptureError.InvalidBatchData,
                        string.Format("Have not enough number of content of batch '{0}'", dbBatch.Id));
                }
                else
                {
                    #region
                    // Check count of submit doc
                    if (dbDocs.Count != batch.Documents.Count)
                    {
                        throw new CaptureException(CaptureError.InvalidBatchData,
                            string.Format("Have not enough number of content of batch '{0}'", dbBatch.Id));
                    }

                    // Get list docType
                    var docTypes = docTypeDao.GetDocumentTypeByBatch(dbBatch.BatchTypeId);
                    if (docTypes == null || docTypes.Count == 0)
                    {
                        throw new CaptureException(CaptureError.NullDatabase,
                             string.Format("Content type of batch '{0}' of batch type '{1}'",
                                            dbBatch.Id, dbBatch.BatchTypeId));
                    }

                    Document dbDoc;
                    foreach (var doc in batch.Documents)
                    {
                        // Get doc
                        dbDoc = dbDocs.FirstOrDefault(h => h.Id == doc.Id);
                        if (dbDoc == null)
                        {
                            throw new CaptureException(CaptureError.InvalidBatchData,
                                string.Format("Invalid updated content '{0}' of batch '{1}'", doc.Id, dbBatch.Id));
                        }

                        // Get document field meta data if not
                        #region
                        if (!dicDocFieldMeta.ContainsKey(doc.DocTypeId))
                        {
                            if (!docTypes.Any(h => h.Id == doc.DocTypeId))
                            {
                                throw new CaptureException(CaptureError.InvalidDataBatch,
                                    string.Format("Invalid content type '{0}' of batch '{1}'",
                                                    doc.DocTypeId, dbBatch.Id));
                            }

                            // Get and add list doc field mata
                            dicDocFieldMeta.Add(doc.DocTypeId,
                                                GetDocFieldMetaDatas(doc.DocTypeId, docMetaDao, picklistDao));
                        }
                        #endregion

                        // Update doc order
                        doc.Order = docOrder++;

                        // Update document

                    }
                    #endregion
                }
            }
        }






        private DocumentTypePermission GetDocTypePermission(
            Guid docTypeId, List<Guid> userGroupIds, DocumentTypePermissionDao docTypePermissionDao)
        {
            // Get list doc type from DB
            var docTypePermissions
                = docTypePermissionDao.GetByGroupRangeAndDocType(userGroupIds, docTypeId);

            var docTypePermission = new DocumentTypePermission();
            if (docTypePermissions != null && docTypePermissions.Count > 0)
            {
                // Union permission
                foreach (var itemDocTypePermission in docTypePermissions)
                {
                    docTypePermission.CanAccess |= itemDocTypePermission.CanAccess;
                }
            }

            return docTypePermission;
        }

        private ActivityPermission GetActivityPermission(
            List<Guid> groupIds, Guid workflowDefinitionId, Guid humanStepId, DapperContext context)
        {
            //var customActivityDao = new CustomActivitySettingDao(context);
            //CustomActivitySetting customActivity = customActivityDao.GetCustomActivitySetting(workflowDefinitionId, humanStepId);
            //ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);
            return null;

        }

        private List<DocumentFieldMetaData> GetDocFieldMetaDatas(
            Guid docTypeId, DocFieldMetaDataDao docMetaDao, PicklistDao picklistDao)
        {
            var docMetas = docMetaDao.GetByDocType(docTypeId);
            if (docMetas == null || docMetas.Count == 0)
            {
                throw new CaptureException(CaptureError.NullDatabase,
                                           string.Format("Doc field meta data of doc type '{0}'", docTypeId));
            }

            // Loop all doc meta to get additional information of data type pick-list and table
            // Also trim and lower data type of field meta
            foreach (var docMeta in docMetas)
            {
                docMeta.DataType = string.Format("{0}", docMeta.DataType).Trim().ToLower();

                // Case pick-list => get list values
                if (docMeta.DataType == "picklist")
                {
                    #region
                    var picklists = picklistDao.GetByField(docMeta.Id);
                    if (picklists == null || picklists.Count == 0)
                    {
                        throw new CaptureException(CaptureError.NullDatabase,
                            string.Format("Pick-list value field meta data '{0}' of doc type '{1}'",
                                           docMeta.Id, docTypeId));
                    }

                    foreach (var picklist in picklists)
                    {
                        picklist.Value = string.Format("{0}", picklist.Value).Trim();
                    }
                    docMeta.Picklists = picklists.Where(h => h.Value != string.Empty).ToList();
                    #endregion
                }
                // Case table => get children field
                else if (docMeta.DataType == "table")
                {
                    #region
                    var children = docMetaDao.GetChildren(docMeta.Id);
                    if (children == null || children.Count == 0)
                    {
                        throw new CaptureException(CaptureError.NullDatabase,
                            string.Format("Children field meta data '{0}' of doc type '{1}'",
                                           docMeta.Id, docTypeId));
                    }

                    foreach (var child in children)
                    {
                        child.DataType = string.Format("{0}", child.DataType).Trim().ToLower();
                    }
                    docMeta.Children = children;
                    #endregion
                }
            }

            return docMetas;
        }








        private bool AddComments(List<Comment> addComments, Guid batchId, DapperContext context)
        {
            // Check valid
            if (addComments == null || addComments.Count == 0)
            {
                return false;
            }
            if (batchId == Guid.Empty)
            {
                throw new ArgumentException(string.Format("Invalid {0} = {1}", nameof(batchId), batchId));
            }

            //Check null parameter
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var commentDao = new CommentDao(context);
            var now = DateTime.Now;
            var index = 0;
            var hasAddComment = false;
            foreach (var addComment in addComments)
            {
                if (addComment.Id != Guid.Empty)
                {
                    continue;
                }

                // Check null note
                var workingValue = string.Format("{0}", addComment.Note).Trim();
                if (workingValue == string.Empty)
                {
                    throw new CaptureException(CaptureError.InvalidDataBatch,
                                                GetError(nameof(addComment.Note), "NULL"));
                }

                // Set value
                addComment.Note = workingValue;
                addComment.CreatedBy = LoginUser.UserName;
                addComment.CreatedDate = now.AddSeconds(index++);
                addComment.IsBatchId = true;
                addComment.InstanceId = batchId;

                // Insert DB
                commentDao.InsertComment(addComment);
                hasAddComment = true;
            }

            return hasAddComment;
        }

        private void AddDocument(Document doc,
                                    Platform platform,
                                    List<DocumentFieldMetaData> docMetas,
                                    Guid batchTypeId,
                                    DocumentDao docDao,
                                    DocumentFieldValueDao docValueDao,
                                    PicklistDao picklistDao,
                                    TableFieldValueDao tableDao,
                                    PageDao pageDao,
                                    AnnotationDao annoDao
                                )
        {
            // Check valid
            if (doc.Pages == null || doc.Pages.Count == 0)
            {
                throw new CaptureException(CaptureError.InvalidDataDoc, "List page is null");
            }

            // Set value
            doc.CreatedDate = DateTime.Now;
            doc.CreatedBy = LoginUser.UserName;
            doc.ModifiedDate = null;
            doc.ModifiedBy = null;
            doc.IsRejected = false;
            doc.PageCount = doc.Pages.Count;

            // Get binary type and normalize file extension
            #region
            var workingBinaryType = BinaryType.None;
            Tuple<BinaryType, string> tupleBinaryType;
            foreach (var addPage in doc.Pages)
            {
                tupleBinaryType = GetBinaryType(addPage.FileExtension);

                workingBinaryType |= tupleBinaryType.Item1;
                addPage.FileExtension = tupleBinaryType.Item2;
            }
            // Set binary type
            if (workingBinaryType == BinaryType.Image)
            {
                doc.BinaryType = "Image";
            }
            else if (workingBinaryType == BinaryType.Media)
            {
                doc.BinaryType = "Media";
            }
            else
            {
                doc.BinaryType = "Compound";
            }
            #endregion

            // Insert to DB
            docDao.Add(doc);

            // Case not loose doc => Add doc index
            #region
            if (doc.DocTypeId != Guid.Empty)
            {
                if (doc.FieldValues == null)
                {
                    doc.FieldValues = new List<DocumentFieldValue>();
                }

                DocumentFieldValue docValue;
                foreach (var docMeta in docMetas)
                {
                    // Get doc field
                    docValue = doc.FieldValues.FirstOrDefault(h => h.FieldId == docMeta.Id);

                    // When client not supply doc field value => auto create this field with empty value
                    if (docValue == null)
                    {
                        docValue = new DocumentFieldValue();
                        docValue.Value = null;
                        docValue.DocId = doc.Id;
                        docValue.FieldId = docMeta.Id;
                    }

                    // Validate doc index
                    ValidateDocFieldValue(docValue, docMeta);

                    // Insert to DB
                    docValue.DocId = doc.Id;
                    docValueDao.Add(docValue);
                }
            }
            #endregion

            // Add page
            #region

            string prefixDocPath = null;
            if (_setting.IsSaveFileInFolder)
            {
                prefixDocPath = Path.Combine(batchTypeId.ToString(), doc.BatchId.ToString(), doc.Id.ToString());
            }

            var workingIndex = 0;
            foreach (var page in doc.Pages)
            {
                // Add related value
                page.DocId = doc.Id;
                page.PageNumber = workingIndex++;

                // AddPage(page, platform, doc.DocTypeId, prefixDocPath, pageDao, annoDao);
            }
            #endregion
        }

        private void UpdateDocument(Document doc, Document dbDoc,
                            Platform platform,
                            List<DocumentFieldMetaData> docMetas,
                            Guid batchTypeId,

                            UserGroupPermission userPermission,
                            AnnotationPermission annoPermission,
                            DocumentPermission docPermission,

                            DocumentDao docDao,
                            DocumentFieldValueDao docValueDao,
                            PicklistDao picklistDao,
                            TableFieldValueDao tableDao,
                            PageDao pageDao,
                            AnnotationDao annoDao,
                            TxFileManager fileMgr
                        )
        {













            // Get list DB page
            var dbPages = pageDao.GetByDoc(dbDoc.Id);
            if (dbPages == null || dbPages.Count == 0)
            {
                throw new CaptureException(CaptureError.NullDatabase,
                    string.Format("Cannot update content '{0}' which have no pages.", dbDoc.Id));
            }

            var hasModify = false;  // Flag determine doc have modify

            // Update index
            #region
            if (doc.FieldValues != null && doc.FieldValues.Count > 0 && userPermission.CanModifyIndexes)
            {
                // Case loose doc => have no index to update
                if (dbDoc.DocTypeId == Guid.Empty)
                {
                    throw new CaptureException(CaptureError.InvalidDataDoc,
                        string.Format("Cannot update index of loose content '{0}'.", dbDoc.Id));
                }

                var dbDocValues = docValueDao.GetByDoc(dbDoc.Id);
                if (dbDocValues == null || dbDocValues.Count == 0)
                {
                    throw new CaptureException(CaptureError.NullDatabase,
                        string.Format("Cannot update index content '{0}' which have no index values.", dbDoc.Id));
                }

                DocumentFieldValue dbDocValue;
                DocumentFieldMetaData docMeta;
                foreach (var docValue in doc.FieldValues)
                {
                    #region
                    // Get DB index
                    dbDocValue = dbDocValues.FirstOrDefault(h => h.Id == docValue.Id);
                    if (dbDocValue == null)
                    {
                        throw new CaptureException(CaptureError.NullDatabase,
                            string.Format("Cannot update index '{0}' which is not existed.", docValue.Id));
                    }

                    // Check different first time
                    if (docValue.Value == dbDocValue.Value)
                    {
                        continue;
                    }

                    // Check can write permission
                    if (!docPermission.FieldPermissions.Any(h => h.FieldId == dbDocValue.FieldId && h.CanWrite))
                    {
                        continue;
                    }

                    // Get meta of DB field
                    docMeta = docMetas.FirstOrDefault(h => h.Id == dbDocValue.FieldId);
                    if (docMeta == null)
                    {
                        throw new CaptureException(CaptureError.InvalidDataDoc,
                            string.Format("Cannot update index '{0}' which is not have field meta data.",
                                            docValue.Id));
                    }

                    // Validate doc index
                    ValidateDocFieldValue(docValue, docMeta);

                    // Check different second time
                    if (docValue.Value == dbDocValue.Value)
                    {
                        continue;
                    }

                    dbDocValue.Value = docValue.Value;
                    // Update DB
                    docValueDao.Update(dbDocValue);
                    hasModify = true;
                    #endregion
                }
            }
            #endregion

            // Delete page
            #region
            if (doc.DeletedPages != null && doc.DeletedPages.Count >= 0)
            {
                // Case no permission
                if (!userPermission.CanModifyDocument)
                {
                    throw new CaptureException(CaptureError.NoPermission,
                        string.Format("Cannot delete pages of content '{0}'.", dbDoc.Id));
                }

                int deletePageIdIndex;

                foreach (var deletePageId in doc.DeletedPages)
                {
                    deletePageIdIndex = dbPages.FindIndex(h => h != null && h.Id == deletePageId);
                    if (deletePageIdIndex == -1)
                    {
                        throw new CaptureException(CaptureError.NullDatabase,
                            string.Format("Cannot delete page '{0}' which is not existed.", deletePageId));
                    }

                    // Delete page;
                    DeletePage(dbPages[deletePageIdIndex].Id, dbPages[deletePageIdIndex].FilePath, pageDao, fileMgr);
                    // Remove this page from list DB pages
                    dbPages[deletePageIdIndex] = null;

                    hasModify = true;
                }
            }
            #endregion

            // Insert/Update page
            #region
            if (doc.Pages != null && doc.Pages.Count >= 0)
            {
                Page dbPage;

                foreach (var page in doc.Pages)
                {
                    // Case create new page
                    if (page.Id == Guid.Empty)
                    {
                        #region
                        // Case no permission
                        if (!userPermission.CanModifyDocument)
                        {
                            throw new CaptureException(CaptureError.NoPermission,
                                string.Format("Cannot add pages of updated content '{0}'.", dbDoc.Id));
                        }

                        AddPage(page, platform, doc.DocTypeId, userPermission, annoPermission, pageDao, annoDao);
                        hasModify = true;
                        #endregion
                    }
                    // Case update page
                    else
                    {
                        #region
                        dbPage = dbPages.FirstOrDefault(h => h != null && h.Id == page.Id);
                        if (dbPage == null)
                        {
                            throw new CaptureException(CaptureError.NullDatabase,
                                string.Format("Cannot update page '{0}' which is not existed.", page.Id));
                        }

                        hasModify |= UpdatePage(page, dbPage, platform, dbDoc.DocTypeId,
                                                userPermission, annoPermission, pageDao, annoDao);
                        #endregion
                    }
                }
            }



            #endregion
        }









        private void AddDocIndexes(List<DocumentFieldValue> docValues,
                                    List<DocumentFieldMetaData> docMetas,
                                    Guid docId,
                                    DocumentFieldValueDao docValueDao)
        {
            DocumentFieldValue docValue;
            foreach (var docMeta in docMetas)
            {
                // Get doc field
                docValue = docValues.FirstOrDefault(h => h.FieldId == docMeta.Id);

                // When client not supply doc field value => auto create this field with empty value
                if (docValue == null)
                {
                    docValue = new DocumentFieldValue();
                    docValue.Value = null;
                    docValue.DocId = docId;
                    docValue.FieldId = docMeta.Id;
                }

                // Validate doc file
                ValidateDocFieldValue(docValue, docMeta);

                // Insert to DB
                docValue.DocId = docId;
                docValueDao.Add(docValue);
            }
        }





        private void AddPage(Page page, Platform platform, Guid docTypeId,
                                UserGroupPermission userGroupPermission, AnnotationPermission annoPermission,
                                PageDao pageDao, AnnotationDao annoDao)
        {
            // Check negative value of page
            #region
            if (page.Height <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataPage, "Height = " + page.Height);
            }
            if (page.Width <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataPage, "Width = " + page.Width);
            }
            #endregion

            // Normalize and check the rotate angle to 0 or 90 or 270
            #region
            page.RotateAngle = page.RotateAngle % 360;
            if (page.RotateAngle < 0)
            {
                page.RotateAngle += 360;
            }
            if (page.RotateAngle != 0 && page.RotateAngle != 90 && page.RotateAngle != 270)
            {
                throw new CaptureException(CaptureError.InvalidDataPage, "RotateAngle = " + page.RotateAngle);
            }
            #endregion

            // Convert base 64 string to binary if in mobile
            #region
            if (platform == Platform.Mobile)
            {
                if (page.FileBinaryBase64 == null)
                {
                    throw new CaptureException(CaptureError.InvalidDataAnno, "FileBinaryBase64 is null");
                }

                page.FileBinary = Convert.FromBase64String(page.FileBinaryBase64);
                page.FileBinaryBase64 = null;
            }
            #endregion

            // Generate file hash
            page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);

            //Check setting save file in folder
            #region
            if (_setting.IsSaveFileInFolder)
            {
                page.FileBinary = null;
            }
            else
            {
                page.FilePath = null;
                page.FileHeader = null;
            }
            #endregion

            // Set other value
            page.OriginalFileName = string.IsNullOrWhiteSpace(page.OriginalFileName)
                                        ? null : page.OriginalFileName.Trim();
            page.Content = null;
            page.IsRejected = false;
            page.Height = Math.Ceiling(page.Height);
            page.Width = Math.Ceiling(page.Width);

            // Set default content language
            page.ContentLanguageCode = string.Format("{0}", page.ContentLanguageCode).Trim().ToLower();
            if (page.ContentLanguageCode == string.Empty)
            {
                page.ContentLanguageCode = "eng";
            }

            // Insert page to DB
            pageDao.Add(page);

            // Add annotation
            #region
            if (page.Annotations != null && page.Annotations.Count > 0)
            {
                foreach (var anno in page.Annotations)
                {
                    // Add related Id
                    anno.PageId = page.Id;
                    anno.DocId = page.DocId;
                    anno.DocTypeId = docTypeId;
                    anno.CreatedBy = page.CreatedBy;
                    anno.CreatedOn = page.CreatedDate;

                    // Insert annotation to DB
                    // AddAnno(anno, platform, annoDao);
                }
            }
            #endregion

        }

        private bool UpdatePage(Page page, Page dbPage, Platform platform, Guid docTypeId,
                                UserGroupPermission userGroupPermission, AnnotationPermission annoPermission,
                                PageDao pageDao, AnnotationDao annoDao)
        {
            var hasModify = false;

            // Check negative value of page
            #region
            if (page.Height <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataPage, "Height = " + page.Height);
            }
            if (page.Width <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataPage, "Width = " + page.Width);
            }
            #endregion

            // Normalize and check the rotate angle to 0 or 90 or 270
            #region
            var rotateAngle = page.RotateAngle % 360;
            if (rotateAngle < 0)
            {
                rotateAngle += 360;
            }
            if (rotateAngle != 0 && rotateAngle != 90 && rotateAngle != 270)
            {
                throw new CaptureException(CaptureError.InvalidDataPage, "RotateAngle = " + page.RotateAngle);
            }
            page.RotateAngle = rotateAngle;
            #endregion

            // Convert base 64 string to binary if in mobile
            #region
            if (platform == Platform.Mobile)
            {
                if (page.FileBinaryBase64 == null)
                {
                    throw new CaptureException(CaptureError.InvalidDataAnno, "FileBinaryBase64 is null");
                }

                page.FileBinary = Convert.FromBase64String(page.FileBinaryBase64);
                page.FileBinaryBase64 = null;
            }
            #endregion

            // Generate file hash
            page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);

            //Check setting save file in folder
            #region
            if (_setting.IsSaveFileInFolder)
            {
                page.FileBinary = null;
            }
            else
            {
                page.FilePath = null;
                page.FileHeader = null;
            }
            #endregion

            // Check permission rejected page
            if (page.IsRejected)
            {
                if (!userGroupPermission.CanReject)
                {
                    throw new CaptureException(CaptureError.NoPermission, "Reject page");
                }
            }

            // Set other value
            page.OriginalFileName = string.IsNullOrWhiteSpace(page.OriginalFileName)
                                    ? null : page.OriginalFileName.Trim();
            page.Content = null;
            page.Height = Math.Ceiling(page.Height);
            page.Width = Math.Ceiling(page.Width);

            // Set default content language
            page.ContentLanguageCode = string.Format("{0}", page.ContentLanguageCode).Trim().ToLower();
            if (page.ContentLanguageCode == string.Empty)
            {
                page.ContentLanguageCode = "eng";
            }

            // Check modify
            #region
            if (page.DocId != dbPage.DocId)
            {
                hasModify = true;
                dbPage.DocId = page.DocId;
            }
            if (page.PageNumber != dbPage.PageNumber)
            {
                hasModify = true;
                dbPage.PageNumber = page.PageNumber;
            }
            if (page.FileExtension != dbPage.FileExtension)
            {
                hasModify = true;
                dbPage.FileExtension = page.FileExtension;
            }
            if (page.FileBinary != dbPage.FileBinary)
            {
                hasModify = true;
                dbPage.FileBinary = page.FileBinary;
            }
            if (page.FileHeader != dbPage.FileHeader)
            {
                hasModify = true;
                dbPage.FileHeader = page.FileHeader;
            }
            if (page.FileHash != dbPage.FileHash)
            {
                hasModify = true;
                dbPage.FileHash = page.FileHash;
            }
            if (page.RotateAngle != dbPage.RotateAngle)
            {
                hasModify = true;
                dbPage.RotateAngle = page.RotateAngle;
            }
            if (page.Height != dbPage.Height)
            {
                hasModify = true;
                dbPage.Height = page.Height;
            }
            if (page.Width != dbPage.Width)
            {
                hasModify = true;
                dbPage.Width = page.Width;
            }
            if (page.IsRejected != dbPage.IsRejected)
            {
                hasModify = true;
                dbPage.IsRejected = page.IsRejected;
            }
            if (page.Content != dbPage.Content)
            {
                hasModify = true;
                dbPage.Content = page.Content;
            }
            if (page.ContentLanguageCode != dbPage.ContentLanguageCode)
            {
                hasModify = true;
                dbPage.ContentLanguageCode = page.ContentLanguageCode;
            }
            if (page.OriginalFileName != dbPage.OriginalFileName)
            {
                hasModify = true;
                dbPage.OriginalFileName = page.OriginalFileName;
            }
            #endregion

            // Update page to DB
            pageDao.Update(dbPage);

            // Get list DB annotation
            var dbAnnos = annoDao.GetByPage(dbPage.Id);

            // Delete annotation
            #region
            if (page.DeleteAnnotations != null && page.DeleteAnnotations.Count > 0 && userGroupPermission.CanAnnotate)
            {
                int deleteIndex;
                var deleteAnnoIds = new List<Guid>(page.DeleteAnnotations.Count);
                foreach (var deleteId in page.DeleteAnnotations)
                {
                    // Find index of deleted DB annotation
                    deleteIndex = dbAnnos.FindIndex(h => h != null && h.Id == deleteId);
                    if (deleteIndex == -1)
                    {
                        continue;
                    }

                    // Check delete permission
                    #region
                    if (dbAnnos[deleteIndex].Type == "Text")
                    {
                        if (!annoPermission.CanDeleteText)
                        {
                            throw new CaptureException(CaptureError.NoPermission,
                                                        "No permission to delete Text annotation");
                        }
                    }
                    else if (dbAnnos[deleteIndex].Type == "Highlight")
                    {
                        if (!annoPermission.CanDeleteHighlight)
                        {
                            throw new CaptureException(CaptureError.NoPermission,
                                                        "No permission to delete Highlight annotation");
                        }
                    }
                    else if (dbAnnos[deleteIndex].Type == "Redaction")
                    {
                        if (!annoPermission.CanDeleteRedaction)
                        {
                            throw new CaptureException(CaptureError.NoPermission,
                                                        "No permission to delete Text Redaction");
                        }
                    }
                    else
                    {
                        throw new CaptureException(CaptureError.InvalidDataAnno,
                                                    "Delete Type = " + dbAnnos[deleteIndex].Type);
                    }
                    #endregion

                    // Add to real delete list
                    deleteAnnoIds.Add(dbAnnos[deleteIndex].Id);
                    // Remove this annotation from list DB
                    dbAnnos[deleteIndex] = null;
                }

                // Get list deleted DB annotation
                if (deleteAnnoIds.Count != 0)
                {
                    // Delete from DB
                    annoDao.Delete(deleteAnnoIds);
                }
            }
            #endregion

            // Add/Update annotation
            #region
            if (page.Annotations != null && page.Annotations.Count > 0 && userGroupPermission.CanAnnotate)
            {
                // Update annotation first
                #region
                Annotation dbAnno;
                foreach (var anno in page.Annotations)
                {
                    // Case add new annotation is doing below => just skip in here
                    if (anno.Id == Guid.Empty)
                    {
                        continue;
                    }

                    // Get updated DB annotation
                    dbAnno = dbAnnos.FirstOrDefault(h => h != null && h.Id == anno.Id);
                    if (dbAnno == null)
                    {
                        continue;
                    }

                    // Update
                    hasModify |= UpdateAnno(anno, dbAnno, platform, annoDao);
                }
                #endregion

                // Add annotation second
                #region
                foreach (var anno in page.Annotations)
                {
                    // Case update annotation is doing above => just skip in here
                    if (anno.Id != Guid.Empty)
                    {
                        continue;
                    }

                    // Set related value
                    anno.PageId = page.Id;
                    anno.DocId = page.DocId;
                    anno.DocTypeId = docTypeId;

                    // Add
                    AddAnno(anno, platform, annoPermission, annoDao);
                    hasModify = true;
                }
                #endregion
            }
            #endregion

            return hasModify;
        }

        private void DeletePage(Guid id, string filePath, PageDao pageDao, TxFileManager fileMgr)
        {
            // Delete file if save in folder
            if (_setting.IsSaveFileInFolder)
            {
                fileMgr.Delete(filePath);
            }

            // Delete page and its anno
            pageDao.DeleteRecur(id);
        }

        private void AddAnno(Annotation anno, Platform platform, AnnotationPermission annoPermission,
                                AnnotationDao annoDao)
        {
            // Check negative value of annotation
            #region
            if (anno.Height <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Height = " + anno.Height);
            }
            if (anno.Width <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Width = " + anno.Width);
            }
            if (anno.Left < 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Left = " + anno.Left);
            }
            if (anno.Top < 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Top = " + anno.Top);
            }
            #endregion

            // Normalize and check the rotate angle to 0 or 90 or 270
            #region
            var rotateAngle = anno.RotateAngle % 360;
            if (rotateAngle < 0)
            {
                rotateAngle += 360;
            }
            if (rotateAngle != 0 && rotateAngle != 90 && rotateAngle != 270)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "RotateAngle = " + anno.RotateAngle);
            }
            anno.RotateAngle = rotateAngle;
            #endregion

            // Normalize and check anno type and content
            #region
            anno.Type = string.Format("{0}", anno.Type).Trim().ToLower();
            anno.Content = string.Format("{0}", anno.Content).Trim();

            switch (anno.Type)
            {
                case "text":
                    #region
                    if (!annoPermission.CanAddText)
                    {
                        throw new CaptureException(CaptureError.NoPermission,
                                                    "No permission to add Text annotation");
                    }
                    else if (anno.Content == string.Empty)
                    {
                        throw new CaptureException(CaptureError.InvalidDataAnno, "Text annotation's content is null");
                    }
                    if (platform == Platform.Mobile)
                    {
                        anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                    }
                    anno.Type = "Text";
                    break;
                #endregion

                case "highlight":
                    #region
                    if (!annoPermission.CanAddHighlight)
                    {
                        throw new CaptureException(CaptureError.NoPermission,
                                                    "No permission to add Highlight annotation");
                    }
                    anno.Type = "Highlight";
                    anno.Content = null;
                    break;
                #endregion

                case "redaction":
                    #region
                    if (!annoPermission.CanAddRedaction)
                    {
                        throw new CaptureException(CaptureError.NoPermission,
                                                    "No permission to add Redaction annotation");
                    }
                    anno.Type = "Redaction";
                    anno.Content = null;
                    break;
                #endregion

                default:
                    throw new CaptureException(CaptureError.InvalidDataAnno, "Type = " + anno.Type);
            }
            #endregion

            // Set value data anno
            anno.CreatedBy = LoginUser.UserName;
            anno.CreatedOn = DateTime.Now;
            anno.ModifiedBy = null;
            anno.ModifiedOn = null;

            // Round ceiling
            anno.Width = Math.Ceiling(anno.Width);
            anno.Height = Math.Ceiling(anno.Height);
            anno.Left = Math.Ceiling(anno.Left);
            anno.Top = Math.Ceiling(anno.Top);

            // Most of case have these value. (No see other value until now)
            anno.LineEndAt = "TopLeft";
            anno.LineStartAt = "TopLeft";
            anno.LineStyle = "ArrowAtEnd";

            // Temp default value for these
            anno.LineWeight = 0;
            anno.LineColor = null;

            // Insert to DB
            annoDao.Add(anno);
        }

        private bool UpdateAnno(Annotation anno, Annotation dbAnno, Platform platform, AnnotationDao annoDao)
        {
            // Check negative value of annotation
            #region
            if (anno.Height <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Height = " + anno.Height);
            }
            if (anno.Width <= 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Width = " + anno.Width);
            }
            if (anno.Left < 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Left = " + anno.Left);
            }
            if (anno.Top < 0)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "Top = " + anno.Top);
            }
            #endregion

            // Normalize and check the rotate angle to 0 or 90 or 270
            #region
            var rotateAngle = anno.RotateAngle % 360;
            if (rotateAngle < 0)
            {
                rotateAngle += 360;
            }
            if (rotateAngle != 0 && rotateAngle != 90 && rotateAngle != 270)
            {
                throw new CaptureException(CaptureError.InvalidDataAnno, "RotateAngle = " + anno.RotateAngle);
            }
            anno.RotateAngle = rotateAngle;
            #endregion

            // Normalize and check annotation type and content
            #region
            anno.Type = string.Format("{0}", anno.Type).Trim().ToLower();
            anno.Content = string.Format("{0}", anno.Content).Trim();

            switch (anno.Type)
            {
                case "text":
                    if (anno.Content == string.Empty)
                    {
                        throw new CaptureException(CaptureError.InvalidDataAnno, "Text annotation's content is null");
                    }
                    if (platform == Platform.Mobile)
                    {
                        anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                    }
                    break;

                case "highlight":
                    anno.Content = null;
                    break;

                case "redaction":
                    anno.Content = null;
                    break;

                default:
                    throw new CaptureException(CaptureError.InvalidDataAnno, "Type = " + anno.Type);
            }
            #endregion

            // Round to ceiling
            anno.Width = Math.Ceiling(anno.Width);
            anno.Height = Math.Ceiling(anno.Height);
            anno.Left = Math.Ceiling(anno.Left);
            anno.Top = Math.Ceiling(anno.Top);

            var hasModify = false;

            // Check modify
            #region
            if (anno.Height != dbAnno.Height)
            {
                hasModify = true;
                dbAnno.Height = anno.Height;
            }
            if (anno.Width != dbAnno.Width)
            {
                hasModify = true;
                dbAnno.Width = anno.Width;
            }
            if (anno.Left != dbAnno.Left)
            {
                hasModify = true;
                dbAnno.Left = anno.Left;
            }
            if (anno.Top != dbAnno.Top)
            {
                hasModify = true;
                dbAnno.Top = anno.Top;
            }
            if (anno.RotateAngle != dbAnno.RotateAngle)
            {
                hasModify = true;
                dbAnno.RotateAngle = anno.RotateAngle;
            }
            if (anno.Content != dbAnno.Content)
            {
                hasModify = true;
                dbAnno.Content = anno.Content;
            }
            #endregion

            // Just update if have real different value
            if (hasModify)
            {
                // Set value data annotation
                dbAnno.ModifiedBy = LoginUser.UserName;
                dbAnno.ModifiedOn = DateTime.Now;

                // Insert to DB
                annoDao.Update(dbAnno);
            }

            return hasModify;
        }





        private string GetError(string name, object value)
        {
            return string.Format("{0} = {1}", name, value);
        }

        private void ValidateBatchFieldValue(BatchFieldValue batchValue, BatchFieldMetaData batchMeta)
        {
            if (string.IsNullOrWhiteSpace(batchValue.Value))
            {
                batchValue.Value = null;
            }
            else
            {
                // Check valid value for data type
                #region
                var value = batchValue.Value.Trim();
                var dataType = string.Format("{0}", batchMeta.DataType).Trim().ToLower();

                switch (dataType)
                {
                    case "string":
                        #region
                        if (value.Length > batchMeta.MaxLength)
                        {
                            throw new CaptureException(CaptureError.InvalidDataDocField,
                                                        GetErrorMaxLength(batchMeta, value));
                        }
                        batchValue.Value = value;
                        return;
                    #endregion

                    case "integer":
                        #region
                        int tempInt;
                        if (int.TryParse(value, out tempInt))
                        {
                            batchValue.Value = tempInt.ToString();
                            return;
                        }
                        break;
                    #endregion

                    case "decimal":
                        #region
                        decimal tempDecimal;
                        if (decimal.TryParse(value, out tempDecimal))
                        {
                            batchValue.Value = tempDecimal.ToString();
                            return;
                        }
                        break;
                    #endregion

                    case "date":
                        #region
                        DateTime tempDateTime;
                        if (DateTime.TryParseExact(value.PadRight(10).Substring(0, 10),
                                                    "yyyy-MM-dd", null, DateTimeStyles.None, out tempDateTime))
                        {
                            batchValue.Value = tempDateTime.ToString("yyyy-MM-dd");
                            return;
                        }
                        break;
                    #endregion

                    case "boolean":
                        #region
                        bool tempBool;
                        if (bool.TryParse(value, out tempBool))
                        {
                            batchValue.Value = tempBool.ToString();
                            return;
                        }
                        break;
                    #endregion

                    default:
                        throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorInvalidDataType(batchMeta));
                }

                // Throw invalid value for data type
                throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorInvalidValue(batchMeta, value));
                #endregion
            }
        }

        private const string ERROR_FORMAT_REQUIRED_VALUE = "Field '{0}' - '{1}' is required.";
        private const string ERROR_FORMAT_MAX_LENGTH_VALUE
            = "Field '{0}' - '{1}' have value '{2}' is exceed max length {3}";
        private const string ERROR_FORMAT_INVALID_VALUE
            = "Field '{0}' - '{1}' have invalid value '{2}' with type '{3}'";
        private const string ERROR_FORMAT_INVALID_DATA_TYPE = "Field '{0}' - '{1}' have invalid type '{2}'";
        private const string ERROR_ANNO_TEXT_REQUIRED = "Text annotation required content";
        private const string ERROR_ANNO_INVALID_TYPE = "Invalid annotation type.";

        private string GetErrorRequiredValue(DocumentFieldMetaData meta)
        {
            return string.Format(ERROR_FORMAT_REQUIRED_VALUE, meta.Id, meta.Name);
        }
        private string GetErrorMaxLength(DocumentFieldMetaData meta, string value)
        {
            return string.Format(ERROR_FORMAT_MAX_LENGTH_VALUE, meta.Id, meta.Name, value, meta.MaxLength);
        }
        private string GetErrorInvalidValue(DocumentFieldMetaData meta, string value)
        {
            return string.Format(ERROR_FORMAT_INVALID_VALUE, meta.Id, meta.Name, value, meta.DataType);
        }
        private string GetErrorInvalidDataType(DocumentFieldMetaData meta)
        {
            return string.Format(ERROR_FORMAT_INVALID_DATA_TYPE, meta.Id, meta.Name, meta.DataType);
        }
        private string GetErrorMaxLength(BatchFieldMetaData meta, string value)
        {
            return string.Format(ERROR_FORMAT_MAX_LENGTH_VALUE, meta.Id, meta.Name, value, meta.MaxLength);
        }
        private string GetErrorInvalidValue(BatchFieldMetaData meta, string value)
        {
            return string.Format(ERROR_FORMAT_INVALID_VALUE, meta.Id, meta.Name, value, meta.DataType);
        }
        private string GetErrorInvalidDataType(BatchFieldMetaData meta)
        {
            return string.Format(ERROR_FORMAT_INVALID_DATA_TYPE, meta.Id, meta.Name, meta.DataType);
        }


        /// <summary>
        /// Validate value for document field.
        /// </summary>
        /// <param name="docValue">Doc field value is need to validate.</param>
        /// <param name="docMeta">Document field meta data. The data type need to be in lower case.</param>
        /// <returns>Validated and normalized value for data type, or null.</returns>
        /// <exception cref="CaptureException">Throw when
        /// <para>- Missing required value.</para>
        /// <para>- Length of value type string is greater than max length.</para>
        /// <para>- Invalid value for data type.</para>
        /// </exception>
        private void ValidateDocFieldValue(DocumentFieldValue docValue, DocumentFieldMetaData docMeta)
        {
            if ("table" == docMeta.DataType)
            {
                #region
                #region Check required field
                if (docValue.TableFieldValue == null || docValue.TableFieldValue.Count == 0)
                {
                    if (docMeta.IsRequired)
                    {
                        throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorRequiredValue(docMeta));
                    }

                    docValue.Value = null;
                    docValue.TableFieldValue = null;
                    return;
                }
                #endregion

                // Declare variable
                #region
                // Grouping table view by row
                var rowIndex = 0;
                var tempTableValues = new List<TableFieldValue>();
                var rowValues = new List<TableFieldValue>(docMeta.Children.Count);
                bool rowHaveValue;
                string value;
                TableFieldValue tableValue;
                var groupRows = docValue.TableFieldValue.GroupBy(h => h.RowNumber).OrderBy(h => h.Key).ToList();
                #endregion

                foreach (var row in groupRows)
                {
                    // Reset flag have at least one field value in row
                    rowHaveValue = false;

                    foreach (var childMeta in docMeta.Children)
                    {
                        #region
                        // Get column value
                        tableValue = row.FirstOrDefault(h => h.FieldId == childMeta.Id);
                        // Create new if null
                        if (tableValue == null)
                        {
                            tableValue = new TableFieldValue();
                            tableValue.FieldId = childMeta.Id;
                        }
                        // Set related value
                        tableValue.DocId = docValue.DocId;
                        tableValue.RowNumber = rowIndex;

                        // Check required table value
                        value = string.Format("{0}", tableValue.Value).Trim();
                        if (value == string.Empty)
                        {
                            tableValue.Value = null;
                            rowValues.Add(tableValue);
                            continue;
                        }

                        // Data type is in lower case
                        #region
                        switch (childMeta.DataType)
                        {
                            case "string":
                                #region
                                if (value.Length > childMeta.MaxLength)
                                {
                                    throw new CaptureException(CaptureError.InvalidDataDocField,
                                                                GetErrorInvalidValue(childMeta, value));
                                }

                                tableValue.Value = value;
                                break;
                            #endregion

                            case "integer":
                                #region
                                int tempInt;
                                if (int.TryParse(value, out tempInt))
                                {
                                    tableValue.Value = tempInt.ToString();
                                }
                                else
                                {
                                    throw new CaptureException(CaptureError.InvalidDataDocField,
                                                                GetErrorInvalidValue(childMeta, value));
                                }

                                break;
                            #endregion

                            case "decimal":
                                #region
                                decimal tempDecimal;
                                if (decimal.TryParse(value, out tempDecimal))
                                {
                                    tableValue.Value = tempDecimal.ToString();
                                }
                                else
                                {
                                    throw new CaptureException(CaptureError.InvalidDataDocField,
                                                                GetErrorInvalidValue(childMeta, value));
                                }
                                break;
                            #endregion

                            case "date":
                                #region
                                DateTime tempDate;
                                if (DateTime.TryParseExact(value.PadRight(10).Substring(0, 10),
                                                            "yyyy-MM-dd", null, DateTimeStyles.None, out tempDate))
                                {
                                    tableValue.Value = tempDate.ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    throw new CaptureException(CaptureError.InvalidDataDocField,
                                                                GetErrorInvalidValue(childMeta, value));
                                }
                                break;
                            #endregion

                            default:
                                throw new CaptureException(CaptureError.InvalidDataDocField,
                                                            GetErrorInvalidDataType(childMeta));
                        }
                        #endregion

                        rowValues.Add(tableValue);
                        rowHaveValue = true;
                        #endregion
                    }

                    if (rowHaveValue)
                    {
                        tempTableValues.AddRange(rowValues);
                        rowIndex++;
                    }
                    rowValues.Clear();
                }

                // Check required again after normalize table value
                if (tempTableValues.Count == 0 && docMeta.IsRequired)
                {
                    throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorRequiredValue(docMeta));
                }

                docValue.Value = null;
                docValue.TableFieldValue = tempTableValues;
                #endregion
            }
            else
            {
                #region
                // Check value empty and required field
                if (string.IsNullOrWhiteSpace(docValue.Value))
                {
                    if (docMeta.IsRequired)
                    {
                        throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorRequiredValue(docMeta));
                    }

                    docValue.Value = null;
                    return;
                }

                var value = docValue.Value.Trim();
                switch (docMeta.DataType)
                {
                    case "string":
                        #region
                        if (value.Length > docMeta.MaxLength)
                        {
                            throw new CaptureException(CaptureError.InvalidDataDocField,
                                                        GetErrorMaxLength(docMeta, value));
                        }
                        docValue.Value = value;
                        return;
                    #endregion

                    case "integer":
                        #region
                        int tempInt;
                        if (int.TryParse(value, out tempInt))
                        {
                            docValue.Value = tempInt.ToString();
                            return;
                        }
                        break;
                    #endregion

                    case "decimal":
                        #region
                        decimal tempDecimal;
                        if (decimal.TryParse(value, out tempDecimal))
                        {
                            docValue.Value = tempDecimal.ToString();
                            return;
                        }
                        break;
                    #endregion

                    case "date":
                        #region
                        DateTime tempDateTime;
                        if (DateTime.TryParseExact(value.PadRight(10).Substring(0, 10),
                                                    "yyyy-MM-dd", null, DateTimeStyles.None, out tempDateTime))
                        {
                            docValue.Value = tempDateTime.ToString("yyyy-MM-dd");
                            return;
                        }
                        break;
                    #endregion

                    case "boolean":
                        #region
                        bool tempBool;
                        if (bool.TryParse(value, out tempBool))
                        {
                            docValue.Value = tempBool.ToString();
                            return;
                        }
                        break;
                    #endregion

                    case "picklist":
                        #region
                        if (docMeta.Picklists.Any(h => h.Value == value))
                        {
                            docValue.Value = value;
                            return;
                        }
                        break;
                    #endregion

                    default:
                        throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorInvalidDataType(docMeta));
                }

                // Throw invalid value for data type
                throw new CaptureException(CaptureError.InvalidDataDocField, GetErrorInvalidValue(docMeta, value));
                #endregion
            }
        }

        private Tuple<BinaryType, string> GetBinaryType(string extension)
        {
            // Check null
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentNullException(nameof(extension));
            }

            // Normalize and remove '.'character
            extension = extension.Trim().ToLower();
            if (extension[0] == '.')
            {
                extension = extension.Substring(1);
            }

            // Check null again
            if (extension == string.Empty)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extensionImage.Contains(extension))
            {
                return new Tuple<BinaryType, string>(BinaryType.Image, extension);
            }
            else if (extensionMedia.Contains(extension))
            {
                return new Tuple<BinaryType, string>(BinaryType.Media, extension);
            }

            return new Tuple<BinaryType, string>(BinaryType.Native, extension);
        }

        private string[] extensionImage = new string[] { "aiff", "asf", "au", "avi", "dvr-ms", "m1v", "mid",
                                                          "midi", "mp3", "mp4", "mpe", "mpeg",
                                                          "mpg", "rmi", "vob", "wav", "wm", "wma",
                                                          "wmv", "dat", "flv",
                                                          "m4v", "mov", "3gp", "3g2", "m2v"};
        private string[] extensionMedia = new string[] { "tif", "tiff", "png", "gif", "bmp", "jpg", "jpeg" };
    }

    public class DaoCollection
    {
        public BatchDao BatchDao { get; }
        public BatchFieldValueDao BatchFieldDao { get; }
        public BatchFieldMetaDataDao BatchMetaDao { get; }
        public BatchTypeDao BatchTypeDao { get; }
        public CommentDao CommentDao { get; }

        public DocumentDao DocDao { get; }
        public DocumentFieldValueDao DocFieldDao { get; }
        public DocFieldMetaDataDao DocMetaDao { get; }
        public TableFieldValueDao TableFieldDao { get; }
        public PicklistDao PicklistDao { get; }

        public PageDao PageDao { get; }
        public AnnotationDao AnnoDao { get; }

        public DaoCollection(DapperContext context)
        {
            DocDao = new DocumentDao(context);
            DocFieldDao = new DocumentFieldValueDao(context);
            DocMetaDao = new DocFieldMetaDataDao(context);
            TableFieldDao = new TableFieldValueDao(context);
            PicklistDao = new PicklistDao(context);

            PageDao = new PageDao(context);
            AnnoDao = new AnnotationDao(context);
        }
    }

    public enum Platform
    {
        None,
        Wpf,
        Mobile,
        Mvc
    }

    [Flags]
    public enum BinaryType
    {
        None = 0,
        Image = 1,
        Media = 2,
        Native = 4
    }
}
