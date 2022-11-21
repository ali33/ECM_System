using System.Collections.Generic;
using System.Security;
using Ecm.DAO;
using Ecm.Domain;
using Ecm.Utility;
using Ecm.DAO.Context;
using Ecm.SecurityDao;
using System;

namespace Ecm.Core
{
    public class AmbiguousDefinitionManager : ManagerBase
    {
        public AmbiguousDefinitionManager(User loginUser) : base (loginUser)
        {
        }

        public List<AmbiguousDefinition> GetAmbiguousDefinitions(Guid languageId)
        {
            using (Ecm.Context.DapperContext primaryContext = new Ecm.Context.DapperContext())
            {
                var languageDao = new LanguageDao(primaryContext);

                using (DapperContext dataContext = new DapperContext(LoginUser))
                {
                    List<AmbiguousDefinition> ambiguousDefinitions = new AmbiguousDefinitionDao(dataContext).GetByLanguage(languageId);
                    foreach (var definition in ambiguousDefinitions)
                    {
                        definition.Language = GetLanguage(languageDao.GetById(definition.LanguageId));
                    }

                    return ambiguousDefinitions;
                }
            }
        }

        public List<AmbiguousDefinition> GetAllAmbiguousDefinitions()
        {
            using (Ecm.Context.DapperContext primaryContext = new Ecm.Context.DapperContext())
            {
                var languageDao = new LanguageDao(primaryContext);

                using (DapperContext dataContext = new DapperContext(LoginUser))
                {
                    List<AmbiguousDefinition> ambiguousDefinitions = new AmbiguousDefinitionDao(dataContext).GetAll();
                    foreach (var definition in ambiguousDefinitions)
                    {
                        definition.Language = GetLanguage(languageDao.GetById(definition.LanguageId));
                    }

                    return ambiguousDefinitions;
                }
            }
        }

        public void Delete(Guid id)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to delete ambiguous definition.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                dataContext.BeginTransaction();
                try
                {
                    new AmbiguousDefinitionDao(dataContext).Delete(id);
                    ActionLogHelper.AddActionLog("Delete ambiguous definition", LoginUser, ActionName.DeleteAmbiguousDefinition, null, null, dataContext);
                    dataContext.Commit();
                }
                catch (System.Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }  
        }

        public void Save(AmbiguousDefinition ambiguousDefinition)
        {
            CommonValidator.CheckNull(ambiguousDefinition);
            CommonValidator.CheckNull(LoginUser);

            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save ambiguous definition.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                try
                {
                    dataContext.BeginTransaction();
                    if (ambiguousDefinition.Id == Guid.Empty)
                    {
                        new AmbiguousDefinitionDao(dataContext).Add(ambiguousDefinition);
                        ActionLogHelper.AddActionLog("Add new ambiguous definition", LoginUser, ActionName.AddAmbiguousDefinition, null, null, dataContext);
                    }
                    else
                    {
                        new AmbiguousDefinitionDao(dataContext).Update(ambiguousDefinition);
                        ActionLogHelper.AddActionLog("Update ambiguous definition", LoginUser, ActionName.UpdateAmbiguousDefinition, null, null, dataContext);
                    }

                    dataContext.Commit();
                }
                catch (System.Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }
    }
}