using System;
using System.Linq;
using System.Activities;
using Ecm.CaptureCore;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.Contract;
using System.Drawing;
using System.ComponentModel;
using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration;
using System.Activities.Presentation.PropertyEditing;
using Ecm.Workflow.Activities.CustomActivityDomain;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Ecm.Workflow.WorkflowExtension;

namespace Ecm.Workflow.Activities.ReleaseToArchive
{
    [Designer(typeof(ReleaseToArchiveDesigner))]
    [ToolboxBitmap(typeof(ReleaseToArchive), "ReleaseToArchive.png")]
    public sealed class ReleaseToArchive : StoppableActivityContract
    {
        [Editor(typeof(ConfigurationDesigner), typeof(DialogPropertyValueEditor))]
        public Guid Setting { get; set; }

        [RequiredArgument]
        public InArgument<Batch> Workitem { get; set; }
        private readonly SecurityManager _securityManager = new SecurityManager();

        protected override void ExecutionBody(NativeActivityContext context)
        {
            try
            {
                var wfSystem = _securityManager.Authorize("WorkflowSystem", "TzmdoMVgNmQ5QMXJDuLBKgKg6CYfx73S/8dPX8Ytva+Eu3hlFNVoAg==");
                wfSystem.ClientHost = string.Empty;

                Batch batch = Workitem.Get(context);
                WorkflowRuntimeData runtimeData = GetWorkflowRuntimeData(context);
                User user = wfSystem;
                ActionLogManager actionLog = new ActionLogManager(user);

                actionLog.AddLog("Begin release to archive process on batch Id: " + batch.Id, user, ActionName.ReleaseToArchive, null, null);

                CustomActivitySetting setting = GetSetting(batch.WorkflowDefinitionId, user);
                ReleaseInfo releaseInfo = null;
                List<Mapping> mappings = null;

                if (setting != null)
                {
                    releaseInfo = Utility.UtilsSerializer.Deserialize<ReleaseInfo>(setting.Value);
                    mappings = releaseInfo.MappingInfos.Where(p => p.CaptureDocumentTypeId != Guid.Empty && p.ReleaseDocumentTypeId != Guid.Empty &&
                                                            p.FieldMaps.Any(q => q.ArchiveFieldId != Guid.Empty && q.CaptureFieldId != Guid.Empty)).ToList();
                    ArchiveProvider provider = new ArchiveProvider(releaseInfo.LoginInfo.UserName, releaseInfo.LoginInfo.Password, releaseInfo.LoginInfo.ArchiveEndPoint);
                    Domain.User archiveUser = provider.ValidateReleaseToArchiveUser();

                    if (archiveUser == null)
                    {
                        actionLog.AddLog("The release to Archive authorization is invalid for batch: " + batch.Id, user, ActionName.ReleaseToArchive, null, null);
                        return;
                    }

                    provider.ConfigUserInfo(archiveUser.UserName, archiveUser.Password);

                    foreach (CaptureDomain.Document captureDoc in batch.Documents)
                    {
                        if (mappings.Any(p => p.CaptureDocumentTypeId == captureDoc.DocTypeId))
                        {
                            foreach (var mapping in mappings)
                            {
                                Domain.Document archiveDocument = new Domain.Document
                                {
                                    BinaryType = captureDoc.BinaryType,
                                    CreatedBy = user.UserName,
                                    CreatedDate = DateTime.Now
                                };

                                if (mapping.CaptureDocumentTypeId != captureDoc.DocTypeId)
                                {
                                    continue;
                                }

                                archiveDocument.DocumentType = provider.GetDocumentType(mapping.ReleaseDocumentTypeId);
                                archiveDocument.Pages = new List<Domain.Page>(); ;
                                archiveDocument.FieldValues = new List<Domain.FieldValue>();

                                archiveDocument.DocTypeId = mapping.ReleaseDocumentTypeId;

                                foreach (CaptureDomain.Page capturePage in captureDoc.Pages)
                                {
                                    #region
                                    Domain.Page archivePage = new Domain.Page
                                                                {
                                                                    Content = capturePage.Content,
                                                                    ContentLanguageCode = capturePage.ContentLanguageCode,
                                                                    FileBinary = capturePage.FileBinary,
                                                                    FileExtension = capturePage.FileExtension,
                                                                    FileHash = capturePage.FileHash,
                                                                    Height = capturePage.Height,
                                                                    PageNumber = capturePage.PageNumber,
                                                                    RotateAngle = capturePage.RotateAngle,
                                                                    Width = capturePage.Width
                                                                };

                                    archiveDocument.Pages.Add(archivePage);

                                    archivePage.Annotations = new List<Domain.Annotation>();

                                    foreach (CaptureDomain.Annotation annotation in capturePage.Annotations)
                                    {
                                        Domain.Annotation archiveAnnotation = new Domain.Annotation
                                        {
                                            Content = annotation.Content,
                                            CreatedBy = user.UserName,
                                            CreatedOn = DateTime.Now,
                                            ModifiedBy = user.UserName,
                                            ModifiedOn = DateTime.Now,
                                            Height = annotation.Height,
                                            Left = annotation.Left,
                                            LineColor = annotation.LineColor,
                                            LineEndAt = annotation.LineEndAt,
                                            LineStartAt = annotation.LineStartAt,
                                            LineStyle = annotation.LineStyle,
                                            LineWeight = annotation.LineWeight,
                                            RotateAngle = annotation.RotateAngle,
                                            Top = annotation.Top,
                                            Type = annotation.Type,
                                            Width = annotation.Width
                                        };

                                        archivePage.Annotations.Add(archiveAnnotation);
                                    }
                                    #endregion
                                }

                                List<MappingField> fieldMappings = mapping.FieldMaps.ToList();

                                foreach (CaptureDomain.DocumentFieldValue captureFieldValue in captureDoc.FieldValues.Where(p => !p.FieldMetaData.IsSystemField))
                                {
                                    MappingField fieldMapping = fieldMappings.FirstOrDefault(p => p.CaptureFieldId == captureFieldValue.FieldMetaData.Id);
                                    if (fieldMapping == null)
                                    {
                                        continue;
                                    }

                                    var archiveDocType = provider.GetDocumentType(mapping.ReleaseDocumentTypeId);

                                    Domain.FieldMetaData field = archiveDocType.FieldMetaDatas.FirstOrDefault(p => p.Id == fieldMapping.ArchiveFieldId);

                                    if (field == null)
                                    {
                                        continue;
                                    }

                                    Domain.FieldValue fieldValue = new Domain.FieldValue
                                    {
                                        FieldId = field.Id,
                                        Value = captureFieldValue.Value,
                                        FieldMetaData = field
                                    };

                                    if (captureFieldValue.TableFieldValue != null && captureFieldValue.TableFieldValue.Count > 0)
                                    {
                                        //int rowNum = 0;

                                        foreach (var captureValue in captureFieldValue.TableFieldValue)
                                        {
                                            #region
                                            MappingField columnMapping = fieldMapping.MappingFields.FirstOrDefault(p => p.CaptureFieldId == captureValue.Field.Id);

                                            if (columnMapping == null)
                                            {
                                                continue;
                                            }

                                            var columnField = field.Children.FirstOrDefault(h => h.Id == columnMapping.ArchiveFieldId);

                                            if (columnField == null)
                                            {
                                                continue;
                                            }

                                            fieldValue.TableFieldValue.Add(new Domain.TableFieldValue
                                            {
                                                Field = columnField,
                                                FieldId = columnField.Id,
                                                RowNumber = captureValue.RowNumber,
                                                Value = captureValue.Value
                                            });

                                            //rowNum++;
                                            #endregion
                                        }
                                    }

                                    archiveDocument.FieldValues.Add(fieldValue);
                                }

                                provider.InsertArchiveDocument(archiveDocument);
                            }
                        }
                    }
                }
                actionLog.AddLog("End release to archive process on batch Id: " + batch.Id, user, ActionName.ReleaseToArchive, null, null);

            }
            catch
            {
            }
        }

        private CustomActivitySetting GetSetting(Guid wfDefinitionId, User user)
        {
            Guid activityId = this.UniqueID;
            return new CustomActivitySettingManager(user).GetCustomActivitySetting(wfDefinitionId, activityId);
        }


    }
}
