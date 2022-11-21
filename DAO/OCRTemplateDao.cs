using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class OCRTemplateDao
    {
        private readonly DapperContext _context;

        public OCRTemplateDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(OCRTemplate obj)
        {
            const string query = @"INSERT INTO [OCRTemplate] ([DocTypeId],[LanguageId])
                                   VALUES (@DocTypeId,@LanguageID)";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                DocTypeID = obj.DocTypeId,
                                                LanguageID = obj.Language.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public void Delete(Guid docTypeId)
        {
            const string query = @"DELETE FROM [OCRTemplate] 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public OCRTemplate GetById(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplate] 
                                   WHERE DocTypeID = @DocTypeID";
            return _context.Connection.Query<OCRTemplate>(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction).FirstOrDefault();
        }
    }
}