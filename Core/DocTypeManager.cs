using System.Collections.Generic;
using System.Security;
using System.Linq;
using Ecm.Domain;
using System;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.SecurityDao;
using System.IO;
using Ecm.Utility;

namespace Ecm.Core
{
    public class DocTypeManager : ManagerBase
    {
        private Setting _setting = new Setting();
        private const string ARCHIVE_VERSION_FOLDER = "ARCHIVE_VERSION";
        private const string ARCHIVE_FOLDER = "ARCHIVE";

        public DocTypeManager(User loginUser) : base(loginUser)
        {
            _setting = new SettingManager(loginUser).GetSettings();
        }

        public List<DocumentType> GetDocumentTypes()
        {
            List<DocumentType> documentTypes;

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocTypeDao docTypeDao = new DocTypeDao(dataContext);
                if (LoginUser.IsAdmin)
                {
                    documentTypes = docTypeDao.GetAll();
                    FillElementToDocumentTypes(documentTypes, dataContext);
                    foreach (var documentType in documentTypes)
                    {
                        documentType.AnnotationPermission = AnnotationPermission.GetAllowAll();
                        documentType.DocumentTypePermission = DocumentTypePermission.GetAllowAll();
                    }
                }
                else
                {
                    List<Guid> groupIds = null;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    documentTypes = docTypeDao.GetByUser(groupIds);
                    FillElementToDocumentTypes(documentTypes, dataContext);
                }

                ActionLogHelper.AddActionLog("Get document types successfully.", LoginUser, ActionName.GetDocumentType,
                                             null, null, dataContext);
            }
            return documentTypes;
        }

        public List<DocumentType> GetCapturedDocumentTypes()
        {
            if (LoginUser.IsAdmin)
            {
                //return GetDocumentTypes().Where(p => p.WorkflowDefinitionID != Guid.Empty).ToList();
                return GetDocumentTypes();
            }
            else
            {
                using (DapperContext dataContext = new DapperContext(LoginUser))
                {
                    //List<DocumentType> documentTypes = new DocTypeDao(dataContext).GetCaptureDocumentTypeByUser(LoginUser.Id)
                    //                                       .Where(p => p.WorkflowDefinitionID != Guid.Empty).ToList();
                    List<Guid> groupIds = null;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    List<DocumentType> documentTypes = new DocTypeDao(dataContext).GetCaptureDocumentTypeByUser(groupIds);

                    FillElementToDocumentTypes(documentTypes, dataContext);

                    ActionLogHelper.AddActionLog("Get captured document types successfully.", LoginUser,
                                             ActionName.GetDocumentType, null, null, dataContext);
                    return documentTypes;
                }
            }
        }

        public DocumentType GetCapturedDocumentType(Guid id)
        {
            DocumentType documentType;

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (LoginUser.IsAdmin)
                {
                    documentType = new DocTypeDao(dataContext).GetById(id);

                    if (documentType != null)
                    {
                        documentType.AnnotationPermission = AnnotationPermission.GetAllowAll();
                        documentType.DocumentTypePermission = DocumentTypePermission.GetAllowAll();
                    }
                }
                else
                {
                    List<Guid> groupIds = null;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    documentType = new DocTypeDao(dataContext).GetCaptureDocumentTypeByUser(id, groupIds);
                }

                //if (documentType != null && documentType.WorkflowDefinitionID != Guid.Empty)
                if (documentType != null)
                {
                    FillElementToDocumentType(documentType, dataContext);
                }
                ActionLogHelper.AddActionLog("Get document type successfully", LoginUser, ActionName.GetDocumentType,
                                                 null, null, dataContext);
            }

