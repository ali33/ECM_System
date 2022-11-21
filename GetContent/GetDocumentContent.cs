using System;
using System.Activities;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.Contract;
using Ecm.CaptureCore;
using System.Drawing;
using System.ComponentModel;
using Ecm.Workflow.WorkflowExtension;

namespace Ecm.Workflow.Activities.GetContent
{
    [Designer(typeof(GetDocumentContentDesigner))]
    [ToolboxBitmap(typeof(GetDocumentContent), "getContent.png")]
    public sealed class GetDocumentContent : ActivityWithResultContract<Batch>
    {
        private readonly SecurityManager _securityManager = new SecurityManager();

        protected override Batch Execute(CodeActivityContext context)
        {
            var wfSystem = _securityManager.Authorize("WorkflowSystem", "TzmdoMVgNmQ5QMXJDuLBKgKg6CYfx73S/8dPX8Ytva+Eu3hlFNVoAg==");
            wfSystem.ClientHost = string.Empty;

            WorkflowRuntimeData runtimeData = GetWorkflowRuntimeData(context);

            ActionLogManager actionLog = new ActionLogManager(wfSystem);

            actionLog.AddLog("Begin get content process on batch Id: " + Guid.Parse(runtimeData.ObjectID.ToString()),
                            wfSystem, ActionName.GetBatchData, null, null);

            if (runtimeData.ObjectType == WorkflowObjectType.Document)
            {
                Batch result = new BatchManager(wfSystem).GetBatch(Guid.Parse(runtimeData.ObjectID.ToString()));

                actionLog.AddLog("End get content process on batch Id: " + Guid.Parse(runtimeData.ObjectID.ToString()),
                                wfSystem, ActionName.GetBatchData, null, null);
                return result;
            }

            return null;
        }
    }
}
