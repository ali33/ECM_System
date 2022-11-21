using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class CommentDao
    {
        private readonly DapperContext _context;

        public CommentDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertComment(Comment comment)
        {
            const string query = @"INSERT INTO [Comment]([InstanceID],[IsBatchID],[Note],[CreatedDate],[CreatedBy])
                                OUTPUT inserted.ID 
                                   VALUES(@InstanceId,@IsBatchId,@Note,@CreatedDate,@CreatedBy)";
            comment.Id = _context.Connection.Query<Guid>(query,
                                        new
                                            {
                                                Id = comment.Id,
                                                InstanceId = comment.InstanceId,
                                                IsBatchId = comment.IsBatchId,
                                                Note = comment.Note,
                                                CreatedDate = comment.CreatedDate,
                                                CreatedBy = comment.CreatedBy
                                            },
                                        _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Comment] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByInstance(Guid instanceId)
        {
            const string query = @"DELETE FROM [Comment] 
                                   WHERE [InstanceID] = @InstanceID";
            _context.Connection.Execute(query, new { InstanceID = instanceId }, _context.CurrentTransaction);
        }

        public List<Comment> GetByInstance(Guid instanceId)
        {
            const string query = @"SELECT * 
                                   FROM [Comment] 
                                   WHERE [InstanceID] = @InstanceID ORDER BY [CreatedDate] DESC";
            return _context.Connection.Query<Comment>(query, new { InstanceID = instanceId }, _context.CurrentTransaction).ToList();
        }
    }
}
