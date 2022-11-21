using System.Collections.Generic;
using Ecm.CaptureDomain;
using System;

namespace Ecm.WorkflowDesigner.Model
{
    //public delegate void SaveWorkflowEventHandler(Guid batchTypeId, string workflowDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings);
    public delegate void SaveWorkflowEventHandler(Guid batchTypeId, string workflowDefinition, List<CustomActivitySetting> customActivitySettings);
}
