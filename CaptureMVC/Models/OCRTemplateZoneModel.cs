using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptureMVC.Models
{
    public class OCRTemplateZoneModel
    {
        public Guid OCRTemplatePageId { get; set; }

        public Guid FieldMetaDataId { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }

    }
}