using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;


namespace Ecm.CaptureDAO
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
                                   VALUES (@DocTypeId,@LanguageId)";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                DocTypeId = obj.DocTypeId,
                                                LanguageId = obj.Language.Id,
                                            },
                                        _context.CurrentTransaction);
        }

        public void Delete(Guid docTypeId)
        {
            const string query = @"DELETE FROM [OCRTemplate] 
                                   WHERE [DocTypeId] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public OCRTemplate GetById(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplate] 
                                   WHERE [DocTypeId] = @DocTypeId";
            return _context.Connection.Query<OCRTemplate>(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction).FirstOrDefault();
        }
    }
}