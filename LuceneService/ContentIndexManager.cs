using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Domain;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Directory = Lucene.Net.Store.Directory;
using Document = Lucene.Net.Documents.Document;
using Dom = Ecm.Domain;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;

namespace Ecm.LuceneService
{
    public class ContentIndexManager
    {
        public const string _fieldCreatedBy = "CreatedBy_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldCreatedDate = "CreatedDate_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldDocTypeId = "DocTypeId_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldId = "Id_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldVersion = "Version_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldModifiedBy = "ModifiedBy_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldModifiedDate = "ModifiedDate_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldPageCount = "PageCount_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldBinaryType = "BinaryType_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldSort = "ModifiedTicks_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _fieldContent = "Content_CFCCCDF6_84CC_43ED_BE4A_617974473143";
        public const string _dateTimeValueFormat = "yyyyMMdd";
        public const string _staticDateTimeValueFormat = "yyyyMMdd hh:mm:ss";

        private static object _myLock = new object();
        private string _content;

        public ContentIndexManager(string content)
        {
            _content = content;
        }

        public void CreateIndex(Dom.Document document)
        {
            _myLock = 1;
            lock (_myLock)
            {
                Directory directory = new FolderManager().GetDirectory(document.DocumentType.Id);
                IndexWriter writer = null;
                try
                {
                    Document luDocument = CreateDocument(document);
                    Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable());
                    writer = new IndexWriter(directory, analyzer, !IndexExists(directory), IndexWriter.MaxFieldLength.UNLIMITED);
                    writer.AddDocument(luDocument);
                    writer.Optimize();
                    writer.Commit();
                }
                finally
                {
                    if (writer != null)
                    {
                        try
                        {
                            writer.Close();
                        }
                        finally
                        {
                            if (IndexWriter.IsLocked(directory))
                            {
                                IndexWriter.Unlock(directory);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateIndex(Dom.Document document)
        {
            Directory directory = new FolderManager().GetDirectory(document.DocumentType.Id);
            lock (_myLock)
            {
                if (!IndexExists(directory))
                {
                    return;
                }

                IndexWriter writer = null;
                try
                {
                    Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable()); //new LowerCaseAnalyzer();///
                    writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                    writer.DeleteDocuments(NumericRangeQuery.NewLongRange(_fieldId, document.Id, document.Id, true, true));
                    writer.Commit();
                    Document luDocument = CreateDocument(document);
                    writer.AddDocument(luDocument);
                    writer.Optimize();
                    writer.Commit();
                }
                finally
                {
                    if (writer != null)
                    {
                        try
                        {
                            writer.Close();
                        }
                        finally
                        {
                            if (IndexWriter.IsLocked(directory))
                            {
                                IndexWriter.Unlock(directory);
                            }
                        }
                    }
                }
            }
        }

        public void DeleteIndex(Dom.Document document)
        {
            Directory directory = new FolderManager().GetDirectory(document.DocumentType.Id);
            lock (_myLock)
            {
                if (!IndexExists(directory))
                {
                    return;
                }

                IndexWriter writer = null;
                try
                {
                    Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable()); //new LowerCaseAnalyzer();//
                    writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                    writer.DeleteDocuments(NumericRangeQuery.NewLongRange(_fieldId, document.Id, document.Id, true, true));
                    writer.Commit();
                }
                finally
                {
                    if (writer != null)
                    {
                        try
                        {
                            writer.Close();
                        }
                        finally
                        {
                            if (IndexWriter.IsLocked(directory))
                            {
                                IndexWriter.Unlock(directory);
                            }
                        }
                    }
                }
            }
        }

        public void DeleteDocumentType(DocumentType documentType)
        {
            var folderManager = new FolderManager();
            Directory directory = folderManager.GetDirectory(documentType.Id);
            lock (_myLock)
            {
                if (!IndexExists(directory))
                {
                    return;
                }

                System.IO.Directory.Delete(folderManager.GetDirectoryPath(documentType.Id.ToString()), true);
            }
        }

        public SearchResult RunSearchContent(DocumentType docType, string text, int pageIndex, int pageSize)
        {
            Directory directory = null;
            Searcher searcher = null;
            IndexReader indexReader = null;
            try
            {
                directory = new FolderManager().GetDirectory(docType.Id);
                if (!IndexExists(directory))
                {
                    return new SearchResult();
                }

                var query = new QueryManager().BuildContentSearchQuery(docType, text);

                indexReader = IndexReader.Open(directory, true);
                searcher = new IndexSearcher(indexReader);
                TopDocs resultDocs = searcher.Search(query, indexReader.MaxDoc());
                var hits = resultDocs.ScoreDocs;
                int first = pageIndex * pageSize;
                int last = (pageIndex + 1) * pageSize;
                List<Document> documents = new List<Document>();
                for (int i = first; i < last && i < hits.Length; i++)
                {
                    documents.Add(searcher.Doc(hits[i].doc));
                }

                return CreateSearchResult(documents, docType, pageIndex, last < hits.Length, hits.Length);
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Close();
                }

                if (directory != null)
                {
                    directory.Close();
                }

                if (indexReader != null)
                {
                    indexReader.Close();
                }
            }
        }

        private Document CreateDocument(Dom.Document document)
        {
            Document lucDocument = new Document();
            lucDocument.Add(new Field(_fieldContent, _content, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));

            return lucDocument;
        }

        private Dom.Document CreateDocument(Document lucDocument, DocumentType docType)
        {
            var document = new Dom.Document();
            ExtractStaticFields(lucDocument, document);
            ExtractDynamicFields(lucDocument, document, docType);
            return document;
        }

        private bool IndexExists(Directory indexDir)
        {
            return IndexReader.IndexExists(indexDir);
        }

        private SearchResult CreateSearchResult(IEnumerable<Document> documents, DocumentType docType, int pageIndex, bool hasMoreResult, int totalCount)
        {
            SearchResult result = new SearchResult { DocumentType = docType, PageIndex = pageIndex, HasMoreResult = hasMoreResult, TotalCount = totalCount };
            foreach (Document document in documents)
            {
                result.Documents.Add(CreateDocument(document, docType));
            }

            return result;
        }
        private void ExtractStaticFields(Document lucDocument, Domain.Document document)
        {
            var fields = lucDocument.GetFields();
            foreach (Field field in fields)
            {
                string fieldName = field.Name();
                switch (fieldName)
                {
                    case _fieldCreatedBy:
                        document.CreatedBy = field.StringValue();
                        break;
                    case _fieldCreatedDate:
                        document.CreatedDate = DateTime.ParseExact(field.StringValue(), _staticDateTimeValueFormat, null);
                        break;
                    case _fieldId:
                        document.Id = Guid.Parse(field.StringValue());
                        break;
                    case _fieldVersion:
                        document.Version = int.Parse(field.StringValue());
                        break;
                    case _fieldModifiedBy:
                        document.ModifiedBy = field.StringValue();
                        break;
                    case _fieldModifiedDate:
                        document.ModifiedDate = DateTime.ParseExact(field.StringValue(), _staticDateTimeValueFormat, null);
                        break;
                    case _fieldPageCount:
                        document.PageCount = int.Parse(field.StringValue());
                        break;
                    case _fieldBinaryType:
                        document.BinaryType = field.StringValue();
                        break;
                    case _fieldDocTypeId:
                        document.DocTypeId = Guid.Parse(field.StringValue());
                        break;
                }
            }
        }

        private void ExtractDynamicFields(Document lucDocument, Domain.Document document, DocumentType docType)
        {
            var fields = lucDocument.GetFields();
            foreach (Field field in fields)
            {
                string fieldName = field.Name();
                //Guid FieldMetaData metaField = docType.FieldMetaDatas.FirstOrDefault(p => p.FieldUniqueId == fieldName);
                FieldMetaData metaField = docType.FieldMetaDatas.FirstOrDefault(p => p.Id .ToString().Equals(fieldName));
                if (metaField != null)
                {
                    document.FieldValues.Add(new FieldValue
                    {
                        DocId = document.Id,
                        FieldId = metaField.Id,
                        FieldMetaData = metaField,
                        Value = field.StringValue()
                    });

                }
            }
        }

    }
}
