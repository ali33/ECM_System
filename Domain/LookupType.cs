using System.Runtime.Serialization;

namespace Ecm.Domain
{
    [DataContract]
    public enum LookupType
    {
        [EnumMember] 
        Table,
        [EnumMember] 
        View,
        [EnumMember] 
        Stored
    }
}