using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArchiveMVC5.Models;
using ArchiveMVC5.Utility;
using ArchiveMVC5.Models.DataProvider;

namespace ArchiveMVC5.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        [AllowAnonymous]
        public ActionResult Index(LoginModel model)
        {
            Utilities.OpenningDocument = null;
            Utilities.CacheCaptureDocuments = null;
            Utilities.OpenedDocuments = null;
            Utilities.UserName = null;
            Utilities.Password = null;
            Utilities.CaptureDocumentTypes = null;
            Utilities.IsAdmin = false;
            ViewBag.CopyRight = "©" + DateTime.Today.Year + " MIA Solution";
            return View(model==null?new LoginModel():model);
        }

        [AllowAnonymous]
        public ActionResult Action(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SecurityProvider _security = new SecurityProvider();
                    UserModel user = _security.Login(login.UserName, login.Password);
                    Utilities.IsAdmin = user.IsAdmin;
                    Utilities.CaptureDocumentTypes = _security.GetCapturedDocumentTypes();
                    if (user != null)
                    {
                        var imageBinary = user.Picture;
                        if (imageBinary != null)
                        {
                            var keyCache = Guid.NewGuid().ToString();
                            System.Web.HttpContext.Current.Cache.Add(keyCache,
                                imageBinary, null, DateTime.Now.AddMinutes(30),
                                System.Web.Caching.Cache.NoSlidingExpiration,
                                System.Web.Caching.CacheItemPriority.Default, null);
                            Session["KEY_CACHE_USER_PROFILE"] = keyCache;
                        }

                        Utilities.LoginUser = user;
                        Utilities.SetSession(Constant.UserID, user.Id);
                        Utilities.SetSession(Constant.UserName, user.Username);
                        Utilities.SetSession(Constant.Password, user.Password);
                        if (!String.IsNullOrEmpty(login.ReturnUrl))
                            return Redirect(login.ReturnUrl);
                        Utilities.RawPassword = login.Password;
                        return RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_SEARCH);
                    }
                    else
                    {
                        login.Message = Resources.Archive.Login_PasswordFailed;
                        login.IsError = true;
                        return View(Constant.ACTION_INDEX, login);
                    }
                }
                catch (Exception e)
                {
                    login.Message = e.Message;
                    login.IsError = true;
                    return View("Login", login);
                }
            }
            login.Message = Resources.Archive.Login_PasswordFailed;
            login.IsError = true;
            return View("Login", login);
        }

        [AllowAnonymous]
        public String ChangePassword(String oldPassword, String newPassword)
        {
            if (!oldPassword.Equals(Utilities.RawPassword))
            {
                return "Old_password_is_not_match";
            }

            String encryptedNewPass = Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(newPassword);
            String encryptedOldPass = Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(oldPassword);
            SecurityProvider securityProvider = new SecurityProvider();
            securityProvider.Configure(Utilities.UserName, Utilities.Password);
            UserModel changePassUser = securityProvider.ChangePassword(Utilities.UserName, encryptedOldPass, encryptedNewPass);

            Utilities.RawPassword = newPassword;
            Utilities.SetSession(Constant.UserID, changePassUser.Id);
            Utilities.SetSession(Constant.UserName, changePassUser.Username);
            Utilities.SetSession(Constant.Password, changePassUser.Password);

            return "Change_password_success";
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            //SecurityProvider.Configure(Constant.NO_USER, string.Empty);
            Utilities.OpenningDocument = null;
            Utilities.CacheCaptureDocuments = null;
            Utilities.OpenedDocuments = null;
            return RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_LOGIN);
        }

        [HttpGet]
        public JsonResult MobileLogin(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SecurityProvider _security = new SecurityProvider();
                    UserModel user = _security.Login(login.UserName, login.Password);
                    
                    if (user != null)
                    {
                        if (!String.IsNullOrEmpty(login.ReturnUrl))
                            return Json("");
                        return Json(user, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    login.Message = e.Message;
                    login.IsError = true;
                    return Json("");
                }
            }
            login.Message = Resources.Archive.Login_PasswordFailed;
            login.IsError = true;
            return Json("");
        }
    }
}
