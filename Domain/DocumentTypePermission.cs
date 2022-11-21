using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the operational permission of a <see cref="UserGroup"/> on a <see cref="DocumentType"/>.
    /// </summary>
    [DataContract]
    public class DocumentTypePermission
    {
        /// <summary>
        /// Identifier of the permission
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of <see cref="DocumentType"/> object the permission effect on.
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Identifier of <see cref="UserGroup"/> object is assigned the permission.
        /// </summary>
        [DataMember]
        public Guid UserGroupId { get; set; }

        /// <summary>
        /// Allow user can delete pages of documents belong to the document type. This permission can be used to delete the documents as well.
        /// </summary>
        [DataMember]
        public bool AllowedDeletePage { get; set; }

        /// <summary>
        /// Allow user can append more page into the documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedAppendPage { get; set; }

        /// <summary>
        /// Allow user can replace a page in the documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedReplacePage { get; set; }

        /// <summary>
        /// Allow user can view the value of the restricted fields of the documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedSeeRetrictedField { get; set; }

        /// <summary>
        /// Allow user can update the value for fields of the documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedUpdateFieldValue { get; set; }

        /// <summary>
        /// Allow user can print the documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AlowedPrintDocument { get; set; }

        /// <summary>
        /// Allow user can email the documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedEmailDocument { get; set; }

        /// <summary>
        /// Allow user can rotate image pages of the documents belong the the document type.
        /// </summary>
        [DataMember]
        public bool AllowedRotatePage { get; set; }

        /// <summary>
        /// Allow user can export the value of fields in the document.
        /// </summary>
        [DataMember]
        public bool AllowedExportFieldValue { get; set; }

        /// <summary>
        /// Allow user can save the documents into the local machine.
        /// </summary>
        [DataMember]
        public bool AllowedDownloadOffline { get; set; }

        /// <summary>
        /// Allow user can hide all annotations in the document.
        /// </summary>
        [DataMember]
        public bool AllowedHideAllAnnotation { get; set; }

        /// <summary>
        /// Allow user can capture documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedCapture { get; set; }

        /// <summary>
        /// Allow user can search documents belong to the document type.
        /// </summary>
        [DataMember]
        public bool AllowedSearch { get; set; }

        /// <summary>
        /// This method is used internally.
        /// </summary>
        public static DocumentTypePermission GetAllowAll()
        {
            return new DocumentTypePermission
            {
                AllowedAppendPage = true,
                AllowedCapture = true,
                AllowedDeletePage = true,
                AllowedDownloadOffline = true,
                AllowedEmailDocument = true,
                AllowedExportFieldValue = true,
                AllowedHideAllAnnotation = true,
                AllowedReplacePage = true,
                AllowedRotatePage = true,
                AllowedSearch = true,
                AllowedSeeRetrictedField = true,
                AllowedUpdateFieldValue = true,
                AlowedPrintDocument = true
            };
        }
    }
}