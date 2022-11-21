using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class ReleaseDocumentFieldValueDao
    {
        private readonly DapperContext _context;

        public ReleaseDocumentFieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertFieldValue(ReleaseDocumentFieldValue obj)
        {
            const string query = @"INSERT INTO [ReleaseDocumentFieldValue] ([ReleaseDocId],[FieldId],[Value])
                                    OUTPUT inserted.ID     
                                   VALUES (@ReleaseDocId,@FieldId,@Value)";
            obj.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {

                                            ReleaseDocId = obj.ReleaseDocId,
                                            FieldId = obj.FieldId,
                                            Value = obj.Value
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [ReleaseDocumentFieldValue] 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [ReleaseDocumentFieldValue]
                                   WHERE [FieldId] IN (SELECT Id 
                                                       FROM DocumentFieldMetaData WHERE [DocTypeId] = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public ReleaseDocumentFieldValue GetById(Guid id)
        {
            const string query = @"SELECT * FROM ReleaseDocumentFieldValue WHERE [ID] = @ID";
            return _context.Connection.Query<ReleaseDocumentFieldValue>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<ReleaseDocumentFieldValue> GetByReleaseDocument(Guid id)
        {
            const string query = @"SELECT * FROM ReleaseDocumentFieldValue WHERE [ReleaseDocID] = @ID";
            return _context.Connection.Query<ReleaseDocumentFieldValue>(query, new { ReleaseDocID = id }, _context.CurrentTransaction).ToList();
        }
    }
}
