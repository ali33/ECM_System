using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class BatchTypePermissionDao
    {
        private readonly DapperContext _context;

        public BatchTypePermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(BatchTypePermission obj)
        {
            const string query = @"INSERT INTO [BatchTypePermission] ([BatchTypeId],[UserGroupId],[CanCapture],[CanAccess],[CanIndex],[CanClassify])
                                OUTPUT inserted.ID     
                                    VALUES (@BatchTypeId,@UserGroupId,@CanCapture,@CanAccess,@CanIndex,@CanClassify)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             BatchTypeId = obj.BatchTypeId,
                                                             UserGroupId = obj.UserGroupId,
                                                             CanCapture = obj.CanCapture,
                                                             CanAccess = obj.CanAccess,
                                                             CanIndex = obj.CanIndex,
                                                             CanClassify = obj.CanClassify
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [BatchTypePermission] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByBatcType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [BatchTypePermission] 
                                   WHERE [BatchTypeId] = @BatchTypeId";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid groupId)
        {
            const string query = @"DELETE FROM [BatchTypePermission] 
                                   WHERE [UserGroupId] = @UserGroupId";
            _context.Connection.Execute(query, new { UserGroupId = groupId }, _context.CurrentTransaction);
        }

        public void Update(BatchTypePermission obj)
        {
            const string query = @"UPDATE [BatchTypePermission] 
                                   SET 
                                        [CanCapture] = @CanCapture, 
                                        [CanAccess] = @CanAccess,
                                        [CanIndex] = @CanIndex,
                                        [CanClassify] = @CanClassify
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            CanCapture = obj.CanCapture,
                                            CanAccess = obj.CanAccess,
                                            CanIndex = obj.CanIndex,
                                            CanClassify = obj.CanClassify,
                                            Id = obj.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public BatchTypePermission GetBatchTypePermission(Guid batchTypeId, Guid userGroupId)
        {
            const string query = @"SELECT * 
                                   FROM [BatchTypePermission] 
                                   WHERE UserGroupId = @UserGroupId AND 
                                         BatchTypeId = @BatchTypeId";
            return _context.Connection.Query<BatchTypePermission>(query, new { UserGroupId = userGroupId, BatchTypeId = batchTypeId }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<BatchTypePermission> GetByUser(List<Guid> groupIds, Guid batchTypeId)
        {
            const string query = @"SELECT * 
                                   FROM BatchTypePermission 
                                   WHERE UserGroupId in @UserGroupIds AND BatchTypeId = @BatchTypeId";
            return _context.Connection.Query<BatchTypePermission>(query, new { UserGroupIds = groupIds, BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public List<BatchTypePermission> GetByUserGroupRange(List<Guid> ids)
        {
            const string query = @"SELECT * 
                                   FROM BatchTypePermission 
                                   WHERE [UserGroupId] in @Ids";
            return _context.Connection.Query<BatchTypePermission>(query, new { Ids = ids }, _context.CurrentTransaction).ToList();
        }
    }
}
