using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Ecm.Domain;

namespace Ecm.Model.DataProvider
{
    public class ObjectMapper
    {
        public static UserModel GetUserModel(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserModel
            {
                Description = user.Type == 0 ? "Built-in User" : user.Type == 1 ? "Active Directory User" : "Single sigin-On",
                EmailAddress = user.Email,
                Fullname = user.FullName,
                Id = user.Id,
                IsAdmin = user.IsAdmin,
                Password = user.Password,
                Type = (UserTypeModel)user.Type,
                Username = user.UserName,
                LanguageId = user.LanguageId,
                UserGroups = GetUserGroupModels(user.UserGroups),
                Language = GetLanguageModel(user.Language),
                Picture = user.Photo,
                EncryptedPassword = user.EncryptedPassword,
                ApplyForCapture = user.ApplyForCapture
            };
        }

        public static ObservableCollection<UserModel> GetUserModels(IList<User> users)
        {
            if (users == null)
            {
                return null;
            }

            var userModels = new ObservableCollection<UserModel>();
            foreach (User user in users)
            {
                UserModel userModel = GetUserModel(user);
                if (userModel == null)
                {
                    continue;
                }

                userModels.Add(userModel);
            }

            return userModels;
        }

        public static User GetUser(UserModel user)
        {
            if (user == null)
            {
                return null;
            }

            return new User
            {
                Email = user.EmailAddress,
                FullName = user.Fullname,
                Id = user.Id,
                IsAdmin = user.IsAdmin,
                Password = user.Password,
                Type = (int)user.Type,
                UserName = user.Username,
                LanguageId = user.LanguageId,
                UserGroups = GetUserGroups(user.UserGroups),
                Photo = user.Picture,
                EncryptedPassword = user.EncryptedPassword,
                ApplyForCapture = user.ApplyForCapture
            };
        }

        public static List<User> GetUsers(IList<UserModel> userModels)
        {
            if (userModels == null)
            {
                return null;
            }

            return userModels.Select(GetUser).ToList();
        }

        public static LanguageModel GetLanguageModel(Language language)
        {
            if (language == null)
            {
                return null;
            }

            return new LanguageModel
            {
                Format = language.Format,
                Id = language.Id,
                Name = language.Name,
                DateFormat = language.DateFormat,
                DecimalChar = language.DecimalChar,
                ThousandChar = language.ThousandChar,
                TimeFormat = language.TimeFormat
            };
        }

        public static List<LanguageModel> GetLanguageModels(List<Language> languages)
        {
            if (languages == null)
            {
                return null;
            }

            return languages.Select(GetLanguageModel).ToList();
        }

        public static Language GetLanguage(LanguageModel languageModel)
        {
            if (languageModel == null)
            {
                return null;
            }

            return new Language
            {
                Format = languageModel.Format,
                Id = languageModel.Id,
                Name = languageModel.Name,
                TimeFormat = languageModel.TimeFormat,
                ThousandChar = languageModel.ThousandChar,
                DecimalChar = languageModel.DecimalChar,
                DateFormat = languageModel.DateFormat
            };
        }

        public List<Language> GetLanguages(List<LanguageModel> languageModels)
        {
            if (languageModels == null)
            {
                return null;
            }

            return languageModels.Select(GetLanguage).ToList();
        }

        public static DocumentTypeModel GetDocumentTypeModel(DocumentType documentType)
        {
            if (documentType == null)
            {
                return null;
            }

            return new DocumentTypeModel
            {
                Id = documentType.Id,
                Name = documentType.Name,
                Fields = GetFieldMetaDataModels(documentType.FieldMetaDatas == null ? null : documentType.FieldMetaDatas.ToList()),
                CreateBy = documentType.CreatedBy,
                CreatedDate = documentType.CreatedDate,
                ModifiedBy = documentType.ModifiedBy,
                ModifiedDate = documentType.ModifiedDate,
                AnnotationPermission = GetAnnotationPermissionModel(documentType.AnnotationPermission),
                DocumentTypePermission = GetDocumentTypePermissionModel(documentType.DocumentTypePermission),
                IsOutlook = documentType.IsOutlook,
                //UniqueId = documentType.UniqueId,
                OCRTemplate = GetOCRTemplateModel(documentType.OCRTemplate),
                Icon = documentType.Icon,
                BarcodeConfigurations = new ObservableCollection<BarcodeConfigurationModel>(GetBarcodeConfigurations(documentType.BarcodeConfigurations))
            };
        }

        public static ObservableCollection<DocumentTypeModel> GetDocumentTypeModels(IList<DocumentType> documentTypes)
        {
            if (documentTypes == null)
            {
                return null;
            }

            var documentTypeModels = new ObservableCollection<DocumentTypeModel>();
            foreach (DocumentType documentType in documentTypes)
            {
                documentTypeModels.Add(GetDocumentTypeModel(documentType));
            }

            return documentTypeModels;
        }

        public static DocumentType GetDocumentType(DocumentTypeModel documentTypeModel)
        {
            if (documentTypeModel == null)
            {
                return null;
            }

            return new DocumentType
            {
                Id = documentTypeModel.Id,
                Name = documentTypeModel.Name,
                FieldMetaDatas = GetFieldMetaDatas(documentTypeModel.Fields),
                DeletedFields = GetFieldMetaDatas(documentTypeModel.DeletedFields),
                ModifiedBy = documentTypeModel.ModifiedBy,
                ModifiedDate = documentTypeModel.ModifiedDate,
                CreatedBy = documentTypeModel.CreateBy,
                CreatedDate = documentTypeModel.CreatedDate,
                AnnotationPermission = GetAnnotationPermission(documentTypeModel.AnnotationPermission),
                DocumentTypePermission = GetDocumentTypePermission(documentTypeModel.DocumentTypePermission),
                IsOutlook = documentTypeModel.IsOutlook,
               // UniqueId = documentTypeModel.UniqueId,
                Icon = documentTypeModel.Icon,
                BarcodeConfigurations = GetBarcodeConfigurations(documentTypeModel.BarcodeConfigurations == null ? new List<BarcodeConfigurationModel>() : documentTypeModel.BarcodeConfigurations.ToList())
            };
        }

        public static IList<DocumentType> GetDocumentTypes(IList<DocumentTypeModel> documentTypeModels)
        {
            if (documentTypeModels == null)
            {
                return null;
            }

            return documentTypeModels.Select(GetDocumentType).ToList();
        }

        public static FieldMetaDataModel GetFieldMetaDataModel(FieldMetaData field)
        {
            if (field == null)
            {
                return null;
            }

            return new FieldMetaDataModel
            {
                DataType = field.DataTypeEnum,
                MaxLength = field.MaxLength,
                DefaultValue = field.DefautValue,
                DisplayOrder = field.DisplayOrder,
                DocTypeId = field.DocTypeId,
                Id = field.Id,
                IsLookup = field.IsLookup,
                IsRequired = field.IsRequired,
                IsRestricted = field.IsRestricted,
                IsSystemField = field.IsSystemField,
                //FieldUniqueId = field.FieldUniqueID,
                Name = field.Name,
                UseCurrentDate = field.UseCurrentDate,
                LookupInfo = GetLookupInfoModel(field.LookupInfo),
                Maps = GetLookupMapModels(field.LookupMaps),
                Picklists = field.Picklists == null ? null : new ObservableCollection<PicklistModel>(GetPicklistModels(field.Picklists)),
                OCRTemplateZone = GetOCRTemplateZoneModel(field.OCRTemplateZone),
                ParentFieldId = field.ParentFieldId,
                Children = field.Children == null ? null : new ObservableCollection<TableColumnModel>(GetTableColumnModels(field.Children))
            };
        }

        public static ObservableCollection<FieldMetaDataModel> GetFieldMetaDataModels(IList<FieldMetaData> fields)
        {
            if (fields == null)
            {
                return null;
            }

            var fieldMetaDataModels = new ObservableCollection<FieldMetaDataModel>();
            foreach (FieldMetaData field in fields)
            {
                fieldMetaDataModels.Add(GetFieldMetaDataModel(field));
            }

            return fieldMetaDataModels;
        }

