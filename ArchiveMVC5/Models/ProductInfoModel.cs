using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Models
{
    public class ProductInfoModel
    {
        [Display(Name = "Product version")]
        public string Version { get; set; }

        [Display(Name = "Copy right")]
        public string CopyRight { get; set; }

        [Display(Name = "Warning")]
        public string Wairning { get; set; }
    }
}