using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class OutlookPicture
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid DocId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public byte[] FileBinary { get; set; }

        [DataMember]
        public string TempPath { get; set; }
    }
}
