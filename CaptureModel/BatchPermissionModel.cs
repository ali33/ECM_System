using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.CaptureModel
{
    public class BatchPermissionModel : BaseDependencyProperty
    {
        public Guid HumanStepID { get; set; }

        public Guid WorkflowDefinitionID { get; set; }

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

        public bool CanInsertDocument { get; set; }

        public bool CanChangeDocumentType { get; set; }

        public bool CanSplitDocument { get; set; }

        public bool CanDelegateItems { get; set; }

        public static BatchPermissionModel GetAllowAll()
        {
            return new BatchPermissionModel
            {
                CanAnnotate = true,
                CanChangeDocumentType = true,
                CanDelegateItems = true,
                CanDelete = true,
                CanDownloadFilesOnDemand = true,
                CanEmail = true,
                CanInsertDocument = true,
                CanModifyDocument = true,
                CanModifyIndexes = true,
                CanPrint = true,
                CanReject = true,
                CanReleaseLoosePage = true,
                CanSendLink = true,
                CanSplitDocument = true,
                CanViewOtherItems = true
            };
        }
    }
}
