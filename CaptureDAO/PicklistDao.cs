using System.Collections.Generic;
using System.Linq;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Dapper;
using System;

namespace Ecm.CaptureDAO
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
            const string query = @"INSERT INTO [Picklist] ([FieldId],[Value])
                                OUTPUT inserted.ID     
                                   VALUES (@FieldId,@Value)
                                ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             FieldId = obj.FieldId,
                                                             Value = obj.Value
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public List<Picklist> GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM [Picklist] 
                                   WHERE [FieldId] = @FieldId";
            return _context.Connection.Query<Picklist>(query, new { FieldId = fieldId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [Picklist] 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Picklist] 
                                   WHERE [FieldId] IN (SELECT [Id]
                                                     FROM [DocumentFieldMetaData]
                                                     WHERE [DocTypeId] = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }
    }
}