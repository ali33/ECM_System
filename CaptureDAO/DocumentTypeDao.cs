using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class DocumentTypeDao
    {
        private readonly DapperContext _context;

        public DocumentTypeDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentType obj)
        {
            const string query = @"INSERT INTO [DocumentType]([BatchTypeID],[Name],[Description],[CreatedDate],[CreatedBy],[UniqueId],[Icon])
                                OUTPUT inserted.ID     
                                VALUES(@BatchTypeId,@Name,@Description,@CreatedDate,@CreatedBy,@UniqueId,@Icon)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             BatchTypeId = obj.BatchTypeId,
                                                             Name = obj.Name,
                                                             Description = obj.Description,
                                                             CreatedDate = obj.CreatedDate,
                                                             CreatedBy = obj.CreatedBy,
                                                             UniqueId = obj.UniqueId,
                                                             Icon = obj.Icon
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [DocumentType] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [DocumentType] 
                                   WHERE [BatchTypeId] = @BatchTypeId";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public List<DocumentType> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [DocumentType]";
            return _context.Connection.Query<DocumentType>(query, null, _context.CurrentTransaction).ToList();
        }

        public DocumentType GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentType] 
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<DocumentType>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(DocumentType obj)
        {
            const string query = @"UPDATE [DocumentType] 
                                   SET    [Name] = @Name,
                                          [Description] = @Description,
                                          [Icon] = @Icon,
                                          [ModifiedDate] = @ModifiedDate,
                                          [ModifiedBy] = @ModifiedBy
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                Name = obj.Name,
                                                Description = obj.Description,
                                                Icon = obj.Icon,
                                                ModifiedDate = obj.ModifiedDate,
                                                ModifiedBy = obj.ModifiedBy,
                                                Id = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public List<DocumentType> GetDocumentTypeByBatch(Guid batchTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentType] 
                                   WHERE BatchTypeId = @BatchTypeId";
            return _context.Connection.Query<DocumentType>(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentType> GetDocumentTypeByBatch(Guid batchTypeId, List<Guid> groupIds)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentType] d LEFT JOIN [DocumentTypePermission] dp ON d.Id = dp.DocTypeId
                                   WHERE d.BatchTypeId = @BatchTypeId AND dp.CanAccess = 1 AND dp.UserGroupId IN @GroupIds";
            return _context.Connection.Query<DocumentType>(query, new { BatchTypeId = batchTypeId, GroupIds = groupIds}, _context.CurrentTransaction).ToList();
        }

        /// <summary>
        /// Get list DocumentType in list Id and in list GroupUser
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        public List<Guid> GetCanAccessDocumentTypeIds(List<Guid> groupIds)
        {
            const string query = @"SELECT DISTINCT DocTypeId 
                                   FROM [DocumentTypePermission]
                                   WHERE CanAccess = 1 AND UserGroupId IN @GroupIds";
            return _context.Connection.Query<Guid>(query, new { GroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }

    }
}