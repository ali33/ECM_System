using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System.Collections.Generic;
using System;

namespace Ecm.CaptureDAO
{
    public class ReleaseBatchFieldValueDao
    {
        private readonly DapperContext _context;

        public ReleaseBatchFieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertFieldValue(ReleaseBatchFieldValue fieldValue)
        {
            const string query = @"INSERT INTO [ReleaseBatchFieldValue] ([ReleaseBatchId],[FieldId],[Value])
                        OUTPUT inserted.ID  
                                   VALUES (@ReleaseBatchId,@FieldId,@Value)";
            fieldValue.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {
                                            Id = fieldValue.Id,
                                            ReleaseBatchId = fieldValue.ReleaseBatchId,
                                            FieldId = fieldValue.FieldId,
                                            Value = fieldValue.Value
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [ReleaseBatchFieldValue] 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [ReleaseBatchFieldValue]
                                   WHERE [FieldId] IN (SELECT Id 
                                                       FROM BatchFieldMetaData WHERE [BatchTypeId] = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public ReleaseBatchFieldValue GetById(Guid id)
        {
            const string query = "SELECT * FROM ReleaseBatchFieldValue WHERE [ID] = @ID";
            return _context.Connection.Query<ReleaseBatchFieldValue>(query, null, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<ReleaseBatchFieldValue> GetByField(Guid fieldId)
        {
            const string query = "SELECT * FROM ReleaseBatchFieldValue WHERE [FieldID] = @FieldID";
            return _context.Connection.Query<ReleaseBatchFieldValue>(query, new { FieldId = fieldId }, _context.CurrentTransaction).ToList();
        }

        public List<ReleaseBatchFieldValue> GetByBatch(Guid batchId)
        {
            const string query = "SELECT * FROM ReleaseBatchFieldValue WHERE [ReleaseBatchID] = @ReleaseBatchID";
            return _context.Connection.Query<ReleaseBatchFieldValue>(query, new { ReleaseBatchID = batchId }, _context.CurrentTransaction).ToList();
        }
    }
}
