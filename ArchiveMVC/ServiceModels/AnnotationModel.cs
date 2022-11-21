using System;

using Ecm.Mvvm;
using System.Collections.Generic;

namespace ArchiveMVC.Models
{
    public class AnnotationModel
    {
        public Guid Id { get; set; }

        public Guid PageId { get; set; }

        public AnnotationTypeModel Type { get; set; }

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

        public double Height 
        {
            get { return _height; }
            set
            {
                _height = value;
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

        public DateTime? ModifiedOn
        {
            get { return _modifiedOn; }
            set
            {
                _modifiedOn = value;
            }
        }

        public OCRTemplateZoneModel OCRTemplateZone
        {
            get { return _ocrTemplateZone; } 
            set
            {
                _ocrTemplateZone = value;
            }
        }

        public List<FieldMetaDataModel> MetaFields { get; set; }


        private OCRTemplateZoneModel _ocrTemplateZone;
        private DateTime _createdOn;
        private string _createdBy;
        private DateTime? _modifiedOn;
        private string _modifiedBy;
        private double _top;
        private double _left;
        private double _width;
        private double _height;
    }
}
