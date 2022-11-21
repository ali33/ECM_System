using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using Ecm.Workflow.Activities.LookupConfiguration;
using Ecm.Workflow.Activities.Contract;
using Ecm.CaptureDomain;
using Ecm.CaptureCore;
using Ecm.Workflow.Activities.CustomActivityDomain;
using Ecm.Utility;
using LookupDomain = Ecm.LookupDomain;
using Ecm.Workflow.WorkflowExtension;

namespace Ecm.Workflow.Activities.Lookup
{
    [Designer(typeof(LookupActivityDesigner))]
    [ToolboxBitmap(typeof(Lookup), "lookup.png")]
    public sealed class Lookup : StoppableActivityContract
    {
        [Editor(typeof(LookupDesigner), typeof(DialogPropertyValueEditor))]
        public Guid Configure { get; set; }
        private readonly SecurityManager _securityManager = new SecurityManager();

        protected override void ExecutionBody(NativeActivityContext context)
        {
            var wfSystem = _securityManager.Authorize("WorkflowSystem", "TzmdoMVgNmQ5QMXJDuLBKgKg6CYfx73S/8dPX8Ytva+Eu3hlFNVoAg==");
            wfSystem.ClientHost = string.Empty;

            WorkflowRuntimeData runtimeData = GetWorkflowRuntimeData(context);
            ActionLogManager actionLogManager = new ActionLogManager(wfSystem);
            Batch batch = new BatchManager(wfSystem).GetBatch(Guid.Parse(runtimeData.ObjectID.ToString()));
            CustomActivitySetting setting = GetSetting(batch.WorkflowDefinitionId, wfSystem);
            LookupConfigurationInfo lookupConfigurationInfo = UtilsSerializer.Deserialize<LookupConfigurationInfo>(setting.Value);

            if (lookupConfigurationInfo.BatchFieldLookupInfo != null && lookupConfigurationInfo.BatchFieldLookupInfo.Count > 0)
            {
                actionLogManager.AddLog("Begin lookup data for batch id " + batch.Id, wfSystem, ActionName.StartLookup, null, null);
                foreach (BatchFieldValue batchFieldValue in batch.FieldValues)
                {
                    var batchLookupInfo = lookupConfigurationInfo.BatchFieldLookupInfo.SingleOrDefault(p => p.FieldId == batchFieldValue.FieldId);
                    if (batchLookupInfo != null)
                    {
                        var mappings = batchLookupInfo.Mappings;
                        DataTable lookupData = GetLookupData(batchLookupInfo, batchFieldValue.Value);
                        actionLogManager.AddLog("Get lookup data for field name: " + batchFieldValue.FieldMetaData.Name, wfSystem, ActionName.DoLookup, null, null);

                        foreach (DataColumn lookupColumn in lookupData.Columns)
                        {
                            string valueData = lookupData.Rows[0][lookupColumn.ColumnName].ToString();
                            //var mapping = mappings.SingleOrDefault(p => p.FieldName == batchFieldValue.FieldMetaData.Name);

                            //if (mapping != null)
                            //{
                            var updateVaue = batch.FieldValues.SingleOrDefault(p => p.FieldMetaData.Name == lookupColumn.ColumnName && !p.FieldMetaData.IsSystemField);

                            if (updateVaue != null)
                            {
                                updateVaue.Value = valueData;
                            }
                            //}
                        }
                    }
                }

                actionLogManager.AddLog("End lookup data for batch id " + batch.Id, wfSystem, ActionName.EndLookup, null, null);
            }

            if (lookupConfigurationInfo.DocumentFieldLookupInfo != null && lookupConfigurationInfo.DocumentFieldLookupInfo.Count > 0)
            {
                foreach (Document document in batch.Documents)
                {
                    actionLogManager.AddLog("Begin lookup data for document id " + document.Id + " of batch id " + batch.Id,
                        wfSystem, ActionName.StartLookup, null, null);

                    foreach (DocumentFieldValue docFieldValue in document.FieldValues)
                    {
                        var documentLookupInfo = lookupConfigurationInfo.DocumentFieldLookupInfo.SingleOrDefault(p => p.DocumentTypeId == document.DocTypeId);

                        if (documentLookupInfo == null)
                        {
                            continue;
                        }

                        var lookupInfo = documentLookupInfo.LookupInfos.SingleOrDefault(p => p.FieldId == docFieldValue.FieldId);
                        if (lookupInfo != null)
                        {
                            var mappings = lookupInfo.Mappings;
                            DataTable lookupData = GetLookupData(lookupInfo, docFieldValue.Value);
                            actionLogManager.AddLog("Get lookup data for field name: " + docFieldValue.FieldMetaData.Name, wfSystem, ActionName.DoLookup, null, null);

                            foreach (DataColumn lookupColumn in lookupData.Columns)
                            {
                                string valueData = lookupData.Rows[0][lookupColumn.ColumnName].ToString();
                                //var mapping = mappings.SingleOrDefault(p => p.FieldName == docFieldValue.FieldMetaData.Name);

                                //if (mapping != null)
                                //{
                                var updateValue = document.FieldValues.SingleOrDefault(p => p.FieldMetaData.Name == lookupColumn.ColumnName && !p.FieldMetaData.IsSystemField);

                                if (updateValue != null)
                                {
                                    updateValue.Value = valueData;
                                }
                                //}
                            }
                        }
                    }

                    actionLogManager.AddLog("End lookup data for document id " + document.Id + " of batch id " + batch.Id,
                                            wfSystem, ActionName.EndLookup, null, null);

                }
            }

            new BatchManager(wfSystem).UpdateBatchAfterProcessLookup(batch);

        }

