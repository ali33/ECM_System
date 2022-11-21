using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ecm.CaptureModel.DataProvider
{
    public class BatchTypeProvider : ProviderBase
    {
        public BatchTypeModel GetBatchType(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchTypeModel(client.Channel.GetBatchType(batchTypeId));
            }
        }

        public ObservableCollection<BatchTypeModel> GetBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchTypeModels(client.Channel.GetBatchTypes().OrderBy(h=>h.Name).ToList());
            }
        }

        public void DeleteBatchType(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteBatchType(batchTypeId);
            }
        }

        public ObservableCollection<BatchTypeModel> GetCaptureBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchTypeModels(client.Channel.GetCaptureBatchType());
            }
        }

        public ObservableCollection<BatchTypeModel> GetAssignWorkBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchTypeModels(client.Channel.GetAssignWorkBatchTypes());
            }
        }

        public void SaveBatchType(BatchTypeModel batchType)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveBatchType(ObjectMapper.GetBatchType(batchType));
            }
        }

        public void SaveBarcodeConfiguration(string xml, Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveBarcodeConfiguration(xml, batchTypeId);
            }
        }

        public bool CanEditBatchTypeField(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.CanEditBatchTypeField(batchTypeId);
            }
        }

    }
}
