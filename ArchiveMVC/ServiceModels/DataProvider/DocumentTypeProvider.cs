using System.Collections.Generic;
using System.Linq;
using Ecm.Domain;

using Ecm.Utility.ProxyHelper;
using Ecm.Service.Contract;
using System;

namespace ArchiveMVC.Models.DataProvider
{
    public class DocumentTypeProvider : ProviderBase
    {
        
        public DocumentTypeProvider(string user, string pass)
        {
            Configure(user, pass);
        }

        /// <summary>
        /// Lấy danh sách CapturedDocumentTypes 
        /// </summary>
        /// <returns></returns>
        public IList<DocumentTypeModel> GetCapturedDocumentTypes()
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {                
                var docTypes = client.Channel.GetCapturedDocumentTypes();
                return ObjectMapper.GetDocumentTypeModels(docTypes);
            }            
        }



        /// <summary>
        /// Lấy DocumentTypes
        /// </summary>
        /// <returns></returns>
        public List<DocumentTypeModel> GetDocumentTypes()
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypeModels(client.Channel.GetDocumentTypes());
            }
        }



        /// <summary>
        /// Lấy DocumentType theo id
        /// </summary>
        /// <param name="id">id DocumentType cần lấy</param>
        /// <returns></returns>
        public DocumentTypeModel GetDocumentType(Guid id)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypeModel(client.Channel.GetDocumentType(id));
            }
        }



        /// <summary>
        /// Lưu DocumentType 
        /// </summary>
        /// <param name="documentType">DocumentType cần lưu</param>
        public void SaveDocumentType(DocumentTypeModel documentType)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.SaveDocumentType(ObjectMapper.GetDocumentType(documentType));
            }
        }


        /// <summary>
        /// Xóa DocumentType
        /// </summary>
        /// <param name="documentType">DocumentType cần xóa</param>
        public void DeleteDocumentType(DocumentTypeModel documentType)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.DeleteDocumentType(ObjectMapper.GetDocumentType(documentType));
            }
        }


        /// <summary>
        /// Lưu OCRTemplate
        /// </summary>
        /// <param name="OCRTemplate">OCRTemplate cần lưu </param>
        public void SaveOCRTemplate(OCRTemplateModel OCRTemplate)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.SaveOCRTemplate(ObjectMapper.GetOCRTemplate(OCRTemplate));
            }
        }


        /// <summary>
        /// Xóa OCRTemplate theo Id
        /// </summary>
        /// <param name="documentTypeId">Id OCRTemplate cần xóa</param>
        public void DeleteOCRTemplate(Guid documentTypeId)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.DeleteOCRTemplate(documentTypeId);
            }
        }




        /// <summary>
        /// Lấy DeletedDocumentTypes 
        /// </summary>
        /// <returns></returns>
        public IList<DocumentTypeModel> GetDeletedDocumentTypes()
        {
            var docTypes = new List<DocumentTypeModel>();

            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                IList<DocumentTypeVersion> docTypeVersions = client.Channel.GetDocumentTypeVersions();

                if (docTypeVersions != null && docTypeVersions.Count > 0)
                {
                    docTypes = docTypeVersions.Select(docType => new DocumentTypeModel
                    {
                        CreateBy = docType.CreatedBy,
                        CreatedDate = docType.CreatedDate,
                        Id = docType.Id,
                        Name = docType.Name,
                        Status = Status.Deleted
                    }).ToList();
                }
            }
            return docTypes;
        }


        /// <summary>
        /// Lấy BarcodeConfigurations theo docTypeId
        /// </summary>
        /// <param name="docTypeId">docTyeId của BarcodeConfigurations cần lấy</param>
        /// <returns></returns>
        public List<BarcodeConfigurationModel> GetBarcodeConfigurations(Guid docTypeId)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetBarcodeConfigurations(client.Channel.GetBarcodeConfigurations(docTypeId).ToList());
            }
        }



        /// <summary>
        /// Xóa BarcodeConfigurations theo id
        /// </summary>
        /// <param name="id">id BarcodeConfigurations cần xóa </param>
        public void DeleteBarcodeConfiguration(Guid id)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.DeleteBarcodeConfiguration(id);
            }
        }


        /// <summary>
        /// Lưu BarcodeConfiguration
        /// </summary>
        /// <param name="barcode">barcode cần lưu </param>
        public void SaveBarcodeConfiguration(BarcodeConfigurationModel barcode)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.SaveBarcodeConfiguration(ObjectMapper.GetBarcodeConfiguration(barcode));
            }
        }



        /// <summary>
        /// Xóa BarcodeConfigurations theo id
        /// </summary>
        /// <param name="documentTypeId">id BarcodeConfigurations cần xóa</param>
        public void ClearBarcodeConfigurations(Guid documentTypeId)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.ClearBarcodeConfigurations(documentTypeId);
            }
        }



        ///// <summary>
        ///// Xóa LookupInfo theo id
        ///// </summary>
        ///// <param name="fieldId">id LookupInfo cần xóa</param>
        //public void DeleteLookupInfo(Guid fieldId)
        //{
        //    using (ClientChannel<IArchive> client = GetArchiveClientChannel())
        //    {
        //        client.Channel.DeleteLookupInfo(fieldId);
        //    }
        //}
    }
}