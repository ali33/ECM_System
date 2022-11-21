using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System.Text;

namespace Ecm.CaptureDAO
{
    public class DocumentDao
    {
        private readonly DapperContext _context;

        public DocumentDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Document document)
        {
            // HungLe - 2014/07/18 - Edit adding doc name - Start
            //            const string query = @"INSERT INTO [Document] 
            //                                          ([DocTypeId],[BinaryType],[PageCount],[IsRejected],[BatchId],[CreatedDate],[CreatedBy])
            //                                OUTPUT inserted.ID  
            //                                   VALUES (@DocTypeId,@BinaryType,@PageCount,@IsRejected,@BatchId,@CreatedDate,@CreatedBy)";
            const string query = @"INSERT INTO [Document] 
                                          ([DocName],[DocTypeId],[BinaryType],[PageCount],[IsRejected],[BatchId],[CreatedDate],[CreatedBy],[Order])
                                OUTPUT inserted.ID  
                                   VALUES (@DocName,@DocTypeId,@BinaryType,@PageCount,@IsRejected,@BatchId,@CreatedDate,@CreatedBy,@Order)";
            // HungLe - 2014/07/18 - Edit adding doc name - End
            document.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {
                                            // HungLe - 2014/07/18 - Adding doc name - Start
                                            DocName = document.DocName,
                                            // HungLe - 2014/07/18 - Adding doc name - End
                                            DocTypeId = document.DocTypeId,
                                            BinaryType = document.BinaryType,
                                            PageCount = document.PageCount,
                                            IsRejected = document.IsRejected,
                                            BatchId = document.BatchId,
                                            CreatedDate = document.CreatedDate,
                                            CreatedBy = document.CreatedBy,
                                            Order = document.Order
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void Delete(List<Guid> ids)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE Id in @Ids";
            _context.Connection.Execute(query, new { Ids = ids }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE [DocTypeId] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByBatch(Guid batchId)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE [BatchId] = @BatchId";
            _context.Connection.Execute(query, new { BatchId = batchId }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [Document] 
                                   WHERE [BatchId] IN (SELECT [Id] FROM [Batch] WHERE BatchTypeId = @BatchTypeId)";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public Document GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [Document] 
                                   WHERE Id = @Id";
            return _context.Connection.Query<Document>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<Document> GetByBatch(Guid batchId)
        {
            const string query = @"SELECT * FROM [Document] WHERE [BatchId] = @BatchId ORDER BY [Order]";
            return _context.Connection.Query<Document>(query, new { BatchId = batchId }, _context.CurrentTransaction).ToList();
        }

        public void Update(Document obj)
        {
            // HungLe - 2014/07/18 - Edit adding doc name - Start
            //            const string query = @"UPDATE [Document]
            //                                   SET   [BinaryType] = @BinaryType,
            //                                         [PageCount] = @PageCount,
            //                                         [IsRejected] = @IsRejected,
            //                                         [DocTypeId] = @DocTypeId,
            //                                         [ModifiedDate] = @ModifiedDate,
            //                                         [ModifiedBy] = @ModifiedBy
            //                                   WHERE Id = @Id";
            const string query = @"UPDATE [Document]
                                   SET   [DocName] = @DocName,
                                         [BinaryType] = @BinaryType,
                                         [PageCount] = @PageCount,
                                         [IsRejected] = @IsRejected,
                                         [DocTypeId] = @DocTypeId,
                                         [ModifiedDate] = @ModifiedDate,
                                         [ModifiedBy] = @ModifiedBy
                                   WHERE Id = @Id";
            // HungLe - 2014/07/18 - Edit adding doc name - End
            _context.Connection.Execute(query,
                                        new
                                        {
                                            // HungLe - 2014/07/18 - Adding doc name - Start
                                            DocName = obj.DocName,
                                            // HungLe - 2014/07/18 - Adding doc name - End
                                            BinaryType = obj.BinaryType,
                                            PageCount = obj.PageCount,
                                            IsRejected = obj.IsRejected,
                                            DocTypeId = obj.DocTypeId,
                                            ModifiedDate = obj.ModifiedDate,
                                            ModifiedBy = obj.ModifiedBy,
                                            Id = obj.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void Reject(Document obj)
        {
            // HungLe - 2014/07/18 - Edit adding doc name - Start
            //            const string query = @"UPDATE [Document]
            //                                   SET   [BinaryType] = @BinaryType,
            //                                         [PageCount] = @PageCount,
            //                                         [IsRejected] = @IsRejected,
            //                                         [DocTypeId] = @DocTypeId,
            //                                         [ModifiedDate] = @ModifiedDate,
            //                                         [ModifiedBy] = @ModifiedBy
            //                                   WHERE Id = @Id";
            const string query = @"UPDATE [Document]
                                   SET   [IsRejected] = @IsRejected,
                                         [ModifiedDate] = @ModifiedDate,
                                         [ModifiedBy] = @ModifiedBy
                                   WHERE Id = @Id";
            // HungLe - 2014/07/18 - Edit adding doc name - End
            _context.Connection.Execute(query,
                                        new
                                        {
                                            IsRejected = obj.IsRejected,
                                            ModifiedDate = obj.ModifiedDate,
                                            ModifiedBy = obj.ModifiedBy,
                                            Id = obj.Id
                                        },
                                        _context.CurrentTransaction);
        }

        /// <summary>
        /// Get list simple information of doc by doc id.
        /// </summary>
        /// <param name="id">Id of document.</param>
        /// <returns></returns>
        public DocumentInfoMobile GetInfo(Guid id)
        {
            const string query = @"SELECT [Id], [DocName], [PageCount] 
                                   FROM [Document] 
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<DocumentInfoMobile>(query, new { Id = id },
                                                                 _context.CurrentTransaction).SingleOrDefault();
        }

        /// <summary>
        /// Get list simple information of doc by batch id.
        /// </summary>
        /// <param name="batchId">Id of batch.</param>
        /// <returns></returns>
        public List<DocumentInfoMobile> GetInfoByBatch(Guid batchId)
        {
            const string query = @"SELECT [Id], [DocName], [DocTypeId], [PageCount] , [IsRejected]
                                   FROM [Document] 
                                   WHERE [BatchId] = @BatchId";
            return _context.Connection.Query<DocumentInfoMobile>(query, new { BatchId = batchId },
                                                                 _context.CurrentTransaction).ToList();
        }

        /// <summary>
        /// Check list document type whether have document or not.
        /// </summary>
        /// <param name="docTypeIds">List document type id need to check</param>
        /// <returns>List of document type id which have document</returns>
        public List<Guid> CheckDocTypeHaveDocument(List<Guid> docTypeIds)
        {
            const string query = @"
SELECT DISTINCT 
    DocTypeId
FROM 
    Document
WHERE 
";

            if (docTypeIds == null || docTypeIds.Count == 0)
            {
                return new List<Guid>();
            }

            var whereBuilder = new StringBuilder();

            whereBuilder.AppendFormat("    DocTypeId = '{0}'", docTypeIds[0].ToString());
            for (int i = 1; i < docTypeIds.Count; i++)
            {
                whereBuilder.AppendFormat("{0}    OR DocTypeId = '{1}'",
                                            Environment.NewLine,
                                            docTypeIds[i].ToString());
            }

            return _context.Connection.Query<Guid>(query + whereBuilder.ToString()).ToList();
        }

        public void DeleteRecur(Guid id)
        {
            const string query = @"
DELETE FROM [Annotation] WHERE DocId = @Id;
DELETE FROM [Page] WHERE DocId = @Id;
DELETE FROM [DocumentFieldValue] WHERE DocId = @Id;
DELETE FROM [Document] WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void UpdateRejectStatus(Guid docId, bool isRejected)
        {

            const string query = @"UPDATE [Document]
                                   SET   [IsRejected] = @IsRejected
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            IsRejected = isRejected,

                                            Id = docId
                                        },
                                        _context.CurrentTransaction);
        }


    }
}