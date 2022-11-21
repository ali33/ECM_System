using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;

using Ecm.Mvvm;
using System.Resources;
using System.Reflection;

namespace Ecm.ContentViewer.Model
{
    public class ContentTypeModel : BaseDependencyProperty, IDataErrorInfo
    {
        private string _name;
        private string _description;
        private byte[] _icon;
        private bool _isSelected;
        private ObservableCollection<FieldModel> _fields = new ObservableCollection<FieldModel>();
        private IList<Guid> _deletedFields = new List<Guid>();
        private OCRTemplateModel _ocrTemplate;
        private bool _hasOCRTemplateDefined;
        private bool _hasBarcodeConfigurations;
        private ObservableCollection<BarcodeConfigurationModel> _barcodeConfigurations;
        private int _documentCount;
        private ResourceManager _resource = new ResourceManager("Ecm.CaptureModel.Resources", Assembly.GetExecutingAssembly());

        [XmlIgnore]
        public Action<ContentTypeModel> ErrorChecked { get; set; }

        public Guid Id { get; set; }

        public Guid BatchTypeId { get; set; }

        [XmlIgnore]
        public string UniqueId { get; set; }

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

        [XmlIgnore]
        public IList<Guid> DeletedFields
        {
            get { return _deletedFields; }
            set { _deletedFields = value; }
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

        [XmlIgnore]
        public OCRTemplateModel OCRTemplate
        {
            get { return _ocrTemplate; }
            set
            {
                _ocrTemplate = value;
                HasOCRTemplateDefined = (value != null);
                OnPropertyChanged("OCRTemplate");
            }
        }

        [XmlIgnore]
        public ObservableCollection<BarcodeConfigurationModel> BarcodeConfigurations
        {
            get { return _barcodeConfigurations; }
            set
            {
                _barcodeConfigurations = value;
                HasBarcodeConfigurations = false;
                if (value != null)
                {
                    _barcodeConfigurations.CollectionChanged += BarcodeConfigurationsCollectionChanged;
                    if (value.Count > 0)
                    {
                        HasBarcodeConfigurations = true;
                    }
                }
            }
        }

        [XmlIgnore]
        public bool HasOCRTemplateDefined
        {
            get { return _hasOCRTemplateDefined; }
            set
            {
                _hasOCRTemplateDefined = value;
                OnPropertyChanged("HasOCRTemplateDefined");
            }
        }

        [XmlIgnore]
        public bool HasBarcodeConfigurations
        {
            get { return _hasBarcodeConfigurations; }
            set
            {
                _hasBarcodeConfigurations = value;
                OnPropertyChanged("HasBarcodeConfigurations");
            }
        }

        [XmlIgnore]
        public ContentTypePermissionModel DocTypePermission { get; set; }

        [XmlIgnore]
        public AnnotationPermissionModel AnnotationPermission { get; set; }

        [XmlIgnore]
        public ObservableCollection<DocumentFieldPermissionModel> FieldPermissions { get; set; }

        private void BarcodeConfigurationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasBarcodeConfigurations = BarcodeConfigurations != null && BarcodeConfigurations.Count > 0;
        }

        [XmlIgnore]
        public int DocumentCount
        {
            get { return _documentCount; }
            set
            {
                _documentCount = value;
                OnPropertyChanged("DocumentCount");
                OnPropertyChanged("DisplayName");
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
                        return string.IsNullOrWhiteSpace(Name) ? _resource.GetString("uiContentTypeNameEmpty") : _resource.GetString("uiContentTypeNameExisted");
                    }
                }

                return msg;
            }
        }
    }
}
