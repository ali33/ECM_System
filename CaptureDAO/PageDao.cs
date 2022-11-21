using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class PageDao
    {
        private readonly DapperContext _context;

        public PageDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Page page)
        {
            const string query = @"INSERT INTO [Page] ([DocId],[PageNumber],[FileExtension],[FileBinary],[FileHeader],[FilePath],[FileHash],[RotateAngle],[Height],[Width],[IsRejected],[Content],[ContentLanguageCode],[OriginalFileName])
                                OUTPUT inserted.ID
                                VALUES (@DocId,@PageNumber,@FileExtension,@Binary,@FileHeader,@FilePath,@FileHash,@RotateAngle,@Height,@Width,@IsRejected,@Content,@ContentLanguageCode,@OriginalFileName)";
            page.Id = _context.Connection.Query<Guid>(query,
                                         new
                                         {
                                             DocId = page.DocId,
                                             PageNumber = page.PageNumber,
                                             FileExtension = page.FileExtension,
                                             Binary = page.FileBinary,
                                             FileHeader = page.FileHeader,
                                             FilePath = page.FilePath,
                                             FileHash = page.FileHash,
                                             RotateAngle = page.RotateAngle,
                                             Height = page.Height,
                                             Width = page.Width,
                                             IsRejected = page.IsRejected,
                                             Content = page.Content,
                                             ContentLanguageCode = page.ContentLanguageCode,
                                             OriginalFileName = page.OriginalFileName
                                         },
                                         _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Page] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Page] 
                                   WHERE [DocId] IN (SELECT [Id] FROM [Document] WHERE [DocTypeId] = @DocTypeId)";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByDoc(Guid docId)
        {
            const string query = @"DELETE FROM [Page]
                                   WHERE [DocId] = @DocId";
            _context.Connection.Execute(query, new { DocId = docId }, _context.CurrentTransaction);
        }

        public List<Page> GetByDoc(Guid docId)
        {
            const string query = @"SELECT * 
                                   FROM [Page] 
                                   WHERE DocId = @DocId ORDER BY [PageNumber] ASC";
            return _context.Connection.Query<Page>(query, new { DocId = docId }, _context.CurrentTransaction).ToList();
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
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<Page>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<Page> GetByDocType(Guid docTypeId)
        {
            const string query = @"SELECT FROM [Page] 
                                   WHERE [DocId] IN (SELECT [Id] FROM [Document] WHERE [DocTypeId] = @DocTypeId) ORDER BY [PageNumber] ASC";
            return _context.Connection.Query<Page>(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction).ToList();
        }

        public void Update(Page page)
        {
            // 2014/08/16 - HungLe - Start - Editing add save DocId
            //            const string query = @"UPDATE [Page]
            //                                   SET [PageNumber] = @PageNumber,
            //                                       [RotateAngle] = @RotateAngle,
            //                                       [Height] = @Height,
            //                                       [Width] = @Width,
            //                                       [IsRejected] = @IsRejected
            //                                   WHERE Id = @Id";
            //_context.Connection.Execute(query,
            //                            new
            //                                {
            //                                    PageNumber = page.PageNumber,
            //                                    RotateAngle = page.RotateAngle,
            //                                    Height = page.Height,
            //                                    Width = page.Width,
            //                                    IsRejected = page.IsRejected,
            //                                    Id = page.Id
            //                                },
            //                            _context.CurrentTransaction);

            const string query = @"UPDATE [Page]
                                   SET  [DocId] = @DocId,
                                        [PageNumber] = @PageNumber,
                                        [FileExtension] = @FileExtension,
                                        [FileBinary] = @FileBinary,
                                        [FileHeader] = @FileHeader,
                                        [FilePath] = @FilePath,
                                        [FileHash] = @FileHash,
                                        [RotateAngle] = @RotateAngle,
                                        [Height] = @Height,
                                        [Width] = @Width,
                                        [IsRejected] = @IsRejected,
                                        [Content] = @Content,
                                        [ContentLanguageCode] = @ContentLanguageCode,
                                        [OriginalFileName] = OriginalFileName
                                   WHERE Id = @Id";

            _context.Connection.Execute(query,
                                        new
                                        {
                                            DocId = page.DocId,
                                            PageNumber = page.PageNumber,
                                            FileExtension = page.FileExtension,
                                            FileBinary = page.FileBinary,
                                            FileHeader = page.FileHeader,
                                            FileHash = page.FileHash,
                                            FilePath = page.FilePath,
                                            RotateAngle = page.RotateAngle,
                                            Height = page.Height,
                                            Width = page.Width,
                                            IsRejected = page.IsRejected,
                                            Content = page.Content,
                                            ContentLanguageCode = page.ContentLanguageCode,
                                            OriginalFileName = page.OriginalFileName,
                                            Id = page.Id,
                                        },
                                        _context.CurrentTransaction);
            // 2014/08/16 - HungLe - End - Editing add save DocId
        }

        public void UpdatePageInfo(Page page)
        {
            const string query = @"UPDATE [Page]
                                   SET  [PageNumber] = @PageNumber,
                                        [RotateAngle] = @RotateAngle,
                                        [Height] = @Height,
                                        [Width] = @Width,
                                        [IsRejected] = @IsRejected,
                                        [Content] = @Content
                                   WHERE Id = @Id";

            _context.Connection.Execute(query,
                                        new
                                        {
                                            PageNumber = page.PageNumber,
                                            RotateAngle = page.RotateAngle,
                                            Height = page.Height,
                                            Width = page.Width,
                                            IsRejected = page.IsRejected,
                                            Content = page.Content,
                                            Id = page.Id,
                                        },
                                        _context.CurrentTransaction);
            // 2014/08/16 - HungLe - End - Editing add save DocId
        }

        public void Reject(Page page)
        {
            const string query = @"UPDATE [Page]
                                   SET [IsRejected] = @IsRejected
                                   WHERE Id = @Id";

            _context.Connection.Execute(query,
                                        new
                                        {
                                            IsRejected = page.IsRejected,
                                            Id = page.Id,
                                        },
                                        _context.CurrentTransaction);
            // 2014/08/16 - HungLe - End - Editing add save DocId
        }

        public void UpdateFile(Page page)
        {
            const string query = @"UPDATE [Page]
                                   SET [FilePath] = @FilePath,
                                       [FileBinary] = @Binary,
                                       [FileHeader] = @FileHeader,
                                       [FileHash] = @FileHash,
                                       [FileExtension] = @FileExtension,
                                       [Content] = @Content,
                                       [ContentLanguageCode] = @ContentLanguageCode,
                                       [OriginalFileName] = @OriginalFileName,
                                       [IsRejected] = @IsRejected
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            FilePath = page.FilePath,
                                            Binary = page.FileBinary,
                                            FileHeader = page.FileHeader,
                                            FileHash = page.FileHash,
                                            FileExtension = page.FileExtension,
                                            Content = page.Content,
                                            ContentLanguageCode = page.ContentLanguageCode,
                                            Id = page.Id,
                                            OriginalFileName = page.OriginalFileName,
                                            IsRejected = page.IsRejected
                                        },
                                        _context.CurrentTransaction);
        }

        public void UpdatePageNumber(Page page)
        {
            const string query = @"UPDATE [Page] SET [PageNumber] = @PageNumber WHERE Id = @Id";

            _context.Connection.Execute(query, new
            {
                PageNumber = page.PageNumber,
                Id = page.Id
            },
                            _context.CurrentTransaction);

        }

        /// <summary>
        /// Get list simple information of page by document id.
        /// </summary>
        /// <param name="docId">Id of document.</param>
        /// <returns></returns>
        public List<PageInfoMobile> GetInfoByDoc(Guid docId)
        {
            const string query = @"SELECT [Id], [PageNumber], [RotateAngle], [Width], [Height], [IsRejected], 
                                          [FileExtension]
                                   FROM [Page] 
                                   WHERE DocId = @DocId";
            return _context.Connection.Query<PageInfoMobile>(query, new { DocId = docId },
                                                             _context.CurrentTransaction).ToList();
        }


        public void DeleteRecur(Guid id)
        {
            const string query = @"
DELETE FROM [Annotation] WHERE [PageId] = @Id;
DELETE FROM [Page] WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }


        public void UpdateRejectStatus(Guid pageId, bool isRejected)
        {

            const string query = @"UPDATE [Page]
                                   SET   [IsRejected] = @IsRejected
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            IsRejected = isRejected,

                                            Id = pageId
                                        },
                                        _context.CurrentTransaction);
        }

    }
}