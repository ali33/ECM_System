using System;
using System.Collections.Generic;
using System.ServiceModel;
using Ecm.CaptureDomain;
using System.Data;
using Ecm.BarcodeDomain;
using Ecm.LookupDomain;

namespace Ecm.Service.Contract
{
    /// <summary>
    /// Represent the API for the CloudECM web service.
    /// </summary>
    [ServiceContract]
    public interface ICapture
    {
        [OperationContract]
        void Ping();

        #region Security

        [OperationContract]
        User Login(string userName, string encryptedPassword, string clientHost);

        [OperationContract]
        User ChangePassword(string userName, string oldEncryptedPassword, string newEncryptedPassword);

        /// <summary>
        /// Reset password in case user forgot their password
        /// </summary>
        /// <param name="username">Username of user that want to reset password</param>
        /// <param name="email">Email address of user that want to reset password</param>
        [OperationContract]
        void ResetPassword(string username);

        [OperationContract]
        User AuthoriseUser(string username, string password);

        #endregion

        #region Logging

        /// <summary>
        /// This method is used internally
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        [OperationContract]
        void Log(string message, string stackTrace);

        #endregion

        #region User

        [OperationContract]
        List<User> GetUsers();

        [OperationContract]
        User GetUser(string username, string passwordHash);

        [OperationContract]
        User GetUserByUserName(string username);

        [OperationContract]
        void SaveUser(User user);

        [OperationContract]
        void DeleteUser(User user);

        [OperationContract]
        List<User> GetAvailableUserToDelegation();

        #endregion

        #region User group

        [OperationContract]
        List<UserGroup> GetUserGroups();

        [OperationContract]
        void SaveUserGroup(UserGroup userGroup);

        [OperationContract]
        void DeleteUserGroup(UserGroup userGroup);

        #endregion

        #region Language

        [OperationContract]
        Language GetLanguage(Guid languageId);

        [OperationContract]
        List<Language> GetLanguages();

        #endregion

        #region Permission

        [OperationContract]
        List<BatchType> GetBatchTypesUnderPermissionConfiguration();

        [OperationContract]
        List<UserGroup> GetUserGroupsUnderPermissionConfiguration();

        [OperationContract]
        BatchTypePermission GetBatchTypePermission(Guid userGroupId, Guid batchTypeId);

        [OperationContract]
        void SavePermission(BatchTypePermission batchTypePermission, List<DocumentTypePermission> documentTypePermissions);

        [OperationContract]
        DocumentTypePermission GetDocumentTypePermission(Guid userGroupId, Guid docTypeId);

        [OperationContract]
        DocumentTypePermission GetDocumentTypePermissionByUser(Guid userId, Guid docTypeId);

        [OperationContract]
        List<DocumentFieldPermission> GetFieldPermission(Guid userGroupId, Guid docTypeId);
        #endregion

        #region Batch type
        [OperationContract]
        BatchType GetBatchType(Guid batchTypeId);

        [OperationContract]
        List<BatchType> GetBatchTypes();

        [OperationContract]
        List<BatchType> GetCaptureBatchType();

        [OperationContract]
        List<BatchType> GetAssignWorkBatchTypes();

        [OperationContract]
        void DeleteBatchType(Guid batchTypeId);

        [OperationContract]
        void SaveBatchType(BatchType batchType);

        [OperationContract]
        bool CanEditBatchTypeField(Guid batchTypeId);

        #endregion

        #region Workflow

        [OperationContract]
        WorkflowDefinition GetWorkflowByBatchTypeId(Guid batchTypeId);

        //[OperationContract]
        //Guid SaveWorkflowDefinition(Guid batchTypeId, WorkflowDefinition wfDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings);
        [OperationContract]
        Guid SaveWorkflowDefinition(Guid batchTypeId, WorkflowDefinition wfDefinition, List<CustomActivitySetting> customActivitySettings);

        //[OperationContract]
        //List<HumanStepPermission> GetWorkflowHumanStepPermissions(Guid workflowDefinitionID);

        [OperationContract]
        CustomActivitySetting GetCustomActivitySetting(Guid wfDefinitionId, Guid activityId);

        [OperationContract]
        List<CustomActivitySetting> GetCustomActivitySettings(Guid wfDefinitionId);
        #endregion

        #region Work Item

