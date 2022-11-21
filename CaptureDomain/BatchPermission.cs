using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class BatchPermission : BaseHumanStepPermission
    {
        // Summary:
        //     Identifier of the human step
        [DataMember]
        public Guid HumanStepID { get; set; }
        //
        // Summary:
        //     Identifier of the workflow definition
        [DataMember]
        public Guid WorkflowDefinitionID { get; set; }

        //[DataMember]
        //public List<
        // Summary:
        //     This method is used internally
        //
        // Returns:
        //     The Ecm.Domain.WorkItemPermission object has all permissions are allowed
        public static BatchPermission GetAllowAll()
        {
            return new BatchPermission
            {
                CanModifyDocument = true,
                CanModifyIndexes = true,
                CanDelete = true,
                CanAnnotate = true,
                CanPrint = true,
                CanEmail = true,
                CanSendLink = true,
                CanDownloadFilesOnDemand = true,
                CanReleaseLoosePage = true,
                CanReject = true,
                CanViewOtherItems = true,
                CanDelegateItems = true
            };
        }

    }
}
