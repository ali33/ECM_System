using System.Collections.Generic;
using Ecm.Mvvm;
using System;

namespace Ecm.ContentViewer.Model
{
    public class OCRTemplateModel : BaseDependencyProperty
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