            return documentType;
        }

        public DocumentType GetDocumentType(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentType documentType;
                if (LoginUser.IsAdmin)
                {
                    documentType = new DocTypeDao(dataContext).GetById(id);
                    if (documentType != null)
                    {
                        documentType.AnnotationPermission = AnnotationPermission.GetAllowAll();
                        documentType.DocumentTypePermission = DocumentTypePermission.GetAllowAll();
                    }
                }
                else
                {
                    List<Guid> groupIds = null;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    documentType = new DocTypeDao(dataContext).GetByAllowedSearch(id, groupIds);
                }

                if (documentType != null)
                {
                    FillElementToDocumentType(documentType, dataContext);
                }

                ActionLogHelper.AddActionLog("Get document type successfully", LoginUser, ActionName.GetDocumentType,
                                             null, null, dataContext);
                return documentType;
            }
        }

        public Guid Save(DocumentType documentType)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save document type.", LoginUser.UserName));
            }

            if (documentType.Id == Guid.Empty)
            {
                AddSystemFields(documentType);
                documentType.CreatedDate = DateTime.Now;
                documentType.CreatedBy = LoginUser.UserName;
                //documentType.UniqueId = GenerateUniqueId();
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                PicklistDao picklistDao = new PicklistDao(dataContext);
                FieldMetaDataDao fieldMetadataDao = new FieldMetaDataDao(dataContext);
                DocTypeDao docTypeDao = new DocTypeDao(dataContext);
                dataContext.BeginTransaction();

                try
                {
                    if (documentType.Id == Guid.Empty)
                    {
                        AddDocType(documentType, dataContext, docTypeDao, fieldMetadataDao, picklistDao);
                    }
                    else
                    {
                        UpdateDocType(documentType, dataContext, docTypeDao, fieldMetadataDao, picklistDao);
                    }

                    dataContext.Commit();
                    return documentType.Id;
                }
                catch (Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public void Delete(DocumentType documentType)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to delete document type.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentTypeVersionDao documentTypeVersionDao = new DocumentTypeVersionDao(dataContext);
                FieldMetaDataVersionDao fieldMetadataFieldVersionDao = new FieldMetaDataVersionDao(dataContext);
                DocumentVersionDao documentVersionDao = new DocumentVersionDao(dataContext);
                PageVersionDao pageVersionDao = new PageVersionDao(dataContext);
                AnnotationVersionDao annotationVersionDao = new AnnotationVersionDao(dataContext);
                DocumentFieldVersionDao fieldValueVersionDao = new DocumentFieldVersionDao(dataContext);

                DocTypeDao docTypeDao = new DocTypeDao(dataContext);
                DocumentDao documentDao = new DocumentDao(dataContext);
                PageDao pageDao = new PageDao(dataContext);
                FieldMetaDataDao fieldMetadataDao = new FieldMetaDataDao(dataContext);
                FieldValueDao fieldValueDao = new FieldValueDao(dataContext);
                AnnotationDao annotationDao = new AnnotationDao(dataContext);
                //LookupInfoDao lookupInfoDao = new LookupInfoDao(dataContext);
                //LookupMapDao lookupMapDao = new LookupMapDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);
                OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
                OCRTemplatePageDao ocrTemplatePageDao = new OCRTemplatePageDao(dataContext);
                OCRTemplateDao ocrTemplateDao = new OCRTemplateDao(dataContext);
                BarcodeConfigurationDao barcodeConfigurationDao = new BarcodeConfigurationDao(dataContext);
                SearchQueryExpressionDao searchQueryExpressionDao = new SearchQueryExpressionDao(dataContext);
                SearchQueryDao searchQueryDao = new SearchQueryDao(dataContext);
                FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                AnnotationPermissionDao annotationPermissionDao = new AnnotationPermissionDao(dataContext);
                DocumentTypePermissionDao documentTypePermissionDao = new DocumentTypePermissionDao(dataContext);

                dataContext.BeginTransaction();

                try
                {
                    #region versioning

                    //Add version
                    DocumentType oldDocumentType = docTypeDao.GetById(documentType.Id);
                    List<Document> oldDocuments = documentDao.GetByDocType(documentType.Id);
                    List<FieldMetaData> oldFieldMetadata = fieldMetadataDao.GetByDocType(documentType.Id);
                    DocumentTypeVersion documentTypeVersion = BaseVersion.GetDocumentTypeVersionFromDocumentType(oldDocumentType);

                    documentTypeVersionDao.Add(documentTypeVersion);

                    #region deleted document (which is deleted before delete document type)

                    // Get deleted document (which is deleted before delete document type)
                    var beforeDelDocs = documentVersionDao.GetByDocType(documentType.Id).Where(h => h.DocTypeVersionId == null).ToList();
                    // Get deleted field value of deleted document
                    var beforeDelFieldValues = new List<DocumentFieldVersion>();
                    foreach (var item in beforeDelDocs)
                    {
                        var listItem = fieldValueVersionDao.GetByDocument(item.DocId);
                        beforeDelFieldValues.AddRange(listItem);
                    }

                    // Update DocTypeVersionId of deleted document (which is deleted before delete document type)
                    documentVersionDao.UpdateDocumentTypeVersionIdAndChangeAction(documentType.Id, documentTypeVersion.Id, (int)ChangeAction.DeleteDocumentType);

                    #endregion

                    var mappingFieldMetaDataToVersion = new Dictionary<Guid, Guid>();

                    foreach (FieldMetaData field in oldFieldMetadata)
                    {
                        var fieldMetaVersion = BaseVersion.GetFieldMetaDataVersionFromFieldMetaData(field);
                        fieldMetadataFieldVersionDao.Add(fieldMetaVersion);

                        if (!mappingFieldMetaDataToVersion.ContainsKey(field.Id))
                        {
                            mappingFieldMetaDataToVersion.Add(field.Id, fieldMetaVersion.Id);
                        }
                    }

                    #region Update field id of deleted field value version (of deleted document which is deleted before delete document type)
                    foreach (var item in beforeDelFieldValues)
                    {
                        if (!mappingFieldMetaDataToVersion.ContainsKey(item.FieldId))
                        {
                            continue;
                        }
                        item.FieldId = mappingFieldMetaDataToVersion[item.FieldId];
                        fieldValueVersionDao.UpdateFieldId(item);
                    }
                    #endregion

                    foreach (Document document in oldDocuments)
                    {
                        DocumentVersion documentVersion = BaseVersion.GetDocumentVersionFromDocument(document, ChangeAction.DeleteDocumentType);
                        documentVersion.DocTypeVersionId = documentTypeVersion.Id;
                        documentVersionDao.Add(documentVersion);

                        ActionLogHelper.AddActionLog("Add document version for document id " + document.Id + " successfully!", LoginUser, ActionName.AddDocumentVersion, ObjectType.Document, documentVersion.Id, dataContext);
                        document.Pages = pageDao.GetByDoc(document.Id);
                        document.FieldValues = fieldValueDao.GetByDoc(document.Id);

                        foreach (var page in document.Pages)
                        {
                            PageVersion pageVersion = BaseVersion.GetPageVersionFromPage(page);
                            pageVersion.DocVersionId = documentVersion.Id;

                            if (_setting.IsSaveFileInFolder)
                            {
                                string path = Path.Combine(document.DocTypeId.ToString(), document.Id.ToString() + "_" + documentVersion.Version.ToString(), Guid.NewGuid().ToString());
                                path = Path.Combine(_setting.LocationSaveFile, ARCHIVE_VERSION_FOLDER, path);
                                FileHelpper.Copy(page.FilePath, path);
                                pageVersion.FilePath = path;
                                pageVersion.FileBinary = null;
                            }

                            pageVersionDao.Add(pageVersion);

                            ActionLogHelper.AddActionLog("Add page version for page id " + page.Id + " successfully!", LoginUser, ActionName.AddPageVersion, ObjectType.Page, pageVersion.Id, dataContext);

                            page.Annotations = annotationDao.GetByPage(page.Id);

                            foreach (var annotation in page.Annotations)
                            {
                                AnnotationVersion annotationVersion = BaseVersion.GetAnnotationVersionFromAnnotation(annotation);
                                annotationVersion.PageVersionId = pageVersion.Id;
                                annotationVersionDao.Add(annotationVersion);
                                ActionLogHelper.AddActionLog("Add annotation version for page id " + page.Id + " successfully!", LoginUser, ActionName.AddAnnotationVersion, ObjectType.Page, pageVersion.Id, dataContext);
                            }
                        }

                        foreach (var fieldValue in document.FieldValues)
                        {
                            fieldValue.FieldMetaData = fieldMetadataDao.GetById(fieldValue.FieldId);
                            DocumentFieldVersion fieldValueVersion = BaseVersion.GetDocumentFieldVersionFromFieldValue(fieldValue);
                            fieldValueVersion.DocVersionID = documentVersion.Id;
                            if (!mappingFieldMetaDataToVersion.ContainsKey(fieldValueVersion.FieldId))
                            {
                                continue;
                            }
                            fieldValueVersion.FieldId = mappingFieldMetaDataToVersion[fieldValueVersion.FieldId];
                            fieldValueVersionDao.Add(fieldValueVersion);
                            ActionLogHelper.AddActionLog("Add field value version for document id " + document.Id + " successfully!", LoginUser, ActionName.AddFieldVersionValue, ObjectType.Document, documentVersion.Id, dataContext);
                        }
                    }

                    List<DocumentVersion> documentVersions = documentVersionDao.GetByDocType(documentTypeVersion.Id);

                    foreach (DocumentVersion docVersion in documentVersions)
                    {
                        if (docVersion.DocTypeVersionId == Guid.Empty)
                        {
                            docVersion.DocTypeVersionId = docVersion.DocTypeId;
                            documentVersionDao.UpdateDocumentTypeVersionId(docVersion);
                        }
                    }

                    //End add version

                    #endregion

                    //lookupInfoDao.DeleteByDocType(documentType.Id);
                    //lookupMapDao.DeleteByDocType(documentType.Id);
                    fieldValueDao.DeleteByDocType(documentType.Id);
                    picklistDao.DeleteByDocType(documentType.Id);
                    ocrTemplateZoneDao.DeleteByDocType(documentType.Id);
                    ocrTemplatePageDao.DeleteByDocType(documentType.Id);
                    ocrTemplateDao.Delete(documentType.Id);
                    barcodeConfigurationDao.DeleteByDocType(documentType.Id);
                    searchQueryExpressionDao.DeleteByDocType(documentType.Id);
                    searchQueryDao.DeleteByDocType(documentType.Id);
                    fieldMetaDataDao.DeleteByDocType(documentType.Id);
                    annotationPermissionDao.DeleteByDocType(documentType.Id);
                    documentTypePermissionDao.DeleteByDocType(documentType.Id);
                    annotationDao.DeleteByDocType(documentType.Id);
                    pageDao.DeleteByDocType(documentType.Id);
                    documentDao.DeleteByDocType(documentType.Id);
                    docTypeDao.Delete(documentType.Id);

                    if (_setting.IsSaveFileInFolder)
                    {
                        FileHelpper.DeleteFolder(Path.Combine(_setting.LocationSaveFile, ARCHIVE_FOLDER, documentType.Id.ToString()));
                    }

                    ActionLogHelper.AddActionLog("Delete document type name " + documentType.Name + " successfully", LoginUser, ActionName.DeleteDocumentType, null, null, dataContext);

                    using (var luceneClient = GetLucenceClientChannel())
                    {
                        luceneClient.Channel.DeleteDocumentType(documentType);
                    }

                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
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
                catch (Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public List<BarcodeConfiguration> GetBarcodeConfigurations(Guid docTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to retrieve barcode configurations.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new BarcodeConfigurationDao(dataContext).GetByDocType(docTypeId);
            }
        }

        public void SaveBarcodeConfiguration(BarcodeConfiguration barcode)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save barcode configuration.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                dataContext.BeginTransaction();

                try
                {
                    if (barcode.Id == Guid.Empty)
                    {
                        new BarcodeConfigurationDao(dataContext).Add(barcode);
                        ActionLogHelper.AddActionLog("Add new barcode configuration for document type id='" + barcode.DocumentTypeId + "'", LoginUser, ActionName.AddBarcodeConfig, null, null, dataContext);
                    }
                    else
                    {
                        new BarcodeConfigurationDao(dataContext).Update(barcode);
                        ActionLogHelper.AddActionLog("Update barcode configuration for document type id='" + barcode.DocumentTypeId + "'", LoginUser, ActionName.UpdateBarcodeConfig, null, null, dataContext);
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

        public void DeleteBarcodeConfiguration(Guid id)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to delete barcode configuration.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                dataContext.BeginTransaction();
                try
                {
                    new BarcodeConfigurationDao(dataContext).Delete(id);
                    ActionLogHelper.AddActionLog("Delete barcode configuration", LoginUser, ActionName.DeleteBarcodeConfig, null, null, dataContext);
                    dataContext.Commit();
                }
                catch (Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public void ClearBarcodeConfigurations(Guid documentTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to clear barcode configuration.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                dataContext.BeginTransaction();
                try
                {
                    new BarcodeConfigurationDao(dataContext).DeleteByDocType(documentTypeId);
                    dataContext.Commit();
                }
                catch (Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        //public void DeleteLookupInfo(Guid fieldId)
        //{
        //    if (!LoginUser.IsAdmin)
        //    {
        //        throw new SecurityException(string.Format("User {0} doesn't have permission to delete lookup info.", LoginUser.UserName));
        //    }

        //    using (DapperContext dataContext = new DapperContext(LoginUser))
        //    {
        //        LookupInfoDao lookupInfoDao = new LookupInfoDao(dataContext);
        //        LookupMapDao lookupMapDao = new LookupMapDao(dataContext);

        //        try
        //        {
        //            lookupInfoDao.Delete(fieldId);
        //            lookupMapDao.DeleteByField(fieldId);

        //            dataContext.Commit();
        //        }
        //        catch
        //        {
        //            dataContext.Rollback();
        //            throw;
        //        }
        //    }
        //}

        //Private methods
        private void AddSystemFields(DocumentType documentType)
        {
            documentType.FieldMetaDatas.Add(new FieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                //FieldUniqueID = GenerateUniqueId(),
                IsRequired = true,
                IsSystemField = true,
                Name = FieldMetaData._sysCreatedBy
            });

            documentType.FieldMetaDatas.Add(new FieldMetaData
            {
                DataType = FieldDataType.Date.ToString(),
                //FieldUniqueID = GenerateUniqueId(),
                IsRequired = true,
                IsSystemField = true,
                Name = FieldMetaData._sysCreatedOn
            });

            documentType.FieldMetaDatas.Add(new FieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                // FieldUniqueID = GenerateUniqueId(),
                IsRequired = false,
                IsSystemField = true,
                Name = FieldMetaData._sysModifiedBy
            });

            documentType.FieldMetaDatas.Add(new FieldMetaData
            {
                DataType = FieldDataType.Date.ToString(),
                // FieldUniqueID = GenerateUniqueId(),
                IsRequired = false,
                IsSystemField = true,
                Name = FieldMetaData._sysModifiedOn
            });

            documentType.FieldMetaDatas.Add(new FieldMetaData
            {
                DataType = FieldDataType.Integer.ToString(),
                // FieldUniqueID = GenerateUniqueId(),
                IsRequired = true,
                IsSystemField = true,
                Name = FieldMetaData._sysPageCount
            });
        }

        private string GenerateUniqueId()
        {
            long i = Guid.NewGuid().ToByteArray().Aggregate<byte, long>(1, (current, b) => current * ((int)b + 1));
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        private void FillElementToDocumentTypes(IEnumerable<DocumentType> documentTypes, DapperContext dataContext)
        {
            foreach (var docType in documentTypes)
            {
                FillElementToDocumentType(docType, dataContext);
            }
        }

        private void FillElementToDocumentType(DocumentType docType, DapperContext dataContext)
        {
            FieldMetaDataDao fieldDao = new FieldMetaDataDao(dataContext);
            docType.FieldMetaDatas = fieldDao.GetByDocType(docType.Id);

            if (!LoginUser.IsAdmin)
            {
                var permissionManager = new PermissionManager(LoginUser);
                docType.AnnotationPermission = permissionManager.GetAnnotationPermissionForUser(LoginUser, docType);
                docType.DocumentTypePermission = permissionManager.GetDocTypePermissionForUser(LoginUser, docType);

                if (!docType.DocumentTypePermission.AllowedSeeRetrictedField)
                {
                    docType.FieldMetaDatas.RemoveAll(p => p.IsRestricted);
                }
            }

            OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
            PicklistDao picklistDao = new PicklistDao(dataContext);
            //LookupInfoDao lookupInfoDao = new LookupInfoDao(dataContext);
            //LookupMapDao lookupMapDao = new LookupMapDao(dataContext);

            foreach (var field in docType.FieldMetaDatas)
            {
                field.Picklists = picklistDao.GetByField(field.Id);
                field.OCRTemplateZone = ocrTemplateZoneDao.GetByField(field.Id);

                if (field.IsLookup && field.LookupXML != null)
                {
                    field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXML);
                    field.LookupMaps = field.LookupInfo.LookupMaps;//lookupMapDao.GetByField(field.Id);
                    foreach (LookupMap mapping in field.LookupMaps)
                    {
                        FieldMetaData archiveField = fieldDao.GetById(mapping.ArchiveFieldId);
                        if (archiveField != null)
                        {
                            mapping.Name = archiveField.Name;
                        }
                    }
                }

                field.Children = fieldDao.GetChildren(field.Id);//docType.FieldMetaDatas.Where(p => p.ParentFieldId != null && p.ParentFieldId == field.Id).ToList();
            }

            docType.BarcodeConfigurations = new BarcodeConfigurationDao(dataContext).GetByDocType(docType.Id);

            foreach (var config in docType.BarcodeConfigurations)
            {
                if (config.MapValueToFieldId != null)
                {
                    config.FieldMetaData = new FieldMetaDataDao(dataContext).GetById(config.MapValueToFieldId ?? (Guid)config.MapValueToFieldId);
                }
            }

            docType.OCRTemplate = new OCRTemplateDao(dataContext).GetById(docType.Id);
            if (docType.OCRTemplate != null)
            {
                using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    docType.OCRTemplate.Language = GetLanguage(new LanguageDao(primaryContext).GetById(docType.OCRTemplate.LanguageId));
                }

                docType.OCRTemplate.OCRTemplatePages = new OCRTemplatePageDao(dataContext).GetByOCRTemplate(docType.OCRTemplate.DocTypeId);
                foreach (var ocrTemplatePage in docType.OCRTemplate.OCRTemplatePages)
                {
                    ocrTemplatePage.OCRTemplateZones = ocrTemplateZoneDao.GetByOCRTemplatePage(ocrTemplatePage.Id);
                }
            }
        }

        private void AddDocType(DocumentType documentType, DapperContext dataContext,
                              DocTypeDao docTypeDao, FieldMetaDataDao fieldMetadataDao, PicklistDao picklistDao)
        {
            docTypeDao.Add(documentType);
            foreach (var field in documentType.FieldMetaDatas)
            {
                field.DocTypeId = documentType.Id;
                AddField(field, fieldMetadataDao, picklistDao);
            }

            ActionLogHelper.AddActionLog("Add document type name " + documentType.Name, LoginUser, ActionName.AddDocumentType, null, null, dataContext);
        }

        private void AddField(FieldMetaData field, FieldMetaDataDao fieldMetadataDao, PicklistDao picklistDao)
        {
            fieldMetadataDao.Add(field);

            if (field.Children != null && field.Children.Count > 0)
            {
                foreach (var child in field.Children)
                {
                    child.ParentFieldId = field.Id;
                    child.DocTypeId = field.DocTypeId;
                    //child.FieldUniqueID = Guid.NewGuid().ToString();

                    fieldMetadataDao.Add(child);
                }
            }

            if (field.LookupInfo != null && field.IsLookup)
            {
                field.LookupXML = UtilsSerializer.Serialize<LookupInfo>(field.LookupInfo);
                //lookupInfoDao.Add(field.LookupInfo);
            }

            //if (field.LookupMaps != null && field.LookupMaps.Count > 0)
            //{
            //    foreach (var map in field.LookupMaps)
            //    {
            //        //lookupMapDao.Add(map);
            //    }
            //}

            if (field.Picklists != null && field.Picklists.Count > 0)
            {
                foreach (var pickList in field.Picklists)
                {
                    pickList.FieldId = field.Id;
                    picklistDao.Add(pickList);
                }
            }
        }


        private void UpdateDocType(DocumentType documentType, DapperContext dataContext,
                              DocTypeDao docTypeDao, FieldMetaDataDao fieldMetadataDao, PicklistDao picklistDao)
        {
            if (documentType.DeletedFields != null)
            {
                foreach (var removeField in documentType.DeletedFields)
                {
                    using (var luceneClient = GetLucenceClientChannel())
                    {
                        luceneClient.Channel.DeleteField(documentType, removeField);
                    }
                    //lookupInfoDao.Delete(removeField.Id);
                    //lookupMapDao.DeleteByField(removeField.Id);
                    new TableFieldValueDao(dataContext).DeleteByParentField(removeField.Id);
                    new FieldValueDao(dataContext).DeleteByField(removeField.Id);
                    new OCRTemplateZoneDao(dataContext).Delete(removeField.Id);
                    new BarcodeConfigurationDao(dataContext).DeleteByField(removeField.Id);
                    picklistDao.DeleteByField(removeField.Id);
                    new SearchQueryExpressionDao(dataContext).DeleteByField(removeField.Id);
                    fieldMetadataDao.DeleteChildren(removeField.Id);
                    fieldMetadataDao.Delete(removeField.Id);
                    ActionLogHelper.AddActionLog("Delete field name " + removeField.Name, LoginUser, ActionName.DeleteFieldMetaData, null, null, dataContext);
                }
            }

            foreach (var field in documentType.FieldMetaDatas)
            {
                //if (field.DeletedLookupMaps != null && field.DeletedLookupMaps.Count > 0)
                //{
                //    lookupMapDao.Delete(field.DeletedLookupMaps.Select(p => p.Id).ToList());
                //    ActionLogHelper.AddActionLog("Delete lookup mapping", LoginUser, ActionName.DeleteLookupMapping, null, null, dataContext);
                //}

                if (field.Id == Guid.Empty)
                {
                    field.DocTypeId = documentType.Id;
                    AddField(field, fieldMetadataDao, picklistDao);
                }
                else
                {
                    if (field.DeleteChildIds != null)
                    {
                        field.DeleteChildIds.ForEach(fieldMetadataDao.Delete);
                    }

                    //foreach (var lookupMap in field.LookupMaps)
                    //{
                    //    if (lookupMap.Id == Guid.Empty)
                    //    {
                    //        lookupMap.FieldId = field.Id;
                    //        lookupMapDao.Add(lookupMap);
                    //        ActionLogHelper.AddActionLog("Add lookup mapping", LoginUser, ActionName.AddLookupMapping, null, null, dataContext);
                    //    }
                    //    else
                    //    {
                    //        lookupMapDao.Update(lookupMap);
                    //        ActionLogHelper.AddActionLog("Update lookup mapping", LoginUser, ActionName.UpdateLookupMapping, null, null, dataContext);
                    //    }
                    //}

                    picklistDao.DeleteByField(field.Id);
                    foreach (var picklist in field.Picklists)
                    {
                        picklist.FieldId = field.Id;
                        picklistDao.Add(picklist);
                        ActionLogHelper.AddActionLog("Add Picklist", LoginUser, ActionName.AddPicklist, null, null, dataContext);
                    }
                    if (field.IsLookup && field.LookupInfo != null)
                    {
                        field.LookupXML = UtilsSerializer.Serialize<LookupInfo>(field.LookupInfo);
                    }
                    //LookupInfo existInfo = lookupInfoDao.GetById(field.Id);
                    //if (field.IsLookup && field.LookupInfo != null)
                    //{
                    //    if (existInfo == null)
                    //    {
                    //        field.LookupInfo.FieldId = field.Id;
                    //        lookupInfoDao.Add(field.LookupInfo);
                    //        ActionLogHelper.AddActionLog("Add lookup info", LoginUser, ActionName.AddLookupInfo, null, null, dataContext);
                    //    }
                    //    else
                    //    {
                    //        lookupInfoDao.Update(field.LookupInfo);
                    //        ActionLogHelper.AddActionLog("Update lookup info", LoginUser, ActionName.UpdateLookupInfo, null, null, dataContext);
                    //    }
                    //}
                    //else
                    //{
                    //    if (existInfo != null)
                    //    {
                    //        lookupInfoDao.Delete(existInfo.FieldId);
                    //        ActionLogHelper.AddActionLog("Delete lookup info", LoginUser, ActionName.DeleteLookupInfo, null, null, dataContext);
                    //    }
                    //}

                    if (field.DataType != "Table" && field.Children.Count > 0)
                    {
                        fieldMetadataDao.DeleteChildren(field.Id);
                    }

                    fieldMetadataDao.Update(field);

                    if (field.Children != null && field.Children.Count > 0)
                    {
                        foreach (var child in field.Children)
                        {
                            if (child.Id == Guid.Empty)
                            {
                                child.DocTypeId = field.DocTypeId;
                                child.ParentFieldId = field.Id;
                                fieldMetadataDao.Add(child);
                            }
                            else
                            {
                                fieldMetadataDao.Update(child);
                            }
                        }
                    }

                    ActionLogHelper.AddActionLog("Update fieldmetadata name " + field.Name, LoginUser, ActionName.UpdateFieldMetaData, null, null, dataContext);
                }

            }

            documentType.ModifiedBy = LoginUser.UserName;
            documentType.ModifiedDate = DateTime.Now;
            docTypeDao.Update(documentType);
            ActionLogHelper.AddActionLog("Update document type name " + documentType.Name, LoginUser, ActionName.UpdateDocumentType, null, null, dataContext);
        }
    }
}