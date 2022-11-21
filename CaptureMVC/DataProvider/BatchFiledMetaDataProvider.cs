using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
namespace CaptureMVC.DataProvider
{
    public class BatchFiledMetaDataProvider : ProviderBase
    {
        /// <summary>
        /// Get list field of Batch Type.
        /// </summary>
        /// <param name="batchTypeId"></param>
        /// <returns></returns>
        public List<BatchFieldMetaData> GetFieldsFromBatchType(Guid batchTypeId)
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetFieldsFromBatchType(batchTypeId);
            }
        }
    }
}
