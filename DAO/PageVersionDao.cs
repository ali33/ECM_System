using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class PageVersionDao
    {
        private readonly DapperContext _context;

        public PageVersionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(PageVersion obj)
        {
            const string query = @"INSERT INTO [PageVersion]
                                            ([DocVersionID],[PageID],[DocID],[PageNumber],[FileExtension],[FileBinary],[FileHeader],[FilePath],[FileHash],[RotateAngle],[Height],[Width])
                                OUTPUT inserted.ID
                                   VALUES   (@DocVersionID,@PageID,@DocID,@PageNumber,@FileExtension,@FileBinary,@FileHeader,@FilePath,@FileHash,@RotateAngle,@Height,@Width)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocVersionID = obj.DocVersionId,
                                                             PageID = obj.PageId,
                                                             DocID = obj.DocId,
                                                             PageNumber = obj.PageNumber,
                                                             FileExtension = obj.FileExtension,
                                                             FileBinary = obj.FileBinary,
                                                             FileHeader = obj.FileHeader,
                                                             FilePath = obj.FilePath,
                                                             FileHash = obj.FileHash,
                                                             RotateAngle = obj.RotateAngle,
                                                             Height = obj.Height,
                                                             Width = obj.Width
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public List<PageVersion> GetByDocVersion(Guid docVersionId)
        {
            const string query = @"SELECT * 
                                   FROM [PageVersion] 
                                   WHERE [DocVersionID] = @DocVersionID";
            return _context.Connection.Query<PageVersion>(query, new { DocVersionID = docVersionId }, _context.CurrentTransaction).ToList();
        }
    }
}