using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecm.Model.DataProvider
{
    public class AmbiguousDefinitionProvider : ProviderBase
    {
        public List<AmbiguousDefinitionModel> GetAmbiguousDefinitions(Guid languageId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAmbiguousDefinitionModels(client.Channel.GetAmbiguousDefinitions(languageId).ToList());
            }
        }

        public List<AmbiguousDefinitionModel> GetAllAmbiguousDefinitions()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAmbiguousDefinitionModels(client.Channel.GetAllAmbiguousDefinitions().ToList());
            }
        }

        public void Delete(Guid Id)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteAmbiguousDefinition(Id);
            }
        }

        public void Save(AmbiguousDefinitionModel ambiguous)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.SaveAmbiguousDefinition(ObjectMapper.GetAmbiguousDefinition(ambiguous));
            }
        }
    }
}