        [OperationContract]
        void InsertBatch(Batch workItem);

        [OperationContract]
        void UpdateWorkItem(Batch workItem);

        [OperationContract]
        void SaveWorkItem(Batch workItem);

        [OperationContract]
        WorkItemSearchResult RunAdvanceSearchWorkItem(Guid batchTypeId, SearchQuery searchQuery, int pageIndex);

        //[OperationContract(Name = "AdvanceSearchWithStringExpression")]
        //WorkItemSearchResult RunAdvanceSearchWorkItem(Guid batchTypeId, string searchQuery, int pageIndex);

        [OperationContract(Name = "ApproveByIds")]
        void ApproveWorkItems(List<Guid> ids);

        [OperationContract]
        void ApproveWorkItems(List<Batch> batchs);

        [OperationContract]
        void RejectWorkItems(List<Batch> batchs, string rejectNote);

        [OperationContract(Name = "RejectByIds")]
        void RejectWorkItems(List<Guid> ids, string rejectNote);

        [OperationContract]
        void DelegateWorkItems(List<Guid> ids, string toUser, string delegatedComment);

        [OperationContract]
        void ResumeWorkItems(List<Guid> ids);

        [OperationContract]
        void UnLockWorkItems(List<Guid> ids);

        [OperationContract]
        Batch GetWorkItem(Guid id);

        [OperationContract]
        Batch OpenWorkItem(Guid id);

        [OperationContract]
        List<Batch> OpenWorkItems(List<Guid> ids);

        [OperationContract]
        void DeleteWorkItems(List<Guid> ids);

        [OperationContract]
        WorkItemSearchResult GetLockedBatchs(Guid batchTypeId, int pageIndex);

        [OperationContract]
        WorkItemSearchResult GetProcessingBatchs(Guid batchTypeId, int pageIndex);

        [OperationContract]
        WorkItemSearchResult GetErrorBatchs(Guid batchTypeId, int pageIndex);

        [OperationContract]
        WorkItemSearchResult GetBatchsByBatchType(Guid batchTypeId, int pageIndex);

        [OperationContract]
        WorkItemSearchResult GetRejectedBatch(Guid batchTypeId, int pageIndex);

        [OperationContract]
        List<Batch> GetBatchs(List<Guid> ids);

        [OperationContract(Name = "GetBatchByBatchType")]
        List<Batch> GetBatchs(Guid batchTypeId);

        [OperationContract]
        void CountBatchs(Guid batchTypeId, out int errorBatchCount, out int inProcessingBatchCount, out int lockedBatchCount, out int availableBatchCount, out int rejectedBatchCount);

        [OperationContract]
        void LockBatchs(List<Guid> ids);

        [OperationContract]
        void UnlockBatchs(List<Guid> ids);
        #endregion

        #region OCR template

        [OperationContract]
        void SaveOcrTemplate(OCRTemplate ocrTemplate);

        [OperationContract]
        void DeleteOcrTemplate(Guid docTypeId);

        #endregion

        #region Barcode

        [OperationContract]
        void SaveBarcodeConfiguration(string xml, Guid batchTypeId);

        #endregion

        #region Lookup

        [OperationContract(Name = "LookupData1")]
        DataTable GetLookupData(LookupInfo lookupInfo, Dictionary<string, string> mappingValue);

        [OperationContract]
        DataTable GetLookupData(LookupInfo lookupInfo, string value);

        [OperationContract]
        bool TestConnection(ConnectionInfo connectionInfo);

        [OperationContract]
        bool TestQueryParam(string query, List<string> fieldNames);

        [OperationContract]
        List<string> GetDatabaseNames(ConnectionInfo connectionInfo);

        [OperationContract]
        List<string> GetSchemas(ConnectionInfo connectionInfo);

        [OperationContract]
        List<string> GetTableNames(ConnectionInfo connectionInfo);

        [OperationContract]
        List<string> GetViewNames(ConnectionInfo connectionInfo);

        [OperationContract]
        List<string> GetStoredProcedureNames(ConnectionInfo connectionInfo);

        //[OperationContract]
        //string GenerateSqlCommand(List<string> indexField, string lookupField, DatabaseType dbType, DBLookupType lookupType, string sourceName, string dbSchema);

        //[OperationContract]
        //string GenerateExampleSqlCommand(DatabaseType dbType, DBLookupType lookupType, string dbSchema);

