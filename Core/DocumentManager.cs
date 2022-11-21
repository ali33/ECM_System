using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Ecm.DAO;
using Ecm.Domain;
using Ecm.LuceneService.Contract;
using Ecm.Utility;
using Ecm.Utility.ProxyHelper;
using Ecm.DAO.Context;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace Ecm.Core
{
    public class DocumentManager : ManagerBase
    {
        private const string AUTHORIZE_ID = "EE2271D5-F17C-4F9D-A85C-1383AAA218D7";
        private const string ARCHIVE_FOLDER = "ARCHIVE";
        private const string ARCHIVE_VERSION_FOLDER = "ARCHIVE_VERSION";
        private Setting _setting = new Setting();
        Thread executeLucence = null;

        public DocumentManager(User loginUser) : base(loginUser)
        {
            _setting = new SettingManager(loginUser).GetSettings();
        }

        public Guid InsertDocument(Document document)
        {
            InsertDocuments(new List<Document> { document });
            return document.Id;
        }

        public void InsertDocuments(List<Document> documents)
        {
            if (!LoginUser.IsAdmin)
            {
                // Validate the permission
                List<Guid> documentTypeIds = (from p in documents select p.DocTypeId).ToList();
                List<DocumentType> docTypes = new DocTypeManager(LoginUser).GetCapturedDocumentTypes();
                if (documentTypeIds.Any(p => !docTypes.Any(q => q.Id == p)))
                {
                    throw new SecurityException(string.Format("User {0} doesn't have permission to insert document.", LoginUser.UserName));
                }
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentDao documentDao = new DocumentDao(dataContext);
                PageDao pageDao = new PageDao(dataContext);
                AnnotationDao annotationDao = new AnnotationDao(dataContext);
                FieldValueDao fieldValueDao = new FieldValueDao(dataContext);
                TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(dataContext);
                OutlookPictureDao picDao = new OutlookPictureDao(dataContext);
                LinkDocumentDao linkDocDao = new LinkDocumentDao(dataContext);
                dataContext.BeginTransaction();
                try
                {
                    foreach (Document document in documents)
                    {
                        document.PageCount = document.Pages.Count;
                        document.Version = 1;
                        document.CreatedDate = DateTime.Now;
                        document.CreatedBy = LoginUser.UserName;
                        documentDao.Add(document);

                        if (document.EmbeddedPictures != null)
                        {
                            foreach (OutlookPicture pic in document.EmbeddedPictures)
                            {
                                pic.DocId = document.Id;                                
                                picDao.InsertPicture(pic);
                            }
                        }

                        foreach (Page page in document.Pages)
                        {
                            page.DocId = document.Id;
                            page.DocTypeId = document.DocTypeId;
                            page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                            page.CreatedBy = LoginUser.UserName;
                            page.CreatedDate = DateTime.Now;

                            if (string.IsNullOrEmpty(page.OriginalFileName))
                            {
                                page.OriginalFileName = Guid.NewGuid().ToString() + "." + page.FileExtension;
                            }
                            else
                            {
                                page.OriginalFileName = CheckOriginalFileExisted(page.OriginalFileName, document.Pages);
                            }

                            if (_setting.IsSaveFileInFolder)
                            {
                                string filename = Path.Combine(document.DocumentType.Id.ToString(), document.Id.ToString(), Guid.NewGuid().ToString());
                                string path = Path.Combine(_setting.LocationSaveFile, ARCHIVE_FOLDER, filename);
                                byte[] header = FileHelpper.CreateFile(path, page.FileBinary, page.FileExtension);

                                page.FilePath = path;
                                page.FileHeader = header;
                                page.FileBinary = null;
                            }
                            
                            pageDao.Add(page);
                            

                            ActionLogHelper.AddActionLog("Insert page", LoginUser, ActionName.InsertPage,
                                                         ObjectType.Page, page.Id, dataContext);

                            if (page.Annotations != null)
                            {
                                foreach (Annotation annotation in page.Annotations)
                                {
                                    annotation.DocTypeId = document.DocTypeId;
                                    annotation.DocId = document.Id;
                                    annotation.PageId = page.Id;
                                    annotationDao.Add(annotation);
                                    ActionLogHelper.AddActionLog("Add annotation with type " + annotation.Type,
                                                                 LoginUser, ActionName.AddAnnotation, ObjectType.Page,
                                                                 page.Id, dataContext);
                                }
                            }
                        }

                        var emptyFields = document.FieldValues.Where(p => string.IsNullOrEmpty(p.Value) || p.FieldMetaData.IsSystemField).ToList();
                        foreach (var emptyField in emptyFields)
                        {
                            if (emptyField.FieldMetaData.DataTypeEnum == FieldDataType.Table
                                && emptyField.TableFieldValue != null && emptyField.TableFieldValue.Count > 0)
                            {
                                continue;
                            }
                            document.FieldValues.Remove(emptyField);
                        }

                        foreach (FieldValue fieldValue in document.FieldValues)
                        {
                            fieldValue.DocId = document.Id;
                            fieldValue.FieldId = fieldValue.FieldMetaData.Id;
                            fieldValueDao.Add(fieldValue);

                            if (fieldValue.TableFieldValue != null && fieldValue.TableFieldValue.Count > 0)
                            {
                                foreach (var tableFieldValue in fieldValue.TableFieldValue)
                                {
                                    tableFieldValue.DocId = document.Id;

                                    if (tableFieldValue.Field.DataTypeEnum == FieldDataType.Date)
                                    {
                                        int index = tableFieldValue.Value.IndexOf(" ");
                                        if(index != -1)
                                        {
                                            tableFieldValue.Value = tableFieldValue.Value.Substring(0, index);
                                        }
                                    }

                                    tableFieldValueDao.Add(tableFieldValue);
                                }
                            }

                            ActionLogHelper.AddActionLog("Add field vale = " + fieldValue.Value + " to field name " + fieldValue.FieldMetaData.Name, 
                                LoginUser, ActionName.AddFieldValue, null, null, dataContext);
                        }

                        if (document.LinkDocuments != null)
                        {
                            foreach(var linkDoc in document.LinkDocuments)
                            {
                                linkDocDao.Add(linkDoc);
                            }
                        }
                    } // end foreach documents

                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }

                // Create lucene index for documents
                executeLucence = new Thread(StartLuceneIndex);
                executeLucence.Start(documents);

                while (!executeLucence.IsAlive) ;
            }
        }

        public List<Document> UpdateDocuments(List<Document> documents)
        {
            var returnDocumentInfo = new List<Document>();
            List<Guid> documentTypeIds = (from p in documents select p.DocTypeId).Distinct().ToList();
            var docTypes = new List<DocumentType>();
            if (!LoginUser.IsAdmin)
            {
                docTypes = new DocTypeManager(LoginUser).GetDocumentTypes();
            }

            foreach (var documentTypeId in documentTypeIds)
            {
                DocumentTypePermission docTypePermission;
                bool allowedUpdateAnnotation;

                // Validate the permission
                if (!LoginUser.IsAdmin)
                {
                    var documentType = docTypes.FirstOrDefault(p => p.Id == documentTypeId);
                    if (documentType == null)
                    {
                        throw new SecurityException(
                            string.Format("User {0} doesn't have permission to update document.", LoginUser.UserName));
                    }

                    docTypePermission = documentType.DocumentTypePermission;
                    allowedUpdateAnnotation = documentType.AnnotationPermission.AllowedAddHighlight ||
                                              documentType.AnnotationPermission.AllowedAddRedaction ||
                                              documentType.AnnotationPermission.AllowedAddText ||
                                              documentType.AnnotationPermission.AllowedDeleteHighlight ||
                                              documentType.AnnotationPermission.AllowedDeleteRedaction ||
                                              documentType.AnnotationPermission.AllowedDeleteText;
                }
                else
                {
                    docTypePermission = DocumentTypePermission.GetAllowAll();
                    allowedUpdateAnnotation = true;
                }

                var documentsOfDocType = documents.Where(p => p.DocumentType.Id == documentTypeId).ToList();
                using (DapperContext dataContext = new DapperContext(LoginUser))
                {
                    DocumentDao _documentDao = new DocumentDao(dataContext);
                    PageDao _pageDao = new PageDao(dataContext);
                    AnnotationDao _annotationDao = new AnnotationDao(dataContext);
                    FieldValueDao _fieldValueDao = new FieldValueDao(dataContext);
                    LinkDocumentDao _linkDocDao = new LinkDocumentDao(dataContext);
                    dataContext.BeginTransaction();

                    try
                    {
                        foreach (var document in documentsOfDocType)
                        {
                            UpdateDocument(docTypePermission.AllowedAppendPage, returnDocumentInfo,
                                           docTypePermission.AllowedReplacePage, allowedUpdateAnnotation,
                                           docTypePermission.AllowedRotatePage, docTypePermission.AllowedDeletePage,
                                           document, docTypePermission.AllowedUpdateFieldValue,
                                           dataContext, _documentDao, _annotationDao, _pageDao, _fieldValueDao,_linkDocDao);
                        }
                        dataContext.Commit();
                    }
                    catch (Exception)
                    {
                        dataContext.Rollback();
                        throw;
                    }

                    executeLucence = new Thread(UpdateLuceneIndex);
                    executeLucence.Start(documentsOfDocType);

                    while (!executeLucence.IsAlive) ;

                }
            }
            return returnDocumentInfo;
        }

        public List<Document> GetDocuments(List<Guid> ids)
        {
            List<Document> documents = new List<Document>();
            foreach (Guid id in ids)
            {
                documents.Add(GetDocument(id));
            }

            return documents;
        }

        public Document GetDocument(Guid id)
        {
            return GetDocumentById(id);
        }

        private Document GetDocumentById(Guid id)
        {
            Document document = null;
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentDao docDao = new DocumentDao(dataContext);
                document = docDao.GetById(id);

                if (document != null)
                {
                    List<DocumentType> docTypes = new DocTypeManager(LoginUser).GetDocumentTypes();
                    DocumentType docType = docTypes.FirstOrDefault(p => p.Id == document.DocTypeId);

                    if (docType == null)
                    {
                        ActionLogHelper.AddActionLog("User do not has permission to get document", LoginUser,
                                                     ActionName.GetDocumentType, ObjectType.Document, id, dataContext);
                        throw new SecurityException(string.Format("User {0} doesn't have permission to open document.",
                                                                  LoginUser.UserName));
                    }

                    document.DocumentType = docType;
                    if (!LoginUser.IsAdmin)
                    {
                        document.FieldValues.RemoveAll(
                            p => !docType.FieldMetaDatas.Any(q => q.Id == p.FieldMetaData.Id));
                            //Guid p => !docType.FieldMetaDatas.Any(q => q.FieldUniqueId == p.FieldMetaData.FieldUniqueId));
                    }

                    document.EmbeddedPictures = new OutlookPictureDao(dataContext).GetPictures(document.Id);

                    document.Pages = new PageDao(dataContext).GetByDoc(document.Id).OrderBy(p => p.PageNumber).ToList();
                    AnnotationDao _annotationDao = new AnnotationDao(dataContext);
                    foreach (var page in document.Pages)
                    {
                        if (_setting.IsSaveFileInFolder)
                        {
                            if (File.Exists(page.FilePath))
                            {
                                page.FileBinary = FileHelpper.ReadFile(page.FilePath, page.FileHeader);
                            }
                        }
                        page.Annotations = _annotationDao.GetByPage(page.Id);
                    }

                    FieldValueDao fieldValueDao = new FieldValueDao(dataContext);
                    document.FieldValues = fieldValueDao.GetByDoc(document.Id);

                    FieldMetaDataDao fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                    TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(dataContext);
                    PicklistDao _picklistDao = new PicklistDao(dataContext);
                    OCRTemplateZoneDao _ocrTemplateZoneDao = new OCRTemplateZoneDao(dataContext);

                    foreach (var fieldValue in document.FieldValues)
                    {
                        fieldValue.FieldMetaData = fieldMetaDataDao.GetById(fieldValue.FieldId);
                        if (fieldValue.FieldMetaData.DataType == "Table")
                        {
                            fieldValue.FieldMetaData.Children = fieldMetaDataDao.GetChildren(fieldValue.FieldId);
                            fieldValue.TableFieldValue = tableFieldValueDao.GetByParentField(fieldValue.FieldId, document.Id);

                            foreach (var tableField in fieldValue.TableFieldValue)
                            {
                                tableField.Field = fieldMetaDataDao.GetChild(tableField.FieldId);
                            }
                        }

                        if (fieldValue.FieldMetaData.DataTypeEnum == FieldDataType.Picklist)
                        {
                            fieldValue.FieldMetaData.Picklists = _picklistDao.GetByField(fieldValue.FieldMetaData.Id);
                        }

                        fieldValue.FieldMetaData.OCRTemplateZone = _ocrTemplateZoneDao.GetByField(fieldValue.FieldId);

                        if (fieldValue.FieldMetaData.IsLookup)
                        {
                            //fieldValue.FieldMetaData.LookupInfo = new LookupInfoDao(dataContext).GetById(fieldValue.FieldId);
                            //fieldValue.FieldMetaData.LookupMaps = new LookupMapDao(dataContext).GetByField(fieldValue.FieldId);
                        }
                    }

                    LinkDocumentDao _linkDocDao = new LinkDocumentDao(dataContext);
                    document.LinkDocuments = _linkDocDao.GetByDocumentId(document.Id);

                    if(document.LinkDocuments != null && document.LinkDocuments.Count > 0)
                    {
                        foreach(var linkDoc in document.LinkDocuments)
                        {
                            linkDoc.LinkedDocument = docDao.GetById(linkDoc.LinkDocumentId);
                            linkDoc.LinkedDocument.FieldValues = fieldValueDao.GetByDoc(linkDoc.LinkDocumentId);

                            foreach(var fieldValue in linkDoc.LinkedDocument.FieldValues)
                            {
                                fieldValue.FieldMetaData = fieldMetaDataDao.GetById(fieldValue.FieldId);
                            }

                            //linkDoc.RootDocument = document;
                        }
                    }

                    ActionLogHelper.AddActionLog("Get document id = " + id + " successfully", LoginUser,
                                                 ActionName.ViewDocument, ObjectType.Document, id, dataContext);
                }
            }

            return document;
        }

        public SearchResult RunAdvanceSearch(Guid docTypeId, SearchQuery query, int pageIndex, int pageSize, string sortColumn, string sortDir)
        {
            DocumentType docType = new DocTypeManager(LoginUser).GetDocumentType(docTypeId);
            if (docType == null)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to search documents.",
                                                          LoginUser.UserName));
            }

            Setting setting = new SettingManager(LoginUser).GetSettings();
            using (var LuceneClient = GetLucenceClientChannel())
            {
                return LuceneClient.Channel.RunAdvanceSearch(AUTHORIZE_ID, docType, query, pageIndex, pageSize, sortColumn, sortDir);
            }
        }

        public List<SearchResult> RunGlobalSearch(string keyword, int pageIndex, int pageSize)
        {
            List<DocumentType> docTypes = new DocTypeManager(LoginUser).GetDocumentTypes();
            if (docTypes.Count == 0)
            {
                return new List<SearchResult>();
            }

            Setting setting = new SettingManager(LoginUser).GetSettings();
            using (var LuceneClient = GetLucenceClientChannel())
            {
                return LuceneClient.Channel.RunGlobalSearch(AUTHORIZE_ID, keyword, docTypes, pageIndex, pageSize);
            }
        }

        public SearchResult RunSearchContent(Guid docTypeId, string text, int pageIndex, int pageSize)
        {
            DocumentType docType = new DocTypeManager(LoginUser).GetDocumentType(docTypeId);
            if (docType == null)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to search documents.",
                                                          LoginUser.UserName));
            }

            Setting setting = new SettingManager(LoginUser).GetSettings();
            using (var LuceneClient = GetLucenceClientChannel())
            {
                return LuceneClient.Channel.RunSearchContent(AUTHORIZE_ID, docType, text, pageIndex, pageSize);
            }
        }

        public void DeleteDocument(Guid docId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentDao _documentDao = new DocumentDao(dataContext);

                Document document = _documentDao.GetById(docId);
                if (!LoginUser.IsAdmin)
                {
                    DocumentType docType = new DocTypeManager(LoginUser).GetDocumentType(document.DocTypeId);
                    if (docType == null || !docType.DocumentTypePermission.AllowedDeletePage)
                    {
                        throw new SecurityException(string.Format("User {0} doesn't have permission to delete document.", LoginUser.UserName));
                    }
                    document.DocumentType = docType;
                }
                else
                {
                    document.DocumentType = new DocTypeDao(dataContext).GetById(document.DocTypeId);
                }
                dataContext.BeginTransaction();

                try
                {
                    PageDao _pageDao = new PageDao(dataContext);
                    AnnotationDao _annotationDao = new AnnotationDao(dataContext);
                    FieldValueDao _fieldValueDao = new FieldValueDao(dataContext);
                    FieldMetaDataDao _fieldMetadataDao = new FieldMetaDataDao(dataContext);
                    OutlookPictureDao picDao = new OutlookPictureDao(dataContext);
                    LinkDocumentDao _linkDocDao = new LinkDocumentDao(dataContext);

                    AddDocumentVersion(document, ChangeAction.DeleteDocument, dataContext, _pageDao,
                        _fieldValueDao, _annotationDao, _fieldMetadataDao, new DocumentVersionDao(dataContext),
                        new PageVersionDao(dataContext), new AnnotationVersionDao(dataContext),
                        new DocumentFieldVersionDao(dataContext));
                    _annotationDao.DeleteByDoc(docId);

                    //Delete File
                    List<Page> pages = _pageDao.GetByDoc(docId);
                    foreach(Page page in pages)
                    {
                        if (_setting.IsSaveFileInFolder)
                        {
                            FileHelpper.DeleteFile(page.FilePath);
                        }
                    }
                    //Delete Page
                    _pageDao.DeleteByDoc(docId);

                    _fieldValueDao.DeleteByDoc(docId);
                    picDao.DeletePicture(docId);
                    _linkDocDao.DeleteByDocument(docId);
                    _documentDao.Delete(docId);

                    if (_setting.IsSaveFileInFolder)
                    {
                        string docInfo = FileHelpper.GetPath_WithoutFileName(pages[0].FilePath);
                        FileHelpper.DeleteFolder(docInfo);
                    }

                    ActionLogHelper.AddActionLog(LoginUser.UserName + " delete document with id = " + docId, LoginUser, ActionName.DeleteDocument, ObjectType.Document, docId, dataContext);
                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }

                executeLucence = new Thread(DeleteLuceneIndex);
                executeLucence.Start(document);

                while (!executeLucence.IsAlive) ;

                ActionLogHelper.AddActionLog("Delete document successfully. Id = " + document.Id, LoginUser, ActionName.CreateIndex, null, null, dataContext);
            }
        }

        public List<Document> GetDocuments(Guid docTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentDao docDao = new DocumentDao(dataContext);
                PageDao _pageDao = new PageDao(dataContext);
                AnnotationDao _annotationDao = new AnnotationDao(dataContext);
                FieldValueDao _fieldValueDao = new FieldValueDao(dataContext);
                FieldMetaDataDao _fieldMetaDataDao = new FieldMetaDataDao(dataContext);
                OutlookPictureDao picDao = new OutlookPictureDao(dataContext);
                LinkDocumentDao _linkDocDao = new LinkDocumentDao(dataContext);

                var documents = docDao.GetByDocType(docTypeId);

                foreach (var document in documents)
                {
                    document.Pages = _pageDao.GetByDoc(document.Id);

                    document.EmbeddedPictures = picDao.GetPictures(document.Id);

                    foreach (var page in document.Pages)
                    {
                        if (_setting.IsSaveFileInFolder)
                        {
                            page.FileBinary = FileHelpper.ReadFile(page.FilePath,page.FileHeader);
                        }

                        page.Annotations = _annotationDao.GetByPage(page.Id);
                    }

                    document.FieldValues = _fieldValueDao.GetByDoc(document.Id);

                    foreach (var fieldValue in document.FieldValues)
                    {
                        fieldValue.FieldMetaData = _fieldMetaDataDao.GetById(fieldValue.FieldId);
                    }

                    document.LinkDocuments = _linkDocDao.GetByDocumentId(document.Id);

                    if (document.LinkDocuments != null && document.LinkDocuments.Count > 0)
                    {
                        foreach (var linkDoc in document.LinkDocuments)
                        {
                            linkDoc.LinkedDocument = docDao.GetById(linkDoc.LinkDocumentId);
                            linkDoc.RootDocument = docDao.GetById(linkDoc.DocumentId);
                        }
                    }

                }

                return documents;
            }
        }

        private void UpdateDocument(bool allowedAppendPage, List<Document> returnDocumentInfo, bool allowedReplacePage,
                                    bool allowedUpdateAnnotation, bool allowedRotatePage, bool allowedDeletePage,
                                    Document document, bool allowedUpdateFieldValue,
                                    DapperContext dataContext, DocumentDao documentDao,
                                    AnnotationDao annotationDao, PageDao pageDao, FieldValueDao fieldValueDao, LinkDocumentDao linkDocDao)
        {

            //Document oldDocument = _documentDao.GetById(document.Id);
            AddDocumentVersion(document, ChangeAction.UpdateDocument, dataContext, pageDao, fieldValueDao,
                annotationDao, new FieldMetaDataDao(dataContext), new DocumentVersionDao(dataContext),
                        new PageVersionDao(dataContext), new AnnotationVersionDao(dataContext),
                        new DocumentFieldVersionDao(dataContext)); 
                
            document.Version++;
            document.ModifiedBy = LoginUser.UserName;
            document.ModifiedDate = DateTime.Now;
            document.PageCount = document.Pages.Count;
            documentDao.Update(document);

            ActionLogHelper.AddActionLog("Update document id " + document.Id + " Successfuly!",
                LoginUser, ActionName.UpdateDocument, ObjectType.Document, document.Id, dataContext);

            if (allowedDeletePage && document.DeletedPages != null && document.DeletedPages.Count > 0)
            {
                foreach (var pageId in document.DeletedPages)
                {
                    Page deletePage = pageDao.GetById(pageId);
                    annotationDao.DeleteByPage(pageId);

                    if (_setting.IsSaveFileInFolder)
                    {
                        if (File.Exists(deletePage.FilePath))
                        {
                            FileHelpper.DeleteFile(deletePage.FilePath);
                        }
                    }

                    pageDao.Delete(pageId);

                    ActionLogHelper.AddActionLog("Delete page from document id" + document.Id, LoginUser,
                                                 ActionName.DeletePage, ObjectType.Page, pageId, dataContext);
                }
            }

            if (allowedUpdateFieldValue)
            {
                TableFieldValueDao tableFieldValueDao = new TableFieldValueDao(dataContext);
                var hasValueFields = document.FieldValues.Where(p => !string.IsNullOrEmpty(p.Value));
                fieldValueDao.DeleteByDoc(document.Id);
                ActionLogHelper.AddActionLog("Delete field value from document id" + document.Id, LoginUser,
                                             ActionName.DeleteFieldValue, ObjectType.Document, document.Id, dataContext);
                foreach (var fieldValue in hasValueFields)
                {
                    fieldValue.DocId = document.Id;

                    if (fieldValue.FieldMetaData.DataTypeEnum == FieldDataType.Date)
                    {
                        int index = fieldValue.Value.IndexOf(" ");
                        if (index != -1)
                        {
                            fieldValue.Value = fieldValue.Value.Substring(0, index);
                        }
                    }

                    fieldValueDao.Add(fieldValue);

                    if (fieldValue.TableFieldValue != null && fieldValue.TableFieldValue.Count > 0)
                    {
                        //tableFieldValueDao.DeleteByDocumentAndField(document.Id, fieldValue.FieldId);
                        tableFieldValueDao.DeleteByDocument(document.Id);

                        foreach (var tableFieldValue in fieldValue.TableFieldValue)
                        {
                            tableFieldValue.DocId = document.Id;
                            tableFieldValueDao.Add(tableFieldValue);
                        }
                    }
                    ActionLogHelper.AddActionLog("Add field value to document id" + document.Id, LoginUser,
                                                 ActionName.AddFieldValue, ObjectType.Document, document.Id, dataContext);
                }
            }      
      
            int order = 0;
            foreach (var page in document.Pages)
            {
                if (page.Id == Guid.Empty)
                {
                    page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                    page.DocId = document.Id;
                    page.DocTypeId = document.DocTypeId;
                    page.CreatedDate = DateTime.Now;
                    page.CreatedBy = LoginUser.UserName;
                    page.PageNumber = order;
                    //TODO:
                    //Extract data from page    
                    if (_setting.IsSaveFileInFolder)
                    {
                        string filename = Path.Combine(document.DocumentType.Id.ToString(), document.Id.ToString(), Guid.NewGuid().ToString());
                        string path = Path.Combine(_setting.LocationSaveFile, filename);

                        byte[] header = FileHelpper.CreateFile(path, page.FileBinary, page.FileExtension);

                        page.FilePath = path;
                        page.FileHeader = header;
                        page.FileBinary = null;
                    }

                    pageDao.Add(page);
                    
                    ActionLogHelper.AddActionLog("Add page to document id" + document.Id, LoginUser, ActionName.InsertPage,
                                                 ObjectType.Page, page.Id, dataContext);
                }
                else
                {
                    page.PageNumber = order;
                    if ((allowedAppendPage || allowedReplacePage) && page.FileBinary != null)
                    {
                        bool isChangeFile = false;
                        var hash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                        if (page.FileHash != hash)
                        {
                            isChangeFile = true;
                            page.FileHash = CryptographyHelper.GenerateFileHash(page.FileBinary);
                        }
                        //TODO:
                        //Extract data from page
                        page.ModifiedBy = LoginUser.UserName;
                        page.ModifiedDate = DateTime.Now;

                        if (_setting.IsSaveFileInFolder)
                        {
                            if (isChangeFile)
                            {
                                FileHelpper.DeleteFile(page.FilePath);

                                string filename = Path.Combine(document.DocumentType.Id.ToString(), document.Id.ToString(), Guid.NewGuid().ToString());
                                string path = Path.Combine(_setting.LocationSaveFile, ARCHIVE_FOLDER, filename);
                                byte[] header = FileHelpper.CreateFile(path, page.FileBinary, page.FileExtension);

                                page.FilePath = filename;
                                page.FileHeader = header;
                                page.FileBinary = null;
                            }
                        }

                        pageDao.UpdateBinary(page);

                        ActionLogHelper.AddActionLog("Replace page binary from document id" + document.Id, LoginUser, ActionName.ReplacePage,
                         ObjectType.Page, page.Id, dataContext);
                    }

                    if (allowedRotatePage)
                    {
                        page.Height = page.Height;
                        page.Width = page.Width;
                        page.RotateAngle = page.RotateAngle;
                    }

                    page.ModifiedBy = LoginUser.UserName;
                    page.ModifiedDate = DateTime.Now;
                    pageDao.Update(page);

                    ActionLogHelper.AddActionLog("Update page to document id" + document.Id, LoginUser,
                                                 ActionName.UpdatePage, ObjectType.Page, page.Id, dataContext);

                    if (allowedUpdateAnnotation)
                    {
                        annotationDao.DeleteByPage(page.Id);
                        ActionLogHelper.AddActionLog("Delete all annotations from page id = " + page.Id,
                                                     LoginUser, ActionName.DeleteAnnotation, ObjectType.Page,
                                                     page.Id, dataContext);
                        foreach (var annotation in page.Annotations)
                        {
                            annotation.DocId = document.Id;
                            annotation.DocTypeId = document.DocTypeId;
                            annotation.PageId = page.Id;
                            annotationDao.Add(annotation);

                            ActionLogHelper.AddActionLog("Add annotation with type " + annotation.Type,
                                                         LoginUser, ActionName.AddAnnotation, ObjectType.Page,
                                                         page.Id, dataContext);
                        }
                    }
                }

                order++;
            }


            if (document.DeletedLinkDocuments != null)
            {
                foreach(var linkDoc in document.DeletedLinkDocuments)
                {
                    if(linkDoc != Guid.Empty)
                    {
                        linkDocDao.Delete(linkDoc);

                        ActionLogHelper.AddActionLog("Delete link document Id" + linkDoc + " of Document Id" + document.Id,
                                                     LoginUser, ActionName.DeleteLinkDoc, ObjectType.LinkDocument,
                                                     linkDoc, dataContext);
                    }
                }
            }

            foreach (var linkDoc in document.LinkDocuments)
            {
                if (linkDoc.Id == Guid.Empty)
                {
                    linkDocDao.Add(linkDoc);
                    ActionLogHelper.AddActionLog("Add link document Id" + linkDoc.Id + " of Document Id" + linkDoc.DocumentId + " and Link document id" + linkDoc.LinkDocumentId,
                                                 LoginUser, ActionName.AddLinkDoc, ObjectType.LinkDocument,
                                                 linkDoc.Id, dataContext);
                }
                else
                {
                    linkDocDao.Update(linkDoc);
                    ActionLogHelper.AddActionLog("Update link document Id" + linkDoc.Id + " with Notes value to " + linkDoc.Notes ,
                                                 LoginUser, ActionName.UpdateLinkDoc, ObjectType.LinkDocument,
                                                 linkDoc.Id, dataContext);
                }
            }


            returnDocumentInfo.Add(new Document
            {
                Id = document.Id,
                BinaryType = document.BinaryType,
                ModifiedBy = document.ModifiedBy,
                ModifiedDate = document.ModifiedDate,
                Version = document.Version
            });
        }

        private void AddDocumentVersion(Document document, ChangeAction changeAction,
                        DapperContext dataContext, PageDao pageDao, FieldValueDao fieldValueDao,
                        AnnotationDao annotationDao, FieldMetaDataDao _fieldMetadataDao,
                        DocumentVersionDao documentVersionDao, PageVersionDao pageVersionDao,
                        AnnotationVersionDao annotationVersionDao, DocumentFieldVersionDao fieldValueVersionDao)
        {
            var oldDocument = GetDocumentById(document.Id);
            oldDocument.Pages = pageDao.GetByDoc(oldDocument.Id);
            oldDocument.FieldValues = fieldValueDao.GetByDoc(oldDocument.Id);

            DocumentVersion documentVersion = BaseVersion.GetDocumentVersionFromDocument(oldDocument, changeAction);
            documentVersion.ModifiedBy = LoginUser.UserName;
            documentVersion.ModifiedDate = DateTime.Now;

            documentVersionDao.Add(documentVersion);
            ActionLogHelper.AddActionLog("Add document version for document id " + document.Id + " successfully!", LoginUser, ActionName.AddDocumentVersion, ObjectType.Document, documentVersion.Id, dataContext);
            
            foreach (var page in oldDocument.Pages)
            {
                page.Annotations = annotationDao.GetByPage(page.Id);
                PageVersion pageVersion = BaseVersion.GetPageVersionFromPage(page);
                pageVersion.DocVersionId = documentVersion.Id;

                //Extract data from page    
                if (_setting.IsSaveFileInFolder)
                {
                    string pathFrom = page.FilePath;
                    string path = Path.Combine(document.DocumentType.Id.ToString(), document.Id.ToString() + "_" + documentVersion.Version.ToString(), Guid.NewGuid().ToString());
                    string pathTo = Path.Combine(_setting.LocationSaveFile, ARCHIVE_VERSION_FOLDER, path);

                    FileHelpper.Copy(pathFrom, pathTo);

                    pageVersion.FilePath = pathTo;
                }

                pageVersionDao.Add(pageVersion);

                ActionLogHelper.AddActionLog("Add page version for page id " + page.Id + " successfully!", LoginUser, ActionName.AddPageVersion, ObjectType.Page, pageVersion.Id, dataContext);
                
                foreach (var annotation in page.Annotations)
                {
                    AnnotationVersion annotationVersion = BaseVersion.GetAnnotationVersionFromAnnotation(annotation);
                    annotationVersion.PageVersionId = pageVersion.Id;
                    annotationVersionDao.Add(annotationVersion);
                    ActionLogHelper.AddActionLog("Add annotation version for page id " + page.Id + " successfully!", LoginUser, ActionName.AddAnnotationVersion, ObjectType.Page, pageVersion.Id, dataContext);
                }
            }

            foreach (var fieldValue in oldDocument.FieldValues)
            {
                fieldValue.FieldMetaData = _fieldMetadataDao.GetById(fieldValue.FieldId);
                DocumentFieldVersion fieldValueVersion = BaseVersion.GetDocumentFieldVersionFromFieldValue(fieldValue);
                fieldValueVersion.DocVersionID = documentVersion.Id;
                fieldValueVersionDao.Add(fieldValueVersion);
                ActionLogHelper.AddActionLog("Add field value version for document id " + document.Id + " successfully!", LoginUser, ActionName.AddFieldVersionValue, ObjectType.Document, documentVersion.Id, dataContext);
            }
        }


        private void StartLuceneIndex(object data)
        {
            //Thread.Sleep(5000);

            var documents = data as List<Document>;
            var workingFolder = GetServerWorkingFolder();
            using (var LuceneClient = GetLucenceClientChannel())
            {
                foreach (Document document in documents)
                {
                    using (var context = new DapperContext(LoginUser.ArchiveConnectionString))
                    {
                        //ActionLogHelper.AddActionLog("Begin extract text from page", LoginUser,
                        //ActionName.ExtractText, null, null, context);

                        //foreach (Page page in document.Pages)
                        //{
                        //    if (string.IsNullOrEmpty(page.Content))
                        //    {
                        //        Extractor extractor = new Extractor();
                                
                        //        if (_setting.IsSaveFileInFolder)
                        //        {
                        //            page.FileBinary = FileHelpper.ReadFile(page.FilePath, page.FileHeader);
                        //        }

                        //        page.Content = extractor.ExtractToText(page.FileBinary, page.FileExtension, page.ContentLanguageCode, workingFolder);
                        //        PageDao pageDao = new PageDao(context);
                        //        pageDao.UpdatePageContent(page);
                        //    }
                        //}

                        //ActionLogHelper.AddActionLog("End extract text from page", LoginUser,
                        //ActionName.ExtractText, null, null, context);

                        ActionLogHelper.AddActionLog("Begin create index", LoginUser,
                        ActionName.CreateIndex, null, null, context);

                        LuceneClient.Channel.CreateIndex(AUTHORIZE_ID, document);

                        ActionLogHelper.AddActionLog("Create index for document successfully. Id = " + document.Id, LoginUser,
                        ActionName.CreateIndex, null, null, context);
                    }

                }
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(workingFolder);

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }

                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    if (dir.Name == dir.Root.Name)
                    {
                        continue;
                    }

                    dir.Delete(true);
                }
            }
            catch
            {
            }
            // Request that oThread be stopped
            executeLucence.Abort();

            // Wait until oThread finishes. Join also has overloads
            // that take a millisecond interval or a TimeSpan object.
            executeLucence.Join();
        }

        private void UpdateLuceneIndex(object data)
        {
            //Thread.Sleep(5000);

            var documents = data as List<Document>;

            using (var LuceneClient = GetLucenceClientChannel())
            {
                foreach (var document in documents)
                {
                    //foreach (Page page in document.Pages)
                    //{
                    //    if (string.IsNullOrEmpty(page.Content))
                    //    {
                    //        Extractor extractor = new Extractor();

                    //        if (_setting.IsSaveFileInFolder)
                    //        {
                    //            page.FileBinary = FileHelpper.ReadFile(page.FilePath, page.FileHeader);
                    //        }

                    //        page.Content = extractor.ExtractToText(page.FileBinary, page.FileExtension, page.ContentLanguageCode, GetServerWorkingFolder());
                    //    }
                    //}
                    LuceneClient.Channel.UpdateIndex(AUTHORIZE_ID, document);
                    using (var context = new DapperContext(LoginUser.ArchiveConnectionString))
                    {
                        ActionLogHelper.AddActionLog("Update index for document successfully. Id = " + document.Id, LoginUser, ActionName.UpdateIndex, null, null, context);
                    }
                }
            }

            // Request that oThread be stopped
            executeLucence.Abort();

            // Wait until oThread finishes. Join also has overloads
            // that take a millisecond interval or a TimeSpan object.
            executeLucence.Join();
        }

        private void DeleteLuceneIndex(object data)
        {
            Thread.Sleep(5000);

            var document = data as Document;

            using (var LuceneClient = GetLucenceClientChannel())
            {
                LuceneClient.Channel.DeleteIndex(AUTHORIZE_ID, document);
                using (var context = new DapperContext(LoginUser.ArchiveConnectionString))
                {
                    ActionLogHelper.AddActionLog("Delete index for document successfully. Id = " + document.Id, LoginUser, ActionName.DeleteIndex, null, null, context);
                }
            }
            // Request that oThread be stopped
            executeLucence.Abort();

            // Wait until oThread finishes. Join also has overloads
            // that take a millisecond interval or a TimeSpan object.
            executeLucence.Join();
        }

        int count = 0;

        private string CheckOriginalFileExisted(string fileName, List<Page> pages)
        {
            string newFileName = fileName;

            if (pages.Exists(p => p.OriginalFileName == fileName))
            {
                if (count != 0)
                {
                    string extension = new FileInfo(newFileName).Extension;
                    newFileName = newFileName.Remove(newFileName.IndexOf("."));
                    newFileName += "-copy(" + count + ")";
                    newFileName += extension;
                }
                count++;
                CheckOriginalFileExisted(newFileName, pages);
            }

            return newFileName;
        }

