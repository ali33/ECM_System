using System.Collections.Generic;
using System.Linq;
using Ecm.Domain;
using System;

namespace Ecm.Model.DataProvider
{
    public class DocumentVersionProvider : ProviderBase
    {
        public DocumentModel GetDocumentVersion(Guid documentVersionId)
        {
            using (var client = GetArchiveClientChannel())
            {
                DocumentVersion documentVersion = client.Channel.GetDocumentVersion(documentVersionId);
                DocumentTypeVersion docTypeVersion = client.Channel.GetDocumentTypeVersion(documentVersion.DocTypeId);
                DocumentTypeModel docType = ObjectMapper.GetDocumentTypeByDocumentTypeVersion(docTypeVersion);

                RemovePermission(docType);
                if (docTypeVersion != null)
                {
                    documentVersion.DocumentTypeVersion = docTypeVersion;
                }

                DocumentModel document;
                if (documentVersion.DocumentTypeVersion == null)
                {
                    document = ObjectMapper.GetDocumentByDocVersion(documentVersion, docType);
                }
                else
                {
                    document = ObjectMapper.GetDocumentByDocVersion(documentVersion);
                }

                return document;
            }
        }

        public DocumentModel GetDeletedDocumentVersion(Guid versionId)
        {
            using (var client = GetArchiveClientChannel())
            {
                DocumentVersion documentVersion = client.Channel.GetVersionOfDeletedDocument(versionId);
                DocumentTypeVersion docTypeVersion = client.Channel.GetDocumentTypeVersion(documentVersion.DocTypeId);
                DocumentTypeModel docType = ObjectMapper.GetDocumentTypeByDocumentTypeVersion(docTypeVersion);

                RemovePermission(docType);
                if (docTypeVersion != null)
                {
                    documentVersion.DocumentTypeVersion = docTypeVersion;
                }

                DocumentModel document;
                if (documentVersion.DocumentTypeVersion == null)
                {
                    document = ObjectMapper.GetDocumentByDocVersion(documentVersion, docType);
                }
                else
                {
                    document = ObjectMapper.GetDocumentByDocVersion(documentVersion);
                }

                return document;
            }
        }

        public DocumentModel GetLatestDeleteDocumentVersion(Guid docId)
        {
            using (var client = GetArchiveClientChannel())
            {
                DocumentVersion docVersion = client.Channel.GetLatestDeletedDocumentVersion(docId);
                DocumentModel doc = ObjectMapper.GetDocumentByDocVersion(docVersion);
                return doc;
            }
        }

        public List<DocumentVersionModel> GetDocumentVersions(Guid docId)
        {
            using (var client = GetArchiveClientChannel())
            {
                List<DocumentVersion> documentVersions = client.Channel.GetDocumentVersionsByExistingDoc(docId).ToList();
                List<DocumentVersionModel> documentVersionModels = new List<DocumentVersionModel>();

                foreach (var docVer in documentVersions)
                {
                    documentVersionModels.Add(new DocumentVersionModel { DocId = docVer.DocId, DocTypeId = docVer.DocTypeId, VersionId = docVer.Id, Version = docVer.Version, ChangeType = GetChangeType((ChangeAction)docVer.ChangeAction) });
                }

                return documentVersionModels;
            }
        }

        public IList<DocumentModel> GetDeletedDocWithExistingDocType(Guid docTypeId)
        {
            using (var client = GetArchiveClientChannel())
            {
                List<DocumentVersion> docVersions = client.Channel.GetDeletedDocWithExistingDocType(docTypeId).ToList();
                List<DocumentModel> documents = ObjectMapper.GetDocumentByDocVersions(docVersions);
                return documents;
            }
        }

        private string GetChangeType(ChangeAction changeAction)
        {
            switch (changeAction)
            {
                case ChangeAction.DeleteDocumentType:
                    return Common.DELETE_DOCUMENT_TYPE;
                case ChangeAction.DeleteDocument:
                    return Common.DELETE_DOCUMENT;
                case ChangeAction.AppendPage:
                    return Common.APPEND_PAGE;
                case ChangeAction.ReplacePage:
                    return Common.REPLACE_PAGE;
                case ChangeAction.DeletePage:
                    return Common.DELETE_PAGE;
                case ChangeAction.UpdateFieldValue:
                    return Common.UPDATE_FIELD_VALUE;
                case ChangeAction.RotatePage:
                    return Common.ROTATE_PAGE;
                case ChangeAction.AddAnnotation:
                    return Common.ADD_ANNOTATION;
                case ChangeAction.UpdateAnnotation:
                    return Common.UPDATE_ANNOTATION;
                case ChangeAction.DeleteAnnotation:
                    return Common.DELETE_ANNOTATION;
                default:
                    return null;
            }
        }

        private void RemovePermission(DocumentTypeModel docType)
        {
            if (docType != null && docType.DocumentTypePermission != null)
            {
                docType.DocumentTypePermission.AllowedAppendPage = false;
                docType.DocumentTypePermission.AllowedCapture = false;
                docType.DocumentTypePermission.AllowedChangeDocumentType = false;
                docType.DocumentTypePermission.AllowedDeletePage = false;
                docType.DocumentTypePermission.AllowedDownloadOffline = false;
                docType.DocumentTypePermission.AllowedHideAllAnnotation = false;
                docType.DocumentTypePermission.AllowedReOrderPage = false;
                docType.DocumentTypePermission.AllowedReplacePage = false;
                docType.DocumentTypePermission.AllowedRotatePage = false;
                docType.DocumentTypePermission.AllowedSearch = false;
                docType.DocumentTypePermission.AllowedSplitDocument = false;
                docType.DocumentTypePermission.AllowedUpdateFieldValue = false;
            }

            if (docType != null && docType.AnnotationPermission != null)
            {
                docType.AnnotationPermission.AllowedAddHighlight = false;
                docType.AnnotationPermission.AllowedAddRedaction = false;
                docType.AnnotationPermission.AllowedAddText = false;
                docType.AnnotationPermission.AllowedDeleteHighlight = false;
                docType.AnnotationPermission.AllowedDeleteRedaction = false;
                docType.AnnotationPermission.AllowedDeleteText = false;
                docType.AnnotationPermission.AllowedHideRedaction = false;
                docType.AnnotationPermission.AllowedSeeHighlight = false;
                docType.AnnotationPermission.AllowedSeeText = false;
            }
        }
    }
}
