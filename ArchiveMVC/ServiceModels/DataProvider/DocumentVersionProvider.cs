using System.Collections.Generic;
using System.Linq;
using Ecm.Domain;
using System;

namespace ArchiveMVC.Models.DataProvider
{
    public class DocumentVersionProvider : ProviderBase
    {
        public DocumentVersionProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// lấy DocumentVersion theo Id
        /// </summary>
        /// <param name="documentVersionId">Id DocumentVersion cần lấy</param>
        /// <returns></returns>
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



        /// <summary>
        /// lấy DocumentVersion bị xóa theo Id
        /// </summary>
        /// <param name="versionId">Id DocumentVersion đã bị xóa</param>
        /// <returns></returns>
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


        /// <summary>
        /// Lấy DocumentVersion bị xóa gần nhất theo Id
        /// </summary>
        /// <param name="docId">Id DocumentVersion bị xóa gần nhất</param>
        /// <returns></returns>
        public DocumentModel GetLatestDeleteDocumentVersion(Guid docId)
        {
            using (var client = GetArchiveClientChannel())
            {
                DocumentVersion docVersion = client.Channel.GetLatestDeletedDocumentVersion(docId);
                DocumentModel doc = ObjectMapper.GetDocumentByDocVersion(docVersion);
                return doc;
            }
        }
        



        /// <summary>
        /// Lấy DocumentVersions theo Id
        /// </summary>
        /// <param name="docId">Id DocumentVersions cần lấy</param>
        /// <returns></returns>
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



        /// <summary>
        /// Lấy DocWithExistingDocType bị xóa theo Id
        /// </summary>
        /// <param name="docTypeId">Id DocWithExistingDocType bị xóa</param>
        /// <returns></returns>
        public IList<DocumentModel> GetDeletedDocWithExistingDocType(Guid docTypeId)
        {
            using (var client = GetArchiveClientChannel())
            {
                List<DocumentVersion> docVersions = client.Channel.GetDeletedDocWithExistingDocType(docTypeId).ToList();
                List<DocumentModel> documents = ObjectMapper.GetDocumentByDocVersions(docVersions);
                return documents;
            }
        }



        /// <summary>
        /// lấy ChangeType theo changeAction
        /// </summary>
        /// <param name="changeAction">changeAction để lấy ChangeType</param>
        /// <returns></returns>
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


        /// <summary>
        /// Xóa Permission theo docType
        /// </summary>
        /// <param name="docType">docType cần xóa Permission</param>
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