//        private List<Document> BuildAdvanceSearchData(DapperContext dataContext, Guid DocTypeId, SearchQuery query, int pageIndex)
//        {
//            const string SEARCH_TABLE = "#AdvanceSearch";

//            var exprsBuilder = new StringBuilder();
//            var hashSetColumns = new HashSet<string>();

//            string value1;
//            string value2;
//            string conjunction;
//            string searchOperator;
//            string strFieldId;
//            string fieldDataType;
//            string columnSearch;
//            string[] systemUniqueInfo;

//            int tempInt;
//            decimal tempDecimal;
//            bool tempBool;
//            DateTime tempDateTime1;
//            DateTime tempDateTime2;

//            var queryCreateColumns = new StringBuilder();
//            var querySelectSystemColumns = new StringBuilder();
//            var queryInsertSystemColumns = new StringBuilder();
//            var querySelectNormalColumns = new StringBuilder();
//            var queryWhereNormalColumns = new StringBuilder();

//            foreach (var expr in query.SearchQueryExpressions)
//            {
//                #region
//                value1 = string.Format("{0}", expr.Value1).Trim();

//                // Just work with search query have the value 1
//                if (value1 == string.Empty)
//                {
//                    continue;
//                }

//                conjunction = string.Format("{0}", expr.Condition).Trim().ToUpper();
//                searchOperator = string.Format("{0}", expr.Operator).Trim().ToUpper();

