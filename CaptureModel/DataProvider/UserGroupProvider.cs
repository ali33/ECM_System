using System.Collections.ObjectModel;
using Ecm.CaptureDomain;

namespace Ecm.CaptureModel.DataProvider
{
    public class UserGroupProvider : ProviderBase
    {
        public ObservableCollection<UserGroupModel> GetUserGroups()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetUserGroupModels(client.Channel.GetUserGroups());
            }
        }

        public void Save(UserGroupModel userGroupModel)
        {
            UserGroup userGroup = ObjectMapper.GetUserGroup(userGroupModel);

            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveUserGroup(userGroup);
            }
        }

        public void DeleteUserGroup(UserGroupModel userGroup)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteUserGroup(ObjectMapper.GetUserGroup(userGroup));
            }
        }
    }
}
