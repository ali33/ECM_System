using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Ecm.CaptureDomain;
using System.Collections.ObjectModel;

namespace Ecm.CaptureModel.DataProvider
{
    public class WorkItemProvider : ProviderBase
    {
        public void InsertBatch(BatchModel batch)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.InsertBatch(ObjectMapper.GetBatch(batch));
            }
        }

        public void UpdateWorkItem(BatchModel workItem)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.UpdateWorkItem(ObjectMapper.GetBatch(workItem));
            }
        }

        public void SaveWorkItem(BatchModel workItem)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.SaveWorkItem(ObjectMapper.GetBatch(workItem));
            }
        }

        public WorkItemSearchResultModel RunAdvanceSearch(int pageIndex, Guid batchTypedId, SearchQueryModel searchQuery)
        {
            using (var client = GetCaptureClientChannel())
            {
                var result = client.Channel.RunAdvanceSearchWorkItem(batchTypedId, ObjectMapper.GetSearchQuery(searchQuery), pageIndex);
                var resultModel = GetSearchResult(result);//, searchQuery);

                return resultModel;
            }
        }

        //public WorkItemSearchResultModel RunAdvanceSearch(int pageIndex, Guid batchTypedId, string searchQuery)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        var result = client.Channel.RunAdvanceSearchWorkItem(batchTypedId, searchQuery, pageIndex);
        //        var resultModel = GetSearchResult(result);

        //        return resultModel;
        //    }
        //}

        public void ApproveWorkItems(List<BatchModel> batchs)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.ApproveWorkItems(ObjectMapper.GetBatchs(batchs));
            }
        }

        public void ApproveWorkItems(List<Guid> Ids)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.ApproveWorkItems(Ids);
            }
        }

        public void RejectWorkItems(List<BatchModel> batchs, string rejectNote)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.RejectWorkItems(ObjectMapper.GetBatchs(batchs), rejectNote);
            }
        }

        public void RejectWorkItems(List<Guid> ids, string rejectNote)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.RejectWorkItems(ids, rejectNote);
            }
        }

        public void ResumeWorkItems(List<Guid> Ids)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.ResumeWorkItems(Ids);
            }
        }

        public void UnLockWorkItems(List<Guid> Ids)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.UnLockWorkItems(Ids);
            }
        }

        public void LockWorkItems(List<Guid> Ids)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.LockBatchs(Ids);
            }
        }

        public void DeleteWorkItems(List<Guid> Ids)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteWorkItems(Ids);
            }
        }

        public void DelegateWorkItems(List<Guid> ids, string toUser, string delegatedComment)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.DelegateWorkItems(ids, toUser, delegatedComment);
            }
        }

        public BatchModel GetWorkItem(Guid Id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchModel(client.Channel.OpenWorkItem(Id));
            }
        }

        public WorkItemSearchResultModel GetLockedBatchs(Guid batchTypeId, int pageIndex)
        {
            using (var client = GetCaptureClientChannel())
            {
                return GetSearchResult(client.Channel.GetLockedBatchs(batchTypeId, pageIndex));
            }
        }

        public WorkItemSearchResultModel GetErrorBatchs(Guid batchTypeId, int pageIndex)
        {
            using (var client = GetCaptureClientChannel())
            {
                return GetSearchResult(client.Channel.GetErrorBatchs(batchTypeId, pageIndex));
            }
        }

        public WorkItemSearchResultModel GetInProcessingBatch(Guid batchTypeId, int pageIndex)
        {
            using (var client = GetCaptureClientChannel())
            {
                return GetSearchResult(client.Channel.GetProcessingBatchs(batchTypeId, pageIndex));
            }
        }

        public WorkItemSearchResultModel GetWaitingBatchs(Guid batchTypeId, int pageIndex)
        {
            using (var client = GetCaptureClientChannel())
            {
                return GetSearchResult(client.Channel.GetBatchsByBatchType(batchTypeId, pageIndex));
            }
        }

        public WorkItemSearchResultModel GetRejectedBatchs(Guid batchTypeId, int pageIndex)
        {
            using (var client = GetCaptureClientChannel())
            {
                return GetSearchResult(client.Channel.GetRejectedBatch(batchTypeId, pageIndex));
            }
        }

        public WorkItemSearchResultModel GetBatchByStatus(string currentSearch, Guid batchTypeId, int pageIndex)
        {
            switch (currentSearch)
            {
                case Common.BATCH_ERROR:
                    return GetErrorBatchs(batchTypeId, pageIndex);
                case Common.BATCH_IN_PROCESSING:
                    return GetInProcessingBatch(batchTypeId, pageIndex);
                case Common.BATCH_LOCKED:
                    return GetLockedBatchs(batchTypeId, pageIndex);
                case Common.BATCH_WAITING:
                    return GetWaitingBatchs(batchTypeId, pageIndex);
            }

            return null;
        }

        public List<BatchModel> GetBatchs(List<Guid> ids)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchModels(client.Channel.GetBatchs(ids));
            }

        }

        public List<BatchModel> GetBatchs(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchModels(client.Channel.GetBatchs(batchTypeId));
            }

        }

        public void CountBatchs(Guid batchTypeId, out int errorBatchCount, out int inProcessingBatchCount, out int lockedBatchCount, out int availableBatchCount, out int rejectedBatchCount)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.CountBatchs(batchTypeId, out errorBatchCount, out inProcessingBatchCount, out lockedBatchCount, out availableBatchCount, out rejectedBatchCount);
            }
        }
        //Private method
        private WorkItemSearchResultModel GetSearchResult(WorkItemSearchResult searchResult)//, SearchQueryModel searchQuery)
        {
            if (searchResult != null && searchResult.BatchType != null)
            {
                return new WorkItemSearchResultModel
                {
                    BatchType = ObjectMapper.GetBatchTypeModel(searchResult.BatchType),
                    DataResult = GetData(searchResult.BatchType, searchResult.WorkItems),
                    //SearchQuery = searchQuery,
                    PageIndex = searchResult.PageIndex,
                    HasMoreResult = searchResult.HasMoreResult
                };
            }

            return null;
        }
        private DataTable GetData(BatchType batchType, IEnumerable<Batch> workItems)
        {
            DataTable dataTable = BuildSchema(batchType);
            foreach (var workItem in workItems)
            {
                var dataRow = dataTable.NewRow();
                dataRow[Common.COLUMN_SELECTED] = false;
                dataRow[Common.COLUMN_CREATED_BY] = workItem.CreatedBy;
                dataRow[Common.COLUMN_CREATED_ON] = workItem.CreatedDate;
                dataRow[Common.COLUMN_MODIFIED_BY] = workItem.ModifiedBy;
                dataRow[Common.COLUMN_MODIFIED_ON] = (object)workItem.ModifiedDate ?? DBNull.Value;
                dataRow[Common.COLUMN_PAGE_COUNT] = workItem.PageCount;
                dataRow[Common.COLUMN_DOCUMENT_COUNT] = workItem.DocCount;
                dataRow[Common.COLUMN_BATCH_ID] = workItem.Id;
                dataRow[Common.COLUMN_BATCH_TYPE_ID] = batchType.Id;
                //dataRow[Common.COLUMN_BINARY_TYPE] = (FileTypeModel)Enum.Parse(typeof(FileTypeModel), workItem.BinaryType, true);

                // Workflow
                dataRow[Common.COLUMN_LOCKED_BY] = workItem.LockedBy;
                dataRow[Common.COLUMN_WORKFLOW_INSTANCE_ID] = workItem.WorkflowInstanceId;
                dataRow[Common.COLUMN_WORKFLOW_DEFINITION_ID] = workItem.WorkflowDefinitionId;
                dataRow[Common.COLUMN_BLOCKING_BOOKMARK] = workItem.BlockingBookmark;
                dataRow[Common.COLUMN_BLOCKING_ACTIVITY_NAME] = workItem.BlockingActivityName;
                dataRow[Common.COLUMN_BLOCKING_ACTIVITY_DESCRIPTION] = workItem.BlockingActivityDescription;
                dataRow[Common.COLUMN_BLOCKING_DATE] = (object)workItem.BlockingDate ?? DBNull.Value;
                dataRow[Common.COLUMN_LAST_ACCESSED_DATE] = (object)workItem.LastAccessedDate ?? DBNull.Value;
                dataRow[Common.COLUMN_LAST_ACCESSED_BY] = workItem.LastAccessedBy;
                dataRow[Common.COLUMN_IS_COMPLETED] = workItem.IsCompleted;
                dataRow[Common.COLUMN_IS_PROCESSING] = workItem.IsProcessing;
                dataRow[Common.COLUMN_HAS_ERROR] = workItem.HasError;
                dataRow[Common.COLUMN_STATUS] = workItem.StatusMsg;
                dataRow[Common.COLUMN_PERMISSION] = workItem.BatchPermission;
                dataRow[Common.COLUMN_IS_REJECTED] = workItem.IsRejected;

                foreach (var fieldValue in workItem.FieldValues)
                {
                    if (fieldValue.FieldMetaData == null || fieldValue.FieldMetaData.DataTypeEnum == FieldDataType.Table ||
                        fieldValue.FieldMetaData.IsSystemField)
                    {
                        continue;
                    }

                    dataRow[fieldValue.FieldMetaData.Name] = ConvertData(fieldValue.Value, fieldValue.FieldMetaData.DataTypeEnum);
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        private DataTable BuildSchema(BatchType batchType)
        {
            var table = new DataTable(batchType.Name);
            table.Columns.Add(GetColumn(Common.COLUMN_SELECTED, FieldDataType.Boolean));
            foreach (var field in batchType.Fields)
            {
                if (field.DataTypeEnum == FieldDataType.Table ||
                    field.DataTypeEnum == FieldDataType.Folder ||
                    field.IsSystemField)
                {
                    continue;
                }

                table.Columns.Add(GetColumn(field.Name, field.DataTypeEnum));
            }

            table.Columns.Add(Common.COLUMN_CREATED_BY, typeof(string));
            table.Columns.Add(Common.COLUMN_CREATED_ON, typeof(DateTime));
            table.Columns.Add(Common.COLUMN_MODIFIED_BY, typeof(string));
            table.Columns.Add(Common.COLUMN_MODIFIED_ON, typeof(DateTime));
            table.Columns.Add(Common.COLUMN_PAGE_COUNT, typeof(int));
            table.Columns.Add(Common.COLUMN_DOCUMENT_COUNT, typeof(int));
            table.Columns.Add(Common.COLUMN_VERSION, typeof(int));
            table.Columns.Add(Common.COLUMN_BATCH_ID, typeof(Guid));
            table.Columns.Add(Common.COLUMN_BATCH_TYPE_ID, typeof(Guid));
            //table.Columns.Add(Common.COLUMN_BINARY_TYPE, typeof(FileTypeModel));
            //table.Columns.Add(Common.COLUMN_DOCUMENT, typeof(DocumentModel));

            // Workflow
            table.Columns.Add(Common.COLUMN_LOCKED_BY, typeof(string));
            table.Columns.Add(Common.COLUMN_WORKFLOW_INSTANCE_ID, typeof(Guid));
            table.Columns.Add(Common.COLUMN_WORKFLOW_DEFINITION_ID, typeof(Guid));
            table.Columns.Add(Common.COLUMN_BLOCKING_BOOKMARK, typeof(string));
            table.Columns.Add(Common.COLUMN_BLOCKING_ACTIVITY_NAME, typeof(string));
            table.Columns.Add(Common.COLUMN_BLOCKING_ACTIVITY_DESCRIPTION, typeof(string));
            table.Columns.Add(Common.COLUMN_BLOCKING_DATE, typeof(DateTime));
            table.Columns.Add(Common.COLUMN_LAST_ACCESSED_DATE, typeof(DateTime));
            table.Columns.Add(Common.COLUMN_LAST_ACCESSED_BY, typeof(string));
            table.Columns.Add(Common.COLUMN_IS_COMPLETED, typeof(bool));
            table.Columns.Add(Common.COLUMN_IS_PROCESSING, typeof(bool));
            table.Columns.Add(Common.COLUMN_HAS_ERROR, typeof(bool));
            table.Columns.Add(Common.COLUMN_STATUS, typeof(string));
            table.Columns.Add(Common.COLUMN_PERMISSION, typeof(BatchPermission));
            table.Columns.Add(Common.COLUMN_IS_REJECTED, typeof(bool));
            return table;
        }

        private DataColumn GetColumn(string name, FieldDataType dataType)
        {
            Type type = typeof(string);
            switch (dataType)
            {
                case FieldDataType.Boolean:
                    type = typeof(bool);
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

        public static object ConvertData(string value, FieldDataType dataType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DBNull.Value;
            }

            switch (dataType)
            {
                case FieldDataType.Boolean:
                    return Convert.ToBoolean(value);
                case FieldDataType.Date:
                    DateTime dateValue;
                    if (DateTime.TryParse(value, out dateValue))
                    {
                        return dateValue;
                    }

                    return DateTime.ParseExact(value, "yyyyMMdd", Thread.CurrentThread.CurrentCulture); // From lucene
                case FieldDataType.Decimal:
                    return Convert.ToDecimal(value);
                case FieldDataType.Integer:
                    return Convert.ToInt32(value);
            }

            return value;
        }

        public Guid GetTransactionId(Guid batchId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetTransactionId(batchId);
            }
        }
    }
}
