using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.Workflow.Activities.CustomActivityDomain;
using System.Collections.ObjectModel;

namespace Ecm.Workflow.Activities.LookupConfiguration
{
    public class Mapper
    {
        public static FieldModel GetFieldModel(BatchFieldMetaData p)
        {
            if (p == null)
            {
                return null;
            }

            return new FieldModel
            {
                BatchTypeId = p.BatchTypeId,
                DataType = p.DataTypeEnum,
                DefaultValue = p.DefaultValue,
                DisplayOrder = p.DisplayOrder,
                Id = p.Id,
                IsSystemField = p.IsSystemField,
                Name = p.Name,
                MaxLength = p.MaxLength,
                UniqueId = p.UniqueId
            };
        }

        public static ObservableCollection<FieldModel> GetFieldModels(List<BatchFieldMetaData> batchFields)
        {
            var fieldModels = new ObservableCollection<FieldModel>();

            foreach (BatchFieldMetaData batchField in batchFields)
            {
                fieldModels.Add(GetFieldModel(batchField));
            }

            return fieldModels;
        }

        public static FieldModel GetFieldModel(DocumentFieldMetaData p)
        {
            if (p == null)
            {
                return null;
            }

            return new FieldModel
            {
                DocTypeId = p.DocTypeId,
                DataType = p.DataTypeEnum,
                DefaultValue = p.DefaultValue,
                DisplayOrder = p.DisplayOrder,
                Id = p.Id,
                IsSystemField = p.IsSystemField,
                Name = p.Name,
                MaxLength = p.MaxLength,
                UniqueId = p.UniqueId
            };
        }

        public static ObservableCollection<FieldModel> GetFieldModels(List<DocumentFieldMetaData> docFields)
        {
            var fieldModels = new ObservableCollection<FieldModel>();

            foreach (DocumentFieldMetaData docField in docFields)
            {
                fieldModels.Add(GetFieldModel(docField));
            }

            return fieldModels;
        }

        public static DocumentTypeModel GetDocumentTypeModel(DocumentType docType)
        {
            if (docType == null)
            {
                return null;
            }

            return new DocumentTypeModel
            {
                BatchTypeId = docType.BatchTypeId,
                Fields = GetFieldModels(docType.Fields),
                Id = docType.Id,
                Name = docType.Name,
                UniqueId = docType.UniqueId
            };
        }

        public static ObservableCollection<DocumentTypeModel> GetDocumentTypeModels(List<DocumentType> docTypes)
        {
            return new ObservableCollection<DocumentTypeModel>(docTypes.Select(p => GetDocumentTypeModel(p)));
        }

        public static LookupConfigurationModel GetLookupConfigurationModel(CustomActivityDomain.LookupConfigurationInfo config)
        {
            if (config == null)
            {
                return null;
            }

            return new LookupConfigurationModel
            {
                BatchLookups = new ObservableCollection<LookupInfoModel>(config.BatchFieldLookupInfo.Select(GetLookupInfoModel)),
                DocumentLookups = new ObservableCollection<DocumentLookupInfoModel>(config.DocumentFieldLookupInfo.Select(GetDocumentLookupInfoModel))
            };
        }

        public static LookupInfoModel GetLookupInfoModel(LookupInfo lookupInfo)
        {
            if (lookupInfo == null)
            {
                return null;
            }

            return new LookupInfoModel
            {
                Connection = GetLookupConnectionModel(lookupInfo.LookupConnection),
                FieldMappings = GetLookupMappingModels(lookupInfo.Mappings),
                LookupAtLostFocus = lookupInfo.LookupAtLostFocus,
                LookupType = lookupInfo.LookupType,
                MaxLookupRow = lookupInfo.MaxLookupRow,
                MinPrefixLength = lookupInfo.MinPrefixLength,
                SourceName = lookupInfo.SourceName,
                SqlCommand = lookupInfo.SqlCommand,
                FieldId = lookupInfo.FieldId,
                ApplyLookupClient = lookupInfo.ApplyClientLookup,
                LookupColumn = lookupInfo.LookupColumn,
                LookupOperator = lookupInfo.LookupOperator,
                Parameters = GetParamaterModels(lookupInfo.Parameters)
            };
        }

        public static ParameterModel GetParameterModel(LookupParameter para)
        {
            if (para == null)
            {
                return null;
            }

            return new ParameterModel
            {
                Mode = para.Mode,
                OrderIndex = para.OrderIndex,
                ParameterName = para.ParameterName,
                ParameterType = para.ParameterType,
                ParameterValue = para.ParameterValue
            };
        }

        public static LookupParameter GetParameter(ParameterModel para)
        {
            if (para == null)
            {
                return null;
            }

            return new LookupParameter
            {
                Mode = para.Mode,
                OrderIndex = para.OrderIndex,
                ParameterName = para.ParameterName,
                ParameterType = para.ParameterType,
                ParameterValue = para.ParameterValue
            };
        }

        public static List<LookupParameter> GetParamaters(ObservableCollection<ParameterModel> paras)
        {
            return paras.Select(GetParameter).ToList();
        }

        public static ObservableCollection<ParameterModel> GetParamaterModels(List<LookupParameter> paras)
        {
            return new ObservableCollection<ParameterModel>(paras.Select(GetParameterModel));
        }

        public static LookupMappingModel GetLookupMappingModel(LookupMapping lookupMapping)
        {
            if (lookupMapping == null)
            {
                return null;
            }

            return new LookupMappingModel
            {
                DataColumn = lookupMapping.DataColumn,
                FieldId = lookupMapping.FieldId,
                FieldName = lookupMapping.FieldName,
                LookupType = lookupMapping.LookupType
            };
        }

