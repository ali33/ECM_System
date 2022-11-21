using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ecm.CaptureDomain;
using Ecm.BarcodeDomain;
using Ecm.LookupDomain;

namespace Ecm.CaptureModel.DataProvider
{
    public class ObjectMapper
    {
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
                Name = languageModel.Name
            };
        }

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
                ApplyForArchive = user.ApplyForArchive
            };
        }

        public static ObservableCollection<UserModel> GetUserModels(IList<User> users)
        {
            var userModels = new ObservableCollection<UserModel>();

            if (users != null)
            {
                foreach (User user in users)
                {
                    UserModel userModel = GetUserModel(user);
                    if (userModel == null)
                    {
                        continue;
                    }

                    userModels.Add(userModel);
                }
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
                EncryptedPassword = user.EncryptedPassword,
                UserGroups = GetUserGroups(user.UserGroups),
                Photo = user.Picture,
                ApplyForArchive = user.ApplyForArchive
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
                userModels = GetUserModels(userGroup.Users);
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
            var userGroupModels = new ObservableCollection<UserGroupModel>();

            if (userGroups != null)
            {
                foreach (var userGroup in userGroups)
                {
                    userGroupModels.Add(GetUserGroupModel(userGroup));
                }
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

        public static BatchTypePermissionModel GetBatchTypePermissionModel(BatchTypePermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new BatchTypePermissionModel
            {
                CanCapture = permission.CanCapture,
                CanAccess = permission.CanAccess,
                CanIndex = permission.CanIndex,
                CanClassify = permission.CanClassify,
                BatchTypeId = permission.BatchTypeId,
                UserGroupId = permission.UserGroupId,
                Id = permission.Id
            };
        }

        public static BatchTypePermission GetBatchTypePermission(BatchTypePermissionModel permissionModel)
        {
            if (permissionModel == null)
            {
                return null;
            }

            return new BatchTypePermission
            {
                CanCapture = permissionModel.CanCapture,
                CanAccess = permissionModel.CanAccess,
                CanClassify = permissionModel.CanClassify,
                CanIndex = permissionModel.CanIndex,
                BatchTypeId = permissionModel.BatchTypeId,
                UserGroupId = permissionModel.UserGroupId,
                Id = permissionModel.Id
            };
        }

        public static ObservableCollection<BatchTypeModel> GetBatchTypeModels(IList<BatchType> batchTypes)
        {
            var batchTypeModels = new ObservableCollection<BatchTypeModel>();

            if (batchTypes != null)
            {
                foreach (BatchType batchType in batchTypes)
                {
                    batchTypeModels.Add(GetBatchTypeModel(batchType));
                }
            }

            return batchTypeModels;
        }

        public static BatchTypeModel GetBatchTypeModel(BatchType batchType)
        {
            if (batchType == null)
            {
                return null;
            }

            return new BatchTypeModel
                       {
                           Id = batchType.Id,
                           Name = batchType.Name,
                           Fields = GetBatchFieldMetaDataModels(batchType.Fields),
                           DocTypes = GetDocTypeModels(batchType.DocTypes),
                           CreatedBy = batchType.CreatedBy,
                           CreatedDate = batchType.CreatedDate,
                           ModifiedBy = batchType.ModifiedBy,
                           ModifiedDate = batchType.ModifiedDate,
                           UniqueId = batchType.UniqueId,
                           Icon = batchType.Icon,
                           WorkflowDefinitionId = batchType.WorkflowDefinitionId,
                           BatchTypePermission = GetBatchTypePermissionModel(batchType.BatchTypePermission),
                           BarcodeConfiguration = GetBarcodeConfigurationModel(batchType.BarcodeConfiguration),
                           Description = batchType.Description,
                           BarcodeConfigurationXml = batchType.BarcodeConfigurationXml,
                           IsApplyForOutlook = batchType.IsApplyForOutlook
                       };
        }

        public static ObservableCollection<FieldModel> GetBatchFieldMetaDataModels(IList<BatchFieldMetaData> fields)
        {
            var fieldMetaDataModels = new ObservableCollection<FieldModel>();

            if (fields != null)
            {
                foreach (BatchFieldMetaData field in fields)
                {
                    fieldMetaDataModels.Add(GetBatchFieldMetaDataModel(field));
                }
            }

            return fieldMetaDataModels;
        }

        public static FieldModel GetBatchFieldMetaDataModel(BatchFieldMetaData field)
        {
            if (field == null)
            {
                return null;
            }

            return new FieldModel
            {
                DataType = field.DataTypeEnum,
                MaxLength = field.MaxLength,
                DefaultValue = field.DefaultValue,
                DisplayOrder = field.DisplayOrder,
                BatchTypeId = field.BatchTypeId,
                Id = field.Id,
                IsSystemField = field.IsSystemField,
                UniqueId = field.UniqueId,
                Name = field.Name,
                UseCurrentDate = field.UseCurrentDate,
                LookupInfo = GetLookupInfoModel(field.LookupInfo),
                Maps = GetLookupMappingModels(field.LookupInfo == null ? null : field.LookupInfo.LookupMaps, field.LookupInfo == null ? Guid.Empty : field.LookupInfo.FieldId),
                RuntimeLookupMaps = field.RuntimeLookupMaps,
                IsLookup = field.IsLookup
            };
        }

        public static ObservableCollection<DocTypeModel> GetDocTypeModels(List<DocumentType> docTypes)
        {
            var docTypeModels = new ObservableCollection<DocTypeModel>();

            if (docTypes != null)
            {
                foreach (DocumentType docType in docTypes.OrderBy(h => h.Name))
                {
                    docTypeModels.Add(GetDocTypeModel(docType));
                }
            }

            return docTypeModels;
        }

        public static DocTypeModel GetDocTypeModel(DocumentType documentType)
        {
            if (documentType == null)
            {
                return null;
            }

            return new DocTypeModel
                       {
                           Id = documentType.Id,
                           Name = documentType.Name,
                           Fields = GetDocFieldMetaDataModels(documentType.Fields),
                           CreatedBy = documentType.CreatedBy,
                           CreatedDate = documentType.CreatedDate,
                           ModifiedBy = documentType.ModifiedBy,
                           ModifiedDate = documentType.ModifiedDate,
                           UniqueId = documentType.UniqueId,
                           Icon = documentType.Icon,
                           OCRTemplate = GetOCRTemplateModel(documentType.OCRTemplate),
                           //BarcodeConfigurations = new ObservableCollection<BarcodeConfigurationModel>(GetBarcodeConfigurationModels(documentType.BarcodeConfigurations)),
                           DocTypePermission = GetDocTypePermissionModel(documentType.DocumentTypePermission),
                           Description = documentType.Description,
                           BatchTypeId = documentType.BatchTypeId
                       };
        }

        public static DocTypePermissionModel GetDocTypePermissionModel(DocumentTypePermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocTypePermissionModel
            {
                CanAccess = permission.CanAccess,
                DocTypeId = permission.DocTypeId,
                UserGroupId = permission.UserGroupId
            };
        }

        public static ObservableCollection<DocTypePermissionModel> GetDocTypePermissionModels(List<DocumentTypePermission> permissions)
        {
            return new ObservableCollection<DocTypePermissionModel>(permissions.Select(GetDocTypePermissionModel).ToList());
        }

        public static DocumentTypePermission GetDocTypePermission(DocTypePermissionModel permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocumentTypePermission
            {
                CanAccess = permission.CanAccess,
                DocTypeId = permission.DocTypeId,
                UserGroupId = permission.UserGroupId
            };
        }

        public static List<DocumentTypePermission> GetDocTypePermissions(List<DocTypePermissionModel> permissions)
        {
            return permissions.Select(p => GetDocTypePermission(p)).ToList();
        }


        public static DocumentPermissionModel GetDocumentPermissionModel(DocumentPermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocumentPermissionModel
            {
                CanSeeRestrictedField = permission.CanSeeRestrictedField,
                DocTypeId = permission.DocTypeId,
                UserGroupId = permission.UserGroupId,
                FieldPermissions = GetFieldPermissionModels(permission.FieldPermissions).ToList()
            };
        }

        public static ObservableCollection<DocumentPermissionModel> GetDocumentPermissionModels(List<DocumentPermission> permissions)
        {
            return new ObservableCollection<DocumentPermissionModel>(permissions.Select(GetDocumentPermissionModel).ToList());
        }

        public static DocumentPermission GetDocumentPermission(DocumentPermissionModel permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocumentPermission
            {
                CanSeeRestrictedField = permission.CanSeeRestrictedField,
                DocTypeId = permission.DocTypeId,
                UserGroupId = permission.UserGroupId,
                FieldPermissions = GetFieldPermissions(new ObservableCollection<DocumentFieldPermissionModel>(permission.FieldPermissions))
            };
        }

        public static List<DocumentPermission> GetDocmentPermissions(List<DocumentPermissionModel> permissions)
        {
            return permissions.Select(p => GetDocumentPermission(p)).ToList();
        }

        public static ObservableCollection<FieldModel> GetDocFieldMetaDataModels(IList<DocumentFieldMetaData> fields)
        {
            var fieldMetaDataModels = new ObservableCollection<FieldModel>();

            if (fields != null)
            {
                foreach (DocumentFieldMetaData field in fields)
                {
                    fieldMetaDataModels.Add(GetDocFieldMetaDataModel(field));
                }
            }

            return fieldMetaDataModels;
        }

        public static FieldModel GetDocFieldMetaDataModel(DocumentFieldMetaData field)
        {
            if (field == null)
            {
                return null;
            }

            var fieldModel = new FieldModel
            {
                DataType = field.DataTypeEnum,
                MaxLength = field.MaxLength,
                DefaultValue = field.DefaultValue,
                DisplayOrder = field.DisplayOrder,
                DocTypeId = field.DocTypeId,
                Id = field.Id,
                IsLookup = field.IsLookup,
                IsRequired = field.IsRequired,
                IsRestricted = field.IsRestricted,
                IsSystemField = field.IsSystemField,
                UniqueId = field.UniqueId,
                Name = field.Name,
                UseCurrentDate = field.UseCurrentDate,
                OCRTemplateZone = GetOCRTemplateZoneModel(field.OCRTemplateZone),
                ValidationPattern = field.ValidationPattern,
                ValidationScript = field.ValidationScript,
                LookupInfo = GetLookupInfoModel(field.LookupInfo),
                Maps = GetLookupMappingModels(field.LookupInfo == null ? null : field.LookupInfo.LookupMaps, field.LookupInfo == null ? Guid.Empty : field.LookupInfo.FieldId),
                RuntimeLookupMaps = field.RuntimeLookupMaps,
                Children = field.Children == null ? null : new ObservableCollection<TableColumnModel>(GetTableColumnModels(field.Children)),
                ParentFieldId = field.ParentFieldId,
                Picklists = new ObservableCollection<PicklistModel>(GetPicklistsModel(field.Picklists))
            };


            if (fieldModel.DataType == FieldDataType.Date && fieldModel.UseCurrentDate)
            {
                fieldModel.DefaultValue = DateTime.Now.ToString("yyyy-MM-dd");
            }

            return fieldModel;
        }

        public static LookupMapModel GetLookupMappingModel(LookupMap lookupMapping, Guid lookupFieldId)
        {
            if (lookupMapping == null)
            {
                return null;
            }

            return new LookupMapModel
            {
                DataColumn = lookupMapping.DataColumn,
                LookupFieldId = lookupFieldId,
                Name = lookupMapping.FieldName,
            };
        }

        public static ObservableCollection<LookupMapModel> GetLookupMappingModels(List<LookupMap> mappings, Guid lookupFieldId)
        {
            if (mappings == null)
            {
                return null;
            }

            return new ObservableCollection<LookupMapModel>(mappings.Select(p => GetLookupMappingModel(p, lookupFieldId)));
        }

        public static LookupInfoModel GetLookupInfoModel(LookupInfo lookupInfo)
        {
            if (lookupInfo == null)
            {
                return null;
            }

            return new LookupInfoModel
            {
                Connection = GetLookupConnectionModel(lookupInfo.ConnectionInfo),
                LookupAtLostFocus = lookupInfo.LookupWhenTabOut,
                LookupType = (int)lookupInfo.LookupType,
                MaxLookupRow = lookupInfo.MaxLookupRow,
                MinPrefixLength = lookupInfo.MinPrefixLength,
                SourceName = lookupInfo.LookupObjectName,
                SqlCommand = lookupInfo.QueryCommand,
                FieldId = lookupInfo.FieldId,
                LookupColumn = lookupInfo.LookupColumn,
                LookupOperator = lookupInfo.LookupOperator,
                LookupMaps = lookupInfo.LookupMaps.Select(h => new LookupMap()
                {
                    DataColumn = h.DataColumn,
                    FieldId = h.FieldId,
                    FieldName = h.FieldName
                }).ToList()
            };
        }

        public static LookupConnectionModel GetLookupConnectionModel(ConnectionInfo connection)
        {
            if (connection == null)
            {
                return null;
            }

            return new LookupConnectionModel
            {
                DatabaseName = connection.DatabaseName,
                DatabaseType = (int)connection.DbType,
                Host = connection.Host,
                Password = connection.Password,
                Port = connection.Port,
                ProviderType = (int)connection.ProviderType,
                Schema = connection.Schema,
                Username = connection.Username
            };
        }

        public static LookupMap GetLookupMapping(LookupMapModel lookupMapping, Guid lookupFieldId)
        {
            if (lookupMapping == null || lookupFieldId == Guid.Empty)
            {
                return null;
            }

            return new LookupMap
            {
                DataColumn = lookupMapping.DataColumn,
                FieldId = lookupFieldId,
                FieldName = lookupMapping.Name
            };
        }

        public static ObservableCollection<LookupMap> GetLookupMappingModels(List<LookupMapModel> mappings, Guid lookupFieldId)
        {
            if (mappings == null)
            {
                return null;
            }

            return new ObservableCollection<LookupMap>(mappings.Select(p => GetLookupMapping(p, lookupFieldId)));
        }

        public static LookupInfo GetLookupInfo(LookupInfoModel lookupInfo)
        {
            if (lookupInfo == null)
            {
                return null;
            }

            return new LookupInfo
            {
                ConnectionInfo = GetLookupConnection(lookupInfo.Connection),
                LookupWhenTabOut = lookupInfo.LookupAtLostFocus,
                LookupType = (LookupType)lookupInfo.LookupType,
                MaxLookupRow = lookupInfo.MaxLookupRow,
                MinPrefixLength = lookupInfo.MinPrefixLength,
                LookupObjectName = lookupInfo.SourceName,
                QueryCommand = lookupInfo.SqlCommand,
                FieldId = lookupInfo.FieldId,
                LookupColumn = lookupInfo.LookupColumn,
                LookupOperator = lookupInfo.LookupOperator,
                LookupMaps = lookupInfo.LookupMaps.Select(h => new Ecm.LookupDomain.LookupMap()
                {
                    DataColumn = h.DataColumn,
                    FieldId = h.FieldId,
                    FieldName = h.FieldName
                }).ToList()
            };
        }

        public static ConnectionInfo GetLookupConnection(LookupConnectionModel connection)
        {
            if (connection == null)
            {
                return null;
            }

            return new ConnectionInfo
            {
                DatabaseName = connection.DatabaseName,
                DbType = (DatabaseType)connection.DatabaseType,
                Host = connection.Host,
                Password = connection.Password,
                Port = connection.Port,
                ProviderType = (ProviderType)connection.ProviderType,
                Schema = connection.Schema,
                Username = connection.Username
            };
        }

        public static BatchType GetBatchType(BatchTypeModel batchTypeModel)
        {
            if (batchTypeModel == null)
            {
                return null;
            }

            return new BatchType
            {
                Id = batchTypeModel.Id,
                Name = batchTypeModel.Name,
                Fields = GetBatchFieldMetaDatas(batchTypeModel.Fields),
                DeletedFields = batchTypeModel.DeletedFields.ToList(),
                DocTypes = GetDocTypes(batchTypeModel.DocTypes),
                DeletedDocTypes = batchTypeModel.DeletedDocTypes.ToList(),
                CreatedBy = batchTypeModel.CreatedBy,
                CreatedDate = batchTypeModel.CreatedDate,
                ModifiedBy = batchTypeModel.ModifiedBy,
                ModifiedDate = batchTypeModel.ModifiedDate,
                UniqueId = batchTypeModel.UniqueId,
                Icon = batchTypeModel.Icon,
                WorkflowDefinitionId = batchTypeModel.WorkflowDefinitionId,
                BatchTypePermission = GetBatchTypePermission(batchTypeModel.BatchTypePermission),
                BarcodeConfiguration = GetBarcodeConfiguration(batchTypeModel.BarcodeConfiguration),
                BarcodeConfigurationXml = batchTypeModel.BarcodeConfigurationXml,
                Description = batchTypeModel.Description,
                IsApplyForOutlook = batchTypeModel.IsApplyForOutlook
            };
        }

        public static List<BatchFieldMetaData> GetBatchFieldMetaDatas(IList<FieldModel> fieldModels)
        {
            var fields = new List<BatchFieldMetaData>();

            if (fieldModels != null)
            {
                fields.AddRange(fieldModels.Select(GetBatchFieldMetaData));
            }

            return fields;
        }

        public static BatchFieldMetaData GetBatchFieldMetaData(FieldModel fieldModel)
        {
            if (fieldModel == null)
            {
                return null;
            }

            return new BatchFieldMetaData
            {
                DataType = fieldModel.DataType.ToString(),
                MaxLength = fieldModel.MaxLength,
                DefaultValue = fieldModel.DefaultValue,
                DisplayOrder = fieldModel.DisplayOrder,
                BatchTypeId = fieldModel.BatchTypeId,
                Id = fieldModel.Id,
                IsSystemField = fieldModel.IsSystemField,
                UniqueId = fieldModel.UniqueId,
                Name = fieldModel.Name,
                UseCurrentDate = fieldModel.UseCurrentDate
            };
        }

        public static List<DocumentType> GetDocTypes(IList<DocTypeModel> docTypeModels)
        {
            var docTypes = new List<DocumentType>();

            if (docTypeModels != null)
            {
                docTypes.AddRange(docTypeModels.Select(GetDocType));
            }

            return docTypes;
        }

        public static DocumentType GetDocType(DocTypeModel docTypeModel)
        {
            if (docTypeModel == null)
            {
                return null;
            }

            return new DocumentType
            {
                Id = docTypeModel.Id,
                Name = docTypeModel.Name,
                Fields = GetDocFieldMetaDatas(docTypeModel.Fields),
                DeletedFields = docTypeModel.DeletedFields.ToList(),
                CreatedBy = docTypeModel.CreatedBy,
                CreatedDate = docTypeModel.CreatedDate,
                ModifiedBy = docTypeModel.ModifiedBy,
                ModifiedDate = docTypeModel.ModifiedDate,
                UniqueId = docTypeModel.UniqueId,
                Icon = docTypeModel.Icon,
                DocumentTypePermission = GetDocTypePermission(docTypeModel.DocTypePermission),
                Description = docTypeModel.Description,
                BatchTypeId = docTypeModel.BatchTypeId,
                OCRTemplate = GetOCRTemplate(docTypeModel.OCRTemplate)
            };
        }

        public static List<DocumentFieldMetaData> GetDocFieldMetaDatas(IList<FieldModel> fieldModels)
        {
            var fieldMetaDatas = new List<DocumentFieldMetaData>();

            if (fieldModels != null)
            {
                fieldMetaDatas.AddRange(fieldModels.Select(GetDocFieldMetaData));
            }

            return fieldMetaDatas;
        }

        public static DocumentFieldMetaData GetDocFieldMetaData(FieldModel fieldModel)
        {
            if (fieldModel == null)
            {
                return null;
            }

            return new DocumentFieldMetaData
            {
                DataType = fieldModel.DataType.ToString(),
                MaxLength = fieldModel.MaxLength,
                DefaultValue = fieldModel.DefaultValue,
                DisplayOrder = fieldModel.DisplayOrder,
                DocTypeId = fieldModel.DocTypeId,
                Id = fieldModel.Id,
                IsLookup = fieldModel.IsLookup,
                IsRequired = fieldModel.IsRequired,
                IsRestricted = fieldModel.IsRestricted,
                IsSystemField = fieldModel.IsSystemField,
                UniqueId = fieldModel.UniqueId,
                Name = fieldModel.Name,
                UseCurrentDate = fieldModel.UseCurrentDate,
                ValidationScript = fieldModel.ValidationScript,
                ValidationPattern = fieldModel.ValidationPattern,
                Children = fieldModel.Children == null ? null : GetTableFieldMetadatas(fieldModel.Children.ToList()),
                DeleteChildIds = fieldModel.DeletedChildrenIds,
                ParentFieldId = fieldModel.ParentFieldId,
                Picklists = GetPicklists(fieldModel.Picklists.ToList())
            };
        }

        public static DocumentFieldMetaData GetDocFieldMetaData(DocFieldMetaDataModel fieldModel)
        {
            if (fieldModel == null)
            {
                return null;
            }

            return new DocumentFieldMetaData
            {
                DataType = fieldModel.DataType.ToString(),
                MaxLength = fieldModel.MaxLength,
                DefaultValue = fieldModel.DefaultValue,
                DisplayOrder = fieldModel.DisplayOrder,
                DocTypeId = fieldModel.DocTypeId,
                Id = fieldModel.Id,
                IsLookup = fieldModel.IsLookup,
                IsRequired = fieldModel.IsRequired,
                IsRestricted = fieldModel.IsRestricted,
                IsSystemField = fieldModel.IsSystemField,
                UniqueId = fieldModel.UniqueId,
                Name = fieldModel.Name,
                UseCurrentDate = fieldModel.UseCurrentDate,
                ValidationScript = fieldModel.ValidationScript,
                ValidationPattern = fieldModel.ValidationPattern,
                ParentFieldId = fieldModel.ParentFieldId,
                Picklists = GetPicklists(fieldModel.Picklists.ToList())
            };
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

        public static List<Picklist> GetPicklists(List<PicklistModel> picklists)
        {
            return picklists.Select(GetPicklist).ToList();
        }

        public static List<PicklistModel> GetPicklistsModel(List<Picklist> picklists)
        {
            return picklists.Select(GetPicklistModel).ToList();
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
                OCRTemplatePages = GetOCRTemplatePageModels(ocrTemplate.OCRTemplatePages)
            };
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

        public static List<OCRTemplatePageModel> GetOCRTemplatePageModels(IList<OCRTemplatePage> ocrTemplatePages)
        {
            if (ocrTemplatePages == null)
            {
                return null;
            }

            return ocrTemplatePages.Select(GetOCRTemplatePageModel).ToList();
        }

        public static List<OCRTemplatePage> GetOCRTemplatePages(IList<OCRTemplatePageModel> ocrTemplatePages)
        {
            if (ocrTemplatePages == null)
            {
                return null;
            }

            return ocrTemplatePages.Select(GetOCRTemplatePage).ToList();
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

        public static List<OCRTemplateZoneModel> GetOCRTemplateZoneModels(IList<OCRTemplateZone> ocrTemplateZones)
        {
            if (ocrTemplateZones == null)
            {
                return null;
            }

            return ocrTemplateZones.Select(GetOCRTemplateZoneModel).ToList();
        }

        public static List<OCRTemplateZone> GetOCRTemplateZones(IList<OCRTemplateZoneModel> ocrTemplateZones)
        {
            if (ocrTemplateZones == null)
            {
                return null;
            }

            return ocrTemplateZones.Select(GetOCRTemplateZone).ToList();
        }

        public static OCRTemplateZoneModel GetOCRTemplateZoneModel(OCRTemplateZone ocrTemplateZone)
        {
            if (ocrTemplateZone == null)
            {
                return null;
            }

            return new OCRTemplateZoneModel
            {
                FieldMetaDataId = ocrTemplateZone.FieldMetaDataId,
                Height = ocrTemplateZone.Height,
                Width = ocrTemplateZone.Width,
                Left = ocrTemplateZone.Left,
                Top = ocrTemplateZone.Top,
                OCRTemplatePageId = ocrTemplateZone.OCRTemplatePageId,
                CreatedBy = ocrTemplateZone.CreatedBy,
                CreatedOn = ocrTemplateZone.CreatedOn,
                ModifiedBy = ocrTemplateZone.ModifiedBy,
                ModifiedOn = ocrTemplateZone.ModifiedOn,
                FieldMetaData = GetDocFieldMetaDataModel(ocrTemplateZone.FieldMetaData)
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
                ModifiedOn = ocrTemplateZone.ModifiedOn,
                FieldMetaData = GetDocFieldMetaData(ocrTemplateZone.FieldMetaData)
            };
        }

        public static BatchBarcodeConfiguration GetBarcodeConfiguration(BarcodeConfigurationModel barcodeConfigurationModel)
        {
            if (barcodeConfigurationModel == null)
            {
                return null;
            }

            return new BatchBarcodeConfiguration
            {
                ReadActions = barcodeConfigurationModel.ReadActions.Select(p => GetReadAction(p)).ToList(),
                SeparationActions = barcodeConfigurationModel.SeparationActions.Select(p => GetSeparationAction(p)).ToList()
            };
        }

        public static List<BarcodeConfigurationModel> GetBarcodeConfigurationModels(List<BatchBarcodeConfiguration> barcodeConfigurations)
        {
            if (barcodeConfigurations == null)
            {
                return null;
            }

            return barcodeConfigurations.Select(GetBarcodeConfigurationModel).ToList();
        }

        public static BarcodeConfigurationModel GetBarcodeConfigurationModel(BatchBarcodeConfiguration barcodeConfiguration)
        {
            if (barcodeConfiguration == null)
            {
                return null;
            }

            return new BarcodeConfigurationModel
            {
                ReadActions = new ObservableCollection<ReadActionModel>(barcodeConfiguration.ReadActions.Select(p => GetReadActionModel(p))),
                SeparationActions = new ObservableCollection<SeparationActionModel>(barcodeConfiguration.SeparationActions.Select(p => GetSeparationActionModel(p)))
            };
        }

        public static ReadAction GetReadAction(ReadActionModel actionModel)
        {
            if (actionModel == null)
            {
                return null;
            }

            return new ReadAction
            {
                BarcodePositionInDoc = actionModel.BarcodePositionInDoc,
                BarcodeType = (int)actionModel.BarcodeType,
                CopyValueToFields = actionModel.CopyValueToFields.Select(p => GetCopyToField(p)).ToList(),
                DocTypeId = actionModel.DocTypeId,
                Id = actionModel.Id,
                IsDocIndex = actionModel.IsDocIndex,
                OverwriteFieldValue = actionModel.OverwriteFieldValue,
                Separator = actionModel.Separator,
                StartsWith = actionModel.StartsWith
            };
        }

        public static SeparationAction GetSeparationAction(SeparationActionModel actionModel)
        {
            if (actionModel == null)
            {
                return null;
            }

            return new SeparationAction
            {
                BarcodePositionInDoc = actionModel.BarcodePositionInDoc,
                BarcodeType = (int)actionModel.BarcodeType,
                DocTypeId = actionModel.DocTypeId,
                Id = actionModel.Id,
                StartsWith = actionModel.StartsWith,
                HasSpecifyDocumentType = actionModel.HasSpecifyDocumentType,
                RemoveSeparatorPage = actionModel.RemoveSeparatorPage
            };
        }

        public static CopyValueToField GetCopyToField(CopyValueToFieldModel copyToFieldModel)
        {
            if (copyToFieldModel == null)
            {
                return null;
            }

            return new CopyValueToField
            {
                FieldGuid = copyToFieldModel.FieldGuid,
                FieldName = copyToFieldModel.FieldName,
                Position = copyToFieldModel.Position
            };
        }

        public static ReadActionModel GetReadActionModel(ReadAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new ReadActionModel
            {
                BarcodePositionInDoc = action.BarcodePositionInDoc,
                BarcodeType = (BarcodeTypeModel)action.BarcodeType,
                CopyValueToFields = action.CopyValueToFields.Select(p => GetCopyToFieldModel(p)).ToList(),
                DocTypeId = action.DocTypeId,
                Id = action.Id,
                IsDocIndex = action.IsDocIndex,
                OverwriteFieldValue = action.OverwriteFieldValue,
                Separator = action.Separator,
                StartsWith = action.StartsWith
            };
        }

        public static SeparationActionModel GetSeparationActionModel(SeparationAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new SeparationActionModel
            {
                BarcodePositionInDoc = action.BarcodePositionInDoc,
                BarcodeType = (BarcodeTypeModel)action.BarcodeType,
                DocTypeId = action.DocTypeId,
                Id = action.Id,
                StartsWith = action.StartsWith,
                HasSpecifyDocumentType = action.HasSpecifyDocumentType,
                RemoveSeparatorPage = action.RemoveSeparatorPage
            };
        }

        public static CopyValueToFieldModel GetCopyToFieldModel(CopyValueToField copyToField)
        {
            if (copyToField == null)
            {
                return null;
            }

            return new CopyValueToFieldModel
            {
                FieldGuid = copyToField.FieldGuid,
                FieldName = copyToField.FieldName,
                Position = copyToField.Position
            };
        }
        //public static WorkItem GetWorkItem(DocumentModel document)
        //{
        //    if (document == null)
        //    {
        //        return null;
        //    }

        //    return new WorkItem
        //    {
        //        CreatedBy = document.CreatedBy,
        //        CreatedDate = document.CreatedDate,
        //        DocumentType = GetDocType(document.DocumentType),
        //        FieldValues = GetFieldValues(document.FieldValues),
        //        Id = document.Id,
        //        ModifiedBy = document.ModifiedBy,
        //        ModifiedDate = document.ModifiedDate,
        //        PageCount = document.PageCount,
        //        Pages = GetPages(document.Pages),
        //        DeletedPages = document.DeletedPages,
        //        BinaryType = document.BinaryType.ToString(),
        //        DocTypeId = document.DocumentType.Id
        //    };
        //}

        //public static List<WorkItem> GetWorkItems(IList<DocumentModel> documents)
        //{
        //    if (documents == null)
        //    {
        //        return null;
        //    }

        //    return documents.Select(GetWorkItem).ToList();
        //}

        public static Page GetPage(PageModel page)
        {
            if (page == null)
            {
                return null;
            }

            return new Page
            {
                DocId = page.DocId,
                FileBinary = page.FileBinary,
                FileHash = page.FileHash,
                Id = page.Id,
                PageNumber = page.PageNumber,
                Annotations = GetAnnotations(page.Annotations),
                FileExtension = page.FileExtension,
                FileHeader = page.FileHeader,
                FilePath = page.FilePath,
                RotateAngle = page.RotateAngle,
                Height = page.Height,
                Width = page.Width,
                Content = page.Content,
                ContentLanguageCode = page.ContentLanguageCode,
                IsRejected = page.IsRejected,
                OriginalFileName = page.OriginalFileName,
                DeleteAnnotations = page.DeleteAnnotations
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
                FileBinary = page.FileBinary,
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
                FilePath = page.FilePath,
                FileHeader = page.FileHeader,
                OriginalFileName = page.OriginalFileName,
                IsRejected = page.IsRejected
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
                Width = annotation.Width,
                PageId = annotation.PageId
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
                // 2014/08/15 - HungLe - Star - Editing set value for ModifedOn
                // ModifiedOn = annotation.ModifiedOn,
                ModifiedOn = annotation.ModifiedOn.HasValue ? annotation.ModifiedOn.Value : DateTime.MinValue,
                // 2014/08/15 - HungLe - End - Editing set value for ModifedOn
                RotateAngle = annotation.RotateAngle,
                Top = annotation.Top,
                Type = (AnnotationTypeModel)Enum.Parse(typeof(AnnotationTypeModel), annotation.Type),
                Width = annotation.Width,
                PageId = annotation.PageId
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

        public static DocumentFieldValue GetFieldValue(FieldValueModel fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }

            return new DocumentFieldValue
            {
                FieldMetaData = GetDocFieldMetaData(fieldValue.Field),
                TableFieldValue = fieldValue.TableValues == null ? null : GetTableFieldValues(fieldValue.TableValues.ToList()),
                Value = fieldValue.Value,
                FieldId = fieldValue.Field.Id,
                DocId = fieldValue.DocId,
                Id = fieldValue.Id
            };
        }

        public static List<DocumentFieldValue> GetFieldValues(List<FieldValueModel> fieldValues)
        {
            if (fieldValues == null)
            {
                return null;
            }

            return fieldValues.Select(GetFieldValue).ToList();
        }

        public static FieldValueModel GetFieldValueModel(DocumentFieldValue fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }

            return new FieldValueModel
            {
                Field = GetDocFieldMetaDataModel(fieldValue.FieldMetaData),
                TableValues = GetTableFieldValueModels(fieldValue.TableFieldValue),
                DocId = fieldValue.DocId,
                FieldId = fieldValue.FieldId,
                Id = fieldValue.Id,
                Value = fieldValue.Value,
                BarcodeOverride = fieldValue.BarcodeOverride,
                BarcodeValue = fieldValue.BarcodeValue
            };
        }

        public static List<FieldValueModel> GetFieldValueModels(IList<DocumentFieldValue> fieldValues)
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

            var model = new TableFieldValue()
            {
                Field = GetDocFieldMetaData(tableFieldValue.Field),
                Id = tableFieldValue.Id,
                RowNumber = tableFieldValue.RowNumber,
                Value = tableFieldValue.Value,
                FieldId = tableFieldValue.Field.Id
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
                Field = GetDocFieldMetaDataModel(tableFieldValue.Field),
                Id = tableFieldValue.Id,
                RowNumber = tableFieldValue.RowNumber,
                Value = tableFieldValue.Value,
                FieldId = tableFieldValue.FieldId
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

        public static BatchFieldValue GetBatchFieldValue(FieldValueModel fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }

            return new BatchFieldValue
            {
                FieldMetaData = GetBatchFieldMetaData(fieldValue.Field),
                //TableFieldValue = fieldValue.TableValues == null ? null : GetTableFieldValues(fieldValue.TableValues.ToList()),
                Value = fieldValue.Value,
                FieldId = fieldValue.Field.Id,
                BatchId = fieldValue.BatchId,
                Id = fieldValue.Id
            };
        }

        public static List<BatchFieldValue> GetBatchFieldValues(List<FieldValueModel> fieldValues)
        {
            if (fieldValues == null)
            {
                return null;
            }

            return fieldValues.Select(GetBatchFieldValue).ToList();
        }

        public static FieldValueModel GetBatchFieldValueModel(BatchFieldValue fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }

            return new FieldValueModel
            {
                Field = GetBatchFieldMetaDataModel(fieldValue.FieldMetaData),
                //TableValues = GetTableFieldValueModels(fieldValue.TableFieldValue),
                Value = fieldValue.Value,
                Id = fieldValue.Id,
                BatchId = fieldValue.BatchId,
                FieldId = fieldValue.FieldId,
                BarcodeOverride = fieldValue.BarcodeOverride,
                BarcodeValue = fieldValue.BarcodeValue
            };
        }

        public static List<FieldValueModel> GetBatchFieldValueModels(IList<BatchFieldValue> fieldValues)
        {
            if (fieldValues == null)
            {
                return null;
            }

            return fieldValues.Select(GetBatchFieldValueModel).ToList();
        }

        public static Document GetDocument(DocumentModel docModel)
        {
            if (docModel == null)
            {
                return null;
            }

            return new Document()
            {
                BatchId = docModel.BatchId,
                // HungLe - 2014/07/18 - Adding doc name - Start
                DocName = docModel.DocName,
                // HungLe - 2014/07/18 - Adding doc name - End
                BinaryType = GetFileType(docModel.BinaryType),
                CreatedBy = docModel.CreatedBy,
                CreatedDate = docModel.CreatedDate,
                DeletedPages = docModel.DeletedPages,
                DocTypeId = docModel.DocTypeId,
                DocumentType = GetDocType(docModel.DocumentType),
                FieldValues = GetFieldValues(docModel.FieldValues),
                Id = docModel.Id,
                IsRejected = docModel.IsRejected,
                ModifiedBy = docModel.ModifiedBy,
                ModifiedDate = docModel.ModifiedDate,
                PageCount = docModel.PageCount,
                Pages = GetPages(docModel.Pages),
                EmbeddedPictures = GetPictures(docModel.EmbeddedPictures),
                AnnotationPermission = GetAnnotationPermission(docModel.AnnotationPermission),
                DocumentPermission = GetDocumentPermission(docModel.DocumentPermission)
            };
        }

        public static List<Document> GetDocuments(ObservableCollection<DocumentModel> documentModels)
        {
            return documentModels.Select(GetDocument).ToList();
        }

        public static DocumentModel GetDocumentModel(Document doc)
        {
            if (doc == null)
            {
                return null;
            }

            return new DocumentModel()
            {
                BatchId = doc.BatchId,
                // HungLe - 2014/07/18 - Adding doc name - Start
                DocName = doc.DocName,
                // HungLe - 2014/07/18 - Adding doc name - End
                BinaryType = GetFileTypeModel(doc.BinaryType),
                CreatedBy = doc.CreatedBy,
                CreatedDate = doc.CreatedDate,
                DeletedPages = doc.DeletedPages,
                DocTypeId = doc.DocTypeId,
                DocumentType = GetDocTypeModel(doc.DocumentType),
                FieldValues = GetFieldValueModels(doc.FieldValues),
                Id = doc.Id,
                IsRejected = doc.IsRejected,
                ModifiedBy = doc.ModifiedBy,
                ModifiedDate = doc.ModifiedDate,
                PageCount = doc.PageCount,
                Pages = GetPageModels(doc.Pages),
                EmbeddedPictures = GetPictureDictionay(doc.EmbeddedPictures),
                AnnotationPermission = GetAnnotationPermissionModel(doc.AnnotationPermission),
                DocumentPermission = GetDocumentPermissionModel(doc.DocumentPermission)
            };


        }

        public static ObservableCollection<DocumentModel> GetDocumentModels(List<Document> documents)
        {
            return new ObservableCollection<DocumentModel>(documents.Select(GetDocumentModel));
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

        public static BatchPermission GetBatchPermission(BatchPermissionModel permissionModel)
        {
            if (permissionModel == null)
            {
                return null;
            }

            return new BatchPermission()
            {
                CanAnnotate = permissionModel.CanAnnotate,
                CanDelete = permissionModel.CanDelete,
                CanDownloadFilesOnDemand = permissionModel.CanDownloadFilesOnDemand,
                CanEmail = permissionModel.CanEmail,
                CanModifyDocument = permissionModel.CanModifyDocument,
                CanModifyIndexes = permissionModel.CanModifyIndexes,
                CanPrint = permissionModel.CanPrint,
                CanReject = permissionModel.CanReject,
                CanReleaseLoosePage = permissionModel.CanReleaseLoosePage,
                CanSendLink = permissionModel.CanSendLink,
                CanViewOtherItems = permissionModel.CanViewOtherItems,
                HumanStepID = permissionModel.HumanStepID,
                WorkflowDefinitionID = permissionModel.WorkflowDefinitionID
            };
        }

        public static BatchPermissionModel GetBatchPermissionModel(BatchPermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new BatchPermissionModel()
            {
                CanAnnotate = permission.CanAnnotate,
                CanDelete = permission.CanDelete,
                CanDownloadFilesOnDemand = permission.CanDownloadFilesOnDemand,
                CanEmail = permission.CanEmail,
                CanModifyDocument = permission.CanModifyDocument,
                CanModifyIndexes = permission.CanModifyIndexes,
                CanPrint = permission.CanPrint,
                CanReject = permission.CanReject,
                CanReleaseLoosePage = permission.CanReleaseLoosePage,
                CanSendLink = permission.CanSendLink,
                CanViewOtherItems = permission.CanViewOtherItems,
                HumanStepID = permission.HumanStepID,
                WorkflowDefinitionID = permission.WorkflowDefinitionID
            };
        }

        public static Batch GetBatch(BatchModel batchModel)
        {
            if (batchModel == null)
            {
                return null;
            }

            return new Batch()
            {
                BatchName = batchModel.BatchName,
                BatchPermission = GetBatchPermission(batchModel.Permission),
                BatchType = GetBatchType(batchModel.BatchType),
                BatchTypeId = batchModel.BatchTypeId,
                BlockingActivityDescription = batchModel.BlockingActivityDescription,
                BlockingActivityName = batchModel.BlockingActivityName,
                BlockingBookmark = batchModel.BlockingBookmark,
                BlockingDate = batchModel.BlockingDate,
                CreatedBy = batchModel.CreatedBy,
                CreatedDate = batchModel.CreatedDate,
                DeletedDocuments = batchModel.DeletedDocuments,
                DeletedLooseDocuments = batchModel.DeletedPages,
                DocCount = batchModel.DocCount,
                Documents = GetDocuments(batchModel.Documents),
                FieldValues = GetBatchFieldValues(batchModel.FieldValues),
                HasError = batchModel.HasError,
                Id = batchModel.Id,
                IsCompleted = batchModel.IsCompleted,
                IsProcessing = batchModel.IsProcessing,
                IsRejected = batchModel.IsRejected,
                LastAccessedBy = batchModel.LastAccessedBy,
                LastAccessedDate = batchModel.LastAccessedDate,
                LockedBy = batchModel.LockedBy,
                ModifiedBy = batchModel.ModifiedBy,
                ModifiedDate = batchModel.ModifiedDate,
                PageCount = batchModel.PageCount,
                StatusMsg = batchModel.StatusMsg,
                WorkflowDefinitionId = batchModel.WorkflowDefinitionId,
                WorkflowInstanceId = batchModel.WorkflowInstanceId,
                Comments = GetComments(batchModel.Comments),
                DelegatedTo = batchModel.DelegatedTo,
                DelegatedBy = batchModel.DelegatedBy,
                TransactionId = batchModel.TransactionId
            };
        }

        public static BatchModel GetBatchModel(Batch batch)
        {
            if (batch == null)
            {
                return null;
            }

            return new BatchModel()
            {
                BatchName = batch.BatchName,
                Permission = GetBatchPermissionModel(batch.BatchPermission),
                BatchType = GetBatchTypeModel(batch.BatchType),
                BatchTypeId = batch.BatchTypeId,
                BlockingActivityDescription = batch.BlockingActivityDescription,
                BlockingActivityName = batch.BlockingActivityName,
                BlockingBookmark = batch.BlockingBookmark,
                BlockingDate = batch.BlockingDate,
                CreatedBy = batch.CreatedBy,
                CreatedDate = batch.CreatedDate,
                DeletedDocuments = batch.DeletedDocuments,
                DeletedPages = batch.DeletedLooseDocuments,
                DocCount = batch.DocCount,
                Documents = GetDocumentModels(batch.Documents),
                FieldValues = GetBatchFieldValueModels(batch.FieldValues),
                HasError = batch.HasError,
                Id = batch.Id,
                IsCompleted = batch.IsCompleted,
                IsProcessing = batch.IsProcessing,
                IsRejected = batch.IsRejected,
                LastAccessedBy = batch.LastAccessedBy,
                LastAccessedDate = batch.LastAccessedDate,
                LockedBy = batch.LockedBy,
                ModifiedBy = batch.ModifiedBy,
                ModifiedDate = batch.ModifiedDate,
                PageCount = batch.PageCount,
                StatusMsg = batch.StatusMsg,
                WorkflowDefinitionId = batch.WorkflowDefinitionId,
                WorkflowInstanceId = batch.WorkflowInstanceId,
                Comments = GetCommentModels(batch.Comments),
                DelegatedBy = batch.DelegatedBy,
                DelegatedTo = batch.DelegatedTo,
                TransactionId = batch.TransactionId
            };
        }

        public static List<BatchModel> GetBatchModels(List<Batch> batchs)
        {
            return batchs.Select(GetBatchModel).ToList();
        }

        public static List<Batch> GetBatchs(List<BatchModel> batchModels)
        {
            return batchModels.Select(GetBatch).ToList();
        }

        public static string GetFileType(FileTypeModel fileType)
        {
            switch (fileType)
            {
                case FileTypeModel.Image:
                    return "Image";
                case FileTypeModel.Native:
                    return "Native";
                case FileTypeModel.Media:
                    return "Media";
                case FileTypeModel.Compound:
                    return "Compound";
                default:
                    return null;
            }
        }

        public static FileTypeModel GetFileTypeModel(string fileType)
        {
            switch (fileType)
            {
                case "Image":
                    return FileTypeModel.Image;
                case "Native":
                    return FileTypeModel.Native;
                case "Media":
                    return FileTypeModel.Media;
                case "Compound":
                    return FileTypeModel.Compound;
                default:
                    return FileTypeModel.Image;
            }
        }

        public static SearchQuery GetSearchQuery(SearchQueryModel searchModel)
        {
            if (searchModel == null)
            {
                return null;
            }

            return new SearchQuery
            {
                BatchTypeId = searchModel.BatchTypeId,
                Id = searchModel.Id,
                Name = searchModel.Name,
                SearchQueryExpressions = GetSearchQueryExpressionModels(searchModel.SearchQueryExpressions.ToList()),
                UserId = searchModel.UserId,
                BatchType = GetBatchType(searchModel.BatchType),
                DeletedSearchQueryExpressions = searchModel.DeletedExpressions,
                SearchQueryString = searchModel.SearchQueryString
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
                FieldMetaData = GetBatchFieldMetaData(expressionModel.Field)
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
                BatchTypeId = searchQuery.BatchTypeId,
                UserId = searchQuery.UserId,
                Name = searchQuery.Name,
                Id = searchQuery.Id,
                SearchQueryExpressions = new ObservableCollection<SearchQueryExpressionModel>(GetSearchQueryExpressionModels(searchQuery.SearchQueryExpressions.ToList())),
                BatchType = GetBatchTypeModel(searchQuery.BatchType),
                SearchQueryString = searchQuery.SearchQueryString
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
                Field = GetBatchFieldMetaDataModel(expression.FieldMetaData)
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

        public static AmbiguousDefinitionModel GetAmbiguousDefinitionModel(AmbiguousDefinition ambiguousDefinition)
        {
            if (ambiguousDefinition == null)
            {
                return null;
            }

            return new AmbiguousDefinitionModel
            {
                Id = ambiguousDefinition.ID,
                Language = GetLanguageModel(ambiguousDefinition.Language),
                IsUnicode = ambiguousDefinition.Unicode,
                LanguageId = ambiguousDefinition.LanguageID,
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
                ID = ambiguousDefinition.Id,
                Language = GetLanguage(ambiguousDefinition.Language),
                Unicode = ambiguousDefinition.IsUnicode,
                LanguageID = ambiguousDefinition.LanguageId,
                Text = ambiguousDefinition.Text
            };
        }

        public static List<AmbiguousDefinition> GetAmbiguousDefinitions(List<AmbiguousDefinitionModel> ambiguousDefinitions)
        {
            return ambiguousDefinitions.Select(GetAmbiguousDefinition).ToList();
        }

        public static Comment GetComment(CommentModel commentModel)
        {
            if (commentModel == null)
            {
                return null;
            }

            return new Comment
            {
                CreatedBy = commentModel.CreatedBy,
                CreatedDate = commentModel.CreatedDate,
                Id = commentModel.Id,
                InstanceId = commentModel.InstanceId,
                IsBatchId = commentModel.IsBatchId,
                Note = commentModel.Note
            };
        }

        public static List<Comment> GetComments(ObservableCollection<CommentModel> commentModels)
        {
            return commentModels.Select(p => GetComment(p)).ToList();
        }

        public static CommentModel GetCommentModel(Comment comment)
        {
            if (comment == null)
            {
                return null;
            }

            return new CommentModel
            {
                CreatedBy = comment.CreatedBy,
                CreatedDate = comment.CreatedDate,
                Id = comment.Id,
                InstanceId = comment.InstanceId,
                IsBatchId = comment.IsBatchId,
                Note = comment.Note
            };
        }

        public static ObservableCollection<CommentModel> GetCommentModels(List<Comment> comments)
        {
            return new ObservableCollection<CommentModel>(comments.Select(p => GetCommentModel(p)));
        }

        public static DocumentFieldPermission GetFieldPermission(DocumentFieldPermissionModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new DocumentFieldPermission
            {
                CanRead = model.CanRead,
                CanWrite = model.CanWrite,
                DocTypeId = model.DocTypeId,
                FieldId = model.FieldId,
                Id = model.Id,
                UserGroupId = model.UserGroupId
            };
        }

        public static List<DocumentFieldPermission> GetFieldPermissions(ObservableCollection<DocumentFieldPermissionModel> models)
        {
            return models.Select(p => GetFieldPermission(p)).ToList();
        }

        public static DocumentFieldPermissionModel GetFieldPermissionModel(DocumentFieldPermission permission)
        {
            if (permission == null)
            {
                return null;
            }

            return new DocumentFieldPermissionModel
            {
                CanRead = permission.CanRead,
                CanWrite = permission.CanWrite,
                UserGroupId = permission.UserGroupId,
                Id = permission.Id,
                FieldId = permission.FieldId,
                DocTypeId = permission.DocTypeId,
            };
        }

        public static ObservableCollection<DocumentFieldPermissionModel> GetFieldPermissionModels(List<DocumentFieldPermission> permissions)
        {
            return new ObservableCollection<DocumentFieldPermissionModel>(permissions.Select(p => GetFieldPermissionModel(p)));
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
                ObjectId = actionLog.ObjectId,
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

        public static SettingModel GetSettingModel(Setting setting)
        {
            if (setting == null)
            {
                return null;
            }

            return new SettingModel
            {
                SearchResultPageSize = setting.SearchResultPageSize,
                EnabledBarcodeClient = setting.EnabledBarcodeClient,
                EnabledOCRClient = setting.EnabledOCRClient,
                ServerWorkingFolder = setting.ServerWorkingFolder,
                IsSaveFileInFolder = setting.IsSaveFileInFolder
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
                SearchResultPageSize = settingModel.SearchResultPageSize,
                EnabledBarcodeClient = settingModel.EnabledBarcodeClient,
                EnabledOCRClient = settingModel.EnabledOCRClient,
                ServerWorkingFolder = settingModel.ServerWorkingFolder,
                IsSaveFileInFolder = settingModel.IsSaveFileInFolder
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

        public static DocumentFieldMetaData GetTableField(TableColumnModel tableColumn)
        {
            return new DocumentFieldMetaData
            {
                DataTypeEnum = tableColumn.DataType,
                DefaultValue = tableColumn.DefaultValue,
                DisplayOrder = tableColumn.DisplayOrder,
                UniqueId = tableColumn.ColumnGuid.ToString(),
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

        public static List<DocumentFieldMetaData> GetTableFieldMetadatas(List<TableColumnModel> tableColumns)
        {
            if (tableColumns == null)
            {
                return null;
            }

            return tableColumns.Select(GetTableField).ToList();
        }

        public static TableColumnModel GetTableColumnModel(DocumentFieldMetaData field)
        {
            return new TableColumnModel
            {
                ColumnGuid = Guid.Parse(field.UniqueId),
                ColumnName = field.Name,
                FieldId = field.Id,
                Field = GetDocFieldMetaDataModel(field),
                DataType = field.DataTypeEnum,
                DefaultValue = field.DefaultValue,
                DisplayOrder = field.DisplayOrder,
                IsRequired = field.IsRequired,
                IsRestricted = field.IsRestricted,
                MaxLength = field.MaxLength,
                DocTypeId = field.DocTypeId,
                ParentFieldId = field.ParentFieldId,
                UseCurrentDate = field.UseCurrentDate
            };
        }

        public static List<TableColumnModel> GetTableColumnModels(List<DocumentFieldMetaData> fields)
        {
            if (fields == null)
            {
                return null;
            }

            return fields.Select(GetTableColumnModel).ToList();
        }

        public static AnnotationPermission GetAnnotationPermission(AnnotationPermissionModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new AnnotationPermission
            {
                CanAddHighlight = model.CanAddHighlight,
                CanAddRedaction = model.CanAddRedaction,
                CanAddText = model.CanAddText,
                CanDeleteHighlight = model.CanDeleteHighlight,
                CanDeleteRedaction = model.CanDeleteRedaction,
                CanDeleteText = model.CanDeleteText,
                CanHideRedaction = model.CanHideRedaction,
                CanSeeHighlight = model.CanSeeHighlight,
                CanSeeText = model.CanSeeText,
                DocTypeId = model.DocTypeId,
                UserGroupId = model.UserGroupId
            };
        }

        public static List<AnnotationPermission> GetAnnotationPermissions(List<AnnotationPermissionModel> models)
        {
            if (models == null)
            {
                return null;
            }

            return models.Select(GetAnnotationPermission).ToList();
        }

        public static AnnotationPermissionModel GetAnnotationPermissionModel(AnnotationPermission model)
        {
            if (model == null)
            {
                return null;
            }

            return new AnnotationPermissionModel
            {
                CanAddHighlight = model.CanAddHighlight,
                CanAddRedaction = model.CanAddRedaction,
                CanAddText = model.CanAddText,
                CanDeleteHighlight = model.CanDeleteHighlight,
                CanDeleteRedaction = model.CanDeleteRedaction,
                CanDeleteText = model.CanDeleteText,
                CanHideRedaction = model.CanHideRedaction,
                CanSeeHighlight = model.CanSeeHighlight,
                CanSeeText = model.CanSeeText,
                DocTypeId = model.DocTypeId,
                UserGroupId = model.UserGroupId
            };
        }

        public static List<AnnotationPermissionModel> GetAnnotationPermissionModels(List<AnnotationPermission> models)
        {
            if (models == null)
            {
                return null;
            }

            return models.Select(GetAnnotationPermissionModel).ToList();
        }
    }
}
