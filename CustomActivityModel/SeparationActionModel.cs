using System;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class SeparationActionModel : BaseDependencyProperty
    {
        private bool _isSelected;

        public Guid Id { get; set; }

        public BarcodeTypeModel BarcodeType { get; set; }

        public int BarcodePositionInDoc { get; set; }

        public string BarcodePositionInDocText
        {
            get { return BarcodeType == BarcodeTypeModel.PATCH ? string.Empty : BarcodePositionInDoc + string.Empty; }
        }

        public string StartsWith { get; set; }

        public bool HasSpecifyDocumentType { get; set; }

        public Guid DocTypeId { get; set; }

        public string DocTypeName { get; set; }

        public bool RemoveSeparatorPage { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public SeparationActionModel Clone()
        {
            return new SeparationActionModel
                       {
                           Id = Id,
                           BarcodeType = BarcodeType,
                           BarcodePositionInDoc = BarcodePositionInDoc,
                           StartsWith = StartsWith,
                           DocTypeId = DocTypeId,
                           RemoveSeparatorPage = RemoveSeparatorPage,
                           HasSpecifyDocumentType = HasSpecifyDocumentType
                       };
        }
    }
}
