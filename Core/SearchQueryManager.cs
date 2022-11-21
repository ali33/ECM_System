using System.Collections.Generic;
using Ecm.DAO;
using Ecm.Domain;
using Ecm.DAO.Context;
using System;

namespace Ecm.Core
{
    public class SearchQueryManager : ManagerBase
    {
        public SearchQueryManager(User loginUser) : base (loginUser)
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
                        if (searchQuery.DeletedSearchQueryExpressions.Count > 0)
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
                List<SearchQuery> queries = new SearchQueryDao(dataContext).GetByUserAndDocType(LoginUser.Id, docTypeId);

                SearchQueryExpressionDao searchQueryExpressionDao = new SearchQueryExpressionDao(dataContext);
                FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                DocTypeDao docTypeDao = new DocTypeDao(dataContext);

                foreach (SearchQuery query in queries)
                {
                    query.SearchQueryExpressions = searchQueryExpressionDao.GetBySearchQuery(query.Id);

                    foreach (var expression in query.SearchQueryExpressions)
                    {
                        expression.FieldMetaData = fieldMetaDataDao.GetById(expression.FieldId);
                    }

                    query.DocumentType = docTypeDao.GetById(query.DocTypeId);
                    query.DocumentType.FieldMetaDatas = fieldMetaDataDao.GetByDocType(query.DocTypeId);
                }

                return queries;
            }
        }

        public SearchQuery GetSavedQuery(Guid queryId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                SearchQuery query = new SearchQueryDao(dataContext).GetById(queryId);

                SearchQueryExpressionDao searchQueryExpressionDao = new SearchQueryExpressionDao(dataContext);
                FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                DocTypeDao docTypeDao = new DocTypeDao(dataContext);

                if (query != null)
                {
                    query.SearchQueryExpressions = searchQueryExpressionDao.GetBySearchQuery(query.Id);

                    foreach (var expression in query.SearchQueryExpressions)
                    {
                        expression.FieldMetaData = fieldMetaDataDao.GetById(expression.FieldId);
                    }

                    query.DocumentType = docTypeDao.GetById(query.DocTypeId);
                    query.DocumentType.FieldMetaDatas = fieldMetaDataDao.GetByDocType(query.DocTypeId);
                }

                return query;
            }
        }
    }
}