using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("NotifySettings")]
    public class MailInfo
    {
        public string MailFrom { get; set; }

        public string MailTos { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string SmtpHostName { get; set; }

        public string SmtpUserName { get; set; }

        public string SmtpPassword { get; set; }

        public string SmtpPortNumber { get; set; }
    }
}
