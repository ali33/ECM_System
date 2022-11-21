using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureDAO
{
    public class AmbiguousDefinitionDao
    {
        private readonly DapperContext _context;

        public AmbiguousDefinitionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(AmbiguousDefinition obj)
        {
            const string query = @"INSERT INTO [AmbiguousDefinition]([LanguageId],[Text],[Unicode])
                                   VALUES(@LanguageId,@Text,@Unicode)
                                   SELECT CAST (SCOPE_IDENTITY() as BIGINT)";
            obj.ID = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             LanguageId = obj.LanguageID,
                                                             Text = obj.Text,
                                                             Unicode = obj.Unicode
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [AmbiguousDefinition] 
                                   WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = id }, _context.CurrentTransaction);
        }

        public List<AmbiguousDefinition> GetAll()
        {
            const string query = @"SELECT * 
                                   FROM [AmbiguousDefinition]";
            return _context.Connection.Query<AmbiguousDefinition>(query, null, _context.CurrentTransaction).ToList();
        }

        public List<AmbiguousDefinition> GetByLanguage(Guid languageId)
        {
            const string query = @"SELECT * 
                                   FROM [AmbiguousDefinition] 
                                   WHERE [LanguageId] = @LanguageId";
            return _context.Connection.Query<AmbiguousDefinition>(query, new { LanguageId = languageId }, _context.CurrentTransaction).ToList();
        }

        public void Update(AmbiguousDefinition obj)
        {
            const string query = @"UPDATE [AmbiguousDefinition]
                                   SET [LanguageId] = @LanguageId,
                                       [Text] = @Text,
                                       [Unicode] = @Unicode
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                LanguageId = obj.LanguageID,
                                                Text = obj.Text,
                                                Unicode = obj.Unicode,
                                                Id = obj.ID
                                            },
                                        _context.CurrentTransaction);
        }
    }
}