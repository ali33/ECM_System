using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class AnnotationDao
    {
        private readonly DapperContext _context;

        public AnnotationDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(Annotation obj)
        {
            const string query = @"INSERT INTO [Annotation]
                                            ([PageID],[Type],[Height],[Width],[Left],[LineEndAt],[LineStartAt],[LineStyle],[LineWeight],[RotateAngle],
                                             [Top],[LineColor],[Content],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn],[DocID],[DocTypeID])
                                OUTPUT inserted.ID
                                   VALUES
                                            (@PageID,@Type,@Height,@Width,@Left,@LineEndAt,@LineStartAt,@LineStyle,@LineWeight,@RotateAngle,
                                             @Top,@LineColor,@Content,@CreatedBy,@CreatedOn,@ModifiedBy,@ModifiedOn,@DocID,@DocTypeID)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
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
                                                             ModifiedOn = obj.ModifiedOn,
                                                             DocID = obj.DocId,
                                                             DocTypeID = obj.DocTypeId
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [Annotation] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [Annotation] 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public void DeleteByDoc(Guid docId)
        {
            const string query = @"DELETE FROM [Annotation]
                                   WHERE DocID = @DocID";
            _context.Connection.Execute(query, new { DocID = docId }, _context.CurrentTransaction);
        }

        public void DeleteByPage(Guid pageId)
        {
            const string query = @"DELETE FROM [Annotation]
                                   WHERE PageID = @PageID";
            _context.Connection.Execute(query, new { PageId = pageId }, _context.CurrentTransaction);
        }

        public List<Annotation> GetByPage(Guid pageId)
        {
            const string query = @"SELECT * 
                                   FROM [Annotation] 
                                   WHERE PageID = @PageID";
            return _context.Connection.Query<Annotation>(query, new { PageID = pageId }, _context.CurrentTransaction).ToList();
        }
    }
}