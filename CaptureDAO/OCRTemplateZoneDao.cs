using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class OCRTemplateZoneDao
    {
        private readonly DapperContext _context;

        public OCRTemplateZoneDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(OCRTemplateZone obj)
        {
            const string query = @"INSERT INTO [OCRTemplateZone]
                                            ([FieldMetaDataId],[OCRTemplatePageId],[Top],[Left],[Width],[Height],[CreatedBy],[CreatedOn])
                                   VALUES   (@FieldMetaDataId,@OCRTemplatePageId,@Top,@Left,@Width,@Height,@CreatedBy,@CreatedOn)";
            _context.Connection.Execute(query,
                                              new
                                                  {
                                                      FieldMetaDataId = obj.FieldMetaDataId,
                                                      OCRTemplatePageId = obj.OCRTemplatePageId,
                                                      Top = obj.Top,
                                                      Left = obj.Left,
                                                      Width = obj.Width,
                                                      Height = obj.Height,
                                                      CreatedBy = obj.CreatedBy,
                                                      CreatedOn = obj.CreatedOn
                                                  },
                                              _context.CurrentTransaction);
        }

        public void Delete(Guid fieldId)
        {
            const string query = @"DELETE FROM [OCRTemplateZone] 
                                   WHERE FieldMetaDataId = @FieldId";
            _context.Connection.Execute(query, new { FieldId = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [OCRTemplateZone]
                                   WHERE [FieldMetaDataId] IN (SELECT [Id] 
                                                               FROM [DocumentFieldMetaData] WHERE [DocTypeId] = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public List<OCRTemplateZone> GetByOCRTemplatePage(Guid ocrTemplatePageId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplateZone] 
                                   WHERE [OCRTemplatePageId] = @OCRTemplatePageId";
            return _context.Connection.Query<OCRTemplateZone>(query, new { OCRTemplatePageId = ocrTemplatePageId }, _context.CurrentTransaction).ToList();
        }

        public OCRTemplateZone GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplateZone] 
                                   WHERE [FieldMetaDataId] = @FieldId";
            return _context.Connection.Query<OCRTemplateZone>(query, new { FieldId = fieldId }, _context.CurrentTransaction).FirstOrDefault();
        }
    }
}