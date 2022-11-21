using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class DocumentDao
    {
        private readonly DapperContext _context;

        public DocumentDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Document obj)
        {
            const string query = @"INSERT INTO [Document] 
                                          ([DocTypeId],[BinaryType],[PageCount],[Version],[CreatedDate],[CreatedBy])
                            OUTPUT inserted.ID
                                   VALUES (@DocTypeID,@BinaryType,@PageCount,@Version,@CreatedDate,@CreatedBy)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocTypeID = obj.DocTypeId,
                                                             BinaryType = obj.BinaryType,
                                                             PageCount = obj.PageCount,
                                                             Version = obj.Version,
                                                             CreatedDate = obj.CreatedDate,
                                                             CreatedBy = obj.CreatedBy
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE DocTypeId = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public Document GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [Document] 
                                   WHERE ID = @ID";
            return _context.Connection.Query<Document>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<Document> GetByDocType(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [Document] 
                                   WHERE DocTypeID = @DocTypeID";
            return _context.Connection.Query<Document>(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public void Update(Document obj)
        {
            const string query = @"UPDATE [Document]
                                   SET   [BinaryType] = @BinaryType,
                                         [PageCount] = @PageCount,
                                         [Version] = @Version,
                                         [ModifiedDate] = @ModifiedDate,
                                         [ModifiedBy] = @ModifiedBy
                                   WHERE ID=@ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                BinaryType = obj.BinaryType,
                                                PageCount = obj.PageCount,
                                                Version = obj.Version,
                                                ModifiedDate = obj.ModifiedDate,
                                                ModifiedBy = obj.ModifiedBy,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public List<Document> GetByRange(List<Guid> ids)
        {
            const string query = @"SELECT * 
                                   FROM [Document] 
                                   WHERE Id IN @list";
            var list = ids.ToArray<Guid>();
            return _context.Connection.Query<Document>(query, new { list }, _context.CurrentTransaction).ToList();
        }
    }
}