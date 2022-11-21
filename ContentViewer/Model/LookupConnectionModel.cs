using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.ContentViewer.Model
{
    public class LookupConnectionModel : BaseDependencyProperty
    {
        private int _databaseType;
        private string _host;
        private int _port;
        private string _username;
        private string _password;
        private string _databaseName;
        private string _schema;
        private int _providerType;
        private string _connectionString;
        private string _sqlCommand;

        public int DatabaseType
        {
            get { return _databaseType; }
            set
            {
                _databaseType = value;
                OnPropertyChanged("DatabaseType");
            }
        }

        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                OnPropertyChanged("Host");
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
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

        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                _databaseName = value;
                OnPropertyChanged("DatabaseName");
            }
        }

        public string Schema
        {
            get { return _schema; }
            set
            {
                _schema = value;
                OnPropertyChanged("Schema");
            }
        }

        public int ProviderType
        {
            get { return _providerType; }
            set
            {
                _providerType = value;
                OnPropertyChanged("ProviderType");
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }

        public string SqlCommand
        {
            get { return _sqlCommand; }
            set
            {
                _sqlCommand = value;
                OnPropertyChanged("SqlCommand");
            }
        }

    }
}
