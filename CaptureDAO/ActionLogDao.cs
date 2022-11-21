using System;
using System.Linq;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Dapper;
using System.Collections.Generic;

namespace Ecm.CaptureDAO
{
    public class ActionLogDao
    {
        private readonly DapperContext _context;

        public ActionLogDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(ActionLog obj)
        {
            const string query = @"INSERT INTO [ActionLog]([ID],[UserName],[IpAddress],[LoggedDate],[ActionName],[Message],[ObjectType],[ObjectId])
                                OUTPUT inserted.ID
                                   VALUES (@Id,@UserName,@IpAddress,@LoggedDate,@ActionName,@Message,@ObjectType,@ObjectId)";
            obj.Id = _context.Connection.Query<Guid>(query, new
                                                   {
                                                       Id = obj.Id,
                                                       Username = obj.Username,
                                                       IpAddress = obj.IpAddress,
                                                       LoggedDate = obj.LoggedDate,
                                                       ActionName = obj.ActionName,
                                                       Message = obj.Message,
                                                       ObjectType = obj.ObjectType,
                                                       ObjectId = obj.ObjectId
                                                   },
                                        _context.CurrentTransaction).SingleOrDefault();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE 
                                   FROM [ActionLog] 
                                   WHERE [Id] = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public ActionLog GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [ActionLog] 
                                   WHERE [Id] = @Id";
            return _context.Connection.Query<ActionLog>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<ActionLog> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM ActionLog";
            return _context.Connection.Query<ActionLog>(query, null, _context.CurrentTransaction).ToList();
        }

        public List<ActionLog> SearchActionLogs(string expresstion, int index, int pageSize, out long totalRow)
        {
            int from = (index * pageSize) + 1;
            int to = (index + 1) * pageSize;

            string query = @"SELECT * 
                             FROM (SELECT TOP (@To) tbl.*, ROW_NUMBER() OVER (ORDER BY ID) rowNumber
                             FROM ActionLog as tbl WHERE " + expresstion + ") as al WHERE rowNumber BETWEEN @From AND @To ORDER BY rowNumber";

            string totalQuery = @"SELECT CAST(COUNT(Id) as BIGINT)
                                        FROM ActionLog WHERE " + expresstion;

            totalRow = _context.Connection.Query<long>(totalQuery).First();
            return _context.Connection.Query<ActionLog>(query, new { From = from, To = to }, _context.CurrentTransaction).ToList();
        }

        public List<ActionLog> SearchActionLogs(string expresstion)
        {
            string query = @"SELECT *
                             FROM ActionLog where " + expresstion;
            return _context.Connection.Query<ActionLog>(query, null, _context.CurrentTransaction).ToList();
        }

        public List<ActionLog> GetActionLogs(int index, int pageSize, out long totalRow)
        {
            int from = (index * pageSize) + 1;
            int to = (index + 1) * pageSize;

            const string query = @"SELECT *
                                   FROM (SELECT TOP (@To) tbl.*, ROW_NUMBER() OVER (ORDER BY ID) rowNumber
                                         FROM ActionLog as tbl) as seq
                                   WHERE seq.rowNumber BETWEEN @From AND @To
                                   ORDER BY seq.rowNumber";

            const string totalQuery = @"SELECT CAST(COUNT(Id) as BIGINT)
                                        FROM ActionLog";

            totalRow = _context.Connection.Query<long>(totalQuery).First();
            return _context.Connection.Query<ActionLog>(query, new { From = from, To = to }, _context.CurrentTransaction).ToList();
        }
    }
}