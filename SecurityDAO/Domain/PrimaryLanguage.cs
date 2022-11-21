using System;
using System.Runtime.Serialization;

namespace Ecm.SecurityDao.Domain
{
    /// <summary>
    /// Represent the information of the supported language in the system.
    /// </summary>
    [DataContract]
    public class PrimaryLanguage
    {
        /// <summary>
        /// Identifier of the language
        /// </summary>
        [DataMember]
        public Guid ID { get; set; }

        /// <summary>
        /// Name of the language
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The format of the language which is used internally.
        /// </summary>
        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public string DateFormat { get; set; }

        [DataMember]
        public string TimeFormat { get; set; }

        [DataMember]
        public string ThousandChar { get; set; }

        [DataMember]
        public string DecimalChar { get; set; }
    }
}