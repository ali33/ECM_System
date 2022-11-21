using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Drawing;

using Ecm.Workflow.Activities.Contract;
using Ecm.Workflow.Activities.HumanStepPermissionDesigner;

namespace Ecm.Workflow.Activities.HumanStep
{
    [Designer(typeof(HumanStepDesigner))]
    [ToolboxBitmap(typeof(HumanStep), "humanStep.png")]
    public sealed class HumanStep : StoppableActivityContract
    {
        [Editor(typeof(PermissionDesigner), typeof(DialogPropertyValueEditor))]
        public Guid Permissions { get; set; }
        
        protected override bool BookmarkWorkflowAfterExecution
        {
            get
            {
                return true;
            }
        }

        protected override void ExecutionBody(NativeActivityContext context)
        {
        }

    }
}
