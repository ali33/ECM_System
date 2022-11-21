using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class BatchDao
    {
        private readonly DapperContext _context;

        public BatchDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Batch batch)
        {
            const string query = @"INSERT INTO [Batch] 
                                          ([Id],[BatchName],[BatchTypeId],[DocCount],[PageCount],[CreatedDate],[CreatedBy],[IsProcessing],[IsRejected],[TransactionId])
                                   VALUES (@Id,@BatchName,@BatchTypeId,@DocCount,@PageCount,@CreatedDate,@CreatedBy,@IsProcessing,@IsRejected,@TransactionId)";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Id = batch.Id,
                                            BatchName = batch.BatchName,
                                            BatchTypeId = batch.BatchTypeId,
                                            DocCount = batch.DocCount,
                                            PageCount = batch.PageCount,
                                            CreatedDate = batch.CreatedDate,
                                            CreatedBy = batch.CreatedBy,
                                            IsProcessing = batch.IsProcessing,
                                            IsRejected = batch.IsRejected,
                                            TransactionId = Guid.NewGuid()
                                        },
                                        _context.CurrentTransaction);
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Batch] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            const string query = @"DELETE FROM [Batch] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction);
        }

        public Batch GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [Batch] 
                                   WHERE Id = @Id";
            return _context.Connection.Query<Batch>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public Batch CheckLock(Guid id)
        {
            const string query = @"SELECT LockedBy, BlockingActivityName
                                   FROM [Batch] 
                                   WHERE Id = @Id";
            return _context.Connection.Query<Batch>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<Batch> GetAllBatch(List<Guid> groupIds)
        {
            const string query = @"SELECT DISTINCT b.* 
                                   FROM [Batch] AS b JOIN [WorkflowHumanStepPermission] AS wfp ON b.WorkflowDefinitionID = wfp.WorkflowDefinitionID
                                   LEFT JOIN [WorkflowHumanStepPermission] AS wfp1 ON b.BlockingBookmark = wfp1.HumanStepID
                                   LEFT JOIN [BatchType] bt ON bt.Id = b.BatchTypeId
                                   LEFT JOIN [DocumentType] dt ON dt.BatchTypeId = bt.Id
                                   LEFT JOIN [DocumentTypePermission] dp ON dp.DocTypeId = dt.Id
                                   WHERE [IsProcessing] = 0 AND wfp1.UserGroupID in @GroupIds AND dp.UserGroupId in @GroupIds AND BlockingBookmark <> 'AUTORESUME'
                                   ORDER BY [CreatedDate] DESC";

            return _context.Connection.Query<Batch>(query, new { GroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }

        public List<Batch> GetAllBatch()
        {
            const string query = @"SELECT * 
                                   FROM [Batch] 
                                   WHERE IsCompleted = 0  AND BlockingBookmark <> 'AUTORESUME' ORDER BY [CreatedDate] DESC";
            return _context.Connection.Query<Batch>(query, null, _context.CurrentTransaction).ToList();
        }

        public List<Batch> GetByBatchType(Guid batchTypeId)
        {
            const string query = @"SELECT * 
                                   FROM [Batch] 
                                   WHERE [BatchTypeId] = @BatchTypeId AND IsCompleted = 0 ORDER BY [CreatedDate] DESC";
            return _context.Connection.Query<Batch>(query, new { BatchTypeId = batchTypeId }, _context.CurrentTransaction).ToList();
        }

        public List<Batch> GetByBatchType(Guid batchTypeId, List<Guid> groupIds)
        {
            const string query = @"SELECT DISTINCT b.* 
                                   FROM [Batch] AS b JOIN [WorkflowHumanStepPermission] AS wfp ON b.WorkflowDefinitionID = wfp.WorkflowDefinitionID
                                   LEFT JOIN [WorkflowHumanStepPermission] AS wfp1 ON b.BlockingBookmark = wfp1.HumanStepID
                                   LEFT JOIN [BatchType] bt ON bt.Id = b.BatchTypeId
                                   LEFT JOIN [DocumentType] dt ON dt.BatchTypeId = bt.Id
                                   LEFT JOIN [DocumentTypePermission] dp ON dp.DocTypeId = dt.Id
                                   WHERE [IsProcessing] = 0 AND b.[BatchTypeId] = @BatchTypeId AND wfp1.UserGroupID in @GroupIds AND dp.UserGroupId in @GroupIds
                                   ORDER BY [CreatedDate] DESC";

            return _context.Connection.Query<Batch>(query, new { BatchTypeId = batchTypeId, GroupIds = groupIds }, _context.CurrentTransaction).ToList();
        }

        public List<Batch> GetBatchByRange(List<Guid> ids)
        {
            const string query = @"SELECT * FROM [Batch] WHERE [ID] in @IDs AND IsCompleted = 0 ORDER BY [CreatedDate] DESC";
            return _context.Connection.Query<Batch>(query, new { IDs = ids }, _context.CurrentTransaction).ToList();
        }

        public List<Batch> GetCompletedBatchByRange(List<Guid> ids)
        {
            const string query = @"SELECT * FROM [Batch] WHERE [ID] in @IDs AND IsCompleted = 1";
            return _context.Connection.Query<Batch>(query, new { IDs = ids }, _context.CurrentTransaction).ToList();
        }

        public List<Batch> RunAdvanceSearch(Guid batchTypeId, string expression, long pageIndex)
        {
            string settingQuery = @"SELECT Cast([Value] as INT) FROM Setting WHERE [Key] = 'SearchResultPageSize'";
            int pageSize = _context.Connection.Query<int>(settingQuery, null, _context.CurrentTransaction).FirstOrDefault();
            int from = ((int)pageIndex * pageSize) + 1;
            int to = ((int)pageIndex + 1) * pageSize;

            if (string.IsNullOrEmpty(expression) || string.IsNullOrWhiteSpace(expression))
            {
                expression = "1=1";
            }

            string query = @"SELECT * FROM (SELECT TOP (@To) b.*, ROW_NUMBER() OVER (ORDER BY b.ID) rowNumber 
                            FROM [Batch] AS b JOIN [BatchFieldValue] f ON b.Id = f.BatchId JOIN [BatchFieldMetaData] fm ON fm.ID = f.FieldID
                            WHERE b.BatchTypeId = @BatchTypeId AND " + expression + ") as result WHERE rowNumber BETWEEN @From AND @To ORDER BY rowNumber";

            return _context.Connection.Query<Batch>(query, new { BatchTypeId = batchTypeId, From = from, To = to }, _context.CurrentTransaction).ToList();
        }

        public void Update(Batch batch)
        {
            const string query = @"UPDATE [Batch]
                                   SET   [DocCount] = @DocCount,
                                         [BatchName] = @BatchName,
                                         [PageCount] = @PageCount,
                                         [LockedBy] = @LockedBy,
                                         [IsRejected] = @IsRejected,
                                         [ModifiedDate] = @ModifiedDate,
                                         [ModifiedBy] = @ModifiedBy,
                                         [DelegatedBy] = @DelegatedBy, 
                                         [DelegatedTo] = @DelegatedTo, 
                                         [LastAccessedBy] = @LastAccessedBy, 
                                         [LastAccessedDate] = @LastAccessedDate
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            DocCount = batch.DocCount,
                                            BatchName = batch.BatchName,
                                            PageCount = batch.PageCount,
                                            LockedBy = batch.LockedBy,
                                            IsRejected = batch.IsRejected,
                                            ModifiedDate = batch.ModifiedDate,
                                            ModifiedBy = batch.ModifiedBy,
                                            DelegatedBy = batch.DelegatedBy,
                                            DelegatedTo = batch.DelegatedTo,
                                            LastAccessedBy = batch.LastAccessedBy,
                                            LastAccessedDate = batch.LastAccessedDate,
                                            Id = batch.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void Reject(Batch batch)
        {
            const string query = @"UPDATE [Batch]
                                   SET   [DocCount] = @DocCount,
                                         [PageCount] = @PageCount,
                                         [IsRejected] = @IsRejected,
                                         [ModifiedDate] = @ModifiedDate,
                                         [ModifiedBy] = @ModifiedBy,
                                         [LastAccessedBy] = @LastAccessedBy, 
                                         [LastAccessedDate] = @LastAccessedDate
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            DocCount = batch.DocCount,
                                            PageCount = batch.PageCount,
                                            IsRejected = batch.IsRejected,
                                            ModifiedDate = batch.ModifiedDate,
                                            ModifiedBy = batch.ModifiedBy,
                                            LastAccessedBy = batch.LastAccessedBy,
                                            LastAccessedDate = batch.LastAccessedDate,
                                            Id = batch.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void RejectRecur(Guid batchId)
        {
            const string query = @"
UPDATE [Batch] SET [IsRejected] = 1 WHERE [Id] = @BatchId
UPDATE [Document] SET [IsRejected] = 1 WHERE [BatchId] = @BatchId
UPDATE [Page] SET [IsRejected] = 1 WHERE [DocId] IN (SELECT ID FROM [Document] WHERE [BatchId] = @BatchId)";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            BatchId = batchId
                                        },
                                        _context.CurrentTransaction);
        }

        public void UnRejectRecur(Guid batchId)
        {
            const string query = @"
UPDATE [Batch] SET [IsRejected] = 0 WHERE [Id] = @BatchId
UPDATE [Document] SET [IsRejected] = 0 WHERE [BatchId] = @BatchId
UPDATE [Page] SET [IsRejected] = 0 WHERE [DocId] IN (SELECT ID FROM [Document] WHERE [BatchId] = @BatchId)";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            BatchId = batchId
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateLockInfo(Guid id, string lockedBy)
        {
            const string query = @"UPDATE [Batch] SET
                                    [LockedBy] = @LockedBy
                                 WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            LockedBy = lockedBy,
                                            Id = id
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateByLastAccess(Guid id, string accessedBy)
        {
            const string query = @"UPDATE [Batch]
                                   SET   [LastAccessedDate] = @LastAccessedDate,
                                         [LastAccessedBy] = @LastAccessedBy
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            LastAccessedDate = DateTime.Now,
                                            LastAccessedBy = accessedBy,
                                            Id = id
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateByWorkflow(Batch batch)
        {
            const string query = @"UPDATE [Batch]
                                   SET   [WorkflowDefinitionId] = @WorkflowDefinitionId,
                                         [WorkflowInstanceId] = @WorkflowInstanceId,
                                         [BlockingBookmark] = @BlockingBookmark,
                                         [BlockingActivityName] = @BlockingActivityName,
                                         [BlockingActivityDescription] = @BlockingActivityDescription,
                                         [BlockingDate] = @BlockingDate,
                                         [IsProcessing] = @IsProcessing,
                                         [IsCompleted] = @IsCompleted,
                                         [IsRejected] = @IsRejected,
                                         [HasError] = @HasError,
                                         [StatusMsg] = @StatusMsg,
                                         [LockedBy] = @LockedBy
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            WorkflowDefinitionId = batch.WorkflowDefinitionId,
                                            WorkflowInstanceId = batch.WorkflowInstanceId,
                                            BlockingBookmark = batch.BlockingBookmark,
                                            BlockingActivityName = batch.BlockingActivityName,
                                            BlockingActivityDescription = batch.BlockingActivityDescription,
                                            BlockingDate = batch.BlockingDate,
                                            IsProcessing = batch.IsProcessing,
                                            IsCompleted = batch.IsCompleted,
                                            IsRejected = batch.IsRejected,
                                            HasError = batch.HasError,
                                            StatusMsg = batch.StatusMsg,
                                            Id = batch.Id,
                                            LockedBy = batch.LockedBy
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateStatus(Batch batch)
        {
            const string query = @"UPDATE [Batch]
                                   SET   [IsProcessing] = @IsProcessing,
	                                     [IsCompleted]  = @IsCompleted,
	                                     [HasError]	   = @HasError,
	                                     [WorkflowInstanceId] = @WorkflowInstanceId,
                                         [StatusMsg] = @StatusMsg";

            string dynamicQuery = query;
            if (batch.BlockingBookmark != null)
            {
                dynamicQuery += ", [BlockingBookmark] = @BlockingBookmark";
            }

            if (batch.BlockingActivityName != null)
            {
                dynamicQuery += ", [BlockingActivityName] = @BlockingActivityName";
            }

            if (!batch.HasError && !batch.IsProcessing)
            {
                dynamicQuery += ", [BlockingDate] = @BlockingDate";
            }

            if (batch.IsRejected)
            {
                dynamicQuery += ", [IsRejected] = @IsRejected";
            }

            dynamicQuery += " WHERE [Id] = @Id";

            _context.Connection.Execute(dynamicQuery,
                                        new
                                        {
                                            IsProcessing = batch.IsProcessing,
                                            IsCompleted = batch.IsCompleted,
                                            HasError = batch.HasError,
                                            StatusMsg = batch.StatusMsg,
                                            WorkflowInstanceId = batch.WorkflowInstanceId,
                                            BlockingBookmark = batch.BlockingBookmark,
                                            BlockingActivityName = batch.BlockingActivityName,
                                            BlockingDate = batch.BlockingDate,
                                            IsRejected = batch.IsRejected,
                                            Id = batch.Id
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdateWorkflowDefinitionToBatch(Guid batchId, Guid workflowDefinitionId)
        {
            const string query = @"UPDATE [Batch]
                                   SET [WorkflowDefinitionId] = @WorkflowDefinitionId
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            WorkflowDefinitionId = workflowDefinitionId,
                                            Id = batchId
                                        },
                                        _context.CurrentTransaction);
        }

        public List<Batch> GetLockedWorkItem(Guid batchId, string userName)
        {
            const string query = @"SELECT * 
                                   FROM [Batch] 
                                   WHERE [BatchId] = @BatchId AND [LockedBy] = @UserName";
            return _context.Connection.Query<Batch>(query, new
                                                            {
                                                                BatchId = batchId,
                                                                LockedBy = userName
                                                            }, _context.CurrentTransaction).ToList();

        }

        /// <summary>
        /// Check the have the running batch with the batch type id
        /// </summary>
        /// <param name="batchTypeId">Batch type id</param>
        /// <returns>1, if have at least one batch. Else return 0</returns>
        public int ExistsBatchOfBatchType(Guid batchTypeId)
        {
            const string query = @"
SELECT TOP 1 1
FROM Batch
WHERE BatchTypeId = @BatchTypeId
";
            return _context.Connection.Query<int>(query, new
            {
                BatchTypeId = batchTypeId
            }, _context.CurrentTransaction).FirstOrDefault();
        }

        public bool GetRejectStatus(Guid batchId)
        {
            const string query = @"
SELECT Batch.IsRejected
FROM Batch
WHERE ID = @BatchId
";
            return _context.Connection.Query<bool>(query, new
            {
                BatchId = batchId
            }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void UpdateTransactionId(Guid batchId, Guid? transactionId)
        {
            const string query = @"
UPDATE [Batch] 
SET
    [TransactionId] = @TransactionId
WHERE 
    [Id] = @Id";

            _context.Connection.Execute(query,
                                        new
                                        {
                                            Id = batchId,
                                            TransactionId = transactionId
                                        }, _context.CurrentTransaction);
        }

    }
}
