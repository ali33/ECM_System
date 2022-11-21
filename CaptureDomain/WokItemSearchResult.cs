using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class WorkItemSearchResult
    {
        /// <summary>
        /// Initialize the search result object
        /// </summary>
        public WorkItemSearchResult()
        {
            WorkItems = new List<Batch>();
        }

        /// <summary>
        /// Document type that the engine search for work items
        /// </summary>
        [DataMember]
        public BatchType BatchType { get; set; }

        /// <summary>
        /// Work items found at a <see cref="PageIndex"/>
        /// </summary>
        [DataMember]
        public List<Batch> WorkItems { get; set; }

        /// <summary>
        /// Whether more items are found
        /// </summary>
        [DataMember]
        public bool HasMoreResult { get; set; }

        /// <summary>
        /// Page index that the engine search for work items
        /// </summary>
        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public int TotalCount { get; set; }
    }
}
