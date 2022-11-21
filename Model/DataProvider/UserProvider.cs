using System.Collections.Generic;
using Ecm.Domain;
using System.Collections.ObjectModel;

namespace Ecm.Model.DataProvider
{
    public class UserProvider : ProviderBase
    {
        public ObservableCollection<UserModel> GetUsers()
        {
            using (var client = GetArchiveClientChannel())
            {
                IList<User> users = client.Channel.GetUsers();
                return ObjectMapper.GetUserModels(users);
            }
        }

        public void SaveUser(UserModel userModel)
        {
            User user = ObjectMapper.GetUser(userModel);

            using (var client = GetArchiveClientChannel())
            {
                client.Channel.SaveUser(user);
            }
        }

        public void DeleteUser(UserModel user)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteUser(ObjectMapper.GetUser(user));
            }
        }
    }
}