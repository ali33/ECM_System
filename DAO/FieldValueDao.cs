using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class FieldValueDao
    {
        private readonly DapperContext _context;

        public FieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(FieldValue obj)
        {
            const string query = @"INSERT INTO [FieldValue] ([DocID],[FieldID],[Value])
                                OUTPUT inserted.ID
                                   VALUES (@DocID,@FieldID,@Value)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocID = obj.DocId,
                                                             FieldID = obj.FieldId,
                                                             Value = obj.Value
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public List<FieldValue> GetByDoc(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM FieldValue 
                                   WHERE [DocID] = @DocID";
            return _context.Connection.Query<FieldValue>(query, new { DocID = docId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByDoc(Guid docId)
        {
            const string query = @"DELETE FROM FieldValue 
                                   WHERE [DocID] = @DocID";
            _context.Connection.Execute(query, new { DocID = docId }, _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM FieldValue 
                                   WHERE [FieldID] = @FieldID";
            _context.Connection.Execute(query, new { FieldID = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [FieldValue]
                                   WHERE FieldID IN (SELECT ID 
                                                     FROM FieldMetaData WHERE DocTypeID = @DocTypeID)";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }
    }
}