using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CaptureMVC.Models;
using CaptureMVC.Utility;
using CaptureMVC.DataProvider;
using Ecm.CaptureDomain;
using log4net;
using System.IO;
using Ecm.Utility;
using System.Collections;


namespace CaptureMVC.Controllers
{
    public class CaptureAdminController : Controller
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: /CaptureAdmin/

        public ActionResult Index()
        {
            if (!Utilities.IsAdmin)
            {
                return RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_SEARCH);
            }

            //Show list document type icon

            String sourcesIcon = "~/Resources/" + Constant.DOCUMENT_TYPE_ICON_FOLDER;
            bool isExist = System.IO.Directory.Exists(Server.MapPath(sourcesIcon));
            if (isExist)
                ViewData["DocumentTypeIcon"] = Directory.EnumerateFiles(Server.MapPath(sourcesIcon))
                                                      .Select(image => sourcesIcon + "/" + Path.GetFileName(image));
            else
                ViewData["DocumentTypeIcon"] = null;
            //end
            return View();
        }

        #region Bacth Types Action Result

        // display batch type list 
        public ActionResult ShowListBatchType()
        {
            CaptureAdminModel model = new CaptureAdminModel();
            BatchTypeProvider _BatchTypeProvider = new BatchTypeProvider();
            IList<BatchType> _BatchTypeList = new List<BatchType>();
            _BatchTypeList = _BatchTypeProvider.GetBatchTypes();

            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var cPri = System.Web.Caching.CacheItemPriority.Normal;
            string id = "";
            string cacheKey = "CaptureControler_GetBatchType";
            object returnList = System.Web.HttpContext.Current.Cache[cacheKey];
            List<BatchTypeResult> ListBatchTypeCache = new List<BatchTypeResult>();
            if (returnList != null)
            {
                ListBatchTypeCache = (List<BatchTypeResult>)returnList;
            }
            bool isUpdateCache = false;
            foreach (BatchType batch in _BatchTypeList)
            {
                if (ListBatchTypeCache.Where(p => p.BatchType.Id == batch.Id).Count() > 0)
                {
                    id = ListBatchTypeCache.Where(p => p.BatchType.Id == batch.Id).FirstOrDefault().IconKey;
                }
                else
                {
                    isUpdateCache = true; //updates cache ds BatchType
                    id = Guid.NewGuid().ToString();
                    if (batch.Icon != null)
                    {
                        System.Web.HttpContext.Current.Cache.Add(id, batch.Icon, null, cTime, cExp, cPri, null);
                        ListBatchTypeCache.Add(new BatchTypeResult
                        {
                            BatchType = new BatchType
                            {
                                Name = batch.Name,
                                Id = batch.Id,
                                Fields = batch.Fields
                            },
                            IconKey = id
                        });
                    }
                }
                model.ListBatchType.Add(id, batch);
            }
            return PartialView("ShowListBatchType", model);
        }

        // Display BatchTypeProperties 
        public ActionResult BatchTypeProperties(CaptureAdminModel model)
        {
            try
            {
                CaptureAdminModel captureModel = new CaptureAdminModel();
                BatchType batchModel = new BatchType();
                BatchTypeProvider batchTypeProvider = new BatchTypeProvider();

                if (model.BatchId == Guid.Empty)
                {
                    batchModel = null;
                }
                else 
                { 
                    batchModel = batchTypeProvider.GetBatchType(model.BatchId);
                }    
          
                captureModel.CaptureBatchTypeModel = batchModel;

                String sourcesIcon = "~/Resources/" + Constant.DOCUMENT_TYPE_ICON_FOLDER;
                var keyCache = Guid.NewGuid().ToString();
                var dockeyCache = Guid.NewGuid().ToString();
                if (model.BatchId != Guid.Empty)
                {
                    if (batchModel.Icon != null)
                    {
                        System.Web.HttpContext.Current.Cache.Add(
                           keyCache,
                           batchModel.Icon,
                           null,
                           DateTime.Now.AddMinutes(15),
                           System.Web.Caching.Cache.NoSlidingExpiration,
                           System.Web.Caching.CacheItemPriority.Default,
                           null
                            );
                    }
                }
                else
                {

                    byte[] defautIcon = System.IO.File.ReadAllBytes(Server.MapPath(sourcesIcon + "/appbar.page.text.png"));
                    System.Web.HttpContext.Current.Cache.Add(
                       keyCache,
                       defautIcon,
                       null,
                       DateTime.Now.AddMinutes(15),
                       System.Web.Caching.Cache.NoSlidingExpiration,
                       System.Web.Caching.CacheItemPriority.Default,
                       null
                        );
                }
                ViewData["KEY_CACHE_ICON"] = keyCache;

                if (batchModel.DocTypes != null) {

                    foreach (var docItem in batchModel.DocTypes)
                    {
                        if (docItem.Icon != null)
                        {
                          System.Web.HttpContext.Current.Cache.Add(
                          dockeyCache,
                          docItem.Icon,
                          null,
                          DateTime.Now.AddMinutes(15),
                          System.Web.Caching.Cache.NoSlidingExpiration,
                          System.Web.Caching.CacheItemPriority.Default,
                          null
                           );
                        }
                        else
                        {

                            byte[] defautIcon = System.IO.File.ReadAllBytes(Server.MapPath(sourcesIcon + "/appbar.page.text.png"));
                            System.Web.HttpContext.Current.Cache.Add(
                               dockeyCache,
                               defautIcon,
                               null,
                               DateTime.Now.AddMinutes(15),
                               System.Web.Caching.Cache.NoSlidingExpiration,
                               System.Web.Caching.CacheItemPriority.Default,
                               null
                                );
                        }
                        var docIcon_Name = "KEY_CACHE_"+docItem.Id;
                        ViewData[docIcon_Name] = dockeyCache;
                    }               
                }
                
                if (batchModel != null)
                    return PartialView("BatchTypeProperties", captureModel);
                else
                    return PartialView("NewBatchTypeProperties");
            }
            catch (Exception e)
            {
                ViewData["ERROR"] = e.Data;
                return PartialView("NewBatchTypeProperties");
            }
        }

        // Display ContentTypeProperties
        public ActionResult ContentTypeProperties(CaptureAdminModel model)
        {
            try
            {
                CaptureAdminModel captureModel = new CaptureAdminModel();
                DocumentType DocModel = new DocumentType();
                DocumentTypeProvider docTypeProvider = new DocumentTypeProvider();
               
                DocModel = docTypeProvider.GetDocumentType(model.DocumentId);
                captureModel.CaptureDocTypeModel = DocModel;

                String sourcesIcon = "~/Resources/" + Constant.DOCUMENT_TYPE_ICON_FOLDER;
                var keyCache = Guid.NewGuid().ToString();
                if (model.DocumentId != Guid.Empty)
                {
                    if (DocModel.Icon != null)
                    {
                        System.Web.HttpContext.Current.Cache.Add(
                           keyCache,
                           DocModel.Icon,
                           null,
                           DateTime.Now.AddMinutes(15),
                           System.Web.Caching.Cache.NoSlidingExpiration,
                           System.Web.Caching.CacheItemPriority.Default,
                           null
                            );
                    }
                }
                else
                {

                    byte[] defautIcon = System.IO.File.ReadAllBytes(Server.MapPath(sourcesIcon + "/appbar.page.text.png"));
                    System.Web.HttpContext.Current.Cache.Add(
                       keyCache,
                       defautIcon,
                       null,
                       DateTime.Now.AddMinutes(15),
                       System.Web.Caching.Cache.NoSlidingExpiration,
                       System.Web.Caching.CacheItemPriority.Default,
                       null
                        );
                }
                ViewData["CONTENT_KEY_CACHE_ICON"] = keyCache;

                if (DocModel != null)
                    return PartialView("_ShowContentTypeProperties", captureModel);
                else
                    return PartialView("_NewContentTypeProperties");
            }
            catch (Exception e)
            {
                ViewData["ERROR"] = e.Data;
                return PartialView("_NewContentTypeProperties");
            }
        }

        public String UploadBatchTypeIcon(String path)
        {
            String iconPath = Server.MapPath(path);
            byte[] binaryIcon = System.IO.File.ReadAllBytes(iconPath);

            var keyCache = Guid.NewGuid().ToString();
            System.Web.HttpContext.Current.Cache.Add(
               keyCache,
               binaryIcon,
               null,
               DateTime.Now.AddMinutes(15),
               System.Web.Caching.Cache.NoSlidingExpiration,
               System.Web.Caching.CacheItemPriority.Default,
               null
                );
            return keyCache;
        }
        //Load table configure 
        public ActionResult LoadTableConfigure(Guid docTypeID, Guid fieldID)
        {
            List<DocumentFieldMetaData> listTableColumn = new List<DocumentFieldMetaData>();
            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider();
            DocumentType docTypeModel = new DocumentType();
            docTypeModel = docTypeProvider.GetDocumentType(docTypeID);
            foreach (var fieldItem in docTypeModel.Fields)
            {
                if (fieldItem.Id == fieldID)
                {
                    listTableColumn = fieldItem.Children;
                }
            }
            return PartialView("_LoadTableConfigure", listTableColumn);
        }

        // Return picklist value
        public string GetPicklistValue(Guid DocTypeID, Guid FieldID)
        {

            string str_result = "";
            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider();
            DocumentType DocTypeModel = new DocumentType();
            DocTypeModel = docTypeProvider.GetDocumentType(DocTypeID);
            foreach (var field_item in DocTypeModel.Fields)
            {
                if (field_item.Id == FieldID)
                {
                    foreach (var picklist_item in field_item.Picklists)
                    {
                        str_result = str_result + picklist_item.Value + "\n";
                    }
                }
            }
            return str_result;
        }
        // use to edit table configure of table fields
        public ActionResult EditTableConfigure(List<DocumentFieldMetaData> tableColumn)
        {

            List<DocumentFieldMetaData> listTableColumn = new List<DocumentFieldMetaData>();
            listTableColumn = tableColumn;
            return PartialView("_LoadTableConfigure", listTableColumn);
        }

        // use to edit Table Content Type of table fields
        public ActionResult EditTableContentType(List<DocumentFieldMetaData> tableColumn)
        {

            List<DocumentFieldMetaData> listDocField = new List<DocumentFieldMetaData>();
            listDocField = tableColumn;
            return PartialView("_LoadTableContentType", listDocField);
        }
        // dùng để lấy  giá trị các colum của các Table Fields
        public JsonResult GetTableValue(CaptureAdminModel model)
        {
            BatchType batchModel = new BatchType();
            BatchTypeProvider batchTypeProvider = new BatchTypeProvider();
            //batchModel = batchTypeProvider.GetBatchType(model.BatchId);
            ArrayList list = new ArrayList();
            List<string> listTemp = new List<string>();
            foreach (var item_field in batchModel.Fields)
            {
                //if (item_field.DataTypeEnum == Ecm.CaptureDomain.FieldDataType.Table)
                //{
                //    foreach (var table_fields in item_field.)
                //    {
                //        list.Add(new
                //        {
                //            FieldId = table_fields.FieldId,
                //            ParentFieldId = table_fields.ParentFieldId,
                //            DocTypeId = table_fields.DocTypeId,
                //            ColumnName = table_fields.ColumnName,
                //            DataType = table_fields.DataType,
                //            MaxLength = table_fields.MaxLength,
                //            DefaultValue = table_fields.DefaultValue,
                //            UseCurrentDate = table_fields.UseCurrentDate
                //        });
                //    }
                //}
            }

            return Json(new { TableValue = list });
        }

        // Configure OCR template
        public ActionResult OCRConfigure(Guid id)
        {
            LanguageProvider _languageProvider = new LanguageProvider();
            List<Language> _languageModel = _languageProvider.GetLanguages();
            ViewData["Languages"] = _languageModel;

            DocumentTypeProvider _docTypeProvider = new DocumentTypeProvider();
            DocumentType _docTypeModel = new DocumentType();
            _docTypeModel = _docTypeProvider.GetDocumentType(id);

            return PartialView("OCRConfigure", _docTypeModel);
        }
        // Save Content type
        public String SaveBatchType(BatchType batchtypemodel, List<DocumentType> listdoctypemodel, string picklist, String keyCacheIcon)
        {
            BatchTypeProvider batchTypeProvider = new BatchTypeProvider();
            DocumentTypeProvider doctypeprovider = new DocumentTypeProvider();

            List<BatchType> listBatchModel = batchTypeProvider.GetBatchTypes();
            List<DocumentType> listDocModel = doctypeprovider.GetDocumentTypes(batchtypemodel.Id);

            Guid result = Guid.Empty;

            #region Batch Type

            if (!batchtypemodel.Id.Equals(Guid.Empty))
                listBatchModel.Remove(batchTypeProvider.GetBatchType(batchtypemodel.Id));

            foreach (var item in listBatchModel)
            {
                if ((item.Name.Equals(batchtypemodel.Name) && batchtypemodel.Id.Equals(Guid.Empty))
                    || (item.Name.Trim().Equals(batchtypemodel.Name.Trim())
                        && !batchtypemodel.Id.Equals(Guid.Empty)
                        && !item.Id.Equals(batchtypemodel.Id)))
                {
                    return result.ToString();
                }
            }

            //Check duplicate batchfield
            if (!batchtypemodel.Id.Equals(Guid.Empty))
            {
                List<BatchFieldMetaData> lstBatchNewFields = new List<BatchFieldMetaData>();
                foreach (var item in batchtypemodel.Fields)
                {
                    var fieldWillDelete = false;
                    foreach (var item_delete in batchtypemodel.Fields)
                    {
                        if (item.Name.Equals(item_delete.Name))
                        {
                            fieldWillDelete = true;
                            break;
                        }
                    }

                    if (item.Id.Equals(Guid.Empty) && !fieldWillDelete)
                    {
                        lstBatchNewFields.Add(item);
                    }
                }

                List<BatchFieldMetaData> lstBatchFields = batchTypeProvider.GetBatchType(batchtypemodel.Id).Fields;
                foreach (var item in lstBatchFields)
                {
                    foreach (var item1 in lstBatchNewFields)
                    {
                        if (item.Name.Equals(item1.Name))
                        {
                            return result.ToString() + "_" + item1.Name;
                        }
                    }
                }
            }
            //End check duplicate field
            #endregion

            #region Document Type
            foreach (var docitem in listdoctypemodel)
            {
                if (!docitem.Id.Equals(Guid.Empty))
                    listDocModel.Remove(doctypeprovider.GetDocumentType(docitem.Id));

                foreach (var item in listDocModel)
                {
                    if ((item.Name.Equals(docitem.Name) && docitem.Id.Equals(Guid.Empty))
                        || (item.Name.Trim().Equals(docitem.Name.Trim())
                            && !docitem.Id.Equals(Guid.Empty)
                            && !item.Id.Equals(docitem.Id)))
                    {
                        return result.ToString();
                    }

                }

                //Check duplicate doc field
                if (!docitem.Id.Equals(Guid.Empty))
                {
                    List<DocumentFieldMetaData> lstDocNewFields = new List<DocumentFieldMetaData>();
                    foreach (var item in docitem.Fields)
                    {
                        var fieldWillDelete = false;
                        foreach (var item_delete in docitem.Fields)
                        {
                            if (item.Name.Equals(item_delete.Name))
                            {
                                fieldWillDelete = true;
                                break;
                            }
                        }

                        if (item.Id.Equals(Guid.Empty) && !fieldWillDelete)
                        {
                            lstDocNewFields.Add(item);
                        }
                    }

                    List<DocumentFieldMetaData> lstDocFields = doctypeprovider.GetDocumentType(docitem.Id).Fields;
                    foreach (var item in lstDocFields)
                    {
                        foreach (var item1 in lstDocNewFields)
                        {
                            if (item.Name.Equals(item1.Name))
                            {
                                return result.ToString() + "_" + item1.Name;
                            }
                        }
                    }
                }
                //End check duplicate field

                //Add content type icon 
                if (!String.IsNullOrEmpty(docitem.IconBase64))
                {
                    byte[] binaryIcons = (byte[])System.Web.HttpContext.Current.Cache.Get(keyCacheIcon);
                    docitem.Icon = binaryIcons;
                }

                batchtypemodel.DocTypes = listdoctypemodel;
            }
          
        

             //PickList
            //if (picklist != "")
            //{
            //    string[] array_picklist = picklist.Split('#');
            //    List<string> list_picklist = new List<string>();
            //    for (int i = 0; i < array_picklist.Length; i++)
            //    {
            //        list_picklist.Add(array_picklist[i]);
            //    }
            //    //list_picklist.RemoveAt(0); 
            //    foreach (var item in doctypemodel.Fields)
            //    {
            //        if (item.DataType.ToString() == "Picklist")
            //        {

            //            List<Picklist> listpick = new List<Picklist>();

            //            string temp = list_picklist.First();
            //            string[] picklist_value = temp.Split('\n');
            //            for (int j = 0; j < picklist_value.Length; j++)
            //            {
            //                if (picklist_value[j] != "")
            //                {
            //                    Picklist pickmodel = new Picklist();
            //                    pickmodel.Value = picklist_value[j];
            //                    pickmodel.FieldId = item.Id;
            //                    listpick.Add(pickmodel);
            //                }
            //            }
            //            list_picklist.RemoveAt(0);
            //            item.Picklists = listpick;
            //        }
            //    }
            //}
            #endregion

            //Add batch type icon 
            if (!String.IsNullOrEmpty(keyCacheIcon))
            {
                byte[] binaryIcon = (byte[])System.Web.HttpContext.Current.Cache.Get(keyCacheIcon);
                batchtypemodel.Icon = binaryIcon;
            }
            
            batchTypeProvider.SaveBatchType(batchtypemodel);
            if (batchtypemodel.Id.Equals(Guid.Empty))
            {
                listBatchModel = batchTypeProvider.GetBatchTypes();
                foreach (var item in listBatchModel)
                {
                    if (item.Name.Equals(batchtypemodel.Name))
                    {
                        result = item.Id;
                        break;
                    }
                }
                return result.ToString();
            }
            else
            {
                return batchtypemodel.Id.ToString();
            }
        }

        //Delete Batch  Type
        public void DeleteBatchType(Guid BatchId)
        {
            BatchType _batchTypeModel = new BatchType();
            BatchTypeProvider _batchtypeProvider = new BatchTypeProvider();
            _batchTypeModel = _batchtypeProvider.GetBatchType(BatchId);
            _batchtypeProvider.DeleteBatchType(_batchTypeModel.Id);
        }
       
        #endregion


        #region Users Action Result

        // Show user properties
        public ActionResult ShowUserProperties(Guid userID)
        {
            CaptureAdminModel captureAdminModel = new CaptureAdminModel();
            //LanguageProvider languageProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            captureAdminModel.ListLanguages = new LanguageProvider().GetLanguages();
            //captureAdminModel.ListLanguages = languageProvider.GetLanguages();
            captureAdminModel.CaptureUserModel = null;
            //UserProvider userProvider = new UserProvider(Utilities.UserName, Utilities.Password);
            List<User> listUsers = new List<User>();
            listUsers = new UserProvider().GetAvailableUserToDelegation();
            //listUsers = userProvider.GetAvailableUserToDelegation();

            foreach (var userItem in listUsers)
            {
                if (userItem.Id == userID)
                {
                    captureAdminModel.CaptureUserModel = userItem;
                    break;
                }
            }

            //Create cache to show profile picture
            if (userID != Guid.Empty)
            {
                if (captureAdminModel.CaptureUserModel.Photo != null)
                {
                    var imageBinary = captureAdminModel.CaptureUserModel.Photo;
                    var keyCache = Guid.NewGuid().ToString();
                    System.Web.HttpContext.Current.Cache.Add(keyCache,
                        imageBinary, null, DateTime.Now.AddMinutes(15),
                        System.Web.Caching.Cache.NoSlidingExpiration,
                        System.Web.Caching.CacheItemPriority.Default, null);
                    ViewData["KEY_CACHE_PROFILE_PIC"] = keyCache;
                }
            }

            if (captureAdminModel.CaptureUserModel != null)
            {
                return PartialView("_ShowUserProperties", captureAdminModel);
            }
            else
            {
                return PartialView("_AddNewUser", captureAdminModel);
            }
        }

        //Show User 
        public ActionResult ShowUser()
        {
            //UserProvider userProvider = new UserProvider(Utilities.UserName, Utilities.Password);
            List<User> usersList = new List<User>();
            //usersList = userProvider.GetAvailableUserToDelegation();
            usersList = new UserProvider().GetAvailableUserToDelegation();
            foreach (var user in usersList)
            {
                if (user.Photo != null)
                {
                    var imageBinary = user.Photo;
                    var keyCache = user.Id.ToString();
                    System.Web.HttpContext.Current.Cache.Add(keyCache,
                        imageBinary, null, DateTime.Now.AddMinutes(15),
                        System.Web.Caching.Cache.NoSlidingExpiration,
                        System.Web.Caching.CacheItemPriority.Default, null);
                    ViewData["KEY_CACHE_PROFILE_PIC"] = keyCache;
                }
            }
            return PartialView("_ShowUser", usersList);
        }

        //Save User
        public Guid SaveUser(User userModel, String keyCache)
        {
            byte[] binaryPic;
            Guid result = Guid.Empty;
            UserProvider userProvider = new UserProvider();
            List<User> lstUserModel = new List<User>(); ;
            lstUserModel = userProvider.GetAvailableUserToDelegation();
            foreach (var item in lstUserModel)
            {
                if ((item.UserName.Equals(userModel.UserName) || userModel.UserName.Equals(Utilities.UserName))
                    && userModel.Id.Equals(Guid.Empty))
                {
                    return result;
                }
            }

            if (userModel.IsAdmin == true)
            {
                userModel.UserGroups.Clear();
            }

            if (!String.IsNullOrEmpty(keyCache))
            {
                binaryPic = (byte[])System.Web.HttpContext.Current.Cache[keyCache];
                userModel.Photo = binaryPic;
            }
            userProvider.SaveUser(userModel);
            if (userModel.Id.Equals(Guid.Empty))
            {
                lstUserModel = userProvider.GetAvailableUserToDelegation();
                foreach (var item in lstUserModel)
                {
                    if (item.UserName.Equals(userModel.UserName))
                    {
                        result = item.Id;
                        break;
                    }
                }
                return result;
            }
            else
            {
                return userModel.Id;
            }
        }

        // Delete user
        public void DeleteUser(User userModel)
        {
            UserProvider userProvider = new UserProvider();
            userProvider.GetAvailableUserToDelegation();
            if (userModel.Id != Guid.Empty)
            {
                userProvider.DeleteUser(userModel);
            }
        }

        // Get profile piture
        public FileResult GetImageFromCacheByKey(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                var imageBinary = (byte[])System.Web.HttpContext.Current.Cache[key];
                return File(imageBinary, Ecm.Utility.ContentTypeEnumeration.Image.PNG);
            }
            else
            {
                return null;
            }
        }
        public string UploadProfilePic(HttpPostedFileBase srcPicture)
        {
            byte[] binaryPic = new byte[srcPicture.InputStream.Length];
            srcPicture.InputStream.Read(binaryPic, 0, System.Convert.ToInt32(srcPicture.InputStream.Length));

            var keyCache = Guid.NewGuid().ToString();
            System.Web.HttpContext.Current.Cache.Add(keyCache,
                    binaryPic, null, DateTime.Now.AddMinutes(15),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Default, null);

            return keyCache;
        }
        #endregion

        #region  User Group Action Result

        //Show User Group
        public ActionResult LoadUserGroups()
        {

            List<UserGroup> listUseGroups = new List<UserGroup>();
            listUseGroups = new UserGroupProvider().GetUserGroups();
            return PartialView("_LoadUserGroups", listUseGroups);

        }

        //show usergroup properties
        public ActionResult ShowUserGroupProperties(UserGroup userGroupModel)
        {
            List<UserGroup> listUserGroupModel = new List<UserGroup>();
            listUserGroupModel = new UserGroupProvider().GetUserGroups();

            if (userGroupModel.Id != Guid.Empty)
            {
                foreach (var userGroupItem in listUserGroupModel)
                {
                    if (userGroupItem.Id == userGroupModel.Id)
                    {
                        userGroupModel = userGroupItem;

                        break;
                    }
                }
            }
            if (userGroupModel.Id != Guid.Empty)
            {
                return PartialView("_ShowUserGroupProperties", userGroupModel);
            }
            else
            {
                return PartialView("_AddUserGroup", userGroupModel);
            }
        }

        //Delete UserGroup
        public void DeleteUserGroup(UserGroup userGroupModel)
        {
            UserGroupProvider userGroupProvider = new UserGroupProvider();
            if (userGroupModel.Id != Guid.Empty)
            {
                userGroupProvider.DeleteUserGroup(userGroupModel);
            }

        }
        // save user group

        public Guid SaveUserGroup(UserGroup userGruopModel)
        {
            Guid result = Guid.Empty;
            UserGroupProvider userGroupProvider = new UserGroupProvider();
            List<UserGroup> lstUserGroupModel = new  List<UserGroup>();
            lstUserGroupModel = userGroupProvider.GetUserGroups();
            foreach (var item in lstUserGroupModel)
            {
                //usergroup name is exsisted
                if ((item.Name.Trim().Equals(userGruopModel.Name) && userGruopModel.Id.Equals(Guid.Empty))
                    || (item.Name.Trim().Equals(userGruopModel.Name.Trim())
                        && !userGruopModel.Id.Equals(Guid.Empty)
                        && !item.Id.Equals(userGruopModel.Id)))
                {
                    return result;
                }
            }

            userGroupProvider.Save(userGruopModel);
            if (userGruopModel.Id.Equals(Guid.Empty))
            {
                lstUserGroupModel = userGroupProvider.GetUserGroups();
                foreach (var item in lstUserGroupModel)
                {
                    if (item.Name.Equals(userGruopModel.Name))
                    {
                        result = item.Id;
                        break;
                    }
                }
                return result;
            }
            else
            {
                return userGruopModel.Id;
            }
        }

        //show usergroup list (of user function)
        public ActionResult ShowListUserGroup(User userModel)
        {
            List<UserGroup> listUserGroup = new List<UserGroup>();
            List<UserGroup> listUserGroupResult = new List<UserGroup>();
            listUserGroup = new UserGroupProvider().GetUserGroups();
            foreach (var userGroupItem in listUserGroup)
            {

                if (userModel.UserGroups.Count > 0)
                {
                    var check = false;
                    foreach (var existUserGroupItem in userModel.UserGroups)
                    {
                        if (userGroupItem.Id == existUserGroupItem.Id)
                        {
                            check = true;
                            break;
                        }
                    }
                    if (!check)
                    {
                        listUserGroupResult.Add(userGroupItem);
                    }

                }
                else
                {
                    listUserGroupResult.Add(userGroupItem);
                }


            }
            return PartialView("_ShowListUserGroup",listUserGroupResult);
        }

        //search user to add into usergroup
        public ActionResult SearchUser(UserGroup userModel)
        {
            List<User> listUser = new List<User>();
            List<User> listUserResult = new List<User>();
            listUser = new UserProvider().GetAvailableUserToDelegation();
            foreach (var userItem in listUser)
            {
                if (userItem.IsAdmin != true)
                {
                    if (userModel.Users.Count > 0)
                    {
                        var check = false;
                        foreach (var existUserItem in userModel.Users)
                        {
                            if (userItem.Id == existUserItem.Id)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (!check)
                        {
                            listUserResult.Add(userItem);
                        }

                    }
                    else
                    {
                        listUserResult.Add(userItem);
                    }

                }
            }
            return PartialView("_SearchUser", listUserResult);
        }

        #endregion

        #region Permissions Action Result
        // show permission
        public ActionResult ShowPermission(string viewBy)
        {
            CaptureAdminModel captureAdminModel = new CaptureAdminModel();
            PermissionProvider permissionProvider = new PermissionProvider();
            captureAdminModel.ListBatchTypeModel = permissionProvider.GetBatchTypes();
            captureAdminModel.ListUserGroup = permissionProvider.GetUserGroups();
            if (viewBy == "bycontent")
            {
                return PartialView("_ShowPermission", captureAdminModel);
            }
            else
            {
                return PartialView("_ShowPermissionByUserGroup", captureAdminModel);
            }

        }

        //// show permissions properties
        //public ActionResult ShowPermissionProperties(UserGroup userGroup, DocumentType docType)
        //{
        //    PermissionProvider permissionProvider = new PermissionProvider(Utilities.UserName, Utilities.Password);
        //    CaptureAdminModel captureAdminModel = new CaptureAdminModel();
        //    //captureAdminModel.DocTypePermission = permissionProvider.GetPermission(userGroup, docType);
        //    //captureAdminModel.AnnotationPermission = permissionProvider.GetAnnotationPermission(userGroup, docType);
        //    if (captureAdminModel.DocTypePermission == null)
        //    {
        //        captureAdminModel.DocTypePermission = SetAllAttributeDocumentFalse();
        //    }
        //    if (captureAdminModel.AnnotationPermission == null)
        //    {
        //        captureAdminModel.AnnotationPermission = SetAllAttributeAnnotationFalse();
        //    }

        //    return PartialView(captureAdminModel);
        //}

        // show permission
        //public ActionResult ShowPermission(string viewBy)
        //{
        //    CaptureAdminModel captureAdminModel = new CaptureAdminModel();
        //    PermissionProvider permissionProvider = new PermissionProvider(Utilities.UserName, Utilities.Password);
        //    //captureAdminModel.ListDocTypeModel = permissionProvider.GetDocTypes();
        //    //captureAdminModel.ListUserGroup = permissionProvider.GetUserGroups();
        //    if (viewBy == "bycontent")
        //    {
        //        return PartialView(captureAdminModel);
        //    }
        //    else
        //    {
        //        return PartialView("_ShowPermissionByUserGroup", captureAdminModel);
        //    }

        //}
        #endregion

        #region
        public DocumentTypePermissionModel SetAllAttributeDocumentFalse()
        {
            DocumentTypePermissionModel docTypePermission = new DocumentTypePermissionModel();
            docTypePermission.Id = Guid.Empty;
            docTypePermission.CanAppendPage = false;
            docTypePermission.CanCapture = false;
            docTypePermission.CanDeletePage = false;
            docTypePermission.CanDownloadOffline = false;
            docTypePermission.CanEmailDocument = false;
            docTypePermission.CanExportFieldValue = false;
            docTypePermission.CanHideAllAnnotation = false;
            docTypePermission.CanReplacePage = false;
            docTypePermission.CanRotatePage = false;
            docTypePermission.CanSearch = false;
            docTypePermission.CanSeeRetrictedField = false;
            docTypePermission.CanUpdateFieldValue = false;
            docTypePermission.CanPrintDocument = false;
            return docTypePermission;
        }

        public AnnotationPermissionModel SetAllAttributeAnnotationFalse()
        {
            AnnotationPermissionModel annotationPermission = new AnnotationPermissionModel();
            annotationPermission.Id = Guid.Empty;
            annotationPermission.CanAddHighlight = false;
            annotationPermission.CanAddRedaction = false;
            annotationPermission.CanAddText = false;
            annotationPermission.CanDeleteHighlight = false;
            annotationPermission.CanDeleteRedaction = false;
            annotationPermission.CanDeleteText = false;
            annotationPermission.CanHideRedaction = false;
            annotationPermission.CanSeeHighlight = false;
            annotationPermission.CanSeeText = false;
            return annotationPermission;

        }
        #endregion
    }
}
