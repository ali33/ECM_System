using System;
using Ecm.Mvvm;

namespace Ecm.Model
{
    public class OCRTemplateZoneModel : BaseDependencyProperty
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
                OnPropertyChanged("FieldMetaData");
                if (_fieldMetaData != null)
                {
                    _fieldMetaData.PropertyChanged += FieldMetaDataPropertyChanged;
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
                OnPropertyChanged("FieldMetaDataId");
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                OnPropertyChanged("Top");
            }
        }

        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnPropertyChanged("Left");
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public string CreatedBy
        {
            get { return _createdBy; }
            set
            {
                _createdBy = value;
                OnPropertyChanged("CreatedBy");
            }
        }

        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set
            {
                _createdOn = value;
                OnPropertyChanged("CreatedOn");
            }
        }

        public string ModifiedBy
        {
            get { return _modifiedBy; }
            set
            {
                _modifiedBy = value;
                OnPropertyChanged("ModifiedBy");
            }
        }

        public DateTime ModifiedOn
        {
            get { return _modifiedOn; }
            set
            {
                _modifiedOn = value;
                OnPropertyChanged("ModifiedOn");
            }
        }

        private void FieldMetaDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Id")
            {
                OnPropertyChanged("FieldMetaData");
            }
        }
    }
}
