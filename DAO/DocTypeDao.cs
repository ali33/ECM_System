using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class DocTypeDao
    {
        private readonly DapperContext _context;

        public DocTypeDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentType obj)
        {
            const string query = @"INSERT INTO [DocumentType]([Name],[CreatedDate],[CreatedBy],[IsOutlook],[Icon])
                            OUTPUT inserted.ID
                                   VALUES(@Name,@CreatedDate,@CreatedBy,@IsOutlook,@Icon)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             Name = obj.Name,
                                                             CreatedDate = obj.CreatedDate,
                                                             CreatedBy = obj.CreatedBy,
                                                             IsOutlook = obj.IsOutlook,
                                                             Icon = obj.Icon
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [DocumentType] 
                                   WHERE [ID] = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
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
                                   WHERE [ID] = @ID";
            return _context.Connection.Query<DocumentType>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(DocumentType obj)
        {
            const string query = @"UPDATE [DocumentType] 
                                   SET    [Name] = @Name,
                                          [ModifiedDate] = @ModifiedDate,
                                          [ModifiedBy] = @ModifiedBy,
                                          [IsOutlook] = @IsOutlook,
                                          [Icon] = @Icon
                                   WHERE [ID] = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                Name = obj.Name,
                                                ModifiedDate = obj.ModifiedDate,
                                                ModifiedBy = obj.ModifiedBy,
                                                IsOutlook = obj.IsOutlook,
                                                Icon = obj.Icon,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public List<DocumentType> GetByUser(List<Guid> userGroupIds)
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM [DocumentType] d 
                                   LEFT JOIN [DocumentTypePermission] dp ON d.Id = dp.DocTypeId 
                                   WHERE dp.UserGroupID IN @UserGroupIDs AND 
                                         dp.AllowedSearch = 1";
            return _context.Connection.Query<DocumentType>(query, new { UserGroupIDs = userGroupIds}, _context.CurrentTransaction).ToList();
        }

        public List<DocumentType> GetCaptureDocumentTypeByUser(List<Guid> userGroupIds)
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM DocumentType d 
                                   LEFT JOIN [DocumentTypePermission] dp ON d.Id = dp.DocTypeId 
                                   WHERE dp.UserGroupID IN @UserGroupIDs AND 
                                         dp.AllowedCapture = 1";
            return _context.Connection.Query<DocumentType>(query, new { UserGroupIDs = userGroupIds }, _context.CurrentTransaction).ToList();
        }

        public DocumentType GetCaptureDocumentTypeByUser(Guid id, List<Guid> userGroupIds)
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM DocumentType d 
                                   LEFT JOIN [DocumentTypePermission] dp ON d.Id = dp.DocTypeId 
                                   WHERE dp.UserGroupId IN @UserGroupIds AND 
                                         d.ID = @ID AND
                                         dp.AllowedCapture = 1";
            return _context.Connection.Query<DocumentType>(query, new { UserGroupIds = userGroupIds, ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public DocumentType GetByAllowedSearch(Guid id, List<Guid> userGroupIds)
        {
            const string query = @"SELECT DISTINCT d.* 
                                   FROM DocumentType d 
                                   LEFT JOIN [DocumentTypePermission] dp ON d.Id = dp.DocTypeId 
                                   WHERE dp.UserGroupId IN @UserGroupIds AND 
                                         dp.AllowedSearch = 1 AND 
                                         d.ID = @ID";
            return _context.Connection.Query<DocumentType>(query, new { UserGroupIds = userGroupIds, ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }
    }
}