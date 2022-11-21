using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureDAO
{
    public class ReleaseAnnotationDao
    {
        private readonly DapperContext _context;

        public ReleaseAnnotationDao(DapperContext context)
        {
            _context = context;
        }

        public void InsertReleaseAnnotation(ReleaseAnnotation annotation)
        {
            const string query = @"INSERT INTO [ReleaseAnnotation]
                                            ([ReleasePageId],[Type],[Height],[Width],[Left],[LineEndAt],[LineStartAt],[LineStyle],[LineWeight],[RotateAngle],
                                             [Top],[LineColor],[Content],[CreatedBy],[CreatedOn])
                                OUTPUT inserted.ID  
                                   VALUES
                                            (@ReleasePageId,@Type,@Height,@Width,@Left,@LineEndAt,@LineStartAt,@LineStyle,
                                             @LineWeight,@RotateAngle,@Top,@LineColor,@Content,@CreatedBy,@CreatedOn)";
            annotation.Id = _context.Connection.Query<Guid>(query, new
             {
                 ReleasePageId = annotation.ReleasePageId,
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
                 CreatedOn = annotation.CreatedOn
             }, _context.CurrentTransaction).Single();
        }

        public void UpdateReleaseAnnotation(ReleaseAnnotation annotation)
        {
            const string query = @"UPDATE [ReleaseAnnotation] SET
                                          [ReleasePageId] = @ReleasePageId,
                                          [Type] = @Type,
                                          [Height] = @Height,
                                          [Width] = @Width,
                                          [Left] = @Left,
                                          [LineEndAt] = @LineEndAt,
                                          [LineStartAt] = @LineStartAt,
                                          [LineStyle] = @LineStyle,
                                          [LineWeight] = @LineWeight,
                                          [RotateAngle] = @RotateAngle,
                                          [Top] = @Top,
                                          [LineColor] = @LineColor,
                                          [Content] = @Content,
                                          [CreatedBy] = @CreatedBy,
                                          [CreatedOn] = @CreatedOn)
                                   WHERE [ID] = @ID";
            _context.Connection.Execute(query, new
            {
                Id = annotation.Id,
                ReleasePageId = annotation.ReleasePageId,
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
                CreatedOn = annotation.CreatedOn
            }, _context.CurrentTransaction);
        }

        public void DeleteReleaseAnnotation(Guid id)
        {
            const string query = @"DELETE FROM [ReleaseAnnotation] WHERE [ID] =@ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public ReleaseAnnotation GetById(Guid id)
        {
            const string query = @"SELECT * FROM [ReleaseAnnotation] WHERE [ID] = @ID";
            return _context.Connection.Query<ReleaseAnnotation>(query, new { ID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<ReleaseAnnotation> GetByReleasePage(Guid pageId)
        {
            const string query = @"SELECT * FROM [ReleaseAnnotation] WHERE [ReleasePageID] = @ReleasePageID";
            return _context.Connection.Query<ReleaseAnnotation>(query, new { ReleasePageID = pageId }, _context.CurrentTransaction).ToList();
        }
    }
}
