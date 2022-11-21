using System.Collections.Generic;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDomain;
using Ecm.Context;
using Ecm.SecurityDao;
using System;

namespace Ecm.CaptureCore
{
    public class LanguageManager : ManagerBase
    {
        public LanguageManager(User loginUser) : base (loginUser)
        {
        }

        public Language GetLanguage(Guid id)
        {
            using (DapperContext dataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(dataContext);
                return GetLanguage(languageDao.GetById(id));
            }
        }

        public List<Language> GetLanguages()
        {
            using (DapperContext dataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(dataContext);
                return GetLanguages(languageDao.GetAll());
            }
        }

        public void Save(Language language)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save language.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext())
            {
                LanguageDao languageDao = new LanguageDao(dataContext);
                

                if (language.Id == Guid.Empty)
                {
                    languageDao.Add(GetPrimaryLanguage(language));
                }
                else
                {
                    languageDao.Update(GetPrimaryLanguage(language));
                }

                ActionLogHelper.AddActionLog("Save language " + language.Name + " successfully", LoginUser, ActionName.Save, null, Guid.Empty, new CaptureDAO.Context.DapperContext(LoginUser.CaptureConnectionString));
            }
        }
    }
}