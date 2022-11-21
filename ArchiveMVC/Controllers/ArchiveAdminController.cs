using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArchiveMVC.Models;
using ArchiveMVC.Models.DataProvider;
using ArchiveMVC.Utility;

using System.Collections;
using Ecm.Domain;
using System.IO;
using Ecm.Utility.ProxyHelper;
using Ecm.Service.Contract;
using log4net;
using Newtonsoft.Json;
using System.Data;

namespace ArchiveMVC.Controllers
{
    public class ArchiveAdminController : Controller
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: /ArchiveAdmin/

        public ActionResult Index()
        {
            Utilities.OpenningDocument = null;
            if (!Utilities.IsAdmin)
            {
                return RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_SEARCH);
            }
            
            //Edit by Loc Ngo
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
        // hiển thị danh sách các content type
        public ActionResult ShowListContentType()
        {
            ArchiveAdminModel model = new ArchiveAdminModel();
            DocumentTypeProvider _DocumentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            // ArchiveModel.ListDocType = _DocumentTypeProvider.GetCapturedDocumentTypes();
            IList<DocumentTypeModel> _DocumentTypeModelList = new List<DocumentTypeModel>();
            _DocumentTypeModelList = _DocumentTypeProvider.GetCapturedDocumentTypes();

            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var cPri = System.Web.Caching.CacheItemPriority.Normal;
            string id = "";
            string cacheKey = "CaptureControler_GetDocType";
            object returnList = System.Web.HttpContext.Current.Cache[cacheKey];
            List<DocTypeResult> ListDocTypeCache = new List<DocTypeResult>();
            if (returnList != null)
            {
                ListDocTypeCache = (List<DocTypeResult>)returnList;
            }
            bool isUpdateCache = false;
            foreach (DocumentTypeModel doc in _DocumentTypeModelList)
            {
                if (ListDocTypeCache.Where(p => p.DocType.Id == doc.Id).Count() > 0)
                {
                    id = ListDocTypeCache.Where(p => p.DocType.Id == doc.Id).FirstOrDefault().IconKey;
                }
                else
                {
                    isUpdateCache = true; //có cập nhật cache ds Doctype
                    id = Guid.NewGuid().ToString();
                    if (doc.Icon != null)
                    {
                        System.Web.HttpContext.Current.Cache.Add(id, doc.Icon, null, cTime, cExp, cPri, null);
                        ListDocTypeCache.Add(new DocTypeResult
                        {
                            DocType = new DocumentTypeModel
                            {
                                Name = doc.Name,
                                Id = doc.Id,
                                Fields = doc.Fields
                            },
                            IconKey = id
                        });
                    }
                }
                model.ListDocType.Add(id, doc);
            }
            return PartialView("ShowListContentType", model);
        }
        
        //Edit by Loc Ngo
        public String UploadContentTypeIcon(String path)
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

        // dùng để hiển thị ContentTypeProperties khi click vào mỗi content type
        public ActionResult ContentTypeProperties(ArchiveAdminModel model)
        {
            try
            {
                ArchiveAdminModel ArchiveModel = new ArchiveAdminModel();
                DocumentTypeModel DocModel = new DocumentTypeModel();
                DocumentTypeProvider _documentprovider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
                DocModel = _documentprovider.GetDocumentType(model.DocumentId);
                ArchiveModel.ArChiveDocTypeModel = DocModel;

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
                ViewData["KEY_CACHE_ICON"] = keyCache;

                if (DocModel != null)
                    return PartialView("ContentTypeProperties", ArchiveModel);
                else
                    return PartialView("NewContentTypeProperties");
            }
            catch (Exception e)
            {
                ViewData["ERROR"] = e.Data;
                return PartialView("NewContentTypeProperties");
            }
        }

