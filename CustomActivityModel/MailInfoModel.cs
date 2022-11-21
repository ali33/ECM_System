using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class MailInfoModel : BaseDependencyProperty
    {
        private string _mailFrom;
        private string _mailSubject;
        private string _mailBody;
        private string _mailTos;
        private string _smtpHostName;
        private string _smtpPortNumber;
        private string _smtpUserName;
        private string _smtpPassword;

        public string MailFrom
        {
            get { return _mailFrom; }
            set
            {
                _mailFrom = value;
                OnPropertyChanged("MailFrom");
            }
        }

        public string MailTos
        {
            get { return _mailTos; }
            set
            {
                _mailTos = value;
                OnPropertyChanged("MailTos");
            }
        }

        public string Subject
        {
            get { return _mailSubject; }
            set
            {
                _mailSubject = value;
                OnPropertyChanged("Subject");
            }
        }

        public string Body
        {
            get { return _mailBody; }
            set
            {
                _mailBody = value;
                OnPropertyChanged("Body");
            }
        }

        public string SmtpHostName
        {
            get { return _smtpHostName; }
            set
            {
                _smtpHostName = value;
                OnPropertyChanged("SmtpHostName");
            }
        }

        public string SmtpUserName
        {
            get { return _smtpUserName; }
            set
            {
                _smtpUserName = value;
                OnPropertyChanged("SmtpUserName");
            }
        }

        public string SmtpPassword
        {
            get { return _smtpPassword; }
            set
            {
                _smtpPassword = value;
                OnPropertyChanged("SmtpPassword");
            }
        }

        public string SmtpPortNumber
        {
            get { return _smtpPortNumber; }
            set
            {
                _smtpPortNumber = value;
                OnPropertyChanged("SmtpPortNumber");
            }
        }
    }
}
