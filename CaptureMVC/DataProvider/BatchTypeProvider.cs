using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
namespace CaptureMVC.DataProvider
{
    public class BatchTypeProvider : ProviderBase
    {

        public BatchType GetBatchType(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetBatchType(id);
            }
        }

        public BatchType CountBatch(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetBatchType(batchTypeId);
            }
        }

        //public ObservableCollection<BatchTypeModel> GetBatchTypes()
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return ObjectMapper.GetBatchTypeModels(client.Channel.GetBatchTypes());
        //    }
        //}

        public void DeleteBatchType(Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteBatchType(batchTypeId);
            }
        }
        public List<BatchType> GetBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetBatchTypes();
            }
        }
        public List<BatchType> GetCaptureBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetCaptureBatchType();
            }
        }

        public List<BatchType> GetAssignWorkBatchTypes()
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetAssignWorkBatchTypes();
            }
        }

        public void SaveBatchType(BatchType batchType)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveBatchType(batchType);
            }
        }

        public void SaveBarcodeConfiguration(string xml, Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveBarcodeConfiguration(xml, batchTypeId);
            }
        }
    }
}
