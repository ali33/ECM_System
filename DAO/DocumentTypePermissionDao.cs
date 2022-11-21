using System.Collections.Generic;
using System.Linq;
using Ecm.DAO.Context;
using Ecm.Domain;
using Dapper;
using System;

namespace Ecm.DAO
{
    public class DocumentTypePermissionDao
    {
        private readonly DapperContext _context;

        public DocumentTypePermissionDao(DapperContext context)
        {
            _context = context;
        }

        public void Add(DocumentTypePermission obj)
        {
            const string query = @"INSERT INTO [DocumentTypePermission]
                                            ([DocTypeID],[UserGroupID],[AllowedDeletePage],[AllowedAppendPage],[AllowedReplacePage],
                                             [AllowedSeeRetrictedField],[AllowedUpdateFieldValue],[AlowedPrintDocument],[AllowedEmailDocument],
                                             [AllowedRotatePage],[AllowedExportFieldValue],[AllowedDownloadOffline],[AllowedHideAllAnnotation],
                                             [AllowedCapture],[AllowedSearch])
                            OUTPUT inserted.ID
                                   VALUES (@DocTypeID,@UserGroupID,@AllowedDeletePage,@AllowedAppendPage,@AllowedReplacePage,
                                           @AllowedSeeRetrictedField,@AllowedUpdateFieldValue,@AlowedPrintDocument,@AllowedEmailDocument,
                                           @AllowedRotatePage,@AllowedExportFieldValue,@AllowedDownloadOffline,@AllowedHideAllAnnotation,
                                           @AllowedCapture,@AllowedSearch)
                                   ";
            obj.Id = _context.Connection.Query<Guid>(query,
                                                     new
                                                         {
                                                             DocTypeID = obj.DocTypeId,
                                                             UserGroupID = obj.UserGroupId,
                                                             AllowedDeletePage = obj.AllowedDeletePage,
                                                             AllowedAppendPage = obj.AllowedAppendPage,
                                                             AllowedReplacePage = obj.AllowedReplacePage,
                                                             AllowedSeeRetrictedField = obj.AllowedSeeRetrictedField,
                                                             AllowedUpdateFieldValue = obj.AllowedUpdateFieldValue,
                                                             AlowedPrintDocument = obj.AlowedPrintDocument,
                                                             AllowedEmailDocument = obj.AllowedEmailDocument,
                                                             AllowedRotatePage = obj.AllowedRotatePage,
                                                             AllowedExportFieldValue = obj.AllowedExportFieldValue,
                                                             AllowedDownloadOffline = obj.AllowedDownloadOffline,
                                                             AllowedHideAllAnnotation = obj.AllowedHideAllAnnotation,
                                                             AllowedCapture = obj.AllowedCapture,
                                                             AllowedSearch = obj.AllowedSearch
                                                         },
                                                     _context.CurrentTransaction).Single();
        }

        public void Delete(Guid id)
        {
            const string query = @"DELETE FROM [DocumentTypePermission] 
                                   WHERE ID = @ID";
            _context.Connection.Execute(query, new { ID = id }, _context.CurrentTransaction);
        }

        public void DeleteByUserGroup(Guid userGroupId)
        {
            const string query = @"DELETE FROM [DocumentTypePermission] 
                                   WHERE UserGroupID = @UserGroupID";
            _context.Connection.Execute(query, new { UserGroupID = userGroupId }, _context.CurrentTransaction);
        }

        public void DeleteByDocType(Guid docTypeId)
        {
            const string query = @"DELETE FROM [DocumentTypePermission] 
                                   WHERE DocTypeID = @DocTypeID";
            _context.Connection.Execute(query, new { DocTypeID = docTypeId }, _context.CurrentTransaction);
        }

        public void Update(DocumentTypePermission obj)
        {
            const string query = @"UPDATE [DocumentTypePermission]
                                   SET [DocTypeID] = @DocTypeID,
                                       [UserGroupID] = @UserGroupID,
                                       [AllowedDeletePage] = @AllowedDeletePage,
                                       [AllowedAppendPage] = @AllowedAppendPage,
                                       [AllowedReplacePage] = @AllowedReplacePage,
                                       [AllowedSeeRetrictedField] = @AllowedSeeRetrictedField,
                                       [AllowedUpdateFieldValue] = @AllowedUpdateFieldValue,
                                       [AlowedPrintDocument] = @AlowedPrintDocument,
                                       [AllowedEmailDocument] = @AllowedEmailDocument,
                                       [AllowedRotatePage] = @AllowedRotatePage,
                                       [AllowedExportFieldValue] = @AllowedExportFieldValue,
                                       [AllowedDownloadOffline] = @AllowedDownloadOffline,
                                       [AllowedHideAllAnnotation] = @AllowedHideAllAnnotation,
                                       [AllowedCapture] = @AllowedCapture,
                                       [AllowedSearch] = @AllowedSearch
                                   WHERE ID = @ID";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                DocTypeID = obj.DocTypeId,
                                                UserGroupID = obj.UserGroupId,
                                                AllowedDeletePage = obj.AllowedDeletePage,
                                                AllowedAppendPage = obj.AllowedAppendPage,
                                                AllowedReplacePage = obj.AllowedReplacePage,
                                                AllowedSeeRetrictedField = obj.AllowedSeeRetrictedField,
                                                AllowedUpdateFieldValue = obj.AllowedUpdateFieldValue,
                                                AlowedPrintDocument = obj.AlowedPrintDocument,
                                                AllowedEmailDocument = obj.AllowedEmailDocument,
                                                AllowedRotatePage = obj.AllowedRotatePage,
                                                AllowedExportFieldValue = obj.AllowedExportFieldValue,
                                                AllowedDownloadOffline = obj.AllowedDownloadOffline,
                                                AllowedHideAllAnnotation = obj.AllowedHideAllAnnotation,
                                                AllowedCapture = obj.AllowedCapture,
                                                AllowedSearch = obj.AllowedSearch,
                                                ID = obj.Id
                                            },
                                        _context.CurrentTransaction);
        }

        public DocumentTypePermission GetDocTypePermission(Guid docTypeId, Guid userGroupId)
        {
            const string query = @"SELECT * 
                                   FROM [DocumentTypePermission] 
                                   WHERE UserGroupID = @UserGroupID AND 
                                         DocTypeID = @DocTypeID";
            return _context.Connection.Query<DocumentTypePermission>(query, new { UserGroupID = userGroupId, DocTypeID = docTypeId }, _context.CurrentTransaction).FirstOrDefault();
        }

        public List<DocumentTypePermission> GetByUser(List<Guid> userGroupIds, Guid docTypeId)
        {
            const string query = @"SELECT dp.* 
                                   FROM DocumentTypePermission dp 
                                   WHERE dp.UserGroupID IN @UserGroupIDs AND 
                                         dp.DocTypeID = @DocTypeID";
            return _context.Connection.Query<DocumentTypePermission>(query, new { UserGroupIDs = userGroupIds, DocTypeID = docTypeId }, _context.CurrentTransaction).ToList();
        }
    }
}