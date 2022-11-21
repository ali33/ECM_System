using System.Activities;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.Contract
{
    internal class ContractHelper
    {
        public static WorkflowRuntimeData GetRuntimeData(ActivityContext context)
        {
            WorkflowDataContext dataContext = context.DataContext;
            System.ComponentModel.PropertyDescriptorCollection propertyDescriptorCollection = dataContext.GetProperties();
            WorkflowRuntimeData runtimeInfo = propertyDescriptorCollection[WorkflowConstant.RuntimeData].GetValue(dataContext) as WorkflowRuntimeData;
            return runtimeInfo;
        }
    }
}
