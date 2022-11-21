using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptureMVC.Models
{
    public static class Constant
    {
        public const string NO_USER = "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A";
        public const string UserID = "UserID";
        public const string UserName = "UserName";
        public const string Password = "Password";
        public const string ITEMS_PER_PAGE = "ITEMS_PER_PAGE";
        public const int LIMIT_WIDTH_OF_PAGE_IMAGE = 600;
        public const int WIDTH_A4 = 600;
        public const string MailUser = "devs@innoria.com";
        public const string MailPass = "@bc123456";
        public const string EnglishGuid = "9a00ae12-1341-e311-a96b-0050568afbf2";
        public const string SETTINGS = "SETTINGS";

        public const string TimeOut = "END_SESSION";

        public const string KEY_ANOTATION_PERMISSION = "ANOTATION_PERMISSION";
        public const string KEY_DOCTYPE_PERMISSION = "DOCTYPE_PERMISSION";
        public const string KEY_FIELDS_VALUE = "VIEW_FIELDS_VALUE";
        public const string KEY_ERROR = "ERROR";
        public const string KEY_DOCUMENT_ID = "DOCUMENT_ID";
        public const string KEY_DOCUMENT_NAME = "DOCUMENT_NAME";
        public const string KEY_PAGE_LIST = "PAGE_LIST";

        public const string CONTROLLER_SEARCH = "Search";
        public const string CONTROLLER_CAPTURE = "Capture";
        public const string CONTROLLER_LOGIN = "Login";

        public const string ACTION_INDEX = "Index";
        public const string DOCUMENT_TYPE_ICON_FOLDER = "DocumentTypeIcon";
        public const string BATCH_TYPE_ICON_FOLDER = "BatchTypeIcon";


        /// <summary>
        /// Use to store the prefix key of assigned menu item in cache
        /// </summary>
        public const string ICON_ASSIGNED_MENU = "ICON_ASSIGNED_MENU";
        /// <summary>
        /// Use to store the key raw password in session
        /// </summary>
        public const string RAW_PASSWORD = "RAW_PASSWORD";
        /// <summary>
        /// Use to store the key language in session
        /// </summary>
        public const string LANGUAGE = "LANGUAGE";
        /// <summary>
        /// Use to store the key IsAdmin in session
        /// </summary>
        public const string IS_ADMIN = "IS_ADMIN";

        public const string SESSION_OPENED_BATCHES = "SESSION_OPENED_BATCHES";
        public const string SESSION_ACTIVE_OPENED_BATCH_ID = "SESSION_ACTIVE_OPENED_BATCH_ID";
        public const string SESSION_OPENED_BATCH_MENU = "SESSION_OPENED_BATCH_MENU";

        /// <summary>
        /// Key in app config, use to store path of temp files folder.
        /// Temp files folder use to store file for upload scanned files, store converted files
        /// </summary>
        public const string APP_KEY_FOLDER_TEMP_FILES = "FolderTempFiles";
        public const string SESSION_TEMP_FILE = "SESSION_TEMP_FILE";
        public const string SESSION_CACHE_NEW_PAGE = "SESSION_CACHE_NEW_PAGE";

        public static readonly List<string> SUPPORT_PREVIEW_FILE_EXTENSIONS = new List<string>() { 
            "doc", "docx", "odt", "xls", "xlsx", "ods", "ppt", "pptx", "odp", "pdf"
        };
        public static readonly List<string> MS_OFFICE_SUPPORT_FILE_EXTENSIONS = new List<string>() { 
            "doc", "docx", "xls", "xlsx", "ppt", "pptx"
        };

        public static readonly int MAX_SIZE_THUMBNAIL = 60;

        public const string TITLE_MESSAGE_DIALOG = "ECM Capture";

        public const int MAX_OF_MIN_SIZE = 816;

        public static readonly string[] DATE_FORMATS = new string[] { "yyyy-MM-dd HH:mm:ss.fff", 
                                                                       "yyyy-MM-dd hh:mm:ss.fff", 
                                                                       "yyyy-MM-dd", 
                                                                       "M/d/yyyy", 
                                                                       "MM/dd/yyyy", 
                                                                       "d/M/yyyy", 
                                                                       "dd/MM/yyyy" };

        /// <summary>Css template of transform origin</summary>
        public const string TRANSFORM_ORIGIN_TEMPLATE = "-moz-transform-origin: {0} {1}; " +
                                                        "-ms-transform-origin: {0} {1}; " +
                                                        "-o-transform-origin: {0} {1}; " +
                                                        "-webkit-transform-origin: {0} {1}; " +
                                                        "transform-origin: {0} {1};";
        /// <summary>Css position of annotation div</summary>
        public const string ANNO_STYLE_TEMPLATE = "width: {0}px; height: {1}px; left: {2}px; top: {3}px;";
        /// <summary>Css style of text</summary>
        public const string TEXT_STYLE_TEMPLATE =
            "color: {0}; font-size: {1}; font-weight: {2}; font-style: {3}; text-decoration: {4};";

        /// <summary>Prefix XML of text document</summary>
        public const string SECTION_TEXT_PREFIX =
            "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";
        /// <summary>Suffix XML of text document</summary>
        public const string SECTION_TEXT_SUFFIX = "</Section>";
        public const string SECTION_TEXT_TEMPLATE = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\" TextAlignment=\"Left\" LineHeight=\"Auto\" IsHyphenationEnabled=\"False\" xml:lang=\"en-us\" FlowDirection=\"LeftToRight\" NumberSubstitution.CultureSource=\"User\" NumberSubstitution.Substitution=\"AsCulture\" FontFamily=\"Segoe UI\" FontStyle=\"Normal\" FontWeight=\"Normal\" FontStretch=\"Normal\" FontSize=\"12\" Foreground=\"#FF000000\" Typography.StandardLigatures=\"True\" Typography.ContextualLigatures=\"True\" Typography.DiscretionaryLigatures=\"False\" Typography.HistoricalLigatures=\"False\" Typography.AnnotationAlternates=\"0\" Typography.ContextualAlternates=\"True\" Typography.HistoricalForms=\"False\" Typography.Kerning=\"True\" Typography.CapitalSpacing=\"False\" Typography.CaseSensitiveForms=\"False\" Typography.StylisticSet1=\"False\" Typography.StylisticSet2=\"False\" Typography.StylisticSet3=\"False\" Typography.StylisticSet4=\"False\" Typography.StylisticSet5=\"False\" Typography.StylisticSet6=\"False\" Typography.StylisticSet7=\"False\" Typography.StylisticSet8=\"False\" Typography.StylisticSet9=\"False\" Typography.StylisticSet10=\"False\" Typography.StylisticSet11=\"False\" Typography.StylisticSet12=\"False\" Typography.StylisticSet13=\"False\" Typography.StylisticSet14=\"False\" Typography.StylisticSet15=\"False\" Typography.StylisticSet16=\"False\" Typography.StylisticSet17=\"False\" Typography.StylisticSet18=\"False\" Typography.StylisticSet19=\"False\" Typography.StylisticSet20=\"False\" Typography.Fraction=\"Normal\" Typography.SlashedZero=\"False\" Typography.MathematicalGreek=\"False\" Typography.EastAsianExpertForms=\"False\" Typography.Variants=\"Normal\" Typography.Capitals=\"Normal\" Typography.NumeralStyle=\"Normal\" Typography.NumeralAlignment=\"Normal\" Typography.EastAsianWidths=\"Normal\" Typography.EastAsianLanguage=\"Normal\" Typography.StandardSwashes=\"0\" Typography.ContextualSwashes=\"0\" Typography.StylisticAlternates=\"0\">{0}</Section>";

        /// <summary>Highlight annotation type</summary>
        public const string ANNO_TYPE_HIGHLIGHT = "Highlight";
        /// <summary>Redaction annotation type</summary>
        public const string ANNO_TYPE_REDACTION = "Redaction";
        /// <summary>Text annotation type</summary>
        public const string ANNO_TYPE_TEXT = "Text";

        /// <summary>Loose doc kind</summary>
        public const string DOC_KIND_LOOSE = "LooseDoc";
        /// <summary>Temp doc kind</summary>
        public const string DOC_KIND_TEMP = "TempDoc";


        #region Capture controller

        /// <summary> Key in session store list captured batches.</summary>
        public const string SESSION_CAPTURE_BATCHES = "SESSION_CAPTURE_BATCHES";
        /// <summary> Key in session store list can captured batche types.</summary>
        public const string SESSION_CAPTURE_BATCH_TYPES = "SESSION_CAPTURE_BATCH_TYPES";
        /// <summary> Key in session store list captured pages.</summary>
        public const string SESSION_CAPTURE_PAGES = "SESSION_CAPTURE_PAGES";
        /// <summary> Key in session store dictionary ambiguous definition use in OCR.</summary>
        public const string SESSION_AMBIGUOUS_DEFINITION = "SESSION_AMBIGUOUS_DEFINITION";
        /// <summary> Key in session store dictionary ocr image.</summary>
        public const string SESSION_CAPTURE_OCR_IMAGES = "SESSION_CAPTURE_OCR_IMAGE";
        #endregion

        #region Session

        /// <summary>
        /// Key store the GUID of session
        /// </summary>
        public const string SESSION_GUID = "SESSION_GUID";

        /// <summary>
        /// Key store the working folder of session
        /// </summary>
        public const string SESSION_FOLDER = "SESSION_FOLDER";

        #endregion

    }
}