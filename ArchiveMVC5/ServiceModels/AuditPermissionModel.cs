
using System;

namespace ArchiveMVC5.Models
{
    public class AuditPermissionModel
    {
        private bool _allowedAudit;
        private bool _allowedViewLog;
        private bool _allowedDeleteLog;
        private bool _allowedViewReport;
        private bool _allowedRestoreDocument;

        public Guid Id { get; set; }

        public Guid UserGroupId { get; set; }

        public Guid DocTypeId { get; set; }

        public bool AllowedAudit
        {
            get { return _allowedAudit; }
            set
            {
                _allowedAudit = value;
            }
        }

        public bool AllowedViewLog
        {
            get { return _allowedViewLog; }
            set
            {
                _allowedViewLog = value;
            }
        }

        public bool AllowedDeleteLog
        {
            get { return _allowedDeleteLog; }
            set
            {
                _allowedDeleteLog = value;
            }
        }

        public bool AllowedViewReport
        {
            get { return _allowedViewReport; }
            set
            {
                _allowedViewReport = value;
            }
        }

        public bool AllowedRestoreDocument
        {
            get { return _allowedRestoreDocument; }
            set
            {
                _allowedRestoreDocument = value;
            }
        }
    }
}
