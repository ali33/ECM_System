using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Dapper;

namespace Ecm.CaptureDAO
{
    public class DocumentTypePermissionDao
    {
        private readonly DapperContext _context;
        
        public DocumentTypePermissionDao(DapperContext context)
        {
            _context = context;
        }

        public DocumentTypePermission GetById(Guid docTypeId, Guid groupId)
        {
            const string query = @"SELECT * FROM [DocumentTypePermission] WHERE [DocTypeId] = @DocTypeID AND [UserGroupID] = @GroupID";
            return _context.Connection.Query<DocumentTypePermission>(query, new { DocTypeID = docTypeId, GroupID = groupId }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<DocumentTypePermission> GetByGroupRangeAndDocType(List<Guid> groupIds, Guid docTypeId)
        {
            const string query = @"SELECT * FROM [DocumentTypePermission] WHERE [DocTypeId] = @DocTypeID AND [UserGroupID] IN @GroupIDs";
            return _context.Connection.Query<DocumentTypePermission>(query, new { DocTypeID = docTypeId, GroupIDs = groupIds }, _context.CurrentTransaction).ToList();

        }

        public void Insert(DocumentTypePermission permission)
        {
            const string query = @"INSERT INTO [DocumentTypePermission]([UserGroupID], [DocTypeID],[CanAccess])
                                   VALUES(@UserGroupID, @DocTypeID, @CanAccess)";

            _context.Connection.Execute(query, new { UserGroupID = permission.UserGroupId, DocTypeID = permission.DocTypeId, CanAccess = permission.CanAccess}, _context.CurrentTransaction);
        }

        public void Update(DocumentTypePermission permission)
        {
            const string query = @"UPDATE [DocumentTypePermission] SET
                                   [CanAccess] = @CanAccess
                                   WHERE [UserGroupID] = @UserGroupID AND [DocTypeID] = @DocTypeID";
            _context.Connection.Execute(query, new { UserGroupID = permission.UserGroupId, DocTypeID = permission.DocTypeId, CanAccess = permission.CanAccess}, _context.CurrentTransaction);
        }

        public void Delete(Guid docTypeId, Guid groupId)
        {
            const string query = @"DELETE FROM [DocumentTypePermission] WHERE [UserGroupID] = @UserGroupID AND [DocTypeID] = @DocTypeID";
            _context.Connection.Execute(query, new { UserGroupID = groupId, DocTypeID = docTypeId}, _context.CurrentTransaction);
        }
    }
}
