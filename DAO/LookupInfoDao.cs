using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
{
    public class LookupInfoDao
    {
        private readonly DapperContext _context;

        public LookupInfoDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(LookupInfo obj)
        {
            const string query = @"INSERT INTO [LookupInfo]
                                            ([FieldId],[ServerName],[DataProvider],[Username],[Password],[LookupType],[SqlCommand],[LookupColumn],
                                             [SourceName],[DatabaseName],[MinPrefixLength],[MaxLookupRow],[ConnectionString],[LookupOperator],[ParameterValue])
                                   VALUES   (@FieldId,@ServerName,@DataProvider,@Username,@Password,@LookupType,@SqlCommand,@LookupColumn,
                                             @SourceName,@DatabaseName,@MinPrefixLength,@MaxLookupRow,@ConnectionString,@LookupOperator,@ParameterValue)";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                FieldID = obj.FieldId,
                                                ServerName = obj.ServerName,
                                                DataProvider = obj.DataProvider,
                                                Username = obj.Username,
                                                Password = obj.Password,
                                                LookupType = obj.LookupType,
                                                SqlCommand = obj.SqlCommand,
                                                LookupColumn = obj.LookupColumn,
                                                SourceName = obj.SourceName,
                                                DatabaseName = obj.DatabaseName,
                                                MinPrefixLength = obj.MinPrefixLength,
                                                MaxLookupRow = obj.MaxLookupRow,
                                                ConnectionString = obj.ConnectionString,
                                                LookupOperator = obj.LookupOperator,
                                                ParameterValue = obj.ParameterValue
                                            },
                                        _context.CurrentTransaction);
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [LookupInfo] 
                                   WHERE FieldID = @FieldID";
            _context.Connection.Execute(query, new { FieldID = id }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [LookupInfo]
                                   WHERE FieldID IN (SELECT FieldID 
                                                     FROM FieldMetaData WHERE DocTypeID = @DocTypeID)";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public LookupInfo GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [LookupInfo] 
                                   WHERE FieldID = @FieldID";
            return _context.Connection.Query<LookupInfo>(query, new { FieldID = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(LookupInfo obj)
        {
            const string query = @"UPDATE [LookupInfo]
                                   SET [ServerName] = @ServerName,
                                       [DataProvider] = @DataProvider,
                                       [Username] = @Username,
                                       [Password] = @Password,
                                       [LookupType] = @LookupType,
                                       [SqlCommand] = @SqlCommand,
                                       [LookupColumn] = @LookupColumn,
                                       [SourceName] = @SourceName,
                                       [DatabaseName] = @DatabaseName,
                                       [MinPrefixLength] = @MinPrefixLength,
                                       [MaxLookupRow] = @MaxLookupRow,
                                       [ConnectionString] = @ConnectionString,
                                       [LookupOperator] = @LookupOperator,
                                       [ParameterValue] = @ParameterValue
                                   WHERE FieldID = @FieldID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                ServerName = obj.ServerName,
                                                DataProvider = obj.DataProvider,
                                                Username = obj.Username,
                                                Password = obj.Password,
                                                LookupType = obj.LookupType,
                                                SqlCommand = obj.SqlCommand,
                                                LookupColumn = obj.LookupColumn,
                                                SourceName = obj.SourceName,
                                                DatabaseName = obj.DatabaseName,
                                                MinPrefixLength = obj.MinPrefixLength,
                                                MaxLookupRow = obj.MaxLookupRow,
                                                ConnectionString = obj.ConnectionString,
                                                FieldID = obj.FieldId,
                                                LookupOperator = obj.LookupOperator,
                                                ParameterValue = obj.ParameterValue
                                            },
                                        _context.CurrentTransaction);
        }
    }
}