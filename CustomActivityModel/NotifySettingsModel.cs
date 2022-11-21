using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class NotifySettingsModel : BaseDependencyProperty
    {
        private NotifyType _notifyType;
        private MailInfoModel _mailInfo = new MailInfoModel();
        private SmsInfoModel _smsInfo = new SmsInfoModel();

        public NotifyType NotifyType
        {
            get { return _notifyType; }
            set
            {
                _notifyType = value;
                OnPropertyChanged("NotifyType");
            }
        }

        public MailInfoModel MailInfo
        {
            get { return _mailInfo; }
            set
            {
                _mailInfo = value;
                OnPropertyChanged("MailInfo");
            }
        }

        public SmsInfoModel SmsInfo
        {
            get { return _smsInfo; }
            set
            {
                _smsInfo = value;
                OnPropertyChanged("SmsInfo");
            }
        }
    }

    public enum NotifyType
    {
        Mail,
        SMS,
        Both
    }
}
