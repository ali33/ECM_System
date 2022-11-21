using System.Collections.Generic;
using Ecm.Domain;


namespace ArchiveMVC.Models.DataProvider
{
    public class UserProvider : ProviderBase
    {
        public UserProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// lấy User
        /// </summary>
        /// <returns></returns>
        public List<UserModel> GetUsers()
        {
            using (var client = GetArchiveClientChannel())
            {
                IList<User> users = client.Channel.GetUsers();
                return ObjectMapper.GetUserModels(users);
            }
        }
            /// <summary>
            /// lưu User
            /// </summary>
        /// <param name="userModel">UserModel</param>
        public void SaveUser(UserModel userModel)
        {
            User user = ObjectMapper.GetUser(userModel);

            using (var client = GetArchiveClientChannel())
            {
                client.Channel.SaveUser(user);
            }
        }


        /// <summary>
        /// xóa User
        /// </summary>
        /// <param name="user">UserModel </param>
        public void DeleteUser(UserModel user)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteUser(ObjectMapper.GetUser(user));
            }
        }
    }
}