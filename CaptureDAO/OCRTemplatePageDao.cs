using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
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
                                   WHERE [OCRTemplateId] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public List<OCRTemplatePage> GetByOCRTemplate(Guid templateId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplatePage] 
                                   WHERE [OCRTemplateId] = @OCRTemplateId";
            return _context.Connection.Query<OCRTemplatePage>(query, new { OCRTemplateId = templateId }, _context.CurrentTransaction).ToList();
        }
    }
}