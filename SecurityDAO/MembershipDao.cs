using System.Collections.Generic;
using System.Linq;
using Ecm.Context;
using Dapper;
using Ecm.SecurityDao.Domain;
using System;

namespace Ecm.SecurityDao
{
    public class MembershipDao
    {
        private readonly DapperContext _context;

        public MembershipDao(DapperContext context)
        {
            _context = context;
        }

        public void Insert(PrimaryMembership membership)
        {
            const string query = @"INSERT INTO [Membership] ([UserGroupId],[UserId])
                                OUTPUT inserted.ID
                                   VALUES (@UserGroupId,@UserId)                                   ";
            membership.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             membership.UserGroupId,
                                                             membership.UserId
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void DeleteByUser(Guid userId)
        {
            const string query = @"DELETE FROM [Membership] 
                                   WHERE UserId = @UserId";
            _context.Connection.Execute(query, new { UserId = userId }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid userGroupId)
        {
            const string query = @"DELETE FROM [Membership] 
                                   WHERE UserGroupId = @UserGroupId";
            _context.Connection.Execute(query, new { UserGroupId = userGroupId }, _context.CurrentTransaction);
        }

        public List<PrimaryMembership> GetByUser(Guid userId)
        {
            const string query = @"SELECT * 
                                   FROM [Membership] 
                                   WHERE UserId = @UserId";
            return _context.Connection.Query<PrimaryMembership>(query, new { UserId = userId }, _context.CurrentTransaction).ToList();
        }

        public List<PrimaryMembership> GetByUserGroup(Guid groupId)
        {
            const string query = @"SELECT * 
                                   FROM [Membership] 
                                   WHERE UserGroupId = @UserGroupId";
            return _context.Connection.Query<PrimaryMembership>(query, new { UserGroupId = groupId }, _context.CurrentTransaction).ToList();
        }
    }
}