using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class ReleaseBatchDao
    {
        private readonly DapperContext _context;

        public ReleaseBatchDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertReleaseBatch(ReleaseBatch batch)
        {
            const string query = @"INSERT INTO [ReleaseBatch] 
                                          ([BatchTypeID],[DocCount],[PageCount],[CreatedDate],[CreatedBy],[IsRejected])
                            OUTPUT inserted.ID  
                                   VALUES (@BatchTypeID, @DocCount, @PageCount, @CreatedDate, @CreatedBy,@IsRejected)";
            batch.Id = _context.Connection.Query<Guid>(query,
                                             new
                                             {
                                                 BatchTypeId = batch.BatchTypeId,
                                                 DocCount = batch.DocCount,
                                                 PageCount = batch.PageCount,
                                                 CreatedDate = batch.CreatedDate,
                                                 CreatedBy = batch.CreatedBy,
                                                 IsRejected = batch.IsRejected
                                             },
                                             _context.CurrentTransaction).Single();
        }

        public void UpdateReleaseBatch(ReleaseBatch batch)
        {
            const string query = @"UPDATE [ReleaseBatch]
                                   SET   [DocCount] = @DocCount,
                                         [PageCount] = @PageCount,
                                         [ModifiedDate] = @ModifiedDate,
                                         [ModifiedBy] = @ModifiedBy
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            DocCount = batch.DocCount,
                                            PageCount = batch.PageCount,
                                            ModifiedDate = batch.ModifiedDate,
                                            ModifiedBy = batch.ModifiedBy,
                                            Id = batch.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void DeleteReleaseBatch(ReleaseBatch batchId)
        {
            const string query = @"DELETE FROM [ReleaseBatch] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = batchId }, _context.CurrentTransaction);
        }

        public ReleaseBatch GetReleaseBatch(Guid batchId)
        {
            const string query = @"SELECT * 
                                   FROM [ReleaseBatch] 
                                   WHERE Id = @Id";
            return _context.Connection.Query<ReleaseBatch>(query, new { Id = batchId }, _context.CurrentTransaction).FirstOrDefault();

        }

        public List<ReleaseBatch> GetReleaseBatchs()
        {
            const string query = @"SELECT * 
                                   FROM [ReleaseBatch]";
            return _context.Connection.Query<ReleaseBatch>(query, null,_context.CurrentTransaction).ToList();
        }

        public List<ReleaseBatch> GetReleaseBatchByBatchType(long batchTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [ReleaseBatch] 
                                   WHERE [BatchTypeId] = @BatchTypeId";
            return _context.Connection.Query<ReleaseBatch>(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }
    }
}
