using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ecm.CaptureDomain;

namespace Ecm.CaptureModel.DataProvider
{
    public class UserProvider : ProviderBase
    {
        public ObservableCollection<UserModel> GetUsers()
        {
            using (var client = GetCaptureClientChannel())
            {
                IList<User> users = client.Channel.GetUsers();
                return ObjectMapper.GetUserModels(users);
            }
        }

        public void SaveUser(UserModel userModel)
        {
            User user = ObjectMapper.GetUser(userModel);

            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveUser(user);
            }
        }

        public void DeleteUser(UserModel user)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteUser(ObjectMapper.GetUser(user));
            }
        }

        public ObservableCollection<UserModel> GetAvailableUsersToDelegate()
        {
            using (var client = GetCaptureClientChannel())
            {
                IList<User> users = client.Channel.GetAvailableUserToDelegation();
                return ObjectMapper.GetUserModels(users);
            }
        }

        public UserModel GetUser(string username)
        {
            using (var client = GetCaptureClientChannel())
            {
                User user = client.Channel.GetUserByUserName(username);
                return ObjectMapper.GetUserModel(user);
            }
        }
    }
}