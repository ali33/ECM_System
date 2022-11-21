using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class ReleaseDocumentDao
    {
        private readonly DapperContext _context;

        public ReleaseDocumentDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(ReleaseDocument obj)
        {
            const string query = @"INSERT INTO [ReleaseDocument] 
                                          ([DocTypeId],[BinaryType],[PageCount],[ReleaseBatchId],[CreatedDate],[CreatedBy])
                                OUTPUT inserted.ID  
                                   VALUES (@DocTypeId,@BinaryType,@PageCount,@ReleaseBatchId,@CreatedDate,@CreatedBy)";
            obj.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {
                                            DocTypeId = obj.DocTypeId,
                                            BinaryType = obj.BinaryType,
                                            PageCount = obj.PageCount,
                                            ReleaseBatchId = obj.ReleaseBatchId,
                                            CreatedDate = obj.CreatedDate,
                                            CreatedBy = obj.CreatedBy
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [ReleaseDocument] 
                                   WHERE [DocTypeId] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [ReleaseDocument] 
                                   WHERE [ReleaseBatchId] IN (SELECT [Id] FROM [ReleaseBatch] WHERE BatchTypeId = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public ReleaseDocument GetById(Guid id)
        {
            const string query = @"SELECT * FROM ReleaseDocument WHERE [ID] = @ID";
            return _context.Connection.Query<ReleaseDocument>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<ReleaseDocument> GetByDocumentType(Guid documentTypeId)
        {
            const string query = @"SELECT * FROM ReleaseDocument WHERE [DocTypeID] = @DocTypeID";
            return _context.Connection.Query<ReleaseDocument>(query, new { DocTypeID = documentTypeId }, _context.CurrentTransaction).ToList();
        }
    }
}
