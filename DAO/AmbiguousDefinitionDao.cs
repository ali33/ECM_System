using System.Collections.Generic;
using System.Linq;
using Dapper;
using Ecm.DAO.Context;
using Ecm.Domain;
using System;

namespace Ecm.DAO
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
                            OUTPUT inserted.ID
                                   VALUES(@LanguageId,@Text,@Unicode)";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             LanguageID = obj.LanguageId,
                                                             Text = obj.Text,
                                                             Unicode = obj.Unicode
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [AmbiguousDefinition] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
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
                                   WHERE [LanguageID] = @LanguageID";
            return _context.Connection.Query<AmbiguousDefinition>(query, new { LanguageID = languageId }, _context.CurrentTransaction).ToList();
        }

        public void Update(AmbiguousDefinition obj)
        {
            const string query = @"UPDATE [AmbiguousDefinition]
                                   SET [LanguageID] = @LanguageID,
                                       [Text] = @Text,
                                       [Unicode] = @Unicode
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                LanguageID = obj.LanguageId,
                                                Text = obj.Text,
                                                Unicode = obj.Unicode,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }
    }
}