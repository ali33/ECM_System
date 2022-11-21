using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    [Serializable()]
    public class LoginInfoModel : BaseDependencyProperty
    {
        [NonSerialized()]
        private string _username;
        [NonSerialized()]
        private string _password;
        [NonSerialized()]
        private string _archiveEndPoint;

        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public string ArchiveEndPoint
        {
            get { return _archiveEndPoint; }
            set
            {
                _archiveEndPoint = value;
                OnPropertyChanged("ArchiveEndPoint");
            }

        }
    }
}
