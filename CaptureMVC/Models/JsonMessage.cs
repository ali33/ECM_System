using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptureMVC.Models
{
    public class ECMJsonMessage
    {
        public bool IsError { set; get; }
        public bool IsTimeOut { set; get; }
        public string Message { set; get; }
        public string Detail { set; get; }
    }
}