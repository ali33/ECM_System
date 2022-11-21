using System.Collections.Generic;
using System;
using System.Data;
using System.Security;
using System.ServiceModel;
using log4net;

using Ecm.Service.Contract;
using Ecm.Domain;
using Ecm.Core;
using SecurityManager = Ecm.Core.SecurityManager;
using Ecm.Utility;
using System.IO;
using System.Reflection;
using System.Linq;

[assembly: log4net.Config.XmlConfigurator(Watch = true, ConfigFile = @"log4net.xml")]

namespace Ecm.Service
{
    [CredentialsExtractor]
    [UserContextBehavior]
    public class Archive : IArchive
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(Archive));

        private Exception ProcessException(Exception ex, string errorMessage)
        {
            _log.Error(errorMessage, ex);
            return new FaultException(errorMessage);
        }

        public void Ping()
        {
        }

        #region Security

        public string EncryptPassword(string password)
        {
            return CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
        }

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

        public User VerifyUser(string userName, string passwordHash)
        {
            try
            {
                return new SecurityManager().Authorize(userName, passwordHash);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.AuthenticateUserFail);
            }
        }

        #endregion

        #region Document type

        public List<DocumentType> GetDocumentTypes()
        {
            try
            {
                return new DocTypeManager(UserContext.Current.User).GetDocumentTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocumentTypeFail);
            }
        }

        public List<DocumentType> GetCapturedDocumentTypes()
        {
            try
            {
                return new DocTypeManager(UserContext.Current.User).GetCapturedDocumentTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetCapturedDocumentTypeFail);
            }
        }

        public DocumentType GetDocumentType(Guid documentTypeId)
        {
            try
            {
                return new DocTypeManager(UserContext.Current.User).GetDocumentType(documentTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocumentTypeFail);
            }
        }

        public Guid SaveDocumentType(DocumentType documentType)
        {
            try
            {
                return new DocTypeManager(UserContext.Current.User).Save(documentType);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveDocumentTypeFail);
            }
        }

        public void DeleteDocumentType(DocumentType documentType)
        {
            try
            {
                new DocTypeManager(UserContext.Current.User).Delete(documentType);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteDocumentTypeFail);
            }
        }

        public void SaveOCRTemplate(OCRTemplate ocrTemplate)
        {
            try
            {
                new DocTypeManager(UserContext.Current.User).SaveOCRTemplate(ocrTemplate);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveOCRTemplateFail);
            }
        }

        public void DeleteOCRTemplate(Guid documentTypeId)
        {
            try
            {
                new DocTypeManager(UserContext.Current.User).DeleteOCRTemplate(documentTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteOCRTemplateFail);
            }
        }

        public List<BarcodeConfiguration> GetBarcodeConfigurations(Guid documentTypeId)
        {
            try
            {
                return new DocTypeManager(UserContext.Current.User).GetBarcodeConfigurations(documentTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetBarcodeConfigurationFail);
            }
        }

        public void SaveBarcodeConfiguration(BarcodeConfiguration barcode)
        {
            try
            {
                new DocTypeManager(UserContext.Current.User).SaveBarcodeConfiguration(barcode);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveBarcodeConfigurationFail);
            }
        }

        public void DeleteBarcodeConfiguration(Guid configurationId)
        {
            try
            {
                new DocTypeManager(UserContext.Current.User).DeleteBarcodeConfiguration(configurationId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteBarcodeConfigurationFail);
            }
        }

        public void ClearBarcodeConfigurations(Guid documentTypeId)
        {
            try
            {
                new DocTypeManager(UserContext.Current.User).ClearBarcodeConfigurations(documentTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteBarcodeConfigurationFail);
            }
        }

        #endregion

        #region User Members

        public List<User> GetUsers()
        {
            try
            {
                var workflowUser = new Guid("40194b17-1418-42be-ad8f-e1e52cb771d3");
                return new UserManager(UserContext.Current.User).GetUsers().Where(h => h.Id != workflowUser).ToList();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetUsersFail);
            }
        }

        public Guid SaveUser(User user)
        {
            try
            {
                return new UserManager(UserContext.Current.User).Save(user);
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

        #endregion

        #region UserGroup Members

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

        #region Permission

        public List<DocumentType> GetDocTypesUnderPermissionConfiguration()
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetDocTypes();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocumentTypeFail);
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

        public DocumentTypePermission GetDocTypePermission(UserGroup userGroup, DocumentType documentType)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetDocTypePermision(userGroup, documentType);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocTypePermissionFail);
            }
        }

        public void SavePermission(DocumentTypePermission documentTypePermission, AnnotationPermission annotationPermission, AuditPermission auditPermission)
        {
            try
            {
                new PermissionManager(UserContext.Current.User).SavePermission(documentTypePermission, annotationPermission, auditPermission);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.SaveDocTypePermissionFail);
            }
        }

        public AnnotationPermission GetAnnotationPermission(UserGroup userGroup, DocumentType documentType)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetAnnotationPermission(userGroup, documentType);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetAnnotationPermissionFail);
            }
        }

        public AuditPermission GetAuditPermission(UserGroup userGroup, DocumentType documentType)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetAuditPermission(userGroup, documentType);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetAuditPermissionFail);
            }
        }

        public AuditPermission GetAuditPermissionByUser(User user)
        {
            try
            {
                return new PermissionManager(UserContext.Current.User).GetAuditPermission(user);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetAuditPermissionFail);
            }
        }

        #endregion

        #region Document

        public void InsertDocuments(List<Document> documents)
        {
            try
            {
                User user = UserContext.Current.User;
                new DocumentManager(user).InsertDocuments(documents);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.InsertDocumentFail);
            }
        }

        public List<Document> UpdateDocuments(List<Document> documents)
        {
            try
            {
                User user = UserContext.Current.User;
                return new DocumentManager(user).UpdateDocuments(documents);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.UpdateDocumentFail);
            }
        }

        public Document GetDocument(Guid documentId)
        {
            try
            {
                User user = UserContext.Current.User;
                return new DocumentManager(user).GetDocument(documentId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocumentFail);
            }
        }

        public List<Document> GetDocuments(List<Guid> documentIds)
        {
            try
            {
                User user = UserContext.Current.User;
                return new DocumentManager(user).GetDocuments(documentIds);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocumentFail);
            }
        }

        public SearchResult RunAdvanceSearch(Guid documentTypeId, SearchQuery query, int pageIndex, int pageSize, string sortColumn, string sortDir)
        {
            try
            {
                User user = UserContext.Current.User;
                return new DocumentManager(user).RunAdvanceSearch(documentTypeId, query, pageIndex, pageSize, sortColumn, sortDir);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.RunAdvanceSearchFail);
            }
        }

        public List<SearchResult> RunGlobalSearch(string query, int pageIndex, int pageSize)
        {
            try
            {
                User user = UserContext.Current.User;
                return new DocumentManager(user).RunGlobalSearch(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.RunGlobalSearchFail);
            }
        }

        public void DeleteDocument(Guid documentId)
        {
            try
            {
                User user = UserContext.Current.User;
                new DocumentManager(user).DeleteDocument(documentId);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.DeleteDocumentFail);
            }
        }

        public SearchResult RunContentSearch(Guid documentTypeId, string text, int pageIndex, int pageSize)
        {
            try
            {
                User user = UserContext.Current.User;
                return new DocumentManager(user).RunSearchContent(documentTypeId, text, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, ErrorMessages.DeleteDocumentFail);
            }
        }
        #endregion

        #region Search query

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

        public bool QueryExisted(Guid documentTypeId, string queryName)
        {
            try
            {
                return new SearchQueryManager(UserContext.Current.User).QueryExisted(documentTypeId, queryName);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

        public List<SearchQuery> GetSavedQueries(Guid documentTypeId)
        {
            try
            {
                return new SearchQueryManager(UserContext.Current.User).GetSavedQueries(documentTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetSaveQuery);
            }
        }

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

        //#region Lookup

        //public DataTable GetLookupData(FieldMetaData field, string value)
        //{
        //    try
        //    {
        //        return new LookupManager(UserContext.Current.User).GetLookupData(field, value);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetLookupDataFail);
        //    }
        //}

        //public IDictionary<string, LookupDataSourceType> GetDataSource(string connetionString, LookupDataSourceType type,
        //                                                               string dataProvider)
        //{
        //    try
        //    {
        //        return new LookupManager(UserContext.Current.User).GetDataSource(connetionString, type, dataProvider);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetLookupDatasourceFail);
        //    }
        //}

        //public DataTable GetParameters(string connectionString, string storedName, string dataProvider)
        //{
        //    try
        //    {
        //        return new LookupManager(UserContext.Current.User).GetParameters(connectionString, storedName, dataProvider);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetLookupDataParameterFail);
        //    }
        //}

        //public IDictionary<string, string> GetColumns(string sourceName, string connectionString,
        //                                              LookupDataSourceType type, string dataProvider)
        //{
        //    try
        //    {
        //        return new LookupManager(UserContext.Current.User).GetColumns(sourceName, connectionString, type, dataProvider);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetDataColumnNameFail);
        //    }
        //}

        //public bool TestConnection(string connectionString, string dataProvider)
        //{
        //    try
        //    {
        //        return new LookupManager(UserContext.Current.User).TestConnection(connectionString, dataProvider);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.TestConnectionFail);
        //    }
        //}

        //public List<string> GetDatabaseNames(string connectionString, string dataProvider)
        //{
        //    try
        //    {
        //        return new LookupManager(UserContext.Current.User).GetDatabaseNames(connectionString, dataProvider);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ProcessException(ex, ErrorMessages.GetDatabaseNameFail);
        //    }
        //}

        //#endregion

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

        public List<string> GetStoredProcedureNames(ConnectionInfo connectionInfo)
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

        public LookupInfo GetLookupInfo(Guid fieldId)
        {
            try
            {
                return new LookupManager().GetLookupInfo(fieldId, UserContext.Current.User);
            }
            catch (Exception ex)
            {
                ProcessException(ex, ErrorMessages.GetLookupInfoFail);
            }

            return null;

        }
        #endregion

        #region Settings

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

        #region Action log

        public ActionLog GetActionLog(Guid id)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetActionLog(id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
        }

        public List<ActionLog> GetActionLogs(int index, int pageSize, out long totalRow)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetActionLogs(index, pageSize, out totalRow);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
        }

        public List<ActionLog> GetActionLogAll()
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetActionLogs();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
        }

        public List<ActionLog> SearchActionLogs(string expression, int index, int pageSize, out long totalRow)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).SearchActionLogs(expression, index, pageSize, out totalRow);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
        }

        public List<ActionLog> SearchActionLogs(string expression)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).SearchActionLogs(expression);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetActionLogFail);
            }
        }

        public void DeleteLog(Guid id)
        {
            try
            {
                new ActionLogManager(UserContext.Current.User).DeleteLog(id);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.DeleteActionLogFail);
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
                throw ProcessException(ex, ErrorMessages.AddActionLogFail);
            }
        }

        public List<ActionLog> GetLogByDocument(Guid docId)
        {
            try
            {
                return new ActionLogManager(UserContext.Current.User).GetLogByDoc(docId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetActionLogFail);
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

        #region Version

        public List<DocumentVersion> GetDocumentVersionsByExistingDoc(Guid documentId)
        {
            try
            {
                return new DocumentVersionManager(UserContext.Current.User).GetDocumentVersionsByExistingDoc(documentId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocVersionFail);
            }
        }

        public List<DocumentVersion> GetDocumentVersionsByDeletedDocType(Guid documentTypeId)
        {
            try
            {
                return new DocumentVersionManager(UserContext.Current.User).GetDocumentVersionsByDeletedDocType(documentTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocVersionFail);
            }
        }

        public DocumentVersion GetDocumentVersion(Guid documentVersionId)
        {
            try
            {
                return new DocumentVersionManager(UserContext.Current.User).GetDocumentVersion(documentVersionId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocVersionFail);
            }
        }

        public List<DocumentTypeVersion> GetDocumentTypeVersions()
        {
            try
            {
                return new DocumentTypeVersionManager(UserContext.Current.User).GetDocumentTypeVersions();
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocTypeVersionFail);
            }
        }

        public DocumentTypeVersion GetDocumentTypeVersion(Guid docTypeId)
        {
            try
            {
                return new DocumentTypeVersionManager(UserContext.Current.User).GetDocumentTypeVersion(docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocTypeVersionFail);
            }
        }

        public DocumentVersion GetLatestDeletedDocumentVersion(Guid docId)
        {
            try
            {
                return new DocumentVersionManager(UserContext.Current.User).GetLatestDeleteDocumentVersion(docId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocVersionFail);
            }
        }

        public DocumentVersion GetVersionOfDeletedDocument(Guid docVersionId)
        {
            try
            {
                return new DocumentVersionManager(UserContext.Current.User).GetDeletedDocumentVersion(docVersionId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocVersionFail);
            }
        }

        public List<DocumentVersion> GetDeletedDocWithExistingDocType(Guid docTypeId)
        {
            try
            {
                return new DocumentVersionManager(UserContext.Current.User).GetDeletedDocWithExistingDocType(docTypeId);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.GetDocVersionFail);
            }
        }

        #endregion

        #region Logging

        public void Log(string message, string stackTrace)
        {
            _log.Error(message + Environment.NewLine + stackTrace);
        }

        #endregion


        public void WriteFileToServer(byte[] fileBytes, string fileName)
        {
            try
            {
                string filePath = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(Assembly.GetExecutingAssembly().CodeBase)), @"OutlookTemp\" + fileName);
                filePath = filePath.Replace("file:\\", string.Empty);
                using (FileStream file = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    file.Write(fileBytes, 0, fileBytes.Length);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, ErrorMessages.WriteFileFail);
            }
        }


    }
}