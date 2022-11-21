using CaptureMVC.Models;
using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptureMVC.DataProvider
{
    public class WorkItemProvider : ProviderBase
    {

        public void ApproveWorkItems(List<Guid> Ids)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.ApproveWorkItems(Ids);
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
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.DeleteWorkItems(Ids);
            }
        }

        public void DelegateWorkItems(List<Guid> ids, string toUser, string delegateNote)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.DelegateWorkItems(ids, toUser, delegateNote);
            }
        }

        /// <summary>
        /// Count items of each status in specified batch type.
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
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.CountBatchs(batchTypeId,
                                           out errorBatchCount,
                                           out inProcessingBatchCount,
                                           out lockedBatchCount,
                                           out availableBatchCount,
                                           out rejectedBatchCount);
            }
        }

        /// <summary>
        /// Get the batches by BatchType and status.
        /// </summary>
        /// <param name="batchTypeId">Guid of Batches's BatchType.</param>
        /// <param name="status">Status of Batches</param>
        /// <param name="pageIndex">Start by 1, set 0 to get all results.</param>
        /// <param name="itemsPerPage">Items per one page.</param>
        /// <returns></returns>
        public WorkItemSearchResult GetBatchByStatus(Guid batchTypeId, BatchStatus status,
                                                     int pageIndex, int itemsPerPage)
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetBatches(batchTypeId, status, pageIndex);
            }
        }

        public WorkItemSearchResult GetBatchByAdvanceSearch(SearchQuery searchQuery, int pageIndex)
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetAdvancedSearchWorkItems(searchQuery, pageIndex);
            }
        }

        public Batch GetWorkItem(Guid Id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.OpenWorkItem(Id);
            }
        }

        /// <summary>
        /// Save the work item in current step
        /// </summary>
        /// <param name="workItem"></param>
        public void SaveWorkItem(Batch workItem)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                client.Channel.SaveWorkItem(workItem);
            }
        }

        /// <summary>
        /// Reject or approve work item
        /// Like save and approve or save and reject.
        /// </summary>
        /// <param name="workItem"></param>
        public void ApproveOrRejectWorkItem(Batch workItem)
        {
            using (var client = GetOneTimeCaptureClientChannel())
            {
                if (workItem.IsRejected)
                {
                    client.Channel.UpdateWorkItem(workItem);
                }
                else
                {
                    var listBatch = new List<Batch>() { workItem };
                    client.Channel.ApproveWorkItems(listBatch);
                }

            }
        }



        //public WorkItemSearchResultModel GetLockedBatchs(Guid batchTypeId, int pageIndex)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return GetSearchResult(client.Channel.GetLockedBatchs(batchTypeId, pageIndex));
        //    }
        //}

        //public WorkItemSearchResultModel GetErrorBatchs(Guid batchTypeId, int pageIndex)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return GetSearchResult(client.Channel.GetErrorBatchs(batchTypeId, pageIndex));
        //    }
        //}

        //public WorkItemSearchResultModel GetInProcessingBatch(Guid batchTypeId, int pageIndex)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return GetSearchResult(client.Channel.GetProcessingBatchs(batchTypeId, pageIndex));
        //    }
        //}

        //public WorkItemSearchResultModel GetWaitingBatchs(Guid batchTypeId, int pageIndex)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return GetSearchResult(client.Channel.GetBatchsByBatchType(batchTypeId, pageIndex));
        //    }
        //}

        //public WorkItemSearchResultModel GetRejectedBatchs(Guid batchTypeId, int pageIndex)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return GetSearchResult(client.Channel.GetRejectedBatch(batchTypeId, pageIndex));
        //    }
        //}

        //public WorkItemSearchResultModel GetBatchByStatus(string currentSearch, Guid batchTypeId, int pageIndex)
        //{
        //    switch (currentSearch)
        //    {
        //        case Common.BATCH_ERROR:
        //            return GetErrorBatchs(batchTypeId, pageIndex);
        //        case Common.BATCH_IN_PROCESSING:
        //            return GetInProcessingBatch(batchTypeId, pageIndex);
        //        case Common.BATCH_LOCKED:
        //            return GetLockedBatchs(batchTypeId, pageIndex);
        //        case Common.BATCH_WAITING:
        //            return GetWaitingBatchs(batchTypeId, pageIndex);
        //    }

        //    return null;
        //}

        //public List<BatchModel> GetBatchs(List<Guid> ids)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return ObjectMapper.GetBatchModels(client.Channel.GetBatchs(ids));
        //    }

        //}

        //public List<BatchModel> GetBatchs(Guid batchTypeId)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return ObjectMapper.GetBatchModels(client.Channel.GetBatchs(batchTypeId));
        //    }

        //}

        //public void InsertBatch(BatchModel batch)
        //{
        //    using (var client = GetOneTimeCaptureClientChannel())
        //    {
        //        client.Channel.InsertBatch(ObjectMapper.GetBatch(batch));
        //    }
        //}

        //public void UpdateWorkItem(BatchModel workItem)
        //{
        //    using (var client = GetOneTimeCaptureClientChannel())
        //    {
        //        client.Channel.UpdateWorkItem(ObjectMapper.GetBatch(workItem));
        //    }
        //}

        //public void SaveWorkItem(BatchModel workItem)
        //{
        //    using (var client = GetOneTimeCaptureClientChannel())
        //    {
        //        client.Channel.SaveWorkItem(ObjectMapper.GetBatch(workItem));
        //    }
        //}

        #region Private methods



        #endregion
    }
}
