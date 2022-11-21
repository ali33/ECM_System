using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;

namespace CaptureMVC.DataProvider
{
    public class SettingsProvider : ProviderBase
    {
        /// <summary>
        /// Lấy Language theo id
        /// </summary>
        /// <param name="id">id Language cần lấy</param>
        /// <returns></returns>
        public Setting GetSettings()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetSettings();
            }
        }
    }
}