        //[OperationContract]
        //string GenerateExampleSqlCommandWithBatchInfo(DatabaseType dbType, DBLookupType lookupType, string dbSchema);

        [OperationContract]
        List<string> GetRuntimeValueParams(string sqlCommand);

        [OperationContract]
        DataTable GetParameterNames(ConnectionInfo connectionInfo, string storedName);

        [OperationContract]
        Dictionary<string, string> GetColummnNames(ConnectionInfo connectionInfo, string sourceName, LookupDataSourceType sourceType);

        [OperationContract]
        void UpdateBatchLookup(Guid fieldId, string xml, Guid? lookupActivityId);

        [OperationContract]
        void UpdateDocumentLookup(Guid fieldId, string xml, Guid? lookupActivityId);
        #endregion

        #region Search Query
        /// <summary>
        /// Save the query which is used in <see cref="RunAdvanceSearch"/> method. This query can be retrieved and used for the sub-sequence search call.
        /// </summary>
        /// <param name="searchQuery">The query will be saved.</param>
        /// <returns>The Id of the saved query if user has search permission to the document type, otherwise <see cref="FaultException"/> exception will be thrown</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        Guid SaveQuery(SearchQuery searchQuery);

        /// <summary>
        /// Retrieve all search queries of the current login user againts one document type
        /// </summary>
        /// <param name="documentTypeId">The document type that user want to retrieve all search queries</param>
        /// <returns>List of <see cref="SearchQuery"/> objects if user has search permission to <see cref="DocumentType"/>, otherwise the <see cref="FaultException"/> exception will be thrown.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        List<SearchQuery> GetSavedQueries(Guid batchTypeId);

        /// <summary>
        /// Check whether a query name is existed or not on a document type against the login user
        /// </summary>
        /// <param name="documentTypeId">The document type for checking</param>
        /// <param name="queryName">The query name for checking</param>
        /// <returns><b>true</b> if the query name is found, otherwise <b>false</b>. The <see cref="FaultException"/> exception will be thrown if the login user doesn't have the search permission on document type.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        bool QueryExisted(Guid batchTypeId, string queryName);

        /// <summary>
        /// Delete the <see cref="SearchQuery"/> by using its id. The <see cref="FaultException"/> exception will be thrown if user doesn't have the search permission on the document type that this query belong to.
        /// </summary>
        /// <param name="queryId">The query id will be deleted</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        void DeleteQuery(Guid queryId);
        #endregion

        #region Ambiguous Definition

        /// <summary>
        /// Retrieve all OCR auto correction defined with a language.
        /// </summary>
        /// <param name="languageId">The language that the OCR is doing.</param>
        /// <returns>List of <see cref="AmbiguousDefinition"/> objects</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        List<AmbiguousDefinition> GetAmbiguousDefinitions(Guid languageId);

        /// <summary>
        /// Retrieve all OCR auto corrections in the CloudECM
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        List<AmbiguousDefinition> GetAllAmbiguousDefinitions();

        /// <summary>
        /// Delete OCR auto correction by its id
        /// </summary>
        /// <param name="id">The id of the OCR auto correction</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have administration priviledge to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        void DeleteAmbiguousDefinition(Guid id);

        /// <summary>
        /// Add an OCR auto correction on a language into the CloudECM
        /// </summary>
        /// <param name="ambiguousDefinition">The OCR auto correction will be saved</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have administration priviledge to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        void SaveAmbiguousDefinition(AmbiguousDefinition ambiguousDefinition);

        #endregion

        #region Doc Type
        [OperationContract]
        List<DocumentType> GetDocumentTypes(Guid batchTypeId);

        [OperationContract]
        DocumentType GetDocumentType(Guid docTypeId);

        [OperationContract]
        /// <summary>
        /// Check list document type whether have document or not.
        /// </summary>
        /// <param name="docTypeIds">List document type id need to check</param>
        /// <returns>List of document type id which have document</returns>
        List<Guid> CheckDocTypeHaveDocument(List<Guid> docTypeId);
        #endregion

        #region ActionLog

        /// <summary>
        /// Retrieve the action log by using its id
        /// </summary>
        /// <param name="id">The id of action log will be retrieved.</param>
        /// <returns>The <see cref="ActionLog"/> object.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have AllowedViewLog permission in <see cref="AuditPermission"/> to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        ActionLog GetActionLog(Guid id);

