using System.Xml.Serialization;
using Ecm.Mvvm;

using System.Collections.Generic;
using System;
using Ecm.Domain;
using System.ComponentModel;

namespace ArchiveMVC.Models
{
    public class DocumentTypeModel
    {
        private Guid _id;
        private string _name;
        private bool _isOutlook;
        private byte[] _icon;
        private List<FieldMetaDataModel> _fields = new List<FieldMetaDataModel>();
        private IList<FieldMetaDataModel> _deletedFields = new List<FieldMetaDataModel>();
        private OCRTemplateModel _ocrTemplate;
        private bool _hasOCRTemplateDefined;
        private bool _hasBarcodeConfigurations;
        private List<BarcodeConfigurationModel> _barcodeConfigurations;
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
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
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
        public List<FieldMetaDataModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
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
            }
        }

        [XmlIgnore]
        public List<BarcodeConfigurationModel> BarcodeConfigurations
        {
            get { return _barcodeConfigurations; }
            set
            {
                _barcodeConfigurations = value;
                HasBarcodeConfigurations = false;
                if (value != null)
                {
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
            }
        }

        [XmlIgnore]
        public bool HasBarcodeConfigurations
        {
            get { return _hasBarcodeConfigurations; }
            set
            {
                _hasBarcodeConfigurations = value;
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
            }
        }

        [XmlIgnore]
        public int DocumentCount
        {
            get { return _documentCount; }
            set
            {
                _documentCount = value;
            }
        }

        public string DisplayName
        {
            get { return string.Format("{0} ({1})", Name, DocumentCount); }
        }


    }
}
