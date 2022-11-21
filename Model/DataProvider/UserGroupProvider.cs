using System.Collections.ObjectModel;
using Ecm.Domain;

namespace Ecm.Model.DataProvider
{
    public class UserGroupProvider : ProviderBase
    {
        public ObservableCollection<UserGroupModel> GetUserGroups()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetUserGroupModels(client.Channel.GetUserGroups());
            }
        }

        public void Save(UserGroupModel userGroupModel)
        {
            UserGroup userGroup = ObjectMapper.GetUserGroup(userGroupModel);

            using (var client = GetArchiveClientChannel())
            {
                client.Channel.SaveUserGroup(userGroup);
            }
        }

        public void DeleteUserGroup(UserGroupModel userGroup)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteUserGroup(ObjectMapper.GetUserGroup(userGroup));
            }
        }
    }
}
