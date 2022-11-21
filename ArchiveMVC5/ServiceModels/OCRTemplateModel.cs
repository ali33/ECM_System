using System.Collections.Generic;

using System;

namespace ArchiveMVC5.Models
{
    public class OCRTemplateModel
    {
        public OCRTemplateModel()
        {
            OCRTemplatePages = new List<OCRTemplatePageModel>();
        }

        public Guid DocTypeId { get; set; }

        public string FileExtension { get; set; }

        public LanguageModel Language { get; set; }

        public IList<OCRTemplatePageModel> OCRTemplatePages { get; set; }
    }
}
