using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class CustomActivitySettingDao
    {
        private readonly DapperContext _context;

        public CustomActivitySettingDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(CustomActivitySetting customSetting)
        {
            const string query = @"INSERT INTO [CustomActivitySetting]([WorkflowDefinitionId],[ActivityId],[Value])
                                OUTPUT inserted.ID  
                                   VALUES(@WorkflowDefinitionId, @ActivityId, @Value)
                                ";
            customSetting.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         WorkflowDefinitionId = customSetting.WorkflowDefinitionId,
                                                         ActivityId = customSetting.ActivityId,
                                                         Value = customSetting.Value
                                                     },
                                                     _context.CurrentTransaction).Single();

        }

        public void Update(CustomActivitySetting customSetting)
        {
            const string query = @"UPDATE [CustomActivitySetting] SET
                                 [Value] = @Value
                                 WHERE [WorkflowDefinitionId] = @WorkflowDefinitionId AND [ActivityId] = @ActivityId";

            _context.Connection.Execute(query,
                                        new
                                        {
                                            WorkflowDefinitionId = customSetting.WorkflowDefinitionId,
                                            ActivityId = customSetting.ActivityId,
                                            Value = customSetting.Value
                                        },
                                        _context.CurrentTransaction);

        }

        public void DeleteByWorkflow(Guid workflowDefinitionId)
        {
            const string query = @"DELETE FROM [CustomActivitySetting]
                                 WHERE [WorkflowDefinitionId] = @WorkflowDefinitionId";

            _context.Connection.Execute(query,
                                        new
                                        {
                                            workflowDefinitionId = workflowDefinitionId
                                        },
                                        _context.CurrentTransaction);
        }

        public CustomActivitySetting GetCustomActivitySetting(Guid WorkflowDefinitionId, Guid customActivityId)
        {
            const string query = @"SELECT * 
                                   FROM [CustomActivitySetting] 
                                   WHERE [WorkflowDefinitionId] = @WorkflowDefinitionId AND [ActivityId] = @ActivityId";
            return _context.Connection.Query<CustomActivitySetting>(query, 
                                new 
                                { 
                                    WorkflowDefinitionId = WorkflowDefinitionId, 
                                    ActivityId = customActivityId 
                                }, _context.CurrentTransaction).FirstOrDefault();

        }

        public List<CustomActivitySetting> GetCustomActivitySettingByWorkflow(Guid WorkflowDefinitionId)
        {
            const string query = @"SELECT * 
                                   FROM [CustomActivitySetting] 
                                   WHERE [WorkflowDefinitionId] = @WorkflowDefinitionId";
            return _context.Connection.Query<CustomActivitySetting>(query, 
                                new 
                                { 
                                    WorkflowDefinitionId = WorkflowDefinitionId
                                }, _context.CurrentTransaction).ToList();

        }
    }
}
