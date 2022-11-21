using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the configuration of the lookup is defined in the <see cref="FieldMetaData"/> object.
    /// </summary>
    [DataContract]
    [Serializable]
    public class LookupInfo
    {
        /// <summary>
        /// Identifier of the <see cref="FieldMetaData"/> object contains the configuration.
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid FieldId { get; set; }

        /// <summary>
        /// The lookup connection info object.
        /// </summary>
        [DataMember]
        [XmlElement]
        public ConnectionInfo ConnectionInfo { get; set; }

        ///// <summary>
        ///// The server name the lookup service retrieve data from.
        ///// </summary>
        //[DataMember]
        //public string ServerName { get; set; }

        ///// <summary>
        ///// Data provider the lookup service use to retrieve data from the server. Right now, CloudECM support 3 types of provider:
        ///// <list type="bullet">
        /////     <item>
        /////         <description><b>System.Data.OleDb</b>: Use to access to MS Access or Excel...</description>
        /////     </item>
        /////     <item>
        /////         <description><b>System.Data.SqlClient</b>: Use to access to MS SQL Server</description>
        /////     </item>
        /////     <item>
        /////         <description><b>System.Data.OracleClient</b>: Use to access to Oracle database</description>
        /////     </item>
        ///// </list>
        ///// </summary>
        //[DataMember]
        //public string DataProvider { get; set; }

        ///// <summary>
        ///// User name is used to access to the server
        ///// </summary>
        //[DataMember]
        //public string Username { get; set; }

        ///// <summary>
        ///// Password is used to access to the server
        ///// </summary>
        //[DataMember]
        //public string Password { get; set; }

        /// <summary>
        /// The type of the SQL command: View, Table, or Stored Procedure.
        /// </summary>
        [DataMember]
        public int LookupType { get; set; }

        /// <summary>
        /// Define the SQL command that the lookup service use to search the data.
        /// </summary>
        [DataMember]
        [XmlElement]
        public string SqlCommand { get; set; }

        /// <summary>
        /// Maximum found items are returned.
        /// </summary>
        [DataMember]
        [XmlElement]
        public int MaxLookupRow { get; set; }

        /// <summary>
        /// How many characters user type will trigger the lookup.
        /// </summary>
        [DataMember]
        [XmlElement]
        public int MinPrefixLength { get; set; }

        /// <summary>
        /// Define the connection string so that the lookup service use to run the SQL command.
        /// </summary>
        [DataMember]
        [XmlElement]
        public string ConnectionString { get; set; }

        ///// <summary>
        ///// The database name that the SQL command will be executed on.
        ///// </summary>
        //[DataMember]
        //public string DatabaseName { get; set; }


        /// <summary>
        /// Define the data source name (table name, stored name or view name)
        /// </summary>
        [DataMember]
        [XmlElement]
        public string SourceName { get; set; }

        /// <summary>
        /// Define the lookup column name (Table column that looked up by)
        /// </summary>
        [DataMember]
        [XmlElement]
        public string LookupColumn { get; set; }

        /// <summary>
        /// Define the lookup operator
        /// </summary>
        [DataMember]
        [XmlElement]
        public string LookupOperator { get; set; }

        //[DataMember]
        //[XmlElement]
        //public string ParameterValue { get; set; }

        [DataMember]
        [XmlArray("Parameters"), XmlArrayItem(typeof(LookupParameter), ElementName = "LookupParameter")]
        public List<LookupParameter> Parameters { get; set; }

        [DataMember]
        [XmlArray("LookupMaps"), XmlArrayItem(typeof(LookupMap), ElementName = "LookupMap")]
        public List<LookupMap> LookupMaps { get; set; }
    }
}