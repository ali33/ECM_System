using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArchiveMVC5.Utility;
using ArchiveMVC5.Models;

using Ecm.Domain;
using System.Collections;
using ArchiveMVC5.Models.DataProvider;
using System.Web.Security;
using System.Configuration;
using System.Text;
using Ecm.Utility;
using System.Net.Mail;
//using ArchiveMVC5.Models;

namespace ArchiveMVC5.Controllers
{
    public class SearchController : BaseController
    {
        //
        // GET: /Search/
        public ActionResult Index()
        {
            Utilities.OpenningDocument = null;
            SearchModel model = new SearchModel();
            SearchQueryProvider searchQueryProvider = new SearchQueryProvider(Utilities.UserName, Utilities.Password);
            IList<SearchQueryModel> queryModels = new List<SearchQueryModel>();
            DocumentTypeProvider documentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            IList<DocumentTypeModel> documentTypeModelList = new List<DocumentTypeModel>();

            documentTypeProvider.Configure((string)Utilities.GetSession(Constant.UserName), (string)Utilities.GetSession(Constant.Password));
            try
            {
                documentTypeModelList = documentTypeProvider.GetDocumentTypes();

                foreach (var docType in documentTypeModelList)
                {
                    docType.Fields = new List<FieldMetaDataModel>(docType.Fields.Where(p => p.DataType != FieldDataType.Folder && p.DataType != FieldDataType.Table));
                }
            }
            catch (ArgumentNullException e)
            {
                ExceptionLog(e, e.Message);
                return RedirectToAction(Constant.ACTION_INDEX,
                        Constant.CONTROLLER_LOGIN,
                        new LoginModel
                        {
                            ReturnUrl = Request.Url.AbsoluteUri
                        });
            }

            //Cache
            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var cPri = System.Web.Caching.CacheItemPriority.Normal;

            model.CacheDocType = Guid.NewGuid().ToString();
            System.Web.HttpContext.Current.Cache.Add(model.CacheDocType, documentTypeModelList, null, cTime, cExp, cPri, null);
            //Cache

            string id = "";

            string cacheKey = "CaptureControler_GetDocType";
            object returnList = System.Web.HttpContext.Current.Cache[cacheKey];
            List<DocTypeResult> ListDocTypeCache = new List<DocTypeResult>();
            if (returnList != null)
            {
                ListDocTypeCache = (List<DocTypeResult>)returnList;
            }

            bool isUpdateCache = false;

            List<string> listTemp = new List<string>();
            foreach (DocumentTypeModel doc in documentTypeModelList)
            {
                if (ListDocTypeCache.Where(p => p.DocType.Id == doc.Id).Count() > 0)
                {
                    id = ListDocTypeCache.Where(p => p.DocType.Id == doc.Id).FirstOrDefault().IconKey;
                }
                else
                {
                    isUpdateCache = true; //có c?p nh?t cache ds Doctype
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
                model.listDocumentTypeModel.Add(id, doc);

                var fields = string.Join(",", doc.Fields.Where(f => f.DataType != FieldDataType.Table).Select(p =>
                                                            String.Format(@"{{
                  Name:'{0}',
                  IDField:'{1}',
                  DataType:'{2}',
                  DisplayOrder:{3},
                  FieldUniqueId:'{4}',
                  IsSystemField:'{5}'
                }}", p.Name, p.Id, p.DataType, p.DisplayOrder, p.FieldUniqueId, p.IsSystemField)).ToArray());
                listTemp.Add(String.Format(@"{{
                      ID:'{0}',
                      Name:'{1}',
                      Fields:[{2}]
                }}", doc.Id, doc.Name, fields));

            }

            if (isUpdateCache)
            {
                if (System.Web.HttpContext.Current.Cache[cacheKey] != null)
                    System.Web.HttpContext.Current.Cache[cacheKey] = ListDocTypeCache;
                else System.Web.HttpContext.Current.Cache.Add(cacheKey, ListDocTypeCache, null, cTime, cExp, cPri, null);
            }
            List<string> listname = new List<string>();
            List<string> listexpress = new List<string>();
            if (documentTypeModelList.Count > 0)
                queryModels = searchQueryProvider.GetSavedQueries(documentTypeModelList.First().Id);
            foreach (SearchQueryModel item in queryModels)
            {
                var expression = string.Join(",", item.SearchQueryExpressions.Select(p => String.Format(@"{{
                                QueryID:'{0}',
                                ID:'{1}',                             
                                Condition:'{2}',
                                Operator:'{3}',
                                Value1:'{4}',
                                Value2:'{5}',
                                FieldId:'{6}',
                                Name:'{7}',
                                IDField:'{8}',
                                DataType:'{9}',
                                DisplayOrder:{10}                        
                        
                }}", p.SearchQueryId, p.Id, p.Condition, p.Operator, p.Value1, p.Value2, p.Field == null ? Guid.Empty : p.Field.Id, p.Field == null ? "Empty" : p.Field.Name, p.Field == null ? Guid.Empty : p.Field.Id, p.Field == null ? "Empty" : p.Field.DataType.ToString(), p.Field == null ? -1 : p.Field.DisplayOrder)).ToArray());

                listname.Add(String.Format(@"{{
                  QueryID:'{0}',
                  QueryName:'{1}',
                  ID:'{2}',
                  Name:'{3}',
                  Expressions:[{4}]
                   
                }}", item.Id, item.Name, item.DocTypeId, item.DocumentType.Name, expression));
            }

            model.DocTypeJson = "{DocTypes:[" + String.Join(",\n", listTemp) + "]}";

            if (documentTypeModelList.Count > 0)
            {
                model.DocTypeIDFirst = documentTypeModelList.First().Id;
            }

            model.QueryNameFirst = "{QueryNames:[" + String.Join(",\n", listname) + "]}";
            model.DataTypeJson = GeneratorJson.CreateDataType();
            model.DataTypeItemJson = GeneratorJson.CreateDataType_Item();
            model.ConjunctionJson = GeneratorJson.CreateConjunction();
            return View(model);
        }
        // save query    
       
       
        public JsonResult SaveQuery(SearchQueryModel queryname)
        {
            queryname.UserId = (Guid)System.Web.HttpContext.Current.Session["UserID"];
            foreach (var item in queryname.SearchQueryExpressions)
            {
                if (item.Field.IsSystemField)
                {
                    item.Field.Id = Guid.Empty;
                    item.FieldUniqueId = GetSystemFieldUniqueId(item.Field.Name);
                }

            }

            SearchQueryProvider _searchprovider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);     

            _searchprovider.SaveQuery(queryname);
            return Json(queryname);

        }

        public string GetSystemFieldUniqueId(string name)
        {
            string result="" ;
            switch (name)
            {
                case "Modified on":
                    result = Common.DOCUMENT_MODIFIED_DATE;
                    break;
                case "Modified by":
                    result = Common.DOCUMENT_MODIFIED_BY;
                    break;
                case "Created by":
                    result = Common.DOCUMENT_CREATED_BY;
                    break;
                case "Created on":
                    result =Common.DOCUMENT_CREATED_DATE;
                    break;
                case "Page count":
                    result = Common.DOCUMENT_PAGE_COUNT;
                    break;
            }
            return result;
                
        }
        // Xóa query
        public void DeteleQuery(Guid QueryID)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            _SearchQueryProvider.DeleteQuery(QueryID);
        }
        // ki?m tra query có t?n t?i chua
        public bool QueryExisted(Guid docTypeId, string name)
        {
            bool kq;
            SearchQueryProvider _searchqueryprovider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            kq= _searchqueryprovider.QueryExisted(docTypeId, name);
            return kq;
        }
        // lay cac query da duoc luu ra 
        public JsonResult GetSaveQueryName(Guid DocID)
        {           
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            IList<SearchQueryModel> _ListQueryModel = new List<SearchQueryModel>();
            ArrayList list = new ArrayList();
            List<string> listname = new List<string>();
            List<string> listexpress = new List<string>();

            _ListQueryModel = _SearchQueryProvider.GetSavedQueries(DocID);

            foreach (SearchQueryModel item in _ListQueryModel)
            {
                list.Add(new {
                    QueryID=item.Id,
                    QueryName=item.Name,
                    ID=item.DocTypeId,
                    Name=item.DocumentType.Name,
                    Fields = item.SearchQueryExpressions.Where(f=> f.Field.DataType != FieldDataType.Table).Select(p => new
                    { 
                    QueryID=p.SearchQueryId,
                    ID= p.Id,
                    Condition= p.Condition, 
                    Operator=p.Operator,
                    Value1= p.Value1,
                    Value2= p.Value2, 
                    FieldId=p.Field == null ? Guid.Empty : p.Field.Id,
                    Name= p.Field == null ? "Empty" : p.Field.Name,
                    IDField = p.Field == null ? Guid .Empty: p.Field.Id, 
                    DataType=p.Field == null ? "Empty" : p.Field.DataType.ToString(),
                    DisplayOrder =p.Field == null ? -1 : p.Field.DisplayOrder  
                    })
                });
                var expression = string.Join(",", item.SearchQueryExpressions.Select(p => String.Format(@"{{
                                ""QueryID"":""{0}"",
                                ""ID"":""{1}"",                             
                                ""Condition"":""{2}"",
                                ""Operator"":""{3}"",
                                ""Value1"":""{4}"",
                                ""Value2"":""{5}"",
                                ""FieldId"":""{6}"",
                                ""Name"":""{7}"",
                                ""IDField"":""{8}"",
                                ""DataType"":""{9}"",
                                ""DisplayOrder"":""{10}""                        
                        
                }}", p.SearchQueryId, p.Id, p.Condition, p.Operator, p.Value1, p.Value2, p.Field == null ? Guid.Empty : p.Field.Id, p.Field == null ? "Empty" : p.Field.Name, p.Field == null ? Guid.Empty: p.Field.Id, p.Field == null ? "Empty" : p.Field.DataType.ToString(), p.Field == null ? -1 : p.Field.DisplayOrder)).ToArray());

                listname.Add(String.Format(@"{{
                  ""QueryID"":""{0}"",
                  ""QueryName"":""{1}"",
                  ""ID"":""{2}"",
                  ""Name"":""{3}"",
                  ""Fields"":[{4}]
                   
                }}", item.Id, item.Name, item.DocTypeId, item.DocumentType.Name, expression));
            }

            string QueryExpression = "{\"QueryNames\":[" + String.Join(",\n", listname) + "]}";
            return Json(new { QueryNames = list });
        } 
  
        public JsonResult LoadQueryName(long QueryID)
        {
            SearchQueryProvider _searchqueryprovider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchQueryModel _searchquerymodel = new SearchQueryModel();
            return Json("id");
        }


        // POST: /Search/RunSearch
        
        public ActionResult RunSearch(String keyword)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            List<SearchResultModel> rs = searchProvider.RunGlobalSearch(keyword, 0, -1);
            return View(rs);
        }

        public JsonResult SendMail(SaveOption mailOptions)
        {
            var doc = mailOptions.Document;
            var b = ProcessImages.ConvertTo(doc, mailOptions.Format, mailOptions.Range, mailOptions.Pages);
            var fileName = Server.MapPath("~/Temp/") + Guid.NewGuid().ToString() + "." + mailOptions.Format;
            System.IO.File.WriteAllBytes(fileName, b);

            SmtpClient smtp = new SmtpClient();

            MailMessage mail = new MailMessage();
            Attachment attch = new Attachment(fileName);
            mail.Attachments.Add(attch);
            mail.IsBodyHtml = true;
            
            if (string.IsNullOrEmpty(mailOptions.Subject))
            {
                mail.Subject = "Archive Notification";
            }
            else
            {
                mail.Subject = mailOptions.Subject;
            }

            if (string.IsNullOrEmpty(mailOptions.Body))
            {
                mail.Body = "No reply this email";
            }
            else
            {
                mail.Body = mailOptions.Body;
            }

            if (mailOptions.MailTo != null)
            {
                var to = mailOptions.MailTo.Split(';');
                for (int i = 0; i < to.Length; i++)
                {
                    mail.To.Add(to[i]);
                }
            }

            if (mailOptions.CC != null)
            {
                var cc = mailOptions.CC.Split(';');
                for (int i = 0; i < cc.Length; i++)
                {
                    mail.CC.Add(cc[i]);
                }
            }

            if (mailOptions.BCC != null)
            {
                var bcc=mailOptions.BCC.Split(';');
                for (int i = 0; i < bcc.Length; i++)
                {
                    mail.Bcc.Add(bcc[i]);
                }
            }

            try
            {
                smtp.Send(mail);
            }
            catch(Exception ex) {
                ExceptionLog(ex, ex.Message);
                return Json("Failed to send your email!"); 
            }

            return Json("Your email has been send");

        }

        public ActionResult SaveLocal(SaveOption mailOptions)
        {
            try
            {
                var document = mailOptions.Document;
                var buffer = ProcessImages.ConvertTo(document, mailOptions.Format, mailOptions.Range, mailOptions.Pages);
                var key = Guid.NewGuid().ToString();
                CacheFilesBinary cache = new CacheFilesBinary
                {
                    FileBinaries = buffer,
                    ContentType = (mailOptions.Format.Equals("pdf") ? ArchiveMVC5.Utility.ContentTypeEnumeration.Document.PDF :
                                                          ArchiveMVC5.Utility.ContentTypeEnumeration.Image.TIFF),
                    OrginalFileName = key + "." + mailOptions.Format
                };
                System.Web.HttpContext.Current.Cache.Add(key,
                    cache, null, DateTime.Now.AddMinutes(15),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Default,
                    null);
                return Json(key);
            }
            catch (Exception ex)
            {
                ExceptionLog(ex, ex.Message);
            }

            return null;
        }

        public ActionResult OpenMail(SaveOption mailOptions)
        {
            return PartialView("ComposeMail", mailOptions);
        }

        public ActionResult CreateSearchConditionFromQueries(Guid queryId)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchQueryModel model = new SearchQueryModel();
            model = _SearchQueryProvider.GetSavedQuery(queryId);

            return PartialView("AdvanceSearchCondition", model);
        }

