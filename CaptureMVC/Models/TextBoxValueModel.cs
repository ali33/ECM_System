using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CaptureMVC.Models
{
    public class TextBoxValueModel
    {
        public string ControlId { get; set; }
        public FieldDataType FieldDataType { get; set; }
        public SearchOperator SearchOperator { get; set; }
        public string Value1 { get; set; }
    }
}
