using System;
using System.Collections.Generic;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDomain;
using Ecm.Utility;
using System.Linq;
using Ecm.CaptureDAO.Context;

namespace Ecm.CaptureCore
{
    public class SettingManager : ManagerBase
    {
        private const string _searchResultPageSize = "SearchResultPageSize";
        private const string _serverWorkingFolder = "ServerWorkingFolder";
        private const string _enabledOcrClient = "EnabledOcrClient";
        private const string _enabledBarcodeClient = "EnabledBarcodeClient";
        private const string _isSaveFileInFolder = "IsSaveFileInFolder";
        private const string _locationSaveFile = "LocationSaveFile";

        public SettingManager(User loginUser) : base(loginUser)
        {
        }

        public Setting GetSettings()
        {
            List<SettingObject> settingObjects;
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
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
                            setting.ServerWorkingFolder = settingObject.Value.ToString();
                            break;
                        case _enabledBarcodeClient:
                            setting.EnabledBarcodeClient = Convert.ToBoolean(settingObject.Value);
                            break;
                        case _enabledOcrClient:
                            setting.EnabledOCRClient = Convert.ToBoolean(settingObject.Value);
                            break;
                        case _isSaveFileInFolder:
                            setting.IsSaveFileInFolder = Convert.ToBoolean(settingObject.Value);
                            break;
                        case _locationSaveFile:
                            setting.LocationSaveFile = settingObject.Value;
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

            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                dataContext.BeginTransaction();
                try
                {
                    SettingDao settingDao = new SettingDao(dataContext);
                    SettingObject settingObject = new SettingObject();

                    //Enabled barcode setting
                    settingObject.Key = _enabledBarcodeClient;
                    settingObject.Value = setting.EnabledBarcodeClient.ToString();
                    
                    if (!CheckSettingExisted(_enabledBarcodeClient, settingDao))
                    {
                        settingDao.Add(settingObject);
                    }
                    else
                    {
                        settingDao.Update(settingObject);
                    }

                    //Enabled OCR setting
                    settingObject = new SettingObject();

                    settingObject.Key = _enabledOcrClient;
                    settingObject.Value = setting.EnabledOCRClient.ToString();

                    if (!CheckSettingExisted(_enabledOcrClient, settingDao))
                    {
                        settingDao.Add(settingObject);
                    }
                    else
                    {
                        settingDao.Update(settingObject);
                    }

                    //Server working folder setting
                    settingObject = new SettingObject();

                    settingObject.Key = _serverWorkingFolder;
                    settingObject.Value = string.IsNullOrEmpty(setting.ServerWorkingFolder) ? string.Empty : setting.ServerWorkingFolder;

                    if (!CheckSettingExisted(_serverWorkingFolder, settingDao))
                    {
                        settingDao.Add(settingObject);
                    }
                    else
                    {
                        settingDao.Update(settingObject);
                    }

                    //Page size setting
                    settingObject = new SettingObject();

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


                    //IsSaveFileInFolder
                    settingObject = new SettingObject();

                    settingObject.Key = _isSaveFileInFolder;
                    settingObject.Value = setting.IsSaveFileInFolder.ToString();

                    if (!CheckSettingExisted(_serverWorkingFolder, settingDao))
                    {
                        settingDao.Add(settingObject);
                    }
                    else
                    {
                        settingDao.Update(settingObject);
                    }

                    //LocationSaveFile
                    if (!string.IsNullOrEmpty(setting.LocationSaveFile))
                    {
                        settingObject = new SettingObject();

                        settingObject.Key = _locationSaveFile;
                        settingObject.Value = setting.LocationSaveFile;

                        if (!CheckSettingExisted(_serverWorkingFolder, settingDao))
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
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
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
