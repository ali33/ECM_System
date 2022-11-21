using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class DocumentVersionDao
    {
        private readonly DapperContext _context;

        public DocumentVersionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentVersion obj)
        {
            const string query = @"INSERT INTO [DocumentVersion] 
                                          ([DocTypeVersionID],[DocID],[DocTypeID],[BinaryType],[PageCount],[Version],
                                           [CreatedDate],[CreatedBy],[ChangeAction])
                                OUTPUT inserted.ID
                                   VALUES (@DocTypeVersionID,@DocID,@DocTypeID,@BinaryType,@PageCount,@Version,
                                           @CreatedDate,@CreatedBy,@ChangeAction)
                                   SELECT CAST (SCOPE_IDENTITY() as BIGINT)";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         DocTypeVersionID = obj.DocTypeVersionId,
                                                         DocID = obj.DocId,
                                                         DocTypeID = obj.DocTypeId,
                                                         BinaryType = obj.BinaryType,
                                                         PageCount = obj.PageCount,
                                                         Version = obj.Version,
                                                         CreatedDate = obj.CreatedDate,
                                                         CreatedBy = obj.CreatedBy,
                                                         ChangeAction = obj.ChangeAction
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

        public DocumentVersion GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentVersion] 
                                   WHERE ID = @ID";
            return _context.Connection.Query<DocumentVersion>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<DocumentVersion> GetByDoc(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentVersion] 
                                   WHERE DocID = @DocID";
            return _context.Connection.Query<DocumentVersion>(query, new { DocID = docId }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentVersion> GetByDocType(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentVersion] 
                                   WHERE DocTypeID = @DocTypeID";
            return _context.Connection.Query<DocumentVersion>(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentVersion> GetDeletedDocWithExistingDocType(Guid docTypeId)
        {
            const string query = @"SELECT * FROM [DocumentVersion] WHERE DocTypeID = @DocTypeID AND [ChangeAction] = 1 AND [DocTypeVersionID] IS NULL";
            return _context.Connection.Query<DocumentVersion>(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public void UpdateDocumentTypeVersionId(DocumentVersion documentVersion)
        {
            const string query = @"Update [DocumentVersion] SET [DocTypeVersionId] = @DocTypeVersionId WHERE [ID] = @ID";
            _context.Connection.Execute(query, new { DocTypeVersionID = documentVersion.DocTypeVersionId, ID = documentVersion.Id }, _context.CurrentTransaction);
        }

        public void UpdateDocumentTypeVersionIdAndChangeAction(Guid docTypeId, Guid docTypeVersionId, int changeAction)
        {
            const string query = @"
                Update [DocumentVersion] 
                SET 
                    [DocTypeVersionId] = @DocTypeVersionId ,
                    [ChangeAction] = @ChangeAction
                WHERE
                        [DocTypeVersionId] IS NULL
                    AND [DocTypeId] = @DocTypeId
            ";
            _context.Connection.Execute(query, new
            {
                DocTypeVersionID = docTypeVersionId,
                DocTypeId = docTypeId,
                ChangeAction = changeAction
            },
            _context.CurrentTransaction);
        }
    }
}