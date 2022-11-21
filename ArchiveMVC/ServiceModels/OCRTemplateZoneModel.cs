using System;
using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    public class OCRTemplateZoneModel
    {
        private double _top;
        private double _left;
        private double _width;
        private double _height;
        private FieldMetaDataModel _fieldMetaData;
        private DateTime _createdOn;
        private string _createdBy;
        private DateTime _modifiedOn;
        private string _modifiedBy;
        private Guid _fieldMetaDataId;

        public Guid OCRTemplatePageId
        {
            get; set;
        }

        public FieldMetaDataModel FieldMetaData
        {
            get { return _fieldMetaData; }
            set
            {
                _fieldMetaData = value;
                if (_fieldMetaData != null)
                {
                    FieldMetaDataId = _fieldMetaData.Id;
                }
            }
        }

        public Guid FieldMetaDataId
        {
            get { return _fieldMetaDataId; }
            set
            {
                _fieldMetaDataId = value;
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
            }
        }

        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
            }
        }

        public string CreatedBy
        {
            get { return _createdBy; }
            set
            {
                _createdBy = value;
            }
        }

        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set
            {
                _createdOn = value;
            }
        }

        public string ModifiedBy
        {
            get { return _modifiedBy; }
            set
            {
                _modifiedBy = value;
            }
        }

        public DateTime ModifiedOn
        {
            get { return _modifiedOn; }
            set
            {
                _modifiedOn = value;
            }
        }

    }
}
