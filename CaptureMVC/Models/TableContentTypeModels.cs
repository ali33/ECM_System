using System;
using System.Collections.Generic;
using Ecm.CaptureDomain;

namespace CaptureMVC.Models
{
    public class TableContentTypeModels
    {
        public Guid ColumnGuid { get; set; }

        public Guid FieldId { get; set; }

        public DocumentFieldMetaData Field { get; set; }

        public Guid? ParentFieldId { get; set; }

        public Guid DocTypeId { get; set; }

        public string ColumnName { get; set; }

        public FieldDataType DataType { get; set; }

        public int DisplayOrder { get; set; }

        public int MaxLength { get; set; }

        public string DefaultValue { get; set; }

        public bool UseCurrentDate { get; set; }

        public bool IsRestricted { get; set; }

        public bool IsRequired { get; set; }
        public bool HasLoopUp { get; set; }

        public string ValidaScript { get; set; }

        public string ValidaPattern { get; set; }

        public string PickList { get; set; }


        public TableContentTypeModels Clone()
        {
            return new TableContentTypeModels
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
                ParentFieldId = ParentFieldId,
                ValidaScript = ValidaScript,
                ValidaPattern = ValidaPattern,
                PickList = PickList,
                HasLoopUp = HasLoopUp
            };
        }
    }
}