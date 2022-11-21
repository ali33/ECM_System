using System.Runtime.Serialization;

namespace Ecm.LookupDomain
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