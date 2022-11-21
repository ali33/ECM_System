using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class BatchFieldMetaDataDao
    {
        private readonly DapperContext _context;

        public BatchFieldMetaDataDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(BatchFieldMetaData obj)
        {
            const string query = @"INSERT INTO [BatchFieldMetaData]
                                            ([BatchTypeId],[Name],[DefaultValue],[DataType],[DisplayOrder],
                                             [IsSystemField],[UniqueId],[MaxLength],[UseCurrentDate])
                                OUTPUT inserted.ID     
                                   VALUES   (@BatchTypeId,@Name,@DefaultValue,@DataType,@DisplayOrder,
                                             @IsSystemField,@UniqueId,@MaxLength,@UseCurrentDate)
                                  ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             BatchTypeId = obj.BatchTypeId,
                                                             Name = obj.Name,
                                                             DefaultValue = obj.DefaultValue,
                                                             DataType = obj.DataType,
                                                             DisplayOrder = obj.DisplayOrder,
                                                             IsSystemField = obj.IsSystemField,
                                                             UniqueId = obj.UniqueId,
                                                             MaxLength = obj.MaxLength,
                                                             UseCurrentDate = obj.UseCurrentDate
                                                         },
                                                     _context.CurrentTransaction).Single(); 
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [BatchFieldMetaData]
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [BatchFieldMetaData] 
                                   WHERE [BatchTypeId] = @BatchTypeId";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public BatchFieldMetaData GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [BatchFieldMetaData] 
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<BatchFieldMetaData>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<BatchFieldMetaData> GetByBatchType(Guid batchTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [BatchFieldMetaData] 
                                   WHERE BatchTypeId = @BatchTypeId AND IsSystemField = 0";
            return _context.Connection.Query<BatchFieldMetaData>(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public void Update(BatchFieldMetaData obj)
        {
            const string query = @"UPDATE [BatchFieldMetaData]
                                   SET [Name] = @Name,
                                       [DefaultValue] = @DefaultValue,
                                       [DataType] = @DataType,
                                       [DisplayOrder] = @DisplayOrder,
                                       [IsSystemField] = @IsSystemField,
                                       [MaxLength] = @MaxLength,
                                       [UseCurrentDate] = @UseCurrentDate 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                Name = obj.Name,
                                                DefaultValue = obj.DefaultValue,
                                                DataType = obj.DataType,
                                                DisplayOrder = obj.DisplayOrder,
                                                IsSystemField = obj.IsSystemField,
                                                MaxLength = obj.MaxLength,
                                                UseCurrentDate = obj.UseCurrentDate,
                                                Id = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public void UpdateLookup(Guid fieldId, string xml, Guid? lookupActivityId)
        {
            const string query = @"UPDATE [BatchFieldMetaData]
                                   SET [LookupXml] = @LookupXml,
                                       [IsLookup] = @IsLookup,
                                       [LookupActivityId] = @LookupActivityId
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            LookupXml = xml,
                                            Id = fieldId,
                                            IsLookup = !string.IsNullOrEmpty(xml),
                                            LookupActivityId = lookupActivityId
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateLookup(Guid bathTypeId, List<Guid> activityIds)
        {
            const string query = @"
UPDATE BatchFieldMetaData 
SET
	[LookupXml] = NULL, 
	[IsLookup] = 0, 
	[LookupActivityId] = NULL
WHERE 
	IsLookup = 1 
	AND BatchTypeId = @BatchTypeId
	AND LookupActivityId NOT IN @ActivityIds";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            BatchTypeId = bathTypeId,
                                            ActivityIds = activityIds
                                        },
                                        _context.CurrentTransaction);
        }

        /// <summary>
        /// Get the field meta data from field value
        /// </summary>
        /// <param name="fieldValueId"></param>
        /// <returns></returns>
        public BatchFieldMetaData GetByFieldValue(Guid fieldValueId)
        {
            const string query = @"SELECT m.* 
                                   FROM [BatchFieldValue] AS v
                                   INNER JOIN [BatchFieldMetaData] AS m
                                   ON v.[FieldId] = m.[Id]
                                   WHERE v.[Id] = @FieldValueId";
            return _context.Connection.Query<BatchFieldMetaData>(query, new { FieldValueId = fieldValueId }, _context.CurrentTransaction).SingleOrDefault();
        }
    }
}