using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
namespace CaptureMVC.DataProvider
{
    public class AmbiguousDefinitionProvider : ProviderBase
    {
        public List<AmbiguousDefinition> GetAmbiguousDefinition(Guid languageId)
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetAmbiguousDefinitions(languageId);
            }
        }
    }
}
