using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ecm.Mvvm;
using System.Collections.ObjectModel;

namespace Ecm.CaptureModel
{
    public class AnnotationModel : BaseDependencyProperty
    {

        private OCRTemplateZoneModel _ocrTemplateZone;
        private DateTime _createdOn;
        private string _createdBy;
        private DateTime _modifiedOn;
        private string _modifiedBy;
        private double _top;
        private double _left;
        private double _width;
        private double _height;

        public Guid Id { get; set; }

        public Guid PageId { get; set; }

        public AnnotationTypeModel Type { get; set; }

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

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
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

        public RectangleVertexModel LineEndAt { get; set; }

        public RectangleVertexModel LineStartAt { get; set; }

        public LineStyleModel LineStyle { get; set; }

        public int LineWeight { get; set; }

        public double RotateAngle { get; set; }

        public string LineColor { get; set; }

        public string Content { get; set; }

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

        public OCRTemplateZoneModel OCRTemplateZone
        {
            get { return _ocrTemplateZone; }
            set
            {
                _ocrTemplateZone = value;
                OnPropertyChanged("OCRTemplateZone");
                if (_ocrTemplateZone != null)
                {
                    _ocrTemplateZone.PropertyChanged += OCRTemplateZonePropertyChanged;
                }
            }
        }

        public ObservableCollection<FieldModel> MetaFields { get; set; }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (Type == AnnotationTypeModel.OCRZone)
            {
                if (propertyName == "Top")
                {
                    OCRTemplateZone.Top = Top;
                }
                else if (propertyName == "Left")
                {
                    OCRTemplateZone.Left = Left;
                }
                else if (propertyName == "Width")
                {
                    OCRTemplateZone.Width = Width;
                }
                else if (propertyName == "Height")
                {
                    OCRTemplateZone.Height = Height;
                }
                else if (propertyName == "CreatedBy")
                {
                    OCRTemplateZone.CreatedBy = CreatedBy;
                }
                else if (propertyName == "CreatedOn")
                {
                    OCRTemplateZone.CreatedOn = CreatedOn;
                }
                else if (propertyName == "ModifiedBy")
                {
                    OCRTemplateZone.ModifiedBy = ModifiedBy;
                }
                else if (propertyName == "ModifiedOn")
                {
                    OCRTemplateZone.ModifiedOn = ModifiedOn;
                }
            }
        }

        private void OCRTemplateZonePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FieldMetaData")
            {
                OnPropertyChanged("OCRTemplateZone");
            }
        }
    }
}