//                #region Check and make the conjunction have the length is 3 character
//                if (conjunction == "OR")
//                {
//                    conjunction = "OR ";
//                }
//                else if (conjunction == string.Empty)
//                {
//                    conjunction = "AND";
//                }
//                else if (conjunction != "AND")
//                {
//                    throw new ArgumentException("Invalid search conjunction: " + conjunction);
//                }
//                #endregion

//                // In case normal field, the left operand is id of field meta
//                // Case system field, the left operand is unique id of field meta
//                strFieldId = expr.FieldMetaData.Id == Guid.Empty ? expr.FieldMetaData.UniqueId
//                                                                 : expr.FieldMetaData.Id.ToString();

//                fieldDataType = string.Format("{0}", expr.FieldMetaData.DataType).ToUpper();
//                columnSearch = BatchManager.MapSystemUniqueIdToName(strFieldId);

//                switch (fieldDataType)
//                {
//                    case "PICKLIST":
//                    case "STRING":
//                        #region Type string
//                        value1 = value1.Replace("'", "''");

//                        if (searchOperator != "Equal" && searchOperator != "NotEqual")
//                        {
//                            value1 = value1.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
//                        }

//                        switch (searchOperator)
//                        {
//                            case "CONTAINS":
//                                exprsBuilder.AppendFormat(" {0} [{1}] LIKE '%{2}%' ", conjunction, columnSearch, value1);
//                                break;
//                            case "NOTCONTAINS":
//                                exprsBuilder.AppendFormat(" {0} [{1}] NOT LIKE '%{2}%' ", conjunction, columnSearch, value1);
//                                break;

