using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ArchiveMVC5.Models
{
    public class DocumentVersionModel
    {
        private int _version;
        private string _changeType;

        public Guid DocId { get; set; }

        public Guid DocTypeId { get; set; }

        public Guid VersionId { get; set; }

        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
            }
        }

        public string ChangeType
        {
            get { return _changeType; }
            set
            {
                _changeType = value;
            }
        }
    }
}
