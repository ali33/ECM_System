using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.SecurityDao;
using Ecm.BarcodeDomain;
using Ecm.LookupDomain;
using Ecm.Utility;
using System.IO;
using Ecm.SecurityDao.Domain;

namespace Ecm.CaptureCore
{
    public class BatchTypeManager : ManagerBase
    {
        private const string CAPTURE_FOLDER = "CAPTURE";
        private Setting _setting = new Setting();

        #region Public methods
        public BatchTypeManager(User loginUser)
            : base(loginUser)
        {
            _setting = new SettingManager(loginUser).GetSettings();
        }

        public BatchType GetBatchType(Guid batchTypeId)
        {
            using (DapperContext context = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(context);
                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(context);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(context);
                OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(context);
                BarcodeConfigurationDao barcodeConfigurationDao = new BarcodeConfigurationDao(context);
                BatchTypePermissionDao batchTypePermissionDao = new BatchTypePermissionDao(context);
                DocumentTypePermissionDao docTypePermissionDao = new DocumentTypePermissionDao(context);
                DocumentFieldPermissionDao fieldPermissionDao = new DocumentFieldPermissionDao(context);
                PicklistDao picklistDao = new PicklistDao(context);

                BatchType batchType = batchTypeDao.GetById(batchTypeId);
                batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batchType.Id);

                foreach (BatchFieldMetaData field in batchType.Fields)
                {
                    if (field.IsLookup && !string.IsNullOrEmpty(field.LookupXml))
                    {
                        field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXml);
                    }
                }

                batchType.DocTypes = docTypeDao.GetDocumentTypeByBatch(batchType.Id);

                if (!string.IsNullOrEmpty(batchType.BarcodeConfigurationXml))
                {
                    batchType.BarcodeConfiguration = Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(batchType.BarcodeConfigurationXml);
                }


                if (LoginUser.IsAdmin)
                {
                    batchType.BatchTypePermission = BatchTypePermission.GetAllowAll();
                }
                else
                {
                    batchType.BatchTypePermission = GetBatchTypePermission(context, batchTypeId);

                }

                foreach (var docType in batchType.DocTypes)
                {
                    docType.Fields = docFieldMetaDataDao.GetByDocType(docType.Id);

                    foreach (DocumentFieldMetaData docField in docType.Fields)
                    {
                        docField.OCRTemplateZone = ocrTemplateZoneDao.GetByField(docField.Id);
                        docField.Children = docFieldMetaDataDao.GetChildren(docField.Id);
                        docField.Picklists = picklistDao.GetByField(docField.Id);
                    }

                    docType.OCRTemplate = GetOcrTemplate(docType.Id, context);
                    docType.DocumentTypePermission = GetDocumentTypePermission(context, docType.Id);
                }

