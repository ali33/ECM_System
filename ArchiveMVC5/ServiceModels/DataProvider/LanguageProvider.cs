using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchiveMVC5.Models.DataProvider
{
    public class LanguageProvider : ProviderBase
    {

        public LanguageProvider(string userName, string password)
        {
            Configure(userName, password);
        }
        /// <summary>
        /// Lấy Language theo id
        /// </summary>
        /// <param name="id">id Language cần lấy</param>
        /// <returns></returns>
        public LanguageModel GetLanguage(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetLanguageModel(client.Channel.GetLanguage(id));
            }
        }


        /// <summary>
        /// Lấy danh sách Language
        /// </summary>
        /// <returns></returns>
        public List<LanguageModel> GetLanguages()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetLanguageModels(client.Channel.GetLanguages().ToList());
            }
        }
    }
}
