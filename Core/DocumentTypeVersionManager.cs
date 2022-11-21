using System;
using System.Collections.Generic;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.Domain;
using System.Linq;
using System.Security;
using Ecm.SecurityDao;

namespace Ecm.Core
{
    public class DocumentTypeVersionManager : ManagerBase
    {
        public DocumentTypeVersionManager(User loginUser) : base (loginUser)
        {
        }

        public List<DocumentTypeVersion> GetDocumentTypeVersions()
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                List<DocumentTypeVersion> docTypeVersions = new DocumentTypeVersionDao(dataContext).GetAll();
                FieldMetaDataVersionDao fieldMetaDataVersionDao = new FieldMetaDataVersionDao(dataContext);
                foreach (DocumentTypeVersion docTypeVersion in docTypeVersions)
                {
                    docTypeVersion.FieldMetaDataVersions = fieldMetaDataVersionDao.GetByDeletedDocType(docTypeVersion.Id);
                }

                return docTypeVersions;
            }
        }

        public DocumentTypeVersion GetDocumentTypeVersion(Guid docTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                DocumentTypeVersion docTypeVersion = new DocumentTypeVersionDao(dataContext).GetById(docTypeId);

                if (docTypeVersion == null)
                {
                    docTypeVersion = BaseVersion.GetDocumentTypeVersionFromDocumentType(new DocTypeDao(dataContext).GetById(docTypeId));
                }

                docTypeVersion.FieldMetaDataVersions = new FieldMetaDataVersionDao(dataContext).GetByDeletedDocType(docTypeId);
                return docTypeVersion;
            }
        }

        public void AddDocumentTypeVersion(DocumentType documentType)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckAuditPermission(dataContext);

                DocumentTypeVersion documentTypeVersion = new DocumentTypeVersion
                {
                    CreatedBy = documentType.CreatedBy,
                    CreatedDate = documentType.CreatedDate,
                    ModifiedBy = documentType.ModifiedBy,
                    ModifiedDate = documentType.ModifiedDate,
                    Name = documentType.Name
                };

                dataContext.BeginTransaction();

                try
                { 
                    new DocumentTypeVersionDao(dataContext).Add(documentTypeVersion);
                    FieldMetaDataVersionDao fieldMetaDataVersionDao = new FieldMetaDataVersionDao(dataContext);
                    foreach (var field in documentType.FieldMetaDatas)
                    {
                        FieldMetadataVersion fieldVersion = BaseVersion.GetFieldMetaDataVersionFromFieldMetaData(field);
                        fieldVersion.DocTypeId = documentType.Id;
                        fieldMetaDataVersionDao.Add(fieldVersion);
                    }

                    var documents = new DocumentDao(dataContext).GetByDocType(documentType.Id);

                    DocumentVersionDao documentVersionDao = new DocumentVersionDao(dataContext);
                    DocumentFieldVersionDao documentValueVersionDao = new DocumentFieldVersionDao(dataContext);
                    PageVersionDao pageVersionDao = new PageVersionDao(dataContext);
                    AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);

                    foreach (var doc in documents)
                    {
                        DocumentVersion documentVersion = BaseVersion.GetDocumentVersionFromDocument(doc, ChangeAction.DeleteDocumentType);
                        documentVersion.DocTypeVersionId = documentType.Id;
                        documentVersionDao.Add(documentVersion);

                        foreach (var fieldValue in doc.FieldValues)
                        {
                            DocumentFieldVersion fieldValueVersion = BaseVersion.GetDocumentFieldVersionFromFieldValue(fieldValue);
                            fieldValueVersion.DocVersionID = documentVersion.Id;
                            documentValueVersionDao.Add(fieldValueVersion);
                        }

                        foreach (var page in doc.Pages)
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
                    }

                    dataContext.Commit();
                }
                catch (Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        private void CheckAuditPermission(DapperContext dataContext)
        {
            if (!LoginUser.IsAdmin)
            {

                List<Guid> groupIds = null;

                using (Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                }

                List<AuditPermission> auditPermissions = new AuditPermissionDao(dataContext).GetByUser(groupIds);

                if (!auditPermissions.Any(p => p.AllowedAudit) && !LoginUser.IsAdmin)
                {
                    throw new SecurityException(string.Format("User {0} doesn't have permission to access audit system.", LoginUser.UserName));
                }
            }
        }
    }
}