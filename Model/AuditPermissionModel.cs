using Ecm.Mvvm;
using System;

namespace Ecm.Model
{
    public class AuditPermissionModel : BaseDependencyProperty
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
                OnPropertyChanged("AllowedAudit");
            }
        }

        public bool AllowedViewLog
        {
            get { return _allowedViewLog; }
            set
            {
                _allowedViewLog = value;
                OnPropertyChanged("AllowedViewLog");
            }
        }

        public bool AllowedDeleteLog
        {
            get { return _allowedDeleteLog; }
            set
            {
                _allowedDeleteLog = value;
                OnPropertyChanged("AllowedDeleteLog");
            }
        }

        public bool AllowedViewReport
        {
            get { return _allowedViewReport; }
            set
            {
                _allowedViewReport = value;
                OnPropertyChanged("AllowedViewReport");
            }
        }

        public bool AllowedRestoreDocument
        {
            get { return _allowedRestoreDocument; }
            set
            {
                _allowedRestoreDocument = value;
                OnPropertyChanged("AllowedRestoreDocument");
            }
        }
    }
}