        public static FieldMetaData GetFieldMetaData(FieldMetaDataModel fieldMetaDataModel)
        {
            if (fieldMetaDataModel == null)
            {
                return null;
            }

            return new FieldMetaData
            {
                DataTypeEnum = fieldMetaDataModel.DataType,
                MaxLength = fieldMetaDataModel.MaxLength,
                DefautValue = fieldMetaDataModel.DefaultValue,
                DisplayOrder = fieldMetaDataModel.DisplayOrder,
                DocTypeId = fieldMetaDataModel.DocTypeId,
                Id = fieldMetaDataModel.Id,
                IsLookup = fieldMetaDataModel.IsLookup,
                IsRequired = fieldMetaDataModel.IsRequired,
                IsRestricted = fieldMetaDataModel.IsRestricted,
                IsSystemField = fieldMetaDataModel.IsSystemField,
                //Guid FieldUniqueId = fieldMetaDataModel.FieldUniqueId,
                Name = fieldMetaDataModel.Name,
                UseCurrentDate = fieldMetaDataModel.UseCurrentDate,
                LookupMaps = GetLookupMaps(fieldMetaDataModel.Maps == null ? new List<LookupMapModel>() : fieldMetaDataModel.Maps.ToList()),
                LookupInfo = GetLookupInfo(fieldMetaDataModel.LookupInfo),
                Picklists = fieldMetaDataModel.Picklists == null ? null : GetPicklists(fieldMetaDataModel.Picklists.ToList()),
                ParentFieldId = fieldMetaDataModel.ParentFieldId,
                Children = GetTableFieldMetadatas(fieldMetaDataModel.Children == null ? null : fieldMetaDataModel.Children.ToList()),
                DeleteChildIds = fieldMetaDataModel.DeletedChildrenIds
            };
        }

        public static List<FieldMetaData> GetFieldMetaDatas(IList<FieldMetaDataModel> fieldMetaDataModels)
        {
            if (fieldMetaDataModels == null)
            {
                return null;
            }

            return fieldMetaDataModels.Select(GetFieldMetaData).ToList();
        }

        public static FieldMetaData GetTableField(TableColumnModel tableColumn)
        {
            return new FieldMetaData
            {
                DataTypeEnum = tableColumn.DataType,
                DefautValue = tableColumn.DefaultValue,
                DisplayOrder = tableColumn.DisplayOrder,
                //Guid FieldUniqueId = tableColumn.ColumnGuid.ToString(),
                Name = tableColumn.ColumnName,
                IsRequired = tableColumn.IsRequired,
                IsRestricted = tableColumn.IsRestricted,
                IsSystemField = false,
                IsLookup = false,
                MaxLength = tableColumn.MaxLength,
                Id = tableColumn.FieldId,
                DocTypeId = tableColumn.DocTypeId,
                ParentFieldId = tableColumn.ParentFieldId,
                UseCurrentDate = tableColumn.UseCurrentDate
            };
        }

        public static List<FieldMetaData> GetTableFieldMetadatas(List<TableColumnModel> tableColumns)
        {
            if (tableColumns == null)
            {
                return null;
            }

            return tableColumns.Select(GetTableField).ToList();
        }

        public static TableColumnModel GetTableColumnModel(FieldMetaData field)
        {
            return new TableColumnModel
            {
                ColumnGuid = Guid.Parse(field.Id.ToString()),
                ColumnName = field.Name,
                FieldId = field.Id,
                Field = GetFieldMetaDataModel(field),
                DataType = field.DataTypeEnum,
                DefaultValue = field.DefautValue,
                DisplayOrder = field.DisplayOrder,
                IsRequired = field.IsRequired,
                IsRestricted = field.IsRestricted,
                MaxLength = field.MaxLength,
                DocTypeId = field.DocTypeId,
                ParentFieldId = field.ParentFieldId,
                UseCurrentDate = field.UseCurrentDate
            };
        }

        public static List<TableColumnModel> GetTableColumnModels(List<FieldMetaData> fields)
        {
            if (fields == null)
            {
                return null;
            }

            return fields.Select(GetTableColumnModel).ToList();
        }

        public static UserGroupModel GetUserGroupModel(UserGroup userGroup)
        {
            if (userGroup == null)
            {
                return null;
            }

            ObservableCollection<UserModel> userModels;
            if (userGroup.Users == null)
            {
                userModels = new ObservableCollection<UserModel>();
            }
            else
            {
                userModels = GetUserModels(userGroup.Users.ToList());
            }

            return new UserGroupModel
            {
                Description = userGroup.Type == 0 ? "Built-in user group" : userGroup.Type == 1 ? "Active Directory user group" : "Single Sign-On user group",
                Id = userGroup.Id,
                Name = userGroup.Name,
                Type = (UserGroupTypeModel)userGroup.Type,
                Users = userModels
            };
        }

        public static ObservableCollection<UserGroupModel> GetUserGroupModels(IList<UserGroup> userGroups)
        {
            if (userGroups == null)
            {
                return null;
            }

            var userGroupModels = new ObservableCollection<UserGroupModel>();
            foreach (var userGroup in userGroups)
            {
                userGroupModels.Add(GetUserGroupModel(userGroup));
            }

            return userGroupModels;
        }

        public static UserGroup GetUserGroup(UserGroupModel userGroupModel)
        {
            if (userGroupModel == null)
            {
                return null;
            }

            return new UserGroup
            {
                Id = userGroupModel.Id,
                Name = userGroupModel.Name,
                Type = (int)userGroupModel.Type,
                Users = GetUsers(userGroupModel.Users)
            };
        }

        public static List<UserGroup> GetUserGroups(IList<UserGroupModel> userGroupModels)
        {
            if (userGroupModels == null)
            {
                return null;
            }

            return userGroupModels.Select(GetUserGroup).ToList();
        }

        public static DocumentTypePermissionModel GetDocumentTypePermissionModel(DocumentTypePermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocumentTypePermissionModel
            {
                AllowedAppendPage = permission.AllowedAppendPage,
                AllowedCapture = permission.AllowedCapture,
                AllowedDeletePage = permission.AllowedDeletePage,
                AllowedDownloadOffline = permission.AllowedDownloadOffline,
                AllowedEmailDocument = permission.AllowedEmailDocument,
                AllowedExportFieldValue = permission.AllowedExportFieldValue,
                AllowedHideAllAnnotation = permission.AllowedHideAllAnnotation,
                AllowedReplacePage = permission.AllowedReplacePage,
                AllowedRotatePage = permission.AllowedRotatePage,
                AllowedSearch = permission.AllowedSearch,
                AllowedSeeRetrictedField = permission.AllowedSeeRetrictedField,
                AllowedUpdateFieldValue = permission.AllowedUpdateFieldValue,
                AlowedPrintDocument = permission.AlowedPrintDocument,
                DocTypeId = permission.DocTypeId,
                UserGroupId = permission.UserGroupId,
                Id = permission.Id
            };
        }

        public static DocumentTypePermission GetDocumentTypePermission(DocumentTypePermissionModel permissionModel)
        {
            if (permissionModel == null)
            {
                return null;
            }

            return new DocumentTypePermission
            {
                AllowedAppendPage = permissionModel.AllowedAppendPage,
                AllowedCapture = permissionModel.AllowedCapture,
                AllowedDeletePage = permissionModel.AllowedDeletePage,
                AllowedDownloadOffline = permissionModel.AllowedDownloadOffline,
                AllowedEmailDocument = permissionModel.AllowedEmailDocument,
                AllowedExportFieldValue = permissionModel.AllowedExportFieldValue,
                AllowedHideAllAnnotation = permissionModel.AllowedHideAllAnnotation,
                AllowedReplacePage = permissionModel.AllowedReplacePage,
                AllowedRotatePage = permissionModel.AllowedRotatePage,
                AllowedSearch = permissionModel.AllowedSearch,
                AllowedSeeRetrictedField = permissionModel.AllowedSeeRetrictedField,
                AllowedUpdateFieldValue = permissionModel.AllowedUpdateFieldValue,
                AlowedPrintDocument = permissionModel.AlowedPrintDocument,
                DocTypeId = permissionModel.DocTypeId,
                UserGroupId = permissionModel.UserGroupId,
                Id = permissionModel.Id
            };
        }