        private DataTable GetLookupData(LookupInfo lookupInfo, string value)
        {
            LookupManager lookupManager = new LookupManager();
            return lookupManager.GetLookupData(GetLookupInfoDomain(lookupInfo), value);
        }

        private CustomActivitySetting GetSetting(Guid wfDefinitionId, User user)
        {
            Guid activityId = this.UniqueID;
            return new CustomActivitySettingManager(user).GetCustomActivitySetting(wfDefinitionId, activityId);
        }

        private LookupDomain.LookupInfo GetLookupInfoDomain(LookupInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new LookupDomain.LookupInfo
            {
                ConnectionInfo = new LookupDomain.ConnectionInfo
                {
                    DatabaseName = info.LookupConnection.DatabaseName,
                    DbType = (LookupDomain.DatabaseType)info.LookupConnection.DbType,
                    Host = info.LookupConnection.Host,
                    Password = info.LookupConnection.Password,
                    Port = info.LookupConnection.Port,
                    ProviderType = (LookupDomain.ProviderType)info.LookupConnection.ProviderType,
                    Schema = info.LookupConnection.Schema,
                    Username = info.LookupConnection.Username
                },
                FieldId = info.FieldId,
                LookupObjectName = info.SourceName,
                LookupType = (LookupDomain.LookupType)info.LookupType,
                LookupWhenTabOut = info.LookupAtLostFocus,
                MinPrefixLength = info.MinPrefixLength,
                QueryCommand = info.SqlCommand,
                LookupMapping = GetMappings(info.Mappings),
                RuntimeMappingInfo = GetMappings(info.Mappings),
                LookupColumn = info.LookupColumn,
                LookupOperator = info.LookupOperator,
                LookupMaps = info.Mappings.Select(h => new Ecm.LookupDomain.LookupMap()
                {
                    DataColumn = h.DataColumn,
                    FieldId = h.FieldId,
                    FieldName = h.FieldName
                }).ToList()
            };
        }

        private List<string> GetMappings(List<LookupMapping> mappings)
        {
            List<string> mappingStrings = new List<string>();
            foreach (LookupMapping mapping in mappings)
            {
                mappingStrings.Add(mapping.FieldName);
            }

            return mappingStrings;
        }

    }
}
