using Ecm.Domain;
using System.Collections.Generic;

namespace Ecm.Core
{
    public class BaseVersion
    {
        public static FieldMetadataVersion GetFieldMetaDataVersionFromFieldMetaData(FieldMetaData field)
        {
            return new FieldMetadataVersion
            {
                Id = field.Id,
                DataType = field.DataType,
                DefautValue = field.DefautValue,
                DisplayOrder = field.DisplayOrder,
                DocTypeId = field.DocTypeId,
                //Guid FieldUniqueId = field.FieldUniqueId,
                IsLookup = field.IsLookup,
                IsRequired = field.IsRequired,
                IsRestricted = field.IsRestricted,
                IsSystemField = field.IsSystemField,
                LookupInfo = field.LookupInfo,
                LookupMaps = field.LookupMaps,
                MaxLength = field.MaxLength,
                Name = field.Name,
                ParentFieldID = field.ParentFieldId,
                Picklists = field.Picklists
            };
        }

        public static DocumentFieldVersion GetDocumentFieldVersionFromFieldValue(FieldValue fieldValue)
        {
            return new DocumentFieldVersion
            {
                DocId = fieldValue.DocId,
                FieldId = fieldValue.FieldMetaData.Id,
                FieldMetadataVersion = GetFieldMetaDataVersionFromFieldMetaData(fieldValue.FieldMetaData),
                Value = fieldValue.Value
            };
        }

        public static DocumentVersion GetDocumentVersionFromDocument(Document document, ChangeAction changeAction)
        {
            return new DocumentVersion
                {
                    ChangeAction = (int)changeAction,
                    CreatedBy = document.CreatedBy,
                    CreatedDate = document.CreatedDate,
                    DocId = document.Id,
                    DocTypeId = document.DocTypeId,
                    Version = document.Version,
                    ModifiedBy = document.ModifiedBy,
                    ModifiedDate = document.ModifiedDate,
                    PageCount = document.PageCount,
                    BinaryType = document.BinaryType
                };
        }

        public static PageVersion GetPageVersionFromPage(Page page)
        {
            return new PageVersion
            {
                DocId = page.DocId,
                FileBinary = page.FileBinary,
                FilePath = page.FilePath,
                FileHeader = page.FileHeader,
                FileExtension = page.FileExtension,
                FileHash = page.FileHash,
                Height = page.Height,
                PageId = page.Id,
                PageNumber = page.PageNumber,
                RotateAngle = page.RotateAngle,
                Width = page.Width
            };
        }

        public static AnnotationVersion GetAnnotationVersionFromAnnotation(Annotation annotation)
        {
            return new AnnotationVersion
            {
                AnnotationId = annotation.Id,
                Content = annotation.Content,
                CreatedBy = annotation.CreatedBy,
                CreatedOn = annotation.CreatedOn,
                Height = annotation.Height,
                Left = annotation.Left,
                LineColor = annotation.LineColor,
                LineEndAt = annotation.LineEndAt,
                LineStartAt = annotation.LineStartAt,
                LineStyle = annotation.LineStyle,
                LineWeight = annotation.LineWeight,
                ModifiedBy = annotation.ModifiedBy,
                ModifiedOn = annotation.ModifiedOn,
                PageId = annotation.PageId,
                RotateAngle = annotation.RotateAngle,
                Top = annotation.Top,
                Type = annotation.Type,
                Width = annotation.Width
            };
        }

        public static DocumentTypeVersion GetDocumentTypeVersionFromDocumentType(DocumentType docType)
        {
            DocumentTypeVersion docTypeVersion = new DocumentTypeVersion
            {
                CreatedBy = docType.CreatedBy,
                CreatedDate = docType.CreatedDate,
                Id = docType.Id,
                IsOutlook = docType.IsOutlook,
                ModifiedBy = docType.ModifiedBy,
                ModifiedDate = docType.ModifiedDate,
                Name = docType.Name
            };

            List<FieldMetadataVersion> fieldMetaDataVersions = new List<FieldMetadataVersion>();

            foreach (FieldMetaData field in docType.FieldMetaDatas)
            {
                fieldMetaDataVersions.Add(GetFieldMetaDataVersionFromFieldMetaData(field));
            }

            docTypeVersion.FieldMetaDataVersions = fieldMetaDataVersions;

            return docTypeVersion;

        }
    }
}