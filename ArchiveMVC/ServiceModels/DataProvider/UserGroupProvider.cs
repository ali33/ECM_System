
using Ecm.Domain;
using System.Collections.Generic;

namespace ArchiveMVC.Models.DataProvider
{
    public class UserGroupProvider : ProviderBase
    {
        public UserGroupProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// lấy UserGroups
        /// </summary>
        /// <returns></returns>
        public List<UserGroupModel> GetUserGroups()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetUserGroupModels(client.Channel.GetUserGroups());
            }
        }
        /// <summary>
        /// lưu UserGroupModel
        /// </summary>
        /// <param name="userGroupModel">UserGroupModel</param>
        public void Save(UserGroupModel userGroupModel)
        {
            UserGroup userGroup = ObjectMapper.GetUserGroup(userGroupModel);

            using (var client = GetArchiveClientChannel())
            {
                client.Channel.SaveUserGroup(userGroup);
            }
        }
        /// <summary>
        /// xóa UserGroup 
        /// </summary>
        /// <param name="userGroup">UserGroupModel</param>
        public void DeleteUserGroup(UserGroupModel userGroup)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteUserGroup(ObjectMapper.GetUserGroup(userGroup));
            }
        }
    }
}
