using Ecm.CaptureDAO.Context;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.CaptureDAO
{
    public class SearchDao
    {
        private readonly DapperContext _context;

        public SearchDao(DapperContext context)
        {
            _context = context;
        }

        public void CreateTempTable(string tableName, string queryString)
        {
            string query = @"CREATE TABLE {0} {1}";
            query = string.Format(query, tableName, queryString);

            _context.Connection.Execute(query, null, _context.CurrentTransaction);
        }

        public void DropTempTable(string tableName)
        {
            string query = @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
                            DROP TABLE {1}";
            query = string.Format(query, tableName, tableName);

            _context.Connection.Execute(query, null, _context.CurrentTransaction);
        }

        public void InsertDataToTempTable(string tableName, string queryString)
        {
            string query = @"INSERT INTO {0} VALUES({1})";
            query = string.Format(query, tableName, queryString);

            _context.Connection.Execute(query, null, _context.CurrentTransaction);
        }

        //        public List<Guid> GetBatchFromSearch(string tableName, string queryString, int pageIndex)
        //        {
        //            string settingQuery = @"SELECT Cast([Value] as INT) FROM Setting WHERE [Key] = 'SearchResultPageSize'";
        //            int pageSize = _context.Connection.Query<int>(settingQuery, null, _context.CurrentTransaction).FirstOrDefault();
        //            int from = ((int)pageIndex * pageSize) + 1;
        //            int to = ((int)pageIndex + 1) * pageSize;

        //            string query = @"SELECT * FROM (SELECT TOP (@To) b.BatchId, ROW_NUMBER() OVER (ORDER BY b.ID) rowNumber 
        //                            FROM {0} b
        //                            WHERE {1}) as result WHERE rowNumber BETWEEN @From AND @To ORDER BY rowNumber";

        //            query = string.Format(query, tableName, queryString);

        //            return _context.Connection.Query<Guid>(query, new { From = from, To = to }, _context.CurrentTransaction).ToList();
        //        }

        public List<Guid> GetBatchFromSearch(string tableName, string queryString, int pageIndex)
        {
            string settingQuery = @"SELECT Cast([Value] as INT) FROM Setting WHERE [Key] = 'SearchResultPageSize'";
            int pageSize = _context.Connection.Query<int>(settingQuery, null, _context.CurrentTransaction).FirstOrDefault();
            int from = ((int)pageIndex * pageSize) + 1;
            int to = ((int)pageIndex + 1) * pageSize;

            return _context.Connection.Query<Guid>(queryString, new { From = from, To = to }, _context.CurrentTransaction).ToList();

        }

        /// <summary>
        /// Get the total result by page index.
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<Guid> GetBatchFromSearch(string queryString)
        {
            return _context.Connection.Query<Guid>(queryString, null, _context.CurrentTransaction).ToList();
        }
    }
}
