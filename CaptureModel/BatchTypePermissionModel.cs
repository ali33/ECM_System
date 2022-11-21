using Ecm.Mvvm;
using System;

namespace Ecm.CaptureModel
{
    public class BatchTypePermissionModel : BaseDependencyProperty
    {
        private bool _canCapture;
        private bool _canAccess;
        private bool _canIndex;
        private bool _canClassify;

        public Guid Id { get; set; }

        public Guid BatchTypeId { get; set; }

        public Guid UserGroupId { get; set; }

        public bool CanCapture
        {
            get { return _canCapture; }
            set
            {
                _canCapture = value;
                OnPropertyChanged("CanCapture");
            }
        }

        public bool CanAccess
        {
            get { return _canAccess; }
            set
            {
                _canAccess = value;
                OnPropertyChanged("CanAccess");
            }
        }

        public bool CanIndex
        {
            get { return _canIndex; }
            set
            {
                _canIndex = value;
                OnPropertyChanged("CanIndex");
            }
        }

        public bool CanClassify
        {
            get { return _canClassify; }
            set
            {
                _canClassify = value;
                OnPropertyChanged("CanClassify");
            }
        }
    }
}
