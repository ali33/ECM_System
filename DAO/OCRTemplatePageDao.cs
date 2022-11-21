using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class OCRTemplatePageDao
    {
        private readonly DapperContext _context;

        public OCRTemplatePageDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(OCRTemplatePage obj)
        {
            const string query = @"INSERT INTO [OCRTemplatePage] ([PageIndex],[Binary],[DPI],[OCRTemplateId],[Width],[Height],[RotateAngle],[FileExtension])
                            OUTPUT inserted.ID
                                VALUES (@PageIndex,@Binary,@DPI,@OCRTemplateId,@Width,@Height,@RotateAngle,@FileExtension)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             PageIndex = obj.PageIndex,
                                                             Binary = obj.Binary,
                                                             DPI = obj.DPI,
                                                             OCRTemplateId = obj.OCRTemplateId,
                                                             Width = obj.Width,
                                                             Height = obj.Height,
                                                             RotateAngle = obj.RotateAngle,
                                                             FileExtension = obj.FileExtension
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [OCRTemplatePage]
                                   WHERE OCRTemplateID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public List<OCRTemplatePage> GetByOCRTemplate(Guid templateId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplatePage] 
                                   WHERE OCRTemplateID = @OCRTemplateID";
            return _context.Connection.Query<OCRTemplatePage>(query, new { OCRTemplateID = templateId }, _context.CurrentTransaction).ToList();
        }
    }
}