using System;
using System.Collections.Generic;

namespace ArchiveMVC.Models
{
    public class BatchModel
    {
        public BatchModel(Guid id, DateTime createdOn, string createdBy, BatchTypeModel batchType)
        {
            Id = id;
            BatchType = batchType;
            FieldValues = new List<FieldValueModel>();
            foreach (FieldMetaDataModel field in batchType.Fields)
            {
                FieldValues.Add(new FieldValueModel { Field = field, Value = field.DefaultValue });
            }

            CreatedOn = createdOn;
            CreatedBy = createdBy;
        }

        public BatchModel()
        {
            
        }

        public Guid Id { get; set; }

        public BatchTypeModel BatchType { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public List<FieldValueModel> FieldValues { get; set; }
    }
}