        public static AnnotationPermissionModel GetAnnotationPermissionModel(AnnotationPermission annotationPermission)
        {
            if (annotationPermission == null)
            {
                return null;
            }

            return new AnnotationPermissionModel
            {
                AllowedAddHighlight = annotationPermission.AllowedAddHighlight,
                AllowedAddRedaction = annotationPermission.AllowedAddRedaction,
                AllowedAddText = annotationPermission.AllowedAddText,
                AllowedDeleteHighlight = annotationPermission.AllowedDeleteHighlight,
                AllowedDeleteRedaction = annotationPermission.AllowedDeleteRedaction,
                AllowedDeleteText = annotationPermission.AllowedDeleteText,
                AllowedHideRedaction = annotationPermission.AllowedHideRedaction,
                AllowedSeeHighlight = annotationPermission.AllowedSeeHighlight,
                AllowedSeeText = annotationPermission.AllowedSeeText,
                DocTypeId = annotationPermission.DocTypeId,
                UserGroupId = annotationPermission.UserGroupId,
                Id = annotationPermission.Id
            };
        }

        public static AnnotationPermission GetAnnotationPermission(AnnotationPermissionModel annotationPermissionModel)
        {
            if (annotationPermissionModel == null)
            {
                return null;
            }

            return new AnnotationPermission
            {
                AllowedAddHighlight = annotationPermissionModel.AllowedAddHighlight,
                AllowedAddRedaction = annotationPermissionModel.AllowedAddRedaction,
                AllowedAddText = annotationPermissionModel.AllowedAddText,
                AllowedDeleteHighlight = annotationPermissionModel.AllowedDeleteHighlight,
                AllowedDeleteRedaction = annotationPermissionModel.AllowedDeleteRedaction,
                AllowedDeleteText = annotationPermissionModel.AllowedDeleteText,
                AllowedHideRedaction = annotationPermissionModel.AllowedHideRedaction,
                AllowedSeeHighlight = annotationPermissionModel.AllowedSeeHighlight,
                AllowedSeeText = annotationPermissionModel.AllowedSeeText,
                DocTypeId = annotationPermissionModel.DocTypeId,
                UserGroupId = annotationPermissionModel.UserGroupId,
                Id = annotationPermissionModel.Id
            };
        }

        public static MemberShipModel GetMembershipModel(Membership membership)
        {
            if (membership == null)
            {
                return null;
            }

            return new MemberShipModel
            {
                Id = membership.Id,
                UserGroupId = membership.UserGroupId,
                UserId = membership.UserId
            };
        }

        public static IList<MemberShipModel> GetMembershipModels(IList<Membership> memberships)
        {
            if (memberships == null)
            {
                return null;
            }

            return memberships.Select(GetMembershipModel).ToList();
        }

        public static Membership GetMemberShip(MemberShipModel memberShipModel)
        {
            if (memberShipModel == null)
            {
                return null;
            }

            return new Membership
            {
                Id = memberShipModel.Id,
                UserGroupId = memberShipModel.UserGroupId,
                UserId = memberShipModel.UserId
            };
        }

        public static List<Membership> GetMemberships(IList<MemberShipModel> memberShipModels)
        {
            if (memberShipModels == null)
            {
                return null;
            }

            return memberShipModels.Select(GetMemberShip).ToList();
        }

        public static LookupDataSourceType GetDataSourceType(string type)
        {
            switch (type.ToLower())
            {
                case "v":
                    return LookupDataSourceType.View;
                case "u":
                    return LookupDataSourceType.Table;
                default:
                    return LookupDataSourceType.StoredProcedure;
            }
        }

        public static string GetLookupType(LookupDataSourceType type)
        {
            switch (type)
            {
                case LookupDataSourceType.StoredProcedure:
                    return "P";
                case LookupDataSourceType.Table:
                    return "U";
                default:
                    return "V";
            }
        }

        public static LookupInfoModel GetLookupInfoModel(LookupInfo lookupInfo)
        {
            if (lookupInfo == null)
            {
                return null;
            }

            return new LookupInfoModel
            {
                ConnectionInfo = GetConnectionInfoModel(lookupInfo.ConnectionInfo),
                ConnectionString = lookupInfo.ConnectionString,
                FieldId = lookupInfo.FieldId,
                MaxLookupRow = lookupInfo.MaxLookupRow,
                MinPrefixLength = lookupInfo.MinPrefixLength,
                SourceName = lookupInfo.SourceName,
                SqlCommand = lookupInfo.SqlCommand,
                LookupColumn = lookupInfo.LookupColumn,
                LookupOperator = lookupInfo.LookupOperator,
                Parameters = GetParamaterModels(lookupInfo.Parameters),
                FieldMappings = GetLookupMapModels(lookupInfo.LookupMaps),
                LookupType = lookupInfo.LookupType
            };
        }

        public static LookupInfo GetLookupInfo(LookupInfoModel lookupInfoModel)
        {
            if (lookupInfoModel == null)
            {
                return null;
            }

            return new LookupInfo
            {
                ConnectionInfo = GetConnectionInfo(lookupInfoModel.ConnectionInfo),
                ConnectionString = lookupInfoModel.ConnectionString,
                FieldId = lookupInfoModel.FieldId,
                MaxLookupRow = lookupInfoModel.MaxLookupRow,
                MinPrefixLength = lookupInfoModel.MinPrefixLength,
                SourceName = lookupInfoModel.SourceName,
                SqlCommand = lookupInfoModel.SqlCommand,
                LookupColumn = lookupInfoModel.LookupColumn,
                LookupOperator = lookupInfoModel.LookupOperator,
                Parameters = GetParamaters(lookupInfoModel.Parameters),
                LookupMaps = GetLookupMaps(lookupInfoModel.FieldMappings.ToList()),
                LookupType = lookupInfoModel.LookupType
            };
        }

        public static ParameterModel GetParameterModel(LookupParameter para)
        {
            if(para == null)
            {
                return null;
            }

            return new ParameterModel
            {
                IsRequired = para.IsRequired,
                Mode = para.Mode,
                OrderIndex = para.OrderIndex,
                ParameterName = para.ParameterName,
                ParameterType = para.ParameterType,
                ParameterValue = para.ParameterValue
            };
        }

        public static LookupParameter GetParameter(ParameterModel para)
        {
            if(para == null)
            {
                return null;
            }

            return new LookupParameter
            {
                IsRequired = para.IsRequired,
                Mode = para.Mode,
                OrderIndex = para.OrderIndex,
                ParameterName = para.ParameterName,
                ParameterType = para.ParameterType,
                ParameterValue = para.ParameterValue
            };
        }

        public static List<LookupParameter> GetParamaters(ObservableCollection<ParameterModel> paras)
        {
            if (paras == null)
            {
                return null;
            }

            return paras.Select(GetParameter).ToList();
        }

        public static ObservableCollection<ParameterModel> GetParamaterModels(List<LookupParameter> paras)
        {
            if (paras == null)
            {
                return null;
            }
            return new ObservableCollection<ParameterModel>(paras.Select(GetParameterModel));
        }

        public static LookupMapModel GetLookupMapModel(LookupMap map)
        {
            if (map == null)
            {
                return null;
            }

            return new LookupMapModel
            {
                DataColumn = map.DataColumn,
                FieldId = map.FieldId,
                ArchiveFieldId = map.ArchiveFieldId,
                Name = map.Name,
                IsChecked = true
            };
        }

        public static LookupMap GetLookupMap(LookupMapModel mapModel)
        {
            if (mapModel == null)
            {
                return null;
            }

            return new LookupMap
            {
                Name = mapModel.Name,
                ArchiveFieldId = mapModel.ArchiveFieldId,
                FieldId = mapModel.FieldId,
                DataColumn = mapModel.DataColumn
            };
        }

        public static ObservableCollection<LookupMapModel> GetLookupMapModels(IList<LookupMap> maps)
        {
            if (maps == null)
            {
                return null;
            }

            return new ObservableCollection<LookupMapModel>(maps.Select(GetLookupMapModel));
        }

        public static List<LookupMap> GetLookupMaps(List<LookupMapModel> mapModels)
        {
            if (mapModels == null)
            {
                return null;
            }

            return mapModels.Select(GetLookupMap).ToList();
        }

