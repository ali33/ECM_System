using System;
using System.Collections.Generic;

namespace Ecm.Model
{
    public class OCRTemplatePageModel
    {
        public OCRTemplatePageModel()
        {
            OCRTemplateZones = new List<OCRTemplateZoneModel>();
        }

        public Guid Id
        {
            get;
            set;
        }

        public int PageIndex
        {
            get;
            set;
        }

        public byte[] Binary
        {
            get;
            set;
        }

        public double DPI
        {
            get;
            set;
        }

        public Guid OCRTemplateId
        {
            get;
            set;
        }

        public IList<OCRTemplateZoneModel> OCRTemplateZones { get; set; }

        public string FileExtension { get; set; }

        /// <summary>
        /// The angle which this page is rotated
        /// </summary>
        public double RotateAngle { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

    }
}
