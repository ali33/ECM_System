using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the permission of one user group on a human step of a workflow.
    /// </summary>
    [DataContract]
    public class WorkflowHumanStepPermission : HumanStepUserGroupPermission
    {
        /// <summary>
        /// Identifier of the human step in workflow
        /// </summary>
        [DataMember]
        public Guid HumanStepId { get; set; }

        /// <summary>
        /// Identifier of the workflow that contains the human step
        /// </summary>
        [DataMember]
        public Guid WorkflowDefinitionId { get; set; }
    }
}
