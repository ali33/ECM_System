using System;
using System.Collections.Generic;
using System.Security;
using Ecm.DAO;
using Ecm.Domain;
using Ecm.Utility;
using System.Linq;
using Ecm.DAO.Context;

namespace Ecm.Core
{
    public class SettingManager : ManagerBase
    {
        private const string _searchResultPageSize = "SearchResultPageSize";
        private const string _serverWorkingFolder = "ServerWorkingFolder";                
        private const string _isSaveFileInFolder = "IsSaveFileInFolder";
        private const string _locationSaveFile = "LocationSaveFile";
        private const string _luceneFolder = "LuceneFolder";

        public SettingManager(User loginUser) : base(loginUser)
        {
        }

        public Setting GetSettings()
        {
            List<SettingObject> settingObjects;
            using (DapperContext dataContext = new DapperContext(LoginUser.ArchiveConnectionString))
            {
                settingObjects = new SettingDao(dataContext).GetAll();
            }

            var setting = new Setting();

            if (settingObjects != null && settingObjects.Count > 0)
            {
                foreach (var settingObject in settingObjects)
                {
                    switch (settingObject.Key)
                    {
                        case _searchResultPageSize:
                            setting.SearchResultPageSize = Convert.ToInt32(settingObject.Value);
                            break;
                        case _serverWorkingFolder:
                            setting.ServerWorkingFolder = settingObject.Value;
                            break;
                        case _isSaveFileInFolder:
                            setting.IsSaveFileInFolder = Convert.ToBoolean(settingObject.Value);
                            break;
                        case _locationSaveFile:
                            setting.LocationSaveFile = settingObject.Value;
                            break;
                        case _luceneFolder:
                            setting.LuceneFolder = settingObject.Value;
                            break;
                    }
                }
            }

            return setting;
        }

        public void WriteSetting(Setting setting)
        {
            CommonValidator.CheckNull(setting);
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to write settings.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser.ArchiveConnectionString))
            {
                dataContext.BeginTransaction();
                try
                {
                    SettingDao settingDao = new SettingDao(dataContext);
                    List<SettingObject> settingObjects = settingDao.GetAll();
                    foreach (var settingObject in settingObjects)
                    {
                        settingObject.Key = _searchResultPageSize;
                        settingObject.Value = setting.SearchResultPageSize.ToString();

                        if (!CheckSettingExisted(_searchResultPageSize, settingDao))
                        {
                            settingDao.Add(settingObject);
                        }
                        else
                        {
                            settingDao.Update(settingObject);
                        }

                        settingObject.Key = _serverWorkingFolder;
                        settingObject.Value = setting.ServerWorkingFolder;

                        if (!CheckSettingExisted(_serverWorkingFolder, settingDao))
                        {
                            settingDao.Add(settingObject);
                        }
                        else
                        {
                            settingDao.Update(settingObject);
                        }

                        settingObject.Key = _isSaveFileInFolder;
                        settingObject.Value = setting.IsSaveFileInFolder.ToString();

                        if (!CheckSettingExisted(_isSaveFileInFolder, settingDao))
                        {
                            settingDao.Add(settingObject);
                        }
                        else
                        {
                            settingDao.Update(settingObject);
                        }

                        if (!string.IsNullOrEmpty(setting.LocationSaveFile))
                        {
                            settingObject.Key = _locationSaveFile;
                            settingObject.Value = setting.LocationSaveFile;

                            if (!CheckSettingExisted(_locationSaveFile, settingDao))
                            {
                                settingDao.Add(settingObject);
                            }
                            else
                            {
                                settingDao.Update(settingObject);
                            }
                        }

                        settingObject.Key = _luceneFolder;
                        settingObject.Value = setting.LuceneFolder;

                        if (!CheckSettingExisted(_luceneFolder, settingDao))
                        {
                            settingDao.Add(settingObject);
                        }
                        else
                        {
                            settingDao.Update(settingObject);
                        }

                    }

                    ActionLogHelper.AddActionLog("Add setting successfully", LoginUser, ActionName.Save, null, null, dataContext);
                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public void InitSetting(Setting setting)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.ArchiveConnectionString))
            {
                new SettingDao(dataContext).Add(new SettingObject 
                                                    { 
                                                        Key = _searchResultPageSize, 
                                                        Value = setting.SearchResultPageSize.ToString() 
                                                    });
            }
        }

        private bool CheckSettingExisted(string key, SettingDao settingDao)
        {
            SettingObject settingObj = settingDao.Get(key);

            return settingObj != null;
        }
    }
}
