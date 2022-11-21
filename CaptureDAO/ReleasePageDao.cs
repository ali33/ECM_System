using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class ReleasePageDao
    {
        private readonly DapperContext _context;

        public ReleasePageDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertReleasePage(ReleasePage releasePage)
        {
            const string query = @"INSERT INTO [ReleasePage] ([ReleaseDocId],[PageNumber],[FileBinary],[FileHeader],[FileExtension],[FilePath],[FileHash],[RotateAngle],[Height],[Width])
                                OUTPUT inserted.ID  
                                   VALUES (@ReleaseDocId,@PageNumber,@FileBinary,@FileHeader,@FileExtension,@FilePath,@FileHash,@RotateAngle,@Height,@Width)";
            releasePage.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {
                                            ReleaseDocId = releasePage.ReleaseDocId,
                                            PageNumber = releasePage.PageNumber,
                                            FileExtension = releasePage.FileExtension,
                                            FilePath = releasePage.FilePath,
                                            FileHash = releasePage.FileHash,
                                            RotateAngle = releasePage.RotateAngle,
                                            Height = releasePage.Height,
                                            Width = releasePage.Width,
                                            FileBinary = releasePage.FileBinary,
                                            FileHeader = releasePage.FileHeader
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [ReleasePage] 
                                   WHERE [ReleaseDocId] IN (SELECT [Id] FROM [ReleaseDocument] WHERE [DocTypeId] = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public ReleasePage GetById(Guid id)
        {
            const string query = @"SELECT * FROM [ReleasePage] WHERE [ID] = @ID";
            return _context.Connection.Query<ReleasePage>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<ReleasePage> GetByDocument(Guid docId)
        {
            const string query = @"SELECT * FROM [ReleasePage] WHERE [ReleaseDocID] = @DocID";
            return _context.Connection.Query<ReleasePage>(query, new { ReleaseDocID = docId }, _context.CurrentTransaction).ToList();
        }
    }
}
