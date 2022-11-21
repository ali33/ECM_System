using System;
using System.Collections.Generic;

namespace ArchiveMVC5.Models.DataProvider
{
    public class DocumentProvider : ProviderBase
    {
        public DocumentProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// Thêm Documents từ danh sách DocumentModel
        /// </summary>
        /// <param name="documents">Danh sách DocumentModel cần thêm</param>
        public void InsertDocuments(List<DocumentModel> documents)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.InsertDocuments(ObjectMapper.GetDocuments(documents));
            }
        }



        /// <summary>
        /// Cập nhật Documents từ danh sách DocumentModel
        /// </summary>
        /// <param name="documents">Danh sách DocumentModel cần cập nhật</param>
        /// <returns></returns>
        public IList<DocumentModel> UpdateDocuments(List<DocumentModel> documents)
        {
            using (var client = GetArchiveClientChannel())
            {
                try
                {
                    var doc = client.Channel.UpdateDocuments(ObjectMapper.GetDocuments(documents));
                    return ObjectMapper.GetDocumentModels(doc);
                }
                catch {
                    return null;
                }
            }
        }


        /// <summary>
        /// Lấy Docment theo id
        /// </summary>
        /// <param name="id">id Document cần lấy</param>
        /// <returns></returns>
        public DocumentModel GetDocument(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentModel(client.Channel.GetDocument(id));
            }
        }


        /// <summary>
        /// Lấy danh sách Document theo danh danh sách ids
        /// </summary>
        /// <param name="ids">Danh sách ids của Document cần lấy</param>
        /// <returns></returns>
        public List<DocumentModel> GetDocuments(List<Guid> ids)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentModels(client.Channel.GetDocuments(ids));
            }
        }


        /// <summary>
        /// Xóa Document theo id
        /// </summary>
        /// <param name="id">id Document cần xóa</param>
        public void DeleteDocument(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteDocument(id);
            }
        }
    }
}
