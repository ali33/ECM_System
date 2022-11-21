using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchiveMVC.Models.DataProvider
{
    public class AmbiguousDefinitionProvider : ProviderBase
    {
        public AmbiguousDefinitionProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// Lấy danh sách AmbiguousDefinitions theo languageId
        /// </summary>
        /// <param name="languageId">languageId của AmbiguousDefinitions cần lấy </param>
        /// <returns></returns>
        public List<AmbiguousDefinitionModel> GetAmbiguousDefinitions(Guid languageId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAmbiguousDefinitionModels(client.Channel.GetAmbiguousDefinitions(languageId).ToList());
            }
        }
        /// <summary>
        /// Lấy tất cả AmbiguousDefinitions
        /// </summary>
        /// <returns></returns>
        public List<AmbiguousDefinitionModel> GetAllAmbiguousDefinitions()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAmbiguousDefinitionModels(client.Channel.GetAllAmbiguousDefinitions().ToList());
            }
        }
        /// <summary>
        /// Xóa AmbiguousDefinitions theo Id
        /// </summary>
        /// <param name="Id">Id AmbiguousDefinitions cần xóa</param>
        public void Delete(Guid Id)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteAmbiguousDefinition(Id);
            }
        }
        /// <summary>
        /// Lưu AmbiguousDefinitions theo AmbiguousDefinitionModel
        /// </summary>
        /// <param name="ambiguous">AmbiguousDefinitionModel cần lưu </param>
        public void Save(AmbiguousDefinitionModel ambiguous)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.SaveAmbiguousDefinition(ObjectMapper.GetAmbiguousDefinition(ambiguous));
            }
        }
    }
}