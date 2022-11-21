namespace Ecm.Model
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

    public delegate void CloseDialog();

    public class Common
    {
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
        public const string STARTS_WITH = "Starts with";
        public const string ENDS_WITH = "End With";

        public const string AND = "AND";
        public const string OR = "OR";
        public const string NONE = "NONE";

        public const string COLUMN_SELECTED = "Selected_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_CHECKED = "Checked_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_PAGE_COUNT = "Page count";
        public const string COLUMN_VERSION = "Version";
        public const string COLUMN_MODIFIED_ON = "Modified date";
        public const string COLUMN_MODIFIED_BY = "Modified by";
        public const string COLUMN_CREATED_ON = "Created date";
        public const string COLUMN_CREATED_BY = "Created by";
        public const string COLUMN_DOCUMENT_ID = "DocumentId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_DOCUMENT = "Document_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_DOCUMENT_TYPE_ID = "DocumentTypeId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        public const string COLUMN_BINARY_TYPE = "File type";
        public const string COLUMN_DOCUMENT_VERSION_ID = "DocVersionId_4E19573E_D42E_4B74_BB81_E3EF56633947";

        public const string DOCUMENT_CREATED_BY = "CreatedBy_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_CREATED_DATE = "CreatedDate_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_ID = "DocTypeId_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string FIELD_ID = "Id_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_VERSION = "Version_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_MODIFIED_BY = "ModifiedBy_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_MODIFIED_DATE = "ModifiedDate_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_PAGE_COUNT = "PageCount_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string DOCUMENT_FILE_BINARY = "BinaryType_CFCCCDF6_84CC_43ED_BE4A_617974473143";

        public const string OUTLOOK_MAIL_BOBY_FIELD_NAME = "Mail body";
        public const string OUTLOOK_MAIL_SUBJECT_FIELD_NAME = "Subject";
        public const string OUTLOOK_MAIL_TO_FIELD_NAME = "Mail to";
        public const string OUTLOOK_MAIL_FROM_FIELD_NAME = "Mail from";
        public const string OUTLOOK_MAIL_RECEIVED_DATE_FIELD_NAME = "Received date";

        public const string REPORT_ACTION_LOG = "Action log";
        public const string REPORT_DOCUMENT = "Document";
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

        public const string NATIVE_DOCUMENT = "Native";
        public const string IMAGE_DOCUMENT = "Image";
        public const string COMPOUND_DOCUMENT = "Compound";
        //// For workflow
        //public const string COLUMN_LOCKED_BY = "Locked by";
        //public const string COLUMN_WORKFLOW_INSTANCE_ID = "WorkflowInstanceId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //public const string COLUMN_WORKFLOW_DEFINITION_ID = "WorkflowDefinitionId_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //public const string COLUMN_BLOCKING_BOOKMARK = "BlockingBookmark_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //public const string COLUMN_BLOCKING_ACTIVITY_NAME = "Activity name";
        //public const string COLUMN_BLOCKING_ACTIVITY_DESCRIPTION = "ActivityDescription_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //public const string COLUMN_BLOCKING_DATE = "Blocking date";
        //public const string COLUMN_LAST_ACCESSED_DATE = "Last accessed date";
        //public const string COLUMN_LAST_ACCESSED_BY = "Last accessed by";
        //public const string COLUMN_IS_COMPLETED = "IsCompleted_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //public const string COLUMN_IS_PROCESSING = "Is_Processing_4E19573E_D42E_4B74_BB81_E3EF56633947";
        //public const string COLUMN_HAS_ERROR = "Has error";
        //public const string COLUMN_STATUS = "Status";
        //public const string COLUMN_PERMISSION = "Permission_4E19573E_D42E_4B74_BB81_E3EF56633947";


        //DB Type
        public const string SQL_SERVER = "MS SQL Server";
        public const string ORACLE = "Oracle Database";
        public const string IBM_DB2 = "IBM DB2";
        public const string MY_SQL = "My Sql";
        public const string POSTGRE_SQL = "Postgre SQL";

        //Provider Type
        public const string ADO_NET = "ADO.NET provider";
        public const string OLEDB = "Oledb provider";


        public const string OUTLOOK_AUTO_SIGNIN_KEY = @"HKEY_CURRENT_USER\Software\Microsoft\Office\Outlook\Addins\OutlookImport\AutoSignIn";
        public const string EXCEL_AUTO_SIGNIN_KEY = @"HKEY_CURRENT_USER\Software\Microsoft\Office\Excel\Addins\ExcelImport\AutoSignIn";
        public const string WORD_AUTO_SIGNIN_KEY = @"HKEY_CURRENT_USER\Software\Microsoft\Office\Word\Addins\WordImport\AutoSignIn";
        public const string POWER_POINT_AUTO_SIGNIN_KEY = @"HKEY_CURRENT_USER\Software\Microsoft\Office\PowerPoint\Addins\PowerPointImport\AutoSignIn";


    }
}