//                            case "STARTSWITH":
//                                exprsBuilder.AppendFormat(" {0} [{1}] LIKE '{2}%' ", conjunction, columnSearch, value1);
//                                break;
//                            case "ENDSWITH":
//                                exprsBuilder.AppendFormat(" {0} [{1}] LIKE '%{2}' ", conjunction, columnSearch, value1);
//                                break;

//                            case "EQUAL":
//                                exprsBuilder.AppendFormat(" {0} [{1}] = '{2}' ", conjunction, columnSearch, value1);
//                                break;
//                            case "NOTEQUAL":
//                                exprsBuilder.AppendFormat(" {0} [{1}] <> '{2}' ", conjunction, columnSearch, value1);
//                                break;

//                            default:
//                                throw new ArgumentException("Invalid search operation for type " + fieldDataType + ": " + searchOperator);
//                        }
//                        break;
//                        #endregion

//                    case "INTEGER":
//                    case "DECIMAL":
//                        #region Type integer or decimal

//                        if (fieldDataType == "INTEGER")
//                        {
//                            if (!int.TryParse(value1, out tempInt))
//                            {
//                                throw new ArgumentException("Invalid type int of value1: " + value1);
//                            }
//                        }
//                        else
//                        {
//                            if (!decimal.TryParse(value1, out tempDecimal))
//                            {
//                                throw new ArgumentException("Invalid type decimal of value1: " + value1);
//                            }
//                        }

