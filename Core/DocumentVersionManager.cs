using System.Collections.Generic;
using System.Linq;
using Ecm.Domain;
using Ecm.DAO;
using System.Security;
using Ecm.DAO.Context;
using Ecm.SecurityDao;
using System;
using Ecm.Utility;

namespace Ecm.Core
{
    public class DocumentVersionManager : ManagerBase
    {
        private Setting _setting = new Setting();

        public DocumentVersionManager(User loginUser) : base(loginUser)
        {
            //_documentVersionDao = new DocumentVersionDao(DataContext);
            //_documentTypeVersionDao = new DocumentTypeVersionDao(DataContext);
            //_fieldValueVersionDao = new DocumentFieldVersionDao(DataContext);
            //_pageVersionDao = new PageVersionDao(DataContext);
            //_annotationVersionDao = new AnnotationVersionDao(DataContext);
            //_fieldMetaDataDao = new FieldMetaDataDao(DataContext);
            //_fieldVersionDao = new FieldMetaDataVersionDao(DataContext);
            //_docTypeDao = new DocTypeDao(DataContext);
            //_auditPermissionDao = new AuditPermissionDao(DataContext);
            //_auditPermissions = _auditPermissionDao.GetByUser(LoginUser.Id);
            _setting = new SettingManager(loginUser).GetSettings();
        }

        public DocumentVersion GetDocumentVersion(Guid docVersionId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                DocumentVersion docVersion = new DocumentVersionDao(dataContext).GetById(docVersionId);
                docVersion.DocumentTypeVersion = new DocumentTypeVersionDao(dataContext).GetById(docVersion.DocTypeId) ??
                                                 BaseVersion.GetDocumentTypeVersionFromDocumentType(new DocTypeDao(dataContext).GetById(docVersion.DocTypeId));

                docVersion.DocumentFieldVersions = new DocumentFieldVersionDao(dataContext).GetByDocumentVersion(docVersionId);
                docVersion.PageVersions = new PageVersionDao(dataContext).GetByDocVersion(docVersionId);

                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                foreach (var pageVersion in docVersion.PageVersions)
                {
                    if (_setting.IsSaveFileInFolder)
                    {
                        pageVersion.FileBinary = FileHelpper.ReadFile(pageVersion.FilePath, pageVersion.FileHeader);
                    }

                    pageVersion.AnnotationVersions = annotationVersionDao.GetByPageVersionId(pageVersion.Id);
                }

                FieldMetaDataVersionDao fieldMetadataVersion = new FieldMetaDataVersionDao(dataContext);
                FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                foreach (var fieldVersionValue in docVersion.DocumentFieldVersions)
                {
                    fieldVersionValue.FieldMetadataVersion = fieldMetadataVersion.GetById(fieldVersionValue.FieldId) ??
                                                             BaseVersion.GetFieldMetaDataVersionFromFieldMetaData(fieldMetaDataDao.GetById(fieldVersionValue.FieldId));
                }

                return docVersion;
            }
        }