                ActionLogHelper.AddActionLog("Get Batch Type name: " + batchType.Name + " successfully", LoginUser, ActionName.GetBatchType, null, null, context);
                return batchType;
            }
        }

        public List<BatchType> GetAllBatchTypes()
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to get all batch types.", LoginUser.UserName));
            }

            List<BatchType> batchTypes;
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(dataContext);
                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(dataContext);
                OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
                BarcodeConfigurationDao barcodeConfigurationDao = new BarcodeConfigurationDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);

                batchTypes = batchTypeDao.GetAll();

                foreach (var batchType in batchTypes)
                {
                    batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batchType.Id);
                    batchType.DocTypes = docTypeDao.GetDocumentTypeByBatch(batchType.Id);
                    batchType.BatchTypePermission = GetBatchTypePermission(dataContext, batchType.Id);

                    if (!string.IsNullOrEmpty(batchType.BarcodeConfigurationXml))
                    {
                        batchType.BarcodeConfiguration = Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(batchType.BarcodeConfigurationXml);
                    }

                    foreach (var docType in batchType.DocTypes)
                    {
                        docType.Fields = docFieldMetaDataDao.GetByDocType(docType.Id);
                        foreach (DocumentFieldMetaData docField in docType.Fields)
                        {
                            docField.OCRTemplateZone = ocrTemplateZoneDao.GetByField(docField.Id);
                            docField.Children = docFieldMetaDataDao.GetChildren(docField.Id);
                            docField.Picklists = picklistDao.GetByField(docField.Id);
                        }

                        docType.OCRTemplate = GetOcrTemplate(docType.Id, dataContext);
                        //docType.BarcodeConfigurations.AddRange(GetBarcodes(docType.Id, barcodeConfigurationDao, docFieldMetaDataDao));
                        docType.DocumentTypePermission = GetDocumentTypePermission(dataContext, docType.Id);
                    }
                }

                ActionLogHelper.AddActionLog("Get list of Batch Type successfully", LoginUser, ActionName.GetBatchType, null, null, dataContext);
            }
            //return new List<BatchType>();
            return batchTypes;
        }

        public List<BatchType> GetCapturedBatchTypes()
        {
            using (DapperContext context = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(context);
                List<BatchType> batchTypes = new List<BatchType>();
                List<Guid> groupIds = null;

                if (LoginUser.IsAdmin)
                {
                    batchTypes = batchTypeDao.GetCapturedBatchTypes();
                }
                else
                {
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    batchTypes = batchTypeDao.GetCapturedBatchTypes(groupIds);
                    ActionLogHelper.AddActionLog("Get list of Batch Type by user name: " + LoginUser.UserName + " successfully",
                        LoginUser, ActionName.GetBatchType, null, null, context);
                }

                foreach (BatchType batchType in batchTypes)
                {
                    BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);
                    DocumentTypeDao docTypeDao = new DocumentTypeDao(context);
                    DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(context);
                    OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(context);
                    BarcodeConfigurationDao barcodeConfigurationDao = new BarcodeConfigurationDao(context);
                    BatchTypePermissionDao batchTypePermissionDao = new BatchTypePermissionDao(context);
                    PicklistDao picklistDao = new PicklistDao(context);

                    batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batchType.Id);

                    foreach (BatchFieldMetaData field in batchType.Fields)
                    {
                        if (field.IsLookup && !string.IsNullOrEmpty(field.LookupXml))
                        {
                            field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXml);
                        }
                    }

                    ActionLogHelper.AddActionLog("Get batch field meta data for batch type name: " + batchType.Name + " successfully",
                        LoginUser, ActionName.GetBatchFieldMetaData, null, null, context);

                    if (!string.IsNullOrEmpty(batchType.BarcodeConfigurationXml))
                    {
                        batchType.BarcodeConfiguration = Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(batchType.BarcodeConfigurationXml);
                    }

                    if (LoginUser.IsAdmin)
                    {
                        batchType.DocTypes = docTypeDao.GetDocumentTypeByBatch(batchType.Id);
                        ActionLogHelper.AddActionLog("Get documents type for batch type name: " + batchType.Name + " with user admin successfully",
                            LoginUser, ActionName.GetDocumentType, null, null, context);
                        batchType.BatchTypePermission = BatchTypePermission.GetAllowAll();
                    }
                    else
                    {
                        batchType.DocTypes = docTypeDao.GetDocumentTypeByBatch(batchType.Id, groupIds);
                        ActionLogHelper.AddActionLog("Get documents type for batch type name: " + batchType.Name + " with user name: " + LoginUser.UserName + " successfully",
                            LoginUser, ActionName.GetDocumentType, null, null, context);
                        batchType.BatchTypePermission = GetBatchTypePermission(context, batchType.Id);
                    }

                    foreach (var docType in batchType.DocTypes)
                    {
                        docType.Fields = docFieldMetaDataDao.GetByDocType(docType.Id);

                        foreach (DocumentFieldMetaData docField in docType.Fields)
                        {
                            docField.OCRTemplateZone = ocrTemplateZoneDao.GetByField(docField.Id);

                            if (docField.IsLookup && !string.IsNullOrEmpty(docField.LookupXml))
                            {
                                docField.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(docField.LookupXml);
                            }

                            docField.Picklists = picklistDao.GetByField(docField.Id);
                            docField.Children = docFieldMetaDataDao.GetChildren(docField.Id);
                        }

                        docType.OCRTemplate = GetOcrTemplate(docType.Id, context);

                        if (LoginUser.IsAdmin)
                        {
                            docType.DocumentTypePermission = DocumentTypePermission.GetAll();
                            foreach (DocumentFieldMetaData field in docType.Fields.Where(p => !p.IsSystemField))
                            {
                                DocumentFieldPermission fieldPermission = DocumentFieldPermission.GetAll();
                                fieldPermission.DocTypeId = docType.Id;
                                fieldPermission.FieldId = field.Id;

                                //docType.DocumentTypePermission.FieldPermissions.Add(fieldPermission);
                            }
                        }
                        else
                        {
                            docType.DocumentTypePermission = GetDocumentTypePermission(context, docType.Id);
                        }
                    }
                }


                return batchTypes;
            }
        }

        public List<BatchType> GetAssignWorkBatchTypes()
        {
            using (DapperContext context = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(context);
                List<BatchType> batchTypes = new List<BatchType>();
                List<Guid> groupIds = null;

                if (LoginUser.IsAdmin)
                {
                    batchTypes = batchTypeDao.GetCapturedBatchTypes();
                }
                else
                {
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }

                    batchTypes = batchTypeDao.GetAssignedBatchTypes(groupIds);
                    ActionLogHelper.AddActionLog("Get list of Batch Type by user name: " + LoginUser.UserName + " successfully",
                        LoginUser, ActionName.GetBatchType, null, null, context);
                }

                foreach (BatchType batchType in batchTypes)
                {
                    BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);
                    DocumentTypeDao docTypeDao = new DocumentTypeDao(context);
                    DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(context);
                    OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(context);
                    BarcodeConfigurationDao barcodeConfigurationDao = new BarcodeConfigurationDao(context);
                    BatchTypePermissionDao batchTypePermissionDao = new BatchTypePermissionDao(context);
                    PicklistDao picklistDao = new PicklistDao(context);

                    batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batchType.Id);

                    foreach (BatchFieldMetaData field in batchType.Fields)
                    {
                        if (field.IsLookup && !string.IsNullOrEmpty(field.LookupXml))
                        {
                            field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXml);
                        }
                    }

                    ActionLogHelper.AddActionLog("Get batch field meta data for batch type name: " + batchType.Name + " successfully",
                        LoginUser, ActionName.GetBatchFieldMetaData, null, null, context);

                    if (!string.IsNullOrEmpty(batchType.BarcodeConfigurationXml))
                    {
                        batchType.BarcodeConfiguration = Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(batchType.BarcodeConfigurationXml);
                    }

                    if (LoginUser.IsAdmin)
                    {
                        batchType.DocTypes = docTypeDao.GetDocumentTypeByBatch(batchType.Id);
                        ActionLogHelper.AddActionLog("Get documents type for batch type name: " + batchType.Name + " with user admin successfully",
                            LoginUser, ActionName.GetDocumentType, null, null, context);
                        batchType.BatchTypePermission = BatchTypePermission.GetAllowAll();
                    }
                    else
                    {
                        batchType.DocTypes = docTypeDao.GetDocumentTypeByBatch(batchType.Id, groupIds);
                        ActionLogHelper.AddActionLog("Get documents type for batch type name: " + batchType.Name + " with user name: " + LoginUser.UserName + " successfully",
                            LoginUser, ActionName.GetDocumentType, null, null, context);
                        batchType.BatchTypePermission = GetBatchTypePermission(context, batchType.Id);
                    }

                    foreach (var docType in batchType.DocTypes)
                    {
                        docType.Fields = docFieldMetaDataDao.GetByDocType(docType.Id);

                        foreach (DocumentFieldMetaData docField in docType.Fields)
                        {
                            docField.OCRTemplateZone = ocrTemplateZoneDao.GetByField(docField.Id);

                            if (docField.IsLookup && !string.IsNullOrEmpty(docField.LookupXml))
                            {
                                docField.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(docField.LookupXml);
                            }

                            docField.Picklists = picklistDao.GetByField(docField.Id);
                            docField.Children = docFieldMetaDataDao.GetChildren(docField.Id);
                        }

                        docType.OCRTemplate = GetOcrTemplate(docType.Id, context);

                        if (LoginUser.IsAdmin)
                        {
                            docType.DocumentTypePermission = DocumentTypePermission.GetAll();
                            foreach (DocumentFieldMetaData field in docType.Fields.Where(p => !p.IsSystemField))
                            {
                                DocumentFieldPermission fieldPermission = DocumentFieldPermission.GetAll();
                                fieldPermission.DocTypeId = docType.Id;
                                fieldPermission.FieldId = field.Id;

                                //docType.DocumentTypePermission.FieldPermissions.Add(fieldPermission);
                            }
                        }
                        else
                        {
                            docType.DocumentTypePermission = GetDocumentTypePermission(context, docType.Id);
                        }
                    }
                }


                return batchTypes;
            }
        }

        public void Save(BatchType batchType)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save batch type.", LoginUser.UserName));
            }

            foreach (var field in batchType.Fields)
            {
                if (string.IsNullOrEmpty(field.UniqueId))
                {
                    field.UniqueId = GenerateUniqueId();
                }
            }

            DateTime now = DateTime.Now;
            if (batchType.Id == Guid.Empty)
            {
                // System field will be store in the main table batch when create batch
                // So no need to store system field in table BatchFieldValue
                //AddBatchSystemFields(batchType);
                batchType.CreatedDate = now;
                batchType.CreatedBy = LoginUser.UserName;
                batchType.UniqueId = GenerateUniqueId();
            }

            foreach (var docType in batchType.DocTypes)
            {
                foreach (var field in docType.Fields)
                {
                    if (string.IsNullOrEmpty(field.UniqueId))
                    {
                        field.UniqueId = GenerateUniqueId();
                    }
                }

                if (docType.Id == Guid.Empty)
                {
                    //AddDocSystemFields(docType);
                    docType.CreatedDate = now;
                    docType.CreatedBy = LoginUser.UserName;
                    docType.UniqueId = GenerateUniqueId();
                }
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(dataContext);
                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);

                dataContext.BeginTransaction();

                try
                {
                    if (batchType.Id == Guid.Empty)
                    {
                        AddBatchType(batchType, dataContext);
                    }
                    else
                    {
                        UpdateBatchType(batchType, dataContext, batchTypeDao, batchFieldMetaDataDao,
                                        docTypeDao, docFieldMetaDataDao, picklistDao);
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

        public void Delete(Guid batchTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to delete batch type.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(dataContext);
                BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(dataContext);
                BatchFieldValueDao batchFieldValueDao = new BatchFieldValueDao(dataContext);
                ReleaseBatchFieldValueDao backupBatchFieldValueDao = new ReleaseBatchFieldValueDao(dataContext);
                ReleaseDocumentDao backupDocDao = new ReleaseDocumentDao(dataContext);
                ReleaseDocumentFieldValueDao backupDocFieldValueDao = new ReleaseDocumentFieldValueDao(dataContext);
                ReleasePageDao backupPageDao = new ReleasePageDao(dataContext);
                DocumentTypeDao docTypeDao = new DocumentTypeDao(dataContext);
                DocFieldMetaDataDao docFieldMetaDataDao = new DocFieldMetaDataDao(dataContext);
                DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(dataContext);
                PageDao pageDao = new PageDao(dataContext);
                PicklistDao picklistDao = new PicklistDao(dataContext);
                OCRTemplateDao ocrTemplateDao = new OCRTemplateDao(dataContext);
                OCRTemplatePageDao ocrTemplatePageDao = new OCRTemplatePageDao(dataContext);
                OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
                DocumentDao docDao = new DocumentDao(dataContext);
                WorkflowDefinitionDao workflowDefinitionDao = new WorkflowDefinitionDao(dataContext);
                WorkflowHumanStepPermissionDao workflowHumanStepPermissionDao = new WorkflowHumanStepPermissionDao(dataContext);
                WorkflowHumanStepDocTypePermissionDao workflowHumanStepDocTypePermissionDao = new WorkflowHumanStepDocTypePermissionDao(dataContext);
                DocumentFieldPermissionDao fieldPermissionDao = new DocumentFieldPermissionDao(dataContext);

                dataContext.BeginTransaction();

                try
                {
                    List<DocumentType> docTypes = docTypeDao.GetDocumentTypeByBatch(batchTypeId);
                    ActionLogHelper.AddActionLog("Get documents type for batch type id: " + batchTypeId + " successfully",
                                                LoginUser, ActionName.GetDocumentType, null, null, dataContext);

                    foreach (var docType in docTypes)
                    {
                        ocrTemplateZoneDao.DeleteByDocType(docType.Id);
                        ocrTemplatePageDao.DeleteByDocType(docType.Id);
                        ocrTemplateDao.Delete(docType.Id);
                        picklistDao.DeleteByDocType(docType.Id);
                        docFieldValueDao.DeleteByDocType(docType.Id);
                        ActionLogHelper.AddActionLog("Delete document fields value on batch type Id: " + batchTypeId + " successfully",
                                                    LoginUser, ActionName.DeleteFieldValue, null, null, dataContext);

                        fieldPermissionDao.Delete(docType.Id);
                        backupPageDao.DeleteByDocType(docType.Id);
                        backupDocFieldValueDao.DeleteByDocType(docType.Id);

                        docFieldMetaDataDao.DeleteByDocType(docType.Id);
                        pageDao.DeleteByDocType(docType.Id);
                        docDao.DeleteByDocType(docType.Id);
                        ActionLogHelper.AddActionLog("Delete documents on batch type Id: " + batchTypeId + " successfully",
                                                    LoginUser, ActionName.DeleteDocument, null, null, dataContext);

                        workflowHumanStepDocTypePermissionDao.DeleteByDocType(docType.Id);
                    }

                    if (_setting.IsSaveFileInFolder)
                    {
                        FileHelpper.DeleteFolder(Path.Combine(_setting.LocationSaveFile, "CAPTURE", batchTypeId.ToString()));
                    }

                    backupDocDao.DeleteByBatchType(batchTypeId);
                    ActionLogHelper.AddActionLog("Delete released documents on batch type Id: " + batchTypeId + " successfully",
                                                LoginUser, ActionName.DeleteDocument, null, null, dataContext);

                    docTypeDao.DeleteByBatchType(batchTypeId);
                    ActionLogHelper.AddActionLog("Delete released document types on batch type Id: " + batchTypeId + " successfully",
                                                LoginUser, ActionName.DeleteDocumentType, null, null, dataContext);

                    backupBatchFieldValueDao.DeleteByBatchType(batchTypeId);
                    batchFieldValueDao.DeleteByBatchType(batchTypeId);
                    batchFieldMetaDataDao.DeleteByBatchType(batchTypeId);

                    workflowHumanStepDocTypePermissionDao.DeleteByBatchType(batchTypeId);
                    workflowHumanStepPermissionDao.DeleteByBatchType(batchTypeId);
                    workflowDefinitionDao.DeleteByBatchType(batchTypeId);

                    batchTypeDao.Delete(batchTypeId);
                    ActionLogHelper.AddActionLog("Delete batch types Id: " + batchTypeId + " successfully",
                                                LoginUser, ActionName.DeleteBatchType, null, null, dataContext);

                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public WorkflowDefinition GetWorkflowByBatchType(Guid batchTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save document type.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new WorkflowDefinitionDao(dataContext).GetByBatchTypeId(batchTypeId);
            }
        }

        public void SaveBarcodeConfiguration(string xml, Guid batchTypeId)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    BarcodeConfigurationDao barcodeDao = new BarcodeConfigurationDao(context);
                    barcodeDao.SetBarcodeConfiguration(xml, batchTypeId);
                    context.Commit();
                }
                catch
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Check the batch type is can/can't edit the field information
        /// </summary>
        /// <param name="batchTypeId">Batch type ID</param>
        /// <returns>True, if the batch type do not have any running batch. Else, return false</returns>
        public bool CanEditBatchTypeField(Guid batchTypeId)
        {
            using (var context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                return new BatchDao(context).ExistsBatchOfBatchType(batchTypeId) == 1 ? false : true;
            }
        }

        #endregion

        #region Private methods

        private string GenerateUniqueId()
        {
            long i = Guid.NewGuid().ToByteArray().Aggregate<byte, long>(1, (current, b) => current * ((int)b + 1));
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        private void AddBatchSystemFields(BatchType batchType)
        {
            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysBatchName
            });

            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysCreatedBy
            });

            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.Date.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysCreatedOn
            });

            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysModifiedBy
            });

            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.Date.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysModifiedOn
            });

            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.Integer.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysDocCount
            });

            batchType.Fields.Add(new BatchFieldMetaData
            {
                DataType = FieldDataType.Integer.ToString(),
                UniqueId = GenerateUniqueId(),
                IsSystemField = true,
                Name = BatchFieldMetaData._sysPageCount
            });
        }

        private void AddDocSystemFields(DocumentType docType)
        {
            docType.Fields.Add(new DocumentFieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                UniqueId = GenerateUniqueId(),
                IsRequired = true,
                IsSystemField = true,
                Name = DocumentFieldMetaData._sysCreatedBy
            });

            docType.Fields.Add(new DocumentFieldMetaData
            {
                DataType = FieldDataType.Date.ToString(),
                UniqueId = GenerateUniqueId(),
                IsRequired = true,
                IsSystemField = true,
                Name = DocumentFieldMetaData._sysCreatedOn
            });

            docType.Fields.Add(new DocumentFieldMetaData
            {
                DataType = FieldDataType.String.ToString(),
                UniqueId = GenerateUniqueId(),
                IsRequired = false,
                IsSystemField = true,
                Name = DocumentFieldMetaData._sysModifiedBy
            });

            docType.Fields.Add(new DocumentFieldMetaData
            {
                DataType = FieldDataType.Date.ToString(),
                UniqueId = GenerateUniqueId(),
                IsRequired = false,
                IsSystemField = true,
                Name = DocumentFieldMetaData._sysModifiedOn
            });

            docType.Fields.Add(new DocumentFieldMetaData
            {
                DataType = FieldDataType.Integer.ToString(),
                UniqueId = GenerateUniqueId(),
                IsRequired = true,
                IsSystemField = true,
                Name = DocumentFieldMetaData._sysPageCount
            });
        }

        private void AddBatchType(BatchType batchType, DapperContext context)
        {
            BatchTypeDao batchTypeDao = new BatchTypeDao(context);
            BatchFieldMetaDataDao batchFieldMetadataDao = new BatchFieldMetaDataDao(context);
            DocumentTypeDao docTypeDao = new DocumentTypeDao(context);
            DocFieldMetaDataDao docFieldMetadataDao = new DocFieldMetaDataDao(context);
            PicklistDao picklistDao = new PicklistDao(context);

            batchTypeDao.Add(batchType);

            foreach (var field in batchType.Fields.Where(h => !h.IsSystemField))
            {
                field.BatchTypeId = batchType.Id;
                batchFieldMetadataDao.Add(field);
                ActionLogHelper.AddActionLog("Create batch field meta data successfully",
                                                LoginUser, ActionName.CreateBatchFieldMetaData, null, null, context);
            }

            foreach (var docType in batchType.DocTypes)
            {
                docType.BatchTypeId = batchType.Id;
                docTypeDao.Add(docType);
                ActionLogHelper.AddActionLog("Create document type for batch type name: " + batchType.Name + " successfully",
                                LoginUser, ActionName.AddDocumentType, null, null, context);

                foreach (var field in docType.Fields.Where(h => !h.IsSystemField))
                {
                    field.DocTypeId = docType.Id;
                    docFieldMetadataDao.Add(field);

                    if (field.Picklists != null && field.Picklists.Count > 0)
                    {
                        foreach (Picklist picklist in field.Picklists)
                        {
                            picklist.FieldId = field.Id;
                            picklistDao.Add(picklist);
                        }
                    }

                    if (field.Children != null && field.Children.Count > 0)
                    {
                        foreach (var child in field.Children)
                        {
                            child.ParentFieldId = field.Id;
                            child.DocTypeId = field.DocTypeId;
                            child.UniqueId = Guid.NewGuid().ToString();

                            docFieldMetadataDao.Add(child);
                        }
                    }

                    ActionLogHelper.AddActionLog("Create document field meta data successfully",
                                                    LoginUser, ActionName.AddFieldMetaData, null, null, context);
                }
            }
        }

        private void UpdateBatchType(BatchType batchType, DapperContext dataContext, BatchTypeDao batchTypeDao, BatchFieldMetaDataDao batchFieldMetadataDao,
                                     DocumentTypeDao docTypeDao, DocFieldMetaDataDao docFieldMetadataDao, PicklistDao picklistDao)
        {
            if (batchType.DeletedFields != null && batchType.DeletedFields.Count > 0)
            {
                BatchFieldValueDao batchFieldValueDao = new BatchFieldValueDao(dataContext);
                ReleaseBatchFieldValueDao backupBatchFieldValueDao = new ReleaseBatchFieldValueDao(dataContext);
                foreach (var removeFieldId in batchType.DeletedFields)
                {
                    backupBatchFieldValueDao.DeleteByField(removeFieldId);
                    ActionLogHelper.AddActionLog("Delete batch field value from released batch successfully",
                                                    LoginUser, ActionName.DeleteBatchFieldValue, null, null, dataContext);
                    batchFieldValueDao.DeleteByField(removeFieldId);
                    ActionLogHelper.AddActionLog("Delete batch field value successfully",
                                                    LoginUser, ActionName.DeleteBatchFieldValue, null, null, dataContext);
                    batchFieldMetadataDao.Delete(removeFieldId);
                    ActionLogHelper.AddActionLog("Delete batch field meta data successfully",
                                                    LoginUser, ActionName.DeleteBatchFieldMetaData, null, null, dataContext);
                }
            }

            foreach (var field in batchType.Fields.Where(h => !h.IsSystemField))
            {
                if (field.Id == Guid.Empty)
                {
                    field.BatchTypeId = batchType.Id;
                    batchFieldMetadataDao.Add(field);
                    ActionLogHelper.AddActionLog("Create batch field meta data successfully",
                                                    LoginUser, ActionName.CreateBatchFieldMetaData, null, null, dataContext);
                }
                else
                {
                    batchFieldMetadataDao.Update(field);
                    ActionLogHelper.AddActionLog("Create batch field meta data successfully",
                                                    LoginUser, ActionName.UpdateBatchFieldMetaData, null, null, dataContext);
                }
            }

            DocumentFieldValueDao docFieldValueDao = new DocumentFieldValueDao(dataContext);
            ReleaseDocumentFieldValueDao backupDocumentFieldValueDao = new ReleaseDocumentFieldValueDao(dataContext);
            ReleaseDocumentDao backupDocDao = new ReleaseDocumentDao(dataContext);
            ReleasePageDao backupPageDao = new ReleasePageDao(dataContext);
            OCRTemplateDao ocrTemplateDao = new OCRTemplateDao(dataContext);
            OCRTemplatePageDao ocrTemplatePageDao = new OCRTemplatePageDao(dataContext);
            OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);
            DocumentDao docDao = new DocumentDao(dataContext);
            PageDao pageDao = new PageDao(dataContext);
            WorkflowHumanStepDocTypePermissionDao workflowHumanStepDocTypePermissionDao = new WorkflowHumanStepDocTypePermissionDao(dataContext);
            TableFieldValueDao tableValueDao = new TableFieldValueDao(dataContext);
            DocumentFieldPermissionDao documentFieldPermissionDao = new DocumentFieldPermissionDao(dataContext);

            Setting setting = new SettingManager(LoginUser).GetSettings();
            foreach (var removeDocTypeId in batchType.DeletedDocTypes)
            {
                ocrTemplateZoneDao.DeleteByDocType(removeDocTypeId);
                ocrTemplatePageDao.DeleteByDocType(removeDocTypeId);
                ocrTemplateDao.Delete(removeDocTypeId);
                picklistDao.DeleteByDocType(removeDocTypeId);
                backupDocumentFieldValueDao.DeleteByDocType(removeDocTypeId);
                backupPageDao.DeleteByDocType(removeDocTypeId);
                backupDocDao.DeleteByDocType(removeDocTypeId);
                tableValueDao.DeleteByDocType(removeDocTypeId);
                docFieldValueDao.DeleteByDocType(removeDocTypeId);
                docFieldMetadataDao.DeleteByDocType(removeDocTypeId);

                if (setting.IsSaveFileInFolder)
                {
                    //Delete File
                    List<Page> listPage = pageDao.GetByDocType(removeDocTypeId);
                    foreach (Page page in listPage)
                    {
                        string path = Path.Combine(setting.LocationSaveFile, page.FilePath == null ? "" : page.FilePath);
                        FileHelpper.DeleteFile(path);
                    }
                }
                pageDao.DeleteByDocType(removeDocTypeId);
                docDao.DeleteByDocType(removeDocTypeId);

                ActionLogHelper.AddActionLog("Delete capture documents successfully",
                                                LoginUser, ActionName.DeleteDocument, null, null, dataContext);
                workflowHumanStepDocTypePermissionDao.DeleteByDocType(removeDocTypeId);
                docTypeDao.Delete(removeDocTypeId);
                ActionLogHelper.AddActionLog("Delete capture documents type successfully",
                                                LoginUser, ActionName.DeleteDocumentType, null, null, dataContext);
            }

            foreach (var docType in batchType.DocTypes)
            {
                if (docType.Id == Guid.Empty)
                {
                    docType.BatchTypeId = batchType.Id;
                    docTypeDao.Add(docType);
                    ActionLogHelper.AddActionLog("Create documents type successfully",
                                                    LoginUser, ActionName.AddDocumentType, null, null, dataContext);

                    foreach (var field in docType.Fields.Where(h => !h.IsSystemField))
                    {
                        field.DocTypeId = docType.Id;
                        docFieldMetadataDao.Add(field);

                        if (field.Picklists != null && field.Picklists.Count > 0)
                        {
                            foreach (Picklist picklist in field.Picklists)
                            {
                                picklist.FieldId = field.Id;
                                picklistDao.Add(picklist);
                            }
                        }

                        if (field.Children != null && field.Children.Count > 0)
                        {
                            foreach (var child in field.Children)
                            {
                                child.ParentFieldId = field.Id;
                                child.DocTypeId = field.DocTypeId;
                                child.UniqueId = Guid.NewGuid().ToString();

                                docFieldMetadataDao.Add(child);
                            }
                        }

                        ActionLogHelper.AddActionLog("Create document field meta data successfully",
                                                        LoginUser, ActionName.AddFieldMetaData, null, null, dataContext);
                    }
                }
                else
                {
                    docTypeDao.Update(docType);
                    foreach (var removeFieldId in docType.DeletedFields)
                    {
                        ocrTemplateZoneDao.Delete(removeFieldId);
                        picklistDao.DeleteByField(removeFieldId);
                        backupDocumentFieldValueDao.DeleteByField(removeFieldId);
                        docFieldValueDao.DeleteByField(removeFieldId);
                        tableValueDao.DeleteByParentField(removeFieldId);
                        documentFieldPermissionDao.DeleteByField(removeFieldId);

                        ActionLogHelper.AddActionLog("Delete document field value successfully",
                                                       LoginUser, ActionName.DeleteFieldValue, null, null, dataContext);
                        docFieldMetadataDao.DeleteChildren(removeFieldId);
                        docFieldMetadataDao.Delete(removeFieldId);
                        ActionLogHelper.AddActionLog("Delete document field meta data successfully",
                                                      LoginUser, ActionName.DeleteFieldMetaData, null, null, dataContext);
                    }

                    foreach (var field in docType.Fields.Where(h => !h.IsSystemField))
                    {
                        if (field.Id == Guid.Empty)
                        {
                            field.DocTypeId = docType.Id;
                            docFieldMetadataDao.Add(field);


                            if (field.Picklists != null && field.Picklists.Count > 0)
                            {
                                foreach (Picklist picklist in field.Picklists)
                                {
                                    picklist.FieldId = field.Id;
                                    picklistDao.Add(picklist);
                                }
                            }

                            if (field.Children != null && field.Children.Count > 0)
                            {
                                foreach (var child in field.Children)
                                {
                                    child.ParentFieldId = field.Id;
                                    child.DocTypeId = field.DocTypeId;
                                    child.UniqueId = Guid.NewGuid().ToString();

                                    docFieldMetadataDao.Add(child);
                                }
                            }
                            ActionLogHelper.AddActionLog("Create document field meta data successfully",
                                                          LoginUser, ActionName.AddFieldMetaData, null, null, dataContext);
                        }
                        else
                        {
                            if (field.DeleteChildIds != null)
                            {
                                field.DeleteChildIds.ForEach(docFieldMetadataDao.Delete);
                            }

                            if (field.DataType != "Table" && field.Children.Count > 0)
                            {
                                docFieldMetadataDao.DeleteChildren(field.Id);
                            }


                            if (field.Picklists != null && field.Picklists.Count > 0)
                            {
                                picklistDao.DeleteByField(field.Id);

                                foreach (Picklist picklist in field.Picklists)
                                {
                                    picklist.FieldId = field.Id;
                                    picklistDao.Add(picklist);
                                }
                            }

                            docFieldMetadataDao.Update(field);

                            if (field.Children != null && field.Children.Count > 0)
                            {
                                foreach (var child in field.Children)
                                {
                                    if (child.Id == Guid.Empty)
                                    {
                                        child.DocTypeId = field.DocTypeId;
                                        child.UniqueId = Guid.NewGuid().ToString();
                                        child.ParentFieldId = field.Id;
                                        docFieldMetadataDao.Add(child);
                                    }
                                    else
                                    {
                                        docFieldMetadataDao.Update(child);
                                    }
                                }
                            }

                            ActionLogHelper.AddActionLog("Update document field meta data successfully",
                                                          LoginUser, ActionName.UpdateFieldMetaData, null, null, dataContext);
                        }
                    }
                }
            }

            batchTypeDao.Update(batchType);
            ActionLogHelper.AddActionLog("Update batch type name: " + batchType.Name + " successfully",
                                          LoginUser, ActionName.UpdateBatchType, null, null, dataContext);
        }

        private OCRTemplate GetOcrTemplate(Guid docTypeId, DapperContext context)
        {
            OCRTemplateDao ocrTemplateDao = new OCRTemplateDao(context);
            OCRTemplatePageDao ocrTemplatePageDao = new OCRTemplatePageDao(context);
            OCRTemplateZoneDao ocrTemplateZoneDao = new OCRTemplateZoneDao(context);
            DocFieldMetaDataDao docFieldDao = new DocFieldMetaDataDao(context);

            OCRTemplate ocrTemplate = ocrTemplateDao.GetById(docTypeId);

            if (ocrTemplate != null)
            {
                using (Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    LanguageDao languageDao = new LanguageDao(primaryContext);
                    ocrTemplate.Language = GetLanguage(languageDao.GetById(ocrTemplate.LanguageId));
                }

                ocrTemplate.OCRTemplatePages.AddRange(ocrTemplatePageDao.GetByOCRTemplate(docTypeId));
                if (ocrTemplate.OCRTemplatePages.Count > 0)
                {
                    foreach (OCRTemplatePage ocrTemplatePage in ocrTemplate.OCRTemplatePages)
                    {
                        List<OCRTemplateZone> templateZones = ocrTemplateZoneDao.GetByOCRTemplatePage(ocrTemplatePage.Id);

                        foreach (OCRTemplateZone zone in templateZones)
                        {
                            zone.FieldMetaData = docFieldDao.GetById(zone.FieldMetaDataId);
                        }

                        ocrTemplatePage.OCRTemplateZones.AddRange(templateZones);
                    }
                }
            }

            return ocrTemplate;
        }

        private BatchTypePermission GetBatchTypePermission(DapperContext context, Guid batchTypeId)
        {
            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {
                List<Guid> groupIds = null;
                groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();

                List<BatchTypePermission> permissions = new BatchTypePermissionDao(context).GetByUser(groupIds, batchTypeId);
                BatchTypePermission permission = new BatchTypePermission();

                for (int i = 0; i < permissions.Count; i++)
                {
                    var per = permissions[i];
                    if (i == 0)
                    {
                        permission = per;
                    }
                    else
                    {
                        permission.CanCapture |= per.CanCapture;

                        // 2014/04/01 - HungLe - Start - Adding set permission for CanAccess, CanIndex, CanClassify
                        permission.CanAccess |= per.CanAccess;
                        permission.CanIndex |= per.CanIndex;
                        permission.CanClassify |= per.CanClassify;
                        // 2014/04/01 - HungLe - End - Adding set permission for CanAccess, CanIndex, CanClassify
                    }
                }

                return permission;
            }
        }

        private DocumentTypePermission GetDocumentTypePermission(DapperContext context, Guid docTypeId)
        {
            List<Guid> groupIds = null;

            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {
                groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
            }

            DocumentTypePermission permission = new DocumentTypePermission();
            List<DocumentTypePermission> permissions = new DocumentTypePermissionDao(context).GetByGroupRangeAndDocType(groupIds, docTypeId);

            for (int i = 0; i < permissions.Count; i++)
            {
                var per = permissions[i];
                if (i == 0)
                {
                    permission = per;
                }
                else
                {
                    permission.CanAccess |= per.CanAccess;
                }
            }

            return permission;
        }

        //public List<DocumentFieldPermission> GetFieldPermissionByUserAndDocType(DapperContext context, Guid docTypeId)
        //{
        //    List<Guid> groupIds = null;

        //    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
        //    {
        //        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
        //    }

        //    List<DocumentFieldPermission> permissions = new DocumentFieldPermissionDao(context).GetByUserAndDocType(groupIds, docTypeId);
        //    List<DocumentFieldPermission> permissionByUsers = new List<DocumentFieldPermission>();

        //    int count = 0;

        //    foreach (Guid groupId in groupIds)
        //    {
        //        DocumentFieldPermission permission = new DocumentFieldPermission();
        //        List<DocumentFieldPermission> permissionByGroups = permissions.Where(p => p.UserGroupId == groupId).ToList();
        //        var permis = permissionByGroups;

        //        if (count == 0)
        //        {
        //            permissionByUsers = permis;
        //        }
        //        else
        //        {
        //            for (int i = 0; i < permissionByGroups.Count; i++)
        //            {
        //                var per = permissionByGroups[i];
        //                if (i == 0)
        //                {
        //                    permission = per;
        //                }
        //                else
        //                {
        //                    permission.CanRead |= per.CanRead;
        //                    permission.CanWrite |= per.CanWrite;
        //                    permission.Hidden |= per.Hidden;
        //                }

        //            }
        //        }
        //        permissionByUsers.Add(permission);
        //    }

        //    return permissionByUsers;
        //}

        //public DocumentFieldPermission GetFieldPermissionByUser(DapperContext context, Guid FieldId)
        //{
        //    List<Guid> groupIds = null;

        //    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
        //    {
        //        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
        //    }

        //    List<DocumentFieldPermission> permissions = new DocumentFieldPermissionDao(context).GetByUser(groupIds, FieldId);
        //    DocumentFieldPermission permission = new DocumentFieldPermission();

        //    for (int i = 0; i < permissions.Count; i++)
        //    {
        //        var per = permissions[i];
        //        if (i == 0)
        //        {
        //            permission = per;
        //        }
        //        else
        //        {
        //            permission.CanRead |= per.CanRead;
        //            permission.CanWrite |= per.CanWrite;
        //            permission.Hidden |= per.Hidden;
        //        }
        //    }

        //    return permission;
        //}

        //public List<DocumentFieldPermission> GetFieldPermissions(Guid userGroupId, Guid docTypeId)
        //{
        //    using (DapperContext dataContext = new DapperContext(LoginUser))
        //    {
        //        DocumentFieldPermissionDao permissionDao = new DocumentFieldPermissionDao(dataContext);
        //        return permissionDao.GetFieldPermissions(docTypeId, userGroupId);
        //    }
        //}

        #endregion

        #region Mobile

        /// <summary>
        /// Get list Batch Type associate with user
        /// </summary>
        /// <returns></returns>
        public List<BatchType> GetAssignedBatchTypes()
        {
            var batchTypes = new List<BatchType>();

            using (DapperContext context = new DapperContext(LoginUser))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(context);
                List<Guid> groupIds = null;

                // Get list batch type associate with user
                if (LoginUser.IsAdmin)
                {
                    // Get by role admin
                    batchTypes = batchTypeDao.GetCapturedBatchTypes();
                }
                else
                {
                    // Get by group user if do not have role admin
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                    }
                    batchTypes = batchTypeDao.GetAssignedBatchTypes(groupIds);
                }

                foreach (BatchType batchType in batchTypes)
                {
                    BatchFieldMetaDataDao batchFieldMetaDataDao = new BatchFieldMetaDataDao(context);

                    batchType.Fields = batchFieldMetaDataDao.GetByBatchType(batchType.Id);

                    foreach (BatchFieldMetaData field in batchType.Fields)
                    {
                        if (field.IsLookup && !string.IsNullOrEmpty(field.LookupXml))
                        {
                            field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXml);
                        }
                    }

                    ActionLogHelper.AddActionLog("Get batch field meta data for batch type name: " + batchType.Name + " successfully",
                        LoginUser, ActionName.GetBatchFieldMetaData, null, null, context);

                    if (!string.IsNullOrEmpty(batchType.BarcodeConfigurationXml))
                    {
                        batchType.BarcodeConfiguration = Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(batchType.BarcodeConfigurationXml);
                    }

                    if (LoginUser.IsAdmin)
                    {
                        batchType.BatchTypePermission = BatchTypePermission.GetAllowAll();
                    }
                    else
                    {
                        batchType.BatchTypePermission = GetBatchTypePermission(context, batchType.Id);
                    }
                }

                ActionLogHelper.AddActionLog("Get list of Batch Type by user name: " + LoginUser.UserName + " successfully",
                                             LoginUser, ActionName.GetBatchType, null, null, context);

                return batchTypes;
            }
        }

        #endregion

        #region Mvc

        /// <summary>
        /// Check the user can access this work BatchType.
        /// </summary>
        /// <param name="captureContext">Current capture context.</param>
        /// <param name="batchTypeId">Id of BatchType.</param>
        /// <returns></returns>
        /// <remarks>Use for Capture client.</remarks>
        public bool CanAccessWorkBatchType(DapperContext captureContext, Guid batchTypeId)
        {
            // Check batch type is existed
            var batchType = new BatchTypeDao(captureContext).GetById(batchTypeId);
            if (batchType == null || !batchType.IsWorkflowDefined)
            {
                return false;
            }

            List<Guid> userGroupIds;
            // Get the group Ids of user
            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {
                userGroupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(h => h.Id).ToList();
            }

            // Get permissions of all group which user is in
            var permissions = new BatchTypePermissionDao(captureContext).GetByUser(userGroupIds, batchTypeId);

            // One group has access permission => user has access permission
            for (int i = 0; i < permissions.Count; i++)
            {
                if (permissions[i].CanAccess)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