//                        switch (searchOperator)
//                        {
//                            case "EQUAL":
//                                exprsBuilder.AppendFormat(" {0} [{1}] = {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "NOTEQUAL":
//                                exprsBuilder.AppendFormat(" {0} [{1}] <> {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "GREATERTHAN":
//                                exprsBuilder.AppendFormat(" {0} [{1}] > {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "GREATERTHANOREQUALTO":
//                                exprsBuilder.AppendFormat(" {0} [{1}] >= {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "LESSTHAN":
//                                exprsBuilder.AppendFormat(" {0} [{1}] < {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "LESSTHANOREQUALTO":
//                                exprsBuilder.AppendFormat(" {0} [{1}] <= {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "INBETWEEN":
//                                value2 = string.Format("{0}", expr.Value2).Trim();

//                                if (fieldDataType == "INTEGER")
//                                {
//                                    if (!int.TryParse(value2, out tempInt))
//                                    {
//                                        throw new ArgumentException("Invalid type int of value2: " + value2);
//                                    }
//                                }
//                                else
//                                {
//                                    if (!decimal.TryParse(value2, out tempDecimal))
//                                    {
//                                        throw new ArgumentException("Invalid type decimal of value2: " + value2);
//                                    }
//                                }
//                                exprsBuilder.AppendFormat(" {0} ([{1}] >= {2} AND [{1}] <= {3}) ", conjunction, columnSearch, value1, value2);
//                                break;
//                            default:
//                                throw new ArgumentException("Invalid search operation for number: " + searchOperator);
//                        }
//                        break;
//                        #endregion

