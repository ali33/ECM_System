namespace Ecm.ContentViewer.Model
{
    public enum UserTypeModel
    {
        BuiltIn,
        ActiveDirectory,
        SingleSignOn
    }

    public enum UserGroupTypeModel
    {
        BuiltIn,
        ActiveDirectory,
        SingleSignOn
    }

    public enum ApplicationMode
    { 
        None,
        AssignedWork,
        Capture,
        WorkItem
    }

    public enum FieldDataType
    {
        String,
        Integer,
        Decimal,
        Picklist,
        Boolean,
        Date,
        Folder,
        Table
    }

    public enum ChangeType
    {
        None = 0,
        NewDocument = 1, // Doc level
        DeleteDocument = 2, // Batch level
        ChangeDocumentType = 4, // Doc level
        NewPage = 8, // Page level
        ReplacePage = 16, // Page level
        RotatePage = 32, // Page level
        AnnotatePage = 64, // Page level
        DeletePage = 128, // Doc, Batch level
        ReOrderPage = 256, // Doc, Batch level
        AddComment = 512, // Doc level
        Reject = 1024, // Doc/Batch/Page level
        UnReject = 2048, // Doc/Batch/Page level
        UpdateIndex = 4096, // Doc, Batch level
        ChangeBatchType = 8192 // batch level
    }

    public delegate void CloseDialog();

    public class Common
    {
        //Menu name
        public const string MENU_BATCH_TYPE = "BatchTypeMenu";
        public const string MENU_USER = "UserMenu";
        public const string MENU_USER_GROUP = "UserGroupMenu";
        public const string MENU_PERMISSION = "PermissionMenu";
        public const string MENU_SETTING = "SettingMenu";
        public const string MENU_ACTION_LOG = "ActionLogMenu";
        

        public const string MAX_SEARCH_ROWS = "MaxSearchRows";
        public const string IN_BETWEEN = "In Between";
        public const string CONTAINS = "Contains";
        public const string NOT_CONTAINS = "Not Contains";
        public const string EQUAL = "Equal";
        public const string NOT_EQUAL = "Not Equal";
        public const string GREATER_THAN = "Greater Than";
        public const string GREATER_THAN_OR_EQUAL_TO = "Greater Than Or Equal To";
        public const string LESS_THAN_OR_EQUAL_TO = "Less Than Or Equal To";
        public const string LESS_THAN = "Less Than";
        public const string STARTS_WITH = "Starts With";
        public const string ENDS_WITH = "End With";

        public const string AND = "AND";
        public const string OR = "OR";

        public const string COLUMN_SELECTED = "Selected_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_PAGE_COUNT = "Page count";
        public const string COLUMN_DOCUMENT_COUNT = "ContentModel count";
        public const string COLUMN_VERSION = "Version";
        public const string COLUMN_MODIFIED_ON = "Modified date";
        public const string COLUMN_MODIFIED_BY = "Modified by";
        public const string COLUMN_CREATED_ON = "Created date";
        public const string COLUMN_CREATED_BY = "Created by";
        public const string COLUMN_BATCH_ID = "DocumentId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BATCH = "Document_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BATCH_TYPE_ID = "DocumentTypeId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BINARY_TYPE = "BinaryType_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_DOCUMENT_VERSION_ID = "DocVersionId_4E19573E_D42E_4B74_BB81_E3EF56633947";

        public const string OUTLOOK_MAIL_BOBY_FIELD_NAME = "Mail body";
        public const string OUTLOOK_MAIL_SUBJECT_FIELD_NAME = "Subject";
        public const string OUTLOOK_MAIL_TO_FIELD_NAME = "Mail to";
        public const string OUTLOOK_MAIL_FROM_FIELD_NAME = "Mail from";
        public const string OUTLOOK_MAIL_RECEIVED_DATE_FIELD_NAME = "Received date";

        public const string REPORT_ACTION_LOG = "Action log";
        public const string REPORT_DOCUMENT = "ContentModel";
        public const string REPORT_PAGE = "Page";

        public const string DELETE_DOCUMENT_TYPE = "Delete document type";
        public const string DELETE_DOCUMENT = "Delete document";
        public const string DELETE_PAGE = "Delete page";
        public const string UPDATE_FIELD_VALUE = "Update field value";
        public const string APPEND_PAGE = "Appent page";
        public const string REPLACE_PAGE = "Replace page";
        public const string ROTATE_PAGE = "Rotate page";
        public const string ADD_ANNOTATION = "Add annotation";
        public const string UPDATE_ANNOTATION = "Update annotation";
        public const string DELETE_ANNOTATION = "Delete annotation";

        // For workflow
        public const string COLUMN_LOCKED_BY = "Locked by";
        public const string COLUMN_WORKFLOW_INSTANCE_ID = "WorkflowInstanceId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_WORKFLOW_DEFINITION_ID = "WorkflowDefinitionId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BLOCKING_BOOKMARK = "BlockingBookmark_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BLOCKING_ACTIVITY_NAME = "Activity name";
        public const string COLUMN_BLOCKING_ACTIVITY_DESCRIPTION = "ActivityDescription_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BLOCKING_DATE = "Blocking date";
        public const string COLUMN_LAST_ACCESSED_DATE = "Last accessed date";
        public const string COLUMN_LAST_ACCESSED_BY = "Last accessed by";
        public const string COLUMN_IS_COMPLETED = "Completed";
        public const string COLUMN_IS_PROCESSING = "Processing";
        public const string COLUMN_HAS_ERROR = "Has error";
        public const string COLUMN_STATUS = "Status";
        public const string COLUMN_PERMISSION = "Permission_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //Batch status
        public const string BATCH_ERROR = "Errors";
        public const string BATCH_IN_PROCESSING = "In Processing";
        public const string BATCH_LOCKED = "Lockeds";
        public const string BATCH_WAITING = "Availables";
        public const string BATCH_REJECTED = "Rejected";

        public const string SYS_BATCH_NAME_INDEX = "Batch name";

        //Batch properties name
        public const string BATCH_ID = "ID";
        public const string BATCH_NAME = "BatchName";
        public const string BATCH_BATCHTYPE_ID = "BatchTypeID";
        public const string BATCH_DOCUMENT_COUNT = "DocCount";
        public const string BATCH_PAGE_COUNT = "PageCount";
        public const string BATCH_CREATED_DATE = "CreatedDate";
        public const string BATCH_CREATED_BY = "CreatedBy";
        public const string BATCH_MODIFIED_DATE = "ModifiedDate";
        public const string BATCH_MODIFIED_BY = "ModifiedBy";
        public const string BATCH_LOCKED_BY = "LockedBy";
        public const string BATCH_DELEGATED_BY = "DelegatedBy";
        public const string BATCH_DELEGATED_TO = "DelegatedTo";
        public const string BATCH_WORKFLOW_DEFINITION_ID = "WorkflowDefinitionID";
        public const string BATCH_WORKFLOW_INSTANCE_ID = "WorkflowInstanceID";
        public const string BATCH_BLOCKING_BOOKMARK = "BlockingBookmark";
        public const string BATCH_BLOCKING_ACTIVITY_NAME = "BlockingActivityName";
        public const string BATCH_BLOCKING_ACTIVITY_DESCRIPTION = "BlockingActivityDescription";
        public const string BATCH_BLOCKING_DATE = "BlockingDate";
        public const string BATCH_LASTACCESSED_DATE = "LastAccessedDate";
        public const string BATCH_LASTACCESSED_BY = "LastAccessedBy";
        public const string BATCH_IS_PROCESSING = "IsProcessing";
        public const string BATCH_IS_COMPLETED = "IsCompleted";
        public const string BATCH_IS_REJECTED = " IsRejected";
        public const string BATCH_HAS_ERROR = "HasError";
        public const string BATCH_STATUS_MSG = "StatusMsg";
        //Batch display name
        public const string BATCH_DISPLAY_ID = "ID";
        public const string BATCH_DISPLAY_NAME = "Batch Name";
        public const string BATCH_DISPLAY_BATCHTYPE_ID = "Batch Type ID";
        public const string BATCH_DISPLAY_DOCUMENT_COUNT = "ContentModel Count";
        public const string BATCH_DISPLAY_PAGE_COUNT = "Page Count";
        public const string BATCH_DISPLAY_CREATED_DATE = "Created Date";
        public const string BATCH_DISPLAY_CREATED_BY = "Created By";
        public const string BATCH_DISPLAY_MODIFIED_DATE = "Modified Date";
        public const string BATCH_DISPLAY_MODIFIED_BY = "Modified By";
        public const string BATCH_DISPLAY_LOCKED_BY = "Locked By";
        public const string BATCH_DISPLAY_DELEGATED_BY = "Delegated By";
        public const string BATCH_DISPLAY_DELEGATED_TO = "Delegated To";
        public const string BATCH_DISPLAY_WORKFLOW_DEFINITION_ID = "Workflow Definition ID";
        public const string BATCH_DISPLAY_WORKFLOW_INSTANCE_ID = "Workflow Instance ID";
        public const string BATCH_DISPLAY_BLOCKING_BOOKMARK = "Blocking Bookmark";
        public const string BATCH_DISPLAY_BLOCKING_ACTIVITY_NAME = "Blocking Activity Name";
        public const string BATCH_DISPLAY_BLOCKING_ACTIVITY_DESCRIPTION = "Blocking Activity Description";
        public const string BATCH_DISPLAY_BLOCKING_DATE = "Blocking Date";
        public const string BATCH_DISPLAY_LASTACCESSED_DATE = "Last Accessed Date";
        public const string BATCH_DISPLAY_LASTACCESSED_BY = "Last Accessed By";
        public const string BATCH_DISPLAY_IS_PROCESSING = "Is Processing";
        public const string BATCH_DISPLAY_IS_COMPLETED = "Is Completed";
        public const string BATCH_DISPLAY_IS_REJECTED = " Is Rejected";
        public const string BATCH_DISPLAY_HAS_ERROR = "Has Error";
        public const string BATCH_DISPLAY_STATUS_MSG = "Status Message";
        //Batch property ID
        public const string FieldCreatedBy = "CreatedBy_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldBatchName = "BatchName_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldCreatedDate = "CreatedDate_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldBatchTypeId = "BatchTypeId_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldId = "Id_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldModifiedBy = "ModifiedBy_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldModifiedDate = "ModifiedDate_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldPageCount = "PageCount_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldDocumentCount = "DocCount_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldSort = "ModifiedTicks_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldLockedBy = "LockedBy_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldWorkflowInstanceId = "WorkflowInstanceId_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldWorkflowDefinitionId = "WorkflowDefinitionId_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldBlockingBookmark = "BlockingBookmark_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldBlockingActivityName = "BlockingActivityName_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldBlockingActivityDescription = "BlockingActivityDescription_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldBlockingDate = "BlockingDate_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldLastAccessedDate = "LastAccessedDate_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldLastAccessedBy = "LastAccessedBy_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldIsCompleted = "IsCompleted_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldIsProcessing = "IsProcessing_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldIsRejected = "IsRejected_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldHasError = "HasError_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldStatusMsg = "StatusMsg_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldDelegatedBy = "DelegatedBy_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
        public const string FieldDelegatedTo = "DelegatedTo_EE8E522A-BEAB-4814-94A0-C2F81283A1EB";
    }
}
