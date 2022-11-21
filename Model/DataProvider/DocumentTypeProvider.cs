using System.Collections.Generic;
using System.Linq;
using Ecm.Domain;
using System.Collections.ObjectModel;
using Ecm.Utility.ProxyHelper;
using Ecm.Service.Contract;
using System;

namespace Ecm.Model.DataProvider
{
    public class DocumentTypeProvider : ProviderBase
    {
        public IList<DocumentTypeModel> GetCapturedDocumentTypes()
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                var docTypes = client.Channel.GetCapturedDocumentTypes();
                return ObjectMapper.GetDocumentTypeModels(docTypes);
            }            
        }

        public ObservableCollection<DocumentTypeModel> GetDocumentTypes()
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypeModels(client.Channel.GetDocumentTypes());
            }
        }

        public DocumentTypeModel GetDocumentType(Guid id)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypeModel(client.Channel.GetDocumentType(id));
            }
        }

        public void SaveDocumentType(DocumentTypeModel documentType)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.SaveDocumentType(ObjectMapper.GetDocumentType(documentType));
            }
        }

        public void DeleteDocumentType(DocumentTypeModel documentType)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.DeleteDocumentType(ObjectMapper.GetDocumentType(documentType));
            }
        }

        public void SaveOCRTemplate(OCRTemplateModel OCRTemplate)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.SaveOCRTemplate(ObjectMapper.GetOCRTemplate(OCRTemplate));
            }
        }

        public void DeleteOCRTemplate(Guid documentTypeId)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.DeleteOCRTemplate(documentTypeId);
            }
        }

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

        public List<BarcodeConfigurationModel> GetBarcodeConfigurations(Guid docTypeId)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetBarcodeConfigurations(client.Channel.GetBarcodeConfigurations(docTypeId).ToList());
            }
        }

        public void DeleteBarcodeConfiguration(Guid id)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.DeleteBarcodeConfiguration(id);
            }
        }

        public void SaveBarcodeConfiguration(BarcodeConfigurationModel barcode)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.SaveBarcodeConfiguration(ObjectMapper.GetBarcodeConfiguration(barcode));
            }
        }

        public void ClearBarcodeConfigurations(Guid documentTypeId)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.ClearBarcodeConfigurations(documentTypeId);
            }
        }

    }
}