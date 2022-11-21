using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class BarcodeConfigurationDao
    {
        private readonly DapperContext _context;

        public BarcodeConfigurationDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(BarcodeConfiguration obj)
        {
            const string query = @"INSERT INTO [BarcodeConfiguration]
                                        ([DocumentTypeID],[BarcodeType],[IsDocumentSeparator],[RemoveSeparatorPage],
                                         [HasDoLookup],[MapValueToFieldId],[BarcodePosition])
                            OUTPUT inserted.ID
                                   VALUES
                                        (@DocumentTypeID,@BarcodeType,@IsDocumentSeparator,@RemoveSeparatorPage,
                                         @HasDoLookup,@MapValueToFieldId,@BarcodePosition)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocumentTypeID = obj.DocumentTypeId,
                                                             BarcodeType = obj.BarcodeType,
                                                             IsDocumentSeparator = obj.IsDocumentSeparator,
                                                             RemoveSeparatorPage = obj.RemoveSeparatorPage,
                                                             HasDoLookup = obj.HasDoLookup,
                                                             MapValueToFieldId = obj.MapValueToFieldId,
                                                             BarcodePosition = obj.BarcodePosition
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [BarcodeConfiguration] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [BarcodeConfiguration]
                                   WHERE DocumentTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public List<BarcodeConfiguration> GetByDocType(Guid docTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [BarcodeConfiguration] 
                                   WHERE DocumentTypeId = @DocumentTypeId";
            return _context.Connection.Query<BarcodeConfiguration>(query, new { DocumentTypeId = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public void DeleteByField(Guid fieldId)
        {
            const string query = @"DELETE FROM [BarcodeConfiguration] 
                                   WHERE MapValueToFieldID = @MapValueToFieldID";
            _context.Connection.Execute(query, new { MapValueToFieldID = fieldId }, _context.CurrentTransaction);
        }

        public void Update(BarcodeConfiguration obj)
        {
            const string query = @"UPDATE [BarcodeConfiguration]
                                   SET [DocumentTypeID] = @DocumentTypeID,
                                       [BarcodeType] = @BarcodeType,
                                       [IsDocumentSeparator] = @IsDocumentSeparator,
                                       [RemoveSeparatorPage] = @RemoveSeparatorPage,
                                       [HasDoLookup] = @HasDoLookup,
                                       [MapValueToFieldId] = @MapValueToFieldId,
                                       [BarcodePosition] = @BarcodePosition
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                DocumentTypeID = obj.DocumentTypeId,
                                                BarcodeType = obj.BarcodeType,
                                                IsDocumentSeparator = obj.IsDocumentSeparator,
                                                RemoveSeparatorPage = obj.RemoveSeparatorPage,
                                                HasDoLookup = obj.HasDoLookup,
                                                MapValueToFieldId = obj.MapValueToFieldId,
                                                BarcodePosition = obj.BarcodePosition,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }
    }
}