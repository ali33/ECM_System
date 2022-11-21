namespace Ecm.Model.DataProvider
{
    public class SettingProvider : ProviderBase
    {
        public SettingModel GetSettings()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetSettingModel(client.Channel.GetSettings());
            }
        }

        public void WriteSetting(SettingModel setting)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.WriteSetting(ObjectMapper.GetSetting(setting));
            }
        }
    }
}
