using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class CustomActivitySetting
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ActivityId { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public Guid WorkflowDefinitionId { get; set; }

        public CustomActivitySetting Clone()
        {
            return new CustomActivitySetting
            {
                ActivityId = ActivityId,
                Value = Value,
                Id = Id,
                WorkflowDefinitionId = WorkflowDefinitionId
            };
        }
    }
}
