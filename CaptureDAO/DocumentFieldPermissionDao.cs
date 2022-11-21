using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Dapper;

namespace Ecm.CaptureDAO
{
    public class DocumentFieldPermissionDao
    {
        private readonly DapperContext _context;

        public DocumentFieldPermissionDao(DapperContext context)
        {
            _context = context;
        }


//        public void Add(DocumentFieldPermission permission)
//        {
//            const string query = @"INSERT INTO [DocumentFieldPermission] ([FieldId],[UserGroupId],[DocTypeId],[CanRead],[CanWrite],[Hidden])
//                                   VALUES (@FieldId,@UserGroupId,@DocTypeId,@CanRead,@CanWrite,@Hidden)";
//            _context.Connection.Execute(query,
//                                        new
//                                        {
//                                            FieldId = permission.FieldId,
//                                            UserGroupId = permission.UserGroupId,
//                                            DocTypeId = permission.DocTypeId,
//                                            CanRead = permission.CanRead,
//                                            CanWrite = permission.CanWrite,
//                                            Hidden = permission.Hidden
//                                        },
//                                        _context.CurrentTransaction);
//        }

//        public void Update(DocumentFieldPermission permission)
//        {
//            const string query = @"UPDATE [DocumentFieldPermission] SET
//                                   [CanRead] = @CanRead, [CanWrite] = @CanWrite, [Hidden] = @Hidden
//                                   WHERE [ID] = @Id";
//            _context.Connection.Execute(query,
//                                        new
//                                        {
//                                            CanRead = permission.CanRead,
//                                            CanWrite = permission.CanWrite,
//                                            Hidden = permission.Hidden,
//                                            Id = permission.Id
//                                        },
//                                        _context.CurrentTransaction);
//        }

        public void Delete(Guid docTypeId)
        {
            const string query = @"DELETE FROM [DocumentFieldPermission]
                                   WHERE [DocTypeID] = @DocTypeId";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            DocTypeId = docTypeId
                                        },
                                        _context.CurrentTransaction);
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [DocumentFieldPermission]
                                   WHERE [FieldID] = @FieldId";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            FieldId = fieldId
                                        },
                                        _context.CurrentTransaction);
        }

        public DocumentFieldPermission GetFieldPermission(Guid id)
        {
            const string query = @"SELECT * FROM [DocumentFieldPermission] WHERE [Id] = @Id";
            return _context.Connection.Query<DocumentFieldPermission>(query, new { Id = id },_context.CurrentTransaction).SingleOrDefault();
        }

        public List<DocumentFieldPermission> GetFieldPermissions(Guid docTypeId, Guid userGroupId)
        {
            const string query = @"SELECT d.* FROM [DocumentFieldPermission] d 
                                   LEFT JOIN [DocumentFieldMetaData] f ON d.FieldId = f.Id
                                   WHERE [UserGroupId] = @UserGroupId AND f.DocTypeId = @DocTypeId";
            return _context.Connection.Query<DocumentFieldPermission>(query, new { DocTypeId = docTypeId, UserGroupId = userGroupId }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentFieldPermission> GetByUserAndDocType(List<Guid> groupIds, Guid docTypeId)
        {
            const string query = @"SELECT d.* FROM [DocumentFieldPermission] d 
                                   LEFT JOIN [DocumentFieldMetaData] f ON d.FieldId = f.Id
                                   WHERE [UserGroupId] IN @GroupIds AND f.DocTypeId = @DocTypeId";
            return _context.Connection.Query<DocumentFieldPermission>(query, new { DocTypeId = docTypeId, GroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentFieldPermission> GetByUser(List<Guid> groupIds, Guid fieldId)
        {
            const string query = @"SELECT * FROM [DocumentFieldPermission] 
                                   WHERE [UserGroupId] IN @GroupIds AND FieldId = @FieldId";
            return _context.Connection.Query<DocumentFieldPermission>(query, new { FieldId = fieldId, GroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }
    }
}
