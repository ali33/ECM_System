using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class BatchTypeDao
    {
        private readonly DapperContext _context;

        public BatchTypeDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(BatchType batchType)
        {
            const string query = @"INSERT INTO [BatchType]([Name],[Description],[Icon],[WorkflowDefinitionId],[CreatedDate],[CreatedBy],[UniqueId],[IsApplyForOutlook])
                            OUTPUT inserted.ID
                                VALUES(@Name, @Description, @Icon, @WorkflowDefinitionId, @CreatedDate, @CreatedBy, @UniqueId, @IsApplyForOutlook)
                                   SELECT CAST (SCOPE_IDENTITY() as BIGINT)";
            batchType.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         Name = batchType.Name,
                                                         Description = batchType.Description,
                                                         Icon = batchType.Icon,
                                                         WorkflowDefinitionId = batchType.WorkflowDefinitionId,
                                                         CreatedDate = batchType.CreatedDate,
                                                         CreatedBy = batchType.CreatedBy,
                                                         UniqueId = batchType.UniqueId,
                                                         IsApplyForOutlook = batchType.IsApplyForOutlook
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [BatchType] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public List<BatchType> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [BatchType]";
            return _context.Connection.Query<BatchType>(query, null, _context.CurrentTransaction).ToList();
        }

        public BatchType GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [BatchType] 
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<BatchType>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<BatchType> GetCapturedBatchTypes()
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM [BatchType] d 
                                   WHERE d.IsWorkflowDefined = 1";
            return _context.Connection.Query<BatchType>(query, null, _context.CurrentTransaction).ToList();
        }

        public List<BatchType> GetCapturedBatchTypes(List<Guid> groupIds)
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM [BatchType] d 
                                   LEFT JOIN [BatchTypePermission] bp ON d.Id = bp.BatchTypeId 
                                   WHERE UserGroupID in @UserGroupIds AND bp.CanCapture = 1 AND d.IsWorkflowDefined = 1";
            return _context.Connection.Query<BatchType>(query, new { UserGroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }

        public List<BatchType> GetAssignedBatchTypes(List<Guid> groupIds)
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM [BatchType] d 
                                   LEFT JOIN [BatchTypePermission] bp ON d.Id = bp.BatchTypeId 
                                   WHERE UserGroupID in @UserGroupIds AND bp.CanAccess = 1 AND d.IsWorkflowDefined = 1";
            return _context.Connection.Query<BatchType>(query, new { UserGroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }

        public void Update(BatchType obj)
        {
            const string query = @"UPDATE [BatchType] 
                                   SET    [Name] = @Name,
                                          [Description] = @Description,
                                          [Icon] = @Icon,
                                          [ModifiedDate] = @ModifiedDate,
                                          [ModifiedBy] = @ModifiedBy,
                                          [IsApplyForOutlook] = @IsApplyForOutlook
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Name = obj.Name,
                                            Description = obj.Description,
                                            Icon = obj.Icon,
                                            ModifiedDate = obj.ModifiedDate,
                                            ModifiedBy = obj.ModifiedBy,
                                            IsApplyForOutlook = obj.IsApplyForOutlook,
                                            Id = obj.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateWorkflow(BatchType obj)
        {
            const string query = @"UPDATE [BatchType] 
                                   SET    [WorkflowDefinitionId] = @WorkflowDefinitionId,
                                          [IsWorkflowDefined] = @IsWorkflowDefined,
                                          [ModifiedDate] = @ModifiedDate,
                                          [ModifiedBy] = @ModifiedBy
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            WorkflowDefinitionId = obj.WorkflowDefinitionId,
                                            IsWorkflowDefined = obj.IsWorkflowDefined,
                                            ModifiedDate = obj.ModifiedDate,
                                            ModifiedBy = obj.ModifiedBy,
                                            Id = obj.Id
                                        },
                                        _context.CurrentTransaction);
        }
    }
}