        public static ConnectionInfo GetConnectionInfo(LookupConnectionModel connectionInfo)
        {
            if (connectionInfo == null)
            {
                return null;
            }
            return new ConnectionInfo
            {
                DatabaseName = connectionInfo.DatabaseName,
                DbType = (DatabaseType)connectionInfo.DatabaseType,
                Host = connectionInfo.Host,
                Password = connectionInfo.Password,
                Port = connectionInfo.Port,
                ProviderType = (ProviderType)connectionInfo.ProviderType,
                Schema = connectionInfo.Schema,
                Username = connectionInfo.Username
            };

        }

        public static LookupConnectionModel GetConnectionInfoModel(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                return null;
            }

            return new LookupConnectionModel
            {
                DatabaseName = connectionInfo.DatabaseName,
                DatabaseType = (int)connectionInfo.DbType,
                Host = connectionInfo.Host,
                Password = connectionInfo.Password,
                Port = connectionInfo.Port,
                ProviderType = (int)connectionInfo.ProviderType,
                Schema = connectionInfo.Schema,
                Username = connectionInfo.Username
            };

        }

        public static SettingModel GetSettingModel(Setting setting)
        {
            if (setting == null)
            {
                return null;
            }

            return new SettingModel
            {
                MaxSearchRows = setting.SearchResultPageSize,
                ServerWorkingFolder = setting.ServerWorkingFolder,
                LuceneFolder = setting.LuceneFolder
            };
        }

        public static Setting GetSetting(SettingModel settingModel)
        {
            if (settingModel == null)
            {
                return null;
            }

            return new Setting
            {
                SearchResultPageSize = settingModel.MaxSearchRows,
                ServerWorkingFolder = settingModel.ServerWorkingFolder,
                LuceneFolder = settingModel.LuceneFolder
            };
        }

        public static IList<Setting> GetSettings(IList<SettingModel> settingModels)
        {
            if (settingModels == null)
            {
                return null;
            }

            return settingModels.Select(GetSetting).ToList();
        }

        public static IList<SettingModel> GetSettingModels(IList<Setting> settings)
        {
            if (settings == null)
            {
                return null;
            }

            return settings.Select(GetSettingModel).ToList();
        }

        public static SearchQuery GetSearchQuery(SearchQueryModel searchModel)
        {
            if (searchModel == null)
            {
                return null;
            }

            return new SearchQuery
            {
                DocTypeId = searchModel.DocTypeId,
                Id = searchModel.Id,
                Name = searchModel.Name,
                SearchQueryExpressions = GetSearchQueryExpressionModels(searchModel.SearchQueryExpressions.ToList()),
                UserId = searchModel.UserId,
                DocumentType = GetDocumentType(searchModel.DocumentType),
                DeletedSearchQueryExpressions = searchModel.DeletedExpressions
            };
        }

        public static SearchQueryExpression GetSearchExpression(SearchQueryExpressionModel expressionModel)
        {
            if (expressionModel == null)
            {
                return null;
            }

            return new SearchQueryExpression
            {
                Condition = GetSearchConjunction(expressionModel.Condition),
                FieldId = expressionModel.Field.Id,
                Id = expressionModel.Id,
                Operator = expressionModel.Operator.ToString(),
                SearchQueryId = expressionModel.SearchQueryId,
                Value1 = expressionModel.Value1,
                Value2 = expressionModel.Value2,
                FieldMetaData = GetFieldMetaData(expressionModel.Field),
                FieldUniqueId = expressionModel.FieldUniqueId
            };
        }

        public static List<SearchQuery> GetSearchQuerys(IList<SearchQueryModel> searchs)
        {
            if (searchs == null)
            {
                return null;
            }

            return searchs.Select(GetSearchQuery).ToList();
        }

        public static List<SearchQueryExpression> GetSearchQueryExpressionModels(IList<SearchQueryExpressionModel> expressions)
        {
            if (expressions == null)
            {
                return null;
            }

            return expressions.Select(GetSearchExpression).ToList();
        }

        public static SearchQueryModel GetSearchQueryModel(SearchQuery searchQuery)
        {
            if (searchQuery == null)
            {
                return null;
            }

            return new SearchQueryModel
            {
                DocTypeId = searchQuery.DocTypeId,
                UserId = searchQuery.UserId,
                Name = searchQuery.Name,
                Id = searchQuery.Id,
                SearchQueryExpressions = new ObservableCollection<SearchQueryExpressionModel>(GetSearchQueryExpressionModels(searchQuery.SearchQueryExpressions.ToList())),
                DocumentType = GetDocumentTypeModel(searchQuery.DocumentType)
            };
        }

        public static List<SearchQueryModel> GetSearchQueries(IList<SearchQuery> searchs)
        {
            if (searchs == null)
            {
                return null;
            }

            return searchs.Select(GetSearchQueryModel).ToList();
        }

        public static SearchQueryExpressionModel GetSearchQueryExpression(SearchQueryExpression expression)
        {
            if (expression == null)
            {
                return null;
            }

            return new SearchQueryExpressionModel
            {
                Value2 = expression.Value2,
                Value1 = expression.Value1,
                SearchQueryId = expression.SearchQueryId,
                Operator = expression.OperatorEnum,
                Id = expression.Id,
                Condition = GetSearchConjunctionEnum(expression.Condition),
                Field = GetFieldMetaDataModel(expression.FieldMetaData),
                FieldUniqueId = expression.FieldUniqueId
            };
        }

        public static List<SearchQueryExpressionModel> GetSearchQueryExpressionModels(IList<SearchQueryExpression> expressions)
        {
            if (expressions == null)
            {
                return null;
            }

            return expressions.Select(GetSearchQueryExpression).ToList();
        }

        public static string GetSearchConjunction(SearchConjunction condition)
        {
            switch (condition)
            {
                case SearchConjunction.And:
                    return Common.AND;
                case SearchConjunction.Or:
                    return Common.OR;
                default:
                    return null;
            }
        }

        public static SearchConjunction GetSearchConjunctionEnum(string condition)
        {
            if (condition == Common.AND)
            {
                return SearchConjunction.And;
            }

            if (condition == Common.OR)
            {
                return SearchConjunction.Or;
            }

            return SearchConjunction.None;
        }

        public static string GetOperator(SearchOperator operatorEnum)
        {
            switch (operatorEnum)
            {
                case SearchOperator.Equal:
                    return Common.EQUAL;
                case SearchOperator.GreaterThan:
                    return Common.GREATER_THAN;
                case SearchOperator.GreaterThanOrEqualTo:
                    return Common.GREATER_THAN_OR_EQUAL_TO;
                case SearchOperator.LessThan:
                    return Common.LESS_THAN;
                case SearchOperator.LessThanOrEqualTo:
                    return Common.LESS_THAN_OR_EQUAL_TO;
                case SearchOperator.InBetween:
                    return Common.IN_BETWEEN;
                case SearchOperator.Contains:
                    return Common.CONTAINS;
                case SearchOperator.NotContains:
                    return Common.NOT_CONTAINS;
                case SearchOperator.NotEqual:
                    return Common.NOT_EQUAL;
                case SearchOperator.StartsWith:
                    return Common.STARTS_WITH;
                case SearchOperator.EndsWith:
                    return Common.ENDS_WITH;
                default:
                    return Common.EQUAL;
            }
        }

        public static SearchOperator GetSearchOperatorEnum(string operatorString)
        {
            switch (operatorString)
            {
                case Common.CONTAINS:
                    return SearchOperator.Contains;
                case Common.ENDS_WITH:
                    return SearchOperator.EndsWith;
                case Common.EQUAL:
                    return SearchOperator.Equal;
                case Common.GREATER_THAN:
                    return SearchOperator.GreaterThan;
                case Common.GREATER_THAN_OR_EQUAL_TO:
                    return SearchOperator.GreaterThanOrEqualTo;
                case Common.IN_BETWEEN:
                    return SearchOperator.InBetween;
                case Common.LESS_THAN:
                    return SearchOperator.LessThan;
                case Common.LESS_THAN_OR_EQUAL_TO:
                    return SearchOperator.LessThanOrEqualTo;
                case Common.NOT_CONTAINS:
                    return SearchOperator.NotContains;
                case Common.NOT_EQUAL:
                    return SearchOperator.NotEqual;
                case Common.STARTS_WITH:
                    return SearchOperator.StartsWith;
                default:
                    return SearchOperator.Equal;
            }
        }

