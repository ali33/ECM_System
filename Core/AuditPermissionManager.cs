using System;
using System.Linq;
using System.Collections.Generic;
using Ecm.Domain;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.SecurityDao;

namespace Ecm.Core
{
    public class AuditPermissionManager : ManagerBase
    {
        public AuditPermissionManager(User loginUser) : base (loginUser)
        {
        }

        public AuditPermission GetById(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new AuditPermissionDao(dataContext).GetById(id);
            }
        }

        public AuditPermission GetAuditPermission(Guid docTypeId, Guid userGroupId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new AuditPermissionDao(dataContext).GetAuditPermission(docTypeId, userGroupId);
            }
        }

        public List<AuditPermission> GetAll()
        {
            throw new NotImplementedException();
        }

        public List<AuditPermission> GetByUser(Guid userId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                List<Guid> groupIds = null;
                using (Context.DapperContext primaryContent = new Context.DapperContext())
                {
                    UserGroupDao userGroupDao = new UserGroupDao(primaryContent);
                    groupIds = userGroupDao.GetByUser(userId).Select(p => p.Id).ToList();
                }

                return new AuditPermissionDao(dataContext).GetByUser(groupIds);
            }
        }
    }
}