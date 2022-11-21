using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class WorkflowHumanStepDocTypePermissionDao
    {
        private readonly DapperContext _context;

        public WorkflowHumanStepDocTypePermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(WorkflowHumanStepDocumentTypePermission obj)
        {
            const string query = @"INSERT INTO [WorkflowHumanStepDocumentTypePermission]
                                               ([HumanStepID], [WorkflowDefinitionID], [DocTypeID], [UserGroupID], [CanAccess], [CanSeeRestrictedField])
                                   VALUES (@HumanStepId, @WorkflowDefinitionId, @DocTypeId, @UserGroupId, @CanAccess, @CanSeeRestrictedField)";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            HumanStepId = obj.HumanStepId,
                                            WorkflowDefinitionId = obj.WorkflowDefinitionId,
                                            DocTypeId = obj.DocTypeId,
                                            UserGroupId = obj.UserGroupId,
                                            CanAccess = obj.CanAccess,
                                            CanSeeRestrictedField = obj.CanSeeRestrictedField
                                        },
                                        _context.CurrentTransaction);
        }

        public List<WorkflowHumanStepDocumentTypePermission> GetByHumanStepId(Guid definitionId, Guid humanStepId)
        {
            const string query = @"SELECT * 
                                   FROM [WorkflowHumanStepDocumentTypePermission]
                                   WHERE [WorkflowDefinitionID] = @DefinitionId AND [HumanStepID] = @HumanStepId";
            return _context.Connection.Query<WorkflowHumanStepDocumentTypePermission>(query,
                                                                                      new
                                                                                      {
                                                                                          DefinitionId = definitionId,
                                                                                          HumanStepId = humanStepId
                                                                                      },
                                                                                      _context.CurrentTransaction).ToList();
        }

        public List<WorkflowHumanStepDocumentTypePermission> GetByWorkflowDefinitionId(Guid definitionId)
        {
            const string query = @"SELECT * 
                                   FROM [WorkflowHumanStepDocumentTypePermission]
                                   WHERE [WorkflowDefinitionID] = @DefinitionId";
            return _context.Connection.Query<WorkflowHumanStepDocumentTypePermission>(query, new { DefinitionId = definitionId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByWorkflowDefinitionId(Guid definitionId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepDocumentTypePermission] 
                                   WHERE [WorkflowDefinitionID] = @DefinitionId";
            _context.Connection.Execute(query, new { DefinitionId = definitionId }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid groupId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepDocumentTypePermission] 
                                   WHERE [UserGroupID] = @UserGroupId";
            _context.Connection.Execute(query, new { UserGroupId = groupId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepDocumentTypePermission] 
                                   WHERE [DocTypeID] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepDocumentTypePermission] 
                                   WHERE [WorkflowDefinitionId] IN (SELECT [Id] FROM [WorkflowDefinition] WHERE [BatchTypeId] = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }
    }
}
