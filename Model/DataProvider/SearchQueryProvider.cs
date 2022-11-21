using System;
using System.Collections.Generic;

namespace Ecm.Model.DataProvider
{
    public class SearchQueryProvider : ProviderBase
    {
        public IList<SearchQueryModel> GetSavedQueries(Guid docTypeId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetSearchQueries(client.Channel.GetSavedQueries(docTypeId));
            }
        }

        public Guid SaveQuery(SearchQueryModel searchQuery)
        {
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.SaveQuery(ObjectMapper.GetSearchQuery(searchQuery));
            }
        }

        public bool QueryExisted(Guid docTypeId, string name)
        {
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.QueryExisted(docTypeId, name);
            }
        }

        public void DeleteQuery(Guid queryId)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteQuery(queryId);
            }
        }
    }
}