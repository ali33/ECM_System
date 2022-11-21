using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the query that user make the search on Archive module.
    /// </summary>
    [DataContract]
    public class SearchQuery
    {
        /// <summary>
        /// Initialize new query object
        /// </summary>
        public SearchQuery()
        {
            SearchQueryExpressions = new List<SearchQueryExpression>();
            DeletedSearchQueryExpressions = new List<Guid>();
        }

        /// <summary>
        /// Identifier of the object
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="User"/> make this query.
        /// </summary>
        [DataMember]
        public Guid UserId { get; set; }

        /// <summary>
        /// Identifier of the <see cref="DocumentType"/> that this query search for documents.
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Name of the query for save
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Collection of criterias that the search need to match documents.
        /// </summary>
        [DataMember]
        public List<SearchQueryExpression> SearchQueryExpressions { get; set; }

        /// <summary>
        /// Contains the deleted criterias
        /// </summary>
        [DataMember]
        public List<Guid> DeletedSearchQueryExpressions { get; set; }

        /// <summary>
        /// The document type object this query search for documents
        /// </summary>
        [DataMember]
        public DocumentType DocumentType { get; set; }
    }
}