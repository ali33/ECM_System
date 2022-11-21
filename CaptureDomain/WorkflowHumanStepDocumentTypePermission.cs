using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class WorkflowHumanStepDocumentTypePermission : BaseHumanStepDocTypePermission
    {
        [DataMember]
        public Guid HumanStepId { get; set; }

        [DataMember]
        public Guid WorkflowDefinitionId { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }
    }
}
