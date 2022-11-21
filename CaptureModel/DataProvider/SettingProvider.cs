using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureModel;
namespace Ecm.CaptureModel.DataProvider
{
    public class SettingProvider : ProviderBase
    {
        public SettingModel GetSettings()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetSettingModel(client.Channel.GetSettings());
            }
        }

        public void WriteSetting(SettingModel setting)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.WriteSetting(ObjectMapper.GetSetting(setting));
            }
        }
    }
}
