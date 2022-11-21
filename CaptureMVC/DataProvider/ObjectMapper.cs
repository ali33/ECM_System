using CaptureMVC.Models;
using CaptureMVC.Utility;
using Ecm.CaptureDomain;
using Ecm.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace CaptureMVC.DataProvider
{
    public class ObjectMapper
    {
        #region Public method

        #region Search controller

        public static IList<SavedQueryModel> GetSavedQueryModels(IList<SearchQuery> items)
        {
            return items.Select(h => new SavedQueryModel() { Id = h.Id, Name = h.Name }).ToList();
        }

        public static List<ConditionModel> GetCondtionModels(IEnumerable<BatchFieldMetaData> items)
        {
            return items.Select(h => new ConditionModel()
            {
                FieldId = h.Id,
                FieldName = h.Name,
                FieldDisplay = h.IsSystemField ? FieldUtils.MapSystemFieldNameDisplay(h.Name)
                                               : h.Name,
                FieldDataType = h.DataTypeEnum,
                IsSystemField = h.IsSystemField
            }).ToList();
        }

        public static List<ConditionModel> GetCondtionModels(IEnumerable<SearchQueryExpression> items)
        {
            return items.Select(h => new ConditionModel()
            {
                Id = h.Id,
                Conjunction = h.Condition == null ? SearchConjunction.And
                                                  : GeneratorJson.GetSearhConjunction(h.Condition),
                FieldId = h.FieldId,
                FieldName = h.FieldMetaData.Name,
                FieldDisplay = h.FieldMetaData
                                .IsSystemField ? FieldUtils.MapSystemFieldNameDisplay(h.FieldMetaData.Name)
                                               : h.FieldMetaData.Name,
                FieldDataType = h.FieldMetaData.DataTypeEnum,
                SearchOperator = h.OperatorEnum,
                Value1 = h.Value1,
                Value2 = h.Value2
            }).ToList();
        }

        public static SearchQuery GetSearchQuery(Guid queryId,
                                                 string queryName,
                                                 Guid batchTypeId,
                                                 IEnumerable<SearchQueryExpression> searchQueryExpressions)
        {
            var query = new SearchQuery()
            {
                Id = queryId,
                Name = queryName,
                BatchTypeId = batchTypeId,
                UserId = Utilities.UserId,
                SearchQueryExpressions = searchQueryExpressions.ToList()
            };

            var none = SearchConjunction.None.ToString().ToUpper();
            foreach (var item in query.SearchQueryExpressions)
            {
                // Set query id
                item.SearchQueryId = queryId;

                // Adjust condition to upper case
                item.Condition = item.Condition.ToUpper();

                // Adjust operator
                item.Operator = Enum.Parse(typeof(SearchOperator), item.Operator, true).ToString();

                // Adjust value1
                if (string.IsNullOrWhiteSpace(item.Value1))
                {
                    item.Value1 = null;
                }
                else
                {
                    item.Value1 = item.Value1.Trim();
                }

                // Adjust value2
                if (string.IsNullOrWhiteSpace(item.Value2))
                {
                    item.Value2 = null;
                }
                else
                {
                    item.Value2 = item.Value2.Trim();
                }
            }

            return query;
        }

        public static SearchQuery GetSearchQuery(Guid batchTypeId,
                                                 List<SearchQueryExpression> searchQueryExpressions)
        {
            var query = new SearchQuery();
            query.BatchTypeId = batchTypeId;

            if (null == searchQueryExpressions)
            {
                query.SearchQueryExpressions = new List<SearchQueryExpression>();
            }
            else
            {
                query.SearchQueryExpressions = searchQueryExpressions;
            }

            return query;
        }

        public static SearchResultModel GetSearchResultModel(WorkItemSearchResult source)
        {
            var result = new SearchResultModel();
            var table = new DataTable();

            result.BatchTypeName = source.BatchType.Name;
            result.TotalCount = source.TotalCount;
            result.ResultCount = source.WorkItems.Count;
            result.HasMoreResult = source.HasMoreResult;
            result.DataResult = table;
            result.PageIndex = source.PageIndex;

            int indexActivityName;
            // Populate column and row
            PopulateColumnsForSearchResultModel(table, source, out indexActivityName);
            PopulateRowsForSearchResultModel(table, source);

            result.IndexActivityName = indexActivityName;
            return result;
        }

        #endregion

        #region View controller

        public static ViewBatchModel GetViewBatchModel(Batch batch)
        {
            var model = new ViewBatchModel();

            #region Map information of batch

            model.BatchName = batch.BatchName;
            model.BatchPermission = batch.BatchPermission;
            model.BatchType = batch.BatchType;
            model.BatchTypeId = batch.BatchTypeId;
            model.BlockingActivityDescription = batch.BlockingActivityDescription;
            model.BlockingActivityName = batch.BlockingActivityName;
            model.BlockingBookmark = batch.BlockingBookmark;
            model.BlockingDate = batch.BlockingDate;
            model.Comments = batch.Comments;
            model.CreatedBy = batch.CreatedBy;
            model.DelegatedBy = batch.DelegatedBy;
            model.DelegatedTo = batch.DelegatedTo;
            model.DeletedDocuments = batch.DeletedDocuments;
            model.DeletedLooseDocuments = batch.DeletedLooseDocuments;
            model.DocCount = batch.DocCount;
            model.FieldValues = batch.FieldValues;
            model.HasError = batch.HasError;
            model.Id = batch.Id;
            model.IsCompleted = batch.IsCompleted;
            model.IsProcessing = batch.IsProcessing;
            model.IsRejected = batch.IsRejected;
            model.LastAccessedBy = batch.LastAccessedBy;
            model.LastAccessedDate = batch.LastAccessedDate;
            model.LockedBy = batch.LockedBy;
            model.ModifiedBy = batch.ModifiedBy;
            model.ModifiedDate = batch.ModifiedDate;
            model.PageCount = batch.PageCount;
            model.CreatedDate = batch.CreatedDate;
            model.StatusMsg = batch.StatusMsg;
            model.WorkflowDefinitionId = batch.WorkflowDefinitionId;
            model.WorkflowInstanceId = batch.WorkflowInstanceId;
            #endregion

            model.IsLoaded = true;
            // Map information of document
            model.Documents = new List<ViewDocumentModel>();
            // Add temp doc for insert new page
            model.Documents.Add(new ViewDocumentModel()
            {
                Id = Guid.NewGuid(),
                Pages = new List<ViewPageModel>(),
                DocKind = Constant.DOC_KIND_TEMP
            });

            var flgRejectBatch = false;
            var totalPageCount = 0;
            var docCount = 0;
            foreach (var doc in batch.Documents)
            {
                var docModel = new ViewDocumentModel();
                #region Doc information
                // Map information of document
                docModel.AnnotationPermission = doc.AnnotationPermission;
                docModel.BatchId = doc.BatchId;
                docModel.BinaryType = doc.BinaryType;
                docModel.CreatedBy = doc.CreatedBy;
                docModel.CreatedDate = doc.CreatedDate;
                docModel.DeletedPages = doc.DeletedPages;
                docModel.DocName = doc.DocName;
                docModel.DocTypeId = doc.DocTypeId;
                docModel.DocumentType = doc.DocumentType;
                docModel.EmbeddedPictures = doc.EmbeddedPictures;
                docModel.FieldValues = doc.FieldValues;
                docModel.Id = doc.Id;
                docModel.IsRejected = doc.IsRejected;
                docModel.IsUndefinedType = doc.IsUndefinedType;
                docModel.ModifiedBy = doc.ModifiedBy;
                docModel.ModifiedDate = doc.ModifiedDate;
                docModel.PageCount = doc.PageCount;
                #endregion

                #region Doc permission
                // Set permission for highlight
                docModel.CanSeeHighlight = batch.BatchPermission.CanAnnotate &
                                           doc.AnnotationPermission.CanSeeHighlight;
                docModel.CanAddHighlight = batch.BatchPermission.CanModifyDocument &
                                           doc.AnnotationPermission.CanAddHighlight;
                docModel.CanDeleteHighlight = batch.BatchPermission.CanModifyDocument &
                                              doc.AnnotationPermission.CanDeleteHighlight;

                // Set permission for redaction
                docModel.CanHideRedaction = batch.BatchPermission.CanAnnotate &
                                            doc.AnnotationPermission.CanHideRedaction;
                docModel.CanAddRedaction = batch.BatchPermission.CanModifyDocument &
                                           doc.AnnotationPermission.CanAddRedaction;
                docModel.CanDeleteRedaction = batch.BatchPermission.CanAnnotate &
                                            doc.AnnotationPermission.CanHideRedaction &
                                            doc.AnnotationPermission.CanDeleteRedaction;

                // Set permission for text
                docModel.CanSeeText = batch.BatchPermission.CanAnnotate &
                                           doc.AnnotationPermission.CanSeeText;
                docModel.CanAddText = batch.BatchPermission.CanModifyDocument &
                                      doc.AnnotationPermission.CanAddText;
                docModel.CanDeleteText = batch.BatchPermission.CanModifyDocument &
                                         doc.AnnotationPermission.CanDeleteText;
                #endregion

                var flgRejectDoc = false;
                docCount++;
                var pageCount = 0;
                docModel.Pages = new List<ViewPageModel>();
                foreach (var page in doc.Pages)
                {
                    // Add new page model to document model
                    var pageModel = new ViewPageModel();
                    docModel.Pages.Add(pageModel);

                    flgRejectDoc |= page.IsRejected;
                    pageCount++;

                    // Map information of page
                    pageModel.OriginPage = page;
                    page.RotateAngle = Utilities.GetRotateAngle((int)page.RotateAngle);
                    page.DeleteAnnotations = new List<Guid>();
                    pageModel.ContentType = MimeMap.ContentTypeFromExtension(page.FileExtension);

                    #region Map information of annotation
                    var tempAnnotations = new List<Annotation>();
                    foreach (var anno in page.Annotations)
                    {
                        switch (anno.Type)
                        {
                            case Constant.ANNO_TYPE_HIGHLIGHT:
                                anno.RotateAngle = 0;
                                if (docModel.CanSeeHighlight)
                                {
                                    tempAnnotations.Add(anno);
                                }
                                else
                                {
                                    pageModel.NotSeeAnnotations.Add(anno);
                                }

                                break;

                            case Constant.ANNO_TYPE_TEXT:
                                anno.RotateAngle = Utilities.GetRotateAngle((int)anno.RotateAngle);
                                if (docModel.CanSeeText)
                                {
                                    tempAnnotations.Add(anno);
                                }
                                else
                                {
                                    pageModel.NotSeeAnnotations.Add(anno);
                                }
                                break;

                            case Constant.ANNO_TYPE_REDACTION:
                                anno.RotateAngle = 0;
                                if (docModel.CanHideRedaction && docModel.CanDeleteRedaction)
                                {
                                    tempAnnotations.Add(anno);
                                }
                                else
                                {
                                    pageModel.NotHideRedactions.Add(anno);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                    pageModel.SeeAnnotations = tempAnnotations;
                    pageModel.OriginPage.Annotations = new List<Annotation>();
                    #endregion
                }

                docModel.IsRejected = flgRejectDoc;
                flgRejectBatch |= flgRejectDoc;
                docModel.PageCount = pageCount;
                totalPageCount += pageCount;

                if (doc.DocumentType == null)
                {
                    docModel.DocKind = Constant.DOC_KIND_LOOSE;
                    // Insert loose doc
                    model.Documents.Insert(1, docModel);
                }
                else
                {
                    // Add normal doc
                    model.Documents.Add(docModel);
                }
            }
            model.IsRejected = flgRejectBatch;
            model.DocCount = docCount;
            model.PageCount = totalPageCount;

            // Check batch have loose doc => if not, add new loose doc
            if (model.Documents[1].DocumentType != null)
            {
                model.Documents.Insert(1, new ViewDocumentModel()
                {
                    Id = Guid.NewGuid(),
                    DocKind = Constant.DOC_KIND_LOOSE
                });
            }

            return model;
        }

        public static Batch GetBatch(ViewBatchModel model)
        {
            var batch = new Batch();

            #region BatchInfo

            batch.BatchName = model.BatchName;
            batch.BatchPermission = model.BatchPermission;
            batch.BatchType = model.BatchType;
            batch.BatchTypeId = model.BatchTypeId;
            batch.BlockingActivityDescription = model.BlockingActivityDescription;
            batch.BlockingActivityName = model.BlockingActivityName;
            batch.BlockingBookmark = model.BlockingBookmark;
            batch.BlockingDate = model.BlockingDate;
            batch.Comments = model.Comments.Where(h => h.Id == Guid.Empty).ToList();
            batch.CreatedBy = model.CreatedBy;
            batch.DelegatedBy = model.DelegatedBy;
            batch.DelegatedTo = model.DelegatedTo;
            batch.DeletedDocuments = model.DeletedDocuments;
            batch.DeletedLooseDocuments = model.DeletedLooseDocuments;
            batch.DocCount = model.DocCount;
            batch.FieldValues = model.FieldValues;
            batch.HasError = model.HasError;
            batch.Id = model.Id;
            batch.IsCompleted = model.IsCompleted;
            batch.IsProcessing = model.IsProcessing;
            batch.IsRejected = model.IsRejected;
            batch.LastAccessedBy = model.LastAccessedBy;
            batch.LastAccessedDate = model.LastAccessedDate;
            batch.LockedBy = model.LockedBy;
            batch.ModifiedBy = model.ModifiedBy;
            batch.ModifiedDate = model.ModifiedDate;
            batch.PageCount = model.PageCount;
            batch.CreatedDate = model.CreatedDate;
            batch.StatusMsg = model.StatusMsg;
            batch.WorkflowDefinitionId = model.WorkflowDefinitionId;
            batch.WorkflowInstanceId = model.WorkflowInstanceId;

            #endregion

            var documents = new List<Document>();
            batch.Documents = documents;

            #region Mapping document

            foreach (var docModel in model.Documents.Where(h => h.DocKind != Constant.DOC_KIND_TEMP))
            {
                var doc = new Document();
                documents.Add(doc);

                #region Mapping doc info
                doc.AnnotationPermission = docModel.AnnotationPermission;
                doc.BatchId = docModel.BatchId;
                doc.BinaryType = docModel.BinaryType;
                doc.CreatedBy = docModel.CreatedBy;
                doc.CreatedDate = docModel.CreatedDate;
                doc.DeletedPages = docModel.DeletedPages;
                doc.DocName = docModel.DocName;
                doc.DocTypeId = docModel.DocTypeId;
                doc.DocumentType = docModel.DocumentType;
                doc.EmbeddedPictures = docModel.EmbeddedPictures;
                doc.FieldValues = docModel.FieldValues;
                doc.Id = docModel.Id;
                doc.IsRejected = docModel.IsRejected;
                doc.IsUndefinedType = docModel.IsUndefinedType;
                doc.ModifiedBy = docModel.ModifiedBy;
                doc.ModifiedDate = docModel.ModifiedDate;
                doc.PageCount = docModel.PageCount;
                #endregion

                var pages = new List<Page>();
                doc.Pages = pages;

                foreach (var pageModel in docModel.Pages)
                {
                    var page = pageModel.OriginPage;
                    pages.Add(page);

                    page.Annotations = new List<Annotation>();
                    page.Annotations.AddRange(pageModel.NotSeeAnnotations);
                    page.Annotations.AddRange(pageModel.NotHideRedactions);

                    foreach (var anno in pageModel.SeeAnnotations)
                    {
                        anno.DocId = doc.Id;
                        anno.DocTypeId = doc.DocTypeId;
                    }
                    page.Annotations.AddRange(pageModel.SeeAnnotations);
                }
            }

            #endregion

            batch.DocCount = batch.Documents.Count;
            var looseDoc = batch.Documents.SingleOrDefault(h => h.DocumentType == null);
            if (looseDoc != null && looseDoc.PageCount == 0)
            {
                batch.DocCount -= 1;
                batch.DeletedDocuments.Add(looseDoc.Id);
                batch.Documents.Remove(looseDoc);
            }

            return batch;
        }

        #endregion

        #region Capture controller

        public static List<CaptureAmbiguousDefinitionModel>
            GetAmbiguousDefinitionModels(List<AmbiguousDefinition> ambiguousDefinitions)
        {
            var results = new List<CaptureAmbiguousDefinitionModel>();

            if (ambiguousDefinitions != null && ambiguousDefinitions.Count > 0)
            {
                foreach (var ambiguousDefinition in ambiguousDefinitions)
                {
                    if (ambiguousDefinition != null)
                    {
                        results.Add(new CaptureAmbiguousDefinitionModel
                        {
                            AmbiguousDefinition = ambiguousDefinition
                        });
                    }
                }
            }

            return results;
        }

        #endregion

        #region Capture Admin Controller

        /// <summary>
        /// Lấy danh sách DocumentTypeModel theo danh sách DocumentType
        /// </summary>
        /// <param name="documentTypes">Danh sách DocumentType để lấy DocumentTypeModel</param>
        /// <returns></returns>
        public static List<DocumentTypeModel> GetDocumentTypeModels(IList<DocumentType> documentTypes)
        {
            if (documentTypes == null)
            {
                return null;
            }

            var documentTypeModels = new List<DocumentTypeModel>();
            foreach (DocumentType documentType in documentTypes)
            {
                documentTypeModels.Add(GetDocumentTypeModel(documentType));
            }

            return documentTypeModels;
        }
        /// <summary>
        /// Lấy DocumentTypeModel theo DocumentType
        /// </summary>
        /// <param name="documentType">DocumentType để lấy DocumentTypeModel</param>
        /// <returns></returns>
        public static DocumentTypeModel GetDocumentTypeModel(DocumentType documentType)
        {
            if (documentType == null)
            {
                return null;
            }

            return new DocumentTypeModel
            {
                Id = documentType.Id,
                Name = documentType.Name,
                CreateBy = documentType.CreatedBy,
                CreatedDate = documentType.CreatedDate,
                ModifiedBy = documentType.ModifiedBy,
                ModifiedDate = documentType.ModifiedDate,
                DocumentTypePermission = GetDocumentTypePermissionModel(documentType.DocumentTypePermission),
                OCRTemplate = GetOCRTemplateModel(documentType.OCRTemplate),
                Icon = documentType.Icon,
            };
        }

        /// <summary>
        /// lây OCRTemplateModel từ OCRTemplate
        /// </summary>
        /// <param name="ocrTemplate">OCRTemplate xác định OCRTemplateModel</param>
        /// <returns></returns>
        public static OCRTemplateModel GetOCRTemplateModel(OCRTemplate ocrTemplate)
        {
            if (ocrTemplate == null)
            {
                return null;
            }

            return new OCRTemplateModel
            {
                DocTypeId = ocrTemplate.DocTypeId,
                FileExtension = ocrTemplate.FileExtension,
                Language = GetLanguageModel(ocrTemplate.Language),
                OCRTemplatePages = GetOCRTemplatePageModels(ocrTemplate.OCRTemplatePages)
            };
        }
        /// <summary>
        /// Lấy LanguageModel theo language
        /// </summary>
        /// <param name="language">language để lấy LanguageModel</param>
        /// <returns></returns>
        public static LanguageModel GetLanguageModel(Language language)
        {
            if (language == null)
            {
                return null;
            }

            return new LanguageModel
            {
                Format = language.Format,
                Id = language.Id,
                Name = language.Name
            };
        }
        /// <summary>
        /// lấy danh sách OCRTemplatePageModels từ danh sách OCRTemplatePage
        /// </summary>
        /// <param name="ocrTemplatePages">danh sách OCRTemplatePage để xác định danh sách danh sách OCRTemplatePageModel</param>
        /// <returns></returns>
        public static IList<OCRTemplatePageModel> GetOCRTemplatePageModels(IList<OCRTemplatePage> ocrTemplatePages)
        {
            if (ocrTemplatePages == null)
            {
                return null;
            }

            return ocrTemplatePages.Select(GetOCRTemplatePageModel).ToList();
        }
        /// <summary>
        /// lấy OCRTemplatePageModel từ OCRTemplatePage
        /// </summary>
        /// <param name="ocrTemplatePage">OCRTemplatePage xác định OCRTemplatePageModel</param>
        /// <returns></returns>
        public static OCRTemplatePageModel GetOCRTemplatePageModel(OCRTemplatePage ocrTemplatePage)
        {
            if (ocrTemplatePage == null)
            {
                return null;
            }

            return new OCRTemplatePageModel
            {
                Binary = ocrTemplatePage.Binary,
                DPI = ocrTemplatePage.DPI,
                Id = ocrTemplatePage.Id,
                OCRTemplateId = ocrTemplatePage.OCRTemplateId,
                PageIndex = ocrTemplatePage.PageIndex,
                OCRTemplateZones = GetOCRTemplateZoneModels(ocrTemplatePage.OCRTemplateZones)
            };
        }
        /// <summary>
        /// lấy danh sách OCRTemplateZoneModels từ danh sách OCRTemplateZone
        /// </summary>
        /// <param name="ocrTemplateZones">danh sách OCRTemplateZone xác định danh sách OCRTemplateZoneModel</param>
        /// <returns></returns>
        public static List<OCRTemplateZoneModel> GetOCRTemplateZoneModels(IList<OCRTemplateZone> ocrTemplateZones)
        {
            if (ocrTemplateZones == null)
            {
                return null;
            }

            return ocrTemplateZones.Select(GetOCRTemplateZoneModel).ToList();
        }
        /// <summary>
        /// lấy OCRTemplateZoneModel từ OCRTemplateZone
        /// </summary>
        /// <param name="ocrTemplateZone">OCRTemplateZone xác định OCRTemplateZoneModel</param>
        /// <returns></returns>
        public static OCRTemplateZoneModel GetOCRTemplateZoneModel(OCRTemplateZone ocrTemplateZone)
        {
            if (ocrTemplateZone == null)
            {
                return null;
            }

            return new OCRTemplateZoneModel
            {
                FieldMetaDataId = ocrTemplateZone.FieldMetaDataId,
                Height = ocrTemplateZone.Height,
                Width = ocrTemplateZone.Width,
                Left = ocrTemplateZone.Left,
                Top = ocrTemplateZone.Top,
                OCRTemplatePageId = ocrTemplateZone.OCRTemplatePageId,
                CreatedBy = ocrTemplateZone.CreatedBy,
                CreatedOn = ocrTemplateZone.CreatedOn,
                ModifiedBy = ocrTemplateZone.ModifiedBy,
                ModifiedOn = ocrTemplateZone.ModifiedOn
            };
        }
        /// <summary>
        /// lấy DocumentTypePermissionModel từ DocumentTypePermission
        /// </summary>
        /// <param name="permission">DocDocumentTypePermission để xác định DocumentTypePermissionModel</param>
        /// <returns></returns>
        public static DocumentTypePermissionModel GetDocumentTypePermissionModel(DocumentTypePermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocumentTypePermissionModel
            {
                DocTypeId = permission.DocTypeId,
                UserGroupId = permission.UserGroupId,
            };
        }
        #endregion

        #endregion

        #region Private methods

        /// <summary>
        /// Populate the column for table search result.
        /// </summary>
        /// <param name="table">Table want to populate</param>
        /// <param name="source">WorkingItmes</param>
        private static void PopulateColumnsForSearchResultModel(DataTable table,
                                                                WorkItemSearchResult source,
                                                                out int indexActivityName)
        {
            var dataTypeFolder = FieldDataType.Folder.ToString();
            var dataTypeTable = FieldDataType.Table.ToString();

            // Create columns for normal batch type field
            var normalFields = source.BatchType.Fields.Where
                (
                    h => !h.IsSystemField &&
                    (
                        !dataTypeFolder.Equals(h.DataType, StringComparison.OrdinalIgnoreCase) ||
                        !dataTypeTable.Equals(h.DataType, StringComparison.OrdinalIgnoreCase)
                    )
                ).OrderBy(h => h.DisplayOrder).ToList();

            DataColumn column;

            // Create columns guid
            column = new DataColumn(FieldUtils.COLUMN_GUID, typeof(Guid));
            table.Columns.Add(column);

            column = new DataColumn("Batch Status", typeof(string));
            table.Columns.Add(column);

            column = new DataColumn("Batch Permission", typeof(string));
            table.Columns.Add(column);

            indexActivityName = 0;

            // Create columns for batch type field
            foreach (var item in normalFields)
            {
                column = new DataColumn(item.Name, FieldUtils.GetSystemType(item.DataTypeEnum));
                column.Caption = item.Name;

                table.Columns.Add(column);
                indexActivityName++;
            }

            // Create columns for system batch type field
            // Create by
            column = new DataColumn(FieldUtils.COLUMN_CREATE_BY, typeof(string));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.CREATE_BY);
            table.Columns.Add(column);

            // Create date
            column = new DataColumn(FieldUtils.COLUMN_CREATE_DATE, typeof(DateTime));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.CREATE_ON);
            table.Columns.Add(column);

            // Page count
            column = new DataColumn(FieldUtils.COLUMN_PAGE_COUNT, typeof(int));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.PAGE_COUNT);
            table.Columns.Add(column);

            // Document count
            column = new DataColumn(FieldUtils.COLUMN_DOCUMENT_COUNT, typeof(int));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.DOCUMENT_COUNT);
            table.Columns.Add(column);

            // Create columns for workflow field
            // Locked by
            column = new DataColumn(FieldUtils.COLUMN_LOCKED_BY, typeof(string));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_LOCKED_BY);
            table.Columns.Add(column);

            // Activity name
            column = new DataColumn(FieldUtils.COLUMN_ACTIVITY_NAME, typeof(string));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_ACTIVITY_NAME);
            table.Columns.Add(column);
            indexActivityName += 6;

            // Blocking date
            column = new DataColumn(FieldUtils.COLUMN_BLOCKING_DATE, typeof(DateTime));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_BLOCKING_DATE);
            table.Columns.Add(column);

            // Last access by
            column = new DataColumn(FieldUtils.COLUMN_LAST_ACCESS_BY, typeof(string));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_LAST_ACCESS_BY);
            table.Columns.Add(column);

            // Last access date
            column = new DataColumn(FieldUtils.COLUMN_LAST_ACCESS_DATE, typeof(DateTime));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_LAST_ACCESS_DATE);
            table.Columns.Add(column);

            // Completed
            column = new DataColumn(FieldUtils.COLUMN_COMPLETED, typeof(bool));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_COMPLETED);
            table.Columns.Add(column);

            // Processing
            column = new DataColumn(FieldUtils.COLUMN_PROCESSING, typeof(bool));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_PROCESSING);
            table.Columns.Add(column);

            // Has error
            column = new DataColumn(FieldUtils.COLUMN_HAS_ERROR, typeof(bool));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_HAS_ERROR);
            table.Columns.Add(column);

            // Status
            column = new DataColumn(FieldUtils.COLUMN_STATUS, typeof(string));
            column.Caption = FieldUtils.MapSystemFieldNameDisplay(FieldUtils.COLUMN_STATUS);
            table.Columns.Add(column);
        }

        /// <summary>
        /// Populate the row for table search result.
        /// </summary>
        /// <param name="table">Table want to populate</param>
        /// <param name="source">WorkingItmes</param>
        private static void PopulateRowsForSearchResultModel(DataTable table, WorkItemSearchResult source)
        {
            DataRow row;

            foreach (var item in source.WorkItems)
            {
                row = table.NewRow();

                // Create value for GUID field
                row[FieldUtils.COLUMN_GUID] = item.Id;
                row["Batch Status"] = GetBatchStatus(item).ToString();
                row["Batch Permission"] = GetJsonSearchBatchPermission(item.BatchPermission);

                // Create value for normal field
                foreach (var field in item.FieldValues)
                {
                    if (table.Columns.Contains(field.FieldMetaData.Name))
                    {
                        if (!string.IsNullOrWhiteSpace(field.Value))
                        {
                            row[field.FieldMetaData.Name] = field.Value;
                        }
                    }
                }

                // Create value for system field
                row[FieldUtils.COLUMN_CREATE_BY] = item.CreatedBy;
                row[FieldUtils.COLUMN_CREATE_DATE] = item.CreatedDate;
                row[FieldUtils.COLUMN_PAGE_COUNT] = item.PageCount;
                row[FieldUtils.COLUMN_DOCUMENT_COUNT] = item.DocCount;

                // Create value for workflow field
                row[FieldUtils.COLUMN_LOCKED_BY] = item.LockedBy;
                row[FieldUtils.COLUMN_ACTIVITY_NAME] = item.BlockingActivityName;
                if (item.BlockingDate.HasValue)
                {
                    row[FieldUtils.COLUMN_BLOCKING_DATE] = item.BlockingDate.Value;
                }
                row[FieldUtils.COLUMN_LAST_ACCESS_BY] = item.LastAccessedBy;
                if (item.LastAccessedDate.HasValue)
                {
                    row[FieldUtils.COLUMN_LAST_ACCESS_DATE] = item.LastAccessedDate.Value;
                }
                row[FieldUtils.COLUMN_COMPLETED] = item.IsCompleted;
                row[FieldUtils.COLUMN_PROCESSING] = item.IsProcessing;
                row[FieldUtils.COLUMN_HAS_ERROR] = item.HasError;
                row[FieldUtils.COLUMN_STATUS] = item.StatusMsg;

                table.Rows.Add(row);
            }
        }

        /// <summary>
        /// Get the status of batch
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private static BatchStatus GetBatchStatus(Batch batch)
        {
            if (!string.IsNullOrWhiteSpace(batch.LockedBy))
            {
                return BatchStatus.Locked;
            }

            if (batch.HasError)
            {
                return BatchStatus.Error;
            }

            if (batch.IsProcessing)
            {
                return BatchStatus.InProcessing;
            }

            if (batch.IsRejected && string.IsNullOrWhiteSpace(batch.LockedBy))
            {
                return BatchStatus.Reject;
            }

            return BatchStatus.Available;
        }

        /// <summary>
        /// Get the Json batch permission
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        private static string GetJsonSearchBatchPermission(BatchPermission permission)
        {
            return JsonConvert.SerializeObject(new
            {
                Reject = permission.CanReject,
                SendLink = permission.CanSendLink,
                Delete = permission.CanDelete,
                Delegate = permission.CanDelegateItems
            });
        }

        /// <summary>
        /// Build the where condition of one expression.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="expression"></param>
        private static void BuildSearchExpression(StringBuilder builder, SearchQueryExpression expression)
        {
            var searchOperator = expression.OperatorEnum;
            var name = expression.FieldMetaData.Name;
            var value1 = expression.Value1;
            var value2 = expression.Value2;

            switch (expression.FieldMetaData.DataTypeEnum)
            {
                case FieldDataType.String:
                    switch (searchOperator)
                    {
                        case SearchOperator.Contains:
                            builder.AppendFormat("[{0}] LIKE '%{1}%' ", name, value1);
                            break;
                        case SearchOperator.NotContains:
                            builder.AppendFormat("[{0}] NOT LIKE '%{1}%' ", name, value1);
                            break;
                        case SearchOperator.EndsWith:
                            builder.AppendFormat("[{0}] LIKE '%{1}' ", name, value1);
                            break;
                        case SearchOperator.StartsWith:
                            builder.AppendFormat("[{0}] LIKE '{1}%' ", name, value1);
                            break;
                        case SearchOperator.Equal:
                            builder.AppendFormat("[{0}] = '{1}' ", name, value1);
                            break;
                        case SearchOperator.NotEqual:
                            builder.AppendFormat("[{0}] <> '{1}' ", name, value1);
                            break;
                        default:
                            break;
                    }
                    break;
                case FieldDataType.Integer:
                case FieldDataType.Decimal:
                    switch (searchOperator)
                    {
                        case SearchOperator.Equal:
                            builder.AppendFormat("[{0}] = {1} ", name, value1);
                            break;
                        case SearchOperator.NotEqual:
                            builder.AppendFormat("[{0}] <> {1} ", name, value1);
                            break;
                        case SearchOperator.GreaterThan:
                            builder.AppendFormat("[{0}] > {1} ", name, value1);
                            break;
                        case SearchOperator.GreaterThanOrEqualTo:
                            builder.AppendFormat("[{0}] >= {1} ", name, value1);
                            break;
                        case SearchOperator.LessThan:
                            builder.AppendFormat("[{0}] < {1} ", name, value1);
                            break;
                        case SearchOperator.LessThanOrEqualTo:
                            builder.AppendFormat("[{0}] <= {1} ", name, value1);
                            break;
                        case SearchOperator.InBetween:
                            if (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2))
                            {
                                builder.AppendFormat("([{0}] >= {1} AND [{0}] <= {2}) ", name, value1, value2);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case FieldDataType.Boolean:
                    switch (searchOperator)
                    {
                        case SearchOperator.Equal:
                            builder.AppendFormat("[{0}] = {1} ", name, bool.Parse(value1) ? 1 : 0);
                            break;
                        case SearchOperator.NotEqual:
                            builder.AppendFormat("[{0}] <> {1} ", name, bool.Parse(value1) ? 1 : 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case FieldDataType.Date:
                    switch (searchOperator)
                    {
                        case SearchOperator.Equal:
                            builder.AppendFormat("([{0}] >= CONVERT(DATETIME,'{1} 00:00:00') " +
                                                 "AND [{0}] <= CONVERT(DATETIME,'{1} 23:59:59')) ",
                                                 name, DateTime.Parse(value1).ToString("yyyy-MM-dd"));
                            break;
                        case SearchOperator.NotEqual:
                            builder.AppendFormat("([{0}] < CONVERT(DATETIME,'{1} 00:00:00') " +
                                                 "AND [{0}] > CONVERT(DATETIME,'{1} 23:59:59')) ",
                                                 name, DateTime.Parse(value1).ToString("yyyy-MM-dd"));
                            break;
                        case SearchOperator.GreaterThan:
                            builder.AppendFormat("[{0}] > CONVERT(DATETIME,'{1} 23:59:59') ",
                                                 name, DateTime.Parse(value1).ToString("yyyy-MM-dd"));
                            break;
                        case SearchOperator.GreaterThanOrEqualTo:
                            builder.AppendFormat("[{0}] >= CONVERT(DATETIME,'{1} 00:00:00') ",
                                                 name, DateTime.Parse(value1).ToString("yyyy-MM-dd"));
                            break;
                        case SearchOperator.LessThan:
                            builder.AppendFormat("[{0}] < CONVERT(DATETIME,'{1} 00:00:00') ",
                                                 name, DateTime.Parse(value1).ToString("yyyy-MM-dd"));
                            break;
                        case SearchOperator.LessThanOrEqualTo:
                            builder.AppendFormat("[{0}] > CONVERT(DATETIME,'{1} 23:59:59') ",
                                                 name, DateTime.Parse(value1).ToString("yyyy-MM-dd"));
                            break;
                        case SearchOperator.InBetween:
                            if (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2))
                            {
                                builder.AppendFormat("([{0}] >= CONVERT(DATETIME,'{1} 00:00:00') " +
                                                     "AND [{0}] <= CONVERT(DATETIME,'{2} 23:59:59')) ",
                                                     name,
                                                     DateTime.Parse(value1).ToString("yyyy-MM-dd"),
                                                     DateTime.Parse(value2).ToString("yyyy-MM-dd"));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Build the where condition of all expression.
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        private static string BuildSearchExpression(IEnumerable<SearchQueryExpression> expressions)
        {
            if (expressions == null || expressions.Count() == 0)
            {
                return string.Empty;
            }

            var expressionBuilder = new StringBuilder();

            // Build first condition
            BuildSearchExpression(expressionBuilder, expressions.ElementAt(0));

            // Build other condition
            for (int i = 1; i < expressions.Count(); i++)
            {
                expressionBuilder.AppendFormat("{0} ", expressions.ElementAt(i).Condition);
            }

            return expressionBuilder.ToString();
        }

        private static CaptureAmbiguousDefinitionModel
            GetAmbiguousDefinitionModel(AmbiguousDefinition ambiguousDefinition)
        {
            if (ambiguousDefinition == null)
            {
                return null;
            }

            return new CaptureAmbiguousDefinitionModel
            {
                AmbiguousDefinition = ambiguousDefinition
            };
        }

        #endregion
    }
}