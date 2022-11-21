using Ecm.DAO.Context;
using Ecm.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Ecm.DAO
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
                                   VALUES (@DocID,@FieldID, @RowNumber,@Value)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         DocId = obj.DocId,
                                                         FieldId = obj.Field.Id,
                                                         RowNumber = obj.RowNumber,
                                                         Value = obj.Value
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [Id] = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [FieldID] = @FieldID";
            _context.Connection.Execute(query, new { FieldID = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByParentField(Guid parentFieldId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [FieldID] IN (SELECT ID FROM FieldMetadata WHERE ParentFieldID = @ParentFieldID)";
            _context.Connection.Execute(query, new { ParentFieldID = parentFieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocument(Guid docId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [DocID] = @DocID";
            _context.Connection.Execute(query, new { DocID = docId }, _context.CurrentTransaction);
        }

        public void DeleteByDocumentAndField(Guid docId, Guid fieldId)
        {
            const string query = @"DELETE FROM TableFieldValue 
                                   WHERE [DocID] = @DocID AND [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { DocID = docId, FieldId = fieldId }, _context.CurrentTransaction);
        }

        public List<TableFieldValue> GetByParentField(Guid parentFieldId, Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE FieldId IN (SELECT ID FROM FieldMetadata WHERE ParentFieldID = @ParentFieldID) AND DocID = @DocID";
            return _context.Connection.Query<TableFieldValue>(query, new { ParentFieldID = parentFieldId, DocID = docId }, _context.CurrentTransaction).ToList();
        }

        public List<TableFieldValue> GetByDocument(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE DocID = @DocID";
            return _context.Connection.Query<TableFieldValue>(query, new { DocID = docId }, _context.CurrentTransaction).ToList();
        }

        public List<TableFieldValue> GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE FieldID = @FieldID";
            return _context.Connection.Query<TableFieldValue>(query, new { FieldID = fieldId }, _context.CurrentTransaction).ToList();
        }

        public TableFieldValue GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM TableFieldValue 
                                   WHERE ID = @ID";
            return _context.Connection.Query<TableFieldValue>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(Domain.TableFieldValue obj)
        {
            const string query = @"Update [TableFieldValue]
                                 SET
                                    [DocId]= @DocID,
                                    [FieldId]=@FieldID,
                                    [RowNumber]=@RowNumber,
                                    [Value]=@Value
                                 Where [ID]=@ID
                                   ";
            _context.Connection.Execute(query,
                                                     new
                                                     {
                                                         DocId = obj.DocId,
                                                         FieldId = obj.Field.Id,
                                                         RowNumber = obj.RowNumber,
                                                         Value = obj.Value,
                                                         Id= obj.Id
                                                     },
                                                     _context.CurrentTransaction);
        }
    }
}