using System.Runtime.Serialization;
using System;
namespace Ecm.CaptureDomain
{
    [DataContract]
    public enum FieldDataType
    {
        [EnumMember]
        String,
        [EnumMember]
        Integer,
        [EnumMember]
        Decimal,
        [EnumMember]
        Picklist,
        [EnumMember]
        Boolean,
        [EnumMember]
        Date,
        [EnumMember]
        Folder,
        [EnumMember]
        Table
    }

    [DataContract]
    public enum ActionName
    {
        [EnumMember]
        GetUser,
        [EnumMember]
        AddUser,
        [EnumMember]
        UpdateUser,
        [EnumMember]
        DeleteUser,
        [EnumMember]
        ChangePassword,
        [EnumMember]
        GetUserGroup,
        [EnumMember]
        AddUserGroup,
        [EnumMember]
        UpdateUserGroup,
        [EnumMember]
        DeleteUserGroup,
        [EnumMember]
        AddMemberShip,
        [EnumMember]
        UpdateMembership,
        [EnumMember]
        DeleteMemberShip,
        [EnumMember]
        GetDocumentType,
        [EnumMember]
        AddDocumentType,
        [EnumMember]
        UpdateDocumentType,
        [EnumMember]
        DeleteDocumentType,
        [EnumMember]
        GetFieldMetaData,
        [EnumMember]
        AddFieldMetaData,
        [EnumMember]
        UpdateFieldMetaData,
        [EnumMember]
        DeleteFieldMetaData,
        [EnumMember]
        GetFieldValue,
        [EnumMember]
        AddFieldValue,
        [EnumMember]
        UpdateFieldValue,
        [EnumMember]
        DeleteFieldValue,
        [EnumMember]
        DoSearchDocument,
        [EnumMember]
        DoGlobalSearchDocument,
        [EnumMember]
        ViewDocument,
        [EnumMember]
        GetDocument,
        [EnumMember]
        AddDocument,
        [EnumMember]
        UpdateDocument,
        [EnumMember]
        DeleteDocument,
        [EnumMember]
        ViewPage,
        [EnumMember]
        ReplacePage,
        [EnumMember]
        InsertPage,
        [EnumMember]
        UpdatePage,
        [EnumMember]
        RotatePage,
        [EnumMember]
        DeletePage,
        [EnumMember]
        AddAnnotation,
        [EnumMember]
        UpdateAnnotation,
        [EnumMember]
        DeleteAnnotation,
        [EnumMember]
        SendEmail,
        [EnumMember]
        Print,
        [EnumMember]
        Login,
        [EnumMember]
        Save,
        [EnumMember]
        GetDocumentTypePermission,
        [EnumMember]
        AddDocumentTypePermission,
        [EnumMember]
        DeleteDocumentTypePermission,
        [EnumMember]
        UpdateDocumentTypePermission,
        [EnumMember]
        GetAnnotationPermission,
        [EnumMember]
        AddAnnotationPermission,
        [EnumMember]
        DeleteAnnotationPermission,
        [EnumMember]
        UpdateAnnotationPermission,
        [EnumMember]
        AddLookupMapping,
        [EnumMember]
        UpdateLookupMapping,
        [EnumMember]
        DeleteLookupMapping,
        [EnumMember]
        AddLookupInfo,
        [EnumMember]
        UpdateLookupInfo,
        [EnumMember]
        DeleteLookupInfo,
        [EnumMember]
        AddPicklist,
        [EnumMember]
        UpdatePicklist,
        [EnumMember]
        DeletePicklist,
        [EnumMember]
        AddOCRTemplate,
        [EnumMember]
        AddAmbiguousDefinition,
        [EnumMember]
        UpdateAmbiguousDefinition,
        [EnumMember]
        DeleteAmbiguousDefinition,
        [EnumMember]
        GetAmbiguousDefinition,
        [EnumMember]
        DeleteLanguage,
        [EnumMember]
        UpdateLanguage,
        [EnumMember]
        AddLanguage,
        [EnumMember]
        AddBarcodeConfig,
        [EnumMember]
        UpdateBarcodeConfig,
        [EnumMember]
        DeleteBarcodeConfig,
        [EnumMember]
        DeleteOCRTemplate,
        [EnumMember]
        AddOCRTemplatePage,
        [EnumMember]
        DeleteOCRTemplatePage,
        [EnumMember]
        AddOCRTemplateZone,
        [EnumMember]
        DeleteOCRTemplateZone,
        [EnumMember]
        OpenDocument,
        [EnumMember]
        DownloadDocument,
        [EnumMember]
        CreateIndex,
        [EnumMember]
        GetBatchType,
        [EnumMember]
        CreateBatchType,
        [EnumMember]
        UpdateBatchType,
        [EnumMember]
        DeleteBatchType,
        [EnumMember]
        CreateBatch,
        [EnumMember]
        UpdateBatch,
        [EnumMember]
        DeleteBatch,
        [EnumMember]
        ApprovedBatch,
        [EnumMember]
        RejectedBatch,
        [EnumMember]
        DelegatedBatch,
        [EnumMember]
        LockedBatch,
        [EnumMember]
        UnlockedBatch,
        [EnumMember]
        ResumeBatch,
        [EnumMember]
        GetLockedBatch,
        [EnumMember]
        GetProcessingBatch,
        [EnumMember]
        GetAwaitingBatch,
        [EnumMember]
        GetErrorBatch,
        [EnumMember]
        DoAvancedSearchBatch,
        [EnumMember]
        ReleaseBatch,
        [EnumMember]
        ReleaseToArchive,
        [EnumMember]
        UpdateBatchAfterProcessBarcode,
        [EnumMember]
        CreateBatchFieldValue,
        [EnumMember]
        UpdateBatchFieldValue,
        [EnumMember]
        DeleteBatchFieldValue,
        [EnumMember]
        CreateBatchFieldMetaData,
        [EnumMember]
        UpdateBatchFieldMetaData,
        [EnumMember]
        DeleteBatchFieldMetaData,
        [EnumMember]
        GetBatchFieldMetaData,
        [EnumMember]
        GetBatchFieldValue,
        [EnumMember]
        AddComment,
        [EnumMember]
        GetComment,
        [EnumMember]
        GetBatchItem,
        [EnumMember]
        GetBatchData,
        [EnumMember]
        ProcessBarcode,
        [EnumMember]
        SentNotify,
        [EnumMember]
        ReloadSearchResult,
        [EnumMember]
        StartLookup,
        [EnumMember]
        DoLookup,
        [EnumMember]
        EndLookup,
        [EnumMember]
        GetRejectedBatch,
        [EnumMember]
        RejectedDocument,
        [EnumMember]
        RejectedPage

    }

    [DataContract]
    public enum ObjectType
    {
        [EnumMember]
        Batch,
        [EnumMember]
        Document,
        [EnumMember]
        Page,
    }

    [DataContract]
    public enum LookupDataSourceType
    {
        [EnumMember]
        Table = 0,
        [EnumMember]
        View = 1,
        [EnumMember]
        StoredProcedure = 2,
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
        Reject = 1024, // Doc/Batch/Page level
        UnReject = 2048, // Doc/Batch/Page level
        UpdateIndex = 4096, // Doc, Batch level
        ChangeBatchType = 8192 // batch level
    }
    
    [DataContract]
    public enum SearchOperator
    {
        [EnumMember]
        Equal = 0,
        [EnumMember]
        GreaterThan = 1,
        [EnumMember]
        GreaterThanOrEqualTo = 2,
        [EnumMember]
        LessThan = 3,
        [EnumMember]
        LessThanOrEqualTo = 4,
        [EnumMember]
        InBetween = 5,
        [EnumMember]
        Contains = 6,
        [EnumMember]
        NotContains = 7,
        [EnumMember]
        NotEqual = 8,
        [EnumMember]
        StartsWith = 9,
        [EnumMember]
        EndsWith = 10,
    }

    [DataContract]
    public enum SearchConjunction
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        And = 0,
        [EnumMember]
        Or = 1,
    }

    [DataContract]
    public enum BatchHistoryAction
    {
        [EnumMember]
        SubmitBatch,
        [EnumMember]
        ApprovedBatch,
        [EnumMember]
        RejectedBatch,
        [EnumMember]
        DeletedBatch,
        [EnumMember]
        ResumeBatch,
        [EnumMember]
        LockBatch,
        [EnumMember]
        UnLockBatch,
        [EnumMember]
        AddCaptureDocument,
        [EnumMember]
        DeleteCaptureDocument,
        [EnumMember]
        UpdateCaptureDocument,
        [EnumMember]
        AddCapturePage,
        [EnumMember]
        DeleteCapturePage,
        [EnumMember]
        ReplaceCapturePage,
        [EnumMember]
        AppendCapturePage,
        [EnumMember]
        ReorderCapturePage,
        [EnumMember]
        RotateCapturePage,
        [EnumMember]
        UpdateDocumentFieldValue,
        [EnumMember]
        UpdateBatchFieldValue,
        [EnumMember]
        AddCaptureAnnotation,
        [EnumMember]
        UpdateCaptureAnnotation,
        [EnumMember]
        DeleteCaptureAnnotaton,
        [EnumMember]
        AddReleaseBatch,
        [EnumMember]
        DeleteReleaseBatch,
        [EnumMember]
        AddReleaseDocument,
        [EnumMember]
        DeleteReleaseDocument,
        [EnumMember]
        AddReleaseDocumentFieldValue,
        [EnumMember]
        DeleteReleaseDocumentFieldValue,
        [EnumMember]
        AddRelasePage,
        [EnumMember]
        DeleteReleasePage,
        [EnumMember]
        AddCaptureComment,
        [EnumMember]
        DeleteCaptureCommemt,
        [EnumMember]
        AddReleaseComment,
        [EnumMember]
        DeleteReleaseComment,
        [EnumMember]
        DelegateBatch,
        [EnumMember]
        UpdateBatchData
    }

    [DataContract]
    public enum BatchStatus
    {
        [EnumMember]
        Locked,
        [EnumMember]
        InProcessing,
        [EnumMember]
        Reject,
        [EnumMember]
        Available,
        [EnumMember]
        Error
    }
}
