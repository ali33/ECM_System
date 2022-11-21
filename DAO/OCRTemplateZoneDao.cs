using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
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
                                            ([FieldMetaDataId],[OCRTemplatePageId],[Top],[Left],[Width],[Height],[CreatedBy],
                                             [CreatedOn],[ModifiedBy],[ModifiedOn])
                                   VALUES   (@FieldMetaDataId,@OCRTemplatePageId,@Top,@Left,@Width,@Height,@CreatedBy,
                                             @CreatedOn,@ModifiedBy,@ModifiedOn)";
            _context.Connection.Execute(query,
                                              new
                                                  {
                                                      FieldMetaDataID = obj.FieldMetaDataId,
                                                      OCRTemplatePageID = obj.OCRTemplatePageId,
                                                      Top = obj.Top,
                                                      Left = obj.Left,
                                                      Width = obj.Width,
                                                      Height = obj.Height,
                                                      CreatedBy = obj.CreatedBy,
                                                      CreatedOn = obj.CreatedOn,
                                                      ModifiedBy = obj.ModifiedBy,
                                                      ModifiedOn = obj.ModifiedOn
                                                  },
                                              _context.CurrentTransaction);
        }

        public void Delete(Guid fieldId)
        {
            const string query = @"DELETE FROM [OCRTemplateZone] 
                                   WHERE FieldMetaDataID = @FieldID";
            _context.Connection.Execute(query, new { FieldID = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [OCRTemplateZone]
                                   WHERE FieldMetaDataID IN (SELECT ID 
                                                             FROM FieldMetaData WHERE DocTypeID = @DocTypeID)";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public List<OCRTemplateZone> GetByOCRTemplatePage(Guid ocrTemplatePageId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplateZone] 
                                   WHERE OCRTemplatePageID = @OCRTemplatePageID";
            return _context.Connection.Query<OCRTemplateZone>(query, new { OCRTemplatePageID = ocrTemplatePageId }, _context.CurrentTransaction).ToList();
        }

        public OCRTemplateZone GetByField(Guid fieldId)
        {
            const string query = @"SELECT * 
                                   FROM [OCRTemplateZone] 
                                   WHERE FieldMetaDataID = @FieldID";
            return _context.Connection.Query<OCRTemplateZone>(query, new { FieldID = fieldId }, _context.CurrentTransaction).FirstOrDefault();
        }
    }
}