using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class OCRTemplate
    {
        public OCRTemplate()
        {
            OCRTemplatePages = new List<OCRTemplatePage>();
        }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public Guid LanguageId { get; set; }

        [DataMember]
        public Language Language { get; set; }

        /// <summary>
        /// Collections of pages in the template.
        /// </summary>
        [DataMember]
        public List<OCRTemplatePage> OCRTemplatePages { get; set; }

        public string FileExtension { get; set; }
    }
}
