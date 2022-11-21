using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the search result on Archive module
    /// </summary>
    [DataContract]
    public class SearchResult
    {
        /// <summary>
        /// Initialize the search result object
        /// </summary>
        public SearchResult()
        {
            Documents = new List<Document>();
        }

        /// <summary>
        /// Document type that the engine search search for documents
        /// </summary>
        [DataMember]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Documents at <see cref="PageIndex"/> the the search engine found
        /// </summary>
        [DataMember]
        public List<Document> Documents { get; set; }

        /// <summary>
        /// Whether more items are found
        /// </summary>
        [DataMember]
        public bool HasMoreResult { get; set; }

        /// <summary>
        /// Total items that the search engine found
        /// </summary>
        [DataMember]
        public int TotalCount { get; set; }

        /// <summary>
        /// Page index the engine search for documents
        /// </summary>
        [DataMember]
        public int PageIndex { get; set; }
    }

    /*
    /// <summary>
    /// Represent the search result for work items of user
    /// </summary>
    [DataContract]
    public class WorkItemSearchResult
    {
        /// <summary>
        /// Initialize the search result object
        /// </summary>
        public WorkItemSearchResult()
        {
            WorkItems = new List<WorkItem>();
        }

        /// <summary>
        /// Document type that the engine search for work items
        /// </summary>
        [DataMember]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Work items found at a <see cref="PageIndex"/>
        /// </summary>
        [DataMember]
        public List<WorkItem> WorkItems { get; set; }

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
    }
     * */
}
