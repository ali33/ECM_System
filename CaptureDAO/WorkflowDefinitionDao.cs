using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class WorkflowDefinitionDao
    {
        private readonly DapperContext _context;

        public WorkflowDefinitionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(WorkflowDefinition obj)
        {
            const string query = @"INSERT INTO [WorkflowDefinition] ([ID], [DefinitionXML], [BatchTypeID])
                                   VALUES (@Id, @DefinitionXML, @BatchTypeId)";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Id = obj.Id,
                                            DefinitionXML= obj.DefinitionXML,
                                            BatchTypeId = obj.BatchTypeId
                                        },
                                        _context.CurrentTransaction);
        }

        public WorkflowDefinition GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [WorkflowDefinition] 
                                   WHERE [ID] = @Id";
            return _context.Connection.Query<WorkflowDefinition>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public WorkflowDefinition GetByBatchTypeId(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [WorkflowDefinition] 
                                   WHERE [ID] = (SELECT [WorkflowDefinitionID] FROM [BatchType] WHERE ID = @Id)";
            return _context.Connection.Query<WorkflowDefinition>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<Guid> GetUnusedWorkflowIds(Guid batchTypeId)
        {
            const string query = @"SELECT [ID] 
                                   FROM [WorkflowDefinition] 
                                   WHERE [BatchTypeID] = @BatchTypeId AND ([ID] NOT IN (SELECT [WorkflowDefinitionID] FROM [Batch] WHERE [BatchTypeID] = @BatchTypeId))";
            return _context.Connection.Query<Guid>(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteById(Guid id)
        {
            const string query = @"DELETE FROM [WorkflowDefinition] 
                                   WHERE [ID] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [WorkflowDefinition] 
                                   WHERE [BatchTypeID] = @BatchTypeId";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }
    }
}
