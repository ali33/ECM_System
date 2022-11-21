using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
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
            const string query = @"INSERT INTO [SearchQuery] ([UserId],[DocTypeId],[Name])
                                OUTPUT inserted.ID
                                   VALUES (@UserId,@DocTypeId,@Name)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             UserID = obj.UserId,
                                                             DocTypeID = obj.DocTypeId,
                                                             Name = obj.Name
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [SearchQuery] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [SearchQuery] 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByUser(Guid userId)
        {
            const string query = @"DELETE FROM [SearchQuery] 
                                   WHERE UserID = @UserID";
            _context.Connection.Execute(query, new { UserID = userId }, _context.CurrentTransaction);
        }

        public bool QueryExisted(Guid docTypeId, string queryName, Guid userId)
        {
            const string query = @"SELECT * 
                                   FROM [SearchQuery]
                                   WHERE UserId=@UserId AND
                                         DocTypeID=@DocTypeID AND
                                         Name=@QueryName";
            return _context.Connection.Query<SearchQuery>(query, new { UserId = userId, DocTypeID = docTypeId, QueryName = queryName }, _context.CurrentTransaction).Any();
        }

        public List<SearchQuery> GetByUserAndDocType(Guid userId, Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM SearchQuery 
                                   WHERE UserID = @UserID AND 
                                         DocTypeID = @DocTypeID";
            return _context.Connection.Query<SearchQuery>(query, new { UserID = userId, DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public SearchQuery GetById(Guid queryId)
        {
            const string query = @"SELECT * 
                                   FROM SearchQuery 
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<SearchQuery>(query, new { Id = queryId}, _context.CurrentTransaction).FirstOrDefault();
        }


    }
}