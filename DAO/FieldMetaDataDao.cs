using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class FieldMetaDataDao
    {
        private readonly DapperContext _context;

        public FieldMetaDataDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(FieldMetaData obj)
        {
            const string query = @"INSERT INTO [FieldMetaData]
                                            ([ParentFieldID],[DocTypeID],[Name],[DefautValue],[DataType],[IsLookup],[DisplayOrder],
                                             [IsRestricted],[IsRequired],[IsSystemField],[MaxLength],[UseCurrentDate],[LookupXML])
                                OUTPUT inserted.ID
                                   VALUES   (@ParentFieldID,@DocTypeID,@Name,@DefautValue,@DataType,@IsLookup,@DisplayOrder,
                                             @IsRestricted,@IsRequired,@IsSystemField,@MaxLength,@UseCurrentDate,@LookupXML)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             ParentFieldID = obj.ParentFieldId,
                                                             DocTypeID = obj.DocTypeId,
                                                             Name = obj.Name,
                                                             DefautValue = obj.DefautValue,
                                                             DataType = obj.DataType,
                                                             IsLookup = obj.IsLookup,
                                                             DisplayOrder = obj.DisplayOrder,
                                                             IsRestricted = obj.IsRestricted,
                                                             IsRequired = obj.IsRequired,
                                                             IsSystemField = obj.IsSystemField,
                                                             MaxLength = obj.MaxLength,
                                                             UseCurrentDate = obj.UseCurrentDate,
                                                             LookupXML = obj.LookupXML
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM FieldMetaData 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteChildren(Guid parentId)
        {
            const string query = @"DELETE FROM FieldMetaData 
                                   WHERE ParentFieldID = @ParentFieldID";
            _context.Connection.Execute(query, new { ParentFieldID = parentId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM FieldMetaData 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public FieldMetaData GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM FieldMetaData 
                                   WHERE Id = @Id AND ParentFieldId IS NULL";
            return _context.Connection.Query<FieldMetaData>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<FieldMetaData> GetByDocType(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM FieldMetaData 
                                   WHERE DocTypeID = @DocTypeID AND ParentFieldId IS NULL ORDER BY DisplayOrder ASC";
            return _context.Connection.Query<FieldMetaData>(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public List<FieldMetaData> GetChildren(Guid parentId)
        {
            const string query = @"SELECT * 
                                   FROM FieldMetaData 
                                   WHERE ParentFieldID = @ParentFieldID";
            return _context.Connection.Query<FieldMetaData>(query, new { ParentFieldID = parentId }, _context.CurrentTransaction).ToList();
        }

        public FieldMetaData GetChild(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM FieldMetaData 
                                   WHERE ID = @ID";
            return _context.Connection.Query<FieldMetaData>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(FieldMetaData obj)
        {
            const string query = @"UPDATE [FieldMetaData]
                                   SET [ParentFieldID] = @ParentFieldID,
                                       [DocTypeID] = @DocTypeID,
                                       [Name] = @Name,
                                       [DefautValue] = @DefautValue,
                                       [DataType] = @DataType,
                                       [IsLookup] = @IsLookup,
                                       [DisplayOrder] = @DisplayOrder,
                                       [IsRestricted] = @IsRestricted,
                                       [IsRequired] = @IsRequired,
                                       [IsSystemField] = @IsSystemField,                                       
                                       [MaxLength] = @MaxLength,
                                       [UseCurrentDate] = @UseCurrentDate,
                                       [LookupXML] = @LookupXML
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                ParentFieldID = obj.ParentFieldId,
                                                DocTypeID = obj.DocTypeId,
                                                Name = obj.Name,
                                                DefautValue = obj.DefautValue,
                                                DataType = obj.DataType,
                                                IsLookup = obj.IsLookup,
                                                DisplayOrder = obj.DisplayOrder,
                                                IsRestricted = obj.IsRestricted,
                                                IsRequired = obj.IsRequired,
                                                IsSystemField = obj.IsSystemField,
                                                MaxLength = obj.MaxLength,
                                                Id = obj.Id,
                                                UseCurrentDate = obj.UseCurrentDate,
                                                LookupXML = obj.LookupXML
                                            },
                                        _context.CurrentTransaction);
        }
    }
}