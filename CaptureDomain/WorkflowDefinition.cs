using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the definition of workflow 
    /// </summary>
    [DataContract]
    public class WorkflowDefinition
    {
        /// <summary>
        /// Identifier of the workflow
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// XAML code that define the workflow
        /// </summary>
        [DataMember]
        public string DefinitionXML { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }
    }
}
