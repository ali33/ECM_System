using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class AnnotationVersionDao
    {
        private readonly DapperContext _context;

        public AnnotationVersionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(AnnotationVersion obj)
        {
            const string query = @"INSERT INTO [AnnotationVersion]
                                        ([PageVersionID],[AnnotationID],[PageID],[Type],[Height],[Width],[Left],[LineEndAt],[LineStartAt],[LineStyle],[LineWeight],
                                         [RotateAngle],[Top],[LineColor],[Content],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
                                    OUTPUT inserted.ID
                                   VALUES
                                        (@PageVersionID,@AnnotationID,@PageID,@Type,@Height,@Width,@Left,@LineEndAt,@LineStartAt,@LineStyle,@LineWeight,
                                         @RotateAngle,@Top,@LineColor,@Content,@CreatedBy,@CreatedOn,@ModifiedBy,@ModifiedOn)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             PageVersionID = obj.PageVersionId,
                                                             AnnotationID = obj.AnnotationId,
                                                             PageID = obj.PageId,
                                                             Type = obj.Type,
                                                             Height = obj.Height,
                                                             Width = obj.Width,
                                                             Left = obj.Left,
                                                             LineEndAt = obj.LineEndAt,
                                                             LineStartAt = obj.LineStartAt,
                                                             LineStyle = obj.LineStyle,
                                                             LineWeight = obj.LineWeight,
                                                             RotateAngle = obj.RotateAngle,
                                                             Top = obj.Top,
                                                             LineColor = obj.LineColor,
                                                             Content = obj.Content,
                                                             CreatedBy = obj.CreatedBy,
                                                             CreatedOn = obj.CreatedOn,
                                                             ModifiedBy = obj.ModifiedBy,
                                                             ModifiedOn = obj.ModifiedOn
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [AnnotationVersion] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id });
        }

        public List<AnnotationVersion> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [AnnotationVersion]";
            return _context.Connection.Query<AnnotationVersion>(query).ToList();
        }

        public AnnotationVersion GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [AnnotationVersion] 
                                   WHERE ID = @ID";
            return _context.Connection.Query<AnnotationVersion>(query, new { ID = id }).FirstOrDefault();
        }

        public List<AnnotationVersion> GetByPageVersionId(Guid pageVersionId)
        {
            const string query = @"SELECT * FROM [AnnotationVersion] WHERE PageVersionId = @PageVersionId";
            return _context.Connection.Query<AnnotationVersion>(query, new { PageVersionID = pageVersionId }, _context.CurrentTransaction).ToList();
        }

        public void Update(AnnotationVersion obj)
        {
            const string query = @"UPDATE [Annotation]
                                   SET [PageID] = @PageID,
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
                                       [CreatedOn] = CreatedOn,
                                       [ModifiedBy] = @ModifiedBy,
                                       [ModifiedOn] = @ModifiedOn,
                                       [PageVersionID] = @PageVersionID,
                                       [AnnotationID] = @AnnotationID
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new
                                                {
                                                    PageVersionID = obj.PageVersionId,
                                                    AnnotationID = obj.AnnotationId,
                                                    Type = obj.Type,
                                                    Height = obj.Height,
                                                    Width = obj.Width,
                                                    Left = obj.Left,
                                                    LineEndAt = obj.LineEndAt,
                                                    LineStartAt = obj.LineStartAt,
                                                    LineStyle = obj.LineStyle,
                                                    LineWeight = obj.LineWeight,
                                                    RotateAngle = obj.RotateAngle,
                                                    Top = obj.Top,
                                                    LineColor = obj.LineColor,
                                                    Content = obj.Content,
                                                    CreatedBy = obj.CreatedBy,
                                                    CreatedOn = obj.CreatedOn,
                                                    ModifiedBy = obj.ModifiedBy,
                                                    ModifiedOn = obj.ModifiedOn,
                                                    ID = obj.Id
                                                }, _context.CurrentTransaction);
        }

    }
}