        //Loc Ngo
        //Delete orc template
        public bool DeleteOcrTemplate(Guid id)
        {
            DocumentTypeProvider documentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            try
            {
                documentTypeProvider.DeleteOCRTemplate(id);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        //Show barcode configure
        public ActionResult ShowBarcodeConfigure(Guid id)
        {
            try
            {
                DocumentTypeProvider documentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
                DocumentTypeModel documentTypeModel = null;
            
                    documentTypeModel = documentTypeProvider.GetDocumentType(id);
            
                if (documentTypeModel.HasBarcodeConfigurations)
                {
                    return PartialView("ShowBarcodeConfigure", documentTypeModel);
                }
                else
                {
                    return PartialView("NewBarcodeConfigure", documentTypeModel);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                return Json(new ECMJsonMessage() { IsError=true, Message = e.Message });
            }
        }

        //Save barcode
        public bool SaveBarcode(List<BarcodeConfigurationModel> save_barcode_fields, List<Guid> delete_barcode_fields)
        {
            try
            {
                DocumentTypeProvider documentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
                if (save_barcode_fields != null)
                {
                    foreach (var item in save_barcode_fields)
                    {
                        documentTypeProvider.SaveBarcodeConfiguration(item);
                    }
                }

                if (delete_barcode_fields != null)
                {
                    foreach (var item in delete_barcode_fields)
                    {
                        documentTypeProvider.DeleteBarcodeConfiguration(item);
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }


        //Delete barcode
        public bool DeleteBarcode(Guid id)
        {
            DocumentTypeProvider documentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            try
            {
                documentTypeProvider.ClearBarcodeConfigurations(id);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        // dùng để lấy  giá trị các colum của các Table Fields
        public JsonResult GetTableValue(ArchiveAdminModel model)
        {
            
            DocumentTypeModel DocModel = new DocumentTypeModel();
            DocumentTypeProvider _documentprovider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocModel = _documentprovider.GetDocumentType(model.DocumentId);
            ArrayList list = new ArrayList();
            List<string> listTemp = new List<string>();
            foreach (var item_field in DocModel.Fields)
            {
                if (item_field.DataType == Ecm.Domain.FieldDataType.Table)
                {
                    foreach (var table_fields in item_field.Children)
                    {
                        list.Add(new {
                            FieldId = table_fields.FieldId,
                            ParentFieldId = table_fields.ParentFieldId,
                            DocTypeId = table_fields.DocTypeId,
                            ColumnName = table_fields.ColumnName,
                            DataType = table_fields.DataType,
                            MaxLength = table_fields.MaxLength,
                            DefaultValue = table_fields.DefaultValue,
                            UseCurrentDate = table_fields.UseCurrentDate
                        });

                        /*listTemp.Add(String.Format(@"{{
                                          FieldId:'{0}',
                                          ParentFieldId:'{1}',
                                          DocTypeId:'{2}',
                                          ColumnName:'{3}',
                                          DataType:'{4}',
                                          MaxLength:'{5}',
                                          DefaultValue:'{6}',
                                          UseCurrentDate:'{7}'
                                    }}", table_fields.FieldId, table_fields.ParentFieldId, table_fields.DocTypeId, table_fields.ColumnName, table_fields.DataType, table_fields.MaxLength, table_fields.DefaultValue, table_fields.UseCurrentDate));*/
                    }
                }
            }
            
            return Json(new { TableValue = list });
        }

        //dùng để xóa content type
        public void DeleteContentType(Guid DocumentId)
        {
            DocumentTypeModel _documenttypemodel = new DocumentTypeModel();
            DocumentTypeProvider _documenttypeprovider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            _documenttypemodel = _documenttypeprovider.GetDocumentType(DocumentId);
            _documenttypeprovider.DeleteDocumentType(_documenttypemodel);
        }

        // Lưu content type
        public String SaveContentType(DocumentTypeModel doctypemodel, string picklist, String keyCacheIcon /*ttpPostedFileBase fleUpload*/)
        {
            //doctypemodel.Icon = ProcessImages.ImageToByteArray(Resources.ArchiveAdmin.FolderOpen);
            //if (fleUpload != null)
            //{
            //    // Create a byte array of file stream length
            //    byte[] FileBinariesPosted = new byte[fleUpload.InputStream.Length];
            //    //Read block of bytes from stream into the byte array
            //    fleUpload.InputStream.Read(FileBinariesPosted, 0, System.Convert.ToInt32(fleUpload.InputStream.Length));
                

            //}

            //Loc Ngo
            DocumentTypeProvider doctypeprovider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            List<DocumentTypeModel> lstModel = doctypeprovider.GetDocumentTypes();
            //doctypemodel.Name = System.Text.RegularExpressions.Regex.Replace(doctypemodel.Name, @"\s+", " ").Trim();

            Guid result = Guid.Empty;
            if (!doctypemodel.Id.Equals(Guid.Empty))
                lstModel.Remove(doctypeprovider.GetDocumentType(doctypemodel.Id));

            foreach (var item in lstModel)
            {
                if ((item.Name.Equals(doctypemodel.Name) && doctypemodel.Id.Equals(Guid.Empty))
                    || (item.Name.Trim().Equals(doctypemodel.Name.Trim())
                        && !doctypemodel.Id.Equals(Guid.Empty)
                        && !item.Id.Equals(doctypemodel.Id)))
                {
                    return result.ToString();
                }
            }

            //Check duplicate field
            if (!doctypemodel.Id.Equals(Guid.Empty))
            {
                List<FieldMetaDataModel> lstNewFields = new List<FieldMetaDataModel>();
                foreach (var item in doctypemodel.Fields)
                {
                    var fieldWillDelete = false;
                    foreach (var item_delete in doctypemodel.DeletedFields)
                    {
                        if (item.Name.Equals(item_delete.Name))
                        {
                            fieldWillDelete = true;
                            break;
                        }
                    }

                    if (item.Id.Equals(Guid.Empty) && !fieldWillDelete)
                    {
                        lstNewFields.Add(item);
                    }
                }



                List<FieldMetaDataModel> lstFields = doctypeprovider.GetDocumentType(doctypemodel.Id).Fields;
                foreach (var item in lstFields)
                {
                    foreach (var item1 in lstNewFields)
                    {
                        if (item.Name.Equals(item1.Name))
                        {
                            return result.ToString() + "_" + item1.Name;
                        }
                    }
                }
            }
            //End check duplicate field

            if (!String.IsNullOrEmpty(keyCacheIcon))
            {
                byte[] binaryIcon = (byte[])System.Web.HttpContext.Current.Cache.Get(keyCacheIcon);
                doctypemodel.Icon = binaryIcon;
            }
            //End Loc Ngo

            if (picklist != "")
            {
                string[] array_picklist = picklist.Split('#');
                List<string> list_picklist = new List<string>();
                for (int i = 0; i < array_picklist.Length; i++)
                {
                    list_picklist.Add(array_picklist[i]);
                }
                //list_picklist.RemoveAt(0); // bỏ khoảng trống đầu tiên
                foreach (var item in doctypemodel.Fields)
                {
                    if (item.DataType.ToString() == "Picklist")
                    {

                        List<PicklistModel> listpick = new List<PicklistModel>();

                        string temp = list_picklist.First();
                        string[] picklist_value = temp.Split('\n');
                        for (int j = 0; j < picklist_value.Length; j++)
                        {
                            if (picklist_value[j] != "")
                            {
                                PicklistModel pickmodel = new PicklistModel();
                                pickmodel.Value = picklist_value[j];
                                pickmodel.FieldId = item.Id;
                                listpick.Add(pickmodel);
                            }
                        }
                        list_picklist.RemoveAt(0);
                        item.Picklists = listpick;
                    }
                }
            }
            
            
            doctypeprovider.SaveDocumentType(doctypemodel);
            if (doctypemodel.Id.Equals(Guid.Empty))
            {
                lstModel = doctypeprovider.GetDocumentTypes();
                foreach (var item in lstModel)
                {
                    if (item.Name.Equals(doctypemodel.Name))
                    {
                        result = item.Id;
                        break;
                    }
                }
                return result.ToString();
            }
            else
            {
                return doctypemodel.Id.ToString();
            }
         }

        //Delete fields of content type 
        public void DeleteFields(DocumentTypeModel doctypemodel)
        {
         
            DocumentTypeModel _doctype = new DocumentTypeModel();
            FieldMetaDataModel _fields = new FieldMetaDataModel();
            _doctype.Id = doctypemodel.Id;
            foreach (var item in doctypemodel.DeletedFields)
            {
                _doctype.DeletedFields.Add(item);
            }
            
           

        }

        public ActionResult ShowDeleteField()
        {
            List<DocumentTypeModel> listdoctypemodel = new List<DocumentTypeModel>();
            DocumentTypeProvider doctypeprovider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            listdoctypemodel = doctypeprovider.GetDocumentTypes();

           
            DocumentTypeModel doctypemodel = new DocumentTypeModel();
            List<FieldMetaDataModel> listfield= new List<FieldMetaDataModel>();
            List<TableColumnModel> _children = new List<TableColumnModel>();
            for (int i = 0; i < 2;i++ )
                {
                    TableColumnModel tb = new TableColumnModel();
                    tb.ColumnName = "col"+i;
                    tb.DataType = Ecm.Domain.FieldDataType.String;
                    _children.Add(tb);
            
                 }               
            
            
            doctypemodel.Name = "Test table";
            for (int i = 0; i<3; i++)
            {
                FieldMetaDataModel f = new FieldMetaDataModel();
                f.Name = "f"+i;
                f.DataType = Ecm.Domain.FieldDataType.String;
                if (i == 2)
                {
                    f.DataType = Ecm.Domain.FieldDataType.Table;
                    f.Children = _children;
                }
                listfield.Add(f);

            }
            doctypemodel.Fields = listfield;
                doctypeprovider.SaveDocumentType(doctypemodel);


            return View();
        }

        // trả về giá trị của picklist
        public string GetPicklistValue(Guid DocTypeID, Guid FieldID)
        {
            
            string str_result = "";
            DocumentTypeProvider doctypeprovider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentTypeModel DocTypeModel = new DocumentTypeModel();
            DocTypeModel = doctypeprovider.GetDocumentType(DocTypeID);
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

        // load table configure 
        public ActionResult LoadTableConfigure(Guid docTypeID, Guid fieldID)
        {
            List<TableColumnModel> listTableColumn = new List<TableColumnModel>();
            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentTypeModel docTypeModel = new DocumentTypeModel();
            docTypeModel = docTypeProvider.GetDocumentType(docTypeID);
            foreach (var fieldItem in docTypeModel.Fields)
            {
                if (fieldItem.Id == fieldID)
                {
                    listTableColumn = fieldItem.Children;
                }
            }
            return PartialView("LoadTableConfigure", listTableColumn);
            //return PartialView("ContentTypeProperties", ArchiveModel);
        }

        // use to edit table configure of table fields
        public ActionResult EditTableConfigure(List<TableColumnModel> tableColumn)
        {

            List<TableColumnModel> listTableColumn = new List<TableColumnModel>();
            listTableColumn = tableColumn;
            return PartialView("LoadTableConfigure", listTableColumn);
        }

        // use to test connection and get databasename
        public JsonResult TestConnection(LookupConnectionModel connectionInfo)
        {
            Boolean result = false;
            List<string> listDatabaseName = new List<string>();
            List<string> schemas = new List<string>();
            LookupProvider looKupProvider = new LookupProvider(Utilities.UserName, Utilities.Password);
            result = looKupProvider.TestConnection(connectionInfo);
            
            if (result)
            {
                listDatabaseName = looKupProvider.GetDatabaseName(connectionInfo);
                connectionInfo.DatabaseName = listDatabaseName[0];
                schemas = looKupProvider.GetSchemas(connectionInfo);
            }
            else
            {
                return Json(new { Testconnectionfail = "false" });
            }
            return Json(new { DatabaseName = listDatabaseName, SchemaName = schemas });
        }

        // use to get datasource
        public JsonResult GetDataSource(LookupConnectionModel connectionInfo, LookupDataSourceType lookupType)
        {
            LookupProvider lookupProvider = new LookupProvider(Utilities.UserName,Utilities.Password);
            List<string> dataSoures = lookupProvider.GetDataSources(connectionInfo, lookupType);

            return Json(new { DataSources = dataSoures });
        }

        public JsonResult GetColumns(LookupConnectionModel connectionInfo, string sourceName, LookupDataSourceType lookupType)
        {
            Dictionary<string, string> columnNames = new Dictionary<string, string>();
            LookupProvider lookupProvider = new LookupProvider(Utilities.UserName,Utilities.Password);
            columnNames = lookupProvider.GetColumns(connectionInfo, sourceName, lookupType);
            return Json(new { LookupColumn = KeyValue<string,string>.ConvertDictionary(columnNames)});
            

        }

        public JsonResult GetOperators(string dataType)
        {
            List<string> operators = new List<string>();
            switch (dataType)
            {
                case "bit":
                case "bool":
                case "boolean":
                    operators.Add(Common.EQUAL);
                    operators.Add(Common.NOT_EQUAL);
                    break;
                case "int":
                case "decimal":
                case "tinyint":
                case "bigint":
                case "float":
                case "date":
                case "datetime":
                case "real":
                case "double precision":
                case "numeric":
                case "number":
                case "integer":
                case "double":
                case "smallint":
                    operators.Add(Common.EQUAL);
                    operators.Add(Common.NOT_EQUAL);
                    operators.Add(Common.GREATER_THAN);
                    operators.Add(Common.LESS_THAN);
                    operators.Add(Common.GREATER_THAN_OR_EQUAL_TO);
                    operators.Add(Common.LESS_THAN_OR_EQUAL_TO);
                    break;
                case "char":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                case "uniqueidentifier":
                case "character varying":
                case "varchar2":
                case "nvarchar2":
                case "longtext":
                case "mediumtext":
                case "tinytext":
                    operators.Add(Common.EQUAL);
                    operators.Add(Common.NOT_EQUAL);
                    operators.Add(Common.STARTS_WITH);
                    operators.Add(Common.ENDS_WITH);
                    operators.Add(Common.CONTAINS);
                    operators.Add(Common.NOT_CONTAINS);
                    break;
            }

            return Json(operators);
        }
        
        public JsonResult GetOperatorsFromStored(string dataType)
        {
            List<string> operators = new List<string>();
            switch (dataType.ToLower())
            {
                case "bool":
                case "boolean":
                    operators.Add(Common.EQUAL);
                    operators.Add(Common.NOT_EQUAL);
                    break;
                case "int16":
                case "int32":
                case "int64":
                case "decimal":
                case "float":
                case "date":
                case "datetime":
                case "real":
                case "double":
                    operators.Add(Common.EQUAL);
                    operators.Add(Common.NOT_EQUAL);
                    operators.Add(Common.GREATER_THAN);
                    operators.Add(Common.LESS_THAN);
                    operators.Add(Common.GREATER_THAN_OR_EQUAL_TO);
                    operators.Add(Common.LESS_THAN_OR_EQUAL_TO);
                    break;
                case "string":
                    operators.Add(Common.EQUAL);
                    operators.Add(Common.NOT_EQUAL);
                    operators.Add(Common.STARTS_WITH);
                    operators.Add(Common.ENDS_WITH);
                    operators.Add(Common.CONTAINS);
                    operators.Add(Common.NOT_CONTAINS);
                    break;
            }

            return Json(operators);
        }

        public ActionResult LoadParameters(LookupConnectionModel connectionInfo, string storedName)
        {
            LookupProvider lookupProvider = new LookupProvider(Utilities.UserName, Utilities.Password);
            List<ParameterModel> parameters = new List<ParameterModel>();
            DataTable dt = lookupProvider.GetParameters(connectionInfo, storedName);

            foreach (DataRow para in dt.Rows)
            {
                ParameterModel paraModel = new ParameterModel
                {
                    Mode = para["Mode"].ToString(),
                    OrderIndex = para["OrderIndex"].ToString(),
                    ParameterName = para["Name"].ToString(),
                    ParameterType = para["DataType"].ToString(),
                };

                parameters.Add(paraModel);
            }

            return PartialView("LookupParameter", parameters);
        }

        public JsonResult BuildCommandText(LookupInfoModel lookupInfo)
        {

            return Json(BuildCommandTextString(lookupInfo));
        }

        public JsonResult BuildExecuteCommand(LookupInfoModel lookupInfo)
        {
            return Json(BuildExecuteCommandString(lookupInfo));
        }

        public JsonResult BuildWhereClause(LookupInfoModel lookupInfo, string lookupColumn, string lookupDataType, string operatorText)
        {
            string value = string.Empty;
            string whereClause = string.Empty;
            var openChar = string.Empty;
            var closeChar = string.Empty;
            var valueChar = string.Empty;

            string sql = BuildCommandTextString(lookupInfo);

            switch (lookupInfo.ConnectionInfo.DatabaseType)
            {
                case (int)DatabaseType.MsSql:
                    openChar = "[";
                    closeChar = "]";
                    valueChar = "'";
                    break;
                case (int)DatabaseType.MySql:
                    openChar = "`";
                    closeChar = "`";
                    valueChar = "\"";
                    break;
                case (int)DatabaseType.PostgreSql:
                    openChar = "\"";
                    closeChar = "\"";
                    valueChar = "'";
                    break;
                case (int)DatabaseType.Oracle:
                    openChar = "\"";
                    closeChar = "\"";
                    valueChar = "\"";
                    break;
                case (int)DatabaseType.DB2:
                    openChar = "\"";
                    closeChar = "\"";
                    valueChar = "\"";
                    break;
                default:
                    throw new NotSupportedException();
            }
            switch (lookupDataType)
            {
                case "real":
                case "double precision":
                case "numeric":
                case "number":
                case "int":
                case "integer":
                case "decimal":
                case "tinyint":
                case "bigint":
                case "float":
                case "double":
                case "smallint":
                case "bit":
                case "bool":
                case "boolean":
                case "uniqueidentifier":
                    value = "<<value>>";
                    break;
                case "char":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                case "date":
                case "character varying":
                case "datetime":
                case "varchar2":
                case "nvarchar2":
                case "longtext":
                case "mediumtext":
                case "tinytext":
                    value = valueChar + "<<value>>" + valueChar;
                    break;
            }

            string searchOperator = string.Empty;

            switch (operatorText)
            {
                case Common.EQUAL:
                    searchOperator = "=";
                    break;
                case Common.GREATER_THAN:
                    searchOperator = ">";
                    break;
                case Common.GREATER_THAN_OR_EQUAL_TO:
                    searchOperator = ">=";
                    break;
                case Common.LESS_THAN:
                    searchOperator = "<";
                    break;
                case Common.LESS_THAN_OR_EQUAL_TO:
                    searchOperator = "<=";
                    break;
                case Common.CONTAINS:
                case Common.STARTS_WITH:
                case Common.ENDS_WITH:
                    searchOperator = "LIKE";
                    break;
                case Common.NOT_CONTAINS:
                    searchOperator = "NOT LIKE";
                    break;
                case Common.NOT_EQUAL:
                    searchOperator = "<>";
                    break;
            }



            if (operatorText == Common.CONTAINS|| operatorText == Common.NOT_CONTAINS)
            {
                value = "" + valueChar + "%<<value>>%" + valueChar + "";
            }

            if (operatorText == Common.STARTS_WITH)
            {
                value = "" + valueChar + "<<value>>%" + valueChar + "";
            }

            if (operatorText == Common.ENDS_WITH)
            {
                value = "" + valueChar + "%<<value>>" + valueChar + "";
            }

            if (string.IsNullOrEmpty(whereClause))
            {
                whereClause += openChar + lookupColumn + closeChar + " " + searchOperator + " " + value;
            }
            else
            {
                whereClause += "AND " + openChar + lookupColumn + closeChar + " " + searchOperator + " " + value;
            }

            return Json(string.Format(sql, whereClause));
        }

        public ActionResult TestLookup(LookupInfoModel lookupInfo, string text)
        {
            LookupProvider lookupProvider = new LookupProvider(Utilities.UserName,Utilities.Password);
            DataTable lookupData = lookupProvider.GetLookupData(lookupInfo, text);

            return PartialView("TestLookupData", lookupData);
        }

        private string BuildCommandTextString(LookupInfoModel lookupInfo)
        {
            var openChar = string.Empty;
            var closeChar = string.Empty;
            var valueChar = string.Empty;
            var sqlCommand = string.Empty;
            var mapFields = lookupInfo.FieldMappings.Where(p => !string.IsNullOrWhiteSpace(p.DataColumn)).ToList();

            string sqlSelect = string.Empty;

            switch (lookupInfo.ConnectionInfo.DatabaseType)
            {
                case (int)DatabaseType.MsSql:
                    sqlCommand = "SELECT TOP {0} {1} FROM [{2}] WHERE {3}";
                    openChar = "[";
                    closeChar = "]";
                    valueChar = "'";
                    break;
                case (int)DatabaseType.MySql:
                    sqlCommand = "SELECT {1} FROM {2} WHERE {3} LIMIT 0, {0}";
                    openChar = "`";
                    closeChar = "`";
                    valueChar = "\"";
                    break;
                case (int)DatabaseType.PostgreSql:
                    sqlCommand = "SELECT {1} FROM {2} WHERE {3} LIMIT {0}";
                    openChar = "\"";
                    closeChar = "\"";
                    valueChar = "'";
                    break;
                case (int)DatabaseType.Oracle:
                    sqlCommand = "SELECT {1} FROM {2} WHERE {3} ROWNUM <= {0}";
                    openChar = "\"";
                    closeChar = "\"";
                    valueChar = "\"";
                    break;
                case (int)DatabaseType.DB2:
                    sqlCommand = "SELECT {1} FROM {2} WHERE {3} FETCH FIRST {0} ROWS ONLY";
                    openChar = "\"";
                    closeChar = "\"";
                    valueChar = "\"";
                    break;
                default:
                    throw new NotSupportedException();
            }

            sqlSelect = mapFields.Aggregate(string.Empty, (current, map) => current + (openChar + map.DataColumn + closeChar + " AS " + openChar + map.Name + closeChar + ","));

            if (sqlSelect.EndsWith(","))
            {
                sqlSelect = sqlSelect.Substring(0, sqlSelect.Length - 1);
            }


            if (lookupInfo.MaxLookupRow != 0)
            {
                sqlCommand = string.Format(sqlCommand, lookupInfo.MaxLookupRow, sqlSelect, lookupInfo.SourceName, "{0}");
            }
            else
            {
                sqlCommand = string.Format(sqlCommand, "1000", sqlSelect, lookupInfo.SourceName, "{0}");
            }

            return sqlCommand;
        }

        private string BuildExecuteCommandString(LookupInfoModel lookupInfo)
        {
            string sql = string.Empty;
            string openChar = string.Empty;
            string closeChar = string.Empty;
            string paraList = string.Empty;

            switch (lookupInfo.ConnectionInfo.DatabaseType)
            {
                case (int)DatabaseType.MsSql:
                    openChar = "[";
                    closeChar = "]";
                    sql = "EXEC {3}{0}{4}.{3}{1}{4} {2}";// + LookupInfo.SourceName + " ";

                    foreach (ParameterModel para in lookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "int":
                            case "decimal":
                            case "tinyint":
                            case "bigint":
                            case "float":
                            case "bit":
                                paraList += para.ParameterName + "=" + para.ParameterValue + ",";
                                break;
                            case "char":
                            case "varchar":
                            case "nvarchar":
                            case "text":
                            case "ntext":
                            case "date":
                            case "datetime":
                                paraList += para.ParameterName + "='" + para.ParameterValue + "',";
                                break;
                        }

                    }

                    break;
                case (int)DatabaseType.MySql:
                    openChar = closeChar = "`";
                    sql = "CALL {3}{0}{4}.{3}{1}{4}({2})";
                    foreach (ParameterModel para in lookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "int":
                            case "decimal":
                            case "tinyint":
                            case "bigint":
                            case "float":
                            case "bit":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "varchar":
                            case "text":
                            case "longtext":
                            case "mediumtext":
                            case "tinytext":
                            case "date":
                            case "datetime":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }
                    }
                    break;
                case (int)DatabaseType.Oracle:
                    openChar = closeChar = "\"";
                    sql = "CALL {3}{0}{4}.{3}{1}{4}({2})";

                    foreach (ParameterModel para in lookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "number":
                            case "float":
                            case "long":
                            case "bigint":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "nchar":
                            case "varchar2":
                            case "nvarchar2":
                            case "text":
                            case "ntext":
                            case "date":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }

                    }
                    break;
                case (int)DatabaseType.PostgreSql:
                    openChar = closeChar = "\"";
                    sql = "SELECT * FROM [{0}].{3}{1}{4}({2})";

                    foreach (ParameterModel para in lookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "smallint":
                            case "numeric":
                            case "integer":
                            case "bigint":
                            case "float":
                            case "boolean":
                            case "real":
                            case "double precision":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "character varying":
                            case "text":
                            case "date":
                            case "datetime":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }
                    }

                    break;
                case (int)DatabaseType.DB2:
                    openChar = closeChar = "\"";
                    sql = "CALL {3}{0}{4}.{3}{1}{4}({2})";

                    foreach (ParameterModel para in lookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "numeric":
                            case "decimal":
                            case "integer":
                            case "float":
                            case "double":
                            case "smallint":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "varchar":
                            case "date":
                            case "time":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }

                    }
                    break;
                default:
                    break;
            }

            if (paraList.EndsWith(","))
            {
                paraList = paraList.Substring(0, paraList.Length - 1);
            }

            sql = string.Format(sql, lookupInfo.ConnectionInfo.Schema, lookupInfo.SourceName, paraList, openChar, closeChar);

            return sql;
        }

        public DocumentTypePermissionModel SetAllAttributeDocumentFalse()
        {
            DocumentTypePermissionModel docTypePermission = new DocumentTypePermissionModel();
            docTypePermission.Id = Guid.Empty;
            docTypePermission.AllowedAppendPage = false;
            docTypePermission.AllowedCapture = false;
            docTypePermission.AllowedDeletePage = false;
            docTypePermission.AllowedDownloadOffline = false;
            docTypePermission.AllowedEmailDocument = false;
            docTypePermission.AllowedExportFieldValue = false;
            docTypePermission.AllowedHideAllAnnotation = false;
            docTypePermission.AllowedReplacePage = false;
            docTypePermission.AllowedRotatePage = false;
            docTypePermission.AllowedSearch = false;
            docTypePermission.AllowedSeeRetrictedField = false;
            docTypePermission.AllowedUpdateFieldValue = false;
            docTypePermission.AlowedPrintDocument = false;         
            return docTypePermission;
        }

        public AnnotationPermissionModel SetAllAttributeAnnotationFalse()
        {
            AnnotationPermissionModel annotationPermission = new AnnotationPermissionModel();
            annotationPermission.Id = Guid.Empty;
            annotationPermission.AllowedAddHighlight = false;
            annotationPermission.AllowedAddRedaction = false;
            annotationPermission.AllowedAddText = false;
            annotationPermission.AllowedDeleteHighlight = false;
            annotationPermission.AllowedDeleteRedaction = false;
            annotationPermission.AllowedDeleteText = false;
            annotationPermission.AllowedHideRedaction = false;
            annotationPermission.AllowedSeeHighlight = false;
            annotationPermission.AllowedSeeText = false;
            return annotationPermission;

        }

        public AuditPermissionModel SetAllAttributeAuditFalse()
        {
            AuditPermissionModel auditPermission = new AuditPermissionModel();
            auditPermission.Id = Guid.Empty;
            auditPermission.AllowedAudit = false;
            auditPermission.AllowedDeleteLog = false;
            auditPermission.AllowedRestoreDocument = false;
            auditPermission.AllowedViewLog = false;
            auditPermission.AllowedViewReport = false;
            return auditPermission;
        }

        // show permissions properties
        public ActionResult ShowPermissionProperties(UserGroupModel userGroup, DocumentTypeModel docType)
        {
            PermissionProvider permissionProvider = new PermissionProvider(Utilities.UserName, Utilities.Password);
            ArchiveAdminModel archiveAdminModel = new ArchiveAdminModel();
            archiveAdminModel.DocTypePermission = permissionProvider.GetPermission(userGroup,docType);
            archiveAdminModel.AnnotationPermission = permissionProvider.GetAnnotationPermission(userGroup, docType);
            archiveAdminModel.AuditPermission = permissionProvider.GetAuditPermission(userGroup, docType);
            if (archiveAdminModel.DocTypePermission == null)
            {
                archiveAdminModel.DocTypePermission = SetAllAttributeDocumentFalse();
            }
            if (archiveAdminModel.AnnotationPermission == null)
            {
                archiveAdminModel.AnnotationPermission = SetAllAttributeAnnotationFalse();
            }
            if (archiveAdminModel.AuditPermission == null)
            {
                archiveAdminModel.AuditPermission = SetAllAttributeAuditFalse();
            }
            
            return PartialView(archiveAdminModel);
        }

        // show permission
        public ActionResult ShowPermission(string viewBy)
        {
            ArchiveAdminModel archiveAdminModel = new ArchiveAdminModel();
            PermissionProvider permissionProvider = new PermissionProvider(Utilities.UserName,Utilities.Password);            
            archiveAdminModel.ListDocTypeModel = permissionProvider.GetDocTypes();
            archiveAdminModel.ListUserGroup = permissionProvider.GetUserGroups();
            if (viewBy == "bycontent")
            {
                return PartialView(archiveAdminModel);
            }
            else
            {
                return PartialView("ShowPermissionByUserGroup",archiveAdminModel);
            }
           
        }

        //savePermission 
        public void SavePermissions(DocumentTypePermissionModel permissionModel, AnnotationPermissionModel annotationPermissionModel, AuditPermissionModel auditPermissionModel)
        {
            PermissionProvider permissionProvider = new PermissionProvider(Utilities.UserName,Utilities.Password);
            permissionProvider.SavePermission(permissionModel, annotationPermissionModel, auditPermissionModel);        
                       
        }

        // show user 
        public ActionResult ShowUser()
        {
            UserProvider userProvider = new UserProvider(Utilities.UserName, Utilities.Password);
            List<UserModel> usersList = new List<UserModel>();
            usersList = userProvider.GetUsers();
            foreach(var user in usersList){
                if (user.Picture != null)
                {
                    var imageBinary = user.Picture;
                    var keyCache = user.Id.ToString();
                    System.Web.HttpContext.Current.Cache.Add(keyCache,
                        imageBinary, null, DateTime.Now.AddMinutes(15),
                        System.Web.Caching.Cache.NoSlidingExpiration,
                        System.Web.Caching.CacheItemPriority.Default, null);
                    ViewData["KEY_CACHE_PROFILE_PIC"] = keyCache;
                }
            }
            return PartialView(usersList);
        }
        
        // show user properties
        //Edit by Loc Ngo
        public ActionResult ShowUserProperties(Guid userID)
        {
            ArchiveAdminModel archiveAdminModel = new ArchiveAdminModel();
            LanguageProvider languageProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            archiveAdminModel.ListLanguages = languageProvider.GetLanguages();
            archiveAdminModel.ArchiveUserModel = null;
            //UserModel userModel = new UserModel();
            UserProvider userProvider = new UserProvider(Utilities.UserName, Utilities.Password);
            List<UserModel> listUsers = new List<UserModel>();
            listUsers = userProvider.GetUsers();


            foreach (var userItem in listUsers)
            {
                if (userItem.Id == userID)
                {
                    archiveAdminModel.ArchiveUserModel = userItem;
                    break;
                }
            }

            //Loc Ngo
            //Create cache to show profile picture
            if (userID != Guid.Empty)
            {
                if (archiveAdminModel.ArchiveUserModel.Picture != null)
                {
                    var imageBinary = archiveAdminModel.ArchiveUserModel.Picture;
                    var keyCache = Guid.NewGuid().ToString();
                    System.Web.HttpContext.Current.Cache.Add(keyCache,
                        imageBinary, null, DateTime.Now.AddMinutes(15),
                        System.Web.Caching.Cache.NoSlidingExpiration,
                        System.Web.Caching.CacheItemPriority.Default, null);
                    ViewData["KEY_CACHE_PROFILE_PIC"] = keyCache;
                }
            }
            //End    

            if (archiveAdminModel.ArchiveUserModel != null)
            {
                return PartialView(archiveAdminModel);
            }
            else
            {
                return PartialView("AddNewUser", archiveAdminModel);
            }
        }

        //Edit by Loc Ngo
        

        //get profile piture
        public FileResult GetImageFromCacheByKey(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                var imageBinary = (byte[])System.Web.HttpContext.Current.Cache[key];
                return File(imageBinary, ContentTypeEnumeration.Image.PNG);
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

        //Save User
        public Guid SaveUser(UserModel userModel, String keyCache) 
        {
            byte[] binaryPic;
            Guid result = Guid.Empty;
            
            UserProvider userProvider = new UserProvider(Utilities.UserName, Utilities.Password);
            List<UserModel> lstUserModel = userProvider.GetUsers();
            foreach (var item in lstUserModel)
            {
                if ((item.Username.Equals(userModel.Username) || userModel.Username.Equals(Utilities.UserName))
                    &&userModel.Id.Equals(Guid.Empty))
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
                userModel.Picture = binaryPic;
            }
                            
            userProvider.SaveUser(userModel);
            if (userModel.Id.Equals(Guid.Empty))
            {
                lstUserModel = userProvider.GetUsers();
                foreach (var item in lstUserModel)
                {
                    if (item.Username.Equals(userModel.Username))
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
        public void DeleteUser(UserModel userModel)
        {
            UserProvider userProvider = new UserProvider(Utilities.UserName,Utilities.Password);
            if (userModel.Id != Guid.Empty)
            {
                userProvider.DeleteUser(userModel);
            }
        }

        //show usergroup list (of user function)
        public ActionResult ShowListUserGroup(UserModel userModel)
        {
            UserGroupProvider userGroupProvider = new UserGroupProvider(Utilities.UserName,Utilities.Password);
            List<UserGroupModel> listUserGroup = new List<UserGroupModel>();
            List<UserGroupModel> listUserGroupResult = new List<UserGroupModel>();
            listUserGroup = userGroupProvider.GetUserGroups();
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
            return PartialView(listUserGroupResult);
         

        }

        //show usergroup
        public ActionResult LoadUserGroups()
        {
            UserGroupProvider userGroupProvider = new UserGroupProvider(Utilities.UserName, Utilities.Password);
            List<UserGroupModel> listUseGroups = new List<UserGroupModel>();
            listUseGroups = userGroupProvider.GetUserGroups();
            return PartialView(listUseGroups);
                 
        }

        //Delete UserGroup
        public void DeleteUserGroup(UserGroupModel userGroupModel)
        {
            UserGroupProvider userGroupProvider = new UserGroupProvider(Utilities.UserName,Utilities.Password);
            if (userGroupModel.Id != Guid.Empty)
            {
                userGroupProvider.DeleteUserGroup(userGroupModel);
            }

        }

        //show usergroup properties
        public ActionResult ShowUserGroupProperties(UserGroupModel userGroupModel)
        {
            UserGroupProvider userGroupProvider = new UserGroupProvider(Utilities.UserName, Utilities.Password);
            List<UserGroupModel> listUserGroupModel = new List<UserGroupModel>();
            listUserGroupModel = userGroupProvider.GetUserGroups();
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
                return PartialView("ShowUserGroupProperties", userGroupModel);
            }
            else
            {
                return PartialView("AddUserGroup", userGroupModel);
            }          
        }

        // save user group
        public Guid SaveUserGroup(UserGroupModel userGruopModel)
        {
            Guid result = Guid.Empty;
            UserGroupProvider userGroupProvider = new UserGroupProvider(Utilities.UserName, Utilities.Password);
            List<UserGroupModel> lstUserGroupModel = userGroupProvider.GetUserGroups();
            //userGruopModel.Name = System.Text.RegularExpressions.Regex.Replace(userGruopModel.Name, @"\s+", " ").Trim();
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

        //search user to add into usergroup
        public ActionResult SearchUser(UserGroupModel userModel)
        {
            UserProvider userProvider = new UserProvider(Utilities.UserName, Utilities.Password);
            List<UserModel> listUser = new List<UserModel>();
            List<UserModel> listUserResult = new List<UserModel>();
            listUser = userProvider.GetUsers();
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
            return PartialView(listUserResult);
        }

        // show setting 
        public ActionResult ShowSettings()
        {
            SettingProvider settingProvider = new SettingProvider(Utilities.UserName, Utilities.Password);
            SettingModel settinghModel = settingProvider.GetSettings();
            return PartialView(settinghModel);
        }

        //Save Setting
        public void SaveSetting(SettingModel settingModel)
        {
            SettingProvider settingProvider = new SettingProvider(Utilities.UserName, Utilities.Password);
            settingProvider.WriteSetting(settingModel);
        }

        //Show all language of database
        public ActionResult ShowSettingConfigureLanguages()
        {
            LanguageProvider languageProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            List<LanguageModel> lstLanguages = new List<LanguageModel>();
            lstLanguages = languageProvider.GetLanguages();
            return PartialView("ShowSettingConfigureLanguages", lstLanguages);
        }

        //Show setting configure by language id
        public ActionResult ShowSettingConfigure(Guid languageId)
        {
            AmbiguousDefinitionProvider ambiguousDefinitionProvider = new AmbiguousDefinitionProvider(Utilities.UserName, Utilities.Password);
            List<AmbiguousDefinitionModel> lstAmbiguousDefinitionModel = new List<AmbiguousDefinitionModel>();
            lstAmbiguousDefinitionModel = ambiguousDefinitionProvider.GetAmbiguousDefinitions(languageId);
            return PartialView(lstAmbiguousDefinitionModel);
        }

        //Add & Update new mapping for setting configure by Id
        public ActionResult ShowSettingConfigureDetail(Guid Id)
        {
            //LanguageProvider languageProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            AmbiguousDefinitionProvider ambiguousDefinitionProvider = new AmbiguousDefinitionProvider(Utilities.UserName, Utilities.Password);
            List<AmbiguousDefinitionModel> lstAmbiguous = ambiguousDefinitionProvider.GetAllAmbiguousDefinitions();
            AmbiguousDefinitionModel result = new AmbiguousDefinitionModel();
            if (Id != Guid.Empty)
            {
                foreach(var item in lstAmbiguous){
                    if (item.Id == Id)
                    {
                        result = item;
                        break;
                    }
                }
            }

            if (Id != Guid.Empty)
            {
                return PartialView(result);
            }
            else
            {
                return PartialView("AddNewSettingConfigureDetails");
            }
        }

        //Save setting configure Mapping into database
        public Guid SaveSettingConfigureMapping(AmbiguousDefinitionModel ambiguousDefinitionModel)
        {
            LanguageProvider languageProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            LanguageModel languageModel = languageProvider.GetLanguage(ambiguousDefinitionModel.LanguageId);
            Guid result = Guid.Empty;
            //set language
            ambiguousDefinitionModel.Language = languageModel;

            AmbiguousDefinitionProvider ambiguousDefinitionProvider = new AmbiguousDefinitionProvider(Utilities.UserName, Utilities.Password);

            //save into database
            ambiguousDefinitionProvider.Save(ambiguousDefinitionModel);
            List<AmbiguousDefinitionModel> lstMapping = ambiguousDefinitionProvider.GetAmbiguousDefinitions(ambiguousDefinitionModel.LanguageId);
            foreach (var item in lstMapping)
            {
                if (item.Text.Equals(ambiguousDefinitionModel.Text))
                {
                    result = item.Id;
                    break;
                }
            }
            return result;
        }

        //Delete setting configure Mapping from database
        public void DeleteSettingConfigureMapping(Guid Id)
        {
            AmbiguousDefinitionProvider ambiguousDefinitionProvider = new AmbiguousDefinitionProvider(Utilities.UserName, Utilities.Password);
            ambiguousDefinitionProvider.Delete(Id);
        }

        ///THODINH: Configure
        ///DATE: 25/11
        [HttpGet]
        public ActionResult Configure(Guid id)
        {
            LanguageProvider languageProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            List<LanguageModel> languageModel = languageProvider.GetLanguages();
            ViewData["Languages"] = languageModel;

            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentTypeModel docTypeModel = new DocumentTypeModel();
            docTypeModel = docTypeProvider.GetDocumentType(id);

            return PartialView("Configure", docTypeModel);
        }

        public JsonResult GetFieldMetaData(Guid docTypeId)
        {
            try
            {
                DocumentTypeProvider p = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
                DocumentTypeModel d = p.GetDocumentType(docTypeId);
                Dictionary<string, string> fields = new Dictionary<string, string>();
                foreach (var f in d.Fields)
                {
                    if (!f.IsSystemField)
                    {
                        fields.Add(f.Id.ToString(), f.Name);
                    }
                }
                return Json(fields);
            }
            catch (Exception e) { return Json(Constant.TimeOut); }
        }

        public ActionResult SaveOCRTemplate(OCRTemplateSerializble ocrTemplate)
        {

            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentTypeModel docType = new DocumentTypeModel();

            ///Language : hard code, remember to fix
            LanguageProvider langProvider = new LanguageProvider(Utilities.UserName, Utilities.Password);
            docType = docTypeProvider.GetDocumentType(ocrTemplate.DocTypeId);
            if (docType.OCRTemplate != null)
                docType.OCRTemplate.OCRTemplatePages.Clear();
            else
                docType.OCRTemplate = new OCRTemplateModel();
            docType.OCRTemplate.DocTypeId = ocrTemplate.DocTypeId;

            if (ocrTemplate.LangId != Guid.Empty)
                docType.OCRTemplate.Language = langProvider.GetLanguage(ocrTemplate.LangId);

            foreach (var ocrPage in ocrTemplate.OCRTemplatePages)
            {
                OCRTemplatePageModel page = new OCRTemplatePageModel();
                page.PageIndex = ocrPage.PageIndex;
                page.OCRTemplateZones = new List<OCRTemplateZoneModel>();
                var img = ((CacheImage)System.Web.HttpContext.Current.Cache[ocrPage.Key]);
                page.Binary = img.FileBinaries;
                page.DPI = img.Resolution;
                page.OCRTemplateId = ocrTemplate.DocTypeId;
                page.FileExtension = ocrPage.FileExtension;

                if (ocrPage.OCRTemplateZone != null)
                {
                    foreach (var ocrZone in ocrPage.OCRTemplateZone)
                    {
                        OCRTemplateZoneModel zone = new OCRTemplateZoneModel();
                        zone.ModifiedBy = zone.CreatedBy = Utilities.UserName;
                        zone.ModifiedOn = zone.CreatedOn = DateTime.Now;
                        zone.Left = ocrZone.Left * 96 / page.DPI;
                        zone.Top = ocrZone.Top * 96 / page.DPI;
                        zone.Width = ocrZone.Width * 96 / page.DPI;
                        zone.Height = ocrZone.Height * 96 / page.DPI;
                        zone.FieldMetaData = new FieldMetaDataModel()
                        {
                            Id = ocrZone.FieldMetaDataId
                        };
                        zone.FieldMetaDataId = ocrZone.FieldMetaDataId;

                        page.OCRTemplateZones.Add(zone);
                    }
                }

                docType.OCRTemplate.OCRTemplatePages.Add(page);
            }

            try
            {
                docTypeProvider.SaveOCRTemplate(docType.OCRTemplate);
                return Json(true);
            }
            catch { }
            return Json(false);

        }
       
    }
}
