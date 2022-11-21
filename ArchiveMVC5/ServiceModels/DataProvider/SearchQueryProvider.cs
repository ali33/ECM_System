using System;
using System.Collections.Generic;

namespace ArchiveMVC5.Models.DataProvider
{
    public class SearchQueryProvider : ProviderBase
    {
        public SearchQueryProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// lấy các truy vấn được lưu trữ dựa vào id
        /// </summary>
        /// <param name="docTypeId">id truy vấn được lưu trữ</param>
        /// <returns></returns>
        public IList<SearchQueryModel> GetSavedQueries(Guid docTypeId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetSearchQueries(client.Channel.GetSavedQueries(docTypeId));
            }
        }
        /// <summary>
        /// Get query detail by queryid
        /// </summary>
        /// <param name="queryId">QueryID value</param>
        /// <returns></returns>
        public SearchQueryModel GetSavedQuery(Guid queryId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetSearchQueryModel(client.Channel.GetSavedQuery(queryId));
            }
        }
        /// <summary>
        /// lưu truy vấn
        /// </summary>
        /// <param name="searchQuery">mẫu truy vấn</param>
        /// <returns></returns>
        public Guid SaveQuery(SearchQueryModel searchQuery)
        {
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.SaveQuery(ObjectMapper.GetSearchQuery(searchQuery));
            }
        }


        /// <summary>
        /// kiểm tra truy vấn có tồn tại không
        /// </summary>
        /// <param name="docTypeId">Id loại tài liệu</param>
        /// <param name="name">tên truy vấn</param>
        /// <returns></returns>
        public bool QueryExisted(Guid docTypeId, string name)
        {
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.QueryExisted(docTypeId, name);
            }
        }
        /// <summary>
        /// xóa truy vấn dựa vào id
        /// </summary>
        /// <param name="queryId">id truy vấn cần xóa</param>

        public void DeleteQuery(Guid queryId)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteQuery(queryId);
            }
        }
    }
}