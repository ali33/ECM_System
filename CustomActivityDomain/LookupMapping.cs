using System;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("Lookup")]
    public class LookupMapping
    {
        private string _fieldName;
        private string _columnName;

        public Guid FieldId { get; set; }

        /// <summary>
        /// Get or Set type of lookup: 0 for Batch and 1 for Document
        /// </summary>
        public int LookupType { get; set; }

        public string FieldName
        {
            get;
            set;
        }

        public string DataColumn
        {
            get;
            set;
        }

        public Guid CaptureFieldId { get; set; }

    }
}
