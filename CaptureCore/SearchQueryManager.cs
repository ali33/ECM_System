using System.Collections.Generic;
using Ecm.CaptureDAO;
using Ecm.CaptureDomain;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDAO;
using System;

namespace Ecm.CaptureCore
{
    public class SearchQueryManager : ManagerBase
    {
        public SearchQueryManager(User loginUser)
            : base(loginUser)
        {
        }

        public bool QueryExisted(Guid docTypeId, string queryName)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new SearchQueryDao(dataContext).QueryExisted(docTypeId, queryName, LoginUser.Id);
            }
        }

        public void Save(SearchQuery searchQuery)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                dataContext.BeginTransaction();

                try
                {
                    SearchQueryExpressionDao searchQueryExpressionDao = new SearchQueryExpressionDao(dataContext);
                    if (searchQuery.Id == Guid.Empty)
                    {
                        new SearchQueryDao(dataContext).Add(searchQuery);
                        foreach (var expression in searchQuery.SearchQueryExpressions)
                        {
                            expression.SearchQueryId = searchQuery.Id;
                            searchQueryExpressionDao.Add(expression);
                        }
                    }
                    else
                    {
                        if (searchQuery.DeletedSearchQueryExpressions != null
                            && searchQuery.DeletedSearchQueryExpressions.Count > 0)
                        {
                            searchQueryExpressionDao.Delete(searchQuery.DeletedSearchQueryExpressions);
                        }

                        foreach (var searchExpression in searchQuery.SearchQueryExpressions)
                        {
                            if (searchExpression.Id == Guid.Empty)
                            {
                                searchQueryExpressionDao.Add(searchExpression);
                            }
                            else
                            {
                                searchQueryExpressionDao.Update(searchExpression);
                            }
                        }
                    }

                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public void DeleteQuery(Guid queryId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                dataContext.BeginTransaction();
                try
                {
                    new SearchQueryExpressionDao(dataContext).DeleteBySearchQuery(queryId);
                    new SearchQueryDao(dataContext).Delete(queryId);
                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public List<SearchQuery> GetSavedQueries(Guid docTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                List<SearchQuery> queries = new SearchQueryDao(dataContext).GetByUserAndBatchType(LoginUser.Id, docTypeId);

                SearchQueryExpressionDao searchQueryExpressionDao = new SearchQueryExpressionDao(dataContext);
                BatchFieldMetaDataDao fieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                BatchTypeDao batchTypeDao = new BatchTypeDao(dataContext);

                foreach (SearchQuery query in queries)
                {
                    query.SearchQueryExpressions = searchQueryExpressionDao.GetBySearchQuery(query.Id);

                    foreach (var expression in query.SearchQueryExpressions)
                    {
                        expression.FieldMetaData = fieldMetaDataDao.GetById(expression.FieldId);
                    }

                    query.BatchType = batchTypeDao.GetById(query.BatchTypeId);
                    query.BatchType.Fields = fieldMetaDataDao.GetByBatchType(query.BatchTypeId);
                }

                return queries;
            }
        }

        #region Mvc

        /// <summary>
        /// Get list saved SearchQuery (just get information in table SearchQuery).
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        public List<SearchQuery> GetSavedQueriesLight(Guid batchTypeId)
        {
            using (var dataContext = new DapperContext(LoginUser))
            {
                return new SearchQueryDao(dataContext).GetByUserAndBatchType(LoginUser.Id, batchTypeId);
            }
        }

        /// <summary>
        /// Get saved SearchQuery (exclude information of BatchType).
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public SearchQuery GetSavedQuery(Guid queryId)
        {
            using (var dataContext = new DapperContext(LoginUser))
            {
                var query = new SearchQueryDao(dataContext).GetById(queryId);

                var searchQueryExpressionDao = new SearchQueryExpressionDao(dataContext);
                var fieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);

                query.SearchQueryExpressions = searchQueryExpressionDao.GetBySearchQuery(query.Id);
                foreach (var expression in query.SearchQueryExpressions)
                {
                    expression.FieldMetaData = fieldMetaDataDao.GetById(expression.FieldId);
                }

                return query;
            }
        }

        #endregion
    }
}