using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("Lookup")]
    public class LookupConnection
    {
        public int DbType
        {
            get;
            set;
        }

        public string Host
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string DatabaseName
        {
            get;
            set;
        }

        public string Schema
        {
            get;
            set;
        }

        public int ProviderType
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }

        public string SqlCommand
        {
            get;
            set;
        }

    }
}
