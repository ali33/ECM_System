using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Model;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.ContentViewer.ViewModel
{
    public class ImageViewerViewModel : BaseDependencyProperty
    {
        public event PromptOCRZoneEventHandler PromptOcrZone;

        public event UnPromptOCRZoneEventHandler UnPromptOcrZone;

        public void PromptOcr(int pageIndex, OCRTemplateZoneModel ocrTemplateZone, OCRTemplatePageModel ocrTemplatePage)
        {
            if (PromptOcrZone != null)
            {
                PromptOcrZone(pageIndex, ocrTemplateZone, ocrTemplatePage);
            }
        }

        public void UnPromptOcr()
        {
            if (UnPromptOcrZone != null)
            {
                UnPromptOcrZone();
            }
        }
    }
}
