using System.Runtime.Serialization;
using System;
namespace Ecm.Domain
{
    [DataContract]
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

    [DataContract]
    public enum LookupDataSourceType
    {
        
        Table = 0,
        
        View = 1,
        
        StoredProcedure = 2,
    }

    [DataContract]
    public enum SearchOperator
    {
        
        Equal = 0,
        
        GreaterThan = 1,
        
        GreaterThanOrEqualTo = 2,
        
        LessThan = 3,
        
        LessThanOrEqualTo = 4,
        
        InBetween = 5,
        
        Contains = 6,
        
        NotContains = 7,
        
        NotEqual = 8,
        
        StartsWith = 9,
        
        EndsWith = 10,
    }

    [DataContract]
    public enum SearchConjunction
    {
        
        None = -1,
        
        And = 0,
        
        Or = 1,
    }

    [DataContract]
    public enum ActionName
    {
        
        GetUser,
        
        AddUser,
        
        UpdateUser,
        
        DeleteUser,
        
        ChangePassword,
        
        GetUserGroup,
        
        AddUserGroup,
        
        UpdateUserGroup,
        
        DeleteUserGroup,
        
        AddMemberShip,
        
        UpdateMembership,
        
        DeleteMemberShip,
        
        GetDocumentType,
        
        AddDocumentType,
        
        UpdateDocumentType,
        
        DeleteDocumentType,
        
        GetFieldMetaData,
        
        AddFieldMetaData,
        
        UpdateFieldMetaData,
        
        DeleteFieldMetaData,
        
        GetFieldValue,
        
        AddFieldValue,
        
        UpdateFieldValue,
        
        DeleteFieldValue,
        
        DoSearchDocument,
        
        DoGlobalSearchDocument,
        
        ViewDocument,
        
        GetDocument,
        
        AddDocument,
        
        UpdateDocument,
        
        DeleteDocument,
        
        ViewPage,
        
        ReplacePage,
        
        InsertPage,
        
        UpdatePage,
        
        RotatePage,
        
        DeletePage,
        
        AddAnnotation,
        
        UpdateAnnotation,
        
        DeleteAnnotation,
        
        SendEmail,
        
        Print,
        
        Login,
        
        Save,
        
        GetDocumentTypePermission,
        
        AddDocumentTypePermission,
        
        DeleteDocumentTypePermission,
        
        UpdateDocumentTypePermission,
        
        GetAnnotationPermission,
        
        AddAnnotationPermission,
        
        DeleteAnnotationPermission,
        
        UpdateAnnotationPermission,
        
        AddLookupMapping,
        
        UpdateLookupMapping,
        
        DeleteLookupMapping,
        
        AddLookupInfo,
        
        UpdateLookupInfo,
        
        DeleteLookupInfo,
        
        AddPicklist,
        
        UpdatePicklist,
        
        DeletePicklist,
        
        AddOCRTemplate,
        
        AddAmbiguousDefinition,
        
        UpdateAmbiguousDefinition,
        
        DeleteAmbiguousDefinition,
        
        GetAmbiguousDefinition,
        
        DeleteLanguage,
        
        UpdateLanguage,
        
        AddLanguage,
        
        AddBarcodeConfig,
        
        UpdateBarcodeConfig,
        
        DeleteBarcodeConfig,
        
        DeleteOCRTemplate,
        
        AddOCRTemplatePage,
        
        DeleteOCRTemplatePage,
        
        AddOCRTemplateZone,
        
        DeleteOCRTemplateZone,
        
        OpenDocument,
        
        DownloadDocument,
        
        CreateIndex,
        
        AddDocumentVersion,
        
        RestoreDocumentVersion,
        
        AddPageVersion,
        
        AddAnnotationVersion,
        
        AddFieldVersionValue,
        
        AddAuditPermission,
        
        DeleteAuditPermission,
        
        GetAuditPermission,
        
        UpdateAuditPermission,
        
        DeleteIndex,
        
        UpdateIndex,
        
        ExtractText,
        
        DeleteLinkDoc,
        
        AddLinkDoc,
        
        GetLinkDoc,
        
        UpdateLinkDoc
    }

    [DataContract]
    public enum ObjectType
    {
        
        Document,
        
        Page,
        
        LinkDocument
    }

    public enum ChangeAction
    {
        DeleteDocumentType,
        DeleteDocument,
        AppendPage,
        ReplacePage,
        DeletePage,
        UpdateFieldValue,
        RotatePage,
        AddAnnotation,
        UpdateAnnotation,
        DeleteAnnotation,
        UpdateDocument
    }

    public enum Status
    {
        Exiting,
        Deleted,
        DeleteWirhDocType
    }

    [Flags]
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
        UpdateIndex = 1024 // Doc, Batch level
    }

}
