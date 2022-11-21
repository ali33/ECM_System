using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class AnnotationPermissionDao
    {
        private readonly DapperContext _context;

        public AnnotationPermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(AnnotationPermission obj)
        {
            const string query = @"INSERT INTO [AnnotationPermission]
                                        ([DocTypeID],[UserGroupId],[AllowedSeeText],[AllowedAddText],[AllowedDeleteText],[AllowedSeeHighlight],
                                         [AllowedAddHighlight],[AllowedDeleteHighlight],[AllowedHideRedaction],[AllowedAddRedaction],[AllowedDeleteRedaction])
                                OUTPUT inserted.ID
                                   VALUES
                                        (@DocTypeID,@UserGroupId,@AllowedSeeText,@AllowedAddText,@AllowedDeleteText,@AllowedSeeHighlight,@AllowedAddHighlight,
                                         @AllowedDeleteHighlight,@AllowedHideRedaction,@AllowedAddRedaction,@AllowedDeleteRedaction)
                                   SELECT CAST (SCOPE_IDENTITY() as BIGINT)";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocTypeID = obj.DocTypeId,
                                                             UserGroupID = obj.UserGroupId,
                                                             AllowedSeeText = obj.AllowedSeeText,
                                                             AllowedAddText = obj.AllowedAddText,
                                                             AllowedDeleteText = obj.AllowedDeleteText,
                                                             AllowedSeeHighlight = obj.AllowedSeeHighlight,
                                                             AllowedAddHighlight = obj.AllowedAddHighlight,
                                                             AllowedDeleteHighlight = obj.AllowedDeleteHighlight,
                                                             AllowedHideRedaction = obj.AllowedHideRedaction,
                                                             AllowedAddRedaction = obj.AllowedAddRedaction,
                                                             AllowedDeleteRedaction = obj.AllowedDeleteRedaction
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [AnnotationPermission] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid userGroupId)
        {
            const string query = @"DELETE FROM [AnnotationPermission] 
                                   WHERE UserGroupId = @UserGroupId";
            _context.Connection.Execute(query, new { UserGroupId = userGroupId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [AnnotationPermission] 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public void Update(AnnotationPermission obj)
        {
            const string query = @"UPDATE [AnnotationPermission]
                                   SET [DocTypeID] = @DocTypeID,
                                       [UserGroupID] = @UserGroupID,
                                       [AllowedSeeText] = @AllowedSeeText,
                                       [AllowedAddText] = @AllowedAddText,
                                       [AllowedDeleteText] = @AllowedDeleteText,
                                       [AllowedSeeHighlight] = @AllowedSeeHighlight,
                                       [AllowedAddHighlight] = @AllowedAddHighlight,
                                       [AllowedDeleteHighlight] = @AllowedDeleteHighlight,
                                       [AllowedHideRedaction] = @AllowedHideRedaction,
                                       [AllowedAddRedaction] = @AllowedAddRedaction,
                                       [AllowedDeleteRedaction] = @AllowedDeleteRedaction
                                    WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                DocTypeID = obj.DocTypeId,
                                                UserGroupID = obj.UserGroupId,
                                                AllowedSeeText = obj.AllowedSeeText,
                                                AllowedAddText = obj.AllowedAddText,
                                                AllowedDeleteText = obj.AllowedDeleteText,
                                                AllowedSeeHighlight = obj.AllowedSeeHighlight,
                                                AllowedAddHighlight = obj.AllowedAddHighlight,
                                                AllowedDeleteHighlight = obj.AllowedDeleteHighlight,
                                                AllowedHideRedaction = obj.AllowedHideRedaction,
                                                AllowedAddRedaction = obj.AllowedAddRedaction,
                                                AllowedDeleteRedaction = obj.AllowedDeleteRedaction,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public List<AnnotationPermission> GetByUser(Guid docTypeId, List<Guid> userGroupIds)
        {
            const string query = @"SELECT DISTINCT a.* 
                                   FROM [AnnotationPermission] a 
                                   WHERE a.DocTypeID = @DocTypeID AND 
                                         a.UserGroupId IN @UserGroupIds";
            return _context.Connection.Query<AnnotationPermission>(query, new { DocTypeID = docTypeId, UserGroupIds = userGroupIds }, _context.CurrentTransaction).ToList();
        }

        public AnnotationPermission GetAnnotationPermission(Guid docTypeId, Guid userGroupId)
        {
            const string query = @"SELECT * 
                                   FROM [AnnotationPermission]  
                                   WHERE DocTypeID = @DocTypeID AND 
                                         UserGroupID = @UserGroupID";
            return _context.Connection.Query<AnnotationPermission>(query, new { DocTypeID = docTypeId, UserGroupID = userGroupId }, _context.CurrentTransaction).FirstOrDefault();
        }
    }
}