        public ActionResult CreateSearchQueries(SearchModel model)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);
            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            
            if (obj == null)
            {
                var cTime = DateTime.Now.AddMinutes(24 * 3600);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;


                var s = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
                obj = s.GetDocumentTypes();

                System.Web.HttpContext.Current.Cache.Add(model.CacheDocType, obj, null, cTime, cExp, cPri, null);
            }

            model.ListDocType = obj;
            return PartialView("SearchQueries",model);
        
        }
        
        public ActionResult AddMoreCondition(SearchModel model)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);
            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            
            if (obj == null)
            {
                var cTime = DateTime.Now.AddMinutes(24 * 3600);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;


                var s = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
                obj = s.GetDocumentTypes();

                System.Web.HttpContext.Current.Cache.Add(model.CacheDocType, obj, null, cTime, cExp, cPri, null);
            }


            model.ListDocType = obj;
            DocumentTypeModel docType = obj.FirstOrDefault(p => p.Id == model.DocumentTypeId);
            if (model.FieldID != Guid.Empty)
            {
                model.DataTypeJson = ((int)(docType.Fields.FirstOrDefault(i => i.Id == model.FieldID).DataType)).ToString();
            }
            else
            {
                model.DataTypeJson = ((int)(docType.Fields.FirstOrDefault().DataType)).ToString();
            }

            return PartialView("AddMoreCondition", model);
        }

        public ActionResult CreateOperator(SearchModel model)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);

            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            
            if (obj == null)
            {
                var cTime = DateTime.Now.AddMinutes(24 * 3600);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;


                var s = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
                obj = s.GetDocumentTypes();

                System.Web.HttpContext.Current.Cache.Add(model.CacheDocType, obj, null, cTime, cExp, cPri, null);
            }


            model.ListDocType = obj;

            DocumentTypeModel docType = obj.FirstOrDefault(p => p.Id == model.DocumentTypeId);
            if (model.FieldID != Guid.Empty)
            {
                model.DataTypeJson = ((int)(docType.Fields.FirstOrDefault(i => i.Id == model.FieldID).DataType)).ToString();
            }
            else
            {
                model.DataTypeJson = ((int)(docType.Fields.FirstOrDefault().DataType)).ToString();
            }

            return PartialView("CreateOperator", model);

        }

        public ActionResult CreateTextBoxValue(SearchModel model)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);            

            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
         
            if (obj == null)
            {
                var cTime = DateTime.Now.AddMinutes(24 * 3600);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;


                var s = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
                obj = s.GetDocumentTypes();

                System.Web.HttpContext.Current.Cache.Add(model.CacheDocType, obj, null, cTime, cExp, cPri, null);
            }

            model.ListDocType = obj;

            DocumentTypeModel docType = obj.FirstOrDefault(p => p.Id == model.DocumentTypeId);
            if (model.FieldID != Guid.Empty)
            {
                model.DataTypeJson = ((int)(docType.Fields.FirstOrDefault(i => i.Id == model.FieldID).DataType)).ToString();
            }
            else
            {
                model.DataTypeJson = ((int)(docType.Fields.FirstOrDefault().DataType)).ToString();
            }

            return PartialView("CreateTextBoxValue", model);

        }

        public ActionResult CreateAdvanceSearchFromQuery(SearchModel model)
        {
            SearchQueryProvider d = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = d.GetSavedQueries(model.DocumentTypeId);
            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            
            if (obj == null)
            {
                var cTime = DateTime.Now.AddMinutes(24 * 3600);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;


                var s = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
                obj = s.GetDocumentTypes();

                System.Web.HttpContext.Current.Cache.Add(model.CacheDocType, obj, null, cTime, cExp, cPri, null);
            }

            foreach (var item in model.ListQueryModel.Where(p => p.Id == model.QueryId).FirstOrDefault().SearchQueryExpressions.Select(p => p))
             {
                   if (item.Field == null)
                    {
                            string UniqueId = item.FieldUniqueId;
                            string[] name = UniqueId.Split('_');
                            string FieldName = name[0];
                
                            switch (FieldName)
                                    {
                                        case "CreatedDate":
                                            FieldName = "Created on";
                                            break;
                                        case "CreatedBy":
                                            FieldName = Common.COLUMN_CREATED_BY;
                                            break;
                                        case "ModifiedBy":
                                            FieldName = Common.COLUMN_MODIFIED_BY;
                                            break;
                                        case "ModifiedDate":
                                            FieldName = "Modified on";
                                            break;
                                        case "PageCount":
                                            FieldName = Common.COLUMN_PAGE_COUNT;
                                            break;
                                    }
                            FieldMetaDataModel fieldtemp = new FieldMetaDataModel();
                            foreach (var itemobj in obj)
                            {
                                if (itemobj.Id == model.DocumentTypeId)
                                {
                                    foreach (var itemfield in itemobj.Fields)
                                    {
                                        if (itemfield.Name == FieldName)
                                        {
                                            fieldtemp = itemfield;
                                            break;                                            
                                        }
                                    }
                                }
                            }
                            item.Field = fieldtemp;                         
                    }
           }

            return PartialView("CreateAdvanceSearchFromQuery", model);
        }

        public ActionResult RunSearchDocumentType(Guid docType, int pageIndex, int pageSize)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            var list = new ArrayList();
            SearchResultModel rs = null;

            try
            {
                rs = searchProvider.RunAdvanceSearch(pageIndex, pageSize,"","",
                                                docType,
                                                new SearchQueryModel()
                                                {
                                                    SearchQueryExpressions =
                                                        new List<SearchQueryExpressionModel>()
                                                });

                rs.Paging = new PagingModel { PageIndex = pageIndex, PageSize = pageSize, SortColumnName = string.Empty, SortDirection = string.Empty, TotalRows = rs.TotalCount };
            }
            catch(Exception ex)
            {
                ExceptionLog(ex, ex.Message);
                ViewData["SearchError"] = "Search content error";
            }

            return PartialView("RunAdvanceSearch", rs);
        }

        public ActionResult RunAdvanceSearch(SearchQueryModel queryname, PagingModel paging)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);

            List<FieldMetaDataModel> fields = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password).GetDocumentType(queryname.DocTypeId).Fields;

            FieldMetaDataModel sortField = null;
            string sortFieldId = string.Empty;

            if (!string.IsNullOrEmpty(paging.SortColumnName))
            {
                switch (paging.SortColumnName)
                {
                    case Common.COLUMN_PAGE_COUNT:
                        sortFieldId = Common.DOCUMENT_PAGE_COUNT;
                        break;
                    case Common.COLUMN_CREATED_BY:
                        sortFieldId = Common.DOCUMENT_CREATED_BY;
                        break;
                    case Common.COLUMN_CREATED_ON:
                        sortFieldId = Common.DOCUMENT_CREATED_DATE;
                        break;
                    case Common.COLUMN_MODIFIED_BY:
                        sortFieldId = Common.DOCUMENT_MODIFIED_BY;
                        break;
                    case Common.COLUMN_MODIFIED_ON:
                        sortFieldId = Common.DOCUMENT_MODIFIED_DATE;
                        break;
                    default:
                        sortField = fields.SingleOrDefault(p => p.Name == paging.SortColumnName);
                        sortFieldId = sortField == null ? "" : sortField.Id.ToString();
                        break;
                }
            }

            SearchResultModel rs = searchProvider.RunAdvanceSearch(paging.PageIndex, paging.PageSize, sortFieldId, paging.SortDirection, queryname.DocTypeId, queryname);

            rs.Paging = new PagingModel { PageIndex = paging.PageIndex, PageSize = paging.PageSize, TotalRows = rs.TotalCount, SortColumnName = paging.SortColumnName, SortDirection = paging.SortDirection };

            return PartialView("RunAdvanceSearch", rs);
        }

        public ActionResult RunContentSearch(SearchModel searchModel)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchResultModel rs = searchProvider.RunContentSearch(0, -1, searchModel.DocumentTypeId, searchModel.ContentSearch);
            
           return PartialView("RunAdvanceSearch", rs);
        }

        public ActionResult RunGlobalSearch(String keyword, int pageIndex = 0)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            List<SearchResultModel> rs = searchProvider.RunGlobalSearch(keyword, pageIndex, -1);
            if (rs == null)
                return null;

            return PartialView("RunGlobalSearch", rs);
        }

        public JsonResult Delete(Guid id)
        {
            DocumentProvider provider = new DocumentProvider(Utilities.UserName, Utilities.Password);
            try
            {
                provider.DeleteDocument(id);
            }
            catch (Exception ex)
            {
                ExceptionLog(ex, ex.Message);
                return Json(new JsonMessage()
                {
                    Code = MsgCode.Error,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new JsonMessage()
            {
                Code = MsgCode.Success,
                Message = "Delete document successful!"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchByContent(String keyword, long docTypeId)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchResultModel rs = new SearchResultModel();
            DataTable tbl = new DataTable("Test");
            DataColumn co0 = new DataColumn("Checked_4E19573E_D42E_4B74_BB81_E3EF56633947");
            DataColumn co1 =new DataColumn("Col");
            DataColumn co2 = new DataColumn("Col1");
            tbl.Columns.Add(co0);
            tbl.Columns.Add(co1);
            tbl.Columns.Add(co2);
            tbl.Rows.Add(new object[]{true,"asd","ád"});
            tbl.Rows.Add(new object[]{false,"asd","ád"});
            rs.DataResult = tbl;
            rs.DocumentType = new DocumentTypeModel();
            rs.DocumentTypeName = "Document Type Name";
            rs.GlobalSearchText = "";
            rs.IsGlobalSearch = false;
            rs.ResultCount = 1;
            rs.SearchQuery = new SearchQueryModel();
            rs.TotalCount = 1;
            return View(rs);
        }

        public JsonResult GetAdvanceSearchValues(Guid DocumentTypeId, Guid fieldId, Ecm.Domain.SearchOperator searchOperator, string value1, string value2)
        {
            DocumentTypeProvider pro = new DocumentTypeProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            DocumentTypeModel docType = pro.GetDocumentType(DocumentTypeId);
            FieldMetaDataModel field = docType.Fields.SingleOrDefault(p => p.Id == fieldId);

            return Json(GeneratorJson.GetComboBoxValue(field, searchOperator, value1, value2));
        }
    }
    [Serializable]
    public class Query
    {
        public Query()
       {
       }
        public string Name { get; set; }
                
        public List<Queryx> SearchQueryExpression { get; set; }

    }
   [Serializable]
    public class Queryx
    {
       public Queryx()
       {
       }
        public string Name { get; set; }       

    }
}
