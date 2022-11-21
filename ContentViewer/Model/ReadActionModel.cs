using System;
using System.Collections.Generic;
using System.Linq;
using Ecm.Mvvm;

namespace Ecm.ContentViewer.Model
{
    public class ReadActionModel : BaseDependencyProperty
    {
        private bool _isSelected;
        public ReadActionModel()
        {
            CopyValueToFields = new List<CopyValueToFieldModel>();
        }

        public Guid Id { get; set; }

        public BarcodeTypeModel BarcodeType { get; set; }

        public int BarcodePositionInDoc { get; set; }

        public string StartsWith { get; set; }

        public string CopyValueToFieldName
        {
            get
            {
                string targetName = string.Empty;

                if (CopyValueToFields != null && CopyValueToFields.Count > 0)
                {
                    if (CopyValueToFields.Count == 1)
                    {
                        targetName = CopyValueToFields[0].FieldName;
                    }
                    else
                    {
                        string template = Separator + " {0}";
                        targetName = CopyValueToFields.Aggregate(targetName, (current, copyValueToIndexModel) => current + string.Format(template, copyValueToIndexModel.FieldName));
                        targetName = targetName.Substring(Separator.Length + 1);
                    }
                }

                return targetName;
            }
        }

        public List<CopyValueToFieldModel> CopyValueToFields { get; set; }

        public bool IsDocIndex { get; set; }

        public Guid DocTypeId { get; set; }

        public string TargetTypeName { get; set; }

        public string Separator { get; set; }

        public bool OverwriteFieldValue { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ReadActionModel Clone()
        {
            ReadActionModel actionModel = new ReadActionModel
                                              {
                                                  Id = Id,
                                                  BarcodeType = BarcodeType,
                                                  BarcodePositionInDoc = BarcodePositionInDoc,
                                                  StartsWith = StartsWith,
                                                  DocTypeId = DocTypeId,
                                                  IsDocIndex = IsDocIndex,
                                                  TargetTypeName = TargetTypeName,
                                                  Separator = Separator,
                                                  OverwriteFieldValue = OverwriteFieldValue
                                              };

            foreach (CopyValueToFieldModel toIndexModel in CopyValueToFields)
            {
                actionModel.CopyValueToFields.Add(toIndexModel.Clone());
            }

            return actionModel;
        }
    }

    public class CopyValueToFieldModel
    {
        public string FieldGuid { get; set; }

        public string FieldName { get; set; }

        public int Position { get; set; }

        public CopyValueToFieldModel Clone()
        {
            return new CopyValueToFieldModel
                       {
                           FieldGuid = FieldGuid,
                           FieldName = FieldName,
                           Position = Position
                       };
        }
    }
}
