using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.LookupDomain;
using Ecm.Utility;
using System;
using System.Collections.Generic;

namespace Ecm.CaptureCore
{
    public class BatchFieldMetaDataManager : ManagerBase
    {

        public BatchFieldMetaDataManager(User loginUser)
            : base(loginUser)
        {
        }

        #region Public methods


        public List<BatchFieldMetaData> GetByBatchType(Guid batchTypeId)
        {
            using (DapperContext context = new DapperContext(LoginUser))
            {
                var bfmdDao = new BatchFieldMetaDataDao(context);
                var fields = bfmdDao.GetByBatchType(batchTypeId);

                foreach (var item in fields)
                {
                    if (item.IsLookup && !string.IsNullOrEmpty(item.LookupXml))
                    {
                        item.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(item.LookupXml);
                    }
                }

                return fields;
            }
        }

        #endregion
    }
}
