using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Define the permissions on a human step of a workflow of user groups in the system
    /// </summary>
    [DataContract]
    public class HumanStepPermission
    {
        /// <summary>
        /// Initialize the object
        /// </summary>
        public HumanStepPermission()
        {
            UserGroupPermissions = new List<HumanStepUserGroupPermission>();
        }

        /// <summary>
        /// Clone to another object.
        /// </summary>
        /// <returns></returns>
        public HumanStepPermission Clone()
        {
            HumanStepPermission permission = new HumanStepPermission
                                                 {
                                                     HumanStepId = HumanStepId,
                                                     WorkflowDefinitionId = WorkflowDefinitionId,
                                                     UserGroupPermissions = new List<HumanStepUserGroupPermission>()
                                                 };

            UserGroupPermissions.ForEach(p => permission.UserGroupPermissions.Add(p.Clone()));

            return permission;
        }

        /// <summary>
        /// Identifier of the human step in the workflow.
        /// </summary>
        [DataMember]
        public Guid HumanStepId { get; set; }

        /// <summary>
        /// Identifier of the workflow definition 
        /// </summary>
        [DataMember]
        public Guid WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Collection of the permission of each user group on the human step.
        /// </summary>
        [DataMember]
        public List<HumanStepUserGroupPermission> UserGroupPermissions { get; set; }
    }
}
