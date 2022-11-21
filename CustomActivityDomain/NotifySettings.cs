using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    public class NotifySettings
    {
        public int NotifyType { get; set; }

        public MailInfo MailInfo { get; set; }

        public SmsInfo SmsInfo { get; set; }
    }

    public enum NotifyTypeEnum
    {
        Mail,
        SMS,
        Both
    }
}
