using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class AuditPermissionDao
    {
        private readonly DapperContext _context;

        public AuditPermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(AuditPermission obj)
        {
            const string query = @"INSERT INTO [AuditPermission]
                                        ([DocTypeID],[UserGroupID],[AllowedAudit],[AllowedViewLog],[AllowedDeleteLog],[AllowedViewReport],[AllowedRestoreDocument])
                            OUTPUT inserted.ID
                                   VALUES(@DocTypeID,@UserGroupID,@AllowedAudit,@AllowedViewLog,@AllowedDeleteLog,@AllowedViewReport,@AllowedRestoreDocument)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocTypeID = obj.DocTypeId,
                                                             UserGroupID = obj.UserGroupId,
                                                             AllowedAudit = obj.AllowedAudit,
                                                             AllowedViewLog = obj.AllowedViewLog,
                                                             AllowedDeleteLog = obj.AllowedDeleteLog,
                                                             AllowedViewReport = obj.AllowedViewReport,
                                                             AllowedRestoreDocument = obj.AllowedRestoreDocument
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Update(AuditPermission obj)
        {
            const string query = @"UPDATE [AuditPermission]
                                   SET [AllowedAudit] = @AllowedAudit,
                                       [AllowedViewLog] = @AllowedViewLog,
                                       [AllowedDeleteLog] = @AllowedDeleteLog,
                                       [AllowedViewReport] = @AllowedViewReport,
                                       [AllowedRestoreDocument] = @AllowedRestoreDocument
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                AllowedAudit = obj.AllowedAudit,
                                                AllowedViewLog = obj.AllowedViewLog,
                                                AllowedDeleteLog = obj.AllowedDeleteLog,
                                                AllowedViewReport = obj.AllowedViewReport,
                                                AllowedRestoreDocument = obj.AllowedRestoreDocument,
                                                ID  = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [AuditPermission] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid userGroupId)
        {
            const string query = @"DELETE FROM [AuditPermission] 
                                   WHERE UserGroupID = @UserGroupID";
            _context.Connection.Execute(query, new { UserGroupID = userGroupId }, _context.CurrentTransaction);
        }

        public List<AuditPermission> GetByUser(List<Guid> userGroupIds)
        {
            const string query = @"SELECT * FROM AuditPermission
                                   WHERE UserGroupID IN @UserGroupIDs";
            return _context.Connection.Query<AuditPermission>(query, new { UserGroupIDs = userGroupIds }, _context.CurrentTransaction).ToList();
        }

//        public List<AuditPermission> GetByUser(List<long> userGroupIds)
//        {
//            const string query = @"SELECT * FROM AuditPermission a 
//                                   WHERE m.UserGroupId in @UserGroupIds";
//            return _context.Connection.Query<AuditPermission>(query, new { UserGroupIds = userGroupIds }, _context.CurrentTransaction).ToList();
//        }

        public AuditPermission GetAuditPermission(Guid docTypeId, Guid groupId)
        {
            const string query = @"SELECT * FROM [AuditPermission] WHERE [DocTypeID] = @DocTypeID AND [UserGroupID] = @UserGroupID";
            return _context.Connection.Query<AuditPermission>(query, new { DocTypeID = docTypeId, UserGroupID = groupId }, _context.CurrentTransaction).SingleOrDefault();
        }

        public AuditPermission GetById(Guid Id)
        {
            const string query = @"SELECT * FROM [AuditPermission] WHERE [ID] = @ID";
            return _context.Connection.Query<AuditPermission>(query, new { ID = Id }, _context.CurrentTransaction).SingleOrDefault();
        }
    }
}