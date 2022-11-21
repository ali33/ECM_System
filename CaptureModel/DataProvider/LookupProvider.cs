
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Ecm.CaptureModel.DataProvider
{
    public class LookupProvider : ProviderBase
    {
        public DataTable GetLookupData(LookupInfoModel lookupInfoModel, string value)
        {
            using (var client = GetCaptureClientChannel())
            {
                var lookup = ObjectMapper.GetLookupInfo(lookupInfoModel);
                return client.Channel.GetLookupData(lookup, value);
            }
        }
    }
}
