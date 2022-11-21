using System.Collections.Generic;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;
using System.Linq;
using Ecm.SecurityDao;

namespace Ecm.CaptureCore
{
    public class DocumentTypeManager : ManagerBase
    {
        public DocumentTypeManager(User loginUser) : base(loginUser)
        {
        }

        public void SaveOCRTemplate(OCRTemplate ocrTemplate)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save OCR template.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
                OCRTemplatePageDao ocrTemplatePageDao = new OCRTemplatePageDao(dataContext);
                OCRTemplateDao ocrTemplateDao = new OCRTemplateDao(dataContext);

                dataContext.BeginTransaction();
                try
                {
                    // Delete the old one
                    ocrTemplateZoneDao.DeleteByDocType(ocrTemplate.DocTypeId);
                    ocrTemplatePageDao.DeleteByDocType(ocrTemplate.DocTypeId);
                    ocrTemplateDao.Delete(ocrTemplate.DocTypeId);

                    // Insert new one
                    ocrTemplateDao.Add(ocrTemplate);
                    foreach (var ocrTemplatePage in ocrTemplate.OCRTemplatePages)
                    {
                        ocrTemplatePage.OCRTemplateId = ocrTemplate.DocTypeId;
                        ocrTemplatePageDao.Add(ocrTemplatePage);
                        foreach (var ocrTemplateZone in ocrTemplatePage.OCRTemplateZones)
                        {
                            ocrTemplateZone.OCRTemplatePageId = ocrTemplatePage.Id;
                            ocrTemplateZoneDao.Add(ocrTemplateZone);
                        }
                    }

                    ActionLogHelper.AddActionLog("Document type id '" + ocrTemplate.DocTypeId + "' add OCR template successfully", LoginUser, ActionName.AddOCRTemplate, null, null, dataContext);
                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public void DeleteOCRTemplate(Guid documentTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to delete OCR template.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
                OCRTemplatePageDao ocrTemplatePageDao = new OCRTemplatePageDao(dataContext);
                OCRTemplateDao ocrTemplateDao = new OCRTemplateDao(dataContext);

                dataContext.BeginTransaction();
                try
                {
                    ocrTemplateZoneDao.DeleteByDocType(documentTypeId);
                    ocrTemplatePageDao.DeleteByDocType(documentTypeId);
                    ocrTemplateDao.Delete(documentTypeId);
                    ActionLogHelper.AddActionLog("Delete OCR template of document type '" + documentTypeId + "'", LoginUser, ActionName.DeleteOCRTemplate, null, null, dataContext);
                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public DocumentType GetDocumentType(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                DocFieldMetaDataDao fieldDao = new DocFieldMetaDataDao(dataContext);

                DocumentType docType = docTypeDao.GetById(id);

                if (docType != null)
                {
                    docType.Fields = fieldDao.GetByDocType(id);
                }

                return docType;
            }
        }

        public List<DocumentType> GetDocumentTypes(Guid batchTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                DocFieldMetaDataDao fieldDao = new DocFieldMetaDataDao(dataContext);

                List<DocumentType> docTypes = docTypeDao.GetDocumentTypeByBatch(batchTypeId);

                foreach(DocumentType docType in docTypes)
                {
                    docType.Fields = fieldDao.GetByDocType(docType.Id);
                }

                return docTypes;
            }
        }

        public List<Guid> GetCanAccessDocumentTypeIds()
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                List<Guid> userGroupIds;
                
                using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                }

                List<Guid> docTypes = docTypeDao.GetCanAccessDocumentTypeIds(userGroupIds);

                return docTypes;
            }
        }

        /// <summary>
        /// Check list document type whether have document or not.
        /// </summary>
        /// <param name="docTypeIds">List document type id need to check</param>
        /// <returns>List of document type id which have document</returns>
        public List<Guid> CheckDocTypeHaveDocument(List<Guid> docTypeIds)
        {
            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                return new DocumentDao(context).CheckDocTypeHaveDocument(docTypeIds);
            }
        }
    }
}
