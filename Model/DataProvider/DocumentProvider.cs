using System;
using System.Collections.Generic;

namespace Ecm.Model.DataProvider
{
    public class DocumentProvider : ProviderBase
    {
        public void InsertDocuments(List<DocumentModel> documents)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.InsertDocuments(ObjectMapper.GetDocuments(documents));
            }
        }

        public IList<DocumentModel> UpdateDocuments(List<DocumentModel> documents)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentModels(client.Channel.UpdateDocuments(ObjectMapper.GetDocuments(documents)));
            }
        }

        public DocumentModel GetDocument(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentModel(client.Channel.GetDocument(id));
            }
        }

        public List<DocumentModel> GetDocuments(List<Guid> ids)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentModels(client.Channel.GetDocuments(ids));
            }
        }

        public void DeleteDocument(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteDocument(id);
            }
        }
    }
}
