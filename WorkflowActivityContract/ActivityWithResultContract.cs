using System.Activities;
using System.ComponentModel.Composition;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.Contract
{
    [InheritedExport(typeof(ActivityWithResult))]
    public abstract class ActivityWithResultContract<T> : CodeActivity<T>
    {
        protected WorkflowRuntimeData GetWorkflowRuntimeData(CodeActivityContext context)
        {
            if (_runtimeData == null)
            {
                _runtimeData = ContractHelper.GetRuntimeData(context);
            }

            return _runtimeData;
        }

        private WorkflowRuntimeData _runtimeData;
    }
}
