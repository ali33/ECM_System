using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class DocumentFieldVersionDao
    {
        private readonly DapperContext _context;

        public DocumentFieldVersionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentFieldVersion obj)
        {
            const string query = @"INSERT INTO [DocumentFieldVersion]([DocVersionID],[DocID],[FieldID],[Value])
                            OUTPUT inserted.ID                                   
                                VALUES (@DocVersionID,@DocID,@FieldID,@Value)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocVersionID = obj.DocVersionID,
                                                             DocID = obj.DocId,
                                                             FieldID = obj.FieldId,
                                                             Value = obj.Value
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public List<DocumentFieldVersion> GetByDocumentVersion(Guid documentVersionId)
        {
            const string query = @"SELECT * 
                                   FROM DocumentFieldVersion 
                                   WHERE DocVersionID = @DocVersionID";
            return _context.Connection.Query<DocumentFieldVersion>(query, new { DocVersionID = documentVersionId }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentFieldVersion> GetByDocument(Guid documentId)
        {
            const string query = @"SELECT * 
                                   FROM DocumentFieldVersion 
                                   WHERE DocId = @DocID";
            return _context.Connection.Query<DocumentFieldVersion>(query, new { DocID = documentId }, _context.CurrentTransaction).ToList();
        }

        public void UpdateFieldId(DocumentFieldVersion obj)
        {
            const string query = @"
                UPDATE [DocumentFieldVersion]
                SET FieldId = @FieldId
                WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Id = obj.Id,
                                            FieldId = obj.FieldId
                                        },
                                        _context.CurrentTransaction);
        }

    }
}