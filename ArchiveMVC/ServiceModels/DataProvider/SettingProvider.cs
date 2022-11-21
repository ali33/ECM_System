namespace ArchiveMVC.Models.DataProvider
{
    public class SettingProvider : ProviderBase
    {

        public SettingProvider(string userName, string password)
        {
            Configure(userName, password);
        }
        /// <summary>
        /// lấy hiệu chỉnh
        /// </summary>
        /// <returns></returns>
        public SettingModel GetSettings()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetSettingModel(client.Channel.GetSettings());
            }
        }
        /// <summary>
        /// ghi hiệu chỉnh
        /// </summary>
        /// <param name="setting">SettingModel</param>
        public void WriteSetting(SettingModel setting)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.WriteSetting(ObjectMapper.GetSetting(setting));
            }
        }
    }
}
