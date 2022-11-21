using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CaptureMVC.Utility;
using CaptureMVC.Models;
using System.Collections.ObjectModel;
using System.Collections;
using System.Web.Security;
using Ecm.CaptureDomain;
using CaptureMVC.DataProvider;
using CaptureMVC.Resources;
using Newtonsoft.Json;

namespace CaptureMVC.Controllers
{
    public class SearchController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                // Get list user name
                var users = new UserProvider().GetAvailableUserToDelegation();
                var userNames = users.Where(h => h.UserName != Utilities.UserName)
                                     .Select(h => h.UserName)
                                     .OrderBy(h => h);
                ViewBag.UserNames = userNames.ToList();

                ViewBag.ServerUrl = Request.Url.AbsoluteUri.Remove(Request.Url.AbsoluteUri.Length - 7);

                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #region Partial methods

        #region Assigned Menu

        [HttpGet]
        public PartialViewResult _LoadAssignedMenu()
        {
            try
            {
                // Load assigned batch types
                var batchTypeProvider = new BatchTypeProvider();
                var batchTypes = batchTypeProvider.GetAssignWorkBatchTypes();

                var models = new List<AssignedMenuModel>();
                AssignedMenuModel modelItem;

                var workItemProvider = new WorkItemProvider();
                var menus = new List<AssignedMenuModel>();

                int errorBatch;
                int inprocessingBatch;
                int lockedBatch;
                int availableBatch;
                int rejectedBatch;

                AssignedMenuItemModel errorItem;
                AssignedMenuItemModel inProcessingItem;
                AssignedMenuItemModel lockedItem;
                AssignedMenuItemModel rejectedItem;
                AssignedMenuItemModel waitingItem;

                // Cache setting for icon of batch types
                var cTime = DateTime.Now.AddMinutes(24 * 3600);
                var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                var cPri = System.Web.Caching.CacheItemPriority.Normal;

                // Reset cache icon of batch types
                foreach (var batchType in batchTypes)
                {
                    modelItem = new AssignedMenuModel()
                    {
                        BatchTypeId = batchType.Id,
                        BatchTypeName = batchType.Name,
                        Items = new List<AssignedMenuItemModel>()
                    };

                    // Store icon if any
                    if (batchType.Icon != null)
                    {
                        modelItem.BatchTypeIcon = ProcessImages.GetStringImage(batchType.Icon);
                    }

                    // Count each kind of bath
                    workItemProvider.CountBatchs(batchType.Id, out errorBatch, out inprocessingBatch, out lockedBatch, out availableBatch, out rejectedBatch);

                    errorItem = new AssignedMenuItemModel()
                    {
                        Status = BatchStatus.Error,
                        Name = BatchStatusResources.Error,
                        Count = errorBatch
                    };
                    modelItem.Items.Add(errorItem);

                    inProcessingItem = new AssignedMenuItemModel()
                    {
                        Status = BatchStatus.InProcessing,
                        Name = BatchStatusResources.InProcessing,
                        Count = inprocessingBatch
                    };
                    modelItem.Items.Add(inProcessingItem);

                    lockedItem = new AssignedMenuItemModel()
                    {
                        Status = BatchStatus.Locked,
                        Name = BatchStatusResources.Locked,
                        Count = lockedBatch
                    };
                    modelItem.Items.Add(lockedItem);

                    rejectedItem = new AssignedMenuItemModel()
                    {
                        Status = BatchStatus.Reject,
                        Name = BatchStatusResources.Reject,
                        Count = rejectedBatch
                    };
                    modelItem.Items.Add(rejectedItem);

                    waitingItem = new AssignedMenuItemModel()
                    {
                        Status = BatchStatus.Available,
                        Name = BatchStatusResources.Available,
                        Count = availableBatch
                    };
                    modelItem.Items.Add(waitingItem);

                    models.Add(modelItem);
                }

                return PartialView("_AssignedMenuView", models);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - 2014/03/26 - need implement
                return null;
            }
        }

        [HttpGet]
        public JsonResult _GetCountStatus(Guid batchTypeId)
        {
            try
            {
                var modelItem = new AssignedMenuModel();
                var workItemProvider = new WorkItemProvider();

                int errorBatch;
                int inprocessingBatch;
                int lockedBatch;
                int availableBatch;
                int rejectedBatch;

                // Count each kind of bath
                workItemProvider.CountBatchs(batchTypeId, out errorBatch, out inprocessingBatch, out lockedBatch, out availableBatch, out rejectedBatch);

                var json = new
                {
                    Error = errorBatch,
                    InProccess = inprocessingBatch,
                    Locked = lockedBatch,
                    Rejected = rejectedBatch,
                    Available = availableBatch
                };

                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - 2014/03/26 - need implement
                return null;
            }
        }

        #endregion