//                    case "BOOLEAN":
//                        #region Type bool

//                        if (!bool.TryParse(value1, out tempBool))
//                        {
//                            throw new ArgumentException("Invalid type bool of value1: " + value1);
//                        }

//                        switch (searchOperator)
//                        {
//                            case "EQUAL":
//                                exprsBuilder.AppendFormat(" {0} [{1}] = {2} ", conjunction, columnSearch, value1);
//                                break;
//                            case "NOTEQUAL":
//                                exprsBuilder.AppendFormat(" {0} [{1}] <> {2} ", conjunction, columnSearch, value1);
//                                break;
//                            default:
//                                throw new ArgumentException("Invalid search operation for type bool: " + searchOperator);
//                        }
//                        break;
//                        #endregion

//                    case "DATE":
//                        #region Type date

//                        if (!DateTime.TryParseExact(value1, "yyyy-MM-dd", null, DateTimeStyles.None, out tempDateTime1))
//                        {
//                            throw new ArgumentException("Invalid type date time with format 'yyyy-MM-dd' of value1: " + value1);
//                        }

//                        switch (searchOperator)
//                        {
//                            case "EQUAL":
//                                exprsBuilder.AppendFormat(" {0} ([{1}] >= '{2}' AND [{1}] < '{3}') ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"), tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
//                                break;
//                            case "NOTEQUAL":
//                                exprsBuilder.AppendFormat(" {0} ([{1}] < '{2}' OR [{1}] >= '{3}') ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"), tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
//                                break;
//                            case "GREATERTHAN":
//                                exprsBuilder.AppendFormat(" {0} [{1}] >= '{2}' ", conjunction, columnSearch, tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
//                                break;
//                            case "GREATERTHANOREQUALTO":
//                                exprsBuilder.AppendFormat(" {0} [{1}] >= '{2}' ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"));
//                                break;
//                            case "LESSTHAN":
//                                exprsBuilder.AppendFormat(" {0} [{1}] < '{2}' ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"));
//                                break;
//                            case "LESSTHANOREQUALTO":
//                                exprsBuilder.AppendFormat(" {0} [{1}] < '{2}' ", conjunction, columnSearch, tempDateTime1.AddDays(1).ToString("yyyy-MM-dd"));
//                                break;
//                            case "INBETWEEN":
//                                value2 = string.Format("{0}", expr.Value2).Trim();
//                                if (!DateTime.TryParseExact(value2, "yyyy-MM-dd", null, DateTimeStyles.None, out tempDateTime2))
//                                {
//                                    throw new ArgumentException("Invalid type date time with format 'yyyy-MM-dd' of value2: " + value2);
//                                }
//                                exprsBuilder.AppendFormat(" {0} ([{1}] >= '{2}' AND [{1}] < '{3}') ", conjunction, columnSearch, tempDateTime1.ToString("yyyy-MM-dd"), tempDateTime2.AddDays(1).ToString("yyyy-MM-dd"));
//                                break;
//                            default:
//                                throw new ArgumentException("Invalid search operation for date time: " + searchOperator);
//                        }
//                        break;
//                        #endregion

