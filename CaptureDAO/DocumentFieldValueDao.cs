using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class DocumentFieldValueDao
    {
        private readonly DapperContext _context;

        public DocumentFieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentFieldValue field)
        {
            const string query = @"INSERT INTO [DocumentFieldValue] ([DocId],[FieldId],[Value])
                                    OUTPUT inserted.ID     
                                   VALUES (@DocId,@FieldId,@Value)";
            field.Id = _context.Connection.Query<Guid>(query,
                                        new
                                            {                                                
                                                DocId = field.DocId,
                                                FieldId = field.FieldId,
                                                Value = field.Value
                                            },
                                        _context.CurrentTransaction).Single();
        }

        public void Update(DocumentFieldValue obj)
        {
            const string query = @"UPDATE [DocumentFieldValue] 
                                   SET [Value] = @Value
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            obj.Value,
                                            obj.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public List<DocumentFieldValue> GetByDoc(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentFieldValue] 
                                   WHERE [DocId] = @DocId";
            return _context.Connection.Query<DocumentFieldValue>(query, new { DocId = docId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByDoc(Guid docId)
        {
            const string query = @"DELETE FROM [DocumentFieldValue] 
                                   WHERE [DocId] = @DocId";
            _context.Connection.Execute(query, new { DocId = docId }, _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [DocumentFieldValue] 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [DocumentFieldValue]
                                   WHERE [FieldId] IN (SELECT Id 
                                                       FROM DocumentFieldMetaData WHERE [DocTypeId] = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        /// <summary>
        /// Get list document field value from list id.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<DocumentFieldValue> GetByIds(List<Guid> ids)
        {
            const string query = @"SELECT *
                                   FROM [DocumentFieldValue] 
                                   WHERE [Id] IN @Ids";
            return _context.Connection.Query<DocumentFieldValue>(query, new { Ids = ids }, _context.CurrentTransaction).ToList();
        }
    }
}