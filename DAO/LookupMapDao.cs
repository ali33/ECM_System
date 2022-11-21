using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class LookupMapDao
    {
        private readonly DapperContext _context;

        public LookupMapDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(LookupMap obj)
        {
            const string query = @"INSERT INTO [LookupMap] ([FieldId],[ArchiveFieldId],[DataColumn])
                            OUTPUT inserted.ID
                                   VALUES (@FieldId,@ArchiveFieldId,@DataColumn)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             FieldID = obj.FieldId,
                                                             ArchiveFieldId = obj.ArchiveFieldId,
                                                             DataColumn = obj.DataColumn
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [LookupMap] 
                                   WHERE FieldID = @FieldID";
            _context.Connection.Execute(query, new { FieldID = fieldId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [LookupMap]
                                   WHERE FieldID IN (SELECT FieldID 
                                                     FROM FieldMetaData WHERE DocTypeID = @DocTypeID)";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public List<LookupMap> GetByField(Guid fieldId)
        {
            const string query = @"SELECT m.*, f.Name FROM [LookupMap] m LEFT JOIN [FieldMetaData] f ON f.ID = m.ArchiveFieldId WHERE FieldID = @FieldID";
            return _context.Connection.Query<LookupMap>(query, new { FieldID = fieldId }, _context.CurrentTransaction).ToList();
        }

        public void Delete(List<Guid> ids)
        {
            const string query = @"DELETE FROM [LookupMap] 
                                   WHERE ID IN @Ids";
            var Ids = ids.ToArray<Guid>();
            _context.Connection.Execute(query, new { Ids }, _context.CurrentTransaction);
        }

        public void Update(LookupMap obj)
        {
            const string query = @"UPDATE [LookupMap] 
                                   SET [FieldID] = @FieldID, 
                                       [ArchiveFieldId] = @ArchiveFieldId,
                                       [DataColumn] = @DataColumn 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                FieldID = obj.FieldId,
                                                ArchiveFieldId = obj.ArchiveFieldId,
                                                DataColumn = obj.DataColumn,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }
    }
}