//                    default:
//                        break;
//                }

//                exprsBuilder.AppendLine();

//                if (hashSetColumns.Contains(strFieldId))
//                {
//                    continue;
//                }

//                // Add to hash set for tracking
//                hashSetColumns.Add(strFieldId);

//                // Case system field
//                if (expr.FieldMetaData.Id == Guid.Empty)
//                {
//                    #region

//                    systemUniqueInfo = BatchManager.MapSystemUniqueIdInfo(strFieldId);
//                    if (systemUniqueInfo == null)
//                    {
//                        throw new ArgumentException("Invalid system field unique id: " + strFieldId);
//                    }

//                    queryCreateColumns.AppendFormat(",[{0}] {1} ", systemUniqueInfo);
//                    queryInsertSystemColumns.AppendFormat(",[{0}] ", systemUniqueInfo[0]);
//                    querySelectSystemColumns.AppendFormat(",[{0}]", systemUniqueInfo[0]);

//                    querySelectSystemColumns.AppendLine();
//                    queryInsertSystemColumns.AppendLine();

//                    #endregion
//                }
//                // Case normal field
//                else
//                {
//                    #region

//                    switch (fieldDataType)
//                    {
//                        case "STRING":
//                        case "PICKLIST":
//                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "NVARCHAR(MAX)");
//                            break;
//                        case "INTEGER":
//                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "INT");
//                            break;
//                        case "DECIMAL":
//                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "DECIMAL(38,10)");
//                            break;
//                        case "BOOLEAN":
//                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "BIT");
//                            break;
//                        case "DATE":
//                            queryCreateColumns.AppendFormat(",[{0}] {1} ", strFieldId, "DATETIME");
//                            break;
//                    }

