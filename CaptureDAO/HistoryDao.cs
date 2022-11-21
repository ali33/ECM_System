using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class HistoryDao
    {
        private readonly DapperContext _context;

        public HistoryDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertHistory(History history)
        {
            const string query = @"INSERT INTO [History]([Action],[ActionDate],[BatchID],[WorkflowStep],[CustomMsg])
                            OUTPUT inserted.ID 
                                   VALUES(@Action,@ActionDate,@BatchId,@WorkflowStep,@CustomMsg)";
            history.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {
                                            Action = history.Action,
                                            ActionDate = history.ActionDate,
                                            BatchId = history.BatchId,
                                            WorkflowStep = history.WorkflowStep,
                                            CustomMsg = history.CustomMsg
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public List<History> GetByBatch(Guid batchId)
        {
            const string query = @"SELECT * 
                                   FROM [History] 
                                   WHERE [BatchID] = @BatchId";
            return _context.Connection.Query<History>(query, new { BatchId = batchId }, _context.CurrentTransaction).ToList();
        }
    }
}