        #region Advance Search

        [HttpGet]
        public PartialViewResult _LoadAdvancedSearch(Guid batchTypeId)
        {
            try
            {
                // Get saved queries
                var savedQueries = new SearchQueryProvider().GetSavedQueriesLight(batchTypeId);

                // Mapping model
                var models = ObjectMapper.GetSavedQueryModels(savedQueries);
                ViewBag.BatchTypeId = batchTypeId;

                return PartialView("_AdvancedSearchView", models);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - 2014/03/25 - need implement
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult _LoadDefaultCondition(Guid batchTypeId)
        {
            try
            {
                // Get list field
                var fields = new BatchFiledMetaDataProvider().GetFieldsFromBatchType(batchTypeId);

                // Get normal field sort by display order
                var normalFields = fields.Where(h => !h.IsSystemField).OrderBy(h => h.DisplayOrder);
                // Get normal field sort by field name
                var systemFields = fields.Where(h => h.IsSystemField).OrderBy(h => h.Name);

                // Mapping model
                var models = ObjectMapper.GetCondtionModels(normalFields.Union(systemFields));

                return PartialView("_DefaultConditionView", models);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - need implement
                return null;
            }
        }

        [HttpGet]
        public ActionResult _LoadSavedCondition(Guid savedQueryId)
        {
            try
            {
                // Get saved query
                var savedQuery = new SearchQueryProvider().GetSavedQuery(savedQueryId);
                List<ConditionModel> models;

                if (savedQuery != null)
                {
                    // Mapping model
                    models = ObjectMapper.GetCondtionModels(savedQuery.SearchQueryExpressions);
                }
                else
                {
                    models = new List<ConditionModel>();
                }

                return PartialView("_SavedConditionView", models);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - 2014/03/25 - need implement
                return null;
            }
        }

        [HttpPost]
        public Guid _SaveQuery(Guid queryId,
                               string queryName,
                               Guid batchTypeId,
                               List<SearchQueryExpression> searchQueryExpressions)
        {
            try
            {
                // Check parameter condition
                if (Guid.Empty == queryId)
                {
                    if (string.IsNullOrWhiteSpace(queryName))
                    {
                        // Save new query but have no name
                        return Guid.Empty;
                    }
                    else if (Guid.Empty == batchTypeId)
                    {
                        // Save new query but have no name
                        return Guid.Empty;
                    }

                }
                if (searchQueryExpressions == null || searchQueryExpressions.Count == 0)
                {
                    // Have no expression
                    return Guid.Empty;
                }
                if (!this.CheckSearchQueryExpression(searchQueryExpressions))
                {
                    // Check the search query expression is not correct
                    return Guid.Empty;
                }

                // Mapping object
                var searchQuery = ObjectMapper.GetSearchQuery(queryId, queryName, batchTypeId, searchQueryExpressions);

                if (Guid.Empty != searchQuery.Id)
                {
                    // Get saved query
                    var savedQuery = new SearchQueryProvider().GetSavedQuery(searchQuery.Id);
                    var oldGuids = savedQuery.SearchQueryExpressions.Select(h => h.Id).ToList();
                    var newGuids = searchQuery.SearchQueryExpressions.Select(h => h.Id).ToList();

                    searchQuery.DeletedSearchQueryExpressions = oldGuids.Except(newGuids).ToList();
                }

                return new SearchQueryProvider().SaveQuery(searchQuery);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - 2014/03/26 - need implement
                return Guid.Empty;
            }
        }

        [HttpPost]
        public bool _DeteleQuery(Guid queryId)
        {
            try
            {
                new SearchQueryProvider().DeleteQuery(queryId);
                return true;
            }
            catch (Exception ex)
            {
                //TODO: HungLe - 2014/03/26 - need implement
                return false;
            }
        }

        [HttpGet]
        public bool _IsQueryNameExisted(Guid batchTypeId, string queryName)
        {
            try
            {
                return new SearchQueryProvider().IsQueryNameExisted(batchTypeId, queryName);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Search Result

        [HttpGet]
        public PartialViewResult _GetBatches(Guid batchTypeId, BatchStatus status, int pageIndex)
        {
            try
            {
                var workItemProvider = new WorkItemProvider();
                var results = workItemProvider.GetBatchByStatus(batchTypeId, status,
                                                                pageIndex, Utilities.ItemsPerPage);
                // Mapping model
                var model = ObjectMapper.GetSearchResultModel(results);
                model.BatchTypeId = batchTypeId;
                model.BatchStatus = status;

                return PartialView("_SearchResultView", model);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - need implement
                return null;
            }
        }

        [HttpPost]
        public PartialViewResult _RunAdvancedSearch(Guid batchTypeId, string jsonSearchExpressions, int pageIndex)
        {
            try
            {
                var expressions = JsonConvert.DeserializeObject<List<SearchQueryExpression>>(jsonSearchExpressions);

                var workItemProvider = new WorkItemProvider();
                var searchQuery = ObjectMapper.GetSearchQuery(batchTypeId, expressions);

                var results = workItemProvider.GetBatchByAdvanceSearch(searchQuery, pageIndex);
                // Mapping model
                var model = ObjectMapper.GetSearchResultModel(results);
                model.JsonSearchExpressions = jsonSearchExpressions;
                model.BatchTypeId = batchTypeId;
                model.SearchByAdvance = true;

                return PartialView("_SearchResultView", model);
            }
            catch (Exception ex)
            {
                //TODO: HungLe - need implement
                return null;
            }
        }

        #endregion

        [HttpPost]
        public bool _UnlockBatches(List<Guid> batchIds)
        {
            try
            {
                new WorkItemProvider().UnLockWorkItems(batchIds);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        public bool _DeleteBatches(List<Guid> batchIds)
        {
            try
            {
                new WorkItemProvider().DeleteWorkItems(batchIds);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        public bool _ResumeBatches(List<Guid> batchIds)
        {
            try
            {
                new WorkItemProvider().ResumeWorkItems(batchIds);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        public bool _ApproveBatches(List<Guid> batchIds)
        {
            try
            {
                new WorkItemProvider().ApproveWorkItems(batchIds);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        public bool _RejectBatches(List<Guid> batchIds, string rejectedNote)
        {
            try
            {
                new WorkItemProvider().RejectWorkItems(batchIds, rejectedNote);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        public bool _DelegateBatches(List<Guid> batchIds, string toUser, string delegateNote)
        {
            try
            {
                new WorkItemProvider().DelegateWorkItems(batchIds, toUser, delegateNote);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Generate the items of each batch type
        /// </summary>
        /// <param name="batchTypes"></param>
        /// <returns>The batch types menu</returns>
        private List<AssignedMenuModel> PopulateMenu(IList<BatchType> batchTypes)
        {
            var workItemProvider = new WorkItemProvider();
            var menus = new List<AssignedMenuModel>();

            int errorBatch = 0;
            int inprocessingBatch = 0;
            int lockedBatch = 0;
            int availableBatch = 0;
            int rejectedBatch = 0;

            foreach (var batchType in batchTypes)
            {
                AssignedMenuModel menu = new AssignedMenuModel()
                {
                    BatchTypeId = batchType.Id,
                    BatchTypeName = batchType.Name,
                    Items = new List<AssignedMenuItemModel>()
                };

                // Count each kind of bath
                workItemProvider.CountBatchs(batchType.Id, out errorBatch, out inprocessingBatch, out lockedBatch, out availableBatch, out rejectedBatch);

                AssignedMenuItemModel errorItem = new AssignedMenuItemModel()
                {
                    Status = BatchStatus.Error,
                    Name = Common.BATCH_ERROR,
                    Count = errorBatch
                };
                menu.Items.Add(errorItem);

                AssignedMenuItemModel inProcessingItem = new AssignedMenuItemModel()
                {
                    Status = BatchStatus.InProcessing,
                    Name = Common.BATCH_IN_PROCESSING,
                    Count = inprocessingBatch
                };
                menu.Items.Add(inProcessingItem);

                AssignedMenuItemModel LockedItem = new AssignedMenuItemModel()
                {
                    Status = BatchStatus.Locked,
                    Name = Common.BATCH_LOCKED,
                    Count = lockedBatch
                };
                menu.Items.Add(LockedItem);

                AssignedMenuItemModel rejectedItem = new AssignedMenuItemModel()
                {
                    Status = BatchStatus.Reject,
                    Name = Common.BATCH_REJECTED,
                    Count = rejectedBatch
                };
                menu.Items.Add(rejectedItem);

                AssignedMenuItemModel waitingItem = new AssignedMenuItemModel()
                {
                    Status = BatchStatus.Available,
                    Name = Common.BATCH_WAITING,
                    Count = availableBatch
                };
                menu.Items.Add(waitingItem);

                menus.Add(menu);
            }

            return menus;
        }

        /// <summary>
        /// Check the search conjunction and search operator is valid.
        /// </summary>
        /// <param name="queries"></param>
        /// <returns></returns>
        private bool CheckSearchQueryExpression(List<SearchQueryExpression> queries)
        {
            var result = true;
            SearchConjunction searchConjunction;
            SearchOperator searchOperator;

            foreach (var item in queries)
            {
                // Check search conjunction
                if (!Enum.TryParse<SearchConjunction>(item.Condition, true, out searchConjunction))
                {
                    result = false;
                    break;
                }

                // Check search operator
                if (!Enum.TryParse<SearchOperator>(item.Operator, true, out searchOperator))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
