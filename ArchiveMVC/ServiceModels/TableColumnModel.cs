using System;
using Ecm.Domain;

namespace ArchiveMVC.Models
{
    public class TableColumnModel
    {
        public Guid ColumnGuid { get; set; }

        public Guid FieldId { get; set; }

        public FieldMetaDataModel Field { get; set; }

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

        public TableColumnModel Clone()
        {
            return new TableColumnModel
            {
                ColumnGuid = ColumnGuid,
                FieldId = FieldId,
                ColumnName = ColumnName,
                DataType = DataType,
                DefaultValue = DefaultValue,
                MaxLength = MaxLength,
                UseCurrentDate = UseCurrentDate,
                DisplayOrder = DisplayOrder,
                IsRequired = IsRequired,
                IsRestricted = IsRestricted,
                ParentFieldId = ParentFieldId
            };
        }
    }
}
