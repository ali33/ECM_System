using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.CaptureDomain
{
    [Serializable]
    [XmlRoot("ActivityPermission")]
    public class UserGroupPermission
    {
        public Guid UserGroupId { get; set; }

        public bool CanModifyDocument { get; set; }

        public bool CanModifyIndexes { get; set; }

        public bool CanDelete { get; set; }

        public bool CanAnnotate { get; set; }

        public bool CanPrint { get; set; }

        public bool CanEmail { get; set; }

        public bool CanSendLink { get; set; }

        public bool CanDownloadFilesOnDemand { get; set; }

        public bool CanReleaseLoosePage { get; set; }

        public bool CanReject { get; set; }

        public bool CanViewOtherItems { get; set; }

        public bool CanDelegateItems { get; set; }
    }
}
