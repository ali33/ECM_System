using System.Collections.Generic;
using System.Security;
using Ecm.DAO;
using Ecm.Domain;
using Ecm.Utility;
using Ecm.Context;
using Ecm.SecurityDao;
using Ecm.SecurityDao.Domain;
using System;

namespace Ecm.Core
{
    public class UserGroupManager : ManagerBase
    {
        public UserGroupManager(User loginUser)
            : base(loginUser)
        {
            if (!loginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to manage usergroup.", LoginUser.UserName));
            }
        }

        public List<UserGroup> GetAll()
        {
            using (DapperContext dataContext = new DapperContext())
            {
                List<UserGroup> userGroups = GetUserGroups(new UserGroupDao(dataContext).GetAll());
                UserPrimaryDao userDao = new UserPrimaryDao(dataContext);
                foreach (var userGroup in userGroups)
                {
                    userGroup.Users = GetUsers(userDao.GetByUserGroup(userGroup.Id));
                }
                using (Ecm.DAO.Context.DapperContext context = new Ecm.DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
                {
                    ActionLogHelper.AddActionLog("Get user group list successfully", LoginUser, ActionName.GetUserGroup, null, null, context);
                }

                return userGroups;
            }
        }

        public Guid Save(UserGroup userGroup)
        {
            CommonValidator.CheckNull(userGroup.Name);

            using (DapperContext dataContext = new DapperContext())
            {
                using (Ecm.DAO.Context.DapperContext context = new Ecm.DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
                {

                    UserGroupDao userGroupDao = new UserGroupDao(dataContext);
                    MembershipDao membershipDao = new MembershipDao(dataContext);
                    dataContext.BeginTransaction();
                    try
                    {
                        if (userGroup.Id == Guid.Empty)
                        {
                            PrimaryUserGroup priUserGroup = GetPrimaryUserGroup(userGroup);
                            userGroupDao.Insert(priUserGroup);
                            userGroup.Id = priUserGroup.Id;
                            foreach (var user in userGroup.Users)
                            {
                                membershipDao.Insert(new PrimaryMembership
                                                      {
                                                          UserId = user.Id,
                                                          UserGroupId = userGroup.Id
                                                      });
                            }

                            ActionLogHelper.AddActionLog("Add user group" + userGroup.Name, LoginUser, ActionName.AddUserGroup, null, null, context);
                        }
                        else
                        {
                            membershipDao.DeleteByUserGroup(userGroup.Id);
                            foreach (var user in userGroup.Users)
                            {
                                membershipDao.Insert(new PrimaryMembership
                                                      {
                                                          UserId = user.Id,
                                                          UserGroupId = userGroup.Id
                                                      });
                            }

                            userGroupDao.Update(GetPrimaryUserGroup(userGroup));
                            ActionLogHelper.AddActionLog("Update user group" + userGroup.Name, LoginUser, ActionName.UpdateUserGroup, null, null, context);
                        }

                        dataContext.Commit();
                        return userGroup.Id;
                    }
                    catch (System.Exception)
                    {
                        dataContext.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Delete(UserGroup userGroup)
        {
            using (DAO.Context.DapperContext dataContext = new DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
            {
                using (DapperContext primaryContext = new DapperContext())
                {
                    dataContext.BeginTransaction();
                    primaryContext.BeginTransaction();
                    try
                    {
                        new AuditPermissionDao(dataContext).DeleteByUserGroup(userGroup.Id);
                        new AnnotationPermissionDao(dataContext).DeleteByUserGroup(userGroup.Id);
                        new DocumentTypePermissionDao(dataContext).DeleteByUserGroup(userGroup.Id);
                        new MembershipDao(primaryContext).DeleteByUserGroup(userGroup.Id);
                        new UserGroupDao(primaryContext).Delete(userGroup.Id);
                        ActionLogHelper.AddActionLog("Delete user group" + userGroup.Name + " successfully.", LoginUser, ActionName.DeleteUserGroup, null, null, dataContext);
                        dataContext.Commit();
                        primaryContext.Commit();
                    }
                    catch
                    {
                        dataContext.Rollback();
                        primaryContext.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}