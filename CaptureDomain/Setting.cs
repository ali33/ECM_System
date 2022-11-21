using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
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
        /// Get or Set value status of OCR client
        /// </summary>
        [DataMember]
        public bool EnabledOCRClient { get; set; }

        /// <summary>
        /// Get or Set value status of barcode client
        /// </summary>
        [DataMember]
        public bool EnabledBarcodeClient { get; set; }

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
    }
}