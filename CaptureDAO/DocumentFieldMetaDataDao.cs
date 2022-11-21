using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class DocFieldMetaDataDao
    {
        private readonly DapperContext _context;

        public DocFieldMetaDataDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentFieldMetaData fieldMetadata)
        {
            const string query = @"INSERT INTO [DocumentFieldMetaData]
                                            ([ParentFieldId],[DocTypeId],[Name],[DefaultValue],[DataType],[IsLookup],[DisplayOrder],
                                             [IsRestricted],[IsRequired],[IsSystemField],[UniqueId],[MaxLength],[UseCurrentDate],[ValidationScript],[ValidationPattern])
                                    OUTPUT inserted.ID     
                                   VALUES   (@ParentFieldId,@DocTypeId,@Name,@DefaultValue,@DataType,@IsLookup,@DisplayOrder,
                                             @IsRestricted,@IsRequired,@IsSystemField,@UniqueId,@MaxLength,@UseCurrentDate,@ValidationScript,@ValidationPattern)
                                   ";
            fieldMetadata.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             ParentFieldId = fieldMetadata.ParentFieldId,
                                                             DocTypeId = fieldMetadata.DocTypeId,
                                                             Name = fieldMetadata.Name,
                                                             DefaultValue = fieldMetadata.DefaultValue,
                                                             DataType = fieldMetadata.DataType,
                                                             IsLookup = fieldMetadata.IsLookup,
                                                             DisplayOrder = fieldMetadata.DisplayOrder,
                                                             IsRestricted = fieldMetadata.IsRestricted,
                                                             IsRequired = fieldMetadata.IsRequired,
                                                             IsSystemField = fieldMetadata.IsSystemField,
                                                             UniqueId = fieldMetadata.UniqueId,
                                                             MaxLength = fieldMetadata.MaxLength,
                                                             UseCurrentDate = fieldMetadata.UseCurrentDate,
                                                             ValidationScript = fieldMetadata.ValidationScript,
                                                             ValidationPattern = fieldMetadata.ValidationPattern
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [DocumentFieldMetaData] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteChildren(Guid parentId)
        {
            const string query = @"DELETE FROM DocumentFieldMetaData 
                                   WHERE ParentFieldId = @ParentFieldId";
            _context.Connection.Execute(query, new { ParentFieldId = parentId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [DocumentFieldMetaData] 
                                   WHERE [DocTypeId] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public DocumentFieldMetaData GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentFieldMetaData]
                                   WHERE [Id] = @Id  AND ParentFieldId IS NULL";
            return _context.Connection.Query<DocumentFieldMetaData>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<DocumentFieldMetaData> GetByDocType(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentFieldMetaData] 
                                   WHERE [DocTypeId] = @DocTypeId AND ParentFieldId IS NULL AND IsSystemField = 0";
            return _context.Connection.Query<DocumentFieldMetaData>(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public List<DocumentFieldMetaData> GetChildren(Guid parentId)
        {
            const string query = @"SELECT * 
                                   FROM DocumentFieldMetaData 
                                   WHERE ParentFieldId = @ParentFieldId";
            return _context.Connection.Query<DocumentFieldMetaData>(query, new { ParentFieldId = parentId }, _context.CurrentTransaction).ToList();
        }

        public DocumentFieldMetaData GetChild(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM DocumentFieldMetaData 
                                   WHERE Id = @Id";
            return _context.Connection.Query<DocumentFieldMetaData>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(DocumentFieldMetaData fieldMetadata)
        {
            const string query = @"UPDATE [DocumentFieldMetaData]
                                   SET [ParentFieldId] = @ParentFieldId,
                                       [Name] = @Name,
                                       [DefaultValue] = @DefaultValue,
                                       [DataType] = @DataType,
                                       [IsLookup] = @IsLookup,
                                       [DisplayOrder] = @DisplayOrder,
                                       [IsRestricted] = @IsRestricted,
                                       [IsRequired] = @IsRequired,
                                       [IsSystemField] = @IsSystemField,
                                       [MaxLength] = @MaxLength,
                                       [UseCurrentDate] = @UseCurrentDate,
                                       [ValidationScript] = @ValidationScript,
                                       [ValidationPattern] = @ValidationPattern  
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                ParentFieldId = fieldMetadata.ParentFieldId,
                                                Name = fieldMetadata.Name,
                                                DefaultValue = fieldMetadata.DefaultValue,
                                                DataType = fieldMetadata.DataType,
                                                IsLookup = fieldMetadata.IsLookup,
                                                DisplayOrder = fieldMetadata.DisplayOrder,
                                                IsRestricted = fieldMetadata.IsRestricted,
                                                IsRequired = fieldMetadata.IsRequired,
                                                IsSystemField = fieldMetadata.IsSystemField,
                                                MaxLength = fieldMetadata.MaxLength,
                                                UseCurrentDate = fieldMetadata.UseCurrentDate,
                                                ValidationScript = fieldMetadata.ValidationScript,
                                                ValidationPattern = fieldMetadata.ValidationPattern,
                                                Id = fieldMetadata.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public void UpdateLookup(Guid fieldId, string xml, Guid? lookupActivityId)
        {
            const string query = @"UPDATE [DocumentFieldMetaData]
                                   SET [LookupXml] = @LookupXml, [IsLookup] = @IsLookup, [LookupActivityId] = @LookupActivityId
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            LookupXml = string.IsNullOrEmpty(xml) ? null : xml,
                                            Id = fieldId,
                                            IsLookup = !string.IsNullOrEmpty(xml),
                                            LookupActivityId = lookupActivityId
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateLookup(Guid bathTypeId, List<Guid> activityIds)
        {
            const string query = @"
UPDATE DocumentFieldMetaData 
SET
	[LookupXml] = NULL, 
	[IsLookup] = 0, 
	[LookupActivityId] = NULL
WHERE 
	IsLookup = 1 
	AND DocTypeId IN 
	(
		SELECT Id FROM DocumentType WHERE BatchTypeId = @BatchTypeId
	)
	AND LookupActivityId NOT IN @ActivityIds";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            BatchTypeId = bathTypeId,
                                            ActivityIds = activityIds
                                        },
                                        _context.CurrentTransaction);
        }
    }
}