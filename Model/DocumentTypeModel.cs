using System.Xml.Serialization;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using Ecm.Domain;
using System.ComponentModel;

namespace Ecm.Model
{
    public class DocumentTypeModel : BaseDependencyProperty, IDataErrorInfo
    {
        private Guid _id;
        private string _name;
        private bool _isOutlook;
        private byte[] _icon;
        private ObservableCollection<FieldMetaDataModel> _fields = new ObservableCollection<FieldMetaDataModel>();
        private IList<FieldMetaDataModel> _deletedFields = new List<FieldMetaDataModel>();
        private OCRTemplateModel _ocrTemplate;
        private bool _hasOCRTemplateDefined;
        private bool _hasBarcodeConfigurations;
        private ObservableCollection<BarcodeConfigurationModel> _barcodeConfigurations;
        private int _documentCount;
        private bool _isSelected;

        public DocumentTypeModel()
        {
        }

        public DocumentTypeModel(Action<DocumentTypeModel> action)
        {
            ErrorChecked = action;
        }

        [XmlIgnore]
        public Action<DocumentTypeModel> ErrorChecked { get; set; }

        public bool HasError { get; set; }

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
        public ObservableCollection<FieldMetaDataModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        [XmlIgnore]
        public IList<FieldMetaDataModel> DeletedFields
        {
            get { return _deletedFields; }
            set
            {
                _deletedFields = value;
            }
        }

        [XmlIgnore]
        public bool IsOutlook
        {
            get { return _isOutlook; }
            set
            {
                _isOutlook = value;
                OnPropertyChanged("IsOutlook");
            }
        }

        //[XmlIgnore]
        //public string UniqueId { get; set; }

        [XmlIgnore]
        public DateTime CreatedDate { get; set; }

        [XmlIgnore]
        public string CreateBy { get; set; }

        [XmlIgnore]
        public string ModifiedBy { get; set; }

        [XmlIgnore]
        public DateTime ModifiedDate { get; set; }

        [XmlIgnore]
        public DocumentTypePermissionModel DocumentTypePermission { get; set; }

        [XmlIgnore]
        public AnnotationPermissionModel AnnotationPermission { get; set; }

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
                    if (value.Count >0)
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
        public Status Status { get; set; }

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

        public string DisplayName
        {
            get { return string.Format("{0} ({1})", Name, DocumentCount); }
        }

        private void BarcodeConfigurationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasBarcodeConfigurations = BarcodeConfigurations != null && BarcodeConfigurations.Count > 0;
        }

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
                        return string.IsNullOrWhiteSpace(Name) ? Resources.uiContentTypeEmpty : Resources.uiContentTypeNameExisted;
                    }
                }

                return msg;
            }
        }
    }
}
