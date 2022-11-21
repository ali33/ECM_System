using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.DAO
{
    public class LinkDocumentDao
    {
        private readonly DapperContext _context;

        public LinkDocumentDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(LinkDocument obj)
        {
            const string query = @"INSERT INTO [LinkDocument] 
                                          ([DocumentId],[LinkDocumentId],[Notes])
                            OUTPUT inserted.ID
                                   VALUES (@DocumentId,@LinkDocumentId,@Notes)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         DocumentId = obj.DocumentId,
                                                         LinkDocumentId = obj.LinkDocumentId,
                                                         Notes = obj.Notes
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

        public void Update(LinkDocument obj)
        {
            const string query = @"UPDATE [LinkDocument]  
                                   SET [Notes] = @Notes
                                   WHERE [ID] = @ID";
            _context.Connection.Query<Guid>(query,new
                                                     {
                                                         Id = obj.DocumentId,
                                                         Notes = obj.Notes
                                                     },
                                                     _context.CurrentTransaction);
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [LinkDocument] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocument(Guid DocumentId)
        {
            const string query = @"DELETE FROM [LinkDocument] 
                                   WHERE DocumentID = @DocumentID";
            _context.Connection.Execute(query, new { ID = DocumentId }, _context.CurrentTransaction);
        }

        public LinkDocument GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [LinkDocument] 
                                   WHERE ID = @ID";
            return _context.Connection.Query<LinkDocument>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<LinkDocument> GetByDocumentId(Guid DocumentId)
        {
            const string query = @"SELECT * 
                                   FROM [LinkDocument] 
                                   WHERE DocumentID = @DocumentID";
            return _context.Connection.Query<LinkDocument>(query, new { DocumentID = DocumentId }, _context.CurrentTransaction).ToList();
        }

    }
}
