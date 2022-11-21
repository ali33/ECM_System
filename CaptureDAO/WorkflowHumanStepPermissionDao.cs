using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class WorkflowHumanStepPermissionDao
    {
        private readonly DapperContext _context;

        public WorkflowHumanStepPermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(WorkflowHumanStepPermission obj)
        {
            const string query = @"INSERT INTO [WorkflowHumanStepPermission]
                                               ([HumanStepId], [UserGroupId], [WorkflowDefinitionId],
                                                [CanModifyDocument], [CanModifyIndexes], [CanDelete], [CanAnnotate], [CanPrint], [CanEmail],
                                                [CanSendLink], [CanDownloadFilesOnDemand], [CanReleaseLoosePage], [CanReject], [CanViewOtherItems])
                                   VALUES (@HumanStepId, @UserGroupId, @WorkflowDefinitionId,
                                           @CanModifyDocument, @CanModifyIndexes, @CanDelete, @CanAnnotate, @CanPrint, @CanEmail,
                                           @CanSendLink, @CanDownloadFilesOnDemand, @CanReleaseLoosePage, @CanReject, @CanViewOtherItems)";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            HumanStepId = obj.HumanStepId,
                                            UserGroupId = obj.UserGroupId,
                                            WorkflowDefinitionId = obj.WorkflowDefinitionId,
                                            CanModifyDocument = obj.CanModifyDocument,
                                            CanModifyIndexes = obj.CanModifyIndexes,
                                            CanDelete = obj.CanDelete,
                                            CanAnnotate = obj.CanAnnotate,
                                            CanPrint = obj.CanPrint,
                                            CanEmail = obj.CanEmail,
                                            CanSendLink = obj.CanSendLink,
                                            CanDownloadFilesOnDemand = obj.CanDownloadFilesOnDemand,
                                            CanReleaseLoosePage = obj.CanReleaseLoosePage,
                                            CanReject = obj.CanReject,
                                            CanViewOtherItems = obj.CanViewOtherItems
                                        },
                                        _context.CurrentTransaction);
        }

        public List<WorkflowHumanStepPermission> GetByHumanStepId(Guid definitionId, Guid humanStepId)
        {
            const string query = @"SELECT * 
                                   FROM [WorkflowHumanStepPermission]
                                   WHERE [WorkflowDefinitionId] = @DefinitionId AND [HumanStepId] = @HumanStepId";
            return _context.Connection.Query<WorkflowHumanStepPermission>(query,
                                                                          new
                                                                              {
                                                                                  DefinitionId = definitionId,
                                                                                  HumanStepId = humanStepId
                                                                              },
                                                                          _context.CurrentTransaction).ToList();
        }

        public List<WorkflowHumanStepPermission> GetByWorkflowDefinitionId(Guid definitionId)
        {
            const string query = @"SELECT * 
                                   FROM [WorkflowHumanStepPermission]
                                   WHERE [WorkflowDefinitionId] = @DefinitionId";
            return _context.Connection.Query<WorkflowHumanStepPermission>(query, new { DefinitionId = definitionId }, _context.CurrentTransaction).ToList();
        }

        public List<WorkflowHumanStepPermission> GetWorkflowHumanStepPermission(List<Guid> groupIds, Guid batchTypeId)
        {
            const string query = @"SELECT DISTINCT p.* 
                                   FROM [WorkflowHumanStepPermission] p
                                   LEFT JOIN [Batch] d ON d.WorkflowDefinitionId = p.WorkflowDefinitionId
                                   WHERE p.UserGroupId in @UserGroupIds AND 
                                         d.IsCompleted = 0 AND
                                         d.IsProcessing = 0 AND
                                         d.BatchTypeId = @BatchTypeId";

            return _context.Connection.Query<WorkflowHumanStepPermission>(query,
                                                                          new
                                                                          {
                                                                              UserGroupIds = groupIds,
                                                                              BatchTypeId = batchTypeId
                                                                          },
                                                                          _context.CurrentTransaction).ToList();
        }

        public void DeleteByWorkflowDefinitionId(Guid definitionId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepPermission] 
                                   WHERE [WorkflowDefinitionId] = @DefinitionId";
            _context.Connection.Execute(query, new { DefinitionId = definitionId }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid groupId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepPermission] 
                                   WHERE [UserGroupId] = @UserGroupId";
            _context.Connection.Execute(query, new { UserGroupId = groupId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [WorkflowHumanStepPermission] 
                                   WHERE [WorkflowDefinitionId] IN (SELECT [Id] FROM [WorkflowDefinition] WHERE [BatchTypeId] = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }
    }
}
