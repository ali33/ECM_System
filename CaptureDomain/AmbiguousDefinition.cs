using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the OCR auto correction dictionary. The structure of the dictionary is defined by the wrong word = right word. Each correction is separated by a new line
    /// </summary>
    /// <example>
    /// <para>ii = n</para>
    /// <para>l&lt;=k</para>
    /// </example>
    [DataContract]
    public class AmbiguousDefinition
    {
        /// <summary>
        /// Identifier of the dictionary
        /// </summary>
        [DataMember]
        public Guid ID { get; set; }

        /// <summary>
        /// The identifier of the <see cref="Language"/> the dictionay belong to.
        /// </summary>
        [DataMember]
        public Guid LanguageID { get; set; }

        /// <summary>
        /// Is applied for unicode font. 
        /// </summary>
        [DataMember]
        public bool Unicode { get; set; }

        /// <summary>
        /// The text defined the dictionay, each item is seprated by new line.
        /// </summary>
        [DataMember]
        public string Text { get; set; }

        /// <summary>
        /// The <see cref="Language"/> object that the dictionary belong to.
        /// </summary>
        [DataMember]
        public Language Language { get; set; }
    }
}