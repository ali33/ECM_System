using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.Context;
using Ecm.SecurityDao.Domain;
using System;

namespace Ecm.SecurityDao
{
    public class LanguageDao
    {
        private readonly DapperContext _context;

        public LanguageDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(PrimaryLanguage language)
        {
            const string query = @"INSERT INTO [Language] ([Name],[Format],[DateFormat],[TimeFormat],[ThousandChar],[DecimalChar])
                                OUTPUT inserted.ID
                                   VALUES (@Name,@Format,@DateFormat,@TimeFormat,@ThousandChar,@DecimalChar)
                                   ";
            language.ID = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         language.Name,
                                                         language.Format,
                                                         language.DateFormat,
                                                         language.TimeFormat,
                                                         language.ThousandChar,
                                                         language.DecimalChar
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

        public List<PrimaryLanguage> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [Language]";
            return _context.Connection.Query<PrimaryLanguage>(query, null, _context.CurrentTransaction).ToList();
        }

        public PrimaryLanguage GetById(Guid id)
        {
            const string query = @"SELECT * 
                                   FROM [Language] 
                                   WHERE Id = @Id";
            return _context.Connection.Query<PrimaryLanguage>(query, new { Id = id }, _context.CurrentTransaction).FirstOrDefault();
        }

        public void Update(PrimaryLanguage language)
        {
            const string query = @"UPDATE [Language] 
                                   SET [Name] = @Name, 
                                       [Format] = @Format ,
                                       [DateFormat] = @DateFormat,
                                       [TimeFormat] = @TimeFormat,
                                       [ThousandChar] = @ThousandChar,
                                       [DecimalChar] = @DecimalChar
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                        {
                                            language.Name,
                                            language.Format
                                        },
                                        _context.CurrentTransaction);
        }

        public void Add(PrimaryLanguage language, Guid langId)
        {
            const string query = @"INSERT INTO [Language] ([Id], [Name],[Format],[DateFormat],[TimeFormat],[ThousandChar],[DecimalChar])
                                   VALUES (@Id, @Name,@Format,@DateFormat,@TimeFormat,@ThousandChar,@DecimalChar)
                                   ";
            language.ID = _context.Connection.Query<Guid>(query,
                                                     new
                                                     {
                                                         language.ID,
                                                         language.Name,
                                                         language.Format,
                                                         language.DateFormat,
                                                         language.TimeFormat,
                                                         language.ThousandChar,
                                                         language.DecimalChar
                                                     },
                                                     _context.CurrentTransaction).Single();
        }

    }
}