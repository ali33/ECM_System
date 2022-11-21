using System.Configuration;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDomain;
using Ecm.Utility;
using Ecm.Context;
using Ecm.SecurityDao;
using System;
using System.Text;
using Ecm.SecurityDao.Domain;
using com.pakhee.common;

namespace Ecm.CaptureCore
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
            string passwordHash = CryptographyHelper.GenerateHash(userName, password);
            User user;

            using (DapperContext dataContext = new DapperContext())
            {
                PrimaryUser priUser = new UserPrimaryDao(dataContext).GetByUserName(userName);

                if (priUser != null && priUser.LanguageId != null)
                {
                    priUser.Language = new LanguageDao(dataContext).GetById(priUser.LanguageId.Value);
                }

                user = GetUser(priUser);

                if (user == null || user.Password != passwordHash || string.IsNullOrEmpty(user.CaptureConnectionString) || string.IsNullOrWhiteSpace(user.CaptureConnectionString))
                {
                    return null;
                }
            }

            using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(user.CaptureConnectionString))
            {   
                user.ClientHost = clientHost;
                ActionLogHelper.AddActionLog(user.UserName + " login successfully from host " + clientHost, user, ActionName.GetUser, null, null, context);
            }

            return user;
        }

        public User Authorize(string userName, string encryptedPassword)
        {
            User user;
            string decryptedPassword = CryptographyHelper.DecryptUsingSymmetricAlgorithm(encryptedPassword);
            string passwordHash = CryptographyHelper.GenerateHash(userName, decryptedPassword);            
            using (DapperContext dataContext = new DapperContext())
            {
                user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(userName));
                user.EncryptedPassword = encryptedPassword;
            }

            if (user == null || (user != null && user.Password != passwordHash))
            {
                throw new SecurityException(string.Format("{0} can't be authorized to use the system.", userName));
            }

            return user;
        }

        public User AuthorizeMobile(string userName, string encryptedPassword)
        {
            User user;
            CryptLib _crypt = new CryptLib();
            String iv = "Lo0bwyhlaB39PB8=";//CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
            string key = CryptLib.getHashSha256("D4A88355-7148-4FF2-A626-151A40F57330", 32); //32 bytes = 256 bits
            string decryptedPassword = _crypt.decrypt(encryptedPassword, key, iv);

            string passwordHash = CryptographyHelper.GenerateHash(userName, decryptedPassword);
            using (DapperContext dataContext = new DapperContext())
            {
                user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(userName));
                user.EncryptedPassword = encryptedPassword;
            }

            if (user == null || (user != null && user.Password != passwordHash))
            {
                throw new SecurityException(string.Format("{0} can't be authorized to use the system.", userName));
            }

            return user;
        }

        public User ChangePasswordMobile(string oldEncryptedPassword, string newEncryptedPassword)
        {
            if (LoginUser == null)
            {
                throw new SecurityException("You aren't authorized to change password.");
            }

            CryptLib _crypt = new CryptLib();
            String iv = "Lo0bwyhlaB39PB8=";//CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
            string key = CryptLib.getHashSha256("D4A88355-7148-4FF2-A626-151A40F57330", 32); //32 bytes = 256 bits

            string password = _crypt.decrypt(oldEncryptedPassword, key, iv);
            string oldPasswordHash = CryptographyHelper.GenerateHash(LoginUser.UserName, password);

            using (DapperContext dataContext = new DapperContext())
            {
                User user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(LoginUser.UserName));
                if (user == null || user.Password != oldPasswordHash)
                {
                    throw new SecurityException(string.Format("{0} can't be authorized to use the system.", LoginUser.UserName));
                }

                password = _crypt.decrypt(newEncryptedPassword, key, iv);
                user.Password = CryptographyHelper.GenerateHash(LoginUser.UserName, password);

                dataContext.BeginTransaction();
                try
                {
                    new UserPrimaryDao(dataContext).ChangePassword(user.UserName, user.Password);
                    using (CaptureDAO.Context.DapperContext userDataContext = new CaptureDAO.Context.DapperContext(user.CaptureConnectionString))
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

        public User AuthorizeUser(string userName, string encryptedPassword)
        {
            User user;
            string decryptedPassword = CryptographyHelper.DecryptUsingSymmetricAlgorithm(encryptedPassword);
            
            using (DapperContext dataContext = new DapperContext())
            {
                user = GetUser(new UserPrimaryDao(dataContext).GetByUserName(userName));
            }

            if (user == null || (user != null && user.Password != decryptedPassword))
            {
                throw new SecurityException(string.Format("{0} can't be authorized to use the system.", userName));
            }

            return user;
        }

        public User ChangePassword(string oldEncryptedPassword, string newEncryptedPassword)
        {
            if (LoginUser == null)
            {
                throw new SecurityException("You aren't authorized to change password.");
            }

            string password = CryptographyHelper.DecryptUsingSymmetricAlgorithm(oldEncryptedPassword);
            string oldPasswordHash = CryptographyHelper.GenerateHash(LoginUser.UserName, password);

            using (DapperContext dataContext = new DapperContext())
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
                    using (CaptureDAO.Context.DapperContext userDataContext = new CaptureDAO.Context.DapperContext(user.CaptureConnectionString))
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
            bodyBuilder.AppendLine("Hi " + username + "<br>");
            bodyBuilder.AppendLine("Your new password is: " + newPassword + "<br>");
            bodyBuilder.AppendLine("Please change your password after first login" + "<br>");
            bodyBuilder.AppendLine("Best Regards,<br>");
            bodyBuilder.AppendLine("Administrator");


            body = bodyBuilder.ToString();

            UtilsMail.SendMail(subject, mailFrom, email, string.Empty, body, host, port, user, pass);
        }

    }
}
