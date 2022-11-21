using System;
using Ecm.CaptureDomain;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CaptureMVC.Models
{
    public class DocumentTypeModel
    {
        private Guid _id;
        private string _name;
        private byte[] _icon;
        private OCRTemplateModel _ocrTemplate;
        private bool _hasOCRTemplateDefined;
        private bool _hasBarcodeConfigurations;
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
        public DateTime CreatedDate { get; set; }

        [XmlIgnore]
        public string CreateBy { get; set; }

        [XmlIgnore]
        public string ModifiedBy { get; set; }

        [XmlIgnore]
        public DateTime? ModifiedDate { get; set; }

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