using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ecm.CaptureModel.DataProvider
{
    public class DocTypeProvider : ProviderBase
    {
        public void SaveOcrTemplate(OCRTemplateModel OCRTemplate)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveOcrTemplate(ObjectMapper.GetOCRTemplate(OCRTemplate));
            }
        }

        public void DeleteOcrTemplate(Guid docTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteOcrTemplate(docTypeId);
            }
        }

        //public List<BarcodeConfigurationModel> GetBarcodeConfigurations(long docTypeId)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return ObjectMapper.GetBarcodeConfigurationModels(client.Channel.GetBarcodeConfigurations(docTypeId).ToList());
        //    }
        //}

        //public void DeleteBarcodeConfiguration(long id)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        client.Channel.DeleteBarcodeConfiguration(id);
        //    }
        //}

        //public void SaveBarcodeConfiguration(BarcodeConfigurationModel barcode)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        client.Channel.SaveBarcodeConfiguration(ObjectMapper.GetBarcodeConfiguration(barcode));
        //    }
        //}

        //public void ClearBarcodeConfigurations(long documentTypeId)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        client.Channel.ClearBarcodeConfigurations(documentTypeId);
        //    }
        //}

        public ObservableCollection<DocTypeModel> GetDocTypes(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetDocTypeModels(client.Channel.GetDocumentTypes(batchTypeId));
            }
        }

        public DocTypeModel GetDocType(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetDocTypeModel(client.Channel.GetDocumentType(id));
            }
        }

        public List<Guid> CheckDocTypeHaveDocument(List<Guid> docTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.CheckDocTypeHaveDocument(docTypeId);
            }
        }
    }
}
