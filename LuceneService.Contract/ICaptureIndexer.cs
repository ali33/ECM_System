using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ecm.CaptureDomain;

namespace Ecm.LuceneService.Contract
{
    [ServiceContract]
    public interface ICaptureIndexer
    {
        [OperationContract]
        void CreateIndex(string authorizeId, Batch batch);

        [OperationContract]
        void UpdateIndex(string authorizeId, Batch batch);

        [OperationContract]
        void DeleteIndex(string authorizeId, Batch batch);

        [OperationContract]
        void DeleteBatchType(BatchType batchType);

        [OperationContract]
        void DeleteField(BatchType batchType, BatchFieldMetaData field);

        [OperationContract]
        WorkItemSearchResult RunAdvanceSearch(string authorizeId, BatchType batchType, SearchQuery query, int pageIndex, int pageSize);
        
        [OperationContract]
        WorkItemSearchResult RunSearchByBatch(string authorizeId, BatchType batchType, BatchStatus status, int pageIndex, int pageSize);

        //[OperationContract]
        //List<SearchResult> RunGlobalSearch(string authorizeId, string keyword, List<BatchType> batchTypes, int pageIndex, int pageSize);
    }
}
