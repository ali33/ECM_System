using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the each page in the OCR template.
    /// </summary>
    [DataContract]
    public sealed class OCRTemplatePage
    {
        /// <summary>
        /// Initialize the page object
        /// </summary>
        public OCRTemplatePage()
        {
            OCRTemplateZones = new List<OCRTemplateZone>();
        }

        /// <summary>
        /// Identifier of the object
        /// </summary>
        [DataMember]
        public Guid Id
        {
            get;
            set;
        }

        /// <summary>
        /// The index of this page in the template
        /// </summary>
        [DataMember]
        public int PageIndex
        {
            get;
            set;
        }

        /// <summary>
        /// The binary of this page in the template
        /// </summary>
        [DataMember]
        public byte[] Binary
        {
            get;
            set;
        }

        /// <summary>
        /// The DPI of this page in the template
        /// </summary>
        [DataMember]
        public double DPI
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of the OCR template
        /// </summary>
        [DataMember]
        public Guid OCRTemplateId
        {
            get;
            set;
        }

        /// <summary>
        /// Collection of zones on the pages. The zone will be used to extract data and then store into fields of the document
        /// </summary>
        [DataMember]
        public List<OCRTemplateZone> OCRTemplateZones { get; set; }

        /// <summary>
        /// The angle which this page is rotated
        /// </summary>
        [DataMember]
        public double RotateAngle { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }


        /// <summary>
        /// The extension of the file is using to define the template.
        /// </summary>
        [DataMember]
        public string FileExtension
        {
            get;
            set;
        }

    }
}