using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class SearchQueryExpressionDao
    {
        private readonly DapperContext _context;

        public SearchQueryExpressionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(SearchQueryExpression obj)
        {
            const string query = @"INSERT INTO [SearchQueryExpression] ([SearchQueryId],[Condition],[FieldId],[Operator],[Value1],[Value2])
                            OUTPUT inserted.ID                                   
                                VALUES (@SearchQueryId,@Condition,@FieldId,@Operator,@Value1,@Value2)
                                ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             SearchQueryId = obj.SearchQueryId,
                                                             Condition = obj.Condition,
                                                             FieldId = obj.FieldId,
                                                             Operator = obj.Operator,
                                                             Value1 = obj.Value1,
                                                             Value2 = obj.Value2
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(List<Guid> ids)
        {
            var Ids = ids.ToArray();
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE Id IN @Ids";
            _context.Connection.Execute(query, new { Ids }, _context.CurrentTransaction);
        }

        public void DeleteBySearchQuery(Guid queryId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE SearchQueryId = @QueryId";
            _context.Connection.Execute(query, new { QueryId = queryId }, _context.CurrentTransaction);
        }

        public List<SearchQueryExpression> GetBySearchQuery(Guid searchQueryId)
        {
            const string query = @"SELECT * 
                                   FROM [SearchQueryExpression] 
                                   WHERE SearchQueryId = @SearchQueryId";
            return _context.Connection.Query<SearchQueryExpression>(query, new { SearchQueryId = searchQueryId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE FieldId = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE FieldId IN (SELECT FieldId
                                                     FROM BatchFieldMetaData
                                                     WHERE BatchTypeId = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByUser(Guid userId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE SearchQueryId IN (SELECT Id
                                                           FROM SearchQuery
                                                           WHERE UserId = @UserId)";
            _context.Connection.Execute(query, new { UserId = userId }, _context.CurrentTransaction);
        }

        public void Update(SearchQueryExpression obj)
        {
            const string query = @"UPDATE [SearchQueryExpression]
                                   SET [SearchQueryId] = @SearchQueryId,
                                       [Condition] = @Condition,
                                       [FieldId] = @FieldId,
                                       [Operator] = @Operator,
                                       [Value1] = @Value1,
                                       [Value2] = @Value2
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                SearchQueryId = obj.SearchQueryId,
                                                Condition = obj.Condition,
                                                FieldId = obj.FieldId,
                                                Operator = obj.Operator,
                                                Value1 = obj.Value1,
                                                Value2 = obj.Value2,
                                                Id = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }
    }
}