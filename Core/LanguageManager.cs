using System.Collections.Generic;
using System.Security;
//using Ecm.DAO;
using Ecm.Domain;
using Ecm.SecurityDao.Domain;
//using Ecm.DAO.Context;
using Ecm.SecurityDao;
using Ecm.Context;
using System;

namespace Ecm.Core
{
    public class LanguageManager : ManagerBase
    {
        public LanguageManager(Domain.User loginUser) : base (loginUser)
        {
        }

        public Domain.Language GetLanguage(Guid id)
        {
            using (DapperContext dataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(dataContext);
                Ecm.SecurityDao.Domain.PrimaryLanguage primaryLanguage = languageDao.GetById(id);
                Domain.Language language = new Domain.Language()
                {
                    Format = primaryLanguage.Format,
                    Id = primaryLanguage.ID,
                    Name = primaryLanguage.Name
                };

                return language;
            }
        }

        public List<Domain.Language> GetLanguages()
        {
            using (DapperContext dataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(dataContext);
                List<Domain.Language> languages = new List<Domain.Language>();
                List<SecurityDao.Domain.PrimaryLanguage> primaryLanguages = languageDao.GetAll();
                primaryLanguages.ForEach(p => languages.Add(GetLanguage(p.ID)));

                return languages;
            }
        }

        public void Save(Domain.Language language)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save language.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(dataContext);
                SecurityDao.Domain.PrimaryLanguage primaryLanguage = new SecurityDao.Domain.PrimaryLanguage()
                {
                    Format = language.Format,
                    ID = language.Id,
                    Name = language.Name
                };

                if (language.Id == Guid.Empty)
                {
                    languageDao.Add(primaryLanguage);
                }
                else
                {
                    languageDao.Update(primaryLanguage);
                }

                using (DAO.Context.DapperContext context = new DAO.Context.DapperContext(LoginUser.ArchiveConnectionString))
                {
                    ActionLogHelper.AddActionLog("Save language " + primaryLanguage.Name + " successfully", LoginUser, ActionName.Save, null, null, context);
                }
            }
        }
    }
}