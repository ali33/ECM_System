using System.Collections.Generic;
using Ecm.CaptureDomain;
using System.Collections.ObjectModel;

namespace CaptureMVC.DataProvider
{
    public class UserGroupProvider : ProviderBase
    {
        public List<UserGroup> GetUserGroups()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetUserGroups();
            }
        }

        /// <summary>
        /// Save UserGroupModel
        /// </summary>
        /// <param name="userGroupModel">UserGroupModel</param>
        public void Save(UserGroup userGroupModel)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveUserGroup(userGroupModel);
            }
        }

        /// <summary>
        /// Delete UserGroup 
        /// </summary>
        /// <param name="userGroup">UserGroupModel</param>
        public void DeleteUserGroup(UserGroup userGroup)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteUserGroup(userGroup);
            }
        }
    }
}