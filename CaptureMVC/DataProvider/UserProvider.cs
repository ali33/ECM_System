using System.Collections.Generic;
using Ecm.CaptureDomain;
using System.Collections.ObjectModel;

namespace CaptureMVC.DataProvider
{
    public class UserProvider : ProviderBase
    {
        //public UserProvider(string userName, string password)
        //{
        //    Configure(userName, password);
        //}
        /// <summary>
        /// lấy User
        /// </summary>
        /// <returns></returns>
        public List<User> GetAvailableUserToDelegation()
        {
            using (var client = base.GetCaptureClientChannel())
            {
                return client.Channel.GetAvailableUserToDelegation();
            }
        }

        /// <summary>
        /// lưu User
        /// </summary>
        /// <param name="userModel">UserModel</param>
        public void SaveUser(User userModel)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.SaveUser(userModel);
            }
        }


        /// <summary>
        /// xóa User
        /// </summary>
        /// <param name="user">UserModel </param>
        public void DeleteUser(User user)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteUser(user);
            }
        }

        internal object SaveUser()
        {
            throw new System.NotImplementedException();
        }
    }
}