using System;
using System.Collections.Generic;
using Ecm.CaptureDomain;

namespace Ecm.CaptureModel.DataProvider
{
    public class WorkflowProvider : ProviderBase
    {
        public List<UserGroup> GetUserGroups()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetUserGroups();
            }
        }

        public WorkflowDefinition GetWorkflowDefinition(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetWorkflowByBatchTypeId(batchTypeId);
            }
        }

        //public List<HumanStepPermission> GetWorkflowHumanStepPermissions(Guid workflowDefinitionId)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return client.Channel.GetWorkflowHumanStepPermissions(workflowDefinitionId);
        //    }
        //}

        //public Guid SaveWorkflow(Guid batchTypeId, WorkflowDefinition workflowDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return client.Channel.SaveWorkflowDefinition(batchTypeId, workflowDefinition, humanStepPermissions, customActivitySettings);
        //    }
        //}

        public Guid SaveWorkflow(Guid batchTypeId, WorkflowDefinition workflowDefinition, List<CustomActivitySetting> customActivitySettings)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.SaveWorkflowDefinition(batchTypeId, workflowDefinition, customActivitySettings);
            }
        }

        public List<CustomActivitySetting> GetCustomActivitySettings(Guid workflowDefinitionId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetCustomActivitySettings(workflowDefinitionId);
            }
        }
    }
}
