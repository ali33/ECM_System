using System.Collections.Generic;
using System.ServiceModel;
using Ecm.Domain;

namespace Ecm.LuceneService.Contract
{
    [ServiceContract]
    public interface IIndexer
    {
        [OperationContract]
        void CreateIndex(string authorizeId, Document document);

        [OperationContract]
        void UpdateIndex(string authorizeId, Document document);

        [OperationContract]
        void DeleteIndex(string authorizeId, Document document);

        [OperationContract]
        void DeleteDocumentType(DocumentType documentType);

        [OperationContract]
        void DeleteField(DocumentType documentType, FieldMetaData field);

        [OperationContract]
        SearchResult RunAdvanceSearch(string authorizeId, DocumentType docType, SearchQuery query, int pageIndex, int pageSize, string sortColumn, string sortDir);

        [OperationContract]
        List<SearchResult> RunGlobalSearch(string authorizeId, string keyword, List<DocumentType> documentTypes, int pageIndex, int pageSize);

        [OperationContract]
        SearchResult RunSearchContent(string authorizeId, DocumentType docType, string text, int pageIndex, int pageSize);
    }
}
