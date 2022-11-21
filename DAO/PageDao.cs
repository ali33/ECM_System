using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class PageDao
    {
        private readonly DapperContext _context;

        public PageDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Page obj)
        {
            const string query = @"INSERT INTO [Page] ([DocId],[PageNumber],[FileExtension],[FileBinary],[FileHeader],[FilePath],[FileHash],[RotateAngle],[Height],[Width],[DocTypeId],[Content],[ContentLanguageCode],[OriginalFileName],[CreatedDate],[CreatedBy])                                   
                             OUTPUT inserted.ID
                                VALUES (@DocId,@PageNumber,@FileExtension,@FileBinary,@FileHeader,@FilePath,@FileHash,@RotateAngle,@Height,@Width,@DocTypeId,@Content,@ContentLanguageCode,@OriginalFileName,@CreatedDate,@CreatedBy)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             obj.DocId,
                                                             obj.PageNumber,
                                                             obj.FileExtension,
                                                             obj.FileBinary,
                                                             obj.FileHeader,
                                                             obj.FilePath,
                                                             obj.FileHash,
                                                             obj.RotateAngle,
                                                             obj.Height,
                                                             obj.Width,
                                                             obj.DocTypeId,
                                                             obj.Content,
                                                             obj.ContentLanguageCode,
                                                             obj.OriginalFileName,
                                                             obj.CreatedDate,
                                                             obj.CreatedBy
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Page] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Page] 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByDoc(Guid docId)
        {
            const string query = @"DELETE FROM [Page]
                                   WHERE DocID = @DocID";
            _context.Connection.Execute(query, new { DocID = docId }, _context.CurrentTransaction);
        }

        public List<Page> GetByDoc(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM [Page] 
                                   WHERE DocID = @DocID";
            return _context.Connection.Query<Page>(query, new { DocID = docId }, _context.CurrentTransaction).ToList();
        }

        /// <summary>
        /// Get page by ID User for delete file
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Page GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [Page] 
                                   WHERE ID = @ID";
            return _context.Connection.Query<Page>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public byte[] GetBinary(Guid pageId)
        {
            const string query = @"SELECT [FileBinary] FROM [Page] WHERE [ID] = @PageID";
            return _context.Connection.Query<byte[]>(query, new { PageID = pageId }, _context.CurrentTransaction).SingleOrDefault();
        }

        public void Update(Page obj)
        {
            const string query = @"UPDATE [Page]
                                   SET [PageNumber] = @PageNumber,
                                       [RotateAngle] = @RotateAngle,
                                       [Height] = @Height,
                                       [Width] = @Width,
                                       [ModifiedDate] = @ModifiedDate,
                                       [ModifiedBy] = @ModifiedBy
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                PageNumber = obj.PageNumber,
                                                RotateAngle = obj.RotateAngle,
                                                Height = obj.Height,
                                                Width = obj.Width,
                                                ID = obj.Id,
                                                ModifiedDate = obj.ModifiedDate,
                                                ModifiedBy = obj.ModifiedBy
                                            },
                                        _context.CurrentTransaction);
        }

        public void UpdateBinary(Page obj)
        {
            const string query = @"UPDATE [Page]
                                   SET [FileBinary] = @FileBinary,
                                       [FileHeader] = @FileHeader,
                                       [FilePath] = @FilePath,
                                       [FileHash] = @FileHash,
                                       [FileExtension] = @FileExtension,
                                       [Content] = @Content,
                                       [ContentLanguageCode] = @ContentLanguageCode,
                                       [OriginalFileName] = @OriginalFileName ,
                                       [ModifiedDate] = @ModifiedDate,
                                       [ModifiedBy] = @ModifiedBy
                                  WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            FileBinary = obj.FileBinary,
                                            FileHeader = obj.FileHeader,
                                            FilePath = obj.FilePath,
                                            FileHash = obj.FileHash,
                                            FileExtension = obj.FileExtension,
                                            Content = obj.Content,
                                            ContentLanguageCode = obj.ContentLanguageCode,
                                            ID = obj.Id,
                                            OriginalFileName = obj.OriginalFileName,
                                            ModifiedDate = obj.ModifiedDate,
                                            ModifiedBy = obj.ModifiedBy
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdatePageContent(Page obj)
        {
            const string query = @"UPDATE [Page]
                                   SET [Content] = @Content,
                                       [ContentLanguageCode] = @ContentLanguageCode,
                                       [ModifiedDate] = @ModifiedDate,
                                       [ModifiedBy] = @ModifiedBy
                                 WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Content = obj.Content,
                                            ContentLanguageCode = obj.ContentLanguageCode,
                                            ID = obj.Id,
                                            ModifiedDate = obj.ModifiedDate,
                                            ModifiedBy = obj.ModifiedBy
                                        },
                                        _context.CurrentTransaction);
        }

    }
}