        /// <summary>
        /// Retrieve all action log in the system with paging capability.
        /// </summary>
        /// <param name="pageIndex">The page index of the logs</param>
        /// <param name="pageSize">How many items on each page</param>
        /// <param name="totalItems">The total items found.</param>
        /// <returns>List of <see cref="ActionLog"/> objects.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have AllowedViewLog permission in <see cref="AuditPermission"/> to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        List<ActionLog> GetActionLogs(int pageIndex, int pageSize, out long totalItems);

        /// <summary>
        /// Retrieve all action log in the system.
        /// </summary>
        /// <returns>List of <see cref="ActionLog"/> objects.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have AllowedViewLog permission in <see cref="AuditPermission"/> to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        List<ActionLog> GetActionLogAll();

        /// <summary>
        /// Search action log base on the criteria defined in the expression with paging capability.
        /// </summary>
        /// <param name="expression">The expression define the criteria for search.</param>
        /// <param name="pageIndex">The page index of the search.</param>
        /// <param name="pageSize">How many items on each page.</param>
        /// <param name="totalItems">The total items found by the search</param>
        /// <returns>List of <see cref="ActionLog"/> objects.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have AllowedViewLog permission in <see cref="AuditPermission"/> to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        List<ActionLog> SearchActionLogs(string expression, int pageIndex, int pageSize, out long totalItems);

        /// <summary>
        /// Search action log base on the criteria defined in the expression.
        /// </summary>
        /// <param name="expression">The expression define the criteria for search.</param>
        /// <returns>List of <see cref="ActionLog"/> objects.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have AllowedViewLog permission in <see cref="AuditPermission"/> to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract(Name = "SearchActionLog1")]
        List<ActionLog> SearchActionLogs(string expression);

        /// <summary>
        /// Delete a log in the system.
        /// </summary>
        /// <param name="id">The id of the log.</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have AllowedDeleteLog permission in <see cref="AuditPermission"/> to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        void DeleteLog(Guid id);

        /// <summary>
        /// Allow add a log into the system.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="actionName">The action.</param>
        /// <param name="type">The log occurs on which type of object (Document or Page).</param>
        /// <param name="objectId">The id of the object that the log occurs.</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        void AddActionLog(string message, ActionName actionName, ObjectType? type, Guid? objectId);

        #endregion

        #region Setting

        /// <summary>
        /// Retrieve all system settings.
        /// </summary>
        /// <returns>The <see cref="Setting"/> object.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        Setting GetSettings();

        /// <summary>
        /// Write the setting into the CloudECM.
        /// </summary>
        /// <param name="setting"></param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>To use this method, the client proxy must pass the credential into the header of service request message. The credential includes user name, password hash (is the Password property of <see cref="User"/> object which was returned by the <see cref="Login"/> method) and client host.</description>
        /// </item>
        /// <item>
        ///     <description>The login user must have administration priviledge to call this method, otherwise <see cref="FaultException"/> exception will be thrown.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// See the example in <see cref="ChangePassword"/> to see how to call this method.
        /// </example>
        [OperationContract]
        void WriteSetting(Setting setting);

        #endregion

        #region Mvc

        #region Search query

        /// <summary>
        /// Get list saved SearchQuery (just get information in table SearchQuery).
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        [OperationContract]
        List<SearchQuery> GetSavedQueriesLight(Guid batchTypeId);

        /// <summary>
        /// Get saved SearchQuery (exclude information of BatchType).
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        [OperationContract]
        SearchQuery GetSavedQuery(Guid queryId); 

        #endregion

        /// <summary>
        /// Get list field of Batch Type.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        [OperationContract]
        List<BatchFieldMetaData> GetFieldsFromBatchType(Guid batchTypeId);

        #region Work item

        /// <summary>
        /// Get the result of advanced search for MVC version.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [OperationContract]
        WorkItemSearchResult GetAdvancedSearchWorkItems(SearchQuery searchQuery, int pageIndex);

        /// <summary>
        /// Get the result of normal search by batch status for MVC version.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [OperationContract]
        WorkItemSearchResult GetBatches(Guid batchTypeId, BatchStatus status, int pageIndex);

        #endregion

        #endregion

        [OperationContract]
        Guid GetTransactionId(Guid batchId);

    }
}
