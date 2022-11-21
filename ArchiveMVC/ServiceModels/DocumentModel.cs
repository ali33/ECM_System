using System;
using System.Collections.Generic;

using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    public class DocumentModel
    {
        #region Private members

        private DateTime? _modifiedDate;

        private int _pageCount;

        #endregion

        public DocumentModel(DateTime createdDate, string createdBy, DocumentTypeModel documentType)
        {
            CreatedDate = createdDate;
            CreatedBy = createdBy;
            DocumentType = documentType;
            FieldValues = new List<FieldValueModel>();

            foreach (FieldMetaDataModel field in DocumentType.Fields)
            {
                FieldValueModel fieldValueModel = new FieldValueModel();

                fieldValueModel.Field = field;

                if (field.DataType == Ecm.Domain.FieldDataType.Date && field.UseCurrentDate)
                {
                    fieldValueModel.Value = DateTime.Now.ToShortDateString();
                }
                else
                {
                    fieldValueModel.Value = field.DefaultValue;
                }

                FieldValues.Add(fieldValueModel);
            }

            Pages = new List<PageModel>();
        }

        public DocumentModel()
        {
            Pages = new List<PageModel>();
            FieldValues = new List<FieldValueModel>();
        }

        public Guid DocVersionId { get; set; }

        public Guid Id { get; set; }

        public int PageCount
        {
            get { return _pageCount; }
            set
            {
                _pageCount = value;
            }
        }

        public int Version { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate
        {
            get { return _modifiedDate; }
            set
            {
                _modifiedDate = value;
            }
        }

        public string ModifiedBy { get; set; }

        public FileTypeModel BinaryType { get; set; }

        public DocumentTypeModel DocumentType { get; set; }

        public List<FieldValueModel> FieldValues { get; set; }

        public List<PageModel> Pages { get; set; }

        public List<Guid> DeletedPages { get; set; }

        public List<CommentModel> Comments { get; set; }

        public Dictionary<string, byte[]> EmbeddedPictures { get; set; }
    }
}
