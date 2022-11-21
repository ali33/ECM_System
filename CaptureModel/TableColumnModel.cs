using System;
using Ecm.CaptureDomain;

namespace Ecm.CaptureModel
{
    public class TableColumnModel : ICloneable
    {
        public Guid ColumnGuid { get; set; }

        public Guid FieldId { get; set; }

        public FieldModel Field { get; set; }

        public Guid? ParentFieldId { get; set; }

        public Guid DocTypeId { get; set; }

        public string ColumnName { get; set; }

        // Supported types include String, Integer, Decimal, Date
        public FieldDataType DataType { get; set; }

        public int DisplayOrder { get; set; }

        public int MaxLength { get; set; }

        public string DefaultValue { get; set; }

        public bool UseCurrentDate { get; set; }

        public bool IsRestricted { get; set; }

        public bool IsRequired { get; set; }

        public object Clone()
        {
            var newObj = (TableColumnModel)MemberwiseClone();
            if (Field != null)
            {
                newObj.Field = (FieldModel)Field.Clone();
            }
            return newObj;
        }
    }
}
