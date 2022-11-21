using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;

namespace CaptureMVC.DataProvider
{
    public class SearchQueryProvider : ProviderBase
    {
        /// <summary>
        /// Get list saved SearchQuery (just get information in table SearchQuery).
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        public List<SearchQuery> GetSavedQueriesLight(Guid batchTypeId)
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetSavedQueriesLight(batchTypeId);
            }
        }

        /// <summary>
        /// Get saved SearchQuery (exclude information of BatchType).
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public SearchQuery GetSavedQuery(Guid queryId)
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetSavedQuery(queryId);
            }
        }

        public Guid SaveQuery(SearchQuery searchQuery)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.SaveQuery(searchQuery);
            }
        }

        public bool IsQueryNameExisted(Guid batchTypeId, string queryName)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.QueryExisted(batchTypeId, queryName);
            }
        }

        public void DeleteQuery(Guid queryId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteQuery(queryId);
            }
        }
    }
}