using Ecm.CaptureDAO.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class ReleaseTableFieldValueDao
    {
        private readonly DapperContext _context;

        public ReleaseTableFieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(ReleaseTableFieldValue obj)
        {
            const string query = @"INSERT INTO [ReleaseTableFieldValue] ([DocId],[FieldId],[RowNumber],[Value])
                                OUTPUT inserted.ID
                                   VALUES (@DocId, @FieldId, @RowNumber,@Value)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         DocId = obj.DocId,
                                                         FieldId = obj.FieldId,
                                                         RowNumber = obj.RowNumber,
                                                         Value = obj.Value
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM ReleaseTableFieldValue 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM ReleaseTableFieldValue 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByParentField(Guid parentFieldId)
        {
            const string query = @"DELETE FROM ReleaseTableFieldValue 
                                   WHERE [FieldId] IN (SELECT Id FROM DocumentFieldMetadata WHERE ParentFieldId = @ParentFieldId)";
            _context.Connection.Execute(query, new { ParentFieldId = parentFieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocument(Guid DocId)
        {
            const string query = @"DELETE FROM ReleaseTableFieldValue 
                                   WHERE [DocId] = @DocId";
            _context.Connection.Execute(query, new { DocId = DocId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM ReleaseTableFieldValue 
                                   WHERE [FieldId] IN (SELECT Id FROM DocumentFieldMetadata WHERE DocTypeId = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public List<ReleaseTableFieldValue> GetByParentField(Guid parentFieldId)
        {
            const string query = @"SELECT * 
                                   FROM ReleaseTableFieldValue 
                                   WHERE FieldId IN (SELECT ID FROM DocumentFieldMetadata WHERE ParentFieldId = @ParentFieldId)";
            return _context.Connection.Query<ReleaseTableFieldValue>(query, new { ParentFieldId = parentFieldId }, _context.CurrentTransaction).ToList();
        }

        public List<ReleaseTableFieldValue> GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM ReleaseTableFieldValue 
                                   WHERE FieldId = @FieldId";
            return _context.Connection.Query<ReleaseTableFieldValue>(query, new { FieldId = fieldId }, _context.CurrentTransaction).ToList();
        }

        public ReleaseTableFieldValue GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM ReleaseTableFieldValue 
                                   WHERE Id = @Id";
            return _context.Connection.Query<ReleaseTableFieldValue>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(ReleaseTableFieldValue obj)
        {
            const string query = @"UPDATE [ReleaseTableFieldValue] SET 
                                   [DocId] = @DocId, [FieldId] = @FieldId,[RowNumber] = @RowNumber,[Value] = @Value
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                                new
                                                {
                                                    DocId = obj.DocId,
                                                    FieldId = obj.FieldId,
                                                    RowNumber = obj.RowNumber,
                                                    Value = obj.Value,
                                                    Id = obj.Id
                                                },
                                                _context.CurrentTransaction);
        }
    }
}
