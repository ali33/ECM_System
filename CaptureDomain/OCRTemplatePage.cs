using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class OCRTemplatePage
    {
        public OCRTemplatePage()
        {
            OCRTemplateZones = new List<OCRTemplateZone>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public byte[] Binary { get; set; }

        [DataMember]
        public double DPI { get; set; }

        [DataMember]
        public Guid OCRTemplateId { get; set; }
        /// <summary>
        /// The angle which this page is rotated
        /// </summary>
        [DataMember]
        public double RotateAngle { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public string FileExtension { get; set; }

        /// <summary>
        /// Collection of zones on the pages. The zone will be used to extract data and then store into fields of the document
        /// </summary>
        [DataMember]
        public List<OCRTemplateZone> OCRTemplateZones { get; set; }
    }
}
