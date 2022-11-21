using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class PicklistDao
    {
        private readonly DapperContext _context;

        public PicklistDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Picklist obj)
        {
            const string query = @"INSERT INTO [Picklist] ([FieldID],[Value])
                                OUTPUT inserted.ID
                                   VALUES (@FieldID,@Value)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             FieldID = obj.FieldId,
                                                             Value = obj.Value
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public List<Picklist> GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM [Picklist] 
                                   WHERE FieldID = @FieldID";
            return _context.Connection.Query<Picklist>(query, new { FieldID = fieldId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [Picklist] 
                                   WHERE FieldID = @FieldID";
            _context.Connection.Execute(query, new { FieldID = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Picklist] 
                                   WHERE FieldID IN (SELECT ID
                                                     FROM FieldMetaData
                                                     WHERE DocTypeID = @DocTypeID)";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }
    }
}