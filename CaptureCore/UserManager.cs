using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using Ecm.CaptureDomain;
using Ecm.Utility;
using Ecm.Utility.Exceptions;
using Ecm.Context;
using Ecm.SecurityDao;
using Ecm.SecurityDao.Domain;
using System;

namespace Ecm.CaptureCore
{
    public class UserManager : ManagerBase
    {
        public UserManager(CaptureDomain.User loginUser)
            : base(loginUser)
        {
        }

        public List<User> GetUsers()
        {
            using (DapperContext primaryDataContext = new DapperContext())
            {
                List<User> users = new List<User>();
                LanguageDao languageDao = new LanguageDao(primaryDataContext);
                UserGroupDao userGroupDao = new UserGroupDao(primaryDataContext);
                UserPrimaryDao userPrimaryDao = new UserPrimaryDao(primaryDataContext);
                List<PrimaryUser> primaryUsers = userPrimaryDao.GetUsers().Where(p => p.Id != LoginUser.Id && !string.IsNullOrEmpty(p.CaptureConnectionString)).ToList();

                foreach (var primaryUser in primaryUsers)
                {
                    if (primaryUser.LanguageId != null)
                    {
                        primaryUser.Language = languageDao.GetById(primaryUser.LanguageId.Value);
                    }

                    primaryUser.UserGroups = userGroupDao.GetByUser(primaryUser.Id);
                }

                primaryUsers.ForEach(p => users.Add(new User()
                {
                    CaptureConnectionString = p.CaptureConnectionString,
                    ArchiveConnectionString = p.ArchiveConnectionString,
                    ApplyForArchive = !string.IsNullOrEmpty(p.ArchiveConnectionString),
                    ClientHost = p.ClientHost,
                    Email = p.Email,
                    FullName = p.FullName,
                    Id = p.Id,
                    IsAdmin = p.IsAdmin,
                    Language = GetLanguage(p.Language),
                    LanguageId = p.LanguageId,
                    Password = p.Password,
                    Photo = p.Photo,
                    Type = p.Type,
                    UserGroups = GetUserGroups(p.UserGroups),
                    UserName = p.UserName
                }));
                using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user list successfully.", LoginUser, ActionName.GetUser, null, null, context);
                }

                return users;
            }

        }

