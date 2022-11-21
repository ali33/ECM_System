using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the mapping of a field with a column name in the return lookup data.
    /// </summary>
    [DataContract]
    public class LookupMap
    {
        /// <summary>
        /// Lookup field id
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid FieldId
        {
            get;
            set;
        }

        /// <summary>
        /// Archive field id that map to data source columns
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid ArchiveFieldId { get; set; }

        /// <summary>
        /// Name of archive field that map to data source columns
        /// </summary>
        [DataMember]
        [XmlElement]
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Data column that lookup call
        /// </summary>
        [DataMember]
        [XmlElement]
        public string DataColumn
        {
            get;
            set;
        }
    }
}