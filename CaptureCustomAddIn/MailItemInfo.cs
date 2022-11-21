using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.CaptureCustomAddIn
{
    public class MailItemInfo
    {
        public int ID { get; set; }
        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string MailBody { get; set; }
        public string MailSubject { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string TempFolderName { get; set; }
        public List<string> Attachments { get; set; }
        public string BodyFileName { get; set; }
        public Dictionary<string, byte[]> EmbeddedPictures { get; set; }
    }
}
