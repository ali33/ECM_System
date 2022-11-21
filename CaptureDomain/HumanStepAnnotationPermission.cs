using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class HumanStepAnnotationPermission : BaseAnnotationPermission
    {
        [DataMember]
        public Guid HumanStepId { get; set; }

        [DataMember]
        public Guid WorkflowDefinitionId { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        public HumanStepAnnotationPermission GetAllowAll()
        {
            return new HumanStepAnnotationPermission
            {
                CanAddHighlight = true,
                CanAddRedaction = true,
                CanAddText = true,
                CanDeleteHighlight = true,
                CanDeleteRedaction = true,
                CanDeleteText = true,
                CanHideRedaction = true,
                CanSeeHighlight = true,
                CanSeeText = true
            };
        }
    }

    [DataContract]
    public class BaseAnnotationPermission
    {
        [DataMember]
        public bool CanSeeText { get; set; }

        [DataMember]
        public bool CanAddText { get; set; }

        [DataMember]
        public bool CanDeleteText { get; set; }

        [DataMember]
        public bool CanSeeHighlight { get; set; }

        [DataMember]
        public bool CanAddHighlight { get; set; }

        [DataMember]
        public bool CanDeleteHighlight { get; set; }

        [DataMember]
        public bool CanHideRedaction { get; set; }

        [DataMember]
        public bool CanAddRedaction { get; set; }

        [DataMember]
        public bool CanDeleteRedaction { get; set; }

        public static BaseAnnotationPermission GetAllowAll()
        {
            return new BaseAnnotationPermission
            {
                CanAddHighlight = true,
                CanAddRedaction = true,
                CanAddText = true,
                CanDeleteHighlight = true,
                CanDeleteRedaction = true,
                CanDeleteText = true,
                CanHideRedaction = true,
                CanSeeHighlight = true,
                CanSeeText = true
            };
        }

    }
}
