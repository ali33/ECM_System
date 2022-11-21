using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArchiveMVC.Utility;
using ArchiveMVC.Models;

using Ecm.Domain;
using System.Collections;
using ArchiveMVC.Models.DataProvider;
using System.Web.Security;
//using ArchiveMVC.Models;

namespace ArchiveMVC.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/
        public ActionResult Index()
        {
            Utilities.OpenningDocument = null;
            SearchModel model = new SearchModel();
            SearchQueryProvider searchQueryProvider = new SearchQueryProvider(Utilities.UserName, Utilities.Password);

            IList<SearchQueryModel> queryModels = new List<SearchQueryModel>();


            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            DocumentTypeProvider documentTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            documentTypeProvider.Configure((string)Utilities.GetSession(Constant.UserName), (string)Utilities.GetSession(Constant.Password));
            IList<DocumentTypeModel> documentTypeModelList = new List<DocumentTypeModel>();
            //IList<FieldMetaDataModel> fieldlist = new List<FieldMetaDataModel>();

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
                return RedirectToAction(Constant.ACTION_INDEX,
                        Constant.CONTROLLER_LOGIN,
                        new LoginModel
                        {
                            ReturnUrl = Request.Url.AbsoluteUri
                        });
            }

            //Cache
            string str;
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

                /*foreach (SearchQueryExpressionModel itemexpress in item.SearchQueryExpressions)
                {
                        listexpress.Add(String.Format(@"{{
                                    
                                QueryID:{0},
                                ID:{1},                                
                                Condition:'{2}',
                                Operator:'{3}',
                                Value1:'{4}',
                                Value2:'{5}',
                                Name:'{6}',
                                IDField:{7},
                                DataType:'{8}',
                                DisplayOrder:{9}
                           
                                             }}", itemexpress.SearchQueryId,itemexpress.Id,itemexpress.Condition,itemexpress.Operator,itemexpress.Value1,itemexpress.Value2,itemexpress.Field.Name,itemexpress.Field.Id,itemexpress.Field.DataType,itemexpress.Field.DisplayOrder));

                }*/
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
            //model.ExpressionsFirst = "{Expression:[" + String.Join(",\n", listexpress) + "]}";
            // model.QueryModel = SearchQuery.GetSavedQueries(model.DocTypeIDFirst);//lay cac query cho DocType dau tien
            model.DataTypeJson = GeneratorJson.CreateDataType();
            model.DataTypeItemJson = GeneratorJson.CreateDataType_Item();
            model.ConjunctionJson = GeneratorJson.CreateConjunction();
            return View(model);
            //return View();
        }
        // save query    
       
       
        public JsonResult SaveQuery(SearchQueryModel queryname)
        {
            queryname.UserId = (Guid)System.Web.HttpContext.Current.Session["UserID"];
           //doi voi cac systemfield  thi dieu chinh ID = -1
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
        // dung de tao UniqueId cho cac SystemField
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
           /* SearchQueryProvider _searchqueryprovider = new SearchQueryProvider();
            IList<SearchQueryModel> _listquerymodel = new List<SearchQueryModel>();
            _listquerymodel = _searchqueryprovider.GetSavedQueries(DocID);
            return Json(_listquerymodel);*/
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            IList<SearchQueryModel> _ListQueryModel = new List<SearchQueryModel>();

            List<string> listname = new List<string>();
            List<string> listexpress = new List<string>();
            _ListQueryModel = _SearchQueryProvider.GetSavedQueries(DocID);
            /*_ListQueryModel.Add(new SearchQueryModel()
            {
                Id = 1,
                Name = "Test",
                DocTypeId = 1,
                SearchQueryExpressions = new List<SearchQueryExpressionModel>().Add(new SearchQueryExpressionModel { Id = 1, Condition = SearchConjunction.And, Operator = SearchOperator.Equal, Field = new FieldMetaDataModel() {Id=1 },Value1= "Chua nhap",Value2="" });
            });*/
            ArrayList list = new ArrayList();
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

                /*foreach (SearchQueryExpressionModel itemexpress in item.SearchQueryExpressions)
                {
                        listexpress.Add(String.Format(@"{{
                                    
                                QueryID:{0},
                                ID:{1},                                
                                Condition:""{2}"",
                                Operator:""{3}"",
                                Value1:""{4}"",
                                Value2:""{5}"",
                                Name:""{6}"",
                                IDField:{7},
                                DataType:""{8}"",
                                DisplayOrder:{9}
                           
                                             }}", itemexpress.SearchQueryId,itemexpress.Id,itemexpress.Condition,itemexpress.Operator,itemexpress.Value1,itemexpress.Value2,itemexpress.Field.Name,itemexpress.Field.Id,itemexpress.Field.DataType,itemexpress.Field.DisplayOrder));

                }*/
                listname.Add(String.Format(@"{{
                  ""QueryID"":""{0}"",
                  ""QueryName"":""{1}"",
                  ""ID"":""{2}"",
                  ""Name"":""{3}"",
                  ""Fields"":[{4}]
                   
                }}", item.Id, item.Name, item.DocTypeId, item.DocumentType.Name, expression));
            }
            //string QueryNameFirst = "{QueryNames:["+String.Join(",\n",listname)+"]}";
            string QueryExpression = "{\"QueryNames\":[" + String.Join(",\n", listname) + "]}";
           // return Json(QueryExpression);

            return Json(new { QueryNames = list });
        }   
        //
        // load query name 
        public JsonResult LoadQueryName(long QueryID)
        {
            SearchQueryProvider _searchqueryprovider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchQueryModel _searchquerymodel = new SearchQueryModel();
            


            return Json("id");
        }


        // POST: /Search/RunSearch
        
        public ActionResult RunSearch(String keyword)
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            List<SearchResultModel> rs = searchProvider.RunGlobalSearch(keyword, 0);
            return View(rs);
        }

        public ActionResult CreateSearchQueries(SearchModel model)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);

            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            //load l?i DS và Cache d? li?u
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
            //load l?i DS và Cache d? li?u
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
            return PartialView("AddMoreCondition", model);
        }

        public ActionResult CreateOperator(SearchModel model)
        {
            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);

            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            //load l?i DS và Cache d? li?u
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
            return PartialView("CreateOperator", model);

        }

        public ActionResult CreateTextBoxValue(SearchModel model)
        {

            SearchQueryProvider _SearchQueryProvider = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = _SearchQueryProvider.GetSavedQueries(model.DocumentTypeId);            

            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            //load l?i DS và Cache d? li?u
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


            return PartialView("CreateTextBoxValue", model);

        }

        public ActionResult CreateAdvanceSearchFromQuery(SearchModel model)
        {
            SearchQueryProvider d = new SearchQueryProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            model.ListQueryModel = d.GetSavedQueries(model.DocumentTypeId);
            // add by Triet, use to set value of SystemField
            IList<DocumentTypeModel> obj = (IList<DocumentTypeModel>)System.Web.HttpContext.Current.Cache[model.CacheDocType];
            
            //load l?i DS và Cache d? li?u
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

        public ActionResult RunSearchDocumentType(Guid docType, int pageIndex = 0)
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);

            SearchResultModel rs = 
                searchProvider.RunAdvanceSearch(pageIndex, 
                                                docType, 
                                                new SearchQueryModel() { 
                                                    SearchQueryExpressions = 
                                                        new List<SearchQueryExpressionModel>() 
                                                });
            var list = new ArrayList();
            //if(rs!=null)
            //    list.Add(new
            //    {
            //        GlobalSearchText = rs.GlobalSearchText,
            //        DocumentTypeID = rs.DocumentType.Id,

            //        ResultCount = rs.ResultCount,
            //        DocumentTypeName = rs.DocumentTypeName,
            //        Data = GeneratorJson.GetJson(rs.DataResult),
            //        HasMoreResult = rs.HasMoreResult,
            //        TotalCount = rs.TotalCount
            //    });
            //return Json(list);
            if (pageIndex > 0)
                return PartialView("RowsSearchResult", rs);
            return PartialView("RunSearchDocumentType", rs);
        }

        public ActionResult RunAdvanceSearch(SearchQueryModel queryname, int pageIndex = 0)
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchResultModel rs = searchProvider.RunAdvanceSearch(pageIndex, queryname.DocTypeId, queryname);

            if (pageIndex > 0)
            {
                return PartialView("RowsSearchResult", rs);
            }

            return PartialView("RunAdvanceSearch", rs);
        }

        public ActionResult RunContentSearch(SearchModel searchModel)
        {
            
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            SearchResultModel rs = searchProvider.RunContentSearch(0, searchModel.DocumentTypeId, searchModel.ContentSearch);
            
           return PartialView("RunAdvanceSearch", rs);
        }

        public ActionResult RunGlobalSearch(String keyword, int pageIndex = 0)
        {
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            List<SearchResultModel> rs = searchProvider.RunGlobalSearch(keyword, pageIndex);
            if (rs == null)
                return null;

            /*var list = new ArrayList();
            foreach (SearchResultModel item in rs)
            {
                list.Add(new
                {
                    GlobalSearchText = item.GlobalSearchText,
                    DocumentTypeID = item.DocumentType.Id,

                    ResultCount = item.ResultCount,
                    DocumentTypeName = item.DocumentTypeName,
                    Data = GeneratorJson.GetJson(item.DataResult)
                });
            }*/
            return PartialView("RunGlobalSearch", rs);
        }
        
        /*
        public JsonResult RunGlobalSearch(String keyword)
        {
            SearchProvider searchProvider = new SearchProvider();
            List<SearchResultModel> rs = searchProvider.RunGlobalSearch(keyword, 0);
            if (rs == null)
                return null;
            
            var list = new ArrayList();
            foreach (SearchResultModel item in rs)
            {
                list.Add(new
                {
                    GlobalSearchText = item.GlobalSearchText,
                    DocumentTypeID = item.DocumentType.Id,

                    ResultCount = item.ResultCount,
                    DocumentTypeName = item.DocumentTypeName,
                    Data = GeneratorJson.GetJson(item.DataResult)
                });
            }
            return Json(list);
        }*/
        public ActionResult SearchByContent(String keyword, long docTypeId)
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            SearchProvider searchProvider = new SearchProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            //SearchResultModel rs = searchProvider.RunContentSearch(1, docTypeId, keyword);
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
            rs.HasMoreResult = false;
            rs.IsChecked = false;
            rs.IsGlobalSearch = false;
            rs.IsSelected = true;
            rs.PageIndex = 1;
            rs.ResultCount = 1;
            rs.SearchQuery = new SearchQueryModel();
            rs.TotalCount = 1;
            return View(rs);
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
