using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;

namespace Ecm.CaptureDAO
{
    public class AnnotationPermissionDao
    {
        private readonly DapperContext _context;

        public AnnotationPermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(HumanStepAnnotationPermission obj)
        {
            var query = @"INSERT INTO [HumanStepAnnotationPermission]([HumanStepId],[WorkflowDefinitionId],[DocTypeId],[UserGroupId],
                                                                      [CanSeeHighlight],[CanAddHighlight],[CanDeleteHighlight],
                                                                      [CanSeeText],[CanAddText],[CanDeleteText],
                                                                      [CanHideRedaction],[CanAddRedaction],[CanDeleteRedaction])
                        VALUES(@HumanStepId,@WorkflowDefinitionId,@DocTypeId,@UserGroupId,
                               @CanSeeHighlight,@CanAddHighlight,@CanDeleteHighlight,
                               @CanSeeText,@CanAddText,@CanDeleteText,
                               @CanHideRedaction,@CanAddRedaction,@CanDeleteRedaction)";

            _context.Connection.Execute(query, new
                                {
                                    obj.HumanStepId,
                                    obj.WorkflowDefinitionId,
                                    obj.DocTypeId,
                                    obj.UserGroupId,
                                    obj.CanSeeHighlight,
                                    obj.CanAddHighlight,
                                    obj.CanDeleteHighlight,
                                    obj.CanSeeText,
                                    obj.CanAddText,
                                    obj.CanDeleteText,
                                    obj.CanHideRedaction,
                                    obj.CanAddRedaction,
                                    obj.CanDeleteRedaction
                                }, _context.CurrentTransaction);
                                                    
        }

        public List<HumanStepAnnotationPermission> GetByHumanStepId(Guid workflowId, Guid activityId)
        {
            const string query = @"SELECT * 
                                   FROM [HumanStepAnnotationPermission]
                                   WHERE [WorkflowDefinitionId] = @DefinitionId AND [HumanStepId] = @HumanStepId";
            return _context.Connection.Query<HumanStepAnnotationPermission>(query,
                                                                          new
                                                                          {
                                                                              DefinitionId = workflowId,
                                                                              HumanStepId = activityId
                                                                          },
                                                                          _context.CurrentTransaction).ToList();
        }

        public List<HumanStepAnnotationPermission> GetByWorkflowDefinitionId(Guid workflowDefinitionId)
        {
            const string query = @"SELECT * 
                                   FROM [HumanStepAnnotationPermission]
                                   WHERE [WorkflowDefinitionId] = @DefinitionId";
            return _context.Connection.Query<HumanStepAnnotationPermission>(query,
                                                                          new
                                                                          {
                                                                              DefinitionId = workflowDefinitionId
                                                                          },
                                                                          _context.CurrentTransaction).ToList();
        }

        public List<HumanStepAnnotationPermission> GetWorkflowHumanStepPermission(List<Guid> groupIds, Guid batchTypeId)
        {
            const string query = @"SELECT DISTINCT p.* 
                                   FROM [HumanStepAnnotationPermission] p
                                   LEFT JOIN [Batch] d ON d.WorkflowDefinitionId = p.WorkflowDefinitionId
                                   WHERE p.UserGroupId in @UserGroupIds AND 
                                         d.IsCompleted = 0 AND
                                         d.IsProcessing = 0 AND
                                         d.BatchTypeId = @BatchTypeId";

            return _context.Connection.Query<HumanStepAnnotationPermission>(query,
                                                                          new
                                                                          {
                                                                              UserGroupIds = groupIds,
                                                                              BatchTypeId = batchTypeId
                                                                          },
                                                                          _context.CurrentTransaction).ToList();
        }

        public void DeleteByWorkflowDefinitionId(Guid definitionId)
        {
            const string query = @"DELETE FROM [HumanStepAnnotationPermission] 
                                   WHERE [WorkflowDefinitionId] = @DefinitionId";
            _context.Connection.Execute(query, new { DefinitionId = definitionId }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid groupId)
        {
            const string query = @"DELETE FROM [HumanStepAnnotationPermission] 
                                   WHERE [UserGroupId] = @UserGroupId";
            _context.Connection.Execute(query, new { UserGroupId = groupId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [HumanStepAnnotationPermission] 
                                   WHERE [WorkflowDefinitionId] IN (SELECT [Id] FROM [WorkflowDefinition] WHERE [BatchTypeId] = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

    }
}