        public static ObservableCollection<LookupMappingModel> GetLookupMappingModels(List<LookupMapping> mappings)
        {
            return new ObservableCollection<LookupMappingModel>(mappings.Select(GetLookupMappingModel));
        }

        public static LookupConnectionModel GetLookupConnectionModel(LookupConnection connection)
        {
            if (connection == null)
            {
                return null;
            }

            return new LookupConnectionModel
            {
                ConnectionString = connection.ConnectionString,
                DatabaseName = connection.DatabaseName,
                DatabaseType = connection.DbType,
                Host = connection.Host,
                Password = connection.Password,
                Port = connection.Port,
                ProviderType = connection.ProviderType,
                Schema = connection.Schema,
                SqlCommand = connection.SqlCommand,
                Username = connection.Username
            };
        }

        public static DocumentLookupInfoModel GetDocumentLookupInfoModel(DocumentLookupInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new DocumentLookupInfoModel
            {
                DocumentTypeId = info.DocumentTypeId,
                LookupInfos = new ObservableCollection<LookupInfoModel>(info.LookupInfos.Select(GetLookupInfoModel))
            };
        }

        public static CustomActivityDomain.LookupConfigurationInfo GetLookupConfiguration(LookupConfigurationModel config)
        {
            if (config == null)
            {
                return null;
            }

            return new CustomActivityDomain.LookupConfigurationInfo
            {
                BatchFieldLookupInfo = config.BatchLookups.Select(GetLookupInfo).ToList(),
                DocumentFieldLookupInfo = config.DocumentLookups.Select(GetDocumentLookupInfo).ToList()
            };
        }

        public static LookupInfo GetLookupInfo(LookupInfoModel lookupInfo)
        {
            if (lookupInfo == null)
            {
                return null;
            }

            return new LookupInfo
            {
                LookupConnection = GetLookupConnection(lookupInfo.Connection),
                Mappings = GetLookupMappings(lookupInfo.FieldMappings.ToList()),
                LookupAtLostFocus = lookupInfo.LookupAtLostFocus,
                LookupType = lookupInfo.LookupType,
                MaxLookupRow = lookupInfo.MaxLookupRow,
                MinPrefixLength = lookupInfo.MinPrefixLength,
                SourceName = lookupInfo.SourceName,
                SqlCommand = lookupInfo.SqlCommand,
                FieldId = lookupInfo.FieldId,
                ApplyClientLookup = lookupInfo.ApplyLookupClient,
                Parameters = GetParamaters(lookupInfo.Parameters),
                LookupOperator = lookupInfo.LookupOperator,
                LookupColumn = lookupInfo.LookupColumn
            };
        }

        public static LookupMapping GetLookupMapping(LookupMappingModel lookupMapping)
        {
            if (lookupMapping == null)
            {
                return null;
            }

            return new LookupMapping
            {
                DataColumn = lookupMapping.DataColumn,
                FieldId = lookupMapping.FieldId,
                FieldName = lookupMapping.FieldName,
                LookupType = lookupMapping.LookupType
            };
        }

        public static List<LookupMapping> GetLookupMappings(List<LookupMappingModel> mappings)
        {
            return mappings.Select(GetLookupMapping).ToList();
        }

        public static LookupConnection GetLookupConnection(LookupConnectionModel connection)
        {
            if (connection == null)
            {
                return null;
            }

            return new LookupConnection
            {
                ConnectionString = connection.ConnectionString,
                DatabaseName = connection.DatabaseName,
                DbType = connection.DatabaseType,
                Host = connection.Host,
                Password = connection.Password,
                Port = connection.Port,
                ProviderType = connection.ProviderType,
                Schema = connection.Schema,
                SqlCommand = connection.SqlCommand,
                Username = connection.Username
            };
        }

        public static DocumentLookupInfo GetDocumentLookupInfo(DocumentLookupInfoModel info)
        {
            if (info == null)
            {
                return null;
            }

            return new DocumentLookupInfo
            {
                DocumentTypeId = info.DocumentTypeId,
                LookupInfos = info.LookupInfos.Select(GetLookupInfo).ToList()
            };
        }

    }

    public delegate void CloseDialog();

    public class Common
    {
        public const string MAX_SEARCH_ROWS = "MaxSearchRows";
        public const string IN_BETWEEN = "In Between";
        public const string CONTAINS = "Contains";
        public const string NOT_CONTAINS = "Not Contains";
        public const string EQUAL = "Equal";
        public const string NOT_EQUAL = "Not Equal";
        public const string GREATER_THAN = "Greater Than";
        public const string GREATER_THAN_OR_EQUAL_TO = "Greater Than Or Equal To";
        public const string LESS_THAN_OR_EQUAL_TO = "Less Than Or Equal To";
        public const string LESS_THAN = "Less Than";
        public const string STARTS_WITH = "Starts with";
        public const string ENDS_WITH = "End With";
        //DB Type
        public const string SQL_SERVER = "MS SQL Server";
        public const string ORACLE = "Oracle Database";
        public const string IBM_DB2 = "IBM DB2";
        public const string MY_SQL = "My Sql";
        public const string POSTGRE_SQL = "Postgre SQL";

        //Provider Type
        public const string ADO_NET = "ADO.NET provider";
        public const string OLEDB = "Oledb provider";

        //Lookup type
        public const string TABLE = "Table";
        public const string VIEW = "View";
        public const string STORED_PROCEDURE = "Stored Procedure";
    }
}
