using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.ServiceModel;
using Ecm.Domain;
using Ecm.LuceneService.Contract;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true, ConfigFile=@"log4net.xml")]

namespace Ecm.LuceneService
{
    public class Indexer : IIndexer
    {
        //Reserved Words "a", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "s", "such", "t", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"
        private readonly ILog _log = LogManager.GetLogger(typeof(Indexer));

        public void CreateIndex(string authorizeId, Document document)
        {
            try
            {
                CheckAuthentication(authorizeId);
                new IndexManager().CreateIndex(document);
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, "Create lucene index fail for document id = " + document.Id);
            }
        }

        public void UpdateIndex(string authorizeId, Document document)
        {
            try
            {
                new IndexManager().UpdateIndex(document);
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, "Update lucene index fail for document id = " + document.Id);
            }
        }

        public void DeleteIndex(string authorizeId, Document document)
        {
            try
            {
                new IndexManager().DeleteIndex(document);
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, "Delete lucene index fail for document id = " + document.Id);
            }
        }

        public void DeleteDocumentType(DocumentType documentType)
        {
            try
            {
                new IndexManager().DeleteDocumentType(documentType);
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, "Delete lucene index fail when deleting document type '" + documentType.Name + "'");
            }
        }

        public void DeleteField(DocumentType documentType, FieldMetaData field)
        {
            try
            {
                //new IndexManager().DeleteField(documentType, field);
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, "Delete lucene index fail when deleting metadata field = " + field.Name + "of document type = " + documentType.Name);
            }
        }

        public SearchResult RunAdvanceSearch(string authorizeId, DocumentType docType, SearchQuery query, int pageIndex, int pageSize, string sortColumn, string sortDir)
        {
            try
            {

                CheckAuthentication(authorizeId);

                return new IndexManager().RunAdvanceSearch(docType, query, pageIndex, pageSize, sortColumn, sortDir);
            }
            catch (Exception ex)
            {
                if (ex is NotSupportedException || ex is ArgumentException || ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, string.Format("Run advance search for documents fail on document type id = {0}, Name={1}.", docType.Id, docType.Name));
            }
        }

        public List<SearchResult> RunGlobalSearch(string authorizeId, string keyword, List<DocumentType> documentTypes, int pageIndex, int pageSize)
        {
            try
            {
                CheckAuthentication(authorizeId);

                return new IndexManager().RunGlobalSearch(keyword, documentTypes, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                if (ex is NotSupportedException || ex is ArgumentException || ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, string.Format("Run global search for documents fail"));
            }
        }

        public SearchResult RunSearchContent(string authorizeId, DocumentType docType, string text, int pageIndex, int pageSize)
        {
            try
            {
                CheckAuthentication(authorizeId);

                return new IndexManager().RunSearchContent(docType, text, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                if (ex is NotSupportedException || ex is ArgumentException || ex is AuthenticationException)
                {
                    throw ProcessException(ex, ex.Message);
                }

                throw ProcessException(ex, string.Format("Run advance search for documents fail on document type id = {0}, Name={1}.", docType.Id, docType.Name));
            }
        }

        private Exception ProcessException(Exception ex, string errorMessage)
        {
            //log4net.Config.XmlConfigurator.Configure();
            _log.Error(errorMessage, ex);
            return new FaultException(errorMessage);
        }

        private void CheckAuthentication(string authorizeId)
        {
            if ("EE2271D5-F17C-4F9D-A85C-1383AAA218D7" != authorizeId)
            {
                throw new AuthenticationException("You're not authorize to use this service.");
            }
        }

    }
}
