using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class DocumentTypeVersionDao
    {
        private readonly DapperContext _context;

        public DocumentTypeVersionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentTypeVersion obj)
        {
            const string query = @"INSERT INTO [DocumentTypeVersion] ([Id], [Name],[CreatedDate],[CreatedBy],[IsOutlook])
                                OUTPUT inserted.ID
                                   VALUES (@Id, @Name,@CreatedDate,@CreatedBy,@IsOutlook)";

            obj.Id = _context.Connection.Query<Guid>(query, new
            {
                Id = obj.Id,
                Name = obj.Name,
                CreatedDate = obj.CreatedDate,
                CreatedBy = obj.CreatedBy,
                IsOutlook = obj.IsOutlook
            }, _context.CurrentTransaction).Single();
        }

        public List<DocumentTypeVersion> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [DocumentTypeVersion]";
            return _context.Connection.Query<DocumentTypeVersion>(query, null, _context.CurrentTransaction).ToList();
        }

        public DocumentTypeVersion GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentTypeVersion] 
                                   WHERE ID = @ID";
            return _context.Connection.Query<DocumentTypeVersion>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

      
    }
}