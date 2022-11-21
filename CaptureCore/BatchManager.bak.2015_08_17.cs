using System;
using System.Linq;
using System.Collections.Generic;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.Utility;
using Ecm.Workflow.WorkflowController;
using Ecm.SecurityDao;
using System.Security;
using Ecm.ScriptEngine;
using System.Reflection;
using Ecm.LookupDomain;
using System.Data;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace Ecm.CaptureCore
{
    public partial class BatchManager : ManagerBase
    {
        private const string AUTHORIZED_KEY = "5883A44C-B5D3-48E0-AC14-A7045FE3B1D2";
        private const string CAPTURE_FOLDER = "CAPTURE";
        private const string CAPTURE_RELEASE_FOLDER = "CAPTURE_RELEASE";
        private Setting _setting = new Setting();

        private const string ANNO_TEXT_FORMAT =
            @"<Section xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xml:space=""preserve"" TextAlignment=""Left"" LineHeight=""Auto"" IsHyphenationEnabled=""False"" xml:lang=""en-us"" FlowDirection=""LeftToRight"" NumberSubstitution.CultureSource=""User"" NumberSubstitution.Substitution=""AsCulture"" FontFamily=""Segoe UI"" FontStyle=""Normal"" FontWeight=""Normal"" FontStretch=""Normal"" FontSize=""12"" Foreground=""#FF000000"" Typography.StandardLigatures=""True"" Typography.ContextualLigatures=""True"" Typography.DiscretionaryLigatures=""False"" Typography.HistoricalLigatures=""False"" Typography.AnnotationAlternates=""0"" Typography.ContextualAlternates=""True"" Typography.HistoricalForms=""False"" Typography.Kerning=""True"" Typography.CapitalSpacing=""False"" Typography.CaseSensitiveForms=""False"" Typography.StylisticSet1=""False"" Typography.StylisticSet2=""False"" Typography.StylisticSet3=""False"" Typography.StylisticSet4=""False"" Typography.StylisticSet5=""False"" Typography.StylisticSet6=""False"" Typography.StylisticSet7=""False"" Typography.StylisticSet8=""False"" Typography.StylisticSet9=""False"" Typography.StylisticSet10=""False"" Typography.StylisticSet11=""False"" Typography.StylisticSet12=""False"" Typography.StylisticSet13=""False"" Typography.StylisticSet14=""False"" Typography.StylisticSet15=""False"" Typography.StylisticSet16=""False"" Typography.StylisticSet17=""False"" Typography.StylisticSet18=""False"" Typography.StylisticSet19=""False"" Typography.StylisticSet20=""False"" Typography.Fraction=""Normal"" Typography.SlashedZero=""False"" Typography.MathematicalGreek=""False"" Typography.EastAsianExpertForms=""False"" Typography.Variants=""Normal"" Typography.Capitals=""Normal"" Typography.NumeralStyle=""Normal"" Typography.NumeralAlignment=""Normal"" Typography.EastAsianWidths=""Normal"" Typography.EastAsianLanguage=""Normal"" Typography.StandardSwashes=""0"" Typography.ContextualSwashes=""0"" Typography.StylisticAlternates=""0"">
    {0}
</Section>";

        public BatchManager(User loginUser)
            : base(loginUser)
        {
            _setting = new SettingManager(loginUser).GetSettings();
        }

        public void InsertBatch(Batch batch, bool fromMobile = false)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();

                    WorkflowDefinitionDao wfDao = new WorkflowDefinitionDao(context);
                    DocumentDao documentDao = new DocumentDao(context);
                    PageDao pageDao = new PageDao(context);
                    AnnotationDao annotationDao = new AnnotationDao(context);
                    TableFieldValueDao tableValueDao = new TableFieldValueDao(context);
                    OutlookPictureDao outlookPicturesDao = new OutlookPictureDao(context);

                    var workflowDefinition = wfDao.GetByBatchTypeId(batch.BatchTypeId);
                    ActionLogHelper.AddActionLog("Get batch type by Id: " + batch.BatchTypeId,
                                                  LoginUser, ActionName.GetBatchType, null, null, context);

                    if (batch.Documents == null || batch.Documents.Count == 0)
                    {
                        throw new InvalidBatchDataException("Batch have no content.");
                    }

                    // Get pages count
                    var totalPageCount = 0;
                    foreach (var document in batch.Documents)
                    {
                        if (document.Pages == null || document.Pages.Count == 0)
                        {
                            throw new InvalidBatchDataException("Batch have content which have no page.");
                        }

                        totalPageCount += document.Pages.Count;
                    }

                    bool flagAddActionLog;
                    var batchDao = new BatchDao(context);

                    #region Set batch info
                    batch.Id = Guid.NewGuid();
                    batch.IsProcessing = true;
                    batch.IsCompleted = false;
                    batch.IsRejected = false;
                    batch.HasError = false;
                    batch.WorkflowDefinitionId = workflowDefinition.Id;
                    batch.WorkflowInstanceId = Guid.Empty;
                    // Set system fields
                    batch.DocCount = batch.Documents.Count;
                    batch.PageCount = totalPageCount;
                    batch.CreatedDate = DateTime.Now;
                    batch.CreatedBy = LoginUser.UserName;
                    batch.ModifiedDate = null;
                    batch.ModifiedBy = null;
                    batch.LockedBy = null;
                    batch.DelegatedBy = null;
                    batch.DelegatedTo = null;
                    batch.BlockingBookmark = null;
                    batch.BlockingActivityName = null;
                    batch.BlockingActivityDescription = null;
                    batch.BlockingDate = null;
                    batch.LastAccessedDate = null;
                    batch.LastAccessedBy = null;
                    batch.StatusMsg = "Creating batch";
                    #endregion

                    // Insert table Batch
                    batchDao.Add(batch);

                    #region Add comment
                    if (batch.Comments != null)
                    {
                        var commentDao = new CommentDao(context);
                        var tempCreateDate = batch.CreatedDate.AddMilliseconds(-batch.CreatedDate.Millisecond);

                        flagAddActionLog = false;

                        foreach (var comment in batch.Comments.OrderBy(h => h.CreatedDate))
                        {
                            if (string.IsNullOrWhiteSpace(comment.Note))
                            {
                                continue;
                            }

                            flagAddActionLog = true;

                            comment.InstanceId = batch.Id;
                            comment.IsBatchId = true;
                            comment.Note = comment.Note.Trim();
                            comment.CreatedDate = batch.CreatedDate;
                            comment.CreatedBy = LoginUser.UserName;

                            // Insert table Comment
                            commentDao.InsertComment(comment);
                        }

                        if (flagAddActionLog)
                        {
                            ActionLogHelper.AddActionLog("Add comment for batch: " + batch.Id,
                                                         LoginUser, ActionName.AddComment, null, null, context);
                        }
                    }
                    #endregion

                    #region Add batch field value
                    var batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);
                    var batchFieldDao = new BatchFieldValueDao(context);

                    var batchFieldMeataDatas = batchFieldMetaDataDao.GetByBatchType(batch.BatchTypeId);
                    batch.FieldValues = batch.FieldValues ?? new List<BatchFieldValue>();

                    BatchFieldValue batchFieldValue;
                    foreach (var batchFieldMetaData in batchFieldMeataDatas)
                    {
                        batchFieldValue = batch.FieldValues.FirstOrDefault(h => h.FieldId == batchFieldMetaData.Id);

                        // Case the user not provider the field value 
                        // => create new field value with null value
                        if (batchFieldValue == null)
                        {
                            batchFieldValue = new BatchFieldValue();
                            batchFieldValue.FieldId = batchFieldMetaData.Id;
                        }
                        else
                        {
                            // Validate batch field value
                            ValidateBatchFieldValue(batchFieldValue, batchFieldMetaData);
                        }

                        batchFieldValue.Id = Guid.NewGuid();
                        batchFieldValue.BatchId = batch.Id;

                        // Insert table BatchFieldValue
                        batchFieldDao.Add(batchFieldValue);
                    }

                    ActionLogHelper.AddActionLog("Add batch field value for batch: " + batch.Id,
                                                 LoginUser, ActionName.CreateBatchFieldValue, null, null, context);
                    #endregion

                    var setting = new SettingManager(LoginUser).GetSettings();
                    var haveMeetLooseDoc = false;

                    Dictionary<Guid, List<DocumentFieldMetaData>> dicDocumentFieldMetaData = new Dictionary<Guid, List<DocumentFieldMetaData>>();
                    var docFieldValueDao = new DocumentFieldValueDao(context);
                    var docFieldMetaDataDao = new DocFieldMetaDataDao(context);
                    var picklistDao = new PicklistDao(context);
                    List<DocumentFieldMetaData> documentFieldMetaDatas;
                    DocumentFieldValue docFieldValue;

                    foreach (var document in batch.Documents)
                    {
                        #region Insert table Document
                        document.BatchId = batch.Id;
                        document.CreatedDate = batch.CreatedDate;
                        document.CreatedBy = batch.CreatedBy;
                        document.IsRejected = false;
                        document.PageCount = document.Pages.Count;
                        documentDao.Add(document);

                        if (document.EmbeddedPictures != null)
                        {
                            foreach (OutlookPicture pic in document.EmbeddedPictures)
                            {
                                pic.DocId = document.Id;
                                outlookPicturesDao.InsertPicture(pic);
                            }
                        }

                        ActionLogHelper.AddActionLog("Create document Id: " + document.Id + " on batch field value for batch: " + batch.Id,
                                  LoginUser, ActionName.AddDocument, null, null, context);
                        #endregion

                        // Loose doc type (only one loose doc type in one batch)
                        if (document.DocTypeId == Guid.Empty)
                        {
                            if (haveMeetLooseDoc)
                            {
                                throw new InvalidBatchDataException("Batch have more than one loose content type.");
                            }
                            haveMeetLooseDoc = true;
                        }
                        else
                        {
                            #region Add doc field
                            // Not load meta data for this document field
                            if (!dicDocumentFieldMetaData.ContainsKey(document.DocTypeId))
                            {
                                #region
                                documentFieldMetaDatas = docFieldMetaDataDao.GetByDocType(document.DocTypeId);
                                foreach (var documentFieldMetaData in documentFieldMetaDatas)
                                {
                                    documentFieldMetaData.DataType = string.Format("{0}", documentFieldMetaData.DataType).Trim().ToUpper();

                                    if (documentFieldMetaData.DataType == "TABLE")
                                    {
                                        documentFieldMetaData.Children = docFieldMetaDataDao.GetChildren(documentFieldMetaData.Id);
                                        foreach (var childMetaData in documentFieldMetaData.Children)
                                        {
                                            childMetaData.DataType = childMetaData.DataType.ToUpper();
                                        }
                                    }
                                    else if (documentFieldMetaData.DataType == "PICKLIST")
                                    {
                                        documentFieldMetaData.Picklists = picklistDao.GetByField(documentFieldMetaData.Id);
                                        for (int i = 0; i < documentFieldMetaData.Picklists.Count; i++)
                                        {
                                            documentFieldMetaData.Picklists[i].Value = documentFieldMetaData.Picklists[i].Value.ToUpper();
                                        }
                                    }
                                }


                                dicDocumentFieldMetaData.Add(document.DocTypeId, documentFieldMetaDatas);
                                #endregion
                            }
                            else
                            {
                                documentFieldMetaDatas = dicDocumentFieldMetaData[document.DocTypeId];
                            }

                            if (document.FieldValues == null)
                            {
                                document.FieldValues = new List<DocumentFieldValue>();
                            }

                            foreach (var docFieldMetaData in documentFieldMetaDatas)
                            {
                                #region
                                if (docFieldMetaData.IsSystemField)
                                {
                                    continue;
                                }

                                docFieldValue = document.FieldValues.FirstOrDefault(h => h.FieldId == docFieldMetaData.Id);

                                if (docFieldValue == null)
                                {
                                    docFieldValue = new DocumentFieldValue();
                                    docFieldValue.FieldId = docFieldMetaData.Id;
                                }

                                // Case data type table
                                if (docFieldMetaData.DataType == "TABLE")
                                {
                                    #region
                                    // Case table field is required but have no field value
                                    if (docFieldValue.TableFieldValue == null || docFieldValue.TableFieldValue.Count == 0)
                                    {
                                        if (docFieldMetaData.IsRequired)
                                        {
                                            throw new InvalidBatchDataException(
                                                string.Format("Batch contains content '{0}' have no value for required content field '{1}'.",
                                                                document.DocName, docFieldMetaData.Name));
                                        }
                                    }

                                    // Validate batch field value
                                    ValidateDocTableFieldValue(docFieldValue, docFieldMetaData, document.DocName);
                                    // Table field have value = null => real value store in table TableFieldValue of DB
                                    docFieldValue.Value = null;
                                    #endregion
                                }
                                // Case data type <> table
                                else
                                {
                                    // Validate batch field value
                                    ValidateDocFieldValue(docFieldValue, docFieldMetaData, document.DocName);
                                }

                                docFieldValue.Id = Guid.NewGuid();
                                docFieldValue.DocId = document.Id;

                                // Insert table BatchFieldValue
                                docFieldValueDao.Add(docFieldValue);

                                // Insert value table TableFieldValue
                                if (docFieldValue.TableFieldValue != null)
                                {
                                    foreach (var tableFieldValue in docFieldValue.TableFieldValue)
                                    {
                                        tableFieldValue.DocId = document.Id;
                                        tableValueDao.Add(tableFieldValue);
                                    }
                                }
                                #endregion
                            }

                            ActionLogHelper.AddActionLog("Add field value on document Id: " + document.Id,
                                                          LoginUser, ActionName.AddFieldValue, null, null, context);
                            #endregion
                        }

                        int pageNumber = 1;

                        foreach (var page in document.Pages)
                        {
                            page.DocId = document.Id;
                            if (page.FileBinaryBase64 != null && page.FileBinary == null)
                            {
                                page.FileBinary = Convert.FromBase64String(page.FileBinaryBase64);
                            }

                            page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                            page.PageNumber = pageNumber;

                            //Check Setting Allow Create File
                            if (setting.IsSaveFileInFolder)
                            {
                                string filename = string.Empty;

                                if (document.DocTypeId == Guid.Empty)
                                {
                                    filename = Path.Combine(GetLooseDocFolder(batch, document), Guid.NewGuid().ToString());
                                }
                                else
                                {
                                    filename = Path.Combine(GetDocFolder(document), Guid.NewGuid().ToString());
                                }

                                string path = Path.Combine(setting.LocationSaveFile, CAPTURE_FOLDER, filename);
                                byte[] header = FileHelpper.CreateFile(path, page.FileBinary, page.FileExtension);

                                page.FilePath = path;
                                page.FileHeader = header;
                                page.FileBinary = null;
                            }

                            page.IsRejected = false;
                            pageDao.Add(page);

                            if (page.Annotations != null)
                            {
                                foreach (var anno in page.Annotations)
                                {
                                    anno.PageId = page.Id;
                                    anno.DocId = document.Id;
                                    anno.DocTypeId = document.DocTypeId;
                                    anno.CreatedBy = batch.CreatedBy;
                                    anno.CreatedOn = batch.CreatedDate;
                                    anno.LineEndAt = "TopLeft";
                                    anno.LineStartAt = "TopLeft";
                                    anno.LineStyle = "ArrowAtEnd";

                                    if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                                    {
                                        anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                                    }

                                    annotationDao.Add(anno);
                                }
                            }

                            pageNumber++;
                        }

                        ActionLogHelper.AddActionLog("Add pages on document Id: " + document.Id,
                                  LoginUser, ActionName.InsertPage, null, null, context);
                    }

                    context.Commit();

                    CallWorkflow(batch, true, null);

                }
                catch (Exception ex)
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        private void ValidateScriptDocFieldValue(DocumentFieldValue docFieldValue, DocumentFieldMetaData docFieldMetaData)
        {
            if (!string.IsNullOrEmpty(docFieldMetaData.ValidationScript))
            {
                var scriptValue = docFieldMetaData.ValidationScript.Replace("<<Value>>", docFieldValue.Value);
                var script = CSharpScriptEngine.script.Replace("<<ScriptHere>>", scriptValue);
                var ass = CSharpScriptEngine.CompileCode(script);

                if (!CSharpScriptEngine.RunScript(ass))
                {
                    throw new InvalidBatchDataException(string.Format("Value '{0}' is invalid for validation script of field {1}", docFieldValue.Value, docFieldMetaData.Name));
                }
            }
        }

        private void ValidatePatterDocFieldValue(DocumentFieldValue docFieldValue, DocumentFieldMetaData docFieldMetaData)
        {
            if (!string.IsNullOrEmpty(docFieldMetaData.ValidationPattern))
            {
                var regEx = new Regex(docFieldMetaData.ValidationPattern);

                if (!regEx.IsMatch(docFieldValue.Value))
                {
                    throw new InvalidBatchDataException(string.Format("Value '{0}' is invalid for validation pattern of field {1}", docFieldValue.Value, docFieldMetaData.Name));
                }
            }
        }

        private void ValidateDocFieldValue(DocumentFieldValue docFieldValue, DocumentFieldMetaData docFieldMetaData, string docName)
        {
            var value = string.Format("{0}", docFieldValue.Value).Trim();

            if (value == string.Empty)
            {
                if (docFieldMetaData.IsRequired)
                {
                    throw new InvalidBatchDataException(
                        string.Format("Batch contains content '{0}' have no value for required content field '{1}'.",
                                        docName, docFieldMetaData.Name));
                }

                docFieldValue.Value = null;
                return;
            }

            // Data type is in upper case
            switch (docFieldMetaData.DataType)
            {
                case "STRING":
                    #region
                    if (value.Length > docFieldMetaData.MaxLength)
                    {
                        throw new InvalidBatchDataException(string.Format("Value '{0}' is exceed max length {1} for data type String.", value, docFieldMetaData.MaxLength));
                    }

                    docFieldValue.Value = value;

                    ValidatePatterDocFieldValue(docFieldValue, docFieldMetaData);
                    ValidateScriptDocFieldValue(docFieldValue, docFieldMetaData);

                    break;
                #endregion

                case "INTEGER":
                    #region
                    int tempInt;
                    if (int.TryParse(value, out tempInt))
                    {
                        docFieldValue.Value = tempInt.ToString();
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Integer.", value));
                    }

                    ValidateScriptDocFieldValue(docFieldValue, docFieldMetaData);

                    break;
                #endregion

                case "DECIMAL":
                    #region
                    decimal tempDecimal;
                    if (decimal.TryParse(value, out tempDecimal))
                    {
                        docFieldValue.Value = tempDecimal.ToString();
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Decimal.", value));
                    }

                    ValidatePatterDocFieldValue(docFieldValue, docFieldMetaData);
                    ValidateScriptDocFieldValue(docFieldValue, docFieldMetaData);

                    break;
                #endregion

                case "DATE":
                    #region
                    DateTime tempDate;
                    if (DateTime.TryParseExact(value.Substring(0, 10), "yyyy-MM-dd", null, DateTimeStyles.None, out tempDate))
                    {
                        docFieldValue.Value = tempDate.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Date.", value));
                    }

                    ValidateScriptDocFieldValue(docFieldValue, docFieldMetaData);

                    break;
                #endregion

                case "BOOLEAN":
                    #region
                    bool tempBool;
                    if (bool.TryParse(value, out tempBool))
                    {
                        docFieldValue.Value = tempBool.ToString();
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Boolean.", value));
                    }

                    ValidateScriptDocFieldValue(docFieldValue, docFieldMetaData);

                    break;
                #endregion

                case "PICKLIST":
                    #region
                    var valueUpper = value.ToUpper();
                    if (docFieldMetaData.Picklists.Any(h => h.Value == valueUpper))
                    {
                        docFieldValue.Value = value;
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Picklist.", value));
                    }

                    break;
                #endregion

                default:
                    throw new InvalidBatchDataException(string.Format("Invalid data type {0} for content field value.", docFieldMetaData.DataType));
            }
        }

        private void ValidateDocTableFieldValue(DocumentFieldValue docFieldValue, DocumentFieldMetaData docFieldMetaData, string docName)
        {
            var groupRows = docFieldValue.TableFieldValue.GroupBy(h => h.RowNumber).OrderBy(h => h.Key).ToList();
            string value;

            var rowIndex = 0;
            var tempTableFieldValues = new List<TableFieldValue>();
            var rowFieldValues = new List<TableFieldValue>(docFieldMetaData.Children.Count);
            bool haveValue;
            TableFieldValue tableFieldValue;

            foreach (var row in groupRows)
            {
                haveValue = false;

                foreach (var childFeldMetaData in docFieldMetaData.Children)
                {
                    tableFieldValue = row.FirstOrDefault(h => h.FieldId == childFeldMetaData.Id);

                    if (tableFieldValue == null)
                    {
                        throw new InvalidBatchDataException(
                            string.Format("Required value of column '{0}' for content table field '{1}'.",
                                           childFeldMetaData.Name, docFieldMetaData.Name));
                    }

                    value = string.Format("{0}", tableFieldValue.Value).Trim();

                    if (value == string.Empty)
                    {
                        #region
                        if (childFeldMetaData.IsRequired)
                        {
                            throw new InvalidBatchDataException(
                                string.Format("Required value of column '{0}' for content table field '{1}'.",
                                               childFeldMetaData.Name, docFieldMetaData.Name));
                        }

                        tableFieldValue.Value = null;
                        tableFieldValue.RowNumber = rowIndex;
                        rowFieldValues.Add(tableFieldValue);
                        continue;
                        #endregion
                    }

                    haveValue = true;

                    // Data type is in upper case
                    switch (childFeldMetaData.DataType)
                    {
                        case "STRING":
                            #region
                            if (value.Length > childFeldMetaData.MaxLength)
                            {
                                throw new InvalidBatchDataException(string.Format("Value '{0}' is exceed max length {1} for data type String.", value, docFieldMetaData.MaxLength));
                            }

                            tableFieldValue.Value = value;
                            break;
                        #endregion

                        case "INTEGER":
                            #region
                            int tempInt;
                            if (int.TryParse(value, out tempInt))
                            {
                                tableFieldValue.Value = tempInt.ToString();
                            }
                            else
                            {
                                throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Integer.", value));
                            }

                            break;
                        #endregion

                        case "DECIMAL":
                            #region
                            decimal tempDecimal;
                            if (decimal.TryParse(value, out tempDecimal))
                            {
                                tableFieldValue.Value = tempDecimal.ToString();
                            }
                            else
                            {
                                throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Decimal.", value));
                            }

                            break;
                        #endregion

                        case "DATE":
                            #region
                            DateTime tempDate;
                            if (DateTime.TryParseExact(value.Substring(0, 10), "yyyy-MM-dd", null, DateTimeStyles.None, out tempDate))
                            {
                                tableFieldValue.Value = tempDate.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Date.", value));
                            }

                            break;
                        #endregion

                        default:
                            throw new InvalidBatchDataException(string.Format("Invalid data type {0} for content table field value.", docFieldMetaData.DataType));
                    }

                    tableFieldValue.RowNumber = rowIndex;
                    rowFieldValues.Add(tableFieldValue);
                }

                if (haveValue)
                {
                    tempTableFieldValues.AddRange(rowFieldValues);
                    rowIndex++;
                }

                rowFieldValues.Clear();
            }

            if (tempTableFieldValues.Count == 0 && docFieldMetaData.IsRequired)
            {
                throw new InvalidBatchDataException(
                    string.Format("Batch contains content '{0}' have no value for required content field '{1}'.",
                                    docName, docFieldMetaData.Name));
            }

            docFieldValue.TableFieldValue = tempTableFieldValues;
        }

        /// <summary>
        /// Validate normal batch field value, and standardize the value
        /// </summary>
        /// <param name="batchFieldValue">Batch field value</param>
        /// <param name="batchFieldMetaData">Normal batch field meta data</param>
        /// <remarks>If value is not valid for field data type, the <see cref="InvalidBatchDataException"/> will be throw</remarks>
        private void ValidateBatchFieldValueOld(BatchFieldValue batchFieldValue, BatchFieldMetaData batchFieldMetaData)
        {
            var value = string.Format("{0}", batchFieldValue.Value).Trim();
            var dataType = string.Format("{0}", batchFieldMetaData.DataType).ToUpper();

            if (value == string.Empty)
            {
                batchFieldValue.Value = null;
                return;
            }

            switch (dataType)
            {
                case "STRING":
                    #region
                    if (value.Length > batchFieldMetaData.MaxLength)
                    {
                        throw new InvalidBatchDataException(string.Format("Value '{0}' is exceed max length {1} for data type String.", value, batchFieldMetaData.MaxLength));
                    }

                    batchFieldValue.Value = value;
                    break;
                #endregion

                case "INTEGER":
                    #region
                    int tempInt;
                    if (int.TryParse(value, out tempInt))
                    {
                        batchFieldValue.Value = tempInt.ToString();
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Integer.", value));
                    }

                    break;
                #endregion

                case "DECIMAL":
                    #region
                    decimal tempDecimal;
                    if (decimal.TryParse(value, out tempDecimal))
                    {
                        batchFieldValue.Value = tempDecimal.ToString();
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Decimal.", value));
                    }

                    break;
                #endregion

                case "DATE":
                    #region
                    DateTime tempDate;
                    if (DateTime.TryParseExact(value.Substring(0, 10), "yyyy-MM-dd", null, DateTimeStyles.None, out tempDate))
                    {
                        batchFieldValue.Value = tempDate.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Date.", value));
                    }

                    break;
                #endregion

                case "BOOLEAN":
                    #region
                    bool tempBool;
                    if (bool.TryParse(value, out tempBool))
                    {
                        batchFieldValue.Value = tempBool.ToString();
                    }
                    else
                    {
                        throw new InvalidBatchDataException(string.Format("Invalid value '{0}' for data type Boolean.", value));
                    }

                    break;
                #endregion

                default:
                    throw new InvalidBatchDataException(string.Format("Invalid data type {0} for batch field value.", batchFieldMetaData.DataType));
            }
        }

        public void UpdateBatch(Batch workItem, bool isMoveNextStep, bool fromMobile = false)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    ValidateBatch(workItem, context);
                    BatchPermission permission = null;

                    if (LoginUser.IsAdmin)
                    {
                        permission = BatchPermission.GetAllowAll();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(workItem.BlockingBookmark))
                        {
                            if (!LoginUser.UserName.Equals(workItem.CreatedBy))
                            {
                                throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, workItem.Id));
                            }

                            permission = GetWorkItemDefaultPermission(LoginUser.Id, workItem.WorkflowDefinitionId, workItem.BatchTypeId, context);
                        }
                        else
                        {
                            permission = GetWorkItemPermission(LoginUser.Id, workItem.WorkflowDefinitionId, Guid.Parse(workItem.BlockingBookmark), context);

                            if (permission == null)
                            {
                                throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, workItem.Id));
                            }

                            if (!LoginUser.UserName.Equals(workItem.LockedBy))
                            {
                                throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, workItem.Id));
                            }
                        }
                    }

                    var batchDao = new BatchDao(context);
                    var dbBatch = batchDao.GetById(workItem.Id);
                    if (dbBatch == null)
                    {
                        //throw new BatchException(CaptureError.DB_BATCH_NULL);
                    }
                    // Check save batch is in the last active transaction id
                    if (dbBatch.TransactionId != workItem.TransactionId)
                    {
                        throw new InvalidTransactionIdException("Your batch transaction ID is different from last active transaction ID");
                    }

                    UpdateBatch(workItem, permission, context, fromMobile);

                    // Reset transaction id of batch
                    batchDao.UpdateTransactionId(batchId: workItem.Id, transactionId: Guid.NewGuid());

                    context.Commit();

                    if (isMoveNextStep)
                    {
                        CallWorkflow(workItem, false);
                    }

                }
                catch
                {
                    context.Rollback();
                    throw;
                }

            }
        }

        public void UpdateBatchAfterProcessBarcode(Batch workItem)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    UpdateBatchForBarcode(workItem, context);

                    context.Commit();

                }
                catch (Exception ex)
                {
                    context.Rollback();
                    throw ex;
                }

            }
        }

        public void UpdateBatchAfterProcessLookup(Batch workItem)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    UpdateBatchForLookup(workItem, context);
                    context.Commit();

                }
                catch (Exception)
                {
                    context.Rollback();
                }

            }
        }

        public void ApproveBatchs(List<Batch> batchs)
        {
            CommonValidator.CheckNull(batchs);
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                List<Batch> approvedBatchs = new List<Batch>();
                try
                {
                    context.BeginTransaction();
                    BatchDao batchDao = new BatchDao(context);
                    CommentDao commentDao = new CommentDao(context);
                    HistoryDao historyDao = new HistoryDao(context);
                    //List<Batch> batchs = batchDao.GetBatchByRange(ids);
                    List<Guid> userGroupIds;

                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    string message = GetBatchComment(BatchHistoryAction.ApprovedBatch) + " at step {0}";
                    bool canApprove = false;

                    if (LoginUser.IsAdmin)
                    {
                        approvedBatchs.AddRange(batchs);
                    }
                    else
                    {
                        foreach (Batch batch in batchs)
                        {
                            //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            CustomActivitySetting customSetting = new CustomActivitySettingDao(context).GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customSetting.Value);
                            canApprove = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanViewOtherItems);

                            if (canApprove)
                            {
                                batch.BatchType = new BatchTypeDao(context).GetById(batch.BatchTypeId);
                                approvedBatchs.Add(batch);
                            }
                        }
                    }

                    if (approvedBatchs.Count > 0)
                    {
                        foreach (Batch approvedBatch in approvedBatchs)
                        {
                            BatchPermission permission = null;

                            if (LoginUser.IsAdmin)
                            {
                                permission = BatchPermission.GetAllowAll();
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(approvedBatch.BlockingBookmark))
                                {
                                    if (!LoginUser.UserName.Equals(approvedBatch.CreatedBy))
                                    {
                                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, approvedBatch.Id));
                                    }

                                    permission = GetWorkItemDefaultPermission(LoginUser.Id, approvedBatch.WorkflowDefinitionId, approvedBatch.BatchTypeId, context);
                                }
                                else
                                {
                                    permission = GetWorkItemPermission(LoginUser.Id, approvedBatch.WorkflowDefinitionId, Guid.Parse(approvedBatch.BlockingBookmark), context);

                                    if (permission == null)
                                    {
                                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, approvedBatch.Id));
                                    }

                                    if (!LoginUser.UserName.Equals(approvedBatch.LockedBy) && !string.IsNullOrEmpty(approvedBatch.LockedBy))
                                    {
                                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, approvedBatch.Id));
                                    }
                                }
                            }

                            var dbBatch = batchDao.GetById(approvedBatch.Id);
                            if (dbBatch == null)
                            {
                                throw new Exception("Work item is have no longer existed.");
                            }
                            // Check save batch is in the last active transaction id
                            if (dbBatch.TransactionId != approvedBatch.TransactionId)
                            {
                                throw new InvalidTransactionIdException("Your batch transaction ID is different from last active transaction ID");
                            }

                            UpdateBatch(approvedBatch, permission, context);

                            approvedBatch.LockedBy = null;
                            approvedBatch.LastAccessedBy = LoginUser.UserName;
                            approvedBatch.LastAccessedDate = DateTime.Now;
                            batchDao.UpdateByWorkflow(approvedBatch);

                            //if (approvedBatch.Comments != null && approvedBatch.Comments.Count > 0)
                            //{
                            //    foreach(Comment comment in approvedBatch.Comments)
                            //    {
                            //        if (comment.Id == Guid.Empty)
                            //        {
                            //            comment.Id = Guid.NewGuid();
                            //            comment.InstanceId = approvedBatch.Id;
                            //            comment.IsBatchId = true;
                            //            commentDao.InsertComment(comment);
                            //        }
                            //    }
                            //}

                            ActionLogHelper.AddActionLog("Approved work item: " + approvedBatch.Id + " successfully",
                                            LoginUser, ActionName.ApprovedBatch, null, null, context);

                            History history = new History()
                            {
                                Action = BatchHistoryAction.ApprovedBatch.ToString(),
                                ActionDate = DateTime.Now,
                                BatchId = approvedBatch.Id,
                                CustomMsg = string.Format(message, approvedBatch.BlockingActivityName),
                                Id = Guid.NewGuid(),
                                WorkflowStep = approvedBatch.BlockingActivityName
                            };

                            new HistoryDao(context).InsertHistory(history);

                            batchDao.UpdateTransactionId(approvedBatch.Id, Guid.NewGuid());
                        }

                    }

                    context.Commit();
                    approvedBatchs.ForEach(p => CallWorkflow(p, false, null));
                }
                catch (Exception)
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        public void ApproveBatchs(List<Guid> ids)
        {
            CommonValidator.CheckNull(ids);
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                List<Batch> approvedBatchs = new List<Batch>();
                try
                {
                    context.BeginTransaction();
                    BatchDao batchDao = new BatchDao(context);
                    CommentDao commentDao = new CommentDao(context);
                    HistoryDao historyDao = new HistoryDao(context);
                    List<Batch> batchs = batchDao.GetBatchByRange(ids);
                    List<Guid> userGroupIds;

                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    string message = GetBatchComment(BatchHistoryAction.ApprovedBatch) + " at step {0}";
                    bool canApprove = false;

                    if (LoginUser.IsAdmin)
                    {
                        approvedBatchs.AddRange(batchs);
                    }
                    else
                    {
                        foreach (Batch batch in batchs)
                        {
                            //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            //canApprove = humanStepPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanViewOtherItems);
                            CustomActivitySetting customSetting = new CustomActivitySettingDao(context).GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customSetting.Value);
                            canApprove = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanViewOtherItems);

                            if (canApprove)
                            {
                                batch.BatchType = new BatchTypeDao(context).GetById(batch.BatchTypeId);
                                approvedBatchs.Add(batch);
                            }
                        }
                    }

                    if (approvedBatchs.Count > 0)
                    {
                        foreach (Batch approvedBatch in approvedBatchs)
                        {
                            approvedBatch.LockedBy = null;
                            approvedBatch.LastAccessedBy = LoginUser.UserName;
                            approvedBatch.LastAccessedDate = DateTime.Now;
                            approvedBatch.IsRejected = false;
                            batchDao.UpdateByWorkflow(approvedBatch);

                            ActionLogHelper.AddActionLog("Approved work item: " + approvedBatch.Id + " successfully",
                                            LoginUser, ActionName.ApprovedBatch, null, null, context);

                            History history = new History()
                            {
                                Action = BatchHistoryAction.ApprovedBatch.ToString(),
                                ActionDate = DateTime.Now,
                                BatchId = approvedBatch.Id,
                                CustomMsg = string.Format(message, approvedBatch.BlockingActivityName),
                                WorkflowStep = approvedBatch.BlockingActivityName
                            };

                            new HistoryDao(context).InsertHistory(history);

                            // Update new Transaction Id
                            batchDao.UpdateTransactionId(approvedBatch.Id, Guid.NewGuid());
                        }

                    }

                    context.Commit();
                    approvedBatchs.ForEach(p => CallWorkflow(p, false, null));
                }
                catch (Exception)
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        public void RejectBatchs(List<Batch> batchs, string rejectNote)
        {
            CommonValidator.CheckNull(batchs);
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    BatchDao batchDao = new BatchDao(context);
                    CommentDao commentDao = new CommentDao(context);
                    HistoryDao historyDao = new HistoryDao(context);
                    List<Batch> rejectItems = new List<Batch>();
                    string message = GetBatchComment(BatchHistoryAction.RejectedBatch) + " at step '{0}' with message '{1}'";
                    //List<Batch> batchs = batchDao.GetBatchByRange(ids);
                    List<Guid> userGroupIds;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    bool canReject = false;

                    if (LoginUser.IsAdmin)
                    {
                        rejectItems.AddRange(batchs);
                    }
                    else
                    {
                        foreach (Batch batch in batchs)
                        {
                            //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            //canReject = humanStepPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanReject);
                            CustomActivitySetting customSetting = new CustomActivitySettingDao(context).GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customSetting.Value);
                            canReject = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanReject);

                            if (canReject)
                            {
                                rejectItems.Add(batch);
                            }
                        }
                    }

                    if (rejectItems.Count > 0)
                    {
                        foreach (Batch rejectedBatch in rejectItems)
                        {
                            BatchPermission permission = null;

                            if (LoginUser.IsAdmin)
                            {
                                permission = BatchPermission.GetAllowAll();
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(rejectedBatch.BlockingBookmark))
                                {
                                    if (!LoginUser.UserName.Equals(rejectedBatch.CreatedBy))
                                    {
                                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, rejectedBatch.Id));
                                    }

                                    permission = GetWorkItemDefaultPermission(LoginUser.Id, rejectedBatch.WorkflowDefinitionId, rejectedBatch.BatchTypeId, context);
                                }
                                else
                                {
                                    permission = GetWorkItemPermission(LoginUser.Id, rejectedBatch.WorkflowDefinitionId, Guid.Parse(rejectedBatch.BlockingBookmark), context);

                                    if (permission == null)
                                    {
                                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, rejectedBatch.Id));
                                    }

                                    if (!LoginUser.UserName.Equals(rejectedBatch.LockedBy))
                                    {
                                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, rejectedBatch.Id));
                                    }
                                }
                            }

                            UpdateBatch(rejectedBatch, permission, context);

                            rejectedBatch.IsRejected = true;
                            rejectedBatch.LockedBy = null;
                            rejectedBatch.LastAccessedBy = LoginUser.UserName;
                            rejectedBatch.LastAccessedDate = DateTime.Now;
                            batchDao.Update(rejectedBatch);
                            ActionLogHelper.AddActionLog("Rejected work item: " + rejectedBatch.Id + " successfully",
                                                    LoginUser, ActionName.RejectedBatch, null, null, context);

                            //Comment comment = new Comment()
                            //{
                            //    CreatedBy = LoginUser.UserName,
                            //    CreatedDate = DateTime.Now,
                            //    InstanceId = rejectedBatch.Id,
                            //    IsBatchId = true,
                            //    Note = string.Format(message, rejectedBatch.BlockingActivityName, rejectNote)
                            //};

                            //new CommentDao(context).InsertComment(comment);

                            History history = new History()
                            {
                                Action = BatchHistoryAction.RejectedBatch.ToString(),
                                ActionDate = DateTime.Now,
                                BatchId = rejectedBatch.Id,
                                CustomMsg = string.Format(message, rejectedBatch.BlockingActivityName, rejectNote),
                                WorkflowStep = rejectedBatch.BlockingActivityName
                            };

                            new HistoryDao(context).InsertHistory(history);

                            batchDao.UpdateTransactionId(rejectedBatch.Id, Guid.NewGuid());
                        }

                    }

                    context.Commit();
                    rejectItems.ForEach(p => CallWorkflow(p, false, null));
                }
                catch
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        public void RejectBatchs(List<Guid> ids, string rejectNote)
        {
            CommonValidator.CheckNull(ids);
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    BatchDao batchDao = new BatchDao(context);
                    DocumentDao docDao = new DocumentDao(context);
                    PageDao pageDao = new PageDao(context);
                    CommentDao commentDao = new CommentDao(context);
                    HistoryDao historyDao = new HistoryDao(context);
                    List<Batch> rejectItems = new List<Batch>();
                    string message = GetBatchComment(BatchHistoryAction.RejectedBatch) + " at step '{0}' with message '{1}'";
                    List<Batch> batchs = batchDao.GetBatchByRange(ids);
                    List<Guid> userGroupIds;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    bool canReject = false;

                    if (LoginUser.IsAdmin)
                    {
                        rejectItems.AddRange(batchs);
                    }
                    else
                    {
                        foreach (Batch batch in batchs)
                        {
                            //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            //canReject = humanStepPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanReject);
                            CustomActivitySetting customSetting = new CustomActivitySettingDao(context).GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customSetting.Value);
                            canReject = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanReject);

                            if (canReject)
                            {
                                rejectItems.Add(batch);
                            }
                        }
                    }

                    if (rejectItems.Count > 0)
                    {
                        foreach (Batch rejectedBatch in rejectItems)
                        {
                            rejectedBatch.IsRejected = true;
                            rejectedBatch.LockedBy = null;
                            rejectedBatch.LastAccessedBy = LoginUser.UserName;
                            rejectedBatch.LastAccessedDate = DateTime.Now;
                            batchDao.Update(rejectedBatch);

                            ActionLogHelper.AddActionLog("Rejected work item: " + rejectedBatch.Id + " successfully",
                                                    LoginUser, ActionName.RejectedBatch, null, null, context);

                            Comment comment = new Comment()
                            {
                                CreatedBy = LoginUser.UserName,
                                CreatedDate = DateTime.Now,
                                InstanceId = rejectedBatch.Id,
                                IsBatchId = true,
                                Note = string.Format(message, rejectedBatch.BlockingActivityName, rejectNote)
                            };

                            new CommentDao(context).InsertComment(comment);

                            History history = new History()
                            {
                                Action = BatchHistoryAction.RejectedBatch.ToString(),
                                ActionDate = DateTime.Now,
                                BatchId = rejectedBatch.Id,
                                CustomMsg = string.Format(message, rejectedBatch.BlockingActivityName, rejectNote),
                                WorkflowStep = rejectedBatch.BlockingActivityName
                            };

                            new HistoryDao(context).InsertHistory(history);

                            batchDao.UpdateTransactionId(rejectedBatch.Id, Guid.NewGuid());
                        }

                    }

                    context.Commit();
                    rejectItems.ForEach(p => CallWorkflow(p, false, null));
                }
                catch (Exception)
                {
                    context.Rollback();
                }
            }
        }


        /// <summary>
        /// Approve, Reject, Save opened batch
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="rejected"></param>
        /// <param name="rejectNote"></param>
        /// <param name="isSave"></param>
        private void SubmitBatchs(Batch batch, bool rejected, string rejectNote, bool isSave)
        {
            //if (batch == null)
            //{
            //    throw new ArgumentNullException(nameof(batch));
            //}

            //using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            //{
            //    context.BeginTransaction();

            //    var batchDao = new BatchDao(context);
            //    var dbBatch = batchDao.GetById(batch.Id);

            //    var workingDateNow = DateTime.Now;
            //    int workingIndex;
            //    var orderDocIndex = 0;
            //    string workingDbValue;
            //    string workingValue;
            //    string workingType;
            //    int tempInt;
            //    decimal tempDecimal;
            //    DateTime tempDateTime;
            //    bool tempBool;
            //    Guid blockingBookmarkId;
            //    bool flgAddLog;

            //    #region Check valid batch
            //    if (dbBatch == null)
            //    {
            //        throw new CaptureException(CaptureError.NullDbBatch);
            //    }
            //    else if (dbBatch.IsCompleted)
            //    {
            //        throw new CaptureException(CaptureError.BatchIsCompleted);
            //    }
            //    else if (dbBatch.IsProcessing)
            //    {
            //        throw new CaptureException(CaptureError.BatchIsProcessing);
            //    }
            //    else if (dbBatch.HasError
            //                || string.IsNullOrWhiteSpace(dbBatch.BlockingBookmark)
            //                || Guid.TryParse(dbBatch.BlockingBookmark, out blockingBookmarkId))
            //    {
            //        throw new CaptureException(CaptureError.BatchHasError);
            //    }
            //    else if (dbBatch.LockedBy != null
            //                && !dbBatch.LockedBy.Equals(LoginUser.UserName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        throw new CaptureException(CaptureError.BatchLockedByOther);
            //    }
            //    else if (dbBatch.TransactionId != batch.TransactionId)
            //    {
            //        throw new CaptureException(CaptureError.InvalidTransaction);
            //    }
            //    #endregion

            //    BatchPermission permission = null;
            //    #region Get batch permission
            //    if (LoginUser.IsAdmin)
            //    {
            //        permission = BatchPermission.GetAllowAll();
            //    }
            //    else
            //    {
            //        permission = GetWorkItemPermission(LoginUser.Id,
            //                                            dbBatch.WorkflowDefinitionId,
            //                                            blockingBookmarkId,
            //                                            context);
            //        if (permission == null)
            //        {
            //            throw new CaptureException(CaptureError.NullPermmission);
            //        }
            //    }
            //    #endregion

            //    // Batch index
            //    if (permission.CanModifyIndexes && batch.FieldValues != null && batch.FieldValues.Count > 0)
            //    {
            //        #region

            //        // Get batch field meta data
            //        var batchMetaDao = new BatchFieldMetaDataDao(context);
            //        var batchMetaDatas = batchMetaDao.GetByBatchType(dbBatch.BatchTypeId);
            //        // Check valid
            //        if (batchMetaDatas == null || batchMetaDatas.Count == 0)
            //        {
            //            throw new CaptureException(CaptureError.NullBatchFieldMetaData);
            //        }

            //        // Get DB batch field value
            //        var batchValueDao = new BatchFieldValueDao(context);
            //        var dbBatchValues = batchValueDao.GetByBatch(dbBatch.Id);

            //        // Check valid
            //        #region
            //        if (dbBatchValues == null || dbBatchValues.Count == 0)
            //        {
            //            throw new CaptureException(CaptureError.NullDbBatchFieldValues);
            //        }
            //        else if (dbBatchValues.Count != batchMetaDatas.Count)
            //        {
            //            throw new CaptureException(CaptureError.DiffDbBatchFieldValues);
            //        }
            //        else if (dbBatchValues.Count != batch.FieldValues.Count)
            //        {
            //            throw new CaptureException(CaptureError.DiffDbBatchFieldValues);
            //        }
            //        #endregion

            //        BatchFieldValue batchValue;
            //        BatchFieldValue dbBatchValue;
            //        flgAddLog = false;

            //        foreach (var batchMeta in batchMetaDatas)
            //        {
            //            #region
            //            // Get submit batch field value
            //            batchValue = batch.FieldValues.FirstOrDefault(h => h.FieldId == batchMeta.Id);
            //            if (batchValue == null)
            //            {
            //                throw new CaptureException(CaptureError.NullBatchFieldValue);
            //            }

            //            // Standardize value
            //            workingValue = string.Format("{0}", batchValue.Value).Trim();
            //            workingType = string.Format("{0}", batchMeta.DataType).Trim().ToUpper();

            //            if (workingValue == string.Empty)
            //            {
            //                workingValue = null;
            //            }
            //            else
            //            {
            //                #region
            //                switch (workingType)
            //                {
            //                    case "STRING":
            //                        #region
            //                        if (workingValue.Length > batchMeta.MaxLength)
            //                        {
            //                            throw new CaptureException(CaptureError.BatchFieldValueMaxLegth);
            //                        }
            //                        break;
            //                    #endregion

            //                    case "INTEGER":
            //                        #region
            //                        if (int.TryParse(workingValue, out tempInt))
            //                        {
            //                            workingValue = tempInt.ToString();
            //                        }
            //                        else
            //                        {
            //                            throw new CaptureException(CaptureError.InvalidBatchValueInteger);
            //                        }
            //                        break;
            //                    #endregion

            //                    case "DECIMAL":
            //                        #region
            //                        if (decimal.TryParse(workingValue, out tempDecimal))
            //                        {
            //                            workingValue = tempDecimal.ToString();
            //                        }
            //                        else
            //                        {
            //                            throw new CaptureException(CaptureError.InvalidBatchValueDecimal);
            //                        }
            //                        break;
            //                    #endregion

            //                    case "DATE":
            //                        #region

            //                        if (DateTime.TryParseExact(workingValue.PadRight(10).Substring(0, 10),
            //                                                    "yyyy-MM-dd", null, DateTimeStyles.None,
            //                                                    out tempDateTime))
            //                        {
            //                            workingValue = tempDateTime.ToString("yyyy-MM-dd");
            //                        }
            //                        else
            //                        {
            //                            throw new CaptureException(CaptureError.InvalidBatchValueDate);
            //                        }
            //                        break;
            //                    #endregion

            //                    case "BOOLEAN":
            //                        #region
            //                        if (bool.TryParse(workingValue, out tempBool))
            //                        {
            //                            workingValue = tempBool.ToString();
            //                        }
            //                        else
            //                        {
            //                            throw new CaptureException(CaptureError.InvalidBatchValueBool);
            //                        }
            //                        break;
            //                    #endregion

            //                    default:
            //                        throw new CaptureException(CaptureError.InvalidBatchFieldValueDataType);
            //                }
            //                #endregion
            //            }

            //            // Get db batch field value to compare modify
            //            dbBatchValue = dbBatchValues.FirstOrDefault(h => h.FieldId == batchMeta.Id);
            //            if (dbBatchValue == null)
            //            {
            //                throw new CaptureException(CaptureError.NullDbBatchFieldValue);
            //            }

            //            workingDbValue = string.Format("{0}", dbBatchValue.Value).Trim();
            //            if (workingDbValue == string.Empty)
            //            {
            //                workingDbValue = null;
            //            }

            //            // Update new batch field value
            //            if (workingDbValue != dbBatchValue.Value)
            //            {
            //                flgAddLog = true;
            //                dbBatchValue.Value = workingValue;
            //                batchValueDao.Update(dbBatchValue);
            //            }

            //            #endregion
            //        }

            //        if (flgAddLog)
            //        {
            //            ActionLogHelper.AddActionLog("Update batch field value for batch: " + dbBatch.Id,
            //                                            LoginUser, ActionName.UpdateBatchFieldValue, null, null,
            //                                            context, workingDateNow);
            //        }

            //        #endregion
            //    }

            //    // Batch comment
            //    if (batch.Comments != null && batch.Comments.Count > 0)
            //    {
            //        #region
            //        workingIndex = 0;
            //        var commentDao = new CommentDao(context);

            //        foreach (var comment in batch.Comments)
            //        {
            //            #region
            //            if (comment.Id != Guid.Empty)
            //            {
            //                continue;
            //            }

            //            // Check null note
            //            workingValue = string.Format("{0}", comment.Note).Trim();
            //            if (workingValue == string.Empty)
            //            {
            //                throw new CaptureException(CaptureError.NullComment);
            //            }

            //            comment.Id = Guid.NewGuid();
            //            comment.Note = workingValue;
            //            comment.CreatedBy = LoginUser.UserName;
            //            comment.CreatedDate = workingDateNow.AddSeconds(workingIndex++);
            //            comment.InstanceId = dbBatch.Id;
            //            comment.IsBatchId = true;
            //            commentDao.InsertComment(comment);

            //            ActionLogHelper.AddActionLog("Add comment for batch: " + dbBatch.Id,
            //                                            LoginUser, ActionName.AddComment, null, null, context,
            //                                            comment.CreatedDate);
            //            #endregion
            //        }
            //        #endregion
            //    }

            //    // Get list db document of batch
            //    var documentDao = new DocumentDao(context);
            //    var dbDocuments = documentDao.GetByBatch(dbBatch.Id);
            //    if (dbDocuments == null || dbDocuments.Count == 0)
            //    {
            //        throw new CaptureException(CaptureError.NullDbDocuments);
            //    }

            //    // Delete document
            //    if (permission.CanModifyDocument && batch.DeletedDocuments != null && batch.DeletedDocuments.Count > 0)
            //    {
            //        #region
            //        // Delete document and its related value (document field, page, anno...)
            //        foreach (var deleteDocumentId in batch.DeletedLooseDocuments)
            //        {
            //            // Get index of delete doc
            //            workingIndex = dbDocuments.FindIndex(h => h.Id == deleteDocumentId);
            //            if (workingIndex < 0)
            //            {
            //                throw new CaptureException(CaptureError.InvalidDeleteDoc);
            //            }

            //            // Remove this document from list update doc
            //            dbDocuments[workingIndex] = null;
            //            // Delete doc
            //            documentDao.DeleteRecur(deleteDocumentId);

            //            ActionLogHelper.AddActionLog(string.Format("Delete document: {0} for batch: {1}",
            //                                                        deleteDocumentId, dbBatch.Id),
            //                                            LoginUser, ActionName.AddComment, null, null, context,
            //                                            workingDateNow);
            //        }
            //        #endregion
            //    }

            //    // Check have submit document
            //    if (permission.CanModifyDocument && batch.Documents == null || batch.Documents.Count > 0)
            //    {
            //        var tableValueDao = new TableFieldValueDao(context);
            //        var docFieldValueDao = new DocumentFieldValueDao(context);
            //        var docFieldMetaDao = new DocFieldMetaDataDao(context);
            //        var picklistDao = new PicklistDao(context);
            //        var dicDocFieldMeta = new Dictionary<Guid, List<DocumentFieldMetaData>>();
            //        DocumentFieldValue docValue;
            //        DocumentFieldValue dbDocValue;

            //        // Loop in submit documents
            //        foreach (var document in batch.Documents.OrderBy(h => h.Order))
            //        {
            //            // Get document field meta data
            //            if (!dicDocFieldMeta.ContainsKey(document.DocTypeId))
            //            {
            //                #region
            //                var dbDocFieldMetas = docFieldMetaDao.GetByDocType(document.DocTypeId);
            //                if (dbDocFieldMetas == null || dbDocFieldMetas.Count == 0)
            //                {
            //                    throw new CaptureException(CaptureError.NullDocFieldMetaDatas);
            //                }

            //                foreach (var dbDocFieldMeta in dbDocFieldMetas)
            //                {
            //                    #region
            //                    dbDocFieldMeta.DataType = string.Format("{0}", dbDocFieldMeta.DataType)
            //                                                                           .Trim().ToUpper();

            //                    if (dbDocFieldMeta.DataType == "PICKLIST")
            //                    {
            //                        #region
            //                        var picklists = picklistDao.GetByField(dbDocFieldMeta.Id);
            //                        if (picklists == null || picklists.Count == 0)
            //                        {
            //                            throw new CaptureException(CaptureError.NullDocFieldMetaPicklists);
            //                        }
            //                        foreach (var picklist in picklists)
            //                        {
            //                            picklist.Value = string.Format("{0}", picklist.Value).Trim();
            //                        }
            //                        dbDocFieldMeta.Picklists = picklists;
            //                        #endregion
            //                    }
            //                    // Case table => get children field
            //                    else if (dbDocFieldMeta.DataType == "TABLE")
            //                    {
            //                        #region
            //                        var children = docFieldMetaDao.GetChildren(dbDocFieldMeta.Id);
            //                        if (children == null || children.Count == 0)
            //                        {
            //                            throw new CaptureException(CaptureError.NullDocFieldMetaChildren);
            //                        }

            //                        foreach (var child in children)
            //                        {
            //                            child.DataType = string.Format("{0}", child.DataType).Trim().ToUpper();
            //                        }

            //                        dbDocFieldMeta.Children = children;
            //                        #endregion
            //                    }
            //                    #endregion
            //                }
            //                #endregion
            //            }

            //            // Case create new document
            //            if (document.Id == Guid.Empty)
            //            {
            //                // Create new doc id
            //                document.Id = Guid.NewGuid();

            //                // Document index
            //                #region
            //                if (document.FieldValues == null || document.FieldValues.Count == 0)
            //                {
            //                    throw new CaptureException(CaptureError.NullDocFieldValues);
            //                }

            //                foreach (var docMeta in dicDocFieldMeta[document.DocTypeId])
            //                {
            //                    #region
            //                    // Get submit doc field value
            //                    docValue = document.FieldValues.FirstOrDefault(h => h.FieldId == docMeta.Id);
            //                    if (docValue == null)
            //                    {
            //                        throw new CaptureException(CaptureError.NullDocFieldValue);
            //                    }

            //                    // Standardize value
            //                    workingValue = string.Format("{0}", docValue.Value).Trim();

            //                    // Valid table
            //                    if (docMeta.DataType == "Table")
            //                    {
            //                        workingValue = string.Empty;
            //                        ValidateTableFieldValue(docMeta, docValue);
            //                    }
            //                    // Validate other data type difference Table
            //                    else
            //                    {
            //                        workingValue = ValidateDocFieldValue(docMeta, workingValue);
            //                    }

            //                    // Add doc value
            //                    docValue.Value = workingValue == string.Empty ? null : workingValue;
            //                    docValue.DocId = document.Id;
            //                    docFieldValueDao.Add(docValue);

            //                    // Add table value
            //                    if (docValue.TableFieldValue != null && docValue.TableFieldValue.Count > 0)
            //                    {
            //                        foreach (var tableValue in docValue.TableFieldValue)
            //                        {
            //                            tableValue.DocId = document.Id;
            //                            tableValue.FieldId = docValue.Id;
            //                            tableValueDao.Add(tableValue);
            //                        }
            //                    }

            //                    #endregion
            //                }
            //                #endregion

            //                // Page
            //                if (document.Pages == null || document.Pages.Count == 0)
            //                {
            //                    throw new CaptureException(CaptureError.NullPages);
            //                }

            //                foreach (var page in document.Pages)
            //                {
            //                }


            //                // Add document
            //                #region
            //                document.CreatedBy = LoginUser.UserName;
            //                document.CreatedDate = workingDateNow;

            //                document.Order = orderDocIndex++;
            //                documentDao.Add(document, document.Id);
            //                #endregion

            //            }
            //            // Case update document
            //            else
            //            {
            //                #region

            //                #endregion
            //            }
            //        }
            //    }




            //    context.Commit();
            //}
        }



        public void ResumeBatchs(List<Guid> ids)
        {
            CommonValidator.CheckNull(ids);
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                BatchDao batchDao = new BatchDao(context);
                List<Batch> batchs = batchDao.GetBatchByRange(ids);
                foreach (Batch batch in batchs)
                {
                    batchDao.UpdateTransactionId(batch.Id, Guid.NewGuid());
                    CallWorkflow(batch, false, null);
                }
            }
        }

        public void DeleteBatchs(List<Guid> ids)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    DeleteBatchs(ids, context);
                    context.Commit();
                }
                catch (Exception)
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        public void UnLockBatchs(List<Guid> ids)
        {
            ChangeLock(ids, false);
        }

        public void LockBatchs(List<Guid> ids)
        {
            ChangeLock(ids, true);
        }

        public Batch CheckLock(Guid id)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                return new BatchDao(context).CheckLock(id);
            }
        }

        public void DelegateBatch(List<Guid> ids, string toUser, string delegatedComment)
        {
            CommonValidator.CheckNull(ids);
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    BatchDao batchDao = new BatchDao(context);
                    CommentDao commentDao = new CommentDao(context);
                    HistoryDao historyDao = new HistoryDao(context);
                    List<Batch> delegatedItems = new List<Batch>();
                    string message = GetBatchComment(BatchHistoryAction.DelegateBatch) + " at step '{0}' with message '{1}'";
                    List<Batch> batchs = batchDao.GetBatchByRange(ids);
                    List<Guid> userGroupIds;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    bool canDelegate = false;

                    if (LoginUser.IsAdmin)
                    {
                        delegatedItems.AddRange(batchs);
                    }
                    else
                    {
                        foreach (Batch batch in batchs)
                        {
                            //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            //canDelegate = humanStepPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanDelegateItems);
                            CustomActivitySetting customSetting = new CustomActivitySettingDao(context).GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customSetting.Value);
                            canDelegate = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanDelegateItems);

                            if (canDelegate)
                            {
                                delegatedItems.Add(batch);
                            }
                        }
                    }

                    if (delegatedItems.Count > 0)
                    {
                        foreach (Batch delegatedBatch in delegatedItems)
                        {
                            delegatedBatch.DelegatedBy = LoginUser.UserName;
                            delegatedBatch.DelegatedTo = toUser;
                            delegatedBatch.LastAccessedBy = LoginUser.UserName;
                            delegatedBatch.LastAccessedDate = DateTime.Now;
                            delegatedBatch.BatchType = new BatchTypeDao(context).GetById(delegatedBatch.BatchTypeId);

                            if (!string.IsNullOrEmpty(delegatedBatch.LockedBy) || !string.IsNullOrWhiteSpace(delegatedBatch.LockedBy))
                            {
                                delegatedBatch.LockedBy = null;
                            }

                            batchDao.Update(delegatedBatch);

                            Comment comment = new Comment()
                            {
                                CreatedBy = LoginUser.UserName,
                                CreatedDate = DateTime.Now,
                                InstanceId = delegatedBatch.Id,
                                IsBatchId = true,
                                Note = string.Format(message, delegatedBatch.BlockingActivityName, delegatedComment)
                            };

                            new CommentDao(context).InsertComment(comment);

                            History history = new History()
                            {
                                Action = BatchHistoryAction.DelegateBatch.ToString(),
                                ActionDate = DateTime.Now,
                                BatchId = delegatedBatch.Id,
                                CustomMsg = string.Format(message, delegatedBatch.BlockingActivityName, delegatedComment),
                                WorkflowStep = delegatedBatch.BlockingActivityName
                            };

                            new HistoryDao(context).InsertHistory(history);

                            ActionLogHelper.AddActionLog("Delegated batch for user: " + toUser,
                                                        LoginUser, ActionName.DelegatedBatch, null, null, context);
                        }

                    }

                    context.Commit();
                }
                catch (Exception)
                {
                    context.Rollback();
                }
            }
        }

        public List<Batch> GetBatchs(List<Guid> ids)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                List<Batch> batchs = new BatchDao(context).GetBatchByRange(ids);

                foreach (Batch item in batchs)
                {
                    FillBatchPermission(new List<Batch> { item }, context);
                    //FillPermission(item);
                }

                return batchs;
            }
        }

        public List<Batch> GetBatchs(Guid batchTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                List<Batch> batchs = null;
                List<Batch> returnBatchs = new List<Batch>();
                List<Guid> groupIds = null;
                BatchDao batchDao = new BatchDao(dataContext);
                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(dataContext);
                DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);
                PageDao pageDao = new PageDao(dataContext);
                AnnotationDao annoDao = new AnnotationDao(dataContext);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);

                if (LoginUser.IsAdmin)
                {
                    batchs = batchDao.GetByBatchType(batchTypeId);
                }
                else
                {
                    using (Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    batchs = batchDao.GetByBatchType(batchTypeId, groupIds);
                }

                if (batchs != null && batchs.Count > 0)
                {
                    foreach (Batch item in batchs)
                    {
                        item.Comments = new CommentDao(dataContext).GetByInstance(item.Id);
                        item.FieldValues = new BatchFieldValueDao(dataContext).GetByBatch(item.Id);
                        item.Documents = new DocumentDao(dataContext).GetByBatch(item.Id);
                        item.BatchType = new BatchTypeDao(dataContext).GetById(item.BatchTypeId);
                        item.BatchType.DocTypes = new DocumentTypeDao(dataContext).GetDocumentTypeByBatch(item.BatchTypeId);

                        foreach (DocumentType docType in item.BatchType.DocTypes)
                        {
                            docType.Fields = docFieldMetaDataDao.GetByDocType(docType.Id);
                            docType.Fields.ForEach(p => p.Children = docFieldMetaDataDao.GetChildren(p.Id));
                        }

                        foreach (var fieldValue in item.FieldValues)
                        {
                            fieldValue.FieldMetaData = batchFieldMetaDataDao.GetById(fieldValue.FieldId);

                            if (fieldValue.FieldMetaData.IsLookup && !string.IsNullOrEmpty(fieldValue.FieldMetaData.LookupXml))
                            {
                                fieldValue.FieldMetaData.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(fieldValue.FieldMetaData.LookupXml);
                            }
                        }

                        Setting setting = new SettingManager(LoginUser).GetSettings();

                        foreach (var doc in item.Documents)
                        {
                            if (doc.DocTypeId != Guid.Empty)
                            {

                                doc.FieldValues = docFieldValueDao.GetByDoc(doc.Id);
                                doc.DocumentType = docTypeDao.GetById(doc.DocTypeId);
                                doc.DocumentType.Fields = docFieldMetaDataDao.GetByDocType(doc.DocTypeId);
                                //FillDocumentTypePermission(doc.DocumentType, dataContext);

                                foreach (var fieldValue in doc.FieldValues)
                                {
                                    fieldValue.FieldMetaData = docFieldMetaDataDao.GetById(fieldValue.FieldId);

                                    if (fieldValue.FieldMetaData.DataType == "Table")
                                    {
                                        fieldValue.FieldMetaData.Children = docFieldMetaDataDao.GetChildren(fieldValue.FieldId);
                                        fieldValue.TableFieldValue = new TableFieldValueDao(dataContext).GetByParentField(fieldValue.FieldId, doc.Id);

                                        foreach (var tableFieldValue in fieldValue.TableFieldValue)
                                        {
                                            tableFieldValue.Field = docFieldMetaDataDao.GetChild(tableFieldValue.FieldId);
                                        }

                                    }

                                    if (fieldValue.FieldMetaData.DataTypeEnum == FieldDataType.Picklist)
                                    {
                                        fieldValue.FieldMetaData.Picklists = picklistDao.GetByField(fieldValue.FieldMetaData.Id);
                                    }

                                    if (fieldValue.FieldMetaData.IsLookup && !string.IsNullOrEmpty(fieldValue.FieldMetaData.LookupXml))
                                    {
                                        fieldValue.FieldMetaData.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(fieldValue.FieldMetaData.LookupXml);
                                    }

                                }
                            }

                            doc.Pages = pageDao.GetByDoc(doc.Id);

                            foreach (Page page in doc.Pages)
                            {
                                if (setting.IsSaveFileInFolder)
                                {
                                    page.FileBinary = FileHelpper.ReadFile(page.FilePath, page.FileHeader);
                                }

                                page.Annotations = annoDao.GetByPage(page.Id);
                            }
                        }

                        FillBatchPermission(new List<Batch> { item }, dataContext);
                        //FillPermission(item);

                    }
                }

                return batchs;
            }
        }

        public Batch GetBatch(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                var batchDao = new BatchDao(dataContext);
                Batch item = batchDao.GetById(id);

                if (item == null)
                {
                    throw new Exception("Work item is have no longer existed.");
                }

                ActionLogHelper.AddActionLog("Get batch Id: " + item.Id,
                              LoginUser, ActionName.GetBatchItem, null, null, dataContext);

                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(dataContext);
                DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);
                PageDao pageDao = new PageDao(dataContext);
                AnnotationDao annoDao = new AnnotationDao(dataContext);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(dataContext);

                item.BatchType = new BatchTypeDao(dataContext).GetById(item.BatchTypeId);
                item.BatchType.Fields = batchFieldMetaDataDao.GetByBatchType(item.BatchType.Id);

                if (!LoginUser.IsAdmin)
                {
                    bool hasPermission = CheckPermission(LoginUser.Id, item, dataContext);

                    if (!hasPermission)
                    {
                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, id));
                    }
                }

                item.Comments = new CommentDao(dataContext).GetByInstance(item.Id);
                item.FieldValues = new BatchFieldValueDao(dataContext).GetByBatch(item.Id);
                item.Documents = new DocumentDao(dataContext).GetByBatch(item.Id);
                item.BatchType.DocTypes = new DocumentTypeDao(dataContext).GetDocumentTypeByBatch(item.BatchTypeId);

                foreach (DocumentType docType in item.BatchType.DocTypes)
                {
                    docType.Fields = docFieldMetaDataDao.GetByDocType(docType.Id);
                    docType.Fields.ForEach(p => p.Children = docFieldMetaDataDao.GetChildren(p.Id));
                }

                foreach (var field in item.BatchType.Fields)
                {
                    BatchFieldValue fieldValue = item.FieldValues.SingleOrDefault(p => p.FieldId == field.Id);

                    if (fieldValue == null)
                    {
                        fieldValue = new BatchFieldValue() { FieldId = field.Id, BatchId = item.Id, FieldMetaData = field };
                        item.FieldValues.Add(fieldValue);
                    }
                }

                foreach (var fieldValue in item.FieldValues)
                {
                    fieldValue.FieldMetaData = batchFieldMetaDataDao.GetById(fieldValue.FieldId);

                    if (fieldValue.FieldMetaData.IsLookup && !string.IsNullOrEmpty(fieldValue.FieldMetaData.LookupXml))
                    {
                        fieldValue.FieldMetaData.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(fieldValue.FieldMetaData.LookupXml);
                    }
                }

                Setting setting = new SettingManager(LoginUser).GetSettings();

                foreach (var doc in item.Documents)
                {
                    if (doc.DocTypeId != Guid.Empty)
                    {

                        doc.FieldValues = docFieldValueDao.GetByDoc(doc.Id);
                        doc.DocumentType = docTypeDao.GetById(doc.DocTypeId);
                        doc.DocumentType.Fields = docFieldMetaDataDao.GetByDocType(doc.DocTypeId);
                        //FillDocumentTypePermission(doc.DocumentType, dataContext);

                        doc.EmbeddedPictures = new OutlookPictureDao(dataContext).GetPictures(doc.Id);

                        foreach (var fieldValue in doc.FieldValues)
                        {
                            fieldValue.FieldMetaData = docFieldMetaDataDao.GetById(fieldValue.FieldId);

                            if (fieldValue.FieldMetaData.DataType == "Table")
                            {
                                fieldValue.FieldMetaData.Children = docFieldMetaDataDao.GetChildren(fieldValue.FieldId);
                                fieldValue.TableFieldValue = new TableFieldValueDao(dataContext).GetByParentField(fieldValue.FieldId, doc.Id);

                                foreach (var tableFieldValue in fieldValue.TableFieldValue)
                                {
                                    tableFieldValue.Field = docFieldMetaDataDao.GetChild(tableFieldValue.FieldId);
                                }

                            }

                            if (fieldValue.FieldMetaData.DataTypeEnum == FieldDataType.Picklist)
                            {
                                fieldValue.FieldMetaData.Picklists = picklistDao.GetByField(fieldValue.FieldMetaData.Id);
                            }

                            if (fieldValue.FieldMetaData.IsLookup && !string.IsNullOrEmpty(fieldValue.FieldMetaData.LookupXml))
                            {
                                fieldValue.FieldMetaData.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(fieldValue.FieldMetaData.LookupXml);
                            }

                        }
                    }

                    doc.Pages = pageDao.GetByDoc(doc.Id);

                    foreach (Page page in doc.Pages)
                    {
                        if (!string.IsNullOrEmpty(page.FilePath) && page.FileHeader != null)
                        {
                            page.FileBinary = FileHelpper.ReadFile(page.FilePath, page.FileHeader);
                        }

                        page.Annotations = annoDao.GetByPage(page.Id);
                    }
                }

                FillBatchPermission(new List<Batch> { item }, dataContext);
                //FillPermission(item);

                ActionLogHelper.AddActionLog("Get batch data on batch Id: " + item.Id,
                                  LoginUser, ActionName.GetBatchData, null, null, dataContext);
                return item;
            }
        }

        private Batch GetBatchData(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                Batch item = new BatchDao(dataContext).GetById(id);

                ActionLogHelper.AddActionLog("Get batch Id: " + item.Id,
                              LoginUser, ActionName.GetBatchItem, null, null, dataContext);

                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);

                item.BatchType = new BatchTypeDao(dataContext).GetById(item.BatchTypeId);
                item.BatchType.Fields = batchFieldMetaDataDao.GetByBatchType(item.BatchType.Id);

                if (!LoginUser.IsAdmin)
                {
                    bool hasPermission = CheckPermission(LoginUser.Id, item, dataContext);

                    if (!hasPermission)
                    {
                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, id));
                    }
                }

                item.Comments = new CommentDao(dataContext).GetByInstance(item.Id);
                item.FieldValues = new BatchFieldValueDao(dataContext).GetByBatch(item.Id);
                item.BatchType.DocTypes = new DocumentTypeDao(dataContext).GetDocumentTypeByBatch(item.BatchTypeId);

                foreach (var field in item.BatchType.Fields)
                {
                    BatchFieldValue fieldValue = item.FieldValues.SingleOrDefault(p => p.FieldId == field.Id);

                    if (fieldValue == null)
                    {
                        fieldValue = new BatchFieldValue() { FieldId = field.Id, BatchId = item.Id, FieldMetaData = field };
                        item.FieldValues.Add(fieldValue);
                    }
                }

                foreach (var fieldValue in item.FieldValues)
                {
                    fieldValue.FieldMetaData = batchFieldMetaDataDao.GetById(fieldValue.FieldId);

                    if (fieldValue.FieldMetaData.IsLookup && !string.IsNullOrEmpty(fieldValue.FieldMetaData.LookupXml))
                    {
                        fieldValue.FieldMetaData.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(fieldValue.FieldMetaData.LookupXml);
                    }
                }

                ActionLogHelper.AddActionLog("Get batch data on batch Id: " + item.Id,
                                  LoginUser, ActionName.GetBatchData, null, null, dataContext);

                return item;
            }
        }

        public Batch GetBatchInfo(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                Batch item = new BatchDao(dataContext).GetById(id);

                ActionLogHelper.AddActionLog("Get batch Id: " + item.Id,
                              LoginUser, ActionName.GetBatchItem, null, null, dataContext);

                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(dataContext);
                DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);
                PageDao pageDao = new PageDao(dataContext);
                AnnotationDao annoDao = new AnnotationDao(dataContext);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(dataContext);

                if (!LoginUser.IsAdmin)
                {
                    bool hasPermission = CheckPermission(LoginUser.Id, item, dataContext);

                    if (!hasPermission)
                    {
                        throw new AccessViolationException(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, id));
                    }
                }

                item.Comments = new CommentDao(dataContext).GetByInstance(item.Id);

                foreach (var comm in item.Comments)
                {
                    if (!string.IsNullOrEmpty(comm.Note))
                    {
                        comm.Note = comm.Note.Replace("\"", "\\\"");
                    }
                }

                item.FieldValues = new BatchFieldValueDao(dataContext).GetByBatch(item.Id);
                item.Documents = new DocumentDao(dataContext).GetByBatch(item.Id);

                foreach (var fieldValue in item.FieldValues)
                {
                    fieldValue.FieldMetaData = batchFieldMetaDataDao.GetById(fieldValue.FieldId);
                    fieldValue.FieldMetaData.LookupXml = string.Empty;
                }

                Setting setting = new SettingManager(LoginUser).GetSettings();

                foreach (var doc in item.Documents)
                {
                    if (doc.DocTypeId != Guid.Empty)
                    {

                        doc.FieldValues = docFieldValueDao.GetByDoc(doc.Id);
                        doc.DocumentType = docTypeDao.GetById(doc.DocTypeId);

                        foreach (var fieldValue in doc.FieldValues)
                        {
                            fieldValue.FieldMetaData = docFieldMetaDataDao.GetById(fieldValue.FieldId);
                            if (fieldValue.FieldMetaData.ValidationPattern != null)
                                fieldValue.FieldMetaData.ValidationPattern = fieldValue.FieldMetaData.ValidationPattern.Replace("\\", "\\\\");

                            if (fieldValue.FieldMetaData.DataType == "Table")
                            {
                                fieldValue.FieldMetaData.Children = docFieldMetaDataDao.GetChildren(fieldValue.FieldId);
                                fieldValue.TableFieldValue = new TableFieldValueDao(dataContext).GetByParentField(fieldValue.FieldId, doc.Id);

                                foreach (var tableFieldValue in fieldValue.TableFieldValue)
                                {
                                    tableFieldValue.Field = docFieldMetaDataDao.GetChild(tableFieldValue.FieldId);
                                }

                            }

                            if (fieldValue.FieldMetaData.DataTypeEnum == FieldDataType.Picklist)
                            {
                                fieldValue.FieldMetaData.Picklists = picklistDao.GetByField(fieldValue.FieldMetaData.Id);
                            }

                            fieldValue.FieldMetaData.LookupXml = string.Empty;

                        }
                    }

                    doc.Pages = pageDao.GetByDoc(doc.Id);

                    foreach (Page page in doc.Pages)
                    {
                        page.FileBinary = null;
                        page.FileBinaryBase64 = string.Empty;
                        page.Annotations = annoDao.GetByPage(page.Id);

                        foreach (var ann in page.Annotations)
                        {
                            if (!string.IsNullOrEmpty(ann.Content))
                            {
                                ann.Content = ann.Content.Replace("\"", "\\\"");
                            }
                            else
                            {
                                ann.Content = string.Empty;
                            }
                        }

                        page.FileHeader = null;
                    }
                }

                FillBatchPermission(new List<Batch> { item }, dataContext);
                item.BatchType.DocTypes = null;
                item.BatchType.Icon = null;
                ActionLogHelper.AddActionLog("Get batch data on batch Id: " + item.Id,
                                  LoginUser, ActionName.GetBatchData, null, null, dataContext);

                return item;
            }
        }

        public Batch OpenWorkItem(Guid id)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                LockBatchs(new List<Guid> { id });
            }
            Batch batch = GetBatch(id: id);

            return batch;
        }

        public List<Batch> OpenWorkItems(List<Guid> ids)
        {
            List<Batch> batchs = new List<Batch>();
            ids.ForEach(p => batchs.Add(OpenWorkItem(p)));

            return batchs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="searchType">searchType</param>
        /// <returns></returns>
        public WorkItemSearchResult GetSearchWorkItems(Guid batchTypeId, int pageIndex, BatchStatus batchStatus)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                // Get list batch
                var batchDao = new BatchDao(context);
                var batches = batchDao.GetByBatchType(batchTypeId);

                // Filter batch by type search
                batches = FilterSearchWorkItemByType(batches, batchStatus);

                // Get batch type
                var batchTypeDao = new BatchTypeDao(context);
                var batchType = batchTypeDao.GetById(batchTypeId);

                // Get field meta of batch
                var batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);
                batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batchTypeId);

                // Get no batch
                if (batches.Count == 0)
                {
                    return new WorkItemSearchResult
                    {
                        BatchType = batchType,
                        HasMoreResult = false,
                        PageIndex = 0,
                        WorkItems = batches
                    };
                }

                // Get dictionary
                var fieldMetas = batchType.Fields.ToDictionary(h => h.Id);

                List<Batch> tempBatches = null;
                var batchFieldValueDao = new BatchFieldValueDao(context);

                if (LoginUser.IsAdmin)
                {
                    #region Administrator user

                    foreach (var batch in batches)
                    {
                        // Set field value and its field meta data
                        batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);
                        foreach (var fieldValue in batch.FieldValues)
                        {
                            var fieldMeta = fieldMetas[fieldValue.FieldId];
                            fieldValue.FieldMetaData = new BatchFieldMetaData()
                            {
                                Id = fieldMeta.Id,
                                BatchTypeId = fieldMeta.BatchTypeId,
                                Name = fieldMeta.Name,
                                DataType = fieldMeta.DataType,
                                IsSystemField = fieldMeta.IsSystemField,
                                DisplayOrder = fieldMeta.DisplayOrder
                            };
                        }

                        // Set batch permission
                        batch.BatchPermission = BatchPermission.GetAllowAll();
                    }

                    #endregion
                }
                else
                {
                    #region Normal user

                    // Get user group ids
                    List<Guid> groupIds;
                    using (Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        var userGroupDao = new UserGroupDao(primaryContext);
                        groupIds = userGroupDao.GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    var customActivityDao = new CustomActivitySettingDao(context);
                    tempBatches = new List<Batch>();

                    foreach (var batch in batches)
                    {
                        // Try batch human step id
                        Guid humanStepId;
                        if (!Guid.TryParse(batch.BlockingBookmark, out humanStepId))
                        {
                            continue;
                        }
                        // Get custom activity and permission
                        var customActivity = customActivityDao.GetCustomActivitySetting(batch.WorkflowDefinitionId,
                                                                                        humanStepId);
                        if (customActivity == null)
                        {
                            continue;
                        }

                        var permission = UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);
                        var userGroupPermissions = permission.UserGroupPermissions
                                                             .Where(h => groupIds.Contains(h.UserGroupId)).ToList();
                        if (userGroupPermissions.Count == 0)
                        {
                            continue;
                        }

                        // Set work item permission
                        batch.BatchPermission = new BatchPermission
                        {
                            WorkflowDefinitionID = batch.WorkflowDefinitionId,
                            HumanStepID = humanStepId,
                            CanAnnotate = userGroupPermissions.Any(h => h.CanAnnotate),
                            CanDelete = userGroupPermissions.Any(h => h.CanDelete),
                            CanDownloadFilesOnDemand = userGroupPermissions.Any(h => h.CanDownloadFilesOnDemand),
                            CanEmail = userGroupPermissions.Any(h => h.CanEmail),
                            CanModifyDocument = userGroupPermissions.Any(h => h.CanModifyDocument),
                            CanModifyIndexes = userGroupPermissions.Any(h => h.CanModifyIndexes),
                            CanPrint = userGroupPermissions.Any(h => h.CanPrint),
                            CanReject = userGroupPermissions.Any(h => h.CanReject),
                            CanReleaseLoosePage = userGroupPermissions.Any(h => h.CanReleaseLoosePage),
                            CanSendLink = userGroupPermissions.Any(h => h.CanSendLink),
                            CanViewOtherItems = userGroupPermissions.Any(h => h.CanViewOtherItems),
                            CanDelegateItems = userGroupPermissions.Any(h => h.CanDelegateItems)
                        };

                        // Set field value and its field meta data
                        batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);
                        foreach (var fieldValue in batch.FieldValues)
                        {
                            var fieldMeta = fieldMetas[fieldValue.FieldId];
                            fieldValue.FieldMetaData = new BatchFieldMetaData()
                            {
                                Id = fieldMeta.Id,
                                BatchTypeId = fieldMeta.BatchTypeId,
                                Name = fieldMeta.Name,
                                DataType = fieldMeta.DataType,
                                IsSystemField = fieldMeta.IsSystemField,
                                DisplayOrder = fieldMeta.DisplayOrder,
                                UniqueId = fieldMeta.UniqueId
                            };
                        }

                        // Add to list can access batch
                        tempBatches.Add(batch);
                    }

                    #endregion
                }

                if (tempBatches == null)
                {
                    return new WorkItemSearchResult
                    {
                        BatchType = batchType,
                        HasMoreResult = false,
                        PageIndex = 0,
                        WorkItems = batches
                    };
                }
                else
                {
                    return new WorkItemSearchResult
                    {
                        BatchType = batchType,
                        HasMoreResult = false,
                        PageIndex = 0,
                        WorkItems = tempBatches
                    };
                }
            }
        }

        public List<Batch> GetWorkItems(BatchStatus batchStatus)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                // Get list batch
                var batchDao = new BatchDao(context);
                var batchTypeDao = new BatchTypeDao(context);
                var batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);
                var batchFieldValueDao = new BatchFieldValueDao(context);
                List<Batch> batches = null;

                if (LoginUser.IsAdmin)
                {
                    #region Administrator user

                    batches = batchDao.GetAllBatch();
                    // Filter batch by type search
                    batches = FilterSearchWorkItemByType(batches, batchStatus);

                    foreach (var batch in batches)
                    {
                        // Get batch type
                        BatchType batchType = batchTypeDao.GetById(batch.BatchTypeId);

                        // Get field meta of batch
                        batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batch.BatchTypeId);
                        batch.BatchType = batchType;
                        // Get dictionary
                        var fieldMetas = batchType.Fields.ToDictionary(h => h.Id);
                        // Set field value and its field meta data
                        batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);
                        foreach (var fieldValue in batch.FieldValues)
                        {
                            var fieldMeta = fieldMetas[fieldValue.FieldId];
                            fieldValue.FieldMetaData = new BatchFieldMetaData()
                            {
                                Id = fieldMeta.Id,
                                BatchTypeId = fieldMeta.BatchTypeId,
                                Name = fieldMeta.Name,
                                DataType = fieldMeta.DataType,
                                IsSystemField = fieldMeta.IsSystemField,
                                DisplayOrder = fieldMeta.DisplayOrder
                            };
                        }

                        // Set batch permission
                        batch.BatchPermission = BatchPermission.GetAllowAll();
                    }

                    #endregion
                }
                else
                {
                    #region Normal user

                    // Get user group ids
                    List<Guid> groupIds;
                    using (Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        var userGroupDao = new UserGroupDao(primaryContext);
                        groupIds = userGroupDao.GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    var customActivityDao = new CustomActivitySettingDao(context);
                    batches = batchDao.GetAllBatch(groupIds);
                    batches = FilterSearchWorkItemByType(batches, batchStatus);

                    foreach (var batch in batches)
                    {
                        // Try batch human step id
                        Guid humanStepId;
                        if (!Guid.TryParse(batch.BlockingBookmark, out humanStepId))
                        {
                            continue;
                        }
                        // Get custom activity and permission
                        var customActivity = customActivityDao.GetCustomActivitySetting(batch.WorkflowDefinitionId,
                                                                                        humanStepId);
                        if (customActivity == null)
                        {
                            continue;
                        }
                        // Get batch type
                        BatchType batchType = batchTypeDao.GetById(batch.BatchTypeId);

                        // Get field meta of batch
                        batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batch.BatchTypeId);
                        batch.BatchType = batchType;

                        // Get dictionary
                        var fieldMetas = batchType.Fields.ToDictionary(h => h.Id);

                        var permission = UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);
                        var userGroupPermissions = permission.UserGroupPermissions
                                                             .Where(h => groupIds.Contains(h.UserGroupId)).ToList();
                        if (userGroupPermissions.Count == 0)
                        {
                            continue;
                        }

                        // Set work item permission
                        batch.BatchPermission = new BatchPermission
                        {
                            WorkflowDefinitionID = batch.WorkflowDefinitionId,
                            HumanStepID = humanStepId,
                            CanAnnotate = userGroupPermissions.Any(h => h.CanAnnotate),
                            CanDelete = userGroupPermissions.Any(h => h.CanDelete),
                            CanDownloadFilesOnDemand = userGroupPermissions.Any(h => h.CanDownloadFilesOnDemand),
                            CanEmail = userGroupPermissions.Any(h => h.CanEmail),
                            CanModifyDocument = userGroupPermissions.Any(h => h.CanModifyDocument),
                            CanModifyIndexes = userGroupPermissions.Any(h => h.CanModifyIndexes),
                            CanPrint = userGroupPermissions.Any(h => h.CanPrint),
                            CanReject = userGroupPermissions.Any(h => h.CanReject),
                            CanReleaseLoosePage = userGroupPermissions.Any(h => h.CanReleaseLoosePage),
                            CanSendLink = userGroupPermissions.Any(h => h.CanSendLink),
                            CanViewOtherItems = userGroupPermissions.Any(h => h.CanViewOtherItems),
                            CanDelegateItems = userGroupPermissions.Any(h => h.CanDelegateItems)
                        };

                        // Set field value and its field meta data
                        batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);
                        foreach (var fieldValue in batch.FieldValues)
                        {
                            var fieldMeta = fieldMetas[fieldValue.FieldId];
                            fieldValue.FieldMetaData = new BatchFieldMetaData()
                            {
                                Id = fieldMeta.Id,
                                BatchTypeId = fieldMeta.BatchTypeId,
                                Name = fieldMeta.Name,
                                DataType = fieldMeta.DataType,
                                IsSystemField = fieldMeta.IsSystemField,
                                DisplayOrder = fieldMeta.DisplayOrder,
                                UniqueId = fieldMeta.UniqueId
                            };
                        }
                    }

                    #endregion
                }
                return batches;
            }
        }

        /// <summary>
        /// This method get search batch for mobile version
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="batchStatus"></param>
        /// <returns></returns>
        public List<Batch> GetSearchBatchs(Guid batchTypeId, int pageIndex, BatchStatus batchStatus)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                // Get list batch
                var batchDao = new BatchDao(context);
                var batches = batchDao.GetByBatchType(batchTypeId);

                // Filter batch by type search
                batches = FilterSearchWorkItemByType(batches, batchStatus);

                // Get batch type
                var batchTypeDao = new BatchTypeDao(context);
                var batchType = batchTypeDao.GetById(batchTypeId);
                if (!string.IsNullOrEmpty(batchType.BarcodeConfigurationXml))
                {
                    batchType.BarcodeConfigurationXml = batchType.BarcodeConfigurationXml.Replace("\"", "\\\"");
                }
                // Get field meta of batch
                var batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);

                // Get dictionary
                var fieldMetas = batchFieldMetaDataDao.GetByBatchType(batchType.Id).ToDictionary(h => h.Id);

                List<Batch> tempBatches = null;
                var batchFieldValueDao = new BatchFieldValueDao(context);

                if (LoginUser.IsAdmin)
                {
                    #region Administrator user

                    foreach (var batch in batches)
                    {
                        // Set field value and its field meta data
                        batch.BatchType = batchType;
                        batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);
                        foreach (var fieldValue in batch.FieldValues)
                        {
                            var fieldMeta = fieldMetas[fieldValue.FieldId];
                            fieldValue.FieldMetaData = new BatchFieldMetaData()
                            {
                                Id = fieldMeta.Id,
                                BatchTypeId = fieldMeta.BatchTypeId,
                                Name = fieldMeta.Name,
                                DataType = fieldMeta.DataType,
                                IsSystemField = fieldMeta.IsSystemField,
                                DisplayOrder = fieldMeta.DisplayOrder
                            };
                        }

                        // Set batch permission
                        batch.BatchPermission = BatchPermission.GetAllowAll();
                    }

                    #endregion
                }
                else
                {
                    #region Normal user

                    // Get user group ids
                    List<Guid> groupIds;
                    using (Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        var userGroupDao = new UserGroupDao(primaryContext);
                        groupIds = userGroupDao.GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    var customActivityDao = new CustomActivitySettingDao(context);
                    tempBatches = new List<Batch>();

                    foreach (var batch in batches)
                    {
                        // Try batch human step id
                        batch.BatchType = batchType;
                        Guid humanStepId;
                        if (!Guid.TryParse(batch.BlockingBookmark, out humanStepId))
                        {
                            continue;
                        }
                        // Get custom activity and permission
                        var customActivity = customActivityDao.GetCustomActivitySetting(batch.WorkflowDefinitionId,
                                                                                        humanStepId);
                        if (customActivity == null)
                        {
                            continue;
                        }

                        var permission = UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);
                        var userGroupPermissions = permission.UserGroupPermissions
                                                             .Where(h => groupIds.Contains(h.UserGroupId)).ToList();
                        if (userGroupPermissions.Count == 0)
                        {
                            continue;
                        }

                        // Set work item permission
                        batch.BatchPermission = new BatchPermission
                        {
                            WorkflowDefinitionID = batch.WorkflowDefinitionId,
                            HumanStepID = humanStepId,
                            CanAnnotate = userGroupPermissions.Any(h => h.CanAnnotate),
                            CanDelete = userGroupPermissions.Any(h => h.CanDelete),
                            CanDownloadFilesOnDemand = userGroupPermissions.Any(h => h.CanDownloadFilesOnDemand),
                            CanEmail = userGroupPermissions.Any(h => h.CanEmail),
                            CanModifyDocument = userGroupPermissions.Any(h => h.CanModifyDocument),
                            CanModifyIndexes = userGroupPermissions.Any(h => h.CanModifyIndexes),
                            CanPrint = userGroupPermissions.Any(h => h.CanPrint),
                            CanReject = userGroupPermissions.Any(h => h.CanReject),
                            CanReleaseLoosePage = userGroupPermissions.Any(h => h.CanReleaseLoosePage),
                            CanSendLink = userGroupPermissions.Any(h => h.CanSendLink),
                            CanViewOtherItems = userGroupPermissions.Any(h => h.CanViewOtherItems),
                            CanDelegateItems = userGroupPermissions.Any(h => h.CanDelegateItems)
                        };

                        // Set field value and its field meta data
                        batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);
                        foreach (var fieldValue in batch.FieldValues)
                        {
                            var fieldMeta = fieldMetas[fieldValue.FieldId];
                            fieldValue.FieldMetaData = new BatchFieldMetaData()
                            {
                                Id = fieldMeta.Id,
                                BatchTypeId = fieldMeta.BatchTypeId,
                                Name = fieldMeta.Name,
                                DataType = fieldMeta.DataType,
                                IsSystemField = fieldMeta.IsSystemField,
                                DisplayOrder = fieldMeta.DisplayOrder,
                                UniqueId = fieldMeta.UniqueId
                            };
                        }

                        // Add to list can access batch
                        tempBatches.Add(batch);
                    }

                    #endregion
                }

                if (tempBatches == null)
                {
                    return batches;
                }
                else
                {
                    return tempBatches;
                }
            }
        }

        /// <summary>
        /// Filter search work item by type.
        /// </summary>
        /// <param name="batches">Source list</param>
        /// <param name="searchType">searchType</param>
        /// <returns></returns>
        private List<Batch> FilterSearchWorkItemByType(List<Batch> batches, BatchStatus searchType)
        {
            switch (searchType)
            {
                case BatchStatus.Available:
                    return batches.Where(h => string.IsNullOrWhiteSpace(h.LockedBy)
                                              && !h.IsProcessing
                                              && !h.IsCompleted
                                              && !h.IsRejected
                                              && !h.HasError).ToList();

                case BatchStatus.Locked:
                    return batches.Where(h => !string.IsNullOrWhiteSpace(h.LockedBy)).ToList();

                case BatchStatus.Error:
                    return batches.Where(h => h.HasError).ToList();

                case BatchStatus.Reject:
                    return batches.Where(h => string.IsNullOrWhiteSpace(h.LockedBy)
                                              && !h.IsCompleted
                                              && h.IsRejected).ToList();

                case BatchStatus.InProcessing:
                    return batches.Where(h => h.IsProcessing).ToList();
                default:
                    return null;
            }

        }

        //public WorkItemSearchResult RunAdvanceSearchWorkItem(Guid batchTypeId, string expression, int pageIndex)
        //{
        //    BatchType batchType = new BatchTypeManager(LoginUser).GetBatchType(batchTypeId);

        //    if (batchType == null)
        //    {
        //        throw new SecurityException(string.Format("User {0} doesn't have permission to search documents.", LoginUser.UserName));
        //    }

        //    WorkItemSearchResult result = new WorkItemSearchResult
        //    {
        //        BatchType = batchType,
        //        HasMoreResult = false,
        //        PageIndex = 0
        //    };

        //    using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
        //    {

        //        if (LoginUser.IsAdmin)
        //        {
        //            BatchPermission permission = BatchPermission.GetAllowAll();

        //            //result.WorkItems = batchs;
        //            result.WorkItems.ForEach(p => p.BatchPermission = permission);
        //        }
        //        else
        //        {
        //            List<BatchPermission> permissions = GetWorkItemPermissions(LoginUser.Id, batchTypeId, new WorkflowHumanStepPermissionDao(dataContext));
        //            if (permissions.Count > 0)
        //            {
        //                //result.WorkItems = batchs;

        //                permissions.ForEach(p =>
        //                {
        //                    var tmps = result.WorkItems.Where(wi => wi.WorkflowDefinitionId == p.WorkflowDefinitionID &&
        //                                                            wi.BlockingBookmark == p.HumanStepID.ToString()).ToList();
        //                    tmps.ForEach(q => q.BatchPermission = p);
        //                });
        //            }
        //        }

        //        return result;
        //    }
        //}

        public WorkItemSearchResult RunAdvanceSearchWorkItem(Guid batchTypeId, SearchQuery query, int pageIndex)
        {
            BatchType batchType = new BatchTypeManager(LoginUser).GetBatchType(batchTypeId);

            if (batchType == null)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to search batches.", LoginUser.UserName));
            }

            WorkItemSearchResult result = new WorkItemSearchResult
            {
                BatchType = batchType,
                HasMoreResult = false,
                PageIndex = 0
            };

            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                List<Batch> batchs = new List<Batch>(); ;
                List<Batch> allBatchs = new List<Batch>();

                if (query.SearchQueryExpressions == null || query.SearchQueryExpressions.Count == 0)
                {
                    allBatchs = new BatchDao(dataContext).GetByBatchType(batchTypeId);
                }
                else
                {
                    allBatchs = BuildAdvanceSearchData(dataContext, batchTypeId, query, pageIndex);
                }

                if (LoginUser.IsAdmin)
                {
                    batchs.AddRange(allBatchs);
                }
                else
                {
                    foreach (Batch batch in allBatchs)
                    {
                        bool hasPermission = CheckPermission(LoginUser.Id, batch, dataContext);

                        if (hasPermission)
                        {
                            batchs.Add(batch);
                        }
                    }
                }

                List<Batch> finalBatchs = new List<Batch>();
                batchs.ForEach(p => finalBatchs.Add(GetBatchData(p.Id)));

                result.WorkItems = finalBatchs;
            }

            return result;
        }

        public void RejectBatchElement(Batch rejectBatch, DapperContext context)
        {
            BatchDao batchDao = new BatchDao(context);
            DocumentDao docDao = new DocumentDao(context);
            PageDao pageDao = new PageDao(context);

            batchDao.Reject(rejectBatch);
            rejectBatch.Documents.ForEach(delegate (Document doc)
            {
                doc.IsRejected = true;
                docDao.Reject(doc);
                doc.Pages.ForEach(delegate (Page page)
                {
                    page.IsRejected = true;
                    pageDao.Reject(page);
                });
            });

        }

        public void ReleaseBatch(Guid batchId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                Batch batch = null;
                try
                {
                    dataContext.BeginTransaction();
                    BatchDao batchDao = new BatchDao(dataContext);
                    DocumentDao documentDao = new DocumentDao(dataContext);
                    DocumentFieldValueDao documentFieldValueDao = new DocumentFieldValueDao(dataContext);
                    BatchFieldValueDao batchFieldValueDao = new BatchFieldValueDao(dataContext);
                    PageDao pageDao = new PageDao(dataContext);
                    AnnotationDao annotationDao = new AnnotationDao(dataContext);
                    ReleaseDocumentDao releaseDocumentDao = new ReleaseDocumentDao(dataContext);
                    ReleaseBatchFieldValueDao releaseBatchFieldValueDao = new ReleaseBatchFieldValueDao(dataContext);
                    TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(dataContext);

                    batch = batchDao.GetById(batchId);
                    batch.BatchType = new BatchTypeDao(dataContext).GetById(batch.BatchTypeId);

                    List<Document> captureDocuments = documentDao.GetByBatch(batchId);

                    ReleaseBatch releaseBatch = new ReleaseBatch()
                    {
                        BatchTypeId = batch.BatchTypeId,
                        CreatedBy = batch.CreatedBy,
                        CreatedDate = DateTime.Now,
                        DocCount = batch.DocCount,
                        PageCount = batch.PageCount,
                        IsRejected = batch.IsRejected
                    };

                    ReleaseBatchDao releaseBatchDao = new ReleaseBatchDao(dataContext);
                    releaseBatchDao.InsertReleaseBatch(releaseBatch);

                    batch.FieldValues = batchFieldValueDao.GetByBatch(batch.Id);

                    foreach (BatchFieldValue batchValue in batch.FieldValues)
                    {
                        ReleaseBatchFieldValue releaseBatchFieldValue = new ReleaseBatchFieldValue
                        {
                            FieldId = batchValue.FieldId,
                            ReleaseBatchId = releaseBatch.Id,
                            Value = batchValue.Value,
                        };

                        releaseBatchFieldValueDao.InsertFieldValue(releaseBatchFieldValue);
                    }

                    releaseBatch.ReleaseDocuments = new List<ReleaseDocument>();
                    foreach (Document captureDocument in captureDocuments)
                    {
                        captureDocument.DocumentType = new DocumentType();
                        captureDocument.DocumentType.BatchTypeId = batch.BatchTypeId;

                        ReleaseDocument releaseDocument = new ReleaseDocument()
                        {
                            BinaryType = captureDocument.BinaryType,
                            CreatedBy = captureDocument.CreatedBy,
                            CreatedDate = captureDocument.CreatedDate,
                            DocTypeId = captureDocument.DocTypeId,
                            PageCount = captureDocument.PageCount,
                            ReleaseBatchId = releaseBatch.Id
                        };

                        releaseDocumentDao.Add(releaseDocument);

                        List<DocumentFieldValue> captureFieldValues = documentFieldValueDao.GetByDoc(captureDocument.Id);
                        releaseDocument.ReleaseFieldValues = new List<ReleaseDocumentFieldValue>();

                        foreach (DocumentFieldValue captureFieldValue in captureFieldValues)
                        {
                            ReleaseDocumentFieldValue releaseFieldValue = new ReleaseDocumentFieldValue()
                            {
                                FieldId = captureFieldValue.FieldId,
                                ReleaseDocId = releaseDocument.Id,
                                Value = captureFieldValue.Value,
                                TableFieldValue = new List<ReleaseTableFieldValue>()
                            };

                            captureFieldValue.TableFieldValue = tableFieldValueDao.GetData(captureFieldValue.FieldId, captureDocument.Id);

                            if (captureFieldValue.TableFieldValue != null && captureFieldValue.TableFieldValue.Count > 0)
                            {
                                foreach (var tableValue in captureFieldValue.TableFieldValue)
                                {
                                    ReleaseTableFieldValue tableFieldValue = new ReleaseTableFieldValue
                                    {
                                        DocId = releaseDocument.Id,
                                        Field = tableValue.Field,
                                        FieldId = tableValue.FieldId,
                                        RowNumber = tableValue.RowNumber,
                                        Value = tableValue.Value
                                    };

                                    new ReleaseTableFieldValueDao(dataContext).Add(tableFieldValue);
                                }
                            }

                            new ReleaseDocumentFieldValueDao(dataContext).InsertFieldValue(releaseFieldValue);
                        }

                        List<Page> capturePages = pageDao.GetByDoc(captureDocument.Id);
                        releaseDocument.ReleasePage = new List<ReleasePage>();
                        foreach (Page capturePage in capturePages)
                        {
                            ReleasePage releasePage = new ReleasePage()
                            {
                                FileExtension = capturePage.FileExtension,
                                FileHash = capturePage.FileHash,
                                FilePath = capturePage.FilePath,
                                Height = capturePage.Height,
                                PageNumber = capturePage.PageNumber,
                                ReleaseDocId = releaseDocument.Id,
                                RotateAngle = capturePage.RotateAngle,
                                Width = capturePage.Width,
                                FileHeader = capturePage.FileHeader
                            };

                            if (_setting.IsSaveFileInFolder)
                            {
                                releasePage.FileBinary = FileHelpper.ReadFile(capturePage.FilePath, capturePage.FileHeader);

                                string filename = Path.Combine(GetDocFolder(captureDocument), Guid.NewGuid().ToString());
                                string path = Path.Combine(_setting.LocationSaveFile, CAPTURE_RELEASE_FOLDER, filename);

                                byte[] header = FileHelpper.CreateFile(path, releasePage.FileBinary, releasePage.FileExtension);

                                releasePage.FilePath = path;
                                releasePage.FileHeader = header;
                                releasePage.FileBinary = null;
                            }

                            new ReleasePageDao(dataContext).InsertReleasePage(releasePage);

                            List<Annotation> captureAnnotations = annotationDao.GetByPage(capturePage.Id);
                            foreach (Annotation captureAnnotation in captureAnnotations)
                            {
                                ReleaseAnnotation releaseAnnotation = new ReleaseAnnotation()
                                {
                                    Content = captureAnnotation.Content,
                                    CreatedBy = captureAnnotation.CreatedBy,
                                    CreatedOn = captureAnnotation.CreatedOn,
                                    Height = captureAnnotation.Height,
                                    Left = captureAnnotation.Left,
                                    LineColor = captureAnnotation.LineColor,
                                    LineEndAt = captureAnnotation.LineEndAt,
                                    LineStartAt = captureAnnotation.LineStartAt,
                                    LineStyle = captureAnnotation.LineStyle,
                                    LineWeight = captureAnnotation.LineWeight,
                                    ReleasePageId = releasePage.Id,
                                    RotateAngle = captureAnnotation.RotateAngle,
                                    Top = captureAnnotation.Top,
                                    Type = captureAnnotation.Type,
                                    Width = captureAnnotation.Width
                                };

                                new ReleaseAnnotationDao(dataContext).InsertReleaseAnnotation(releaseAnnotation);
                            }

                        }
                    }

                    DeleteCompletedBatchsData(new List<Guid> { batch.Id }, dataContext);

                    ActionLogHelper.AddActionLog("Released batch Id: " + batch.Id + " successfully",
                              LoginUser, ActionName.ReleaseBatch, null, null, dataContext);

                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }
        // Private methods
        private void DeleteBatchs(List<Guid> ids, DapperContext context)
        {
            CommonValidator.CheckNull(ids);

            try
            {
                BatchDao batchDao = new BatchDao(context);
                List<Batch> batchs = batchDao.GetBatchByRange(ids);
                bool canDelete = false;
                List<Guid> userGroupIds;
                using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                }
                string message = GetBatchComment(BatchHistoryAction.DeletedBatch);
                List<Batch> deletedBatchs = new List<Batch>();

                if (LoginUser.IsAdmin)
                {
                    deletedBatchs.AddRange(batchs);
                }
                else
                {
                    foreach (Batch batch in batchs)
                    {
                        //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                        //canDelete = humanStepPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanDelete);
                        CustomActivitySetting customSetting = new CustomActivitySettingDao(context).GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                        ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customSetting.Value);
                        canDelete = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId) && p.CanDelete);

                        if (canDelete)
                        {
                            deletedBatchs.Add(batch);
                        }
                    }
                }

                if (deletedBatchs.Count > 0)
                {
                    PageDao pageDao = new PageDao(context);
                    DocumentDao documentDao = new DocumentDao(context);
                    AnnotationDao annoDao = new AnnotationDao(context);
                    DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(context);
                    BatchFieldValueDao batchFieldValueDao = new BatchFieldValueDao(context);
                    TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(context);

                    Setting setting = new SettingManager(LoginUser).GetSettings();

                    foreach (Batch deletedBatch in deletedBatchs)
                    {
                        List<Document> deletedDocuments = documentDao.GetByBatch(deletedBatch.Id);

                        foreach (Document deletedDocument in deletedDocuments)
                        {
                            List<Page> deletedPages = pageDao.GetByDoc(deletedDocument.Id);
                            List<DocumentFieldValue> fieldValue = docFieldValueDao.GetByDoc(deletedDocument.Id);

                            foreach (Page deletedPage in deletedPages)
                            {
                                annoDao.DeleteByPage(deletedPage.Id);
                            }

                            if (setting.IsSaveFileInFolder)
                            {
                                List<Page> listPage = pageDao.GetByDoc(deletedDocument.Id);
                                foreach (Page page in listPage)
                                {
                                    if (File.Exists(page.FilePath))
                                    {
                                        FileHelpper.DeleteFile(page.FilePath);
                                    }
                                }
                            }

                            pageDao.DeleteByDoc(deletedDocument.Id);
                            docFieldValueDao.DeleteByDoc(deletedDocument.Id);
                            tableFieldValueDao.DeleteByDocument(deletedDocument.Id);

                        }

                        documentDao.DeleteByBatch(deletedBatch.Id);
                        batchDao.Delete(deletedBatch.Id);

                        if (_setting.IsSaveFileInFolder)
                        {
                            FileHelpper.DeleteFolder(Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, deletedBatch.Id.ToString()));
                        }


                        ActionLogHelper.AddActionLog("Delete batch Id: " + deletedBatch.Id + " successfully",
                                                    LoginUser, ActionName.DeleteBatch, null, null, context);

                        new HistoryDao(context).InsertHistory(new History()
                        {
                            Action = BatchHistoryAction.DeletedBatch.ToString(),
                            ActionDate = DateTime.Now,
                            BatchId = deletedBatch.Id,
                            CustomMsg = message,
                            WorkflowStep = deletedBatch.BlockingActivityName
                        });
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteBatchsData(List<Guid> ids, DapperContext context)
        {
            CommonValidator.CheckNull(ids);

            try
            {
                BatchDao batchDao = new BatchDao(context);
                List<Batch> batchs = batchDao.GetBatchByRange(ids);
                string message = GetBatchComment(BatchHistoryAction.DeletedBatch);

                Setting setting = new SettingManager(LoginUser).GetSettings();
                if (batchs.Count > 0)
                {
                    PageDao pageDao = new PageDao(context);
                    DocumentDao documentDao = new DocumentDao(context);
                    AnnotationDao annoDao = new AnnotationDao(context);
                    DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(context);
                    BatchFieldValueDao batchFieldValueDao = new BatchFieldValueDao(context);
                    TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(context);

                    foreach (Batch deletedBatch in batchs)
                    {
                        List<Document> deletedDocuments = documentDao.GetByBatch(deletedBatch.Id);

                        foreach (Document deletedDocument in deletedDocuments)
                        {
                            List<Page> deletedPages = pageDao.GetByDoc(deletedDocument.Id);
                            List<DocumentFieldValue> fieldValue = docFieldValueDao.GetByDoc(deletedDocument.Id);

                            foreach (Page deletedPage in deletedPages)
                            {
                                if (setting.IsSaveFileInFolder)
                                {
                                    if (File.Exists(deletedPage.FilePath))
                                    {
                                        FileHelpper.DeleteFile(deletedPage.FilePath);
                                    }
                                }

                                annoDao.DeleteByPage(deletedPage.Id);
                            }

                            pageDao.DeleteByDoc(deletedDocument.Id);
                            docFieldValueDao.DeleteByDoc(deletedDocument.Id);
                            tableFieldValueDao.DeleteByDocument(deletedDocument.Id);

                        }

                        documentDao.DeleteByBatch(deletedBatch.Id);
                        batchDao.Delete(deletedBatch.Id);

                        if (_setting.IsSaveFileInFolder)
                        {
                            FileHelpper.DeleteFolder(Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, deletedBatch.Id.ToString()));
                        }

                        ActionLogHelper.AddActionLog("Delete batch Id: " + deletedBatch.Id + " successfully",
                            LoginUser, ActionName.DeleteBatch, null, null, context);

                        new HistoryDao(context).InsertHistory(new History()
                        {
                            Action = BatchHistoryAction.DeletedBatch.ToString(),
                            ActionDate = DateTime.Now,
                            BatchId = deletedBatch.Id,
                            CustomMsg = message,
                            WorkflowStep = deletedBatch.BlockingActivityName
                        });
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteCompletedBatchsData(List<Guid> ids, DapperContext context)
        {
            CommonValidator.CheckNull(ids);

            try
            {
                BatchDao batchDao = new BatchDao(context);
                List<Batch> batchs = batchDao.GetCompletedBatchByRange(ids);
                string message = GetBatchComment(BatchHistoryAction.DeletedBatch);

                Setting setting = new SettingManager(LoginUser).GetSettings();
                if (batchs.Count > 0)
                {
                    PageDao pageDao = new PageDao(context);
                    DocumentDao documentDao = new DocumentDao(context);
                    AnnotationDao annoDao = new AnnotationDao(context);
                    DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(context);
                    BatchFieldValueDao batchFieldValueDao = new BatchFieldValueDao(context);
                    TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(context);

                    foreach (Batch deletedBatch in batchs)
                    {
                        List<Document> deletedDocuments = documentDao.GetByBatch(deletedBatch.Id);

                        foreach (Document deletedDocument in deletedDocuments)
                        {
                            List<Page> deletedPages = pageDao.GetByDoc(deletedDocument.Id);
                            List<DocumentFieldValue> fieldValue = docFieldValueDao.GetByDoc(deletedDocument.Id);

                            foreach (Page deletedPage in deletedPages)
                            {
                                if (setting.IsSaveFileInFolder)
                                {
                                    if (File.Exists(deletedPage.FilePath))
                                    {
                                        FileHelpper.DeleteFile(deletedPage.FilePath);
                                    }
                                }

                                annoDao.DeleteByPage(deletedPage.Id);
                            }

                            pageDao.DeleteByDoc(deletedDocument.Id);
                            docFieldValueDao.DeleteByDoc(deletedDocument.Id);
                            tableFieldValueDao.DeleteByDocument(deletedDocument.Id);

                        }

                        documentDao.DeleteByBatch(deletedBatch.Id);
                        batchDao.Delete(deletedBatch.Id);

                        if (_setting.IsSaveFileInFolder)
                        {
                            FileHelpper.DeleteFolder(Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, deletedBatch.Id.ToString()));
                        }

                        ActionLogHelper.AddActionLog("Delete completed batch Id: " + deletedBatch.Id + " successfully",
                            LoginUser, ActionName.DeleteBatch, null, null, context);

                        new HistoryDao(context).InsertHistory(new History()
                        {
                            Action = BatchHistoryAction.DeletedBatch.ToString(),
                            ActionDate = DateTime.Now,
                            BatchId = deletedBatch.Id,
                            CustomMsg = message,
                            WorkflowStep = deletedBatch.BlockingActivityName
                        });
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool ChangeLock(List<Guid> ids, bool isLock)
        {
            CommonValidator.CheckNull(ids);
            string lockedBy = isLock ? LoginUser.UserName : null;
            bool isChanged = false;

            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    BatchDao batchDao = new BatchDao(context);
                    List<Batch> batchs = batchDao.GetBatchByRange(ids);
                    List<Guid> userGroupIds;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }
                    string message = GetBatchComment(isLock ? BatchHistoryAction.LockBatch : BatchHistoryAction.UnLockBatch) + " by {0} user from batch name {1}";
                    bool canLockBatch = false;

                    foreach (Batch batch in batchs)
                    {
                        string actionLogMessage = isLock ? "Lock work item id: " + batch.Id : "Unlock work item id: " + batch.Id;
                        ActionName lockedAction = isLock ? ActionName.LockedBatch : ActionName.UnlockedBatch;

                        if (LoginUser.IsAdmin)
                        {
                            #region
                            // Lock batch
                            if (isLock)
                            {
                                if (string.IsNullOrWhiteSpace(batch.LockedBy) || LoginUser.UserName == batch.LockedBy)
                                {
                                    batchDao.UpdateLockInfo(batch.Id, lockedBy);
                                    isChanged = true;
                                }
                                else
                                {
                                    actionLogMessage = string.Format(ErrorMessages.NoLockPermission, LoginUser, batch.Id);
                                    isChanged = false;
                                }
                            }
                            // Unlock batch
                            else
                            {
                                batchDao.UpdateLockInfo(batch.Id, null);
                                isChanged = true;
                            }
                            #endregion
                        }
                        else
                        {
                            #region
                            //List<WorkflowHumanStepPermission> humanStepPermissions = new WorkflowHumanStepPermissionDao(context).GetByHumanStepId(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                            //bool hasPermission = humanStepPermissions.Any(p => userGroupIds.Contains(p.UserGroupId));

                            bool hasPermission = CheckPermission(LoginUser.Id, batch, context);

                            if (isLock)
                            {
                                canLockBatch = hasPermission && batch.LockedBy == null;
                            }
                            else
                            {
                                canLockBatch = !string.IsNullOrEmpty(batch.LockedBy) && batch.LockedBy.Equals(LoginUser.UserName) && hasPermission;
                            }

                            if (canLockBatch)
                            {
                                batchDao.UpdateLockInfo(batch.Id, lockedBy);
                                isChanged = true;
                            }
                            else
                            {
                                actionLogMessage = string.Format(ErrorMessages.NoLockPermission, LoginUser, batch.Id);
                                //throw new AccessViolationException(string.Format(ErrorMessages.NoLockPermission, LoginUser, batch.Id));
                                isChanged = false;
                            }
                            #endregion
                        }

                        ActionLogHelper.AddActionLog(actionLogMessage, LoginUser, lockedAction, null, null, context);

                        //if (isChanged)
                        //{
                        //    batchDao.UpdateTransactionId(batch.Id, Guid.NewGuid());
                        //}

                        //Batch updatedBatch = batchDao.GetById(batch.Id);

                        #region
                        //using (var luceneService = GetLucenceCaptureClientChannel())
                        //{
                        //    luceneService.Channel.UpdateIndex(AUTHORIZED_KEY, updatedBatch);

                        //    History indexHistory = new History
                        //    {
                        //        Action = BatchHistoryAction.SubmitBatch.ToString(),
                        //        ActionDate = DateTime.Now,
                        //        BatchId = updatedBatch.Id,
                        //        CustomMsg = string.Format("Update index for batch successfully. Id = " + updatedBatch.Id),
                        //        Id = Guid.NewGuid(),
                        //        WorkflowStep = updatedBatch.BlockingActivityName
                        //    };
                        //    new HistoryDao(context).InsertHistory(indexHistory);
                        //}

                        //Comment comment = new Comment()
                        //{
                        //    CreatedBy = LoginUser.UserName,
                        //    CreatedDate = DateTime.Now,
                        //    InstanceId = batch.Id,
                        //    IsBatchId = true,
                        //   Note = string.Format(message, LoginUser.UserName, batch.BlockingActivityName)
                        //};

                        //new CommentDao(context).InsertComment(comment); 
                        #endregion

                        History history = new History()
                        {
                            Action = BatchHistoryAction.RejectedBatch.ToString(),
                            ActionDate = DateTime.Now,
                            BatchId = batch.Id,
                            CustomMsg = string.Format(message, LoginUser.UserName, batch.BlockingActivityName),
                            WorkflowStep = batch.BlockingActivityName
                        };

                        new HistoryDao(context).InsertHistory(history);
                    }

                    context.Commit();

                    return isChanged;
                }
                catch (Exception)
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        private string GetBatchComment(BatchHistoryAction action)
        {
            switch (action)
            {
                case BatchHistoryAction.ApprovedBatch:
                    return "Approved batch";
                case BatchHistoryAction.RejectedBatch:
                    return "Reject batch";
                case BatchHistoryAction.SubmitBatch:
                    return "Submit batch";
                case BatchHistoryAction.LockBatch:
                    return "Locked batch to process";
                case BatchHistoryAction.UnLockBatch:
                    return "Unlocked batch";
                default:
                    return null;
            }
        }

        private void FillPermission(Batch batch)
        {
            if (LoginUser.IsAdmin)
            {
                batch.BatchType.BatchTypePermission = BatchTypePermission.GetAllowAll();
            }
            else
            {
                using (Ecm.Context.DapperContext primaryContext = new Ecm.Context.DapperContext())
                {
                    UserGroupDao groupDao = new UserGroupDao(primaryContext);
                    List<Guid> groupIds = groupDao.GetByUser(LoginUser.Id).Select(p => p.Id).ToList();

                    using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
                    {
                        //BatchTypePermissionDao batchTypePermissionDao = new BatchTypePermissionDao(context);
                        //List<BatchTypePermission> batchTypePermissions = batchTypePermissionDao.GetByUserGroupRange(groupIds);
                        batch.BatchType.BatchTypePermission = new PermissionManager(LoginUser).GetBatchTypePermissionByUser(LoginUser.Id, batch.BatchTypeId);

                    }
                }
            }
        }

        private void FillBatchPermission(List<Batch> batchs, DapperContext context)
        {
            foreach (Batch batch in batchs)
            {
                batch.BatchType = new BatchTypeDao(context).GetById(batch.BatchTypeId);
                batch.BatchType.DocTypes = new DocumentTypeDao(context).GetDocumentTypeByBatch(batch.BatchTypeId);
                batch.BatchType.DocTypes.ForEach(p => p.Fields = new DocFieldMetaDataDao(context).GetByDocType(p.Id));

                if (LoginUser.IsAdmin || string.IsNullOrWhiteSpace(batch.BlockingBookmark))
                {
                    batch.BatchPermission = BatchPermission.GetAllowAll();
                    batch.BatchType.BatchTypePermission = BatchTypePermission.GetAllowAll();

                    foreach (Document doc in batch.Documents)
                    {
                        doc.AnnotationPermission = AnnotationPermission.GetAllowAll();
                        doc.DocumentPermission = DocumentPermission.GetAll();

                        // HungLe - 2015-06-09 - Fix
                        // Case loose doc
                        if (doc.DocumentType == null)
                        {
                            continue;
                        }

                        doc.DocumentType.DocumentTypePermission = DocumentTypePermission.GetAll();

                        foreach (DocumentFieldMetaData field in doc.DocumentType.Fields.Where(p => !p.IsSystemField))
                        {
                            DocumentFieldPermission fieldPermission = DocumentFieldPermission.GetAll();
                            fieldPermission.DocTypeId = doc.DocTypeId;
                            fieldPermission.FieldId = field.Id;
                            doc.DocumentPermission.FieldPermissions.Add(fieldPermission);
                        }
                    }
                }
                else if (batch.BlockingBookmark != "AUTORESUME")
                {
                    List<Guid> groupIds = null;

                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(context);
                    CustomActivitySetting customActivity = customActivityDao.GetCustomActivitySetting(batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark));
                    ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);

                    var batchPermission = GetWorkItemPermission(LoginUser.Id, batch.WorkflowDefinitionId, Guid.Parse(batch.BlockingBookmark), permission);
                    batch.BatchPermission = batchPermission;
                    batch.BatchType.BatchTypePermission = new PermissionManager(LoginUser).GetBatchTypePermissionByUser(LoginUser.Id, batch.BatchTypeId);

                    foreach (Document doc in batch.Documents)
                    {
                        // Case loose doc
                        if (doc.DocTypeId == Guid.Empty)
                        {
                            doc.DocumentType = new DocumentType();
                            doc.DocumentType.DocumentTypePermission = DocumentTypePermission.GetAll();
                            doc.AnnotationPermission = AnnotationPermission.GetAllowAll();
                            doc.DocumentPermission = DocumentPermission.GetAll();
                        }
                        else
                        {
                            doc.DocumentType.DocumentTypePermission = GetDocumentTypePermission(context, groupIds, doc.DocTypeId);
                            doc.AnnotationPermission = GetWorkItemDocumentTypeAnnotationPermission(LoginUser.Id, doc.DocTypeId, permission);
                            doc.DocumentPermission = GetDocumentPermission(permission, groupIds, doc.DocTypeId);
                        }
                    }

                }
            }

        }

        //private void FillDocumentTypePermission(DocumentType docType, DapperContext context)
        //{
        //    if (LoginUser.IsAdmin)
        //    {
        //        docType.DocumentTypePermission = DocumentTypePermission.GetAll();
        //        foreach (DocumentFieldMetaData field in docType.Fields.Where(p => !p.IsSystemField))
        //        {
        //            DocumentFieldPermission fieldPermission = DocumentFieldPermission.GetAll();
        //            fieldPermission.DocTypeId = docType.Id;
        //            fieldPermission.FieldId = field.Id;

        //            //docType.DocumentTypePermission.FieldPermissions.Add(fieldPermission);
        //        }
        //    }
        //    else
        //    {
        //        docType.DocumentTypePermission = GetDocumentTypePermission(context, docType.Id);
        //    }
        //}

        private List<BatchPermission> GetWorkItemPermissions(Guid userId, Guid batchTypeId, WorkflowHumanStepPermissionDao permissionDao)
        {
            List<BatchPermission> workItemPermissions = new List<BatchPermission>();
            List<Guid> groupIds = null;

            using (Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(userId).Select(p => p.Id).ToList();
            }

            List<WorkflowHumanStepPermission> wfPermissions = permissionDao.GetWorkflowHumanStepPermission(groupIds, batchTypeId);
            List<Guid> workflowDefinitionIds = wfPermissions.Select(p => p.WorkflowDefinitionId).Distinct().ToList();

            workflowDefinitionIds.ForEach(wfId =>
            {
                List<WorkflowHumanStepPermission> humanPermissions = wfPermissions.Where(p => p.WorkflowDefinitionId == wfId).ToList();
                List<Guid> humanIds = humanPermissions.Select(p => p.HumanStepId).Distinct().ToList();

                humanIds.ForEach(p =>
                {
                    List<WorkflowHumanStepPermission> groupPermissions = humanPermissions.Where(q => q.HumanStepId == p).ToList();
                    workItemPermissions.Add(new BatchPermission
                    {
                        WorkflowDefinitionID = wfId,
                        HumanStepID = p,
                        CanAnnotate = groupPermissions.Any(q => q.CanAnnotate),
                        CanDelete = groupPermissions.Any(q => q.CanDelete),
                        CanDownloadFilesOnDemand = groupPermissions.Any(q => q.CanDownloadFilesOnDemand),
                        CanEmail = groupPermissions.Any(q => q.CanEmail),
                        CanModifyDocument = groupPermissions.Any(q => q.CanModifyDocument),
                        CanModifyIndexes = groupPermissions.Any(q => q.CanModifyIndexes),
                        CanPrint = groupPermissions.Any(q => q.CanPrint),
                        CanReject = groupPermissions.Any(q => q.CanReject),
                        CanReleaseLoosePage = groupPermissions.Any(q => q.CanReleaseLoosePage),
                        CanSendLink = groupPermissions.Any(q => q.CanSendLink),
                        CanViewOtherItems = groupPermissions.Any(q => q.CanViewOtherItems)
                    });
                });
            });

            return workItemPermissions;
        }

        private BatchPermission GetWorkItemPermission(Guid userId, Guid workflowDefinitionId, Guid humanStepId, DapperContext context)
        {
            //WorkflowHumanStepPermissionDao permissionDao = new WorkflowHumanStepPermissionDao(context);
            //List<WorkflowHumanStepPermission> humanPermissions = permissionDao.GetByHumanStepId(workflowDefinitionId, humanStepId);
            List<Guid> groupIds = null;

            using (Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(userId).Select(p => p.Id).ToList();
            }

            //humanPermissions = humanPermissions.Where(p => groupIds.Contains(p.UserGroupId)).ToList();

            CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(context);
            CustomActivitySetting customActivity = customActivityDao.GetCustomActivitySetting(workflowDefinitionId, humanStepId);
            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);
            List<UserGroupPermission> userGroupPermissions = permission.UserGroupPermissions.Where(p => groupIds.Contains(p.UserGroupId)).ToList();

            return new BatchPermission
            {
                WorkflowDefinitionID = workflowDefinitionId,
                HumanStepID = humanStepId,
                CanAnnotate = userGroupPermissions.Any(q => q.CanAnnotate),
                CanDelete = userGroupPermissions.Any(q => q.CanDelete),
                CanDownloadFilesOnDemand = userGroupPermissions.Any(q => q.CanDownloadFilesOnDemand),
                CanEmail = userGroupPermissions.Any(q => q.CanEmail),
                CanModifyDocument = userGroupPermissions.Any(q => q.CanModifyDocument),
                CanModifyIndexes = userGroupPermissions.Any(q => q.CanModifyIndexes),
                CanPrint = userGroupPermissions.Any(q => q.CanPrint),
                CanReject = userGroupPermissions.Any(q => q.CanReject),
                CanReleaseLoosePage = userGroupPermissions.Any(q => q.CanReleaseLoosePage),
                CanSendLink = userGroupPermissions.Any(q => q.CanSendLink),
                CanViewOtherItems = userGroupPermissions.Any(q => q.CanViewOtherItems),
                CanDelegateItems = userGroupPermissions.Any(q => q.CanDelegateItems)
            };
        }

        private BatchPermission GetWorkItemPermission(Guid userId, Guid workflowDefinitionId, Guid humanStepId, ActivityPermission permission)
        {
            List<Guid> groupIds = null;

            using (Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(userId).Select(p => p.Id).ToList();
            }

            List<UserGroupPermission> userGroupPermissions = permission.UserGroupPermissions.Where(p => groupIds.Contains(p.UserGroupId)).ToList();

            return new BatchPermission
            {
                WorkflowDefinitionID = workflowDefinitionId,
                HumanStepID = humanStepId,
                CanAnnotate = userGroupPermissions.Any(q => q.CanAnnotate),
                CanDelete = userGroupPermissions.Any(q => q.CanDelete),
                CanDownloadFilesOnDemand = userGroupPermissions.Any(q => q.CanDownloadFilesOnDemand),
                CanEmail = userGroupPermissions.Any(q => q.CanEmail),
                CanModifyDocument = userGroupPermissions.Any(q => q.CanModifyDocument),
                CanModifyIndexes = userGroupPermissions.Any(q => q.CanModifyIndexes),
                CanPrint = userGroupPermissions.Any(q => q.CanPrint),
                CanReject = userGroupPermissions.Any(q => q.CanReject),
                CanReleaseLoosePage = userGroupPermissions.Any(q => q.CanReleaseLoosePage),
                CanSendLink = userGroupPermissions.Any(q => q.CanSendLink),
                CanViewOtherItems = userGroupPermissions.Any(q => q.CanViewOtherItems),
                CanDelegateItems = userGroupPermissions.Any(q => q.CanDelegateItems)
            };
        }

        private BatchPermission GetWorkItemDefaultPermission(Guid userId, Guid workflowDefinitionId, Guid batchTypeId, DapperContext context)
        {
            List<Guid> groupIds = null;

            using (Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(userId).Select(p => p.Id).ToList();
            }

            BatchType batchType = new BatchTypeDao(context).GetById(batchTypeId);
            BatchTypePermission permission = new PermissionManager(LoginUser).GetBatchTypePermissionByUser(userId, batchTypeId);

            return new BatchPermission
            {
                WorkflowDefinitionID = workflowDefinitionId,
                CanAnnotate = permission.CanCapture && permission.CanClassify,
                CanDelete = true,
                CanDownloadFilesOnDemand = true,
                CanEmail = permission.CanCapture,
                CanModifyDocument = permission.CanCapture && permission.CanClassify,
                CanModifyIndexes = permission.CanCapture && permission.CanIndex,
                CanPrint = permission.CanCapture,
                CanReject = permission.CanCapture,
                CanReleaseLoosePage = permission.CanCapture,
                CanSendLink = permission.CanCapture,
                CanViewOtherItems = permission.CanCapture,
                CanDelegateItems = false
            };
        }

        private AnnotationPermission GetWorkItemDocumentTypeAnnotationPermission(Guid userId, Guid workflowDefinitionId, Guid humanStepId,
                                                                                            Guid docTypeId, DapperContext context)
        {
            CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(context);
            CustomActivitySetting customActivity = customActivityDao.GetCustomActivitySetting(workflowDefinitionId, humanStepId);
            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);

            List<AnnotationPermission> annotationPermissions = new List<AnnotationPermission>();
            List<Guid> groupIds = null;

            using (Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(userId).Select(p => p.Id).ToList();
            }

            annotationPermissions = permission.AnnotationPermissions.Where(p => groupIds.Contains(p.UserGroupId)).ToList();

            return new AnnotationPermission
            {
                CanAddHighlight = annotationPermissions.Any(p => p.CanAddHighlight && p.DocTypeId == docTypeId),
                CanAddRedaction = annotationPermissions.Any(p => p.CanAddRedaction && p.DocTypeId == docTypeId),
                CanAddText = annotationPermissions.Any(p => p.CanAddText && p.DocTypeId == docTypeId),
                CanDeleteHighlight = annotationPermissions.Any(p => p.CanDeleteHighlight && p.DocTypeId == docTypeId),
                CanDeleteRedaction = annotationPermissions.Any(p => p.CanDeleteRedaction && p.DocTypeId == docTypeId),
                CanDeleteText = annotationPermissions.Any(p => p.CanDeleteText && p.DocTypeId == docTypeId),
                CanHideRedaction = annotationPermissions.Any(p => p.CanHideRedaction && p.DocTypeId == docTypeId),
                CanSeeHighlight = annotationPermissions.Any(p => p.CanSeeHighlight && p.DocTypeId == docTypeId),
                CanSeeText = annotationPermissions.Any(p => p.CanSeeText && p.DocTypeId == docTypeId)
            };
        }

        private AnnotationPermission GetWorkItemDocumentTypeAnnotationPermission(Guid userId, Guid docTypeId, ActivityPermission permission)
        {
            List<AnnotationPermission> annotationPermissions = new List<AnnotationPermission>();
            List<Guid> groupIds = null;

            using (Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(userId).Select(p => p.Id).ToList();
            }

            annotationPermissions = permission.AnnotationPermissions.Where(p => groupIds.Contains(p.UserGroupId)).ToList();

            return new AnnotationPermission
            {
                CanAddHighlight = annotationPermissions.Any(p => p.CanAddHighlight && p.DocTypeId == docTypeId),
                CanAddRedaction = annotationPermissions.Any(p => p.CanAddRedaction && p.DocTypeId == docTypeId),
                CanAddText = annotationPermissions.Any(p => p.CanAddText && p.DocTypeId == docTypeId),
                CanDeleteHighlight = annotationPermissions.Any(p => p.CanDeleteHighlight && p.DocTypeId == docTypeId),
                CanDeleteRedaction = annotationPermissions.Any(p => p.CanDeleteRedaction && p.DocTypeId == docTypeId),
                CanDeleteText = annotationPermissions.Any(p => p.CanDeleteText && p.DocTypeId == docTypeId),
                CanHideRedaction = annotationPermissions.Any(p => p.CanHideRedaction && p.DocTypeId == docTypeId),
                CanSeeHighlight = annotationPermissions.Any(p => p.CanSeeHighlight && p.DocTypeId == docTypeId),
                CanSeeText = annotationPermissions.Any(p => p.CanSeeText && p.DocTypeId == docTypeId)
            };
        }

        private void UpdateBatch(Batch batch, BatchPermission permission, DapperContext context, bool fromMobile = false)
        {
            #region Declare
            var now = DateTime.Now;
            var batchDao = new BatchDao(context);
            var batchFieldDao = new BatchFieldValueDao(context);
            var documentDao = new DocumentDao(context);
            var documentFieldDao = new DocumentFieldValueDao(context);
            var pageDao = new PageDao(context);
            var annoDao = new AnnotationDao(context);
            var commentDao = new CommentDao(context);
            var canReject = LoginUser.IsAdmin || permission.CanReject;
            var canUpdateIndexValue = LoginUser.IsAdmin || permission.CanModifyIndexes;
            var canModifyDoc = LoginUser.IsAdmin || permission.CanModifyDocument;
            var canReleaseLoosePage = LoginUser.IsAdmin || permission.CanReleaseLoosePage;
            //var canDelete = LoginUser.IsAdmin || permission.CanModifyDocument;
            Guid activeBatchId = batch.Id;
            int pageCount = 0;
            #endregion

            #region Batch information
            batch.LastAccessedBy = LoginUser.UserName;
            batch.LastAccessedDate = now;
            batch.LockedBy = null;
            batch.BatchName = batch.BatchName;
            batch.ModifiedBy = LoginUser.UserName;
            batch.ModifiedDate = now;
            if (!canReject && batch.IsRejected)
            {
                batch.IsRejected = batchDao.GetRejectStatus(batch.Id);
            }
            #endregion

            #region Batch index
            if (canUpdateIndexValue)
            {
                foreach (BatchFieldValue fieldValue in batch.FieldValues)
                {
                    if (fieldValue.FieldMetaData.IsSystemField)
                    {
                        if (fieldValue.FieldMetaData.Name == BatchFieldMetaData._sysModifiedOn)
                        {
                            fieldValue.Value = batch.LastAccessedDate.Value.ToString(Constants.DATE_FULL_FORMAT_DB);
                        }
                        else if (fieldValue.FieldMetaData.Name == BatchFieldMetaData._sysModifiedBy)
                        {
                            fieldValue.Value = batch.LastAccessedBy;
                        }
                    }

                    if (fieldValue.Id == Guid.Empty)
                    {
                        batchFieldDao.Add(fieldValue);
                    }
                    else
                    {
                        batchFieldDao.Update(fieldValue);
                    }
                }

                ActionLogHelper.AddActionLog("Update batch field value on batch Id: " + batch.Id,
                          LoginUser, ActionName.UpdateBatchFieldValue, null, null, context);
            }
            #endregion

            #region Comment
            if (batch.Comments != null && batch.Comments.Count > 0)
            {
                foreach (Comment comment in batch.Comments)
                {
                    if (comment.Id == Guid.Empty)
                    {
                        // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                        comment.CreatedBy = LoginUser.UserName;
                        comment.CreatedDate = DateTime.Now;
                        // 2014/08/15 - HungLe - End - Adding code save create date by time of service
                        comment.InstanceId = batch.Id;
                        comment.IsBatchId = true;
                        commentDao.InsertComment(comment);
                    }
                }
                ActionLogHelper.AddActionLog("Add comment for batch: " + batch.Id,
                              LoginUser, ActionName.AddComment, null, null, context);
            }
            #endregion

            #region Update document
            foreach (Document document in batch.Documents)
            {
                if (!ValidateFieldValue(document.FieldValues))
                {
                    throw new Exception(ErrorMessages.InvalidEnterData);
                }

                if (document.Id == Guid.Empty)
                {
                    var docTypePermission =
                        new PermissionManager(LoginUser).GetDocumentTypePermissionByUser(LoginUser.Id,
                                                                                         batch.BatchTypeId);
                    if (docTypePermission == null || !docTypePermission.CanAccess)
                    {
                        throw new AccessViolationException(string.Format(ErrorMessages.NoAccessPermission,
                                                                         LoginUser.UserName,
                                                                         document.DocumentType.Name));
                    }

                    document.BatchId = batch.Id;
                    InsertDocument(batch, document, context, fromMobile);
                    ActionLogHelper.AddActionLog("Create document on batch Id: " + batch.Id,
                                         LoginUser, ActionName.AddDocument, null, null, context);
                }
                else
                {
                    UpdateDocument(document, permission, context, fromMobile);
                    ActionLogHelper.AddActionLog("Update document on batch Id: " + batch.Id,
                                        LoginUser, ActionName.UpdateDocument, null, null, context);
                }

                pageCount += document.Pages.Count;
            }
            #endregion

            var tableFiledValueDao = new TableFieldValueDao(context);

            #region Delete document
            if (canModifyDoc)
            {
                if (batch.DeletedDocuments != null && batch.DeletedDocuments.Count > 0)
                {
                    if (_setting.IsSaveFileInFolder)
                    {
                        foreach (Guid docId in batch.DeletedDocuments)
                        {
                            string docFolder = Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, batch.Id.ToString(), docId.ToString());
                            FileHelpper.DeleteFolder(docFolder);
                        }
                    }

                    // 2014/08/15 - HungLe - Start - Adding code delete field values, annotations , pages
                    foreach (var delDocId in batch.DeletedDocuments)
                    {
                        tableFiledValueDao.DeleteByParentField(delDocId);
                        // Delete indexes of doc
                        documentFieldDao.DeleteByDoc(delDocId);
                        // Delete anno of doc
                        annoDao.DeleteByDoc(delDocId);
                        // Delete page of doc
                        pageDao.DeleteByDoc(delDocId);
                    }
                    // 2014/08/15 - HungLe - End - Adding code delete field values, annotations , pages

                    documentDao.Delete(batch.DeletedDocuments);
                    ActionLogHelper.AddActionLog("Delete documents on batch Id: " + batch.Id,
                              LoginUser, ActionName.DeleteDocument, null, null, context);
                }

                if (batch.DeletedLooseDocuments != null && batch.DeletedLooseDocuments.Count > 0)
                {
                    foreach (Guid deleteDoc in batch.DeletedLooseDocuments)
                    {
                        documentFieldDao.DeleteByDoc(deleteDoc);
                    }

                    if (_setting.IsSaveFileInFolder)
                    {
                        foreach (Guid docId in batch.DeletedLooseDocuments)
                        {
                            string docFolder = Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, batch.Id.ToString(), docId.ToString());
                            FileHelpper.DeleteFolder(docFolder);
                        }
                    }

                    documentDao.Delete(batch.DeletedLooseDocuments);
                    ActionLogHelper.AddActionLog("Delete loose documents on batch Id: " + batch.Id,
                             LoginUser, ActionName.DeleteDocument, null, null, context);
                }
            }
            #endregion

            batchDao.Update(batch);
            ActionLogHelper.AddActionLog("Update batch: " + batch.Id + " successfully",
                            LoginUser, ActionName.UpdateBatch, null, null, context);
        }

        private void UpdateBatchForBarcode(Batch batch, DapperContext context)
        {
            Guid activeBatchId = batch.Id;
            DateTime now = DateTime.Now;
            int pageCount = 0;

            batch.LastAccessedBy = LoginUser.UserName;
            batch.LastAccessedDate = now;
            batch.LockedBy = null;
            batch.BatchName = batch.BatchName;
            batch.ModifiedBy = LoginUser.UserName;
            batch.ModifiedDate = now;

            BatchDao batchDao = new BatchDao(context);
            BatchFieldValueDao batchFieldDao = new BatchFieldValueDao(context);
            DocumentDao documentDao = new DocumentDao(context);
            DocumentFieldValueDao documentFieldDao = new DocumentFieldValueDao(context);
            PageDao pageDao = new PageDao(context);
            AnnotationDao annoDao = new AnnotationDao(context);
            CommentDao commentDao = new CommentDao(context);
            HistoryDao historyDao = new HistoryDao(context);

            foreach (BatchFieldValue fieldValue in batch.FieldValues)
            {
                batchFieldDao.Update(fieldValue);
            }

            if (batch.DeletedDocuments != null && batch.DeletedDocuments.Count > 0)
            {
                if (_setting.IsSaveFileInFolder)
                {
                    foreach (Guid docId in batch.DeletedDocuments)
                    {
                        string docFolder = Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, batch.Id.ToString(), docId.ToString());
                        FileHelpper.DeleteFolder(docFolder);
                    }
                }
                foreach (var delDoc in batch.DeletedDocuments)
                {
                    documentFieldDao.DeleteByDoc(delDoc);
                    annoDao.DeleteByDoc(delDoc);
                    pageDao.DeleteByDoc(delDoc);
                }

                documentDao.Delete(batch.DeletedDocuments);
            }

            if (batch.DeletedLooseDocuments != null && batch.DeletedLooseDocuments.Count > 0)
            {
                if (_setting.IsSaveFileInFolder)
                {
                    foreach (Guid docId in batch.DeletedLooseDocuments)
                    {
                        string docFolder = Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, batch.Id.ToString(), docId.ToString());
                        FileHelpper.DeleteFolder(docFolder);
                    }
                }

                foreach (var delDoc in batch.DeletedDocuments)
                {
                    documentFieldDao.DeleteByDoc(delDoc);
                    annoDao.DeleteByDoc(delDoc);
                    pageDao.DeleteByDoc(delDoc);
                }

                documentDao.Delete(batch.DeletedLooseDocuments);
            }

            foreach (Document document in batch.Documents)
            {
                if (!ValidateFieldValue(document.FieldValues))
                {
                    throw new Exception(ErrorMessages.InvalidEnterData);
                }

                if (document.Id == Guid.Empty)
                {
                    //if (!LoginUser.IsAdmin)
                    //{
                    //    DocumentTypePermission docTypePermission = new PermissionManager(LoginUser).GetDocumentTypePermissionByUser(LoginUser.Id, batch.BatchTypeId);
                    //    if (docTypePermission == null || !docTypePermission.CanAccess)
                    //    {
                    //        ActionLogHelper.AddActionLog("User do not have permission on content: " + document.DocName,
                    //                        LoginUser, ActionName.UpdateBatchAfterProcessBarcode, null, null, context);
                    //        continue;
                    //        //throw new AccessViolationException(string.Format(ErrorMessages.NoAccessPermission, LoginUser.UserName, document.DocumentType.Name));
                    //    }
                    //}

                    document.BatchId = activeBatchId;
                    InsertDocument(batch, document, context);
                }
                else
                {
                    UpdateDocument(document, BatchPermission.GetAllowAll(), context);
                }

                pageCount += document.Pages.Count;
            }

            History history = new History()
            {
                Action = BatchHistoryAction.UpdateBatchData.ToString(),
                ActionDate = DateTime.Now,
                BatchId = batch.Id,
                CustomMsg = "Update batch after process barcode",
                WorkflowStep = batch.BlockingActivityName
            };

            historyDao.InsertHistory(history);

            batch.DocCount = batch.Documents.Count;
            batch.PageCount = pageCount;

            batchDao.Update(batch);
            ActionLogHelper.AddActionLog("Update batch Id: " + batch.Id + " after process barcode successfully",
                            LoginUser, ActionName.UpdateBatchAfterProcessBarcode, null, null, context);

        }

        private void UpdateBatchForLookup(Batch batch, DapperContext context)
        {
            Guid activeBatchId = batch.Id;
            DateTime now = DateTime.Now;

            batch.LastAccessedBy = LoginUser.UserName;
            batch.LastAccessedDate = now;
            batch.LockedBy = null;
            batch.BatchName = batch.BatchName;
            batch.ModifiedBy = LoginUser.UserName;
            batch.ModifiedDate = now;

            BatchDao batchDao = new BatchDao(context);
            BatchFieldValueDao batchFieldDao = new BatchFieldValueDao(context);
            DocumentFieldValueDao documentFieldDao = new DocumentFieldValueDao(context);
            CommentDao commentDao = new CommentDao(context);
            HistoryDao historyDao = new HistoryDao(context);

            foreach (BatchFieldValue fieldValue in batch.FieldValues)
            {
                batchFieldDao.Update(fieldValue);
            }

            foreach (Document document in batch.Documents)
            {
                if (!ValidateFieldValue(document.FieldValues))
                {
                    throw new Exception(ErrorMessages.InvalidEnterData);
                }

                foreach (DocumentFieldValue fieldValue in document.FieldValues)
                {
                    new DocumentFieldValueDao(context).Update(fieldValue);
                }

                ActionLogHelper.AddActionLog("Update document field value on document Id: " + document.Id + " after lookup on server",
                                            LoginUser, ActionName.UpdateFieldValue, null, null, context);
            }

            History history = new History()
            {
                Action = BatchHistoryAction.UpdateBatchData.ToString(),
                ActionDate = DateTime.Now,
                BatchId = batch.Id,
                CustomMsg = "Update batch after process lookup",
                WorkflowStep = batch.BlockingActivityName
            };

            historyDao.InsertHistory(history);

            batchDao.Update(batch);
            ActionLogHelper.AddActionLog("Update batch Id: " + batch.Id + " after process lookup successfully",
                            LoginUser, ActionName.UpdateBatchAfterProcessBarcode, null, null, context);

        }

        private void InsertDocument(Batch batch, Document document, DapperContext context, bool fromMobile = false)
        {
            DocumentDao documentDao = new DocumentDao(context);
            DocumentFieldValueDao fieldDao = new DocumentFieldValueDao(context);
            PageDao pageDao = new PageDao(context);
            AnnotationDao annotationDao = new AnnotationDao(context);

            documentDao.Add(document);
            ActionLogHelper.AddActionLog("Create document Id: " + document.Id + " successfully",
                LoginUser, ActionName.AddDocument, null, null, context);

            int pageNumber = 1;

            foreach (var page in document.Pages)
            {
                page.DocId = document.Id;
                if (page.FileBinaryBase64 != null && page.FileBinary == null)
                {
                    page.FileBinary = Convert.FromBase64String(page.FileBinaryBase64);
                }
                page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                page.PageNumber = pageNumber;

                if (_setting.IsSaveFileInFolder)
                {
                    string filename = string.Empty;

                    if (document.DocTypeId == Guid.Empty)
                    {
                        filename = Path.Combine(GetLooseDocFolder(batch, document), Guid.NewGuid().ToString());
                    }
                    else
                    {
                        filename = Path.Combine(GetDocFolder(document), Guid.NewGuid().ToString());
                    }

                    string path = Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, filename);

                    byte[] header = FileHelpper.CreateFile(path, page.FileBinary, page.FileExtension);

                    page.FilePath = path;
                    page.FileHeader = header;
                    page.FileBinary = null;
                }

                pageDao.Add(page);

                if (page.Annotations != null)
                {
                    foreach (var anno in page.Annotations)
                    {
                        anno.PageId = page.Id;
                        anno.DocId = document.Id;
                        anno.DocTypeId = document.DocTypeId;
                        // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                        anno.CreatedBy = LoginUser.UserName;
                        anno.CreatedOn = DateTime.Now;
                        anno.ModifiedBy = null;
                        anno.ModifiedOn = null;
                        anno.LineEndAt = anno.LineEndAt != null ? anno.LineEndAt : "TopLeft";
                        anno.LineStartAt = anno.LineStartAt != null ? anno.LineStartAt : "TopLeft";
                        anno.LineStyle = anno.LineStyle != null ? anno.LineStyle : "ArrowAtEnd";
                        // 2014/08/15 - HungLe - End - Adding code save create date by time of service

                        if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                        {
                            anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                        }

                        annotationDao.Add(anno);
                    }
                }

                pageNumber++;
            }

            ActionLogHelper.AddActionLog("Create pages on document Id: " + document.Id,
                                LoginUser, ActionName.InsertPage, null, null, context);

            #region Doc index
            if (document.DocTypeId != Guid.Empty)
            {
                foreach (var documentFieldValue in document.FieldValues)
                {
                    documentFieldValue.DocId = document.Id;

                    fieldDao.Add(documentFieldValue);

                    if (documentFieldValue.TableFieldValue != null && documentFieldValue.TableFieldValue.Count > 0)
                    {
                        foreach (var tableValue in documentFieldValue.TableFieldValue)
                        {
                            tableValue.DocId = document.Id;

                            if (tableValue.Field.DataTypeEnum == FieldDataType.Date)
                            {
                                int index = tableValue.Value.IndexOf(" ");
                                if (index != -1)
                                {
                                    tableValue.Value = tableValue.Value.Substring(0, index);
                                }
                            }

                            new TableFieldValueDao(context).Add(tableValue);
                        }
                    }
                }
            }
            #endregion

            ActionLogHelper.AddActionLog("Add field values on document Id: " + document.Id,
                                LoginUser, ActionName.AddFieldValue, null, null, context);
        }

        private void UpdateDocument(Document document, BatchPermission permission, DapperContext context, bool fromMobile = false)
        {
            DocumentDao documentDao = new DocumentDao(context);
            DocumentFieldValueDao fieldDao = new DocumentFieldValueDao(context);
            PageDao pageDao = new PageDao(context);
            AnnotationDao annotationDao = new AnnotationDao(context);
            bool canUpdateIndexValue = LoginUser.IsAdmin || permission.CanModifyIndexes;
            bool canModifyDoc = LoginUser.IsAdmin || permission.CanModifyDocument;
            bool canReleaseLoosePage = LoginUser.IsAdmin || permission.CanReleaseLoosePage;
            bool canAnnotate = LoginUser.IsAdmin || permission.CanAnnotate;
            //bool canDelete = LoginUser.IsAdmin || permission.CanDelete;
            bool canReject = LoginUser.IsAdmin || permission.CanReject;

            if (canReject && document.IsRejected)
            {
                documentDao.Reject(document);
                ActionLogHelper.AddActionLog("Reject document Id: " + document.Id + " successfully",
                                    LoginUser, ActionName.RejectedDocument, null, null, context);

                foreach (Page rejectedPage in document.Pages)
                {
                    if (rejectedPage.IsRejected)
                    {
                        pageDao.Reject(rejectedPage);
                    }
                }

                ActionLogHelper.AddActionLog("Reject pages from document: " + document.Id + " successfully",
                                    LoginUser, ActionName.RejectedPage, null, null, context);
            }

            if (canModifyDoc)
            {
                documentDao.Update(document);
                ActionLogHelper.AddActionLog("Update document Id: " + document.Id + " successfully",
                                    LoginUser, ActionName.UpdateDocument, null, null, context);

                #region Delete page
                if (document.DeletedPages != null && document.DeletedPages.Count > 0)
                {
                    foreach (Guid pageId in document.DeletedPages)
                    {
                        List<Guid> willDeleteAnnotationIds = annotationDao.GetByPage(pageId).Select(p => p.Id).ToList();
                        if (willDeleteAnnotationIds != null && willDeleteAnnotationIds.Count > 0)
                        {
                            annotationDao.DeleteByPage(pageId);
                        }

                        if (_setting.IsSaveFileInFolder)
                        {
                            Page page = pageDao.GetById(pageId);
                            if (page != null && !string.IsNullOrEmpty(page.FilePath))
                            {
                                if (File.Exists(page.FilePath))
                                {
                                    FileHelpper.DeleteFile(page.FilePath);
                                }
                            }
                        }

                        pageDao.Delete(pageId);

                    }
                    ActionLogHelper.AddActionLog("Delete removed pages on document Id: " + document.Id,
                                        LoginUser, ActionName.DeletePage, null, null, context);
                }
                #endregion

                int pageIndex = 1;

                foreach (Page page in document.Pages)
                {
                    page.PageNumber = pageIndex;


                    if (page.Id == Guid.Empty)
                    {
                        #region Add new page
                        page.DocId = document.Id;
                        if (page.FileBinaryBase64 != null && page.FileBinary == null)
                        {
                            page.FileBinary = Convert.FromBase64String(page.FileBinaryBase64);
                        }
                        page.FileHash = Utility.CryptographyHelper.GenerateFileHash(page.FileBinary);

                        if (_setting.IsSaveFileInFolder)
                        {
                            string filename = Path.Combine(GetDocFolder(document), Guid.NewGuid().ToString());
                            string path = Path.Combine(_setting.LocationSaveFile, CAPTURE_FOLDER, filename);
                            byte[] header = FileHelpper.CreateFile(path, page.FileBinary, page.FileExtension);

                            page.FilePath = path;
                            page.FileHeader = header;
                            page.FileBinary = null;
                        }


                        pageDao.Add(page);

                        if (canAnnotate)
                        {
                            foreach (Annotation anno in page.Annotations)
                            {
                                anno.PageId = page.Id;
                                // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                                anno.CreatedBy = LoginUser.UserName;
                                anno.CreatedOn = DateTime.Now;
                                anno.ModifiedBy = null;
                                anno.ModifiedOn = null;
                                anno.DocId = document.Id;
                                anno.DocTypeId = document.DocTypeId;
                                anno.LineEndAt = anno.LineEndAt != null ? anno.LineEndAt : "TopLeft";
                                anno.LineStartAt = anno.LineStartAt != null ? anno.LineStartAt : "TopLeft";
                                anno.LineStyle = anno.LineStyle != null ? anno.LineStyle : "ArrowAtEnd";
                                // 2014/08/15 - HungLe - End - Adding code save create date by time of service

                                if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                                {
                                    anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                                }

                                annotationDao.Add(anno);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Update page

                        Page existingPage = pageDao.GetById(page.Id);
                        if (page.FileBinaryBase64 != null && page.FileBinary == null)
                        {
                            page.FileBinary = Convert.FromBase64String(page.FileBinaryBase64);
                        }

                        if (page.FileBinary != null)
                        {
                            string newHash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                            string exitingHash = existingPage.FileHash;

                            if (newHash != exitingHash && !string.IsNullOrEmpty(existingPage.FilePath))
                            {
                                FileHelpper.DeleteFile(existingPage.FilePath);
                            }

                            if (_setting.IsSaveFileInFolder)
                            {
                                page.FilePath = existingPage.FilePath;
                                page.FileHeader = FileHelpper.CreateFile(existingPage.FilePath, page.FileBinary, page.FileExtension);
                                page.FileBinary = null;
                            }

                            if (newHash != exitingHash)
                            {
                                page.FileHash = newHash;
                            }

                            //
                            pageDao.Update(page);
                        }
                        else
                        {
                            pageDao.UpdatePageInfo(page);
                        }

                        //TODO: will review
                        //pageDao.Update(page);

                        #region Annotation
                        if (canAnnotate)
                        {
                            #region Add or update annotation
                            if (page.Annotations != null)
                            {
                                foreach (Annotation anno in page.Annotations)
                                {
                                    if (anno.Id == Guid.Empty)
                                    {
                                        anno.PageId = page.Id;
                                        // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                                        anno.CreatedBy = LoginUser.UserName;
                                        anno.CreatedOn = DateTime.Now;
                                        anno.ModifiedBy = null;
                                        anno.ModifiedOn = null;
                                        anno.DocId = document.Id;
                                        anno.DocTypeId = document.DocTypeId;
                                        anno.LineEndAt = anno.LineEndAt != null ? anno.LineEndAt : "TopLeft";
                                        anno.LineStartAt = anno.LineStartAt != null ? anno.LineStartAt : "TopLeft";
                                        anno.LineStyle = anno.LineStyle != null ? anno.LineStyle : "ArrowAtEnd";
                                        // 2014/08/15 - HungLe - End - Adding code save create date by time of service

                                        if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                                        {
                                            anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                                        }

                                        annotationDao.Add(anno);
                                    }
                                    else
                                    {
                                        // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                                        anno.ModifiedBy = LoginUser.UserName;
                                        anno.ModifiedOn = DateTime.Now;
                                        anno.DocId = document.Id;
                                        anno.DocTypeId = document.DocTypeId;
                                        anno.LineEndAt = anno.LineEndAt != null ? anno.LineEndAt : "TopLeft";
                                        anno.LineStartAt = anno.LineStartAt != null ? anno.LineStartAt : "TopLeft";
                                        anno.LineStyle = anno.LineStyle != null ? anno.LineStyle : "ArrowAtEnd";
                                        // 2014/08/15 - HungLe - End - Adding code save create date by time of service

                                        if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                                        {
                                            anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                                        }

                                        annotationDao.Update(anno);
                                    }
                                }
                            }
                            #endregion

                            // 2014/08/15 - HungLe - Start - Adding delete anno page
                            if (page.DeleteAnnotations != null)
                            {
                                foreach (var delAnnoId in page.DeleteAnnotations)
                                {
                                    annotationDao.Delete(delAnnoId);
                                }

                            }
                        }
                        #endregion
                        // 2014/08/15 - HungLe - End - Adding delete anno page

                        #endregion
                    }

                    pageIndex++;
                }

                ActionLogHelper.AddActionLog("Update pages on document Id: " + document.Id,
                    LoginUser, ActionName.UpdatePage, null, null, context);

                // Set this to false for not get update annotation duplication in below
                canAnnotate = false;
            }

            #region Update annotation
            if (canAnnotate)
            {
                foreach (Page page in document.Pages)
                {
                    if (page.Id != Guid.Empty)
                    {
                        #region Add or update annotation
                        if (page.Annotations != null)
                        {
                            foreach (Annotation anno in page.Annotations)
                            {
                                if (anno.Id == Guid.Empty)
                                {
                                    anno.PageId = page.Id;
                                    // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                                    anno.CreatedBy = LoginUser.UserName;
                                    anno.CreatedOn = DateTime.Now;
                                    anno.ModifiedBy = null;
                                    anno.ModifiedOn = null;
                                    anno.DocId = document.Id;
                                    anno.DocTypeId = document.DocTypeId;
                                    anno.LineEndAt = anno.LineEndAt != null ? anno.LineEndAt : "TopLeft";
                                    anno.LineStartAt = anno.LineStartAt != null ? anno.LineStartAt : "TopLeft";
                                    anno.LineStyle = anno.LineStyle != null ? anno.LineStyle : "ArrowAtEnd";
                                    // 2014/08/15 - HungLe - End - Adding code save create date by time of service

                                    if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                                    {
                                        anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                                    }

                                    annotationDao.Add(anno);
                                }
                                else
                                {
                                    // 2014/08/15 - HungLe - Start - Adding code save create date by time of service
                                    anno.ModifiedBy = LoginUser.UserName;
                                    anno.ModifiedOn = DateTime.Now;
                                    anno.DocId = document.Id;
                                    anno.DocTypeId = document.DocTypeId;
                                    anno.LineEndAt = anno.LineEndAt != null ? anno.LineEndAt : "TopLeft";
                                    anno.LineStartAt = anno.LineStartAt != null ? anno.LineStartAt : "TopLeft";
                                    anno.LineStyle = anno.LineStyle != null ? anno.LineStyle : "ArrowAtEnd";
                                    // 2014/08/15 - HungLe - End - Adding code save create date by time of service

                                    if (fromMobile && "text".Equals(anno.Type, StringComparison.OrdinalIgnoreCase))
                                    {
                                        anno.Content = string.Format(ANNO_TEXT_FORMAT, anno.Content);
                                    }

                                    annotationDao.Update(anno);
                                }
                            }
                        }
                        #endregion

                        // 2014/08/15 - HungLe - Start - Adding delete anno page
                        if (page.DeleteAnnotations != null)
                        {
                            foreach (var delAnnoId in page.DeleteAnnotations)
                            {
                                annotationDao.Delete(delAnnoId);
                            }
                        }
                        // 2014/08/15 - HungLe - End - Adding delete anno page
                    }
                }
            }
            #endregion

            #region DocIndex

            if (canUpdateIndexValue)
            {
                TableFieldValueDao tableValueDao = new TableFieldValueDao(context);
                if (document.DocTypeId != Guid.Empty && canUpdateIndexValue
                    && document.FieldValues != null && document.FieldValues.Count > 0)
                {
                    foreach (DocumentFieldValue fieldValue in document.FieldValues)
                    {
                        fieldDao.Update(fieldValue);
                        if (fieldValue.TableFieldValue != null && fieldValue.TableFieldValue.Count > 0)
                        {
                            var keepTablesIds = new List<Guid>(fieldValue.TableFieldValue.Count);

                            foreach (var tableValue in fieldValue.TableFieldValue)
                            {
                                tableValue.DocId = document.Id;
                                if (tableValue.Field != null)
                                {
                                    if (tableValue.Field.DataTypeEnum == FieldDataType.Date)
                                    {
                                        int index = tableValue.Value.IndexOf(" ");
                                        if (index != -1)
                                        {
                                            tableValue.Value = tableValue.Value.Substring(0, index);
                                        }
                                    }
                                }

                                if (tableValue.Id == Guid.Empty)
                                {
                                    tableValueDao.Add(tableValue);
                                }
                                else
                                {
                                    tableValueDao.Update(tableValue);
                                }

                                keepTablesIds.Add(tableValue.Id);
                            }

                            tableValueDao.DeleteIfNotInListId(document.Id, keepTablesIds);
                        }

                        // 2014/08/15 - HungLe - Start - Adding delete table filed value
                        if (fieldValue.DeleteTableFieldValueIds != null)
                        {
                            foreach (var delTablFieldId in fieldValue.DeleteTableFieldValueIds)
                            {
                                tableValueDao.Delete(delTablFieldId);
                            }
                        }
                        // 2014/08/15 - HungLe - End - Adding delete table filed value
                    }

                    ActionLogHelper.AddActionLog("Update document field value on document Id: " + document.Id,
                                                LoginUser, ActionName.UpdateFieldValue, null, null, context);
                }
            }
            #endregion
        }

        private void ResetPageNumber(List<Page> pages, PageDao pageDao)
        {
            int pageIndex = 1;

            foreach (Page page in pages)
            {
                page.PageNumber = pageIndex;
                pageDao.UpdatePageNumber(page);
                pageIndex++;
            }
        }

        private void UpdateDocumentWorkflowStatus(Batch batch, BatchDao workItemDao, DapperContext context)
        {
            workItemDao.UpdateStatus(batch);

            ActionLogHelper.AddActionLog("Change workflow status on batch Id: " + batch.Id,
                                            LoginUser, ActionName.UpdateFieldValue, null, null, context);
        }

        private void ValidateBatch(Batch batch, DapperContext context)
        {
            BatchDao batchDao = new BatchDao(context);
            Batch existingBatch = batchDao.GetById(batch.Id);

            if (existingBatch == null)
            {
                throw new Exception(ErrorMessages.BatchNotExist);
            }

            if (existingBatch.LockedBy != null && existingBatch.LockedBy != LoginUser.UserName)
            {
                throw new Exception(ErrorMessages.BatchLocked);
            }

            if (existingBatch.IsCompleted)
            {
                throw new Exception(ErrorMessages.BatchCompleted);
            }

            if (existingBatch.IsProcessing)
            {
                throw new Exception(ErrorMessages.BatchInProcess);
            }
        }

        private bool ValidateFieldValue(List<DocumentFieldValue> fieldValues)
        {
            foreach (DocumentFieldValue fieldValue in fieldValues)
            {
                if (!string.IsNullOrEmpty(fieldValue.FieldMetaData.ValidationScript))
                {
                    string scriptValue = fieldValue.FieldMetaData.ValidationScript.Replace("<<Value>>", fieldValue.Value);
                    string script = CSharpScriptEngine.script.Replace("<<ScriptHere>>", scriptValue);
                    Assembly ass = CSharpScriptEngine.CompileCode(script);

                    if (!CSharpScriptEngine.RunScript(ass))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private DocumentTypePermission GetDocumentTypePermission(DapperContext context, List<Guid> groupIds, Guid docTypeId)
        {
            DocumentTypePermission permission = new DocumentTypePermission();
            List<DocumentTypePermission> permissions = new DocumentTypePermissionDao(context).GetByGroupRangeAndDocType(groupIds, docTypeId);

            for (int i = 0; i < permissions.Count; i++)
            {
                var per = permissions[i];
                if (i == 0)
                {
                    permission = per;
                }
                else
                {
                    permission.CanAccess |= per.CanAccess;
                }
            }

            return permission;
        }

        private DocumentPermission GetDocumentPermission(ActivityPermission permission, List<Guid> groupIds, Guid docTypeId)
        {
            DocumentPermission docPermission = new DocumentPermission();
            List<DocumentPermission> permissions = permission.DocumentPermissions.Where(p => groupIds.Contains(p.UserGroupId) && p.DocTypeId == docTypeId).ToList();

            for (int i = 0; i < permissions.Count; i++)
            {
                var per = permissions[i];
                if (i == 0)
                {
                    docPermission = per;
                }
                else
                {
                    docPermission.CanSeeRestrictedField |= per.CanSeeRestrictedField;
                }
            }

            List<DocumentFieldPermission> fieldPermissions = new List<DocumentFieldPermission>();
            permissions.ForEach(p => fieldPermissions.AddRange(p.FieldPermissions));
            docPermission.FieldPermissions = GetFieldPermissionByUserAndDocType(groupIds, fieldPermissions);

            return docPermission;
        }

        public List<DocumentFieldPermission> GetFieldPermissionByUserAndDocType(List<Guid> groupIds, List<DocumentFieldPermission> fieldPermissions)
        {
            List<DocumentFieldPermission> permissionByUsers = new List<DocumentFieldPermission>();

            int count = 0;

            foreach (Guid groupId in groupIds)
            {
                List<DocumentFieldPermission> permissionByGroups = fieldPermissions.Where(p => p.UserGroupId == groupId).ToList();
                var permis = permissionByGroups;

                if (count == 0)
                {
                    permissionByUsers = permis;
                }
                else
                {
                    for (int i = 0; i < permissionByGroups.Count; i++)
                    {
                        var per = permissionByGroups[i];
                        var permission = permissionByUsers.SingleOrDefault(p => p.FieldId == per.FieldId);
                        permission.CanRead |= per.CanRead;
                        permission.CanWrite |= per.CanWrite;
                    }
                }

                count++;
            }

            return permissionByUsers;
        }

        /* ------------------------------- Old (Stop using. will be deleted) ----------------------------------------
        public List<DocumentFieldPermission> GetFieldPermissionByUserAndDocType(DapperContext context, Guid docTypeId)
        {
            List<Guid> groupIds = null;

            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
            }

            List<DocumentFieldPermission> permissions = new DocumentFieldPermissionDao(context).GetByUserAndDocType(groupIds, docTypeId);
            List<DocumentFieldPermission> permissionByUsers = new List<DocumentFieldPermission>();

            int count = 0;

            foreach (Guid groupId in groupIds)
            {
                DocumentFieldPermission permission = new DocumentFieldPermission();
                List<DocumentFieldPermission> permissionByGroups = permissions.Where(p => p.UserGroupId == groupId).ToList();
                var permis = permissionByGroups;

                if (count == 0)
                {
                    permissionByUsers = permis;
                }
                else
                {
                    for (int i = 0; i < permissionByGroups.Count; i++)
                    {
                        var per = permissionByGroups[i];
                        if (i == 0)
                        {
                            permission = per;
                        }
                        else
                        {
                            permission.CanRead |= per.CanRead;
                            permission.CanWrite |= per.CanWrite;
                            permission.Hidden |= per.Hidden;
                        }

                    }
                }
                permissionByUsers.Add(permission);

                count++;
            }

            return permissionByUsers;
        }

         */

        private List<Batch> BuildAdvanceSearchData(DapperContext dataContext, Guid batchTypeId, SearchQuery query, int pageIndex)
        {
            const string SEARCH_TABLE = "#AdvanceSearch";

            var exprsBuilder = new StringBuilder();
            var hashSetColumns = new HashSet<string>();

            string value1;
            string value2;
            string conjunction;
            string searchOperator;
            string strFieldId;
            string fieldDataType;
            string columnSearch;
            string[] systemUniqueInfo;

            int tempInt;
            decimal tempDecimal;
            bool tempBool;
            DateTime tempDateTime1;
            DateTime tempDateTime2;

            var queryCreateColumns = new StringBuilder();
            var querySelectSystemColumns = new StringBuilder();
            var queryInsertSystemColumns = new StringBuilder();
            var querySelectNormalColumns = new StringBuilder();
            var queryWhereNormalColumns = new StringBuilder();

            foreach (var expr in query.SearchQueryExpressions)
            {
                #region
                value1 = string.Format("{0}", expr.Value1).Trim();

                // Just work with search query have the value 1
                if (value1 == string.Empty)
                {
                    continue;
                }

                conjunction = string.Format("{0}", expr.Condition).Trim().ToUpper();
                searchOperator = string.Format("{0}", expr.Operator).Trim().ToUpper();

                #region Check and make the conjunction have the length is 3 character
                if (conjunction == "OR")
                {
                    conjunction = "OR ";
                }
                else if (conjunction == string.Empty)
                {
                    conjunction = "AND";
                }
                else if (conjunction != "AND")
                {
                    throw new ArgumentException("Invalid search conjunction: " + conjunction);
                }
                #endregion

                // In case normal field, the left operand is id of field meta
                // Case system field, the left operand is unique id of field meta
                strFieldId = expr.FieldMetaData.Id == Guid.Empty ? expr.FieldMetaData.UniqueId
                                                                 : expr.FieldMetaData.Id.ToString();

                fieldDataType = string.Format("{0}", expr.FieldMetaData.DataType).ToUpper();
                columnSearch = BatchManager.MapSystemUniqueIdToName(strFieldId);

                switch (fieldDataType)
                {
                    case "PICKLIST":
                    case "STRING":
                        #region Type string
                        value1 = value1.Replace("'", "''");

                        if (searchOperator != "Equal" && searchOperator != "NotEqual")
                        {
                            value1 = value1.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
                        }

                        switch (searchOperator)
                        {
                            case "CONTAINS":
                                exprsBuilder.AppendFormat(" {0} [{1}] LIKE '%{2}%' ", conjunction, columnSearch, value1);
                                break;
                            case "NOTCONTAINS":
                                exprsBuilder.AppendFormat(" {0} [{1}] NOT LIKE '%{2}%' ", conjunction, columnSearch, value1);
                                break;

                            case "STARTSWITH":
                                exprsBuilder.AppendFormat(" {0} [{1}] LIKE '{2}%' ", conjunction, columnSearch, value1);
                                break;
                            case "ENDSWITH":
                                exprsBuilder.AppendFormat(" {0} [{1}] LIKE '%{2}' ", conjunction, columnSearch, value1);
                                break;

                            case "EQUAL":
                                exprsBuilder.AppendFormat(" {0} [{1}] = '{2}' ", conjunction, columnSearch, value1);
                                break;
                            case "NOTEQUAL":
                                exprsBuilder.AppendFormat(" {0} [{1}] <> '{2}' ", conjunction, columnSearch, value1);
                                break;

                            default:
                                throw new ArgumentException("Invalid search operation for type " + fieldDataType + ": " + searchOperator);
                        }
                        break;
                    #endregion

                    case "INTEGER":
                    case "DECIMAL":
                        #region Type integer or decimal

                        if (fieldDataType == "INTEGER")
                        {
                            if (!int.TryParse(value1, out tempInt))
                            {
                                throw new ArgumentException("Invalid type int of value1: " + value1);
                            }
                        }
                        else
                        {
                            if (!decimal.TryParse(value1, out tempDecimal))
                            {
                                throw new ArgumentException("Invalid type decimal of value1: " + value1);
                            }
                        }

                        switch (searchOperator)
                        {
                            case "EQUAL":
                                exprsBuilder.AppendFormat(" {0} [{1}] = {2} ", conjunction, columnSearch, value1);
                                break;
                            case "NOTEQUAL":
                                exprsBuilder.AppendFormat(" {0} [{1}] <> {2} ", conjunction, columnSearch, value1);
                                break;
                            case "GREATERTHAN":
                                exprsBuilder.AppendFormat(" {0} [{1}] > {2} ", conjunction, columnSearch, value1);
                                break;
                            case "GREATERTHANOREQUALTO":
                                exprsBuilder.AppendFormat(" {0} [{1}] >= {2} ", conjunction, columnSearch, value1);
                                break;
                            case "LESSTHAN":
                                exprsBuilder.AppendFormat(" {0} [{1}] < {2} ", conjunction, columnSearch, value1);
                                break;
                            case "LESSTHANOREQUALTO":
                                exprsBuilder.AppendFormat(" {0} [{1}] <= {2} ", conjunction, columnSearch, value1);
                                break;
                            case "INBETWEEN":
                                value2 = string.Format("{0}", expr.Value2).Trim();

                                if (fieldDataType == "INTEGER")
                                {
                                    if (!int.TryParse(value2, out tempInt))
                                    {
                                        throw new ArgumentException("Invalid type int of value2: " + value2);
                                    }
                                }
                                else
                                {
                                    if (!decimal.TryParse(value2, out tempDecimal))
                                    {
                                        throw new ArgumentException("Invalid type decimal of value2: " + value2);
                                    }
                                }
                                exprsBuilder.AppendFormat(" {0} ([{1}] >= {2} AND [{1}] <= {3}) ", conjunction, columnSearch, value1, value2);
                                break;
                            default:
                                throw new ArgumentException("Invalid search operation for number: " + searchOperator);
                        }
                        break;
                    #endregion

                    case "BOOLEAN":
                        #region Type bool

                        if (!bool.TryParse(value1, out tempBool))
                        {
                            throw new ArgumentException("Invalid type bool of value1: " + value1);
                        }

                        switch (searchOperator)
                        {
                            case "EQUAL":
                                exprsBuilder.AppendFormat(" {0} [{1}] = {2} ", conjunction, columnSearch, value1);
                                break;
                            case "NOTEQUAL":
                                exprsBuilder.AppendFormat(" {0} [{1}] <> {2} ", conjunction, columnSearch, value1);
                                break;
                            default:
                                throw new ArgumentException("Invalid search operation for type bool: " + searchOperator);
                        }
                        break;
                    #endregion

                    case "DATE":
                        #region Type date

                        if (!DateTime.TryParseExact(value1, "yyyy-MM-dd", null, DateTimeStyles.None, out tempDateTime1))
                        {
                            throw new ArgumentException("Invalid type date time with format 'yyyy-MM-dd' of value1: " + value1);
                        }

                        switch (searchOperator)
                        {
                            case "EQUAL":
                                exprsBuilder.AppendFormat(" {0} ([{1}] >= '{2}' AND [{1}] < '{3}') ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"), tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
                                break;
                            case "NOTEQUAL":
                                exprsBuilder.AppendFormat(" {0} ([{1}] < '{2}' OR [{1}] >= '{3}') ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"), tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
                                break;
                            case "GREATERTHAN":
                                exprsBuilder.AppendFormat(" {0} [{1}] >= '{2}' ", conjunction, columnSearch, tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
                                break;
                            case "GREATERTHANOREQUALTO":
                                exprsBuilder.AppendFormat(" {0} [{1}] >= '{2}' ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"));
                                break;
                            case "LESSTHAN":
                                exprsBuilder.AppendFormat(" {0} [{1}] < '{2}' ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"));
                                break;
                            case "LESSTHANOREQUALTO":
                                exprsBuilder.AppendFormat(" {0} [{1}] < '{2}' ", conjunction, columnSearch, tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
                                break;
                            case "INBETWEEN":
                                value2 = string.Format("{0}", expr.Value2).Trim();
                                if (!DateTime.TryParseExact(value2, "yyyy-MM-dd", null, DateTimeStyles.None, out tempDateTime2))
                                {
                                    throw new ArgumentException("Invalid type date time with format 'yyyy-MM-dd' of value2: " + value2);
                                }
                                exprsBuilder.AppendFormat(" {0} ([{1}] >= '{2}' AND [{1}] < '{3}') ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"), tempDateTime2.AddDays(1).ToString("yyyy-MM-dd"));
                                break;
                            default:
                                throw new ArgumentException("Invalid search operation for date time: " + searchOperator);
                        }
                        break;
                    #endregion

                    default:
                        break;
                }

                exprsBuilder.AppendLine();

                if (hashSetColumns.Contains(strFieldId))
                {
                    continue;
                }

                // Add to hash set for tracking
                hashSetColumns.Add(strFieldId);

                // Case system field
                if (expr.FieldMetaData.Id == Guid.Empty)
                {
                    #region

                    systemUniqueInfo = BatchManager.MapSystemUniqueIdInfo(strFieldId);
                    if (systemUniqueInfo == null)
                    {
                        throw new ArgumentException("Invalid system field unique id: " + strFieldId);
                    }

                    queryCreateColumns.AppendFormat(",[{0}] {1} ", systemUniqueInfo);
                    queryInsertSystemColumns.AppendFormat(",[{0}] ", systemUniqueInfo[0]);
                    querySelectSystemColumns.AppendFormat(",[{0}]", systemUniqueInfo[0]);

                    querySelectSystemColumns.AppendLine();
                    queryInsertSystemColumns.AppendLine();

                    #endregion
                }
                // Case normal field
                else
                {
                    #region

                    switch (fieldDataType)
                    {
                        case "STRING":
                        case "PICKLIST":
                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "NVARCHAR(MAX)");
                            break;
                        case "INTEGER":
                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "INT");
                            break;
                        case "DECIMAL":
                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "DECIMAL(38,10)");
                            break;
                        case "BOOLEAN":
                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "BIT");
                            break;
                        case "DATE":
                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "DATETIME");
                            break;
                    }

                    querySelectNormalColumns.AppendFormat(",[{0}] ", strFieldId);
                    queryWhereNormalColumns.AppendFormat("OR FieldId = '{0}' ", strFieldId);

                    querySelectNormalColumns.AppendLine();
                    queryWhereNormalColumns.AppendLine();

                    #endregion
                }

                queryCreateColumns.AppendLine();
                #endregion
            }

            // Check have at least on valid search condition
            if (queryCreateColumns.Length == 0)
            {
                throw new ArgumentException("Have no valid search expression.");
            }

            string querySearch;

            // In case search by only system fields
            // => just need search in table Batch
            if (querySelectNormalColumns.Length == 0)
            {
                #region
                querySearch = string.Format(@"
SELECT
    Id
FROM
(
    SELECT TOP (@To)
        ROW_NUMBER() OVER (ORDER BY Id) rowNumber,
        Id
    FROM
        Batch
    WHERE
        BatchTypeId = '{0}' -- batchTypeId
        AND (
{1} -- exprsBuilder
        )
) result
WHERE
    rowNumber BETWEEN @From AND @To
ORDER BY
    rowNumber
", batchTypeId, exprsBuilder.Remove(0, 4).ToString());
                #endregion
            }
            // In case search by only normal fields
            else if (queryInsertSystemColumns.Length == 0)
            {
                #region
                querySearch = string.Format(@"
IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
	DROP TABLE {0}; -- temp table name

CREATE TABLE {0} -- temp table name
(
    BatchId uniqueidentifier
    {1} -- queryCreateColumns
);

INSERT INTO {0} -- temp table name 
(
    BatchId,
    {2} -- querySelectNormalColumns
)
SELECT
    BatchId,
    {2} -- querySelectNormalColumns
FROM
(
	SELECT
		BatchId, FieldId, Value
	FROM
		BatchFieldValue
	WHERE 
		{3} -- queryWhereNormalColumns
) d
PIVOT
(
	MAX(Value)
	FOR FieldId IN (
        {2} -- querySelectNormalColumns
    )
) piv;

SELECT
    BatchId
FROM
(
    SELECT TOP (@To)
        ROW_NUMBER() OVER (ORDER BY BatchId) rowNumber,
        BatchId
    FROM
        {0} -- temp table name 
    WHERE
        {4} -- exprsBuilder
) result
WHERE
    rowNumber BETWEEN @From AND @To
ORDER BY
    rowNumber;

IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
	DROP TABLE {0}; -- temp table name
",
                SEARCH_TABLE,
                queryCreateColumns.ToString(),
                querySelectNormalColumns.Remove(0, 1).ToString(),
                queryWhereNormalColumns.Remove(0, 2).ToString(),
                exprsBuilder.Remove(0, 4).ToString());
                #endregion
            }
            // In case search by system and normal fields
            else
            {
                #region
                querySearch = string.Format(@"
IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
	DROP TABLE {0}; -- temp table name

CREATE TABLE {0} -- temp table name
(
    BatchId uniqueidentifier
    {1} -- queryCreateColumns
);

INSERT INTO {0} -- temp table name 
(
    BatchId,
    {2} -- querySelectNormalColumns
    {5} -- querySelectSystemColumns
)
SELECT
    BatchId,
    {2} -- querySelectNormalColumns
    {5} -- querySelectSystemColumns
FROM
(
	SELECT
		BatchId, FieldId, Value
	FROM
		BatchFieldValue
	WHERE 
		{3} -- queryWhereNormalColumns
) d
PIVOT
(
	MAX(Value)
	FOR FieldId IN (
        {2} -- querySelectNormalColumns
    )
) piv
INNER JOIN Batch ON piv.BatchId = Batch.Id;

SELECT
    BatchId
FROM
(
    SELECT TOP (@To)
        ROW_NUMBER() OVER (ORDER BY BatchId) rowNumber,
        BatchId
    FROM
        {0} -- temp table name 
    WHERE
        {4} -- exprsBuilder
) result
WHERE
    rowNumber BETWEEN @From AND @To
ORDER BY
    rowNumber;

IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
	DROP TABLE {0}; -- temp table name
",
                SEARCH_TABLE,
                queryCreateColumns.ToString(),
                querySelectNormalColumns.Remove(0, 1).ToString(),
                queryWhereNormalColumns.Remove(0, 2).ToString(),
                exprsBuilder.Remove(0, 4).ToString(),
                querySelectSystemColumns.ToString());
                #endregion
            }

            var batchIds = new SearchDao(dataContext).GetBatchFromSearch(null, querySearch, pageIndex);

            return new BatchDao(dataContext).GetBatchByRange(batchIds);
        }

        private static Dictionary<string, string[]> _dicSystemUniqueIdInfo;

        static BatchManager()
        {
            _dicSystemUniqueIdInfo = new Dictionary<string, string[]>(17);

            _dicSystemUniqueIdInfo.Add(Commons.FieldBatchName, new string[] { "BatchName", "NVARCHAR(250)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldCreatedBy, new string[] { "CreatedBy", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldCreatedDate, new string[] { "CreatedDate", "DATETIME" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldModifiedBy, new string[] { "ModifiedBy", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldModifiedDate, new string[] { "ModifiedDate", "DATETIME" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldLockedBy, new string[] { "LockedBy", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldLastAccessedBy, new string[] { "LastAccessedBy", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldLastAccessedDate, new string[] { "LastAccessedDate", "DATETIME" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldBlockingBookmark, new string[] { "BlockingBookmark", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldBlockingActivityName, new string[] { "BlockingActivityName", "NVARCHAR(1024)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldBlockingActivityDescription, new string[] { "BlockingActivityDescription", "NVARCHAR(1024)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldBlockingDate, new string[] { "BlockingDate", "DATETIME" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldDocumentCount, new string[] { "DocCount", "INT" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldPageCount, new string[] { "PageCount", "INT" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldDelegatedBy, new string[] { "DelegatedBy", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldDelegatedTo, new string[] { "DelegatedTo", "NVARCHAR(50)" });
            _dicSystemUniqueIdInfo.Add(Commons.FieldStatusMsg, new string[] { "StatusMsg", "NVARCHAR(MAX)" });

        }

        private static string[] MapSystemUniqueIdInfo(string uniqueId)
        {
            return _dicSystemUniqueIdInfo.ContainsKey(uniqueId) ? _dicSystemUniqueIdInfo[uniqueId]
                                                                : null;
        }
        private static string MapSystemUniqueIdToName(string uniqueId)
        {
            return _dicSystemUniqueIdInfo.ContainsKey(uniqueId) ? _dicSystemUniqueIdInfo[uniqueId][0] : uniqueId;
        }

        private DataColumn GetColumn(string name, FieldDataType dataType)
        {
            Type type = typeof(string);
            switch (dataType)
            {
                case FieldDataType.String:
                    type = typeof(string);
                    break;
                case FieldDataType.Date:
                    type = typeof(DateTime);
                    break;
                case FieldDataType.Decimal:
                    type = typeof(decimal);
                    break;
                case FieldDataType.Integer:
                    type = typeof(int);
                    break;
            }

            return new DataColumn(name, type);
        }

        private string GetColumnType(FieldDataType dataType)
        {
            string sqlType = "VARCHAR(50)";

            switch (dataType)
            {
                case FieldDataType.String:
                case FieldDataType.Picklist:
                    return "VARCHAR(MAX)";
                case FieldDataType.Integer:
                    return "INT";
                case FieldDataType.Decimal:
                    return "DECIMAL(38,10)";
                case FieldDataType.Boolean:
                    return "BIT";
                case FieldDataType.Date:
                    return "DATETIME";
                default:
                    return sqlType;
            }
        }

        private string GetDocFolder(Document doc)
        {
            string path = Path.Combine(doc.DocumentType.BatchTypeId.ToString(), doc.BatchId.ToString(), doc.Id.ToString());

            return path;
        }

        private string GetLooseDocFolder(Batch batch, Document doc)
        {
            string path = Path.Combine(batch.BatchTypeId.ToString(), batch.Id.ToString(), doc.Id.ToString());

            return path;
        }

        private bool CheckPermission(Guid userId, Batch item, DapperContext context)
        {
            if (item.BlockingBookmark == "AUTORESUME" || string.IsNullOrWhiteSpace(item.BlockingBookmark))
            {
                return true;
            }

            CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(context);
            List<Guid> userGroupIds;
            List<Guid> docTypeIds;

            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {
                userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
            }

            BatchType batchType = new BatchTypeDao(context).GetById(item.BatchTypeId);

            docTypeIds = new DocumentTypeDao(context).GetDocumentTypeByBatch(batchType.Id).Select(p => p.Id).ToList();

            //bool hasPermission = new WorkflowHumanStepPermissionDao(dataContext).GetWorkflowHumanStepPermission(userGroupIds, item.BatchTypeId).Count > 0;
            bool hasPermission = false;

            CustomActivitySetting customActivity = customActivityDao.GetCustomActivitySetting(item.WorkflowDefinitionId, Guid.Parse(item.BlockingBookmark));

            if (customActivity == null)
            {
                return false;
            }

            ActivityPermission permission = (ActivityPermission)Utility.UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);

            hasPermission = permission.AnnotationPermissions.Any(p => userGroupIds.Contains(p.UserGroupId)) ||
                            permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId));


            return hasPermission;
        }

        ///// <summary>
        ///// Check batch work-flow permission.
        ///// </summary>
        ///// <param name="batch">Batch use want to check.</param>
        ///// <param name="userGroupIds">User group id of current login user.</param>
        ///// <param name="customActivityDao">Custom activity setting DAO.</param>
        ///// <returns></returns>
        //private bool CheckSearchWorkflowPermission(Batch batch,
        //                                           List<Guid> userGroupIds,
        //                                           CustomActivitySettingDao customActivityDao)
        //{

        //    // Get custom activity
        //    var customActivity = customActivityDao.GetCustomActivitySetting(batch.WorkflowDefinitionId,
        //                                                                    Guid.Parse(batch.BlockingBookmark));
        //    // Check null object
        //    if (customActivity == null)
        //    {
        //        return false;
        //    }

        //    // Get permission
        //    var permission = UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);

        //    return permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId));
        //}


        //Workflow

        private void CallWorkflow(Batch batch, bool isNewDocument, params object[] resumeData)
        {
            WorkflowDefinition workflow;
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                BatchDao batchDao = new BatchDao(dataContext);
                WorkflowDefinitionDao workflowDefinitionDao = new WorkflowDefinitionDao(dataContext);

                if (isNewDocument)
                {
                    workflow = new WorkflowDefinitionDao(dataContext).GetByBatchTypeId(batch.BatchTypeId);
                    batch.WorkflowDefinitionId = workflow.Id;
                    batchDao.UpdateWorkflowDefinitionToBatch(batch.Id, workflow.Id);
                    batch.WorkflowInstanceId = Guid.Empty;
                }
                else
                {
                    workflow = new WorkflowDefinitionDao(dataContext).GetById(batch.WorkflowDefinitionId);
                }

                batch.IsProcessing = true;
                batch.StatusMsg = "Lock document to process workflow.";
                UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);

                WorkflowRuntimeData runtimeData = new WorkflowRuntimeData
                {
                    ObjectID = batch.Id,
                    ObjectType = WorkflowObjectType.Document,
                    User = LoginUser
                };

                Controller wfCore = new Controller(runtimeData, workflow.DefinitionXML, ConnectionStringEncryptionHelper.GetDecryptpedConnectionString(LoginUser.CaptureConnectionString, "D4A88355-7148-4FF2-A626-151A40F57330"), LoginUser);

                if (wfCore.HasError) // Initialization of workflow instance failed
                {
                    batch.HasError = true;
                    batch.IsProcessing = false;
                    UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);
                    return;
                }

                wfCore.WorkflowAborted += WorkflowAborted_Handler;
                wfCore.WorkflowBookmarked += WorkflowBookmarked_Handler;
                wfCore.WorkflowCompleted += WorkflowCompleted_Handler;

                if (isNewDocument)
                {
                    wfCore.StartWorkflow(LoginUser);
                }
                else
                {
                    if (resumeData != null && resumeData.Count() > 0)
                    {
                        // always get first param because the ResumeCallback method (of WF Engine) has single object input
                        wfCore.ResumeData = resumeData[0];
                    }

                    wfCore.ResumeWorkflow(batch.WorkflowInstanceId, batch.BlockingBookmark, LoginUser);
                }

                if (wfCore.HasError)
                {
                    batch.HasError = true;
                    batch.IsProcessing = false;
                    if (isNewDocument)
                    {
                        batch.StatusMsg = "Start Workflow error: " + wfCore.LastErrorMessage;
                    }
                    else
                    {
                        batch.StatusMsg = "Workflow resume error: " + wfCore.LastErrorMessage;
                    }

                    UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);
                }
            }

        }

        private void WorkflowAborted_Handler(WorkflowRuntimeData runtimeData, Guid workflowId, Exception reason)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                BatchDao batchDao = new BatchDao(dataContext);
                Batch batch = batchDao.GetById((Guid)runtimeData.ObjectID);

                batch.WorkflowInstanceId = workflowId;
                batch.IsProcessing = false;
                batch.IsCompleted = false;
                batch.HasError = true;
                batch.BlockingDate = DateTime.Now;
                batch.StatusMsg = "Workflow aborted: " + reason.Message;

                UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);

                dataContext.Commit();
            }
        }

        private void WorkflowBookmarked_Handler(WorkflowRuntimeData runtimeData, Guid workflowId, string activityName, string bookmarkName, bool hasError)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                BatchDao batchDao = new BatchDao(dataContext);

                Batch batch = batchDao.GetById((Guid)runtimeData.ObjectID);

                batch.WorkflowInstanceId = workflowId;
                batch.IsProcessing = false;
                batch.IsCompleted = false;
                batch.HasError = hasError;
                batch.BlockingDate = DateTime.Now;
                batch.StatusMsg = "Workflow bookmarked.";
                batch.BlockingBookmark = bookmarkName;
                batch.BlockingActivityName = activityName;

                UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);

                dataContext.Commit();
            }
        }

        private void WorkflowCompleted_Handler(WorkflowRuntimeData runtimeData, Guid workflowId, bool hasError)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                BatchDao batchDao = new BatchDao(dataContext);
                Batch batch = batchDao.GetById((Guid)runtimeData.ObjectID);

                batch.Id = (Guid)runtimeData.ObjectID;
                batch.WorkflowInstanceId = workflowId;
                batch.IsProcessing = false;
                batch.BlockingDate = DateTime.Now;

                UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);

                if (hasError)
                {
                    batch.HasError = true;
                    batch.IsCompleted = true;
                    batch.StatusMsg = "Workflow stopped with error.";
                    UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);
                }
                else
                {
                    //if (batch.IsRejected)
                    //{
                    //    DeleteBatchsData(new List<Guid> { batch.Id }, dataContext);
                    //}
                    //else
                    //{
                    batch.HasError = false;
                    batch.IsCompleted = true;
                    batch.StatusMsg = "Workflow completed successfully.";
                    UpdateDocumentWorkflowStatus(batch, batchDao, dataContext);

                    ReleaseBatch((Guid)runtimeData.ObjectID);
                    //}
                }
            }
        }

        //        #region New code

        /// <summary>
        /// Count the item in each status.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <param name="errorBatchCount"></param>
        /// <param name="inProcessingBatchCount"></param>
        /// <param name="lockedBatchCount"></param>
        /// <param name="availableBatchCount"></param>
        /// <param name="rejectedBatchCount"></param>
        public void CountBatchs(Guid batchTypeId,
                                out int errorBatchCount,
                                out int inProcessingBatchCount,
                                out int lockedBatchCount,
                                out int availableBatchCount,
                                out int rejectedBatchCount)
        {
            using (DapperContext captureContext = new DapperContext(LoginUser))
            {
                // Get all batch
                var allBatches = new BatchDao(captureContext).GetByBatchType(batchTypeId);
                // Filter batch with BlockingBookmark <> "AUTORESUME";
                var batches = allBatches.Where(h => !"AUTORESUME".Equals(h.BlockingBookmark)).ToList();

                if (!LoginUser.IsAdmin)
                {
                    List<Guid> userGroupIds;

                    // Get the group Ids of user
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id)
                                                                       .Select(h => h.Id).ToList();
                    }

                    batches = batches.Where(h => this.CanAccessWorkBatch(captureContext, h, userGroupIds)).ToList();
                }

                // Count item
                errorBatchCount = batches.Count(p => p.HasError);
                inProcessingBatchCount = batches.Count(p => p.IsProcessing);
                lockedBatchCount = batches.Count(p => !string.IsNullOrEmpty(p.LockedBy));
                rejectedBatchCount = batches.Count(p => p.IsRejected && string.IsNullOrEmpty(p.LockedBy));
                availableBatchCount = batches.Count(p => string.IsNullOrEmpty(p.LockedBy)
                                                         && !p.IsProcessing
                                                         && !p.IsCompleted
                                                         && !p.IsRejected
                                                         && !p.HasError);
            }
        }
        /// <summary>
        /// Count the item in each status.
        /// </summary>
        /// <param name="errorBatchCount"></param>
        /// <param name="availableBatchCount"></param>
        /// <param name="rejectedBatchCount"></param>
        public void CountBatchs(out int errorBatchCount, out int availableBatchCount, out int rejectedBatchCount)
        {
            using (DapperContext captureContext = new DapperContext(LoginUser))
            {
                List<Batch> batches = new List<Batch>();
                BatchDao batchDao = new BatchDao(captureContext);
                if (LoginUser.IsAdmin)
                {
                    // Get all batch
                    batches = batchDao.GetAllBatch();
                }
                else
                {
                    List<Guid> userGroupIds;
                    // Get the group Ids of user
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id)
                                                                       .Select(h => h.Id).ToList();
                    }

                    batches = batchDao.GetAllBatch(userGroupIds);
                }

                // Count item
                errorBatchCount = batches.Count(p => p.HasError);
                rejectedBatchCount = batches.Count(p => p.IsRejected && string.IsNullOrEmpty(p.LockedBy));
                availableBatchCount = batches.Count(p => string.IsNullOrEmpty(p.LockedBy)
                                                         && !p.IsProcessing
                                                         && !p.IsCompleted
                                                         && !p.IsRejected
                                                         && !p.HasError);
            }
        }

        public List<Comment> GetTopComment()
        {
            using (DapperContext captureContext = new DapperContext(LoginUser))
            {
                List<Batch> batches = new List<Batch>();
                List<Comment> comments = new List<Comment>();

                BatchDao batchDao = new BatchDao(captureContext);
                CommentDao commentDao = new CommentDao(captureContext);

                if (LoginUser.IsAdmin)
                {
                    // Get all batch
                    batches = batchDao.GetAllBatch();
                }
                else
                {
                    List<Guid> userGroupIds;
                    // Get the group Ids of user
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id)
                                                                       .Select(h => h.Id).ToList();
                    }

                    batches = batchDao.GetAllBatch(userGroupIds);
                }

                foreach (Batch batch in batches)
                {
                    var commentList = commentDao.GetByInstance(batch.Id);
                    Comment comment = commentList.FirstOrDefault();

                    if (comment != null)
                    {
                        comment.BatchName = batch.BatchName;

                        using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                        {
                            UserPrimaryDao userDao = new UserPrimaryDao(primaryContext);
                            comment.Photo = userDao.GetByUserName(comment.CreatedBy).Photo;
                        }

                        comments.Add(comment);
                    }
                }

                return comments;
            }

        }

        /// <summary>
        /// Get to
        /// </summary>
        /// <returns></returns>
        public ListCommentOptimize GetTopCommentOptimize()
        {
            using (DapperContext captureContext = new DapperContext(LoginUser))
            {
                List<Batch> batches = new List<Batch>();
                List<Comment> comments = new List<Comment>();
                Dictionary<string, string> photos = new Dictionary<string, string>();


                BatchDao batchDao = new BatchDao(captureContext);
                CommentDao commentDao = new CommentDao(captureContext);
                Ecm.Context.DapperContext primaryContext = new Context.DapperContext();

                if (LoginUser.IsAdmin)
                {
                    // Get all batch
                    batches = batchDao.GetAllBatch();
                }
                else
                {
                    List<Guid> userGroupIds;
                    // Get the group Ids of user
                    userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id)
                                                                   .Select(h => h.Id).ToList();

                    batches = batchDao.GetAllBatch(userGroupIds);
                }

                var userDao = new UserPrimaryDao(primaryContext); ;
                foreach (Batch batch in batches)
                {
                    var commentList = commentDao.GetByInstance(batch.Id);
                    Comment comment = commentList.FirstOrDefault();

                    if (comment == null)
                    {
                        continue;
                    }

                    comments.Add(comment);
                    comment.BatchName = batch.BatchName;

                    if (photos.ContainsKey(comment.CreatedBy))
                    {
                        continue;
                    }

                    var photo = userDao.GetByUserName(comment.CreatedBy).Photo;
                    if (photo == null)
                    {
                        continue;
                    }

                    #region Generate thumbnail
                    try
                    {

                        using (MemoryStream ms = new MemoryStream(photo))
                        {
                            var image = Image.FromStream(ms);

                            // Figure out the ratio
                            double ratioX = (double)24 / (double)image.Width;
                            double ratioY = (double)24 / (double)image.Height;
                            // use whichever multiplier is smaller
                            double ratio = ratioX < ratioY ? ratioX : ratioY;

                            // now we can get the new height and width
                            int newHeight = Convert.ToInt32(image.Height * ratio);
                            int newWidth = Convert.ToInt32(image.Width * ratio);

                            // A holder for the thumbnail
                            Bitmap result = new Bitmap(image, newWidth, newHeight);

                            using (MemoryStream jpgStream = new MemoryStream())
                            {
                                result.Save(jpgStream, ImageFormat.Jpeg);
                                var phot0Base64 = Convert.ToBase64String(jpgStream.ToArray());
                                photos.Add(comment.CreatedBy, phot0Base64);
                            }
                        }
                    }
                    catch { }
                    #endregion

                }

                primaryContext.Dispose();

                var listComments = new ListCommentOptimize();
                listComments.Photos = photos;
                listComments.Comments = comments;

                return listComments;
            }

        }


        /// <summary>
        /// Get information and thumbnail of page by doc id.
        /// </summary>
        /// <param name="docId">Id of document.</param>
        /// <returns></returns>
        public List<Page> GetThumbnailPagesByDoc(Guid docId, int thumbWidth, int thumbHeight,
                                                           int pageIndex, int itemPerPage)
        {
            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                // Get doc in table "Document"
                var docDao = new DocumentDao(context);
                var doc = docDao.GetById(docId);

                // Get batch in table "Batch"
                var batchDao = new BatchDao(context);
                var batch = batchDao.GetById(doc.BatchId);

                // Check permission to access batch
                if (!LoginUser.IsAdmin && !this.CheckPermission(LoginUser.Id, batch, context))
                {
                    throw new Exception(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, batch.Id));
                }

                // Get page in table "Page"
                var pageDao = new PageDao(context);
                var pages = pageDao.GetByDoc(doc.Id).OrderBy(h => h.PageNumber).ToList();

                // Get annotation permission
                var permission = AnnotationPermission.GetAllowAll();
                if (!LoginUser.IsAdmin)
                {
                    permission = this.GetWorkItemDocumentTypeAnnotationPermission(LoginUser.Id,
                                                                                  batch.WorkflowDefinitionId,
                                                                                  Guid.Parse(batch.BlockingBookmark),
                                                                                  doc.DocTypeId,
                                                                                  context);
                }

                var thumbnailPages = new List<Page>(pages.Count);
                var annoDao = new AnnotationDao(context);

                // Paging
                var pagingHelper = new PagingHelper(LoginUser);
                pages = pagingHelper.PagingMobile(pages, pageIndex, itemPerPage);

                foreach (var page in pages)
                {
                    var pageInfo = new Page()
                    {
                        Id = page.Id,
                        IsRejected = page.IsRejected,
                        FileExtension = page.FileExtension,
                        PageNumber = page.PageNumber,
                        RotateAngle = page.RotateAngle
                    };
                    thumbnailPages.Add(pageInfo);

                    #region Generate thumbnail
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(page.FileBinary))
                        {
                            var image = Image.FromStream(ms);

                            // A holder for the thumbnail
                            Bitmap result = new Bitmap(image, thumbWidth, thumbHeight);

                            using (MemoryStream jpgStream = new MemoryStream())
                            {
                                result.Save(jpgStream, ImageFormat.Jpeg);
                                pageInfo.FileBinaryBase64 = Convert.ToBase64String(jpgStream.ToArray());
                            }
                        }
                    }
                    catch
                    {
                        pageInfo.FileBinaryBase64 = null;
                    }
                    #endregion

                    #region Scale annotation
                    double scaleX = thumbWidth / page.Width;
                    double scaleY = thumbHeight / page.Height;

                    // Get anno in table "Annotation" with permission
                    var annoes = annoDao.GetByPage(page.Id);

                    if (!LoginUser.IsAdmin)
                    {
                        if (!permission.CanSeeHighlight && !permission.CanSeeText)
                        {
                            annoes = annoes.Where(h => h.Type != Constants.ANNO_TYPE_HIGHLIGHT &&
                                                       h.Type != Constants.ANNO_TYPE_TEXT).ToList();
                        }
                        else if (!permission.CanSeeHighlight)
                        {
                            annoes = annoes.Where(h => h.Type != Constants.ANNO_TYPE_HIGHLIGHT).ToList();
                        }
                        else if (!permission.CanSeeText)
                        {
                            annoes = annoes.Where(h => h.Type != Constants.ANNO_TYPE_TEXT).ToList();
                        }
                    }

                    // Set annotation information
                    pageInfo.Annotations = annoes.Select(h => new Annotation()
                    {
                        Id = h.Id,
                        Content = h.Content != null ? h.Content.Replace("\"", "\\\"") : string.Empty,
                        CreatedBy = h.CreatedBy,
                        CreatedOn = h.CreatedOn,
                        DocId = h.DocId,
                        DocTypeId = h.DocTypeId,
                        Width = h.Width * scaleX,
                        Height = h.Height * scaleY,
                        RotateAngle = h.RotateAngle,
                        Left = h.Left * scaleX,
                        Top = h.Top * scaleY,
                        Type = h.Type,
                        LineColor = h.LineColor,
                        LineEndAt = h.LineEndAt,
                        LineStartAt = scaleY.ToString(),
                        LineStyle = h.LineStyle,
                        LineWeight = h.LineWeight,
                        ModifiedBy = scaleY.ToString(),
                        ModifiedOn = h.ModifiedOn,
                        PageId = h.PageId
                    }).ToList();
                    #endregion
                }

                return thumbnailPages;
            }
        }

        /// <summary>
        /// Get page by id (Use in mobile).
        /// </summary>
        /// <param name="id">Id of page</param>
        /// <returns></returns>
        public Page GetWorkPage(Guid id)
        {
            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                // Get page in table "Page"
                var pageDao = new PageDao(context);
                var page = pageDao.GetById(id);

                // Get doc in table "Document"
                var docDao = new DocumentDao(context);
                var doc = docDao.GetById(page.DocId);

                // Get batch in table "Batch"
                var batchDao = new BatchDao(context);
                var batch = batchDao.GetById(doc.BatchId);

                // Check permission to access batch
                if (!LoginUser.IsAdmin && !this.CheckPermission(LoginUser.Id, batch, context))
                {
                    throw new Exception(string.Format(ErrorMessages.NoPermission, LoginUser.UserName, batch.Id));
                }

                // Generate FileBinaryBase64
                if ("tiff".Equals(page.FileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    using (MemoryStream ms = new MemoryStream(page.FileBinary))
                    {
                        var image = Image.FromStream(ms);

                        // A holder for the thumbnail
                        Bitmap result = new Bitmap(image, (int)Math.Round(page.Width), (int)Math.Round(page.Height));

                        using (MemoryStream jpgStream = new MemoryStream())
                        {
                            result.Save(jpgStream, ImageFormat.Jpeg);
                            page.FileBinaryBase64 = Convert.ToBase64String(jpgStream.ToArray());
                            page.FileExtension = "jpeg";
                        }
                    }
                }
                else if (page.FileBinary != null)
                {
                    page.FileBinaryBase64 = Convert.ToBase64String(page.FileBinary);
                }
                // Do not use in mobile => use FileBinaryBase64
                page.FileBinary = null;

                // Get annotation in table "Annotation"
                var annoDao = new AnnotationDao(context);
                page.Annotations = annoDao.GetByPage(id);

                return page;
            }
        }

        public List<string> GetLockedWortItemNames()
        {
            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                var batchTypeDao = new BatchTypeDao(context);
                var batchDao = new BatchDao(context);

                List<BatchType> canAccessBatchTypes;
                List<Guid> groupIds = null;

                // Get list batch type associate with user
                if (LoginUser.IsAdmin)
                {
                    // Get by role admin
                    canAccessBatchTypes = batchTypeDao.GetCapturedBatchTypes();
                }
                else
                {
                    // Get by group user if do not have role admin
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }
                    canAccessBatchTypes = batchTypeDao.GetAssignedBatchTypes(groupIds);
                }

                var lockedBatches = new List<Batch>();
                var userName = LoginUser.UserName;
                foreach (var batchType in canAccessBatchTypes)
                {
                    // Get locked batch
                    var batches = batchDao.GetByBatchType(batchType.Id)
                                          .Where(h => h.LockedBy == userName && "AUTORESUME" != (h.BlockingBookmark))
                                          .ToList();
                    lockedBatches.AddRange(batches);
                }

                // Sorting
                lockedBatches = lockedBatches.OrderByDescending(h => h.CreatedDate).ToList();

                return lockedBatches.Select(h => h.BatchName).ToList();
            }
        }

        /// <summary>
        /// Check the access work Batch.
        /// </summary>
        /// <param name="captureContext">Current capture context.</param>
        /// <param name="batch">Batch</param>
        /// <param name="userGroupIds">List user group id.</param>
        /// <returns></returns>
        /// <remarks>Use for capture client</remarks>
        private bool CanAccessWorkBatch(DapperContext captureContext, Batch batch, List<Guid> userGroupIds)
        {
            var customActivityDao = new CustomActivitySettingDao(captureContext);

            var bookmarkId = Guid.Empty;
            if (!Guid.TryParse(batch.BlockingBookmark, out bookmarkId))
            {
                return false;
            }

            // Get custom activity
            var customActivity = customActivityDao.GetCustomActivitySetting(batch.WorkflowDefinitionId, bookmarkId);
            if (customActivity == null)
            {
                return false;
            }

            var permission = UtilsSerializer.Deserialize<ActivityPermission>(customActivity.Value);
            if (permission == null)
            {
                return false;
            }

            var canAccess = permission.UserGroupPermissions.Any(p => userGroupIds.Contains(p.UserGroupId));

            // Just check UserGroupPermission is enough
            return canAccess;
        }
    }
}
