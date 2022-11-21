using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.Context;
using Ecm.SecurityDao.Domain;
using System;

namespace Ecm.SecurityDao
{
    public class UserGroupDao
    {
        private readonly DapperContext _context;

        public UserGroupDao(DapperContext context)
        {
            _context = context;
        }

        public void Insert(PrimaryUserGroup usergroup)
        {
            const string query = @"INSERT INTO [UserGroup] ([Name],[Type]) 
                                OUTPUT inserted.ID
                                   VALUES (@Name ,@Type)
                                ";
            usergroup.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             usergroup.Name,
                                                             usergroup.Type
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [UserGroup] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public List<PrimaryUserGroup> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [UserGroup]";
            return _context.Connection.Query<PrimaryUserGroup>(query, null, _context.CurrentTransaction).ToList();
        }

        public PrimaryUserGroup GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [UserGroup] 
                                   WHERE Id = @Id";
            return _context.Connection.Query<PrimaryUserGroup>(query, new { Id = id }, _context.CurrentTransaction).SingleOrDefault();
        }

        public void Update(PrimaryUserGroup usergroup)
        {
            const string query = @"UPDATE [UserGroup] 
                                   SET [Name] = @Name, 
                                       [Type] = @Type 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                usergroup.Name,
                                                usergroup.Type,
                                                Id = usergroup.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public List<PrimaryUserGroup> GetByUser(Guid userId)
        {
            const string query = @"SELECT DISTINCT g.* 
                                   FROM [UserGroup] g 
                                   LEFT JOIN [Membership] m ON g.Id = m.UserGroupId 
                                   WHERE m.UserId = @UserId";
            return _context.Connection.Query<PrimaryUserGroup>(query, new { UserId = userId }, _context.CurrentTransaction).ToList();
        }
    }
}