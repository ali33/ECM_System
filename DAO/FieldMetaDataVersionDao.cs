using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class FieldMetaDataVersionDao
    {
        private readonly DapperContext _context;

        public FieldMetaDataVersionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(FieldMetadataVersion obj)
        {
            const string query = @"INSERT INTO [FieldMetaDataVersion]
                                            ([DocTypeID],[Name],[DefautValue],[DataType],[IsLookup],[DisplayOrder],
                                             [IsRestricted],[IsRequired],[IsSystemField],[MaxLength])
                                OUTPUT inserted.ID
                                   VALUES   (@DocTypeID,@Name,@DefautValue,@DataType,@IsLookup,@DisplayOrder,
                                             @IsRestricted,@IsRequired,@IsSystemField,@MaxLength)";
            obj.Id = _context.Connection.Query<Guid>(query,new
                                                    {
                                                        DocTypeID = obj.DocTypeId,
                                                        Name = obj.Name,
                                                        DefautValue = obj.DefautValue,
                                                        DataType = obj.DataType,
                                                        IsLookup = obj.IsLookup,
                                                        DisplayOrder = obj.DisplayOrder,
                                                        IsRestricted = obj.IsRestricted,
                                                        IsRequired = obj.IsRequired,
                                                        IsSystemField = obj.IsSystemField,
                                                        //obj.FieldUniqueId,
                                                        MaxLength = obj.MaxLength
                                                    },
                                                _context.CurrentTransaction).Single();
        }

        public FieldMetadataVersion GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM FieldMetaDataVersion 
                                   WHERE ID = @ID";
            return _context.Connection.Query<FieldMetadataVersion>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<FieldMetadataVersion> GetByDeletedDocType(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM FieldMetaDataVersion 
                                   WHERE DocTypeID = @DocTypeID";
            return _context.Connection.Query<FieldMetadataVersion>(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();

        }
    }
}