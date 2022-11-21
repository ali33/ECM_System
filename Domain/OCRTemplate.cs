using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the OCR template that define on each <see cref="DocumentType"/>. In general, OCR template specify which field of document will extract data from which zone of the image automatically by OCR engine.
    /// </summary>
    [DataContract]
    public class OCRTemplate
    {
        /// <summary>
        /// Initialize new template object
        /// </summary>
        public OCRTemplate()
        {
            OCRTemplatePages = new List<OCRTemplatePage>();
        }

        /// <summary>
        /// Identifier of the <see cref="DocumentType"/> object contains the template definition
        /// </summary>
        [DataMember]
        public Guid DocTypeId
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of the <see cref="Language"/> object the OCR template is defined on.
        /// </summary>
        [DataMember]
        public Guid LanguageId { get; set; }

        /// <summary>
        /// The <see cref="Language"/> object.
        /// </summary>
        [DataMember]
        public Language Language
        {
            get;
            set;
        }

        /// <summary>
        /// Collections of pages in the template.
        /// </summary>
        [DataMember]
        public List<OCRTemplatePage> OCRTemplatePages { get; set; }
    }
}