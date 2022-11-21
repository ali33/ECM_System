using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Ecm.CaptureDAO
{
    public class TableFieldValueDao
    {
        private readonly DapperContext _context;

        public TableFieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(TableFieldValue obj)
        {
            const string query = @"INSERT INTO [TableFieldValue] ([DocId],[FieldId],[RowNumber],[Value])
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
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByParentField(Guid parentFieldId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [FieldId] IN (SELECT Id FROM DocumentFieldMetadata WHERE ParentFieldId = @ParentFieldId)";
            _context.Connection.Execute(query, new { ParentFieldId = parentFieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocument(Guid DocId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [DocId] = @DocId";
            _context.Connection.Execute(query, new { DocId = DocId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [FieldId] IN (SELECT Id FROM DocumentFieldMetadata WHERE DocTypeId = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByDocAndField(Guid docId, Guid fieldId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [DocId] = @DocId AND [FieldId] = @fieldId";
            _context.Connection.Execute(query, new { DocId = docId, FieldId = fieldId }, _context.CurrentTransaction);
        }

        public List<TableFieldValue> GetByParentField(Guid parentFieldId, Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE FieldId IN (SELECT ID FROM DocumentFieldMetadata WHERE ParentFieldId = @ParentFieldId) AND DocId = @DocId";
            return _context.Connection.Query<TableFieldValue>(query, new { ParentFieldId = parentFieldId, DocId = docId }, _context.CurrentTransaction).ToList();
        }

        public List<TableFieldValue> GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE FieldId = @FieldId";
            return _context.Connection.Query<TableFieldValue>(query, new { FieldId = fieldId }, _context.CurrentTransaction).ToList();
        }

        public List<TableFieldValue> GetData(Guid parentFieldId, Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE FieldId IN (SELECT ID FROM DocumentFieldMetadata WHERE ParentFieldId = @ParentFieldId) AND DocId = @DocId";
            return _context.Connection.Query<TableFieldValue>(query, new { ParentFieldId = parentFieldId, DocId = docId }, _context.CurrentTransaction).ToList();
        }

        public TableFieldValue GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE Id = @Id";
            return _context.Connection.Query<TableFieldValue>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(TableFieldValue obj)
        {
            const string query = @"UPDATE [TableFieldValue] SET 
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

        /// <summary>
        /// Get table value from ids
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public List<TableFieldValue> GetByIds(List<Guid> ids)
        {
            const string query = @"SELECT * 
                                   FROM [TableFieldValue] 
                                   WHERE [Id] IN @Ids";
            return _context.Connection.Query<TableFieldValue>(query, new { Ids = ids }, _context.CurrentTransaction).ToList();
        }

        public void DeleteIfNotInListId(Guid docId, Guid fieldId, List<Guid> keepIds)
        {
            const string query = @"DELETE FROM [TableFieldValue] 
                                   WHERE [DocId] = @DocId AND [FieldId] = @FieldId AND [Id] NOT IN @KeepIds";
            _context.Connection.Execute(query, new
            {
                DocId = docId,
                FieldId = fieldId,
                KeepIds = keepIds
            }, _context.CurrentTransaction);
        }


    }
}