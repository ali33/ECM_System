using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.Domain
{
    [DataContract]
    [Serializable, XmlRoot("LookupInfo")]
    public class ConnectionInfo
    {
        [DataMember(IsRequired = true)]
        [XmlElement]
        public DatabaseType DbType
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public string Host
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public int Port
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public string Username
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public string Password
        {
            get;
            set;
        }

        [DataMember(IsRequired = false)]
        [XmlElement]
        public string DatabaseName
        {
            get;
            set;
        }

        [DataMember(IsRequired = false)]
        [XmlElement]
        public string Schema
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public ProviderType ProviderType
        {
            get;
            set;
        }
    }
}