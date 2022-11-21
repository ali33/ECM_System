using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class AnnotationDao
    {
        private readonly DapperContext _context;

        public AnnotationDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Annotation annotation)
        {
            const string query = @"INSERT INTO [Annotation]
                                            ([PageId],[Type],[Height],[Width],[Left],[LineEndAt],[LineStartAt],[LineStyle],[LineWeight],[RotateAngle],
                                             [Top],[LineColor],[Content],[CreatedBy],[CreatedOn],[DocId],[DocTypeId])
                                OUTPUT inserted.ID  
                                   VALUES
                                            (@PageId,@Type,@Height,@Width,@Left,@LineEndAt,@LineStartAt,@LineStyle,@LineWeight,@RotateAngle,
                                             @Top,@LineColor,@Content,@CreatedBy,@CreatedOn,@DocId,@DocTypeId)";
            annotation.Id = _context.Connection.Query<Guid>(query,
                                        new
                                        {
                                            PageId = annotation.PageId,
                                            Type = annotation.Type,
                                            Height = annotation.Height,
                                            Width = annotation.Width,
                                            Left = annotation.Left,
                                            LineEndAt = annotation.LineEndAt,
                                            LineStartAt = annotation.LineStartAt,
                                            LineStyle = annotation.LineStyle,
                                            LineWeight = annotation.LineWeight,
                                            RotateAngle = annotation.RotateAngle,
                                            Top = annotation.Top,
                                            LineColor = annotation.LineColor,
                                            Content = annotation.Content,
                                            CreatedBy = annotation.CreatedBy,
                                            CreatedOn = annotation.CreatedOn,
                                            DocId = annotation.DocId,
                                            DocTypeId = annotation.DocTypeId
                                        },
                                        _context.CurrentTransaction).Single();
        }

        public void Update(Annotation annotation)
        {
            const string query = @"UPDATE [Annotation] SET
                                      [PageId] = @PageId,  [Height] = @Height,[Width] = @Width,[Left] = @Left,
                                      [LineEndAt] = @LineEndAt,[LineStartAt] = @LineStartAt,[LineStyle] = @LineStyle,
                                      [LineWeight] = @LineWeight,[RotateAngle] = @RotateAngle,
                                      [Top] = @Top,[LineColor] = @LineColor,[Content] = @Content,
                                      [ModifiedOn] = @ModifiedOn, [ModifiedBy] = @ModifiedBy
                                  WHERE [ID] = @ID";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            Id = annotation.Id,
                                            PageId = annotation.PageId,
                                            Height = annotation.Height,
                                            Width = annotation.Width,
                                            Left = annotation.Left,
                                            LineEndAt = annotation.LineEndAt,
                                            LineStartAt = annotation.LineStartAt,
                                            LineStyle = annotation.LineStyle,
                                            LineWeight = annotation.LineWeight,
                                            RotateAngle = annotation.RotateAngle,
                                            Top = annotation.Top,
                                            LineColor = annotation.LineColor,
                                            Content = annotation.Content,
                                            ModifiedOn = annotation.ModifiedOn,
                                            ModifiedBy = annotation.ModifiedBy
                                        },
                                        _context.CurrentTransaction);
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Annotation] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(long docTypeId)
        {
            const string query = @"DELETE FROM [Annotation] 
                                   WHERE [DocTypeId] = @DocTypeId";
            _context.Connection.Execute(query, new { DocTypeId = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByDoc(Guid docId)
        {
            const string query = @"DELETE FROM [Annotation]
                                   WHERE [DocId] = @DocId";
            _context.Connection.Execute(query, new { DocId = docId }, _context.CurrentTransaction);
        }

        public void DeleteByPage(Guid pageId)
        {
            const string query = @"DELETE FROM [Annotation]
                                   WHERE [PageId] = @PageId";
            _context.Connection.Execute(query, new { PageId = pageId }, _context.CurrentTransaction);
        }

        public List<Annotation> GetByPage(Guid pageId)
        {
            const string query = @"SELECT * 
                                   FROM [Annotation] 
                                   WHERE [PageId] = @PageId";
            return _context.Connection.Query<Annotation>(query, new { PageId = pageId }, _context.CurrentTransaction).ToList();
        }

        public Annotation GetById(Guid annoId)
        {
            const string query = @"SELECT * 
                                   FROM [Annotation] 
                                   WHERE [Id] = @AnnoId";
            return _context.Connection.Query<Annotation>(query, new { AnnoId = annoId }, _context.CurrentTransaction).SingleOrDefault();
        }

        public void Delete(List<Guid> ids)
        {
            const string query = @"DELETE FROM [Annotation] 
                                   WHERE [Id] IN @Ids";
            _context.Connection.Execute(query, new { Ids = ids }, _context.CurrentTransaction);
        }


    }
}