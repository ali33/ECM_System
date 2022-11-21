using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class BatchFieldValueDao
    {
        private readonly DapperContext _context;

        public BatchFieldValueDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(BatchFieldValue field)
        {
            const string query = @"INSERT INTO [BatchFieldValue] ([BatchId],[FieldId],[Value])
                                   VALUES (@BatchId,@FieldId,@Value)";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                BatchId = field.BatchId,
                                                FieldId = field.FieldId,
                                                Value = field.Value
                                            },
                                        _context.CurrentTransaction);
        }

        public void Update(BatchFieldValue field)
        {
            const string query = @"UPDATE [BatchFieldValue] 
                                   SET [Value] = @Value
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Value = field.Value,
                                            Id = field.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public List<BatchFieldValue> GetByBatch(Guid batchId)
        {
            const string query = @"SELECT * 
                                   FROM [BatchFieldValue] 
                                   WHERE [BatchId] = @BatchId";
            return _context.Connection.Query<BatchFieldValue>(query, new { BatchId = batchId }, _context.CurrentTransaction).ToList();
        }

        public List<BatchFieldValue> GetValue(string expression, Guid batchTypeId)
        {
            string query = @"select * 
                             from BatchFieldValue 
                             WHERE FieldID in (select id from BatchFieldMetaData where BatchTypeId = @BatchTypeId and " + expression +") ORDER BY BatchId DESC";

            return _context.Connection.Query<BatchFieldValue>(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByBatch(Guid batchId)
        {
            const string query = @"DELETE FROM [BatchFieldValue] 
                                   WHERE [BatchId] = @BatchId";
            _context.Connection.Execute(query, new { BatchId = batchId }, _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [BatchFieldValue] 
                                   WHERE [FieldId] = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [BatchFieldValue]
                                   WHERE [FieldId] IN (SELECT Id 
                                                       FROM BatchFieldMetaData WHERE [BatchTypeId] = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        /// <summary>
        /// Get list distinct batch id.
        /// </summary>
        /// <param name="fieldValueIds"></param>
        /// <returns></returns>
        public List<Guid> GetBatchIds(List<Guid> fieldValueIds)
        {
            const string query = @"SELECT DISTINCT [BatchId] 
                                   FROM [BatchFieldValue] 
                                   WHERE [Id] IN @FieldValueIds";
            return _context.Connection.Query<Guid>(query, new { FieldValueIds = fieldValueIds }, _context.CurrentTransaction).ToList();
        }
    }
}