        public User GetUser(string userName, string passwordHash)
        {
            using (DapperContext primaryDataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(primaryDataContext);
                UserGroupDao userGroupDao = new UserGroupDao(primaryDataContext);
                UserPrimaryDao userPrimaryDao = new UserPrimaryDao(primaryDataContext);
                PrimaryUser primaryUser = userPrimaryDao.GetByUserName(userName);

                if (primaryUser.Password != passwordHash)
                {
                    throw new Exception("Invaid username or password");
                }

                if (primaryUser.LanguageId != null)
                {
                    primaryUser.Language = languageDao.GetById(primaryUser.LanguageId.Value);
                }

                primaryUser.UserGroups = userGroupDao.GetByUser(primaryUser.Id);

                User user = new User
                {
                    CaptureConnectionString = primaryUser.CaptureConnectionString,
                    ArchiveConnectionString = primaryUser.ArchiveConnectionString,
                    ApplyForArchive = !string.IsNullOrEmpty(primaryUser.ArchiveConnectionString),
                    ClientHost = primaryUser.ClientHost,
                    Email = primaryUser.Email,
                    FullName = primaryUser.FullName,
                    Id = primaryUser.Id,
                    IsAdmin = primaryUser.IsAdmin,
                    Language = GetLanguage(primaryUser.Language),
                    LanguageId = primaryUser.LanguageId,
                    Password = primaryUser.Password,
                    Photo = primaryUser.Photo,
                    Type = primaryUser.Type,
                    UserGroups = GetUserGroups(primaryUser.UserGroups),
                    UserName = primaryUser.UserName
                };

                using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user successfully.", LoginUser, ActionName.GetUser, null, null, context);
                }

                return user;
            }

        }

        public User GetUser(string email)
        {
            using (DapperContext primaryDataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(primaryDataContext);
                UserGroupDao userGroupDao = new UserGroupDao(primaryDataContext);
                UserPrimaryDao userPrimaryDao = new UserPrimaryDao(primaryDataContext);
                PrimaryUser primaryUser = userPrimaryDao.GetByEmail(email);

                if (primaryUser.LanguageId != null)
                {
                    primaryUser.Language = languageDao.GetById(primaryUser.LanguageId.Value);
                }

                primaryUser.UserGroups = userGroupDao.GetByUser(primaryUser.Id);

                User user = new User
                {                    
                    CaptureConnectionString = primaryUser.CaptureConnectionString,
                    ArchiveConnectionString = primaryUser.ArchiveConnectionString,
                    ApplyForArchive = !string.IsNullOrEmpty(primaryUser.ArchiveConnectionString),
                    ClientHost = primaryUser.ClientHost,
                    Email = primaryUser.Email,
                    FullName = primaryUser.FullName,
                    Id = primaryUser.Id,
                    IsAdmin = primaryUser.IsAdmin,
                    Language = GetLanguage(primaryUser.Language),
                    LanguageId = primaryUser.LanguageId,
                    Password = primaryUser.Password,
                    Photo = primaryUser.Photo,
                    Type = primaryUser.Type,
                    UserGroups = GetUserGroups(primaryUser.UserGroups),
                    UserName = primaryUser.UserName
                };

                using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user successfully.", LoginUser, ActionName.GetUser, null, null, context);
                }

                return user;
            }

        }

        public User GetUserByUserName(string username)
        {
            using (DapperContext primaryDataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(primaryDataContext);
                UserGroupDao userGroupDao = new UserGroupDao(primaryDataContext);
                UserPrimaryDao userPrimaryDao = new UserPrimaryDao(primaryDataContext);
                PrimaryUser primaryUser = userPrimaryDao.GetByUserName(username);

                if (primaryUser.LanguageId != null)
                {
                    primaryUser.Language = languageDao.GetById(primaryUser.LanguageId.Value);
                }

                primaryUser.UserGroups = userGroupDao.GetByUser(primaryUser.Id);

                User user = new User
                {                    
                    CaptureConnectionString = primaryUser.CaptureConnectionString,
                    ArchiveConnectionString = primaryUser.ArchiveConnectionString,
                    ApplyForArchive = !string.IsNullOrEmpty(primaryUser.ArchiveConnectionString),
                    ClientHost = primaryUser.ClientHost,
                    Email = primaryUser.Email,
                    FullName = primaryUser.FullName,
                    Id = primaryUser.Id,
                    IsAdmin = primaryUser.IsAdmin,
                    Language = GetLanguage(primaryUser.Language),
                    LanguageId = primaryUser.LanguageId,
                    Password = primaryUser.Password,
                    Photo = primaryUser.Photo,
                    Type = primaryUser.Type,
                    UserGroups = GetUserGroups(primaryUser.UserGroups),
                    UserName = primaryUser.UserName
                };

                using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user successfully.", LoginUser, ActionName.GetUser, null, null, context);
                }

                return user;
            }

        }

        public List<User> GetAvailableUserToDelegation()
        {
            using (DapperContext primaryDataContext = new DapperContext())
            {
                List<User> users = new List<User>();
                LanguageDao languageDao = new LanguageDao(primaryDataContext);
                UserGroupDao userGroupDao = new UserGroupDao(primaryDataContext);
                UserPrimaryDao userPrimaryDao = new UserPrimaryDao(primaryDataContext);
                List<PrimaryUser> primaryUsers = userPrimaryDao.GetUsers().Where(p => p.Id != LoginUser.Id && !string.IsNullOrEmpty(p.CaptureConnectionString)).ToList();

                foreach (var primaryUser in primaryUsers)
                {
                    if (primaryUser.LanguageId != null)
                    {
                        primaryUser.Language = languageDao.GetById(primaryUser.LanguageId.Value);
                    }

                    primaryUser.UserGroups = userGroupDao.GetByUser(primaryUser.Id);
                }

                primaryUsers.Where(p => !p.UserName.Equals(LoginUser.UserName)).ToList().ForEach(p => users.Add(new User()
                {
                    ArchiveConnectionString = p.ArchiveConnectionString,
                    ApplyForArchive = !string.IsNullOrEmpty(p.ArchiveConnectionString),
                    CaptureConnectionString = p.CaptureConnectionString,
                    ClientHost = p.ClientHost,
                    Email = p.Email,
                    FullName = p.FullName,
                    Id = p.Id,
                    IsAdmin = p.IsAdmin,
                    Language = GetLanguage(p.Language),
                    LanguageId = p.LanguageId,
                    Password = p.Password,
                    Photo = p.Photo,
                    Type = p.Type,
                    UserGroups = GetUserGroups(p.UserGroups),
                    UserName = p.UserName
                }));
                using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user list successfully.", LoginUser, ActionName.GetUser, null, null, context);
                }

                return users;
            }
        }

        public void Delete(User user)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to manage user.", LoginUser.UserName));
            }
            using (DapperContext primaryDataContext = new DapperContext())
                {
                    new MembershipDao(primaryDataContext).DeleteByUser(user.Id);
                    new UserPrimaryDao(primaryDataContext).Delete(user.Id);

                    primaryDataContext.BeginTransaction();
                    try
                    {
                        new UserPrimaryDao(primaryDataContext).Delete(user.Id);
                        primaryDataContext.Commit();
                    }
                    catch (System.Exception)
                    {
                        primaryDataContext.Rollback();
                        throw;
                    }
                }
                using (CaptureDAO.Context.DapperContext dataContext = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                {
                    ActionLogHelper.AddActionLog("Delete user " + user.UserName + " successfully", LoginUser,
                                            ActionName.DeleteUser, null, null, dataContext);
                }
        }

        public void Save(User user)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to manage user.", LoginUser.UserName));
            }

            CommonValidator.CheckNull(user.UserName, user.Email, user.Password);
            CommonValidator.CheckEmail(user.Email);

            using (DapperContext primaryDataContext = new DapperContext())
            {
                try
                {
                    UserPrimaryDao userDao = new UserPrimaryDao(primaryDataContext);
                    if (user.Id == Guid.Empty && userDao.GetByUserName(user.UserName) != null)
                    {
                        throw new DataException(string.Format("{0} is existed.", user.UserName));
                    }

                    bool isAdd = user.Id == Guid.Empty;
                    
                    user.CaptureConnectionString = LoginUser.CaptureConnectionString;

                    if (user.ApplyForArchive)
                    {
                        user.ArchiveConnectionString = LoginUser.ArchiveConnectionString;
                    }

                    MembershipDao membershipDao = new MembershipDao(primaryDataContext);

                    primaryDataContext.BeginTransaction();
                    PrimaryUser primaryUser = GetPrimaryUser(user);

                    if (primaryUser.Id == Guid.Empty)
                    {
                        primaryUser.Password = CryptographyHelper.GenerateHash(primaryUser.UserName, primaryUser.Password);
                        userDao.Insert(primaryUser);

                        if (!primaryUser.IsAdmin)
                        {
                            foreach (var userGroup in user.UserGroups)
                            {
                                membershipDao.Insert(new PrimaryMembership { UserGroupId = userGroup.Id, UserId = primaryUser.Id });
                            }
                        }

                        using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                        {
                            ActionLogHelper.AddActionLog("Add new user " + user.UserName + " successfully", LoginUser,
                                ActionName.AddUser, null, null, context);
                        }
                    }
                    else
                    {
                        membershipDao.DeleteByUser(primaryUser.Id);
                        if (!primaryUser.IsAdmin)
                        {
                            foreach (var userGroup in user.UserGroups)
                            {
                                membershipDao.Insert(new PrimaryMembership { UserGroupId = userGroup.Id, UserId = primaryUser.Id });
                            }
                        }

                        userDao.Update(primaryUser);
                        using (CaptureDAO.Context.DapperContext context = new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString))
                        {
                            ActionLogHelper.AddActionLog("Update user " + primaryUser.UserName, LoginUser,
                                ActionName.UpdateUser, null, null, context);
                        }
                    }

                    primaryDataContext.Commit();
                }
                catch (System.Exception)
                {
                    primaryDataContext.Rollback();
                    throw;
                }
            }
        }

    }
}