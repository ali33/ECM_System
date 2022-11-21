using System.Collections.Generic;
using Ecm.Mvvm;
using System;

namespace Ecm.CaptureModel
{
    public class OCRTemplateModel : BaseDependencyProperty
    {
        public OCRTemplateModel()
        {
            OCRTemplatePages = new List<OCRTemplatePageModel>();
        }

        public Guid DocTypeId { get; set; }

        public LanguageModel Language { get; set; }

        public IList<OCRTemplatePageModel> OCRTemplatePages { get; set; }
    }
}
