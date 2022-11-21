using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ArchiveMVC5.Models
{
    public class LookupConnectionModel
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
            }
        }

        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
            }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                _databaseName = value;
            }
        }

        public string Schema
        {
            get { return _schema; }
            set
            {
                _schema = value;
            }
        }

        public int ProviderType
        {
            get { return _providerType; }
            set
            {
                _providerType = value;
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
            }
        }

        public string SqlCommand
        {
            get { return _sqlCommand; }
            set
            {
                _sqlCommand = value;
            }
        }

    }
}
