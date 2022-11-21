using System.Security;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.Domain;
using Ecm.Utility;
using System.Configuration;
using Ecm.SecurityDao;
using System.Text;
using System;
using Ecm.SecurityDao.Domain;

namespace Ecm.Core
{
    public class SecurityManager : ManagerBase
    {
        public User LoginUser { get; private set; }

        public SecurityManager()
        {
        }

        public SecurityManager(User loginUser)
        {
            LoginUser = loginUser;
        }

        public User Login(string userName, string password, string clientHost)
        {
            using (Ecm.Context.DapperContext dataContext = new Ecm.Context.DapperContext())
            {
                User user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(userName));
                string passwordHash = CryptographyHelper.GenerateHash(userName, password);

                if (user == null || user.Password != passwordHash || string.IsNullOrWhiteSpace(user.ArchiveConnectionString) || string.IsNullOrEmpty(user.ArchiveConnectionString))
                {
                    return null;
                }

                user.ClientHost = clientHost;

                if (user.LanguageId != null)
                {
                    user.Language = GetLanguage(new LanguageDao(dataContext).GetById(user.LanguageId.Value));
                }

                return user;
            }
        }

        public User Authorize(string userName, string passwordHash)
        {
            using (Ecm.Context.DapperContext dataContext = new Ecm.Context.DapperContext())
            {
                User user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(userName));

                if (user == null || user.Password != passwordHash)
                {
                    throw new SecurityException(string.Format("{0} can't be authorized to use the system.", userName));
                }

                if (user.LanguageId != null)
                {
                    user.Language = GetLanguage(new LanguageDao(dataContext).GetById(user.LanguageId.Value));
                }

                return user;
            }
        }

        public User ChangePassword(string oldEncryptedPassword, string newEncryptedPassword)
        {
            if (LoginUser == null)
            {
                throw new SecurityException("You aren't authorized to change password.");
            }

            string password = CryptographyHelper.DecryptUsingSymmetricAlgorithm(oldEncryptedPassword);
            string oldPasswordHash = CryptographyHelper.GenerateHash(LoginUser.UserName, password);

            using (Ecm.Context.DapperContext dataContext = new Ecm.Context.DapperContext())
            {
                User user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(LoginUser.UserName));
                if (user == null || user.Password != oldPasswordHash)
                {
                    throw new SecurityException(string.Format("{0} can't be authorized to use the system.", LoginUser.UserName));
                }

                password = CryptographyHelper.DecryptUsingSymmetricAlgorithm(newEncryptedPassword);
                user.Password = CryptographyHelper.GenerateHash(LoginUser.UserName, password);

                dataContext.BeginTransaction();
                try
                {
                    new UserPrimaryDao(dataContext).ChangePassword(user.UserName, user.Password);
                    using (DapperContext userDataContext = new DapperContext(user.ArchiveConnectionString))
                    {
                        ActionLogHelper.AddActionLog(LoginUser.UserName + " change password successfully", LoginUser, ActionName.ChangePassword, null, null, userDataContext);
                    }
                    dataContext.Commit();
                }
                catch (System.Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
                LoginUser.Password = user.Password;

                return LoginUser;
            }
        }

        public void ResetPassword(string username)
        {
            using (Ecm.Context.DapperContext context = new Ecm.Context.DapperContext())
            {
                try
                {
                    context.BeginTransaction();
                    UserPrimaryDao userDao = new UserPrimaryDao(context);
                    PrimaryUser validUser = userDao.GetByUserName(username);

                    if (validUser == null)
                    {
                        throw new Exception("Username not found");
                    }

                    string newPassword = Utility.CryptographyHelper.RandomString(8);
                    string passwordHash = Utility.CryptographyHelper.GenerateHash(username, newPassword);

                    userDao.ChangePassword(username, passwordHash);

                    context.Commit();
                    SendMailNewPassword(username, newPassword, validUser.Email);
                }
                catch
                {
                    context.Rollback();
                    throw;
                }
            }
        }

        private void SendMailNewPassword(string username, string newPassword, string email)
        {
            string mailFrom = ConfigurationManager.AppSettings["NoReplyMail"].ToString();
            string body = string.Empty;
            string subject = "No-Reply : Reset password";
            string host = ConfigurationManager.AppSettings["SMTPHost"].ToString();
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["PortNumber"].ToString());
            string user = ConfigurationManager.AppSettings["Username"].ToString();
            string pass = ConfigurationManager.AppSettings["Password"].ToString();
            StringBuilder bodyBuilder = new StringBuilder();
            
            bodyBuilder.AppendLine("Hi " + username);
            bodyBuilder.AppendLine("Your new password is: " + newPassword);
            bodyBuilder.AppendLine("Please change your password after first login");

            body = bodyBuilder.ToString();

            UtilsMail.SendMail(subject, mailFrom, email, string.Empty, body, host, port, user, pass);
        }
    }
}