using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ecm.Domain
{
    /// <summary>
    /// Contain infomation about linked documents
    /// </summary>
    [DataContract]
    public class LinkDocument
    {
        /// <summary>
        /// Id of document info
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Parent document id
        /// </summary>
        [DataMember]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Linked document id
        /// </summary>
        [DataMember]
        public Guid LinkDocumentId { get; set; }

        /// <summary>
        /// Linked notes
        /// </summary>
        [DataMember]
        public string Notes { get; set; }

        /// <summary>
        /// Root document object
        /// </summary>
        [DataMember]
        public Document RootDocument { get; set; }

        /// <summary>
        /// Linked document object
        /// </summary>
        [DataMember]
        public Document LinkedDocument { get; set; }
    }
}
