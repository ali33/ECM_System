using System.Collections.Generic;
using Ecm.Mvvm;
using System;

namespace Ecm.Model
{
    public class OCRTemplateModel : BaseDependencyProperty
    {
        private LanguageModel _language;

        public OCRTemplateModel()
        {
            OCRTemplatePages = new List<OCRTemplatePageModel>();
        }

        public Guid DocTypeId { get; set; }

        public LanguageModel Language{ get; set; }

        public IList<OCRTemplatePageModel> OCRTemplatePages { get; set; }
    }
}
