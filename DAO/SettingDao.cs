using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;

namespace Ecm.DAO
{
    public class SettingDao
    {
        private readonly DapperContext _context;

        public SettingDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(SettingObject obj)
        {
            const string query = @"INSERT INTO [Setting] ([Key],[Value])
                                   VALUES (@Key,@Value)
                                   SELECT CAST (SCOPE_IDENTITY() as BIGINT)";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                Key = obj.Key,
                                                Value = obj.Value
                                            },
                                        _context.CurrentTransaction);
        }

        public List<SettingObject> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [Setting]";
            return _context.Connection.Query<SettingObject>(query, null, _context.CurrentTransaction).ToList();
        }

        public void Update(SettingObject obj)
        {
            const string query = @"UPDATE [Setting]
                                   SET [Value] = @Value
                                   WHERE [Key] = @Key";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                Key = obj.Key,
                                                Value = obj.Value
                                            },
                                        _context.CurrentTransaction);
        }

        public SettingObject Get(string key)
        {
            const string query = @"SELECT * 
                                   FROM [Setting] WHERE [Key] = @Key";
            return _context.Connection.Query<SettingObject>(query, new { Key = key }, _context.CurrentTransaction).FirstOrDefault();
        }

    }
}