        public DocumentVersion GetDeletedDocumentVersion(Guid docVersionId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                DocumentVersion documentVersion = new DocumentVersionDao(dataContext).GetById(docVersionId);
                DocumentTypeVersion docTypeVersion = new DocumentTypeVersionDao(dataContext).GetById(documentVersion.DocTypeId);
                documentVersion.DocumentFieldVersions = new DocumentFieldVersionDao(dataContext).GetByDocumentVersion(documentVersion.Id);
                documentVersion.PageVersions = new PageVersionDao(dataContext).GetByDocVersion(docVersionId);

                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                foreach (var pageVersion in documentVersion.PageVersions)
                {
                    if (_setting.IsSaveFileInFolder)
                    {
                        pageVersion.FileBinary = FileHelpper.ReadFile(pageVersion.FilePath, pageVersion.FileHeader);
                    }
                    pageVersion.AnnotationVersions = annotationVersionDao.GetByPageVersionId(pageVersion.Id);
                }

                if (docTypeVersion != null)
                {
                    FieldMetaDataVersionDao fieldMetadataVersionDao = new FieldMetaDataVersionDao(dataContext);
                    foreach (var fieldVersionValue in documentVersion.DocumentFieldVersions)
                    {
                        fieldVersionValue.FieldMetadataVersion = fieldMetadataVersionDao.GetById(fieldVersionValue.FieldId);
                    }
                }
                else
                {
                    FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                    foreach (var fieldVersionValue in documentVersion.DocumentFieldVersions)
                    {
                        FieldMetaData field = fieldMetaDataDao.GetById(fieldVersionValue.FieldId);
                        fieldVersionValue.FieldMetadataVersion = BaseVersion.GetFieldMetaDataVersionFromFieldMetaData(field);
                    }
                }

                return documentVersion;
            }
        }

        public List<DocumentVersion> GetDocumentVersionsByExistingDoc(Guid docId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                List<DocumentVersion> documentVersions = new DocumentVersionDao(dataContext).GetByDoc(docId);
                DocumentFieldVersionDao documentFieldVersionDao = new DocumentFieldVersionDao(dataContext);
                PageVersionDao pageVersionDao = new PageVersionDao(dataContext);
                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                FieldMetaDataVersionDao fieldMetadataVersionDao = new FieldMetaDataVersionDao(dataContext);

                foreach (var docVersion in documentVersions)
                {
                    docVersion.DocumentFieldVersions = documentFieldVersionDao.GetByDocumentVersion(docVersion.Id);
                    docVersion.PageVersions = pageVersionDao.GetByDocVersion(docVersion.Id);

                    foreach (var pageVersion in docVersion.PageVersions)
                    {
                        if (_setting.IsSaveFileInFolder)
                        {
                            pageVersion.FileBinary = FileHelpper.ReadFile(pageVersion.FilePath, pageVersion.FileHeader);
                        }
                        pageVersion.AnnotationVersions = annotationVersionDao.GetByPageVersionId(pageVersion.Id);
                    }

                    foreach (var fieldVersionValue in docVersion.DocumentFieldVersions)
                    {
                        fieldVersionValue.FieldMetadataVersion = fieldMetadataVersionDao.GetById(fieldVersionValue.FieldId);
                    }
                }

                return documentVersions;
            }

        }

        public List<DocumentVersion> GetDocumentVersionsByDeletedDocType(Guid docTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);
                var changeActionDeletDocType = (int)ChangeAction.DeleteDocumentType;
                List<DocumentVersion> documentVersions =
                    new DocumentVersionDao(dataContext).GetByDocType(docTypeId)
                                                       .Where(d => d.DocTypeVersionId != null
                                                                   && d.ChangeAction == changeActionDeletDocType)
                                                       .ToList();
                DocumentFieldVersionDao documentFieldVersionDao = new DocumentFieldVersionDao(dataContext);
                PageVersionDao pageVersionDao = new PageVersionDao(dataContext);
                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                FieldMetaDataVersionDao fieldMetadataVersionDao = new FieldMetaDataVersionDao(dataContext);
                foreach (var documentVersion in documentVersions)
                {
                    documentVersion.DocumentFieldVersions = documentFieldVersionDao.GetByDocumentVersion(documentVersion.Id);
                    documentVersion.PageVersions = pageVersionDao.GetByDocVersion(documentVersion.Id);

                    foreach (var pageVersion in documentVersion.PageVersions)
                    {
                        if (_setting.IsSaveFileInFolder)
                        {
                            pageVersion.FileBinary = FileHelpper.ReadFile(pageVersion.FilePath, pageVersion.FileHeader);
                        }
                        pageVersion.AnnotationVersions = annotationVersionDao.GetByPageVersionId(pageVersion.Id);
                    }

                    foreach (var fieldValueVersion in documentVersion.DocumentFieldVersions)
                    {
                        fieldValueVersion.FieldMetadataVersion = fieldMetadataVersionDao.GetById(fieldValueVersion.FieldId);
                    }
                }

                return documentVersions;
            }
        }

        public DocumentVersion GetLatestDeleteDocumentVersion(Guid docId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                DocumentVersion documentVersion = new DocumentVersionDao(dataContext).GetByDoc(docId).First(d => d.DocTypeVersionId != null);
                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                FieldMetaDataVersionDao fieldMetadataVersionDao = new FieldMetaDataVersionDao(dataContext);

                DocumentTypeVersion docTypeVersion = new DocumentTypeVersionDao(dataContext).GetById(documentVersion.DocTypeId);
                documentVersion.PageVersions = new PageVersionDao(dataContext).GetByDocVersion(documentVersion.Id);

                foreach (var pageVersion in documentVersion.PageVersions)
                {
                    if (_setting.IsSaveFileInFolder)
                    {
                        pageVersion.FileBinary = FileHelpper.ReadFile(pageVersion.FilePath, pageVersion.FileHeader);
                    }
                    pageVersion.AnnotationVersions = annotationVersionDao.GetByPageVersionId(pageVersion.Id);
                }

                documentVersion.DocumentTypeVersion = docTypeVersion;

                foreach (var fieldValue in documentVersion.DocumentFieldVersions)
                {
                    fieldValue.FieldMetadataVersion = fieldMetadataVersionDao.GetById(fieldValue.FieldId);
                }

                return documentVersion;
            }
        }

        public List<DocumentVersion> GetDeletedDocWithExistingDocType(Guid docTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                List<DocumentVersion> documentVersions = new DocumentVersionDao(dataContext).GetDeletedDocWithExistingDocType(docTypeId);

                DocumentFieldVersionDao documentFieldVersionDao = new DocumentFieldVersionDao(dataContext);
                PageVersionDao pageVersionDao = new PageVersionDao(dataContext);
                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                FieldMetaDataVersionDao fieldMetadataVersionDao = new FieldMetaDataVersionDao(dataContext);
                FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                foreach (var documentVersion in documentVersions)
                {
                    documentVersion.DocumentFieldVersions = documentFieldVersionDao.GetByDocumentVersion(documentVersion.Id);
                    documentVersion.PageVersions = pageVersionDao.GetByDocVersion(documentVersion.Id);

                    foreach (var pageVersion in documentVersion.PageVersions)
                    {
                        if (_setting.IsSaveFileInFolder)
                        {
                            pageVersion.FileBinary = FileHelpper.ReadFile(pageVersion.FilePath, pageVersion.FileHeader);
                        }
                        pageVersion.AnnotationVersions = annotationVersionDao.GetByPageVersionId(pageVersion.Id);
                    }

                    foreach (var fieldVersion in documentVersion.DocumentFieldVersions)
                    {
                        fieldVersion.FieldMetadataVersion = BaseVersion.GetFieldMetaDataVersionFromFieldMetaData(fieldMetaDataDao.GetById(fieldVersion.FieldId));
                    }
                }
                return documentVersions;
            }
        }

        public void AddDocumentVersion(Document document, ChangeAction changeAction)
        {
            DocumentVersion documentVersion = BaseVersion.GetDocumentVersionFromDocument(document, changeAction);
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                new DocumentVersionDao(dataContext).Add(documentVersion);
                DocumentFieldVersionDao documentFieldVersionDao = new DocumentFieldVersionDao(dataContext);
                PageVersionDao pageVersionDao = new PageVersionDao(dataContext);
                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                foreach (var page in document.Pages)
                {
                    PageVersion pageVersion = BaseVersion.GetPageVersionFromPage(page);
                    pageVersion.DocVersionId = documentVersion.Id;
                    pageVersionDao.Add(pageVersion);

                    foreach (var annotation in page.Annotations)
                    {
                        AnnotationVersion annotationVersion = BaseVersion.GetAnnotationVersionFromAnnotation(annotation);
                        annotationVersion.PageVersionId = pageVersion.Id;
                        annotationVersionDao.Add(annotationVersion);
                    }
                }

                foreach (var fieldValue in document.FieldValues)
                {
                    DocumentFieldVersion fieldValueVersion = BaseVersion.GetDocumentFieldVersionFromFieldValue(fieldValue);
                    fieldValueVersion.DocVersionID = documentVersion.Id;
                    documentFieldVersionDao.Add(fieldValueVersion);
                }
            }
        }

        private void CheckAuditPermission(DapperContext dataContext)
        {
            if (!LoginUser.IsAdmin)
            {
                List<Guid> groupIds = null;
                using (Context.DapperContext primaryContent = new Context.DapperContext())
                {
                    UserGroupDao userGroupDao = new UserGroupDao(primaryContent);
                    groupIds = userGroupDao.GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                }
                List<AuditPermission> auditPermissions = new AuditPermissionDao(dataContext).GetByUser(groupIds);
                if (!auditPermissions.Any(p => p.AllowedAudit))
                {
                    throw new SecurityException(string.Format("User {0} doesn't have permission to access audit system.", LoginUser.UserName));
                }
            }
        }
    }
}