using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

using Ecm.Mvvm;
using System.Resources;
using System.Reflection;

namespace Ecm.ContentViewer.Model
{
    public class BatchTypeModel : BaseDependencyProperty, IDataErrorInfo
    {
        private Guid _id;
        private string _name;
        private string _description;
        private byte[] _icon;
        private bool _isSelected;
        private ObservableCollection<FieldModel> _fields = new ObservableCollection<FieldModel>();
        private IList<Guid> _deletedFields = new List<Guid>();
        private ObservableCollection<ContentTypeModel> _docTypes = new ObservableCollection<ContentTypeModel>();
        private IList<Guid> _deletedDocTypes = new List<Guid>();
        private ResourceManager _resource = new ResourceManager("Ecm.ContentViewer.Model.Resources", Assembly.GetExecutingAssembly());

        public BatchTypeModel()
        {
        }

        public BatchTypeModel(Action<BatchTypeModel> action)
        {
            ErrorChecked = action;
        }
            
        [XmlIgnore]
        public Action<BatchTypeModel> ErrorChecked { get; set; }

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
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        [XmlIgnore]
        public byte[] Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        [XmlIgnore]
        public string UniqueId { get; set; }

        [XmlIgnore]
        public Guid WorkflowDefinitionId { get; set; }

        [XmlIgnore]
        public DateTime CreatedDate { get; set; }

        [XmlIgnore]
        public string CreatedBy { get; set; }

        [XmlIgnore]
        public string ModifiedBy { get; set; }

        [XmlIgnore]
        public DateTime? ModifiedDate { get; set; }

        [XmlIgnore]
        public ObservableCollection<FieldModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        public BatchTypePermissionModel BatchTypePermission { get; set; }

        public BarcodeConfigurationModel BarcodeConfiguration { get; set; }

        public string BarcodeConfigurationXml { get; set; }

        [XmlIgnore]
        public IList<Guid> DeletedFields
        {
            get { return _deletedFields; }
            set
            {
                _deletedFields = value;
            }
        }

        [XmlIgnore]
        public ObservableCollection<ContentTypeModel> DocTypes
        {
            get { return _docTypes; }
            set
            {
                _docTypes = value;
                OnPropertyChanged("DocTypes");
            }
        }

        [XmlIgnore]
        public IList<Guid> DeletedDocTypes
        {
            get { return _deletedDocTypes; }
            set
            {
                _deletedDocTypes = value;
            }
        }
        
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public bool HasError { get; set; }

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                string msg = string.Empty;
                if (columnName == "Name")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasError)
                    {
                        return string.IsNullOrWhiteSpace(Name) ? _resource.GetString("uiBatchTypeNameEmpty") : _resource.GetString("uiBatchTypeNameExisted");
                    }
                }

                return msg;
            }
        }
    }
}
