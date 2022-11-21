using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
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
            const string query = @"INSERT INTO [SearchQueryExpression] ([SearchQueryId],[Condition],[FieldId],[Operator],[Value1],[Value2],[FieldUniqueId])
                                OUTPUT inserted.ID
                                   VALUES (@SearchQueryId,@Condition,@FieldId,@Operator,@Value1,@Value2,@FieldUniqueId)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             SearchQueryId = obj.SearchQueryId,
                                                             Condition = obj.Condition,
                                                             FieldId = obj.FieldId,
                                                             Operator = obj.Operator,
                                                             Value1 = obj.Value1,
                                                             Value2 = obj.Value2,
                                                             FieldUniqueId = obj.FieldUniqueId
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(List<Guid> ids)
        {
            var Ids = ids.ToArray();
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE ID IN @Ids";
            _context.Connection.Execute(query, new { Ids }, _context.CurrentTransaction);
        }

        public void DeleteBySearchQuery(Guid queryId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE SearchQueryID = @QueryID";
            _context.Connection.Execute(query, new { QueryID = queryId }, _context.CurrentTransaction);
        }

        public List<SearchQueryExpression> GetBySearchQuery(Guid searchQueryId)
        {
            const string query = @"SELECT * 
                                   FROM [SearchQueryExpression] 
                                   WHERE SearchQueryID = @SearchQueryID";
            return _context.Connection.Query<SearchQueryExpression>(query, new { SearchQueryID = searchQueryId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE FieldID = @FieldID";
            _context.Connection.Execute(query, new { FieldID = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE FieldID IN (SELECT FieldID
                                                     FROM FieldMetaData
                                                     WHERE DocTypeID = @DocTypeID)";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByUser(Guid userId)
        {
            const string query = @"DELETE FROM [SearchQueryExpression] 
                                   WHERE SearchQueryID IN (SELECT ID
                                                           FROM SearchQuery
                                                           WHERE UserID = @UserID)";
            _context.Connection.Execute(query, new { UserID = userId }, _context.CurrentTransaction);
        }

        public void Update(SearchQueryExpression obj)
        {
            const string query = @"UPDATE [SearchQueryExpression]
                                   SET [SearchQueryId] = @SearchQueryID,
                                       [Condition] = @Condition,
                                       [FieldId] = @FieldID,
                                       [Operator] = @Operator,
                                       [Value1] = @Value1,
                                       [Value2] = @Value2
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                SearchQueryID = obj.SearchQueryId,
                                                Condition = obj.Condition,
                                                FieldID = obj.FieldId,
                                                Operator = obj.Operator,
                                                Value1 = obj.Value1,
                                                Value2 = obj.Value2,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }
    }
}