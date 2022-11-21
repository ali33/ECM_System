using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent each zone on a page of the <see cref="OCRTemplate"/>.
    /// </summary>
    [DataContract]
    public class OCRTemplateZone
    {        

        /// <summary>
        /// Identifier of the page in the <see cref="OCRTemplate"/>
        /// </summary>
        [DataMember]
        public Guid OCRTemplatePageId
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of the <see cref="FieldMetaData"/> which will store the extracted data.
        /// </summary>
        [DataMember]
        public Guid FieldMetaDataId
        {
            get;
            set;
        }

        /// <summary>
        /// Top position of the zone on the page
        /// </summary>
        [DataMember]
        public double Top
        {
            get;
            set;
        }

        /// <summary>
        /// Left position of the zone on the page
        /// </summary>
        [DataMember]
        public double Left
        {
            get;
            set;
        }

        /// <summary>
        /// Width of the zone on the page
        /// </summary>
        [DataMember]
        public double Width
        {
            get;
            set;
        }

        /// <summary>
        /// Height of the zone on the page
        /// </summary>
        [DataMember]
        public double Height
        {
            get;
            set;
        }

        /// <summary>
        /// The user name of the user create this zone
        /// </summary>
        [DataMember]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date when this zone is created
        /// </summary>
        [DataMember]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// The user name of the user modify this zone
        /// </summary>
        [DataMember]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date when this zone is modified
        /// </summary>
        [DataMember]
        public DateTime ModifiedOn { get; set; }
    }
}