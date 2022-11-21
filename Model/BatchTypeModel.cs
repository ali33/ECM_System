using System.Xml.Serialization;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System;

namespace Ecm.Model
{
    public class BatchTypeModel : BaseDependencyProperty
    {
        private Guid _id;
        private string _name;
        private ObservableCollection<DocumentTypeModel> _documentTypes = new ObservableCollection<DocumentTypeModel>();
        private ObservableCollection<FieldMetaDataModel> _fields = new ObservableCollection<FieldMetaDataModel>();

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [XmlIgnore]
        public ObservableCollection<DocumentTypeModel> DocumentTypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
                OnPropertyChanged("DocumentTypes");
            }
        }

        [XmlIgnore]
        public ObservableCollection<FieldMetaDataModel> Fields
        {
            get { return _fields; }
            set 
            { 
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }
    }
}
