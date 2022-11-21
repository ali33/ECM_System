using System.Runtime.Serialization;

namespace Ecm.Domain
{
    [DataContract]
    public enum DatabaseType
    {
        [EnumMember] 
        MsSql,
        [EnumMember] 
        MySql,
        [EnumMember] 
        Oracle,
        [EnumMember] 
        PostgreSql,
        [EnumMember] 
        DB2
    }
}