        public static Type GetDataType(string type)
        {
            switch (type)
            {
                case "String":
                    return typeof(string);
                case "Date":
                    return typeof(DateTime);
                case "Integer":
                    return typeof(int);
                case "Decimal":
                    return typeof(decimal);
                case "Boolean":
                    return typeof(bool);
                default:
                    return typeof(string);
            }
        }

        public static Document GetDocument(DocumentModel document)
        {
            if (document == null)
            {
                return null;
            }

            return new Document
            {
                CreatedBy = document.CreatedBy,
                CreatedDate = document.CreatedDate,
                DocumentType = GetDocumentType(document.DocumentType),
                FieldValues = GetFieldValues(document.FieldValues),
                Id = document.Id,
                Version = document.Version,
                ModifiedBy = document.ModifiedBy,
                ModifiedDate = document.ModifiedDate,
                PageCount = document.PageCount,
                Pages = GetPages(document.Pages),
                DeletedPages = document.DeletedPages,
                BinaryType = document.BinaryType.ToString(),
                DocTypeId = document.DocumentType.Id,
                EmbeddedPictures = GetPictures(document.EmbeddedPictures),
                DeletedLinkDocuments = document.DeletedLinkDocuments,
                LinkDocuments = GetLinkDocuments(document.LinkDocuments.ToList())
            };
        }

        public static List<Document> GetDocuments(IList<DocumentModel> documents)
        {
            if (documents == null)
            {
                return null;
            }

            return documents.Select(GetDocument).ToList();
        }

        public static DocumentModel GetDocumentModel(Document document)
        {
            if (document == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(document.BinaryType))
            {
                document.BinaryType = FileTypeModel.Compound.ToString();
            }

            return new DocumentModel
            {
                CreatedBy = document.CreatedBy,
                CreatedDate = document.CreatedDate,
                DocumentType = GetDocumentTypeModel(document.DocumentType),
                FieldValues = GetFieldValueModels(document.FieldValues),
                Id = document.Id,
                Version = document.Version,
                ModifiedBy = document.ModifiedBy,
                ModifiedDate = document.ModifiedDate,
                PageCount = document.PageCount,
                Pages = GetPageModels(document.Pages),
                BinaryType = (FileTypeModel)Enum.Parse(typeof(FileTypeModel), document.BinaryType),
                EmbeddedPictures = GetPictureDictionay(document.EmbeddedPictures),
                LinkDocuments = new ObservableCollection<LinkDocumentModel>(GetLinkDocumentModels(document.LinkDocuments))
            };
        }

        public static List<DocumentModel> GetDocumentModels(IList<Document> documents)
        {
            if (documents == null)
            {
                return null;
            }

            return documents.Select(GetDocumentModel).ToList();
        }

        public static LinkDocument GetLinkDocument(LinkDocumentModel model)
        {
            return new LinkDocument
            {
                Id = model.Id,
                DocumentId = model.DocumentId,
                LinkDocumentId = model.LinkedDocumentId,
                Notes = model.Notes
                //LinkedDocument = GetDocument(model.LinkedDocument),
                //RootDocument = GetDocument(model.RootDocument)
            };
        }

        public static List<LinkDocument> GetLinkDocuments(List<LinkDocumentModel> models)
        {
            if (models == null)
            {
                return new List<LinkDocument>();
            }

            return models.Select(GetLinkDocument).ToList();

        }

        public static LinkDocumentModel GetLinkDocumentModel(LinkDocument obj)
        {
            return new LinkDocumentModel
            {
                Id = obj.Id,
                DocumentId = obj.DocumentId,
                LinkedDocumentId = obj.LinkDocumentId,
                Notes = obj.Notes,
                RootDocument = GetDocumentModel(obj.RootDocument),
                LinkedDocument = GetDocumentModel(obj.LinkedDocument)
            };
        }

        public static List<LinkDocumentModel> GetLinkDocumentModels(List<LinkDocument> objs)
        {
            if (objs == null)
            {
                return new List<LinkDocumentModel>();
            }

            return objs.Select(GetLinkDocumentModel).ToList();

        }
        public static List<OutlookPicture> GetPictures(Dictionary<string, byte[]> pics)
        {
            List<OutlookPicture> outlookPic = new List<OutlookPicture>();
            if (pics != null)
            {
                pics.ToList().ForEach(p => outlookPic.Add(new OutlookPicture { FileBinary = p.Value, FileName = p.Key }));
            }

            return outlookPic;
        }

        public static Dictionary<string, byte[]> GetPictureDictionay(List<OutlookPicture> pics)
        {
            Dictionary<string, byte[]> picsDic = new Dictionary<string, byte[]>();

            if (pics != null)
            {
                pics.ForEach(p => picsDic.Add(p.FileName, p.FileBinary));
            }

            return picsDic;
        }

        public static Page GetPage(PageModel page)
        {
            if (page == null)
            {
                return null;
            }

            return new Page
            {
                DocId = page.DocId,
                FileBinary = page.FileBinaries,
                FileHash = page.FileHash,
                Id = page.Id,
                PageNumber = page.PageNumber,
                Annotations = GetAnnotations(page.Annotations),
                FileExtension = page.FileExtension,
                RotateAngle = page.RotateAngle,
                Height = page.Height,
                Width = page.Width,
                Content = page.Content,
                ContentLanguageCode = page.ContentLanguageCode,
                OriginalFileName = page.OriginalFileName
            };
        }

        public static List<Page> GetPages(List<PageModel> pages)
        {
            if (pages == null)
            {
                return null;
            }

            return pages.Select(GetPage).ToList();
        }

        public static PageModel GetPageModel(Page page)
        {
            if (page == null)
            {
                return null;
            }

            return new PageModel
            {
                DocId = page.DocId,
                FileBinaries = page.FileBinary,
                FileHash = page.FileHash,
                Id = page.Id,
                PageNumber = page.PageNumber,
                Annotations = GetAnnotationModels(page.Annotations),
                FileExtension = page.FileExtension,
                RotateAngle = page.RotateAngle,
                Height = page.Height,
                Width = page.Width,
                Content = page.Content,
                ContentLanguageCode = page.ContentLanguageCode,
                OriginalFileName = page.OriginalFileName
            };
        }

        public static List<PageModel> GetPageModels(IEnumerable<Page> pages)
        {
            if (pages == null)
            {
                return null;
            }

            return pages.Select(GetPageModel).ToList();
        }

        public static Annotation GetAnnotation(AnnotationModel annotation)
        {
            if (annotation == null)
            {
                return null;
            }

            return new Annotation
            {
                Content = annotation.Content,
                CreatedBy = annotation.CreatedBy,
                CreatedOn = annotation.CreatedOn,
                Height = annotation.Height,
                Id = annotation.Id,
                Left = annotation.Left,
                LineColor = annotation.LineColor,
                LineEndAt = annotation.LineEndAt.ToString(),
                LineStartAt = annotation.LineStartAt.ToString(),
                LineStyle = annotation.LineStyle.ToString(),
                LineWeight = annotation.LineWeight,
                ModifiedBy = annotation.ModifiedBy,
                ModifiedOn = annotation.ModifiedOn,
                RotateAngle = annotation.RotateAngle,
                Top = annotation.Top,
                Type = annotation.Type.ToString(),
                Width = annotation.Width
            };
        }

        public static List<Annotation> GetAnnotations(IEnumerable<AnnotationModel> annotations)
        {
            if (annotations == null)
            {
                return null;
            }

            return annotations.Select(GetAnnotation).ToList();
        }

        public static AnnotationModel GetAnnotationModel(Annotation annotation)
        {
            if (annotation == null)
            {
                return null;
            }

            return new AnnotationModel
            {
                Content = annotation.Content,
                CreatedBy = annotation.CreatedBy,
                CreatedOn = annotation.CreatedOn,
                Height = annotation.Height,
                Id = annotation.Id,
                Left = annotation.Left,
                LineColor = annotation.LineColor,
                LineEndAt = (RectangleVertexModel)Enum.Parse(typeof(RectangleVertexModel), annotation.LineEndAt),
                LineStartAt = (RectangleVertexModel)Enum.Parse(typeof(RectangleVertexModel), annotation.LineStartAt),
                LineStyle = (LineStyleModel)Enum.Parse(typeof(LineStyleModel), annotation.LineStyle),
                LineWeight = annotation.LineWeight,
                ModifiedBy = annotation.ModifiedBy,
                ModifiedOn = annotation.ModifiedOn,
                RotateAngle = annotation.RotateAngle,
                Top = annotation.Top,
                Type = (AnnotationTypeModel)Enum.Parse(typeof(AnnotationTypeModel), annotation.Type),
                Width = annotation.Width
            };
        }

        public static List<AnnotationModel> GetAnnotationModels(IEnumerable<Annotation> annotations)
        {
            if (annotations == null)
            {
                return null;
            }

            return annotations.Select(GetAnnotationModel).ToList();
        }

        public static FieldValue GetFieldValue(FieldValueModel fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }

