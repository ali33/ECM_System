using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class SearchQueryDao
    {
        private readonly DapperContext _context;

        public SearchQueryDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(SearchQuery obj)
        {
            const string query = @"INSERT INTO [SearchQuery] ([UserId],[BatchTypeId],[Name])
                                OUTPUT inserted.ID     
                                   VALUES (@UserId,@BatchTypeId,@Name)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             UserId = obj.UserId,
                                                             BatchTypeId = obj.BatchTypeId,
                                                             Name = obj.Name
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [SearchQuery] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [SearchQuery] 
                                   WHERE BatchTypeId = @BatchTypeId";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByUser(Guid userId)
        {
            const string query = @"DELETE FROM [SearchQuery] 
                                   WHERE UserId = @UserId";
            _context.Connection.Execute(query, new { UserId = userId }, _context.CurrentTransaction);
        }

        public bool QueryExisted(Guid batchTypeId, string queryName, Guid userId)
        {
            const string query = @"SELECT * 
                                   FROM [SearchQuery]
                                   WHERE UserId=@UserId AND
                                         BatchTypeId=@BatchTypeId AND
                                         Name=@QueryName";
            return _context.Connection.Query<SearchQuery>(query, new { UserId = userId, BatchTypeId = batchTypeId, QueryName = queryName }, _context.CurrentTransaction).Any();
        }

        public List<SearchQuery> GetByUserAndBatchType(Guid userId, Guid batchTypeId)
        {
            const string query = @"SELECT * 
                                   FROM SearchQuery 
                                   WHERE UserId = @UserId AND 
                                         BatchTypeId = @BatchTypeId";
            return _context.Connection.Query<SearchQuery>(query, new { UserId = userId, BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public SearchQuery GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM SearchQuery 
                                   WHERE Id = @Id";
            return _context.Connection.Query<SearchQuery>(query, new { Id = id }, _context.CurrentTransaction).SingleOrDefault();
        }
    }
}