using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent all the settings in CloudECM
    /// </summary>
    [DataContract]
    public class Setting
    {
        /// <summary>
        /// Number of items on each page of the search result.
        /// </summary>
        [DataMember]
        public int SearchResultPageSize { get; set; }

        /// <summary>
        /// Get or Set the path of working folder on server
        /// </summary>
        [DataMember]
        public string ServerWorkingFolder { get; set; }

        /// <summary>
        /// Get or set Location Save File upload from client
        /// </summary>
        [DataMember]
        public bool IsSaveFileInFolder { get; set; }

        /// <summary>
        /// Get or Set location save files
        /// </summary>
        [DataMember]
        public string LocationSaveFile { get; set; }

        [DataMember]
        public string LuceneFolder { get; set; }

    }
}