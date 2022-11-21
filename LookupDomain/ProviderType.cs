using System.Runtime.Serialization;

namespace Ecm.LookupDomain
{
    [DataContract]
    public enum ProviderType
    {
       [EnumMember] 
        OleDb = 1,
       [EnumMember] 
        AdoNet = 2
    }
}