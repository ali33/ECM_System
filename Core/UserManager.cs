using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using Ecm.Domain;
using Ecm.Utility;
using Ecm.Utility.Exceptions;
using Ecm.SecurityDao;
using Ecm.SecurityDao.Domain;
using Ecm.Context;
using Ecm.DAO;
using System;

namespace Ecm.Core
{
    public class UserManager : ManagerBase
    {
        public UserManager(Domain.User loginUser) : base(loginUser)
        {
            if (!loginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to manage user.", LoginUser.UserName));
            }
        }

        public List<User> GetUsers()
        {
            using (DapperContext primaryDataContext = new DapperContext())
            {
                List<User> users = new List<User>();
                LanguageDao languageDao = new LanguageDao(primaryDataContext);
                UserGroupDao userGroupDao = new UserGroupDao(primaryDataContext);
                UserPrimaryDao userPrimaryDao = new UserPrimaryDao(primaryDataContext);
                List<PrimaryUser> primaryUsers = userPrimaryDao.GetUsers().Where(p => p.Id != LoginUser.Id && !string.IsNullOrEmpty(p.ArchiveConnectionString)).ToList();

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
                    ArchiveConnectionString = p.ArchiveConnectionString,
                    CaptureConnectionString = p.CaptureConnectionString,
                    ApplyForCapture = !string.IsNullOrEmpty(p.CaptureConnectionString),
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
                using (Ecm.DAO.Context.DapperContext context = new Ecm.DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user list successfully.", LoginUser, ActionName.GetUser, null, null, context);
                }

                return users;
            }
            
        }

        public void Delete(User user)
        {
            using (Ecm.DAO.Context.DapperContext dataContext = new Ecm.DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
            {
                dataContext.BeginTransaction();

                new SearchQueryExpressionDao(dataContext).DeleteByUser(user.Id);
                new SearchQueryDao(dataContext).DeleteByUser(user.Id);


                using (DapperContext primaryDataContext = new DapperContext())
                {
                    new MembershipDao(primaryDataContext).DeleteByUser(user.Id);
                    new UserPrimaryDao(primaryDataContext).Delete(user.Id);

                    primaryDataContext.BeginTransaction();
                    try
                    {
                        new UserPrimaryDao(primaryDataContext).Delete(user.Id);
                        primaryDataContext.Commit();
                        dataContext.Commit();
                    }
                    catch (System.Exception)
                    {
                        primaryDataContext.Rollback();
                        dataContext.Rollback();
                        throw;
                    }
                }
                ActionLogHelper.AddActionLog("Delete user " + user.UserName + " successfully", LoginUser,
                                            ActionName.DeleteUser, null, null, dataContext);          
            }
        }

        public Guid Save(User user)
        {
            if (user.Id == Guid.Empty)
            {
                CommonValidator.CheckNull(user.UserName, user.Email, user.Password);
            }
            else
            {
                CommonValidator.CheckNull(user.UserName, user.Email);
            }
            CommonValidator.CheckEmail(user.Email);

            using (DapperContext primaryDataContext = new DapperContext())
            {
                using (Ecm.DAO.Context.DapperContext context = new Ecm.DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
                {

                    try
                    {
                        UserPrimaryDao userDao = new UserPrimaryDao(primaryDataContext);
                        if (user.Id == Guid.Empty && userDao.GetByUserName(user.UserName) != null)
                        {
                            throw new DataException(string.Format("{0} is existed.", user.UserName));
                        }

                        bool isAdd = user.Id == Guid.Empty;
                        user.ArchiveConnectionString = LoginUser.ArchiveConnectionString;

                        if (user.ApplyForCapture)
                        {
                            user.CaptureConnectionString = LoginUser.CaptureConnectionString;
                        }

                        MembershipDao membershipDao = new MembershipDao(primaryDataContext);

                        primaryDataContext.BeginTransaction();
                        PrimaryUser primaryUser = GetPrimaryUser(user);

                        if (primaryUser.Id == Guid.Empty)
                        {
                            primaryUser.Password = CryptographyHelper.GenerateHash(user.UserName, user.Password);
                            userDao.Insert(primaryUser);

                            if (!primaryUser.IsAdmin)
                            {
                                foreach (var userGroup in user.UserGroups)
                                {
                                    membershipDao.Insert(new PrimaryMembership { UserGroupId = userGroup.Id, UserId = primaryUser.Id });
                                }
                            }
                            ActionLogHelper.AddActionLog("Add new user " + user.UserName + " successfully", LoginUser,
                                ActionName.AddUser, null, null, context);
                        }
                        else
                        {
                            membershipDao.DeleteByUser(primaryUser.Id);
                            if (!primaryUser.IsAdmin)
                            {
                                foreach (var userGroup in user.UserGroups)
                                {
                                    membershipDao.Insert(new PrimaryMembership { UserGroupId = userGroup.Id, UserId = user.Id });
                                }
                            }

                            userDao.Update(primaryUser);
                            ActionLogHelper.AddActionLog("Update user " + user.UserName, LoginUser,
                                ActionName.UpdateUser, null, null, context);
                        }

                        primaryDataContext.Commit();
                        return primaryUser.Id;
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
}