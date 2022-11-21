using System.Runtime.Serialization;

namespace Ecm.Domain
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