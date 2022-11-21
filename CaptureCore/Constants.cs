using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.CaptureCore
{
    public class Constants
    {
        public const string ITEMS_PER_PAGE = "SearchResultPageSize";
        public const int MAX_ITEMS_PER_PAGE = 50;

        public const string CONJUNCTION_NONE_UPPER = "NONE";

        public const string OPERATOR_EQUAL_UPPER = "EQUAL";
        public const string OPERATOR_NOT_EQUAL_UPPER = "NOTEQUAL";
        public const string OPERATOR_GREATER_THAN_UPPER = "GREATERTHAN";
        public const string OPERATOR_GREATER_THAN_OR_EQUAL_TO_UPPER = "GREATERTHANOREQUALTO";
        public const string OPERATOR_LESS_THAN_UPPER = "LESSTHAN";
        public const string OPERATOR_LESS_THAN_OR_EQUAL_TO_UPPER = "LESSTHANOREQUALTO";
        public const string OPERATOR_IN_BEWTEEN_UPPER = "INBETWEEN";
        public const string OPERATOR_CONTAINS_UPPER = "CONTAINS";
        public const string OPERATOR_NOT_CONTAINS_UPPER = "NOTCONTAINS";
        public const string OPERATOR_ENDS_WITH_UPPER = "ENDSWITH";
        public const string OPERATOR_STARTS_WITH_UPPER = "STARTSWITH";

        public const string DATA_TYPE_STRING_UPPER = "STRING";
        public const string DATA_TYPE_INTEGER_UPPER = "INTEGER";
        public const string DATA_TYPE_DECIMAL_UPPER = "DECIMAL";
        public const string DATA_TYPE_BOOLEAN_UPPER = "BOOLEAN";
        public const string DATA_TYPE_DATE_UPPER = "DATE";
        public const string DATA_TYPE_PICKLIST_UPPER = "PICKLIST";

        /// <summary>Date format yyyy-MM-dd</summary>
        public const string DATE_FORMAT_DB = "yyyy-MM-dd";
        /// <summary>Date format yyyy-MM-dd HH:mm:ss.fff</summary>
        public const string DATE_FULL_FORMAT_DB = "yyyy-MM-dd HH:mm:ss.fff";
        /// <summary>Date formats use in parser: "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd", "yyyy-M-d"</summary>
        public static readonly string[] DATE_FORMATS = new string[] 
        { 
            "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd", "yyyy-M-d", "dd/MM/yyyy", "d/M/yyyy"
        };

        /// <summary>Annotation Highlight type</summary>
        public const string ANNO_TYPE_HIGHLIGHT = "Highlight";
        /// <summary>Annotation Redaction type</summary>
        public const string ANNO_TYPE_REDACTION = "Redaction";
        /// <summary>Annotation Text type</summary>
        public const string ANNO_TYPE_TEXT = "Text";

        /// <summary>Template of text annotation use for mobile</summary>
        public const string ANNO_TEXT_TEMPLATE = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\" TextAlignment=\"Left\" LineHeight=\"Auto\" IsHyphenationEnabled=\"False\" xml:lang=\"en-us\" FlowDirection=\"LeftToRight\" NumberSubstitution.CultureSource=\"User\" NumberSubstitution.Substitution=\"AsCulture\" FontFamily=\"Segoe UI\" FontStyle=\"Normal\" FontWeight=\"Normal\" FontStretch=\"Normal\" FontSize=\"12\" Foreground=\"#FF000000\" Typography.StandardLigatures=\"True\" Typography.ContextualLigatures=\"True\" Typography.DiscretionaryLigatures=\"False\" Typography.HistoricalLigatures=\"False\" Typography.AnnotationAlternates=\"0\" Typography.ContextualAlternates=\"True\" Typography.HistoricalForms=\"False\" Typography.Kerning=\"True\" Typography.CapitalSpacing=\"False\" Typography.CaseSensitiveForms=\"False\" Typography.StylisticSet1=\"False\" Typography.StylisticSet2=\"False\" Typography.StylisticSet3=\"False\" Typography.StylisticSet4=\"False\" Typography.StylisticSet5=\"False\" Typography.StylisticSet6=\"False\" Typography.StylisticSet7=\"False\" Typography.StylisticSet8=\"False\" Typography.StylisticSet9=\"False\" Typography.StylisticSet10=\"False\" Typography.StylisticSet11=\"False\" Typography.StylisticSet12=\"False\" Typography.StylisticSet13=\"False\" Typography.StylisticSet14=\"False\" Typography.StylisticSet15=\"False\" Typography.StylisticSet16=\"False\" Typography.StylisticSet17=\"False\" Typography.StylisticSet18=\"False\" Typography.StylisticSet19=\"False\" Typography.StylisticSet20=\"False\" Typography.Fraction=\"Normal\" Typography.SlashedZero=\"False\" Typography.MathematicalGreek=\"False\" Typography.EastAsianExpertForms=\"False\" Typography.Variants=\"Normal\" Typography.Capitals=\"Normal\" Typography.NumeralStyle=\"Normal\" Typography.NumeralAlignment=\"Normal\" Typography.EastAsianWidths=\"Normal\" Typography.EastAsianLanguage=\"Normal\" Typography.StandardSwashes=\"0\" Typography.ContextualSwashes=\"0\" Typography.StylisticAlternates=\"0\">{0}</Section>";
        /// <summary>Default annotation property</summary>
        public const string LINE_START_AT = "TopLeft";
        /// <summary>Default annotation property</summary>
        public const string LINE_END_AT = "TopLeft";
        /// <summary>Default annotation property</summary>
        public const string LINE_STYLE = "ArrowAtEnd";
        /// <summary>Default annotation property</summary>
        public const string LINE_COLOR = null;
        /// <summary>Default annotation property</summary>
        public const int LINE_WEIGHT = 0;

        /// <summary>Default content language code English</summary>
        public const string CONTENT_LANGUAGE_CODE_DEFAULT = "eng";

        /// <summary>Status of batch</summary>
        public const string STATUS_MESSAGE_CREATING_BATCH = "Creating batch";

        /// <summary>Log message create new batch</summary>
        public const string LOG_CREATE_BATCH = "Create new batch Id: ";
        /// <summary>Log message create new batch field value</summary>
        public const string LOG_CREATE_BATCH_FIELD_VALUE = "Add batch field value for batch Id: ";
        /// <summary>Log message create new comment</summary>
        public const string LOG_CREATE_COMMENT = "Add comment for batch Id: ";
        /// <summary>Log message create new document</summary>
        public const string LOG_CREATE_DOC = "Create new content Id: {0} on batch Id: {1}";
        /// <summary>Log message create new batch field value</summary>
        public const string LOG_CREATE_DOC_FIELD_VALUE = "Add content field value for content Id: ";
        /// <summary>Log message create new page</summary>
        public const string LOG_CREATE_PAGE = "Add pages on document Id: ";

        /// <summary>Log message update batch</summary>
        public const string LOG_UPDATE_BATCH = "Update batch Id: ";
        /// <summary>Log message update batch field value</summary>
        public const string LOG_UPDATE_BATCH_FIELD_VALUE = "Update batch field value for batch Id: ";
        /// <summary>Log message update document</summary>
        public const string LOG_UPDATE_DOC = "Update content Id: {0} on batch Id: {1}";
        /// <summary>Log message delete document</summary>
        public const string LOG_DELETE_DOC = "Delete content Id: {0} on batch Id: {1}";
        /// <summary>Log message update page</summary>
        public const string LOG_UPDATE_PAGE = "Update pages on document Id: ";

        /// <summary>Log message approve batch</summary>
        public const string LOG_APPROVE_BATCH = "Approved work item: {0} successfully.";
        /// <summary>Log message reject batch</summary>
        public const string LOG_REJECT_BATCH = "Reject work item: {0} successfully.";

        /// <summary>Action approve batch key</summary>
        public const string ACTION_APPROVE_BATCH = "approve";
        /// <summary>Action reject batch key</summary>
        public const string ACTION_REJECT_BATCH = "reject";

    }
}
