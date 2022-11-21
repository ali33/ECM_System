using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecm.CaptureModel.DataProvider
{
    public class LanguageProvider : ProviderBase
    {
        public LanguageModel GetLanguage(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetLanguageModel(client.Channel.GetLanguage(id));
            }
        }

        public List<LanguageModel> GetLanguages()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetLanguageModels(client.Channel.GetLanguages().ToList());
            }
        }
    }
}