//                    querySelectNormalColumns.AppendFormat(",[{0}] ", strFieldId);
//                    queryWhereNormalColumns.AppendFormat("OR FieldId = '{0}' ", strFieldId);

//                    querySelectNormalColumns.AppendLine();
//                    queryWhereNormalColumns.AppendLine();

//                    #endregion
//                }

//                queryCreateColumns.AppendLine();
//                #endregion
//            }

//            // Check have at least on valid search condition
//            if (queryCreateColumns.Length == 0)
//            {
//                throw new ArgumentException("Have no valid search expression.");
//            }

//            string querySearch;

//            // In case search by only system fields
//            // => just need search in table Batch
//            if (querySelectNormalColumns.Length == 0)
//            {
//                #region
//                querySearch = string.Format(@"
//SELECT
//    Id
//FROM
//(
//    SELECT TOP (@To)
//        ROW_NUMBER() OVER (ORDER BY Id) rowNumber,
//        Id
//    FROM
//        Batch
//    WHERE
//        BatchTypeId = '{0}' -- batchTypeId
//        AND (
//{1} -- exprsBuilder
//        )
//) result
//WHERE
//    rowNumber BETWEEN @From AND @To
//ORDER BY
//    rowNumber
//", DocTypeId, exprsBuilder.Remove(0, 4).ToString());
//                #endregion
//            }
//            // In case search by only normal fields
//            else if (queryInsertSystemColumns.Length == 0)
//            {
//                #region
//                querySearch = string.Format(@"
//IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
//	DROP TABLE {0}; -- temp table name
//
//CREATE TABLE {0} -- temp table name
//(
//    BatchId uniqueidentifier
//    {1} -- queryCreateColumns
//);
//
//INSERT INTO {0} -- temp table name 
//(
//    BatchId,
//    {2} -- querySelectNormalColumns
//)
//SELECT
//    BatchId,
//    {2} -- querySelectNormalColumns
//FROM
//(
//	SELECT
//		BatchId, FieldId, Value
//	FROM
//		BatchFieldValue
//	WHERE 
//		{3} -- queryWhereNormalColumns
//) d
//PIVOT
//(
//	MAX(Value)
//	FOR FieldId IN (
//        {2} -- querySelectNormalColumns
//    )
//) piv;
//
//SELECT
//    BatchId
//FROM
//(
//    SELECT TOP (@To)
//        ROW_NUMBER() OVER (ORDER BY BatchId) rowNumber,
//        BatchId
//    FROM
//        {0} -- temp table name 
//    WHERE
//        {4} -- exprsBuilder
//) result
//WHERE
//    rowNumber BETWEEN @From AND @To
//ORDER BY
//    rowNumber;
//
//IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
//	DROP TABLE {0}; -- temp table name
//",
//                SEARCH_TABLE,
//                queryCreateColumns.ToString(),
//                querySelectNormalColumns.Remove(0, 1).ToString(),
//                queryWhereNormalColumns.Remove(0, 2).ToString(),
//                exprsBuilder.Remove(0, 4).ToString());
//                #endregion
//            }
//            // In case search by system and normal fields
//            else
//            {
//                #region
//                querySearch = string.Format(@"
//IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
//	DROP TABLE {0}; -- temp table name
//
//CREATE TABLE {0} -- temp table name
//(
//    BatchId uniqueidentifier
//    {1} -- queryCreateColumns
//);
//
//INSERT INTO {0} -- temp table name 
//(
//    BatchId,
//    {2} -- querySelectNormalColumns
//    {5} -- querySelectSystemColumns
//)
//SELECT
//    BatchId,
//    {2} -- querySelectNormalColumns
//    {5} -- querySelectSystemColumns
//FROM
//(
//	SELECT
//		BatchId, FieldId, Value
//	FROM
//		BatchFieldValue
//	WHERE 
//		{3} -- queryWhereNormalColumns
//) d
//PIVOT
//(
//	MAX(Value)
//	FOR FieldId IN (
//        {2} -- querySelectNormalColumns
//    )
//) piv
//INNER JOIN Batch ON piv.BatchId = Batch.Id;
//
//SELECT
//    BatchId
//FROM
//(
//    SELECT TOP (@To)
//        ROW_NUMBER() OVER (ORDER BY BatchId) rowNumber,
//        BatchId
//    FROM
//        {0} -- temp table name 
//    WHERE
//        {4} -- exprsBuilder
//) result
//WHERE
//    rowNumber BETWEEN @From AND @To
//ORDER BY
//    rowNumber;
//
//IF OBJECT_ID('tempdb..{0}') IS NOT NULL 
//	DROP TABLE {0}; -- temp table name
//",
//                SEARCH_TABLE,
//                queryCreateColumns.ToString(),
//                querySelectNormalColumns.Remove(0, 1).ToString(),
//                queryWhereNormalColumns.Remove(0, 2).ToString(),
//                exprsBuilder.Remove(0, 4).ToString(),
//                querySelectSystemColumns.ToString());
//                #endregion
//            }

//            var batchIds = new SearchDao(dataContext).GetBatchFromSearch(null, querySearch, pageIndex);

//            return new BatchDao(dataContext).GetBatchByRange(batchIds);
//        }

    }
}