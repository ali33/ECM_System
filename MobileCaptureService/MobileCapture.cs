using System;
using System.Collections.Generic;
using System.Security;
using System.ServiceModel;
using Ecm.CaptureCore;
using Ecm.CaptureDomain;
using Ecm.MobileCaptureService.Properties;
using Ecm.Mobile.Contract;
using Ecm.Utility;
using log4net;
using SecurityManager = Ecm.CaptureCore.SecurityManager;
using System.Data;
using Ecm.BarcodeDomain;
using Ecm.LookupDomain;
using System.ServiceModel.Web;
using System.Net;
using System.Linq;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.IO;
using AntsCode.Util;
using System.Runtime.Serialization.Json;
using System.Text;
using HttpMultipartParser;
using System.Xml.Serialization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using com.pakhee.common;

namespace Ecm.MobileCaptureService
{
    [CredentialsExtractor]
    //[UserContextBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class MobileCapture : IMobileCapture
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MobileCapture));

        private Exception ProcessException(Exception ex, string errorMessage)
        {
            _log.Error(errorMessage, ex);
            return new WebFaultException<string>(errorMessage, HttpStatusCode.BadRequest);
        }

        #region Old code

        #region Security

        public User Login(string userName, string encryptedPassword, string clientHost)
        {
            try
            {
                CryptLib _crypt = new CryptLib();
                String iv = "Lo0bwyhlaB39PB8=";//CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
                string key = CryptLib.getHashSha256("D4A88355-7148-4FF2-A626-151A40F57330", 32); //32 bytes = 256 bits
                string decryptedPassword = _crypt.decrypt(encryptedPassword, key, iv);
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
                return new SecurityManager(UserContext.Current.User).ChangePasswordMobile(oldEncryptedPassword, newEncryptedPassword);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.ChangePasswordFail);
            }
        }

        public void ResetPassword(string username)
        {
            try{
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
                return new UserManager(UserContext.Current.User).GetUsers();
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

        public void ResumeWorkItems(List<Guid> ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).ResumeBatchs(ids);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.ResumeWorkItemsFail);
            }
        }

        public void UnLockWorkItems(List<Guid> ids)
        {
            try
            {
                new BatchManager(UserContext.Current.User).UnLockBatchs(ids);
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

        public Batch OpenWorkItem(Guid id)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).OpenWorkItem(id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
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

        #region Private methods



        #endregion

        #endregion

        #region New code

        public bool Ping()
        {
            return true;
        }

        #region Permission

        /// <summary>
        /// Get permission of batch type by batch type id.
        /// </summary>
        /// <param name="batchTypeId">Id of batch type</param>
        /// <returns></returns>
        public BatchTypePermission GetBatchTypePermissionByBatchType(Guid batchTypeId)
        {
            try
            {
                if (UserContext.Current.User.IsAdmin)
                {
                    return BatchTypePermission.GetAllowAll();
                }

                var permissionManager = new PermissionManager(UserContext.Current.User);
                return permissionManager.GetBatchTypePermissionByUser(UserContext.Current.User.Id, batchTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypePermissionFail);
            }
        }

        #endregion

        #region User

        public List<string> GetUserNames()
        {
            try
            {
                var users = new UserManager(UserContext.Current.User).GetUsers();
                return users.Select(h => h.UserName).ToList();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }

        public List<string> GetUserEmails()
        {
            try
            {
                var users = new UserManager(UserContext.Current.User).GetUsers();
                return users.Select(h => h.Email).ToList();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }

        #endregion

        #region BatchType

        public List<BatchType> GetCaptureBatchTypeMobile()
        {
            try
            {
                var captureBatches = new BatchTypeManager(UserContext.Current.User).GetCapturedBatchTypes();
                foreach (var batch in captureBatches)
                {
                    // Remove system field
                    batch.Fields = batch.Fields.Where(h => !h.IsSystemField).ToList();
                    batch.Icon = null;

                    foreach (var field in batch.Fields)
                    {
                        if (field.IsLookup)
                        {
                            field.LookupInfo = null;
                        }
                    }

                    foreach (var docType in batch.DocTypes)
                    {
                        // Remove icon
                        docType.Icon = null;
                        docType.IconBase64 = null;
                        // Remove OCR
                        docType.OCRTemplate = null;
                        docType.OCRTemplate = null;

                        // Remove system field
                        docType.Fields = docType.Fields.Where(h => !h.IsSystemField).ToList();

                        foreach (var docField in docType.Fields)
                        {
                            if (docField.IsLookup)
                            {
                                docField.LookupInfo = null;
                            }
                        }
                    }
                }

                return captureBatches;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        /// <summary>
        /// Get all assigned batch type associate with current user.
        /// </summary>
        /// <returns></returns>
        public List<BatchType> GetAssignedBatchTypes()
        {
            try
            {
                var captureBatches = new BatchTypeManager(UserContext.Current.User).GetAssignedBatchTypes();
                foreach (var batch in captureBatches)
                {
                    batch.Fields = batch.Fields.Where(h => !h.IsSystemField).ToList();
                    batch.Icon = null;

                    foreach (var docType in batch.DocTypes)
                    {
                        // Remove icon
                        docType.Icon = null;
                        docType.IconBase64 = null;
                        // Remove OCR
                        docType.OCRTemplate = null;
                        docType.OCRTemplate = null;

                        // Remove system field
                        docType.Fields = docType.Fields.Where(h => !h.IsSystemField).ToList();

                        //Linh: Do not apply for capture process. Can see restricted field move to human step permission inside Document Permission.
                        // Remove restricted field if have no see permission
                        //if (!docType.DocumentTypePermission.CanSeeRestrictedField)
                        //{
                        //    docType.Fields = docType.Fields.Where(h => !h.IsRestricted).ToList();
                        //}
                    }
                }

                return captureBatches;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchTypeFail);
            }
        }

        /// <summary>
        /// Get list normal field of Batch Type.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        public List<BatchFieldMetaData> GetNormalFieldsFromBatchType(Guid batchTypeId)
        {
            try
            {
                var allFields = new BatchFieldMetaDataManager(UserContext.Current.User).GetByBatchType(batchTypeId);
                return allFields.Where(h => !h.IsSystemField).ToList();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFieldMetaDataFail);
            }
        }

        #endregion

        #region DocumentType

        /// <summary>
        /// Get list normal field of Document Type.
        /// </summary>
        /// <param name="docTypeId"></param>
        /// <returns></returns>
        public List<DocumentFieldMetaData> GetNormalFieldsFromDocType(Guid docTypeId)
        {
            try
            {
                var allFields = new DocumentFieldMetaDataManager(UserContext.Current.User).GetByDocumentType(docTypeId);
                return allFields.Where(h => !h.IsSystemField).ToList();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocumentFieldMetaDataFail);
            }
        }

        #endregion

        #region Work Item
        public List<string> CheckLock(Guid id)
        {
            Batch batch = new BatchManager(UserContext.Current.User).CheckLock(id);
            List<string> result = new List<string>();
            result.Add(batch.LockedBy);
            result.Add(batch.BlockingActivityName);
            return result;
        }

        /// <summary>
        /// Get all list available Batch associate with login user
        /// </summary>GetAvailableBatchs
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Batch> GetAvailableBatchs(int pageIndex, int itemPerPage)
        {
            try
            {
                List<BatchType> batchTypes;

                // Get all associate batch type with user
                batchTypes = new BatchTypeManager(UserContext.Current.User).GetAssignedBatchTypes();

                var batchManger = new BatchManager(UserContext.Current.User);
                var availableBatchs = new List<Batch>();
                List<Batch> tempBatchs;

                // Get all batch
                foreach (var batchType in batchTypes)
                {
                    tempBatchs = batchManger.GetSearchBatchs(batchType.Id, pageIndex, BatchStatus.Available);
                    availableBatchs.AddRange(tempBatchs);
                }

                // Sorting by new create date
                availableBatchs = availableBatchs.OrderByDescending(h => h.CreatedDate).ToList();

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                availableBatchs = pagingHelper.PagingMobile(availableBatchs, pageIndex, itemPerPage);

                foreach (var batch in availableBatchs)
                {
                    batch.BatchType.Icon = null;
                }


                return availableBatchs;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }
       
        /// <summary>
        /// Get list Pages by Document
        /// </summary>
        /// <param name="batchTypeId">Id of Document</param>
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Page> GetPagesByDocId(Guid docId, int pageIndex, int itemPerPage, out int totalPages)
        {
            List<Page> pages;

            try
            {
                totalPages = 0;

                // Get pages
                pages = new PageManager(UserContext.Current.User).GetPagesByDocId(docId);
                totalPages = pages.Count;

                // Paging
                if (pageIndex > 0)
                {
                    var skipItemCount = (pageIndex - 1) * itemPerPage;
                    pages = pages.Skip(skipItemCount).Take(itemPerPage).ToList();
                }

                // Convert image byte array to Base64 string image
                foreach (var page in pages)
                {
                    if ("tiff".Equals(page.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        using (MemoryStream ms = new MemoryStream(page.FileBinary))
                        {
                            var image = Image.FromStream(ms);

                            using (MemoryStream jpgStream = new MemoryStream())
                            {
                                image.Save(jpgStream, ImageFormat.Jpeg);
                                page.FileBinaryBase64 = Convert.ToBase64String(jpgStream.ToArray());
                                page.FileExtension = "jpeg";
                            }
                        }
                    }
                    else if (null != page.FileBinary)
                    {
                        page.FileBinaryBase64 = Convert.ToBase64String(page.FileBinary);
                    }

                    // Do not use in mobile => use FileBinaryBase64
                    page.FileBinary = null;
                }
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetPageFail);
            }

            return pages;
        }

        /// <summary>
        /// Get list error Batch by BatchType
        /// </summary>
        /// <param name="batchTypeId">ID of BatchType</param>
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Batch> GetErrorBatchesByBatchType(Guid batchTypeId, int pageIndex, int itemPerPage)
        {
            try
            {
                // Get all result
                var workItem = new BatchManager(UserContext.Current.User).GetSearchBatchs(batchTypeId, pageIndex, BatchStatus.Error);

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                var results = pagingHelper.PagingMobile(workItem, pageIndex, itemPerPage);

                return results;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }

        /// <summary>
        /// Get list in-processing Batch by BatchType
        /// </summary>
        /// <param name="batchTypeId">ID of BatchType</param>
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Batch> GetInProcessingBatchesByBatchType(Guid batchTypeId, int pageIndex, int itemPerPage)
        {
            try
            {
                // Get all result
                var workItem = new BatchManager(UserContext.Current.User).GetSearchBatchs(batchTypeId, pageIndex, BatchStatus.InProcessing);

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                return pagingHelper.PagingMobile(workItem, pageIndex, itemPerPage);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }

        /// <summary>
        /// Get list locked Batch by BatchType
        /// </summary>
        /// <param name="batchTypeId">ID of BatchType</param>
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Batch> GetLockedBatchesByBatchType(Guid batchTypeId, int pageIndex, int itemPerPage)
        {
            try
            {
                // Get all result
                var workItem = new BatchManager(UserContext.Current.User).GetSearchBatchs(batchTypeId, pageIndex, BatchStatus.Locked);

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                return pagingHelper.PagingMobile(workItem, pageIndex, itemPerPage);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }

        /// <summary>
        /// Get list rejected Batch by BatchType
        /// </summary>
        /// <param name="batchTypeId">ID of BatchType</param>
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Batch> GetRejectedBatchesByBatchType(Guid batchTypeId, int pageIndex, int itemPerPage)
        {
            try
            {
                // Get all result
                var workItem = new BatchManager(UserContext.Current.User).GetSearchBatchs(batchTypeId, pageIndex, BatchStatus.Reject);

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                return pagingHelper.PagingMobile(workItem, pageIndex, itemPerPage);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }

        /// <summary>
        /// Get list available Batch by BatchType
        /// </summary>
        /// <param name="batchTypeId">ID of BatchType</param>
        /// <param name="pageIndex">The page index which want to get. To get all set page index = 0.</param>
        /// <param name="itemPerPage">Item per page</param>
        /// <returns></returns>
        public List<Batch> GetAvailableBatchesByBatchType(Guid batchTypeId, int pageIndex, int itemPerPage)
        {
            try
            {
                // Get all result
                var workItem = new BatchManager(UserContext.Current.User).GetSearchBatchs(batchTypeId, pageIndex, BatchStatus.Available);

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                return pagingHelper.PagingMobile(workItem, pageIndex, itemPerPage);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }

        /// <summary>
        /// Get the result of advanced search for MVC version.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<Batch> RunAdvancedSearchWorkItem(SearchQuery searchQuery, int pageIndex, int itemPerPage)
        {
            try
            {
                // Get all result
                var workItem = new BatchManager(UserContext.Current.User).RunAdvanceSearchWorkItem(searchQuery.BatchTypeId, searchQuery, pageIndex);

                var pagingHelper = new PagingHelper(UserContext.Current.User);
                // Paging result
                return pagingHelper.PagingMobile(workItem.WorkItems, pageIndex, itemPerPage);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.RunAdvanceSearchWorkItemFail);
            }
        }

        /// <summary>
        /// Insert new batch work item
        /// </summary>
        /// <param name="batch"></param>
        public void InsertWorkBatch(Stream batch)
        {
            try
            {
                MultipartParser parser = new MultipartParser(batch);

                if (parser.Success)
                {
                    var jsonHelper = new JSonHelper();

                    var batchObj = jsonHelper.ConvertJSonToObject<Batch>(new MemoryStream(parser.FileContents));

                    new BatchManager(UserContext.Current.User).InsertBatch(batchObj, true);

                    return;
                }

                throw new Exception("Parse content batch error.");
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.InsertWorkItemFail);
            }
        }

        /// <summary>
        /// Save work item and unlock it.
        /// </summary>
        /// <param name="batch"></param>
        public void SaveWorkBatch(Stream batch)
        {
            try
            {
                MultipartParser parser = new MultipartParser(batch);

                if (parser.Success)
                {
                    var jsonHelper = new JSonHelper();

                    var batchObj = jsonHelper.ConvertJSonToObject<Batch>(new MemoryStream(parser.FileContents));
                    new BatchManager(UserContext.Current.User).UpdateBatch(batchObj, false, true);

                    return;
                }

                throw new Exception("Parse content batch error.");
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.InsertWorkItemFail);
            }
        }

        /// <summary>
        /// Approve work item and unlock it.
        /// </summary>
        /// <param name="batch"></param>
        public void ApproveWorkBatch(Stream batch)
        {
            try
            {
                MultipartParser parser = new MultipartParser(batch);

                if (parser.Success)
                {
                    var jsonHelper = new JSonHelper();

                    var batchObj = jsonHelper.ConvertJSonToObject<Batch>(new MemoryStream(parser.FileContents));
                    new BatchManager(UserContext.Current.User).ApproveBatchs(new List<Batch> { batchObj });//(batchObj, Constants.ACTION_APPROVE_BATCH);

                    return;
                }

                throw new Exception("Parse content batch error.");
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.InsertWorkItemFail);
            }
        }

        /// <summary>
        /// Reject work item and unlock it.
        /// </summary>
        /// <param name="batch"></param>
        public void RejectWorkBatch(Stream batch)
        {
            try
            {
                MultipartParser parser = new MultipartParser(batch);

                if (parser.Success)
                {
                    var jsonHelper = new JSonHelper();

                    var batchObj = jsonHelper.ConvertJSonToObject<Batch>(new MemoryStream(parser.FileContents));
                    new BatchManager(UserContext.Current.User).RejectBatchs(new List<Batch> { batchObj }, "Reject workitem by " + UserContext.Current.User.UserName);

                    return;
                }

                throw new Exception("Parse content batch error.");
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.InsertWorkItemFail);
            }
        }

        /// <summary>
        /// Get simple information of batch by batch id.
        /// </summary>
        /// <param name="id">Id of batch.</param>
        /// <param name="includePageInfo">Set true: include page info in document information.</param>
        /// <returns></returns>
        public Batch GetBatchInfo(Guid id)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetBatchInfo(id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, string.Format(ErrorMessages.GetBatchInfoFail, id));
            }
        }

        /// <summary>
        /// Get information and thumbnail of page by doc id.
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="thumbWidth"></param>
        /// <param name="thumbHeight"></param>
        /// <param name="pageIndex"></param>
        /// <param name="itemPerPage"></param>
        /// <returns></returns>
        public List<Page> GetThumbnailPagesByDoc(Guid docId,
                                                           int thumbWidth, int thumbHeight,
                                                           int pageIndex, int itemPerPage)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetThumbnailPagesByDoc(docId,
                                                                                         thumbWidth, thumbHeight,
                                                                                         pageIndex, itemPerPage);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, string.Format(ErrorMessages.GetPageThumbnailFail, docId));
            }
        }

        /// <summary>
        /// Get page by id (Use in mobile).
        /// </summary>
        /// <param name="id">Id of page</param>
        /// <returns></returns>
        public Page GetWorkPage(Guid id)
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetWorkPage(id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, string.Format(ErrorMessages.GetPageFail, id));
            }

        }

        public Stream GetWorkPageFile(Guid id)
        {
            var page = GetWorkPage(id);
            var jsonHelper = new JSonHelper();
            var pageJson = jsonHelper.ConvertObjectToJSonMemory(page);

            String headerInfo = "attachment; filename=" + id + ".txt";
            WebOperationContext.Current.OutgoingResponse.Headers["Content-Disposition"] = headerInfo;
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
            WebOperationContext.Current.OutgoingResponse.ContentLength = pageJson.Length;

            pageJson.Position = 0;

            return pageJson;
        }

        /// <summary>
        /// Get list name of locked assigned work item which lock by current user.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLockedWorkItemNames()
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetLockedWortItemNames();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex,ErrorMessages.GetWorkItemFail);
            }
        }

        #region Private methods

        #endregion

        #endregion

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

        #region Lookup

        public List<Dictionary<string, object>> GetLookupData(string lookupInfo, string value)
        {
            try
            {
                
                var serializer = new XmlSerializer(typeof(LookupInfo));
                LookupInfo lookupInfoObj;
                using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(lookupInfo)))
                {
                    lookupInfoObj = (LookupInfo)serializer.Deserialize(stream);
                }

                var dataTable = new LookupManager().GetLookupData(lookupInfoObj, value);
                
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row = null;

                foreach (DataRow dr in dataTable.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        if (dr[col] == DBNull.Value)
                        {
                            row.Add(col.ColumnName.Trim(), string.Empty);
                        }
                        else
                        {
                            if (col.DataType == typeof(DateTime) && (DateTime)dr[col] == DateTime.MinValue )
                            {
                                row.Add(col.ColumnName.Trim(), DateTime.MinValue.ToUniversalTime());
                            }
                            else
                            {
                                row.Add(col.ColumnName.Trim(), dr[col]);
                            }
                        }
                    }
                    rows.Add(row);
                }
                return rows;
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                ProcessException(ex, ErrorMessages.GetLookupDataFail);
            }

            return null;
        }

        #endregion

        #endregion

        #region Mobile Dashboard
        public List<Batch> GetNewWorkItemsForDashboard()
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetWorkItems(BatchStatus.Available);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public List<Batch> GetRejectedWorkItemsForDashboard()
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetWorkItems(BatchStatus.Reject);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public List<Batch> GetErrorWorkItemsForDashboard()
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetWorkItems(BatchStatus.Error);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetWorkItemFail);
            }
        }

        public void CountBatchForDashboard(out int errorBatchCount, out int availableBatchCount, out int rejectedBatchCount)
        {
            try
            {
                new BatchManager(UserContext.Current.User).CountBatchs(out errorBatchCount, out availableBatchCount, out rejectedBatchCount);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBatchFail);
            }
        }

        public List<Comment> GetTopComment()
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetTopComment();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetCommentFail);
            }
        }


        public ListCommentOptimize GetTopCommentOptimize()
        {
            try
            {
                return new BatchManager(UserContext.Current.User).GetTopCommentOptimize();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetCommentFail);
            }
        }


        #endregion
    }

    public class JSonHelper
    {
        public MemoryStream ConvertObjectToJSonMemory<T>(T obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, obj);
            return ms;
        }
        public string ConvertObjectToJSon<T>(T obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, obj);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        public T ConvertJSonToObject<T>(string jsonString)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)serializer.ReadObject(ms);
            return obj;
        }
        public T ConvertJSonToObject<T>(MemoryStream ms)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T obj = (T)serializer.ReadObject(ms);
            return obj;
        }
    }
}
