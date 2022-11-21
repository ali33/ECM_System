using System.Xml.Serialization;
using Ecm.Mvvm;

using System;
using System.Collections.Generic;

namespace ArchiveMVC.Models
{
    public class BatchTypeModel
    {
        private Guid _id;
        private string _name;
        private List<DocumentTypeModel> _documentTypes = new List<DocumentTypeModel>();
        private List<FieldMetaDataModel> _fields = new List<FieldMetaDataModel>();

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        [XmlIgnore]
        public List<DocumentTypeModel> DocumentTypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
            }
        }

        [XmlIgnore]
        public List<FieldMetaDataModel> Fields
        {
            get { return _fields; }
            set 
            { 
                _fields = value;
            }
        }
    }
}
