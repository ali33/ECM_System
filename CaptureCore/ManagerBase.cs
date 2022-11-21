using System.Linq;
using Ecm.CaptureDomain;
using Ecm.Utility.ProxyHelper;
using Ecm.SecurityDao.Domain;
using System.Collections.Generic;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using System;
using System.IO;

namespace Ecm.CaptureCore
{
    public class ManagerBase
    {
        public User LoginUser { get; private set; }

        public ManagerBase(User user)
        {
            LoginUser = user;
        }

        public ManagerBase()
        {
        }

        //public ClientChannel<ICaptureIndexer> GetLucenceCaptureClientChannel()
        //{
        //    return ChannelManager<ICaptureIndexer>.Instance.GetChannel(LUCENE_SERVICE_ENDPOINT, "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
        //}

        public User GetUser(PrimaryUser primaryUser)
        {
            if (primaryUser == null)
            {
                return null;
            }

            return new User()
            {
                ArchiveConnectionString = primaryUser.ArchiveConnectionString,
                CaptureConnectionString = primaryUser.CaptureConnectionString,
                ClientHost = primaryUser.ClientHost,
                Email = primaryUser.Email,
                FullName = primaryUser.FullName,
                Id = primaryUser.Id,
                IsAdmin = primaryUser.IsAdmin,
                Language = GetLanguage(primaryUser.Language),
                LanguageId = primaryUser.LanguageId,
                Password = primaryUser.Password,
                Photo = primaryUser.Photo,
                Type = primaryUser.Type,
                UserGroups = GetUserGroups(primaryUser.UserGroups),
                UserName = primaryUser.UserName
            };
        }

        public List<User> GetUsers(List<PrimaryUser> primaryUsers)
        {
            List<User> users = new List<User>();
            primaryUsers.ForEach(p => users.Add(GetUser(p)));
            return users;
        }

        public UserGroup GetUserGroup(PrimaryUserGroup primaryGroup)
        {
            if (primaryGroup == null)
            {
                return null;
            }

            return new UserGroup()
            {
                Id = primaryGroup.Id,
                Name = primaryGroup.Name,
                Type = primaryGroup.Type,
                Users = GetUsers(primaryGroup.Users)
            };
        }

        public List<UserGroup> GetUserGroups(List<PrimaryUserGroup> primaryGroups)
        {
            List<UserGroup> userGroups = new List<UserGroup>();
            primaryGroups.ForEach(p => userGroups.Add(GetUserGroup(p)));
            return userGroups;
        }

        public Language GetLanguage(PrimaryLanguage primaryLanguage)
        {
            if (primaryLanguage == null)
            {
                return null;
            }

            return new Language()
            {
                Name = primaryLanguage.Name,
                Id = primaryLanguage.ID,
                Format = primaryLanguage.Format,
                DateFormat = primaryLanguage.DateFormat,
                TimeFormat = primaryLanguage.TimeFormat,
                ThousandChar = primaryLanguage.ThousandChar,
                DecimalChar = primaryLanguage.DecimalChar
            };
        }

        public List<Language> GetLanguages(List<PrimaryLanguage> primaryLanguages)
        {
            List<Language> languages = new List<Language>();
            primaryLanguages.ForEach(p => languages.Add(GetLanguage(p)));
            return languages;
        }

        public PrimaryUser GetPrimaryUser(User user)
        {
            if (user == null)
            {
                return null;
            }
            return new PrimaryUser()
            {
                CaptureConnectionString = user.CaptureConnectionString,
                ArchiveConnectionString = user.ArchiveConnectionString,
                ClientHost = user.ClientHost,
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                IsAdmin = user.IsAdmin,
                Language = GetPrimaryLanguage(user.Language),
                LanguageId = user.LanguageId,
                Password = user.Password,
                Photo = user.Photo,
                Type = user.Type,
                UserGroups = GetPrimaryUserGroups(user.UserGroups),
                UserName = user.UserName
            };
        }

        public List<PrimaryUser> GetPrimaryUsers(List<User> users)
        {
            List<PrimaryUser> primaryUsers = new List<PrimaryUser>();
            users.ForEach(p => primaryUsers.Add(GetPrimaryUser(p)));
            return primaryUsers;
        }

        public PrimaryUserGroup GetPrimaryUserGroup(UserGroup userGroup)
        {
            if (userGroup == null)
            {
                return null;
            }

            return new PrimaryUserGroup()
            {
                Id = userGroup.Id,
                Name = userGroup.Name,
                Type = userGroup.Type,
                Users = GetPrimaryUsers(userGroup.Users)
            };
        }

        public List<PrimaryUserGroup> GetPrimaryUserGroups(List<UserGroup> userGroups)
        {
            List<PrimaryUserGroup> primaryUserGroups = new List<PrimaryUserGroup>();
            userGroups.ForEach(p => primaryUserGroups.Add(GetPrimaryUserGroup(p)));
            return primaryUserGroups;
        }

        public PrimaryLanguage GetPrimaryLanguage(Language language)
        {
            if (language == null)
            {
                return null;
            }
            return new PrimaryLanguage()
            {
                Name = language.Name,
                ID = language.Id,
                Format = language.Format
            };
        }

        public List<PrimaryLanguage> GetPrimaryLanguages(List<Language> languages)
        {
            List<PrimaryLanguage> primaryLanguages = new List<PrimaryLanguage>();
            languages.ForEach(p => primaryLanguages.Add(GetPrimaryLanguage(p)));
            return primaryLanguages;
        }

        public string GetServerWorkingFolder()
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                SettingDao settingDao = new SettingDao(context);
                List<SettingObject> settings = settingDao.GetAll();

                string root = settings.FirstOrDefault(p => p.Key == "ServerWorkingFolder").Value;

                if (root != null)
                {
                    return root + @"\" + LoginUser.UserName;
                }
                else
                {
                    root = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot") + string.Empty, "Temp", "ECM Solutions", "eDocPro", LoginUser.UserName);
                    return root;
                }
            }
        }

        private const string LUCENE_SERVICE_ENDPOINT = "LuceneCaptureEndPoint";
    }
}
