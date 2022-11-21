using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Text;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class OutlookPictureDao
    {
        private readonly DapperContext _context;

        public OutlookPictureDao(DapperContext context)
        {
            _context = context;
        }

        public List<OutlookPicture> GetPictures(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM [OutlookPictures] 
                                   WHERE DocID = @DocID";
            return _context.Connection.Query<OutlookPicture>(query, new { DocID = docId }, _context.CurrentTransaction).ToList();

        }

        public void InsertPicture(OutlookPicture pics)
        {
            const string query = @"INSERT INTO [OutlookPictures] 
                                          ([DocID],[FileName],[FileBinary])
                                OUTPUT inserted.ID
                                   VALUES (@DocID,@FileName,@FileBinary)";
            pics.Id = _context.Connection.Query<Guid>(query, new
                                        {                                            
                                            DocID = pics.DocId,
                                            FileName = pics.FileName,
                                            FileBinary = pics.FileBinary
                                        }, _context.CurrentTransaction).Single();
        }

        public void DeletePicture(Guid docId)
        {
            const string query = @"DELETE FROM OutlookPictures WHERE [DocID] = @DocID";
            _context.Connection.Execute(query, new { DocID = docId }, _context.CurrentTransaction);
        }
    }
}
