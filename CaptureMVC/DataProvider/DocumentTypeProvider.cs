using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptureMVC.DataProvider
{
    public class DocumentTypeProvider : ProviderBase
    {
        /// <summary>
        /// get DocumentType follow id
        /// </summary>
        /// <param name="id">id DocumentType</param>
        /// <returns></returns>
        public DocumentType GetDocumentType(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetDocumentType(id);
            }
        }
        /// <summary>
        /// Get DocumentTypes
        /// </summary>
        /// <returns></returns>
        public List<DocumentType> GetDocumentTypes(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetDocumentTypes(id);
            }
        }
    }
}