            return new FieldValue
            {
                FieldMetaData = GetFieldMetaData(fieldValue.Field),
                TableFieldValue = fieldValue.TableValues == null ? null : GetTableFieldValues(fieldValue.TableValues.ToList()),
                Value = fieldValue.Value,
                FieldId = fieldValue.Field.Id
            };
        }

        public static List<FieldValue> GetFieldValues(List<FieldValueModel> fieldValues)
        {
            if (fieldValues == null)
            {
                return null;
            }

            return fieldValues.Select(GetFieldValue).ToList();
        }

        public static FieldValueModel GetFieldValueModel(FieldValue fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }

            return new FieldValueModel
            {
                Field = GetFieldMetaDataModel(fieldValue.FieldMetaData),
                TableValues = GetTableFieldValueModels(fieldValue.TableFieldValue),
                Value = fieldValue.Value
            };
        }

        public static List<FieldValueModel> GetFieldValueModels(IList<FieldValue> fieldValues)
        {
            if (fieldValues == null)
            {
                return null;
            }

            return fieldValues.Select(GetFieldValueModel).ToList();
        }

        public static TableFieldValue GetTableFieldValue(TableFieldValueModel tableFieldValue)
        {
            if (tableFieldValue == null)
            {
                return null;
            }

            var model = new TableFieldValue
            {
                Field = GetFieldMetaData(tableFieldValue.Field),
                Id = tableFieldValue.Id,
                RowNumber = tableFieldValue.RowNumber,
                Value = tableFieldValue.Value,
                FieldId = tableFieldValue.Field.Id,
                DocId = tableFieldValue.DocId
            };

            if (tableFieldValue.IsNew)
            {
                model.Id = Guid.Empty;
            }

            return model;
        }

        public static List<TableFieldValue> GetTableFieldValues(List<TableFieldValueModel> tableFieldValues)
        {
            if (tableFieldValues == null)
            {
                return null;
            }

            return tableFieldValues.Select(GetTableFieldValue).ToList();
        }

        public static TableFieldValueModel GetTableFieldValueModel(TableFieldValue tableFieldValue)
        {
            if (tableFieldValue == null)
            {
                return null;
            }

            return new TableFieldValueModel
            {
                Field = GetFieldMetaDataModel(tableFieldValue.Field),
                Id = tableFieldValue.Id,
                RowNumber = tableFieldValue.RowNumber,
                Value = tableFieldValue.Value,
                FieldId = tableFieldValue.FieldId,
                DocId = tableFieldValue.DocId
            };
        }

        private static ObservableCollection<TableFieldValueModel> GetTableFieldValueModels(List<TableFieldValue> tableFieldValues)
        {
            if (tableFieldValues == null)
            {
                return null;
            }

            return new ObservableCollection<TableFieldValueModel>(tableFieldValues.Select(GetTableFieldValueModel));
        }

        public static ActionLogModel GetActionLogModel(ActionLog actionLog)
        {
            if (actionLog == null)
            {
                return null;
            }

            return new ActionLogModel
            {
                ActionName = actionLog.ActionName,
                Id = actionLog.Id,
                IpAddress = actionLog.IpAddress,
                LoggedDate = actionLog.LoggedDate,
                Message = actionLog.Message,
                ObjectId = actionLog.ObjectId,
                ObjectType = actionLog.ObjectType,
                Username = actionLog.Username,
                User = GetUserModel(actionLog.User)
            };
        }

        public static List<ActionLogModel> GetActionLogModels(IList<ActionLog> actionLogs)
        {
            if (actionLogs == null)
            {
                return null;
            }

            return actionLogs.Select(GetActionLogModel).ToList();
        }

        public static ActionLog GetActionLog(ActionLogModel actionLog)
        {
            if (actionLog == null)
            {
                return null;
            }

            return new ActionLog
            {
                ActionName = actionLog.ActionName,
                Id = actionLog.Id,
                IpAddress = actionLog.IpAddress,
                LoggedDate = actionLog.LoggedDate,
                Message = actionLog.Message,
                ObjectId = actionLog.ObjectId ?? Guid.Empty,// == null ? 0 : actionLog.ObjectId.Value,
                ObjectType = actionLog.ObjectType,
                Username = actionLog.Username
            };
        }

        public static IList<ActionLog> GetActionLogs(List<ActionLogModel> actionLogs)
        {
            if (actionLogs == null)
            {
                return null;
            }

            return actionLogs.Select(GetActionLog).ToList();
        }

        public static PicklistModel GetPicklistModel(Picklist picklist)
        {
            if (picklist == null)
            {
                return null;
            }

            return new PicklistModel
            {
                FieldId = picklist.FieldId,
                Id = picklist.Id,
                Value = picklist.Value
            };
        }

        public static List<PicklistModel> GetPicklistModels(List<Picklist> picklists)
        {
            if (picklists == null)
            {
                return null;
            }

            return picklists.Select(GetPicklistModel).ToList();
        }

        public static Picklist GetPicklist(PicklistModel picklist)
        {
            if (picklist == null)
            {
                return null;
            }

            return new Picklist
            {
                FieldId = picklist.FieldId,
                Id = picklist.Id,
                Value = picklist.Value
            };
        }

        public static List<Picklist> GetPicklists(List<PicklistModel> picklists)
        {
            if (picklists == null)
            {
                return null;
            }

            return picklists.Select(GetPicklist).ToList();
        }

        public static OCRTemplateModel GetOCRTemplateModel(OCRTemplate ocrTemplate)
        {
            if (ocrTemplate == null)
            {
                return null;
            }

            return new OCRTemplateModel
            {
                DocTypeId = ocrTemplate.DocTypeId,
                Language = GetLanguageModel(ocrTemplate.Language),
                OCRTemplatePages = GetOCRTemplatePageModels(ocrTemplate.OCRTemplatePages),
            };
        }

        public static OCRTemplatePageModel GetOCRTemplatePageModel(OCRTemplatePage ocrTemplatePage)
        {
            if (ocrTemplatePage == null)
            {
                return null;
            }

            return new OCRTemplatePageModel
            {
                Binary = ocrTemplatePage.Binary,
                DPI = ocrTemplatePage.DPI,
                Id = ocrTemplatePage.Id,
                OCRTemplateId = ocrTemplatePage.OCRTemplateId,
                PageIndex = ocrTemplatePage.PageIndex,
                OCRTemplateZones = GetOCRTemplateZoneModels(ocrTemplatePage.OCRTemplateZones),
                Height = ocrTemplatePage.Height,
                RotateAngle = ocrTemplatePage.RotateAngle,
                Width = ocrTemplatePage.Width,
                FileExtension = ocrTemplatePage.FileExtension
            };
        }

        public static IList<OCRTemplatePageModel> GetOCRTemplatePageModels(IList<OCRTemplatePage> ocrTemplatePages)
        {
            if (ocrTemplatePages == null)
            {
                return null;
            }

            return ocrTemplatePages.Select(GetOCRTemplatePageModel).ToList();
        }

        public static OCRTemplatePage GetOCRTemplatePage(OCRTemplatePageModel ocrTemplatePage)
        {
            if (ocrTemplatePage == null)
            {
                return null;
            }

            return new OCRTemplatePage
            {
                Binary = ocrTemplatePage.Binary,
                DPI = ocrTemplatePage.DPI,
                Id = ocrTemplatePage.Id,
                OCRTemplateId = ocrTemplatePage.OCRTemplateId,
                PageIndex = ocrTemplatePage.PageIndex,
                OCRTemplateZones = GetOCRTemplateZones(ocrTemplatePage.OCRTemplateZones),
                Height = ocrTemplatePage.Height,
                RotateAngle = ocrTemplatePage.RotateAngle,
                Width = ocrTemplatePage.Width,
                FileExtension = ocrTemplatePage.FileExtension
            };
        }

        public static List<OCRTemplatePage> GetOCRTemplatePages(IList<OCRTemplatePageModel> ocrTemplatePages)
        {
            if (ocrTemplatePages == null)
            {
                return null;
            }

            return ocrTemplatePages.Select(GetOCRTemplatePage).ToList();
        }

        public static OCRTemplateZoneModel GetOCRTemplateZoneModel(OCRTemplateZone ocrTemplateZone)
        {
            if (ocrTemplateZone == null)
            {
                return null;
            }

            return new OCRTemplateZoneModel
            {
                FieldMetaData = new FieldMetaDataModel { Id = ocrTemplateZone.FieldMetaDataId },
                Height = ocrTemplateZone.Height,
                Width = ocrTemplateZone.Width,
                Left = ocrTemplateZone.Left,
                Top = ocrTemplateZone.Top,
                OCRTemplatePageId = ocrTemplateZone.OCRTemplatePageId,
                CreatedBy = ocrTemplateZone.CreatedBy,
                CreatedOn = ocrTemplateZone.CreatedOn,
                ModifiedBy = ocrTemplateZone.ModifiedBy,
                ModifiedOn = ocrTemplateZone.ModifiedOn
            };
        }

        public static List<OCRTemplateZoneModel> GetOCRTemplateZoneModels(IList<OCRTemplateZone> ocrTemplateZones)
        {
            if (ocrTemplateZones == null)
            {
                return null;
            }

            return ocrTemplateZones.Select(GetOCRTemplateZoneModel).ToList();
        }

        public static OCRTemplate GetOCRTemplate(OCRTemplateModel ocrTemplate)
        {
            if (ocrTemplate == null)
            {
                return null;
            }

            return new OCRTemplate
            {
                DocTypeId = ocrTemplate.DocTypeId,
                Language = GetLanguage(ocrTemplate.Language),
                OCRTemplatePages = GetOCRTemplatePages(ocrTemplate.OCRTemplatePages)
            };
        }

        public static OCRTemplateZone GetOCRTemplateZone(OCRTemplateZoneModel ocrTemplateZone)
        {
            if (ocrTemplateZone == null)
            {
                return null;
            }

            return new OCRTemplateZone
            {
                FieldMetaDataId = ocrTemplateZone.FieldMetaData.Id,
                Height = ocrTemplateZone.Height,
                Width = ocrTemplateZone.Width,
                Left = ocrTemplateZone.Left,
                Top = ocrTemplateZone.Top,
                OCRTemplatePageId = ocrTemplateZone.OCRTemplatePageId,
                CreatedBy = ocrTemplateZone.CreatedBy,
                CreatedOn = ocrTemplateZone.CreatedOn,
                ModifiedBy = ocrTemplateZone.ModifiedBy,
                ModifiedOn = ocrTemplateZone.ModifiedOn
            };
        }

        public static List<OCRTemplateZone> GetOCRTemplateZones(IList<OCRTemplateZoneModel> ocrTemplateZones)
        {
            if (ocrTemplateZones == null)
            {
                return null;
            }

            return ocrTemplateZones.Select(GetOCRTemplateZone).ToList();
        }

        public static AmbiguousDefinitionModel GetAmbiguousDefinitionModel(AmbiguousDefinition ambiguousDefinition)
        {
            if (ambiguousDefinition == null)
            {
                return null;
            }

            return new AmbiguousDefinitionModel
            {
                Id = ambiguousDefinition.Id,
                Language = GetLanguageModel(ambiguousDefinition.Language),
                IsUnicode = ambiguousDefinition.Unicode,
                LanguageId = ambiguousDefinition.LanguageId,
                Text = ambiguousDefinition.Text
            };
        }

        public static List<AmbiguousDefinitionModel> GetAmbiguousDefinitionModels(List<AmbiguousDefinition> ambiguousDefinitions)
        {
            return ambiguousDefinitions.Select(GetAmbiguousDefinitionModel).ToList();
        }

        public static AmbiguousDefinition GetAmbiguousDefinition(AmbiguousDefinitionModel ambiguousDefinition)
        {
            if (ambiguousDefinition == null)
            {
                return null;
            }

            return new AmbiguousDefinition
            {
                Id = ambiguousDefinition.Id,
                Language = GetLanguage(ambiguousDefinition.Language),
                Unicode = ambiguousDefinition.IsUnicode,
                LanguageId = ambiguousDefinition.LanguageId,
                Text = ambiguousDefinition.Text
            };
        }

        public static List<AmbiguousDefinition> GetAmbiguousDefinitions(List<AmbiguousDefinitionModel> ambiguousDefinitions)
        {
            return ambiguousDefinitions.Select(GetAmbiguousDefinition).ToList();
        }

        public static BarcodeConfigurationModel GetBarcodeConfigurationModel(BarcodeConfiguration barcodeConfiguration)
        {
            if (barcodeConfiguration == null)
            {
                return null;
            }

            return new BarcodeConfigurationModel
            {
                Id = barcodeConfiguration.Id,
                BarcodeType = (BarcodeTypeModel)Enum.Parse(typeof(BarcodeTypeModel), barcodeConfiguration.BarcodeType),
                DocumentTypeId = barcodeConfiguration.DocumentTypeId,
                HasDoLookup = barcodeConfiguration.HasDoLookup,
                IsDocumentSeparator = barcodeConfiguration.IsDocumentSeparator,
                BarcodePosition = barcodeConfiguration.BarcodePosition,
                MapValueToFieldId = barcodeConfiguration.MapValueToFieldId,
                RemoveSeparatorPage = barcodeConfiguration.RemoveSeparatorPage,
                FieldMetaData = GetFieldMetaDataModel(barcodeConfiguration.FieldMetaData)
            };
        }

        public static BarcodeConfiguration GetBarcodeConfiguration(BarcodeConfigurationModel barcodeConfiguration)
        {
            if (barcodeConfiguration == null)
            {
                return null;
            }

            return new BarcodeConfiguration
            {
                Id = barcodeConfiguration.Id,
                BarcodeType = barcodeConfiguration.BarcodeType.ToString(),
                DocumentTypeId = barcodeConfiguration.DocumentTypeId,
                HasDoLookup = barcodeConfiguration.HasDoLookup,
                IsDocumentSeparator = barcodeConfiguration.IsDocumentSeparator,
                BarcodePosition = barcodeConfiguration.BarcodePosition,
                MapValueToFieldId = barcodeConfiguration.MapValueToFieldId,
                RemoveSeparatorPage = barcodeConfiguration.RemoveSeparatorPage
            };
        }

        public static List<BarcodeConfigurationModel> GetBarcodeConfigurations(List<BarcodeConfiguration> barcodeConfigurations)
        {
            if (barcodeConfigurations == null)
            {
                return null;
            }

            return barcodeConfigurations.Select(GetBarcodeConfigurationModel).ToList();
        }

        public static List<BarcodeConfiguration> GetBarcodeConfigurations(List<BarcodeConfigurationModel> barcodeConfigurations)
        {
            if (barcodeConfigurations == null)
            {
                return null;
            }

            return barcodeConfigurations.Select(GetBarcodeConfiguration).ToList();
        }

        public static DocumentModel GetDocumentByDocVersion(DocumentVersion docVersion)
        {
            if (docVersion == null)
                return null;

            return new DocumentModel
            {
                CreatedBy = docVersion.CreatedBy,
                CreatedDate = docVersion.CreatedDate,
                DocumentType = GetDocumentTypeByDocumentTypeVersion(docVersion.DocumentTypeVersion),
                Id = docVersion.DocId,
                Version = docVersion.Version,
                ModifiedBy = docVersion.ModifiedBy,
                ModifiedDate = docVersion.ModifiedDate,
                Pages = GetPageByPageVersions(docVersion.PageVersions),
                FieldValues = GetFieldValueByDocumentFieldVersion(docVersion.DocumentFieldVersions),
                DocVersionId = docVersion.Id
            };
        }

        public static DocumentModel GetDocumentByDocVersion(DocumentVersion docVersion, DocumentTypeModel docType)
        {
            if (docVersion == null)
                return null;

            return new DocumentModel
            {
                CreatedBy = docVersion.CreatedBy,
                CreatedDate = docVersion.CreatedDate,
                DocumentType = docType,
                Id = docVersion.DocId,
                Version = docVersion.Version,
                ModifiedBy = docVersion.ModifiedBy,
                ModifiedDate = docVersion.ModifiedDate,
                Pages = GetPageByPageVersions(docVersion.PageVersions),
                FieldValues = GetFieldValueByDocumentFieldVersion(docVersion.DocumentFieldVersions),
                DocVersionId = docVersion.Id
            };
        }

        public static List<DocumentModel> GetDocumentByDocVersions(List<DocumentVersion> docVersions)
        {
            if (docVersions == null)
            {
                return null;
            }

            return docVersions.Select(GetDocumentByDocVersion).ToList();
        }

        public static FieldMetaDataModel GetFieldMetaDataByFieldMetaDataVersion(FieldMetadataVersion fieldMetaData)
        {
            if (fieldMetaData == null)
            {
                return null;
            }

            return new FieldMetaDataModel
            {
                DataType = fieldMetaData.DataTypeEnum,
                DefaultValue = fieldMetaData.DefautValue,
                DisplayOrder = fieldMetaData.DisplayOrder,
                DocTypeId = fieldMetaData.DocTypeId,
                Id = fieldMetaData.Id,
                Name = fieldMetaData.Name,
                MaxLength = fieldMetaData.MaxLength,
                IsRequired = fieldMetaData.IsRequired,
                IsLookup = fieldMetaData.IsLookup,
                IsRestricted = fieldMetaData.IsRestricted,
                IsSystemField = fieldMetaData.IsSystemField
            };
        }

        public static List<FieldMetaDataModel> GetFieldMetaDataByFieldMetaDataVersions(List<FieldMetadataVersion> fieldVerions)
        {
            if (fieldVerions == null)
            {
                return null;
            }

            return fieldVerions.Select(GetFieldMetaDataByFieldMetaDataVersion).ToList();
        }

        public static FieldValueModel GetFieldValueByDocumentFieldVersion(DocumentFieldVersion fieldValueVersion)
        {
            if (fieldValueVersion == null)
            {
                return null;
            }

            return new FieldValueModel
            {
                Field = GetFieldMetaDataByFieldMetaDataVersion(fieldValueVersion.FieldMetadataVersion),
                Value = fieldValueVersion.Value
            };
        }

        public static List<FieldValueModel> GetFieldValueByDocumentFieldVersion(List<DocumentFieldVersion> fieldValueVersions)
        {
            if (fieldValueVersions == null)
            {
                return null;
            }

            return fieldValueVersions.Select(GetFieldValueByDocumentFieldVersion).ToList();
        }

        public static PageModel GetPageByPageVersion(PageVersion pageVersion)
        {
            if (pageVersion == null)
            {
                return null;
            }

            return new PageModel
            {
                DocId = pageVersion.DocId,
                FileBinaries = pageVersion.FileBinary,
                FileExtension = pageVersion.FileExtension,
                FileHash = pageVersion.FileHash,
                Id = pageVersion.PageId,
                Height = pageVersion.Height,
                RotateAngle = pageVersion.RotateAngle,
                PageNumber = pageVersion.PageNumber,
                Width = pageVersion.Width,
                Annotations = GetAnnotationByAnnotationVersion(pageVersion.AnnotationVersions)
            };
        }

        public static List<PageModel> GetPageByPageVersions(List<PageVersion> pageVersions)
        {
            if (pageVersions == null)
            {
                return null;
            }

            return pageVersions.Select(GetPageByPageVersion).ToList();
        }

        public static AnnotationModel GetAnnotationByAnnotationVersion(AnnotationVersion annotationVersion)
        {
            if (annotationVersion == null)
            {
                return null;
            }

            return new AnnotationModel
            {
                Content = annotationVersion.Content,
                CreatedBy = annotationVersion.CreatedBy,
                CreatedOn = annotationVersion.CreatedOn,
                Height = annotationVersion.Height,
                Id = annotationVersion.AnnotationId,
                Left = annotationVersion.Left,
                LineColor = annotationVersion.LineColor,
                LineEndAt = (RectangleVertexModel)Enum.Parse(typeof(RectangleVertexModel), annotationVersion.LineEndAt),
                LineStartAt = (RectangleVertexModel)Enum.Parse(typeof(RectangleVertexModel), annotationVersion.LineStartAt),
                LineStyle = (LineStyleModel)Enum.Parse(typeof(LineStyleModel), annotationVersion.LineStyle),
                LineWeight = annotationVersion.LineWeight,
                ModifiedBy = annotationVersion.ModifiedBy,
                ModifiedOn = annotationVersion.ModifiedOn,
                PageId = annotationVersion.PageId,
                RotateAngle = annotationVersion.RotateAngle,
                Top = annotationVersion.Top,
                Type = (AnnotationTypeModel)Enum.Parse(typeof(AnnotationTypeModel), annotationVersion.Type),
                Width = annotationVersion.Width
            };
        }

        public static List<AnnotationModel> GetAnnotationByAnnotationVersion(List<AnnotationVersion> annotationVersions)
        {
            if (annotationVersions == null)
                return null;

            return annotationVersions.Select(GetAnnotationByAnnotationVersion).ToList();
        }

        public static DocumentTypeModel GetDocumentTypeByDocumentTypeVersion(DocumentTypeVersion docTypeVersion)
        {
            if (docTypeVersion == null)
            {
                return null;
            }

            return new DocumentTypeModel
            {
                CreateBy = docTypeVersion.CreatedBy,
                CreatedDate = docTypeVersion.CreatedDate,
                Id = docTypeVersion.Id,
                ModifiedBy = docTypeVersion.ModifiedBy,
                ModifiedDate = docTypeVersion.ModifiedDate,
                Name = docTypeVersion.Name
            };

        }

        public static List<DocumentTypeModel> GetDocumentTypesByDocumentTypeVersion(List<DocumentTypeVersion> documentTypeVersions)
        {
            if (documentTypeVersions == null)
            {
                return null;
            }

            return documentTypeVersions.Select(GetDocumentTypeByDocumentTypeVersion).ToList();
        }

        public static AuditPermission GetAuditPermission(AuditPermissionModel auditPermissionModel)
        {
            if (auditPermissionModel == null)
            {
                return null;
            }

            return new AuditPermission
            {
                AllowedAudit = auditPermissionModel.AllowedAudit,
                AllowedDeleteLog = auditPermissionModel.AllowedDeleteLog,
                AllowedViewLog = auditPermissionModel.AllowedViewLog,
                AllowedRestoreDocument = auditPermissionModel.AllowedRestoreDocument,
                AllowedViewReport = auditPermissionModel.AllowedViewReport,
                Id = auditPermissionModel.Id,
                UserGroupId = auditPermissionModel.UserGroupId,
                DocTypeId = auditPermissionModel.DocTypeId
            };
        }

        public static AuditPermissionModel GetAuditPermissionModel(AuditPermission auditPermission)
        {
            if (auditPermission == null)
            {
                return null;
            }

            return new AuditPermissionModel
            {
                AllowedAudit = auditPermission.AllowedAudit,
                AllowedDeleteLog = auditPermission.AllowedDeleteLog,
                AllowedRestoreDocument = auditPermission.AllowedRestoreDocument,
                AllowedViewLog = auditPermission.AllowedViewLog,
                AllowedViewReport = auditPermission.AllowedViewReport,
                Id = auditPermission.Id,
                UserGroupId = auditPermission.UserGroupId,
                DocTypeId = auditPermission.DocTypeId
            };
        }
    }
}
