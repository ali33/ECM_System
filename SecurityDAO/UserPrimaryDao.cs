using System.Linq;
using Dapper;
using Ecm.Context;
using Ecm.SecurityDao.Domain;
using System.Collections.Generic;
using System;

namespace Ecm.SecurityDao
{
    public class UserPrimaryDao
    {
        private readonly DapperContext _context;

        public UserPrimaryDao(DapperContext context)
        {
            _context = context;
        }

        public void Insert(PrimaryUser user)
        {
            const string query = @"INSERT INTO [UserPrimary]([UserName],[Password],[Email],[FullName],[ArchiveConnectionString],[CaptureConnectionString],[IsAdmin],[Photo],[Type],[LanguageId])
                                OUTPUT inserted.ID
                                   VALUES (@UserName,@Password,@Email,@FullName,@ArchiveConnectionString,@CaptureConnectionString,@IsAdmin,@Photo,@Type,@LanguageId)
                                   ";
            user.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             user.UserName,
                                                             user.Password,
                                                             user.Email,
                                                             user.FullName,
                                                             user.ArchiveConnectionString,
                                                             user.CaptureConnectionString,
                                                             user.IsAdmin,
                                                             user.Photo,
                                                             user.Type,
                                                             user.LanguageId
                                                         },
                                                     _context.CurrentTransaction).SingleOrDefault();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [UserPrimary] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public List<PrimaryUser> GetUsers()
        {
            const string query = @"SELECT * FROM [UserPrimary]";
            return _context.Connection.Query<PrimaryUser>(query, null, _context.CurrentTransaction).ToList();
        }

        public List<PrimaryUser> GetByUserGroup(Guid userGroupId)
        {
            const string query = @"SELECT DISTINCT u.* 
                                   FROM [UserPrimary] u
                                   LEFT JOIN [Membership] m ON u.Id = m.UserId
                                   WHERE m.UserGroupId = @UserGroupId";
            return _context.Connection.Query<PrimaryUser>(query, new { UserGroupId = userGroupId }, _context.CurrentTransaction).ToList();
        }

        public PrimaryUser GetByUserName(string userName)
        {
            const string query = @"SELECT * 
                                   FROM [UserPrimary] 
                                   WHERE [UserName] = @UserName";
            return _context.Connection.Query<PrimaryUser>(query, new { UserName = userName }, _context.CurrentTransaction).FirstOrDefault();
        }

        public PrimaryUser GetByEmail(string email)
        {
            const string query = @"SELECT * 
                                   FROM [UserPrimary] 
                                   WHERE [Email] = @Email";
            return _context.Connection.Query<PrimaryUser>(query, new { Email = email }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void ChangePassword(string userName, string password)
        {
            const string query = @"UPDATE [UserPrimary]
                                   SET Password = @Password
                                   WHERE UserName = @UserName";
            _context.Connection.Execute(query, new { UserName = userName, Password = password }, _context.CurrentTransaction);
        }

        public void Update(PrimaryUser user)
        {
            const string query = @"UPDATE [UserPrimary]
                                   SET [UserName] = @UserName,
                                       [Email] = @Email,
                                       [FullName] = @FullName,
                                       [IsAdmin] = @IsAdmin,
                                       [LanguageId] = @LanguageId,
                                       [Photo] = @Photo,
                                       [ArchiveConnectionString] = @ArchiveConnectionString,
                                       [CaptureConnectionString] = @CaptureConnectionString
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            user.UserName,
                                            user.Password,
                                            user.Email,
                                            user.FullName,
                                            user.IsAdmin,
                                            user.LanguageId,
                                            user.Photo,
                                            user.CaptureConnectionString,
                                            user.ArchiveConnectionString,
                                            user.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void ChangePhoto(PrimaryUser obj)
        {
            const string query = @"UPDATE [UserPrimary]
                                   SET [Photo] = @Photo
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            obj.Photo,
                                            obj.Id
                                        },
                                        _context.CurrentTransaction);
        }

    }
}