using CaptureMVC.DataProvider;
using CaptureMVC.Models;
using CaptureMVC.Utility;
using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;

namespace CaptureMVC.Controllers
{
    public class LoginController : BaseController
    {
        // GET: /Login/
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Index(LoginModel model)
        {
            Utilities.UserName = null;
            Utilities.Password = null;
            Utilities.IsAdmin = false;
            return View(model == null ? new LoginModel() : model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Action(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SecurityProvider _security = new SecurityProvider();
                    var user = _security.Login(login.UserName, login.Password);

                    if (user != null)
                    {
                        Utilities.IsAdmin = user.IsAdmin;

                        var imageBinary = user.Photo;
                        if (imageBinary != null)
                        {
                            var keyCache = Guid.NewGuid().ToString();
                            System.Web.HttpContext.Current.Cache.Add(keyCache,
                                imageBinary, null, DateTime.Now.AddMinutes(30),
                                System.Web.Caching.Cache.NoSlidingExpiration,
                                System.Web.Caching.CacheItemPriority.Default, null);
                            Session["KEY_CACHE_USER_PROFILE"] = keyCache;
                        }

                        Utilities.UserName = user.UserName;
                        Utilities.Password = user.Password;
                        Utilities.RawPassword = login.Password;
                        Utilities.UserId = user.Id;

                        // Get language
                        var languageProvider = new LanguageProvider();
                        if (user.LanguageId.HasValue)
                        {
                            var language = languageProvider.GetLanguage(user.LanguageId.Value);
                            Utilities.Language = language;
                        }

                        // Get server setting for login
                        var settingProvider = new SettingsProvider();
                        Utilities.Settings = settingProvider.GetSettings();

                        if (!String.IsNullOrEmpty(login.ReturnUrl))
                        {
                            return Redirect(login.ReturnUrl);
                        }

                        return RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_SEARCH);
                    }
                    else
                    {
                        login.Message = Resources.CaptureResources.Login_PasswordFailed;
                        login.IsError = true;
                        return View(Constant.ACTION_INDEX, login);
                    }
                }
                catch (Exception e)
                {
                    login.Message = e.Message;
                    login.IsError = true;
                    return View(Constant.ACTION_INDEX, login);
                }
            }

            login.Message = Resources.CaptureResources.Login_PasswordFailed;
            login.IsError = true;
            return View(Constant.ACTION_INDEX, login);
        }

        [AllowAnonymous]
        public String ChangePassword(String oldPassword, String newPassword)
        {
            if (!oldPassword.Equals(Utilities.RawPassword))
            {
                return "Old_password_is_not_match";
            }

            String encryptedNewPass = CryptographyHelper.EncryptUsingSymmetricAlgorithm(newPassword);
            String encryptedOldPass = CryptographyHelper.EncryptUsingSymmetricAlgorithm(oldPassword);
            SecurityProvider securityProvider = new SecurityProvider();

            var changePassUser = securityProvider.ChangePassword(Utilities.UserName, encryptedOldPass, encryptedNewPass);

            Utilities.UserName = changePassUser.UserName;
            Utilities.Password = changePassUser.Password;
            Utilities.RawPassword = newPassword;
            //Utilities.SetSession(Constant.UserID, changePassUser.Id);

            return "Change_password_success";
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Logout()
        {
            // Delete temp file
            try
            {
                var tempFiles = Session[Constant.SESSION_TEMP_FILE] as List<string>;
                var fileFoder = System.Web.Hosting.HostingEnvironment.MapPath("~/" + Constant.APP_KEY_FOLDER_TEMP_FILES);

                foreach (var file in tempFiles)
                {
                    System.IO.File.Delete(fileFoder + "/" + file);
                }
            }
            catch (Exception ex)
            {
                // Do nothing
            }

            // Unlock batch
            try
            {
                // Get opened batches from session
                var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;

                // Get list batch id to unlock
                var unlockBatchIds = new List<Guid>();
                ViewBatchModel openedBatch;
                for (int i = 0; i < openedBatches.Count; i++)
                {
                    openedBatch = openedBatches[i] as ViewBatchModel;
                    unlockBatchIds.Add(openedBatch.Id);
                }

                // Unlock batches
                var workItemProvider = new WorkItemProvider();
                workItemProvider.UnLockWorkItems(unlockBatchIds);
            }
            catch (Exception ex)
            {
                // Do nothing
            }

            Session[Constant.SESSION_OPENED_BATCHES] = new OrderedDictionary();
            Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] = Guid.Empty;
            Session[Constant.SESSION_TEMP_FILE] = new List<string>();

            return RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_LOGIN);
        }
    }
}
