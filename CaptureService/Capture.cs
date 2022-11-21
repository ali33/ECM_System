using System;
using System.Collections.Generic;
using System.Security;
using System.ServiceModel;
using Ecm.CaptureCore;
using Ecm.CaptureDomain;
using Ecm.CaptureService.Properties;
using Ecm.Service.Contract;
using Ecm.Utility;
using log4net;
using SecurityManager = Ecm.CaptureCore.SecurityManager;
using System.Data;
using Ecm.BarcodeDomain;
using Ecm.LookupDomain;
using System.Linq;

namespace Ecm.CaptureService
{
    [CredentialsExtractor]
    [UserContextBehavior]
    public class Capture : ICapture
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(Capture));

        private Exception ProcessException(Exception ex, string errorMessage)
        {
            _log.Error(errorMessage, ex);
            return new FaultException(errorMessage);
        }

        #region Security

        public User Login(string userName, string encryptedPassword, string clientHost)
        {
            try
            {
                string decryptedPassword = CryptographyHelper.DecryptUsingSymmetricAlgorithm(encryptedPassword);
                return new SecurityManager().Login(userName, decryptedPassword, clientHost);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.AuthenticateUserFail);
            }
        }

        public User AuthoriseUser(string username, string password)
        {
            try
            {
                return new SecurityManager().AuthorizeUser(username, password);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.AuthenticateUserFail);
            }
        }

        public User ChangePassword(string userName, string oldEncryptedPassword, string newEncryptedPassword)
        {
            try
            {
                return new SecurityManager(UserContext.Current.User).ChangePassword(oldEncryptedPassword, newEncryptedPassword);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.ChangePasswordFail);
            }
        }

        public void ResetPassword(string username)
        {
            try
            {
                new SecurityManager().ResetPassword(username);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.ResetPasswordFail);
            }
        }

        #endregion

        #region Logging

        public void Log(string message, string stackTrace)
        {
            _log.Error(message + Environment.NewLine + stackTrace);
        }

        #endregion

        #region User

        public List<User> GetUsers()
        {
            try
            {
                var workflowId = new Guid("40194b17-1418-42be-ad8f-e1e52cb771d3");
                return new UserManager(UserContext.Current.User).GetUsers().Where(h => h.Id != workflowId).ToList();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }

        public User GetUser(string username, string passwordHash)
        {
            try
            {
                return new UserManager(UserContext.Current.User).GetUser(username, passwordHash);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }

        public User GetUserByUserName(string username)
        {
            try
            {
                return new UserManager(UserContext.Current.User).GetUserByUserName(username);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }

        public void SaveUser(User user)
        {
            try
            {
                new UserManager(UserContext.Current.User).Save(user);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveUserFail);
            }
        }

        public void DeleteUser(User user)
        {
            try
            {
                new UserManager(UserContext.Current.User).Delete(user);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteUserFail);
            }
        }

        public List<User> GetAvailableUserToDelegation()
        {
            try
            {
                return new UserManager(UserContext.Current.User).GetAvailableUserToDelegation();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }
        #endregion

        #region User group

        public List<UserGroup> GetUserGroups()
        {
            try
            {
                return new UserGroupManager(UserContext.Current.User).GetAll();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUserGroupsFail);
            }
        }

        public void SaveUserGroup(UserGroup userGroup)
        {
            try
            {
                Guid returnValue = new UserGroupManager(UserContext.Current.User).Save(userGroup);

                if (returnValue == null || returnValue == Guid.Empty)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveUserGroupFail);
            }
        }

        public void DeleteUserGroup(UserGroup userGroup)
        {
            try
            {
                new UserGroupManager(UserContext.Current.User).Delete(userGroup);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteUserGroupFail);
            }
        }

        #endregion

        #region Language

        public Language GetLanguage(Guid id)
        {
            try
            {
                return new LanguageManager(UserContext.Current.User).GetLanguage(id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetLanguageDataFail);
            }
        }

        public List<Language> GetLanguages()
        {
            try
            {
                return new LanguageManager(UserContext.Current.User).GetLanguages();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetLanguageDataFail);
            }
        }

        #endregion

        #region Permission

        public List<BatchType> GetBatchTypesUnderPermissionConfiguration()
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetBatchTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        public List<UserGroup> GetUserGroupsUnderPermissionConfiguration()
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetUserGroups();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUserGroupsFail);
            }
        }

        public BatchTypePermission GetBatchTypePermission(Guid userGroupId, Guid batchTypeId)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetBatchPermision(userGroupId, batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypePermissionFail);
            }
        }

        public void SavePermission(BatchTypePermission batchTypePermission, List<DocumentTypePermission> documentTypePermissions)
        {
            try
            {
                new PermissionManager(UserContext.Current.User).SavePermission(batchTypePermission, documentTypePermissions);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBatchTypePermissionFail);
            }
        }

        public DocumentTypePermission GetDocumentTypePermission(Guid userGroupId, Guid docTypeId)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetDocumentTypePermission(userGroupId, docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBatchTypePermissionFail);
            }
        }

        public DocumentTypePermission GetDocumentTypePermissionByUser(Guid userId, Guid docTypeId)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetDocumentTypePermissionByUser(userId, docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBatchTypePermissionFail);
            }
        }

        public List<DocumentFieldPermission> GetFieldPermission(Guid userGroupId, Guid docTypeId)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetFieldPermission(userGroupId, docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBatchTypePermissionFail);
            }
        }
        #endregion

        #region Batch type

        public BatchType GetBatchType(Guid batchTypeId)
        {
            try
            {
                return new BatchTypeManager(UserContext.Current.User).GetBatchType(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        public List<BatchType> GetCaptureBatchType()
        {
            try
            {
                return new BatchTypeManager(UserContext.Current.User).GetCapturedBatchTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        public List<BatchType> GetBatchTypes()
        {
            try
            {
                return new BatchTypeManager(UserContext.Current.User).GetAllBatchTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        public void DeleteBatchType(Guid batchTypeId)
        {
            try
            {
                new BatchTypeManager(UserContext.Current.User).Delete(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteBatchTypeFail);
            }
        }

        public void SaveBatchType(BatchType batchType)
        {
            try
            {
                new BatchTypeManager(UserContext.Current.User).Save(batchType);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBatchTypeFail);
            }
        }

        public List<BatchType> GetAssignWorkBatchTypes()
        {
            try
            {
                return new BatchTypeManager(UserContext.Current.User).GetAssignWorkBatchTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        public bool CanEditBatchTypeField(Guid batchTypeId)
        {
            try
            {
                return new BatchTypeManager(UserContext.Current.User).CanEditBatchTypeField(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.CanEditBatchTypeFieldFail);
            }
        }

        #endregion

        #region Workflow

        public WorkflowDefinition GetWorkflowByBatchTypeId(Guid batchTypeId)
        {
            try
            {
                return new WorkflowManager(UserContext.Current.User).GetWorkflowByBatchTypeId(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkflowDataFail);
            }
        }

        public Guid SaveWorkflowDefinition(Guid batchTypeId, WorkflowDefinition wfDefinition, List<CustomActivitySetting> customActivitySettings)
        {
            try
            {
                return new WorkflowManager(UserContext.Current.User).SaveWorkflowDefinition(batchTypeId, wfDefinition, customActivitySettings);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveWorkflowDefinitionFail);
            }
        }
        //public Guid SaveWorkflowDefinition(Guid batchTypeId, WorkflowDefinition wfDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings)
        //{
        //    try
        //    {
        //        return new WorkflowManager(UserContext.Current.User).SaveWorkflowDefinition(batchTypeId, wfDefinition, humanStepPermissions, customActivitySettings);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.SaveWorkflowDefinitionFail);
        //    }
        //}

        //public List<HumanStepPermission> GetWorkflowHumanStepPermissions(Guid workflowDefinitionID)
        //{
        //    try
        //    {
        //        return new WorkflowManager(UserContext.Current.User).GetWorkflowHumanStepPermissions(workflowDefinitionID);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetWorkflowHumanStepPermissionsFail);
        //    }
        //}

        public CustomActivitySetting GetCustomActivitySetting(Guid wfDefinitionId, Guid activityId)
        {
            try
            {
                return new CustomActivitySettingManager(UserContext.Current.User).GetCustomActivitySetting(wfDefinitionId, activityId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetCustonActivitySettingFail);
            }
        }

        public List<CustomActivitySetting> GetCustomActivitySettings(Guid wfDefinitionId)
        {
            try
            {
                return new CustomActivitySettingManager(UserContext.Current.User).GetCustomActivitySettings(wfDefinitionId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetCustonActivitySettingFail);
            }
        }

        #endregion

        #region Work Item

        public void InsertBatch(Batch batch)
        {
            try
            {
                User user = UserContext.Current.User;
                new BatchManager(user).InsertBatch(batch);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.InsertWorkItemFail);
            }
        }

        public void UpdateWorkItem(Batch workItem)
        {
            try
            {
                User user = UserContext.Current.User;
                new BatchManager(user).UpdateBatch(workItem, true);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                else if (ex is InvalidTransactionIdException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.UpdateWorkItemFail);
            }
        }

        public void SaveWorkItem(Batch workItem)
        {
            try
            {
                User user = UserContext.Current.User;
                new BatchManager(user).UpdateBatch(workItem, false);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                else if (ex is InvalidTransactionIdException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.UpdateWorkItemFail);
            }
        }

        public WorkItemSearchResult RunAdvanceSearchWorkItem(Guid batchTypeId, SearchQuery searchQuery, int pageIndex)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).RunAdvanceSearchWorkItem(batchTypeId, searchQuery, pageIndex);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.RunAdvanceSearchWorkItemFail);
            }
        }

        //public WorkItemSearchResult RunAdvanceSearchWorkItem(Guid batchTypeId, string searchQuery, int pageIndex)
        //{
        //    try
        //    {
        //        return new BatchManager(UserContext.Current.User).RunAdvanceSearchWorkItem(batchTypeId, searchQuery, pageIndex);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.RunAdvanceSearchWorkItemFail);
        //    }
        //}

        public void ApproveWorkItems(List<Batch> batchs)
        {
            try
            {
                new BatchManager(UserContext.Current.User).ApproveBatchs(batchs);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                else if (ex is InvalidTransactionIdException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.ApproveWorkItemsFail);
            }
        }

        public void ApproveWorkItems(List<Guid> ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).ApproveBatchs(ids);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                else if (ex is InvalidTransactionIdException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                throw ProcessException(ex, ErrorMessages.ApproveWorkItemsFail);
            }
        }

        public void RejectWorkItems(List<Batch> batchs, string rejectNote)
        {
            try
            {
                new BatchManager(UserContext.Current.User).RejectBatchs(batchs, rejectNote);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                else if (ex is InvalidTransactionIdException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.RejectWorkItemsFail);
            }
        }

        public void RejectWorkItems(List<Guid> ids, string rejectNote)
        {
            try
            {
                new BatchManager(UserContext.Current.User).RejectBatchs(ids, rejectNote);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                else if (ex is InvalidTransactionIdException)
                {
                    throw ProcessException(ex, ex.Message);
                }
                throw ProcessException(ex, ErrorMessages.RejectWorkItemsFail);
            }
        }

        public void DelegateWorkItems(List<Guid> ids, string toUser, string delegatedComment)
        {
            try
            {
                new BatchManager(UserContext.Current.User).DelegateBatch(ids, toUser, delegatedComment);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DelegateWorkItemFail);
            }
        }

        public void ResumeWorkItems(List<Guid> Ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).ResumeBatchs(Ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.ResumeWorkItemsFail);
            }
        }

        public void UnLockWorkItems(List<Guid> Ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).UnLockBatchs(Ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.UnLockWorkItemsFail);
            }
        }

        public Batch GetWorkItem(Guid Id)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetBatch(Id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public void DeleteWorkItems(List<Guid> Ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).DeleteBatchs(Ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteWorkItemFail);
            }
        }

        public WorkItemSearchResult GetLockedBatchs(Guid batchTypeId, int pageIndex)
        {
            try
            {
                var batchManager = new BatchManager(UserContext.Current.User);
                return batchManager.GetSearchWorkItems(batchTypeId, pageIndex, BatchStatus.Locked);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public WorkItemSearchResult GetProcessingBatchs(Guid batchTypeId, int pageIndex)
        {
            try
            {
                var batchManager = new BatchManager(UserContext.Current.User);
                return batchManager.GetSearchWorkItems(batchTypeId, pageIndex, BatchStatus.InProcessing);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public WorkItemSearchResult GetErrorBatchs(Guid batchTypeId, int pageIndex)
        {
            try
            {
                var batchManager = new BatchManager(UserContext.Current.User);
                return batchManager.GetSearchWorkItems(batchTypeId, pageIndex, BatchStatus.Error);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public WorkItemSearchResult GetBatchsByBatchType(Guid batchTypeId, int pageIndex)
        {
            try
            {
                var batchManager = new BatchManager(UserContext.Current.User);
                return batchManager.GetSearchWorkItems(batchTypeId, pageIndex, BatchStatus.Available);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public WorkItemSearchResult GetRejectedBatch(Guid batchTypeId, int pageIndex)
        {
            try
            {
                var batchManager = new BatchManager(UserContext.Current.User);
                return batchManager.GetSearchWorkItems(batchTypeId, pageIndex, BatchStatus.Reject);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public void CountBatchs(Guid batchTypeId, out int errorBatchCount, out int inProcessingBatchCount, out int lockedBatchCount, out int availableBatchCount, out int rejectedBatchCount)
        {
            try
            {
                new BatchManager(UserContext.Current.User).CountBatchs(batchTypeId, out errorBatchCount, out inProcessingBatchCount, out lockedBatchCount, out availableBatchCount, out rejectedBatchCount);
            }
            catch (Exception ex)
            {
                errorBatchCount = 0;
                inProcessingBatchCount = 0;
                lockedBatchCount = 0;
                availableBatchCount = 0;
                rejectedBatchCount = 0;
                ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public Batch OpenWorkItem(Guid id)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).OpenWorkItem(id);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Work item is have no longer existed.")
                {
                    throw ProcessException(ex, ex.Message);
                }
                else
                {
                    throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
                }
            }
        }

        public List<Batch> OpenWorkItems(List<Guid> ids)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).OpenWorkItems(ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public void LockBatchs(List<Guid> ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).LockBatchs(ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public void UnlockBatchs(List<Guid> ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).UnLockBatchs(ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public List<Batch> GetBatchs(List<Guid> ids)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetBatchs(ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public List<Batch> GetBatchs(Guid batchTypeId)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetBatchs(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        #endregion

        #region OCR template

        public void SaveOcrTemplate(OCRTemplate ocrTemplate)
        {
            try
            {
                new DocumentTypeManager(UserContext.Current.User).SaveOCRTemplate(ocrTemplate);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveOCRTemplateFail);
            }
        }

        public void DeleteOcrTemplate(Guid docTypeId)
        {
            try
            {
                new DocumentTypeManager(UserContext.Current.User).DeleteOCRTemplate(docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteOCRTemplateFail);
            }
        }

        #endregion

        #region Barcode

        //public List<BarcodeConfiguration> GetBarcodeConfigurations(long docTypeId)
        //{
        //    try
        //    {
        //        return new DocumentTypeManager(UserContext.Current.User).GetBarcodeConfigurations(docTypeId);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetBarcodeConfigurationFail);
        //    }
        //}

        public void SaveBarcodeConfiguration(string xml, Guid batchTypeId)
        {
            try
            {
                new BatchTypeManager(UserContext.Current.User).SaveBarcodeConfiguration(xml, batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBarcodeConfigurationFail);
            }
        }

        //public void DeleteBarcodeConfiguration(long configurationId)
        //{
        //    try
        //    {
        //        new DocumentTypeManager(UserContext.Current.User).DeleteBarcodeConfiguration(configurationId);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.DeleteBarcodeConfigurationFail);
        //    }
        //}

        //public void ClearBarcodeConfigurations(long docTypeId)
        //{
        //    try
        //    {
        //        new DocumentTypeManager(UserContext.Current.User).ClearBarcodeConfigurations(docTypeId);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.DeleteBarcodeConfigurationFail);
        //    }
        //}

        #endregion

        #region Lookup

        public DataTable GetLookupData(LookupInfo lookupInfo, Dictionary<string, string> mappingValue)
        {
            try
            {
                return new LookupManager().GetLookupData(lookupInfo, mappingValue);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupDataFail);
            }

            return null;
        }

        public DataTable GetLookupData(LookupInfo lookupInfo, string value)
        {
            try
            {
                return new LookupManager().GetLookupData(lookupInfo, value);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupDataFail);
            }

            return null;
        }

        public bool TestConnection(ConnectionInfo connectionInfo)
        {
            try
            {
                return new LookupManager().TestConnection(connectionInfo);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.TestConnectionFail);
            }

            return false;
        }

        public bool TestQueryParam(string query, List<string> fieldNames)
        {
            try
            {
                return new LookupManager().TestQueryParam(query, fieldNames);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.TestConnectionFail);
            }

            return false;
        }

        public List<string> GetDatabaseNames(ConnectionInfo connectionInfo)
        {
            try
            {
                return new LookupManager().GetDatabaseNames(connectionInfo);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetDatabaseNameFail);
            }

            return null;
        }

        public List<string> GetSchemas(ConnectionInfo connectionInfo)
        {
            try
            {
                return new LookupManager().GetDatabaseSchemas(connectionInfo);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetDBSchemaFail);
            }

            return null;
        }

        public List<string> GetTableNames(ConnectionInfo connectionInfo)
        {
            try
            {
                return new LookupManager().GetTableNames(connectionInfo);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupDatasourceFail);
            }

            return null;
        }

        public List<string> GetViewNames(ConnectionInfo connectionInfo)
        {
            try
            {
                return new LookupManager().GetViewNames(connectionInfo);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupDatasourceFail);
            }

            return null;
        }

        public List<string> GetStoredProcedureNames(Ecm.LookupDomain.ConnectionInfo connectionInfo)
        {
            try
            {
                return new LookupManager().GetStoredProcedureNames(connectionInfo);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupDatasourceFail);
            }

            return null;
        }

        public List<string> GetRuntimeValueParams(string sqlCommand)
        {
            try
            {
                return new LookupManager().GetRuntimeValueParams(sqlCommand);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupDataParameterFail);
            }

            return null;
        }

        public DataTable GetParameterNames(ConnectionInfo connectionInfo, string storedName)
        {
            try
            {
                return new LookupManager().GetParameters(connectionInfo, storedName);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetParameterFail);
            }

            return null;
        }

        public Dictionary<string, string> GetColummnNames(ConnectionInfo connectionInfo, string sourceName, LookupDataSourceType sourceType)
        {
            try
            {
                return new LookupManager().GetColumns(connectionInfo, sourceName, sourceType);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetColumnFail);
            }

            return null;
        }

        public void UpdateBatchLookup(Guid fieldId, string xml, Guid? lookupActivityId)
        {
            try
            {
                new LookupManager().UpdateBatchLookup(fieldId, xml, UserContext.Current.User, lookupActivityId);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.UpdateLookupInfoFail);
            }
        }

        public void UpdateDocumentLookup(Guid fieldId, string xml, Guid? lookupActivityId)
        {
            try
            {
                new LookupManager().UpdateDocumentLookup(fieldId, xml, UserContext.Current.User, lookupActivityId);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.UpdateLookupInfoFail);
            }
        }

        #endregion

        public void Ping()
        {

        }

        #region Search Query

        public Guid SaveQuery(SearchQuery searchQuery)
        {
            try
            {
                new SearchQueryManager(UserContext.Current.User).Save(searchQuery);
                return searchQuery.Id;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveSearchQuery);
            }
        }

        public bool QueryExisted(Guid batchTypeId, string queryName)
        {
            try
            {
                return new SearchQueryManager(UserContext.Current.User).QueryExisted(batchTypeId, queryName);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

        public List<SearchQuery> GetSavedQueries(Guid batchTypeId)
        {
            try
            {
                return new SearchQueryManager(UserContext.Current.User).GetSavedQueries(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

        public void DeleteQuery(Guid queryId)
        {
            try
            {
                new SearchQueryManager(UserContext.Current.User).DeleteQuery(queryId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSearchQuery);
            }
        }


        #endregion

        #region Ambiguous definitions

        public List<AmbiguousDefinition> GetAmbiguousDefinitions(Guid languageId)
        {
            try
            {
                return new AmbiguousDefinitionManager(UserContext.Current.User).GetAmbiguousDefinitions(languageId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetAmbiguousDefinitionFail);
            }
        }

        public List<AmbiguousDefinition> GetAllAmbiguousDefinitions()
        {
            try
            {
                return new AmbiguousDefinitionManager(UserContext.Current.User).GetAllAmbiguousDefinitions();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetAmbiguousDefinitionFail);
            }
        }

        public void DeleteAmbiguousDefinition(Guid Id)
        {
            try
            {
                new AmbiguousDefinitionManager(UserContext.Current.User).Delete(Id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteAmbiguousDefinitionFail);
            }
        }

        public void SaveAmbiguousDefinition(AmbiguousDefinition ambiguousDefinition)
        {
            try
            {
                new AmbiguousDefinitionManager(UserContext.Current.User).Save(ambiguousDefinition);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveAmbiguousDefinitionFail);
            }
        }

        #endregion

        #region Doc Type
        public List<DocumentType> GetDocumentTypes(Guid batchTypeId)
        {
            try
            {
                return new DocumentTypeManager(UserContext.Current.User).GetDocumentTypes(batchTypeId);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetDocTypeFail);
            }

            return null;
        }

        public DocumentType GetDocumentType(Guid docTypeId)
        {
            try
            {
                return new DocumentTypeManager(UserContext.Current.User).GetDocumentType(docTypeId);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetDocTypeFail);
            }

            return null;
        }

        /// <summary>
        /// Check list document type whether have document or not.
        /// </summary>
        /// <param name="docTypeIds">List document type id need to check</param>
        /// <returns>List of document type id which have document</returns>
        public List<Guid> CheckDocTypeHaveDocument(List<Guid> docTypeId)
        {
            try
            {
                return new DocumentTypeManager(UserContext.Current.User).CheckDocTypeHaveDocument(docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.CanEditDocTypeFieldFail);
            }
        }

        #endregion

        #region Action Log
        public ActionLog GetActionLog(Guid id)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetActionLog(id);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetActionLogFail);
            }

            return null;
        }

        public List<ActionLog> GetActionLogs(int pageIndex, int pageSize, out long totalItems)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetActionLogs(pageIndex, pageSize, out totalItems);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
            totalItems = 0;
            return null;
        }

        public List<ActionLog> GetActionLogAll()
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetActionLogs();
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetActionLogFail);
            }

            return null;
        }

        public List<ActionLog> SearchActionLogs(string expression, int pageIndex, int pageSize, out long totalItems)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).SearchActionLogs(expression, pageIndex, pageSize, out totalItems);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
            totalItems = 0;
            return null;
        }

        public List<ActionLog> SearchActionLogs(string expression)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).SearchActionLogs(expression);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetActionLogFail);
            }

            return null;
        }

        public void DeleteLog(Guid id)
        {
            try
            {
                new ActionLogManager(UserContext.Current.User).DeleteLog(id);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.DeleteActionLogFail);
            }
        }

        public void AddActionLog(string message, ActionName actionName, ObjectType? type, Guid? objectId)
        {
            try
            {
                new ActionLogManager(UserContext.Current.User).AddLog(message, UserContext.Current.User, actionName, type, objectId);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetDocTypeFail);
            }
        }
        #endregion

        #region Setting
        public Setting GetSettings()
        {
            try
            {
                return new SettingManager(UserContext.Current.User).GetSettings();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSettingFail);
            }
        }

        public void WriteSetting(Setting setting)
        {
            try
            {
                new SettingManager(UserContext.Current.User).WriteSetting(setting);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.WriteSettingFail);
            }
        }
        #endregion

        #region Mvc

        #region Search query

        /// <summary>
        /// Get list saved SearchQuery (just get information in table SearchQuery).
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        public List<SearchQuery> GetSavedQueriesLight(Guid batchTypeId)
        {
            try
            {
                return new SearchQueryManager(UserContext.Current.User).GetSavedQueriesLight(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

        /// <summary>
        /// Get saved SearchQuery (exclude information of BatchType).
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public SearchQuery GetSavedQuery(Guid queryId)
        {
            try
            {
                return new SearchQueryManager(UserContext.Current.User).GetSavedQuery(queryId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

        #endregion

        /// <summary>
        /// Get list field of Batch Type.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        public List<BatchFieldMetaData> GetFieldsFromBatchType(Guid batchTypeId)
        {
            try
            {
                return new BatchFieldMetaDataManager(UserContext.Current.User).GetByBatchType(batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

        #region Work item

        /// <summary>
        /// Get the result of normal search by batch status for MVC version.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public WorkItemSearchResult GetBatches(Guid batchTypeId, BatchStatus status, int pageIndex)
        {
            try
            {
                // Get all result
                var result = new BatchManager(UserContext.Current.User).GetSearchWorkItems(batchTypeId, pageIndex, status);

                var totalPages = 0;
                var pagingHelper = new PagingHelper(UserContext.Current.User);

                // Paging result
                result.WorkItems = pagingHelper.PagingMvc(result.WorkItems, ref pageIndex, out totalPages);
                result.PageIndex = pageIndex;
                result.HasMoreResult = totalPages > 0 && pageIndex < totalPages;

                return result;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.RunAdvanceSearchWorkItemFail);
            }
        }

        /// <summary>
        /// Get the result of advanced search for MVC version.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public WorkItemSearchResult GetAdvancedSearchWorkItems(SearchQuery searchQuery, int pageIndex)
        {
            try
            {
                // Get all result
                var result = new BatchManager(UserContext.Current.User).RunAdvanceSearchWorkItem(searchQuery.BatchTypeId, searchQuery, pageIndex);

                var totalPages = 0;
                var pagingHelper = new PagingHelper(UserContext.Current.User);

                // Paging result
                result.WorkItems = pagingHelper.PagingMvc(result.WorkItems, ref pageIndex, out totalPages);
                result.PageIndex = pageIndex;
                result.HasMoreResult = totalPages > 0 && pageIndex < totalPages;

                return result;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.RunAdvanceSearchWorkItemFail);
            }
        }

        #endregion

        #endregion

        public Guid GetTransactionId(Guid batchId)
        {
            try
            {
                User user = UserContext.Current.User;
                return new BatchManager(user).GetTransactionId(batchId);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, "Get transaction ID of batch fail.");
            }
        }
    }
}
