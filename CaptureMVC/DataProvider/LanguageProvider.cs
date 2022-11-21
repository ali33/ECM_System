using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;

namespace CaptureMVC.DataProvider
{
    public class LanguageProvider : ProviderBase
    {
        //public LanguageProvider(string userName, string password)
        //{
        //    Configure(userName, password);
        //}
        /// <summary>
        /// Lấy Language theo id
        /// </summary>
        /// <param name="id">id Language cần lấy</param>
        /// <returns></returns>
        public Language GetLanguage(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetLanguage(id);
            }
        }

        /// <summary>
        /// Get List Language
        /// </summary>
        /// <returns></returns>
        public List<Language> GetLanguages()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetLanguages();
            }
        }
    }
}
