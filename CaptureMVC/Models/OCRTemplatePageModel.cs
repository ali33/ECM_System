using System;
using System.Collections.Generic;

namespace CaptureMVC.Models
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
    }
}