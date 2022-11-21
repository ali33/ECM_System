using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecm.Model.DataProvider
{
    public class LanguageProvider : ProviderBase
    {
        public LanguageModel GetLanguage(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetLanguageModel(client.Channel.GetLanguage(id));
            }
        }

        public List<LanguageModel> GetLanguages()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetLanguageModels(client.Channel.GetLanguages().ToList());
            }
        }
    }
}
