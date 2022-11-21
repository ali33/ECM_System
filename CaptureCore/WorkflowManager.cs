using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.Workflow.WorkflowController;

namespace Ecm.CaptureCore
{
    public class WorkflowManager : ManagerBase
    {
        public WorkflowManager(User loginUser) : base(loginUser)
        {
        }

        public WorkflowDefinition GetWorkflowByBatchTypeId(Guid batchTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save batch type.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new WorkflowDefinitionDao(dataContext).GetByBatchTypeId(batchTypeId);
            }
        }

        public WorkflowDefinition GetById(Guid workflowDefinitionId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                return new WorkflowDefinitionDao(dataContext).GetById(workflowDefinitionId);
            }
        }

        public void DeleteByBatchType(Guid batchTypeId)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save workflow definition.", LoginUser.UserName));
            }

            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                WorkflowDefinitionDao workflowDefinitionDao = new WorkflowDefinitionDao(dataContext);
                WorkflowHumanStepPermissionDao workflowHumanStepPermissionDao = new WorkflowHumanStepPermissionDao(dataContext);
                WorkflowHumanStepDocTypePermissionDao workflowHumanStepDocTypePermissionDao = new WorkflowHumanStepDocTypePermissionDao(dataContext);

                dataContext.BeginTransaction();

                try
                {
                    workflowHumanStepDocTypePermissionDao.DeleteByBatchType(batchTypeId);
                    workflowHumanStepPermissionDao.DeleteByBatchType(batchTypeId);
                    workflowDefinitionDao.DeleteByBatchType(batchTypeId);

                    dataContext.Commit();
                }
                catch (Exception)
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public Guid SaveWorkflowDefinition(Guid batchTypeId, WorkflowDefinition wfDefinition, List<CustomActivitySetting> customActivitySettings)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save workflow definition.", LoginUser.UserName));
            }

            wfDefinition.Id = Guid.NewGuid();
            wfDefinition.BatchTypeId = batchTypeId;

            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                BatchTypeDao batchTypeDao = new BatchTypeDao(dataContext);
                WorkflowDefinitionDao wfDefinitionDao = new WorkflowDefinitionDao(dataContext);
                WorkflowHumanStepPermissionDao wfHumanStepPermissionDao = new WorkflowHumanStepPermissionDao(dataContext);
                WorkflowHumanStepDocTypePermissionDao wfHumanStepDocTypePermissionDao = new WorkflowHumanStepDocTypePermissionDao(dataContext);
                CustomActivitySettingDao customActiovitySettingDao = new CustomActivitySettingDao(dataContext);
                List<Guid> willBeRemovedWorkflowIds = wfDefinitionDao.GetUnusedWorkflowIds(batchTypeId);

                dataContext.BeginTransaction();

                try
                {
                    BatchType batchType = new BatchType
                                              {
                                                  Id = batchTypeId,
                                                  WorkflowDefinitionId = wfDefinition.Id,
                                                  IsWorkflowDefined = true,
                                                  ModifiedBy = LoginUser.UserName,
                                                  ModifiedDate = DateTime.Now
                                              };
                    batchTypeDao.UpdateWorkflow(batchType);
                    wfDefinitionDao.Add(wfDefinition);

                    var listActivityIds = new List<Guid>();

                    if (customActivitySettings != null && customActivitySettings.Count > 0)
                    {
                        foreach (CustomActivitySetting activitySetting in customActivitySettings)
                        {
                            activitySetting.WorkflowDefinitionId = wfDefinition.Id;
                            customActiovitySettingDao.Add(activitySetting);
                            listActivityIds.Add(activitySetting.ActivityId);
                        }
                    }

                    // Delete lookup client in case save lookup client in one lookup activity
                    // then delete this lookup activity
                    var docFieldMetaDao = new DocFieldMetaDataDao(dataContext);
                    docFieldMetaDao.UpdateLookup(batchType.Id, listActivityIds);
                    var batchFieldMetaDao = new BatchFieldMetaDataDao(dataContext);
                    batchFieldMetaDao.UpdateLookup(batchType.Id, listActivityIds);


                    //dataContext.Commit();

                    if (willBeRemovedWorkflowIds.Count > 0)
                    {
                       // dataContext.BeginTransaction();

                        try
                        {
                            foreach (Guid workflowId in willBeRemovedWorkflowIds)
                            {
                                wfHumanStepDocTypePermissionDao.DeleteByWorkflowDefinitionId(workflowId);
                                wfHumanStepPermissionDao.DeleteByWorkflowDefinitionId(workflowId);
                                customActiovitySettingDao.DeleteByWorkflow(workflowId);
                                wfDefinitionDao.DeleteById(workflowId);
                            }

                           // dataContext.Commit();
                        }
                        catch (Exception) // Ignore this exception
                        {
                            dataContext.Rollback();
                        }
                    }

                    dataContext.Commit();

                    return wfDefinition.Id;
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }
        //public Guid SaveWorkflowDefinition(Guid batchTypeId, WorkflowDefinition wfDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings)
        //{
        //    if (!LoginUser.IsAdmin)
        //    {
        //        throw new SecurityException(string.Format("User {0} doesn't have permission to save workflow definition.", LoginUser.UserName));
        //    }

        //    wfDefinition.Id = Guid.NewGuid();
        //    wfDefinition.BatchTypeId = batchTypeId;

        //    using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
        //    {
        //        BatchTypeDao batchTypeDao = new BatchTypeDao(dataContext);
        //        WorkflowDefinitionDao wfDefinitionDao = new WorkflowDefinitionDao(dataContext);
        //        WorkflowHumanStepPermissionDao wfHumanStepPermissionDao = new WorkflowHumanStepPermissionDao(dataContext);
        //        WorkflowHumanStepDocTypePermissionDao wfHumanStepDocTypePermissionDao = new WorkflowHumanStepDocTypePermissionDao(dataContext);
        //        CustomActivitySettingDao customActiovitySettingDao = new CustomActivitySettingDao(dataContext);
        //        AnnotationPermissionDao annotationPermissionDao = new AnnotationPermissionDao(dataContext);
        //        List<Guid> willBeRemovedWorkflowIds = wfDefinitionDao.GetUnusedWorkflowIds(batchTypeId);

        //        dataContext.BeginTransaction();

        //        try
        //        {
        //            BatchType batchType = new BatchType
        //                                      {
        //                                          Id = batchTypeId,
        //                                          WorkflowDefinitionId = wfDefinition.Id,
        //                                          IsWorkflowDefined = true,
        //                                          ModifiedBy = LoginUser.UserName,
        //                                          ModifiedDate = DateTime.Now
        //                                      };
        //            batchTypeDao.UpdateWorkflow(batchType);
        //            wfDefinitionDao.Add(wfDefinition);

        //            if (humanStepPermissions != null && humanStepPermissions.Count > 0)
        //            {
        //                foreach (var humanStepPermission in humanStepPermissions)
        //                {
        //                    if (humanStepPermission.UserGroupPermissions != null && humanStepPermission.UserGroupPermissions.Count > 0)
        //                    {
        //                        foreach (var groupPermission in humanStepPermission.UserGroupPermissions)
        //                        {
        //                            wfHumanStepPermissionDao.Add(new WorkflowHumanStepPermission
        //                                                             {
        //                                                                 HumanStepId = humanStepPermission.HumanStepId,
        //                                                                 WorkflowDefinitionId = wfDefinition.Id,
        //                                                                 UserGroupId = groupPermission.UserGroupId,
        //                                                                 CanAnnotate = groupPermission.CanAnnotate,
        //                                                                 CanDelete = groupPermission.CanDelete,
        //                                                                 CanDownloadFilesOnDemand = groupPermission.CanDownloadFilesOnDemand,
        //                                                                 CanEmail = groupPermission.CanEmail,
        //                                                                 CanModifyDocument = groupPermission.CanModifyDocument,
        //                                                                 CanModifyIndexes = groupPermission.CanModifyIndexes,
        //                                                                 CanPrint = groupPermission.CanPrint,
        //                                                                 CanReject = groupPermission.CanReject,
        //                                                                 CanReleaseLoosePage = groupPermission.CanReleaseLoosePage,
        //                                                                 CanSendLink = groupPermission.CanSendLink,
        //                                                                 CanViewOtherItems = groupPermission.CanViewOtherItems
        //                                                             });

        //                            //if (groupPermission.DocTypePermissions != null && groupPermission.DocTypePermissions.Count > 0)
        //                            //{
        //                            //    foreach (var docTypePermission in groupPermission.DocTypePermissions)
        //                            //    {
        //                            //        wfHumanStepDocTypePermissionDao.Add(new WorkflowHumanStepDocumentTypePermission
        //                            //                                                {
        //                            //                                                    HumanStepId = humanStepPermission.HumanStepId,
        //                            //                                                    WorkflowDefinitionId = wfDefinition.Id,
        //                            //                                                    UserGroupId = groupPermission.UserGroupId,
        //                            //                                                    DocTypeId = docTypePermission.DocTypeId,
        //                            //                                                    CanAccess = docTypePermission.CanAccess,
        //                            //                                                    CanSeeRestrictedField = docTypePermission.CanSeeRestrictedField
        //                            //                                                });
        //                            //    }
        //                            //}
        //                        }
        //                    }

        //                    if (humanStepPermission.AnnotationPermissions != null && humanStepPermission.AnnotationPermissions.Count > 0)
        //                    {
        //                        foreach (var annoPermission in humanStepPermission.AnnotationPermissions)
        //                        {
        //                            annotationPermissionDao.Add(annoPermission);
        //                        }
        //                    }
        //                }
        //            }

        //            if (customActivitySettings != null && customActivitySettings.Count > 0)
        //            {
        //                foreach (CustomActivitySetting activitySetting in customActivitySettings)
        //                {
        //                    activitySetting.WorkflowDefinitionId = wfDefinition.Id;
        //                    customActiovitySettingDao.Add(activitySetting);
        //                }
        //            }

        //            dataContext.Commit();

        //            if (willBeRemovedWorkflowIds.Count > 0)
        //            {
        //                dataContext.BeginTransaction();

        //                try
        //                {
        //                    foreach (Guid workflowId in willBeRemovedWorkflowIds)
        //                    {
        //                        wfHumanStepDocTypePermissionDao.DeleteByWorkflowDefinitionId(workflowId);
        //                        wfHumanStepPermissionDao.DeleteByWorkflowDefinitionId(workflowId);
        //                        customActiovitySettingDao.DeleteByWorkflow(workflowId);
        //                        wfDefinitionDao.DeleteById(workflowId);
        //                    }

        //                    dataContext.Commit();
        //                }
        //                catch (Exception) // Ignore this exception
        //                {
        //                    dataContext.Rollback();
        //                }
        //            }

        //            return wfDefinition.Id;
        //        }
        //        catch
        //        {
        //            dataContext.Rollback();
        //            throw;
        //        }
        //    }
        //}

        //public List<HumanStepPermission> GetWorkflowHumanStepPermissions(Guid wfDefinitionId)
        //{
        //    using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
        //    {
        //        List<WorkflowHumanStepPermission> groupPermissions = new WorkflowHumanStepPermissionDao(dataContext).GetByWorkflowDefinitionId(wfDefinitionId);
        //        List<WorkflowHumanStepDocumentTypePermission> docTypePermissions = new WorkflowHumanStepDocTypePermissionDao(dataContext).GetByWorkflowDefinitionId(wfDefinitionId);
        //        List<Guid> humanStepIds = groupPermissions.Select(p => p.HumanStepId).Distinct().ToList();
        //        List<HumanStepPermission> humanStepPermissions = new List<HumanStepPermission>();

        //        foreach (Guid humanStepId in humanStepIds)
        //        {
        //            HumanStepPermission humanStepPermission = new HumanStepPermission
        //                                                          {
        //                                                              HumanStepId = humanStepId,
        //                                                              WorkflowDefinitionId = wfDefinitionId,
        //                                                              UserGroupPermissions = new List<HumanStepUserGroupPermission>()
        //                                                          };

        //            groupPermissions.Where(p => p.HumanStepId == humanStepId).ToList().ForEach(p => humanStepPermission.UserGroupPermissions.Add(p.Clone()));
        //            List<WorkflowHumanStepDocumentTypePermission> stepDocTypePermissions = docTypePermissions.Where(p => p.HumanStepId == humanStepId).ToList();

        //            //foreach (var stepGroupPermission in humanStepPermission.UserGroupPermissions)
        //            //{
        //            //    stepDocTypePermissions.Where(p => p.UserGroupId == stepGroupPermission.UserGroupId).ToList().ForEach(p => stepGroupPermission.DocTypePermissions.Add(p.Clone()));
        //            //}

        //            humanStepPermissions.Add(humanStepPermission);
        //        }

        //        return humanStepPermissions;
        //    }
        //}

        public List<CustomActivitySetting> GetWorkflowCustomActivitySetting(Guid wfDefinitionId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(dataContext);
                return customActivityDao.GetCustomActivitySettingByWorkflow(wfDefinitionId);
            }
        }
    }
}
