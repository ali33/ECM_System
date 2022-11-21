using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using Lucene.Net.Analysis.Tokenattributes;

namespace Ecm.LuceneService
{
    public class IndexManager
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
        public const string _dateTimeValueFormat = "yyyyMMdd HH:mm:ss";
        public const string _staticDateTimeValueFormat = "yyyyMMdd HH:mm:ss";

        private static object _myLock = new object();

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
                    Analyzer analyzer = new LowerCaseAnalyzer();//new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable());//
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
            //Edit
            //Directory directory = new FolderManager().GetDirectory(document.DocumentType.UniqueId);

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
                    Analyzer analyzer = new LowerCaseAnalyzer();///new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable()); //
                    writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                    writer.DeleteDocuments(new Term(_fieldId, document.Id.ToString()));// NumericRangeQuery.NewLongRange(_fieldId, document.Id, document.Id, true, true));
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
            //Edit 
            //Directory directory = new FolderManager().GetDirectory(document.DocumentType.UniqueId);
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
                    Analyzer analyzer = new LowerCaseAnalyzer();//new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable()); //
                    writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                    writer.DeleteDocuments(new Term(_fieldId, document.Id.ToString()));//NumericRangeQuery.NewLongRange(_fieldId, document.Id, document.Id, true, true));
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
            //Edit
            //Directory directory = folderManager.GetDirectory(documentType.UniqueId);
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

        public void DeleteField(DocumentType documentType, FieldMetaData field)
        {
            Directory directory = new FolderManager().GetDirectory(documentType.Id);
            Searcher searcher = null;
            lock (_myLock)
            {
                if (!IndexExists(directory))
                {
                    return;
                }

                IndexWriter writer = null;
                try
                {
                    IndexReader indexReader = IndexReader.Open(directory, true);
                    searcher = new IndexSearcher(indexReader);
                    TopDocs resultDocs = searcher.Search(new MatchAllDocsQuery(), searcher.MaxDoc());
                    var hits = resultDocs.ScoreDocs;
                    Analyzer analyzer = new LowerCaseAnalyzer();//new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29, new System.Collections.Hashtable()); //new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);////
                    writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Document document = searcher.Doc(hits[i].doc);
                        document.RemoveField(field.Id.ToString());
                        var documentId = Guid.Parse(document.Get(_fieldId));
                        writer.DeleteDocuments(new Term(_fieldId, documentId.ToString()));//NumericRangeQuery.NewLongRange(_fieldId, documentId, documentId, true, true));
                        writer.AddDocument(document);
                    }

                    writer.Optimize();
                    writer.Commit();
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

        public SearchResult RunAdvanceSearch(DocumentType docType, SearchQuery searchQuery, int pageIndex, int pageSize, string sortField, string sortDir)
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

                Query query = new QueryManager().BuildAdvanceSearchQuery(docType, searchQuery);
                indexReader = IndexReader.Open(directory, true);
                searcher = new IndexSearcher(indexReader);
                var sortData = sortField;
                var isAsc = string.IsNullOrEmpty(sortDir) ? true : (sortDir == "asc" ? true : false);
                Sort sort = null;

                if (!string.IsNullOrEmpty(sortField))
                {
                    switch (sortField)
                    {
                        case _fieldCreatedDate:
                        case _fieldCreatedBy:
                        case _fieldModifiedBy:
                        case _fieldModifiedDate:
                            sort = new Sort(new SortField(sortData, SortField.STRING, isAsc));
                            break;
                        case _fieldPageCount:
                        case _fieldVersion:
                            sort = new Sort(new SortField(sortData, SortField.LONG, isAsc));
                            break;
                        default:
                            FieldMetaData field = docType.FieldMetaDatas.FirstOrDefault(p => p.Id.ToString() == sortField);
                            if (field == null)
                            {
                                sortData = _fieldSort;
                                sort = new Sort(new SortField(sortData, SortField.LONG, isAsc));
                            }
                            else
                            {
                                switch (field.DataTypeEnum)
                                {
                                    case FieldDataType.Integer:
                                        sort = new Sort(new SortField(sortData, SortField.LONG, isAsc));
                                        break;
                                    case FieldDataType.Decimal:
                                        sort = new Sort(new SortField(sortData, SortField.DOUBLE, isAsc));
                                        break;
                                    case FieldDataType.String:
                                    case FieldDataType.Picklist:
                                    case FieldDataType.Boolean:
                                    case FieldDataType.Date:
                                        sort = new Sort(new SortField(sortData, SortField.STRING, isAsc));
                                        break;
                                    default:
                                        sort = new Sort(new SortField(sortData, SortField.STRING, isAsc));
                                        break;
                                }
                            }

                            break;
                    }
                }
                else
                {
                    sortData = _fieldSort;
                    sort = new Sort(new SortField(sortData, SortField.LONG, isAsc));
                }

                var filter = new QueryWrapperFilter(query);
                TopDocs resultDocs = searcher.Search(query, filter, indexReader.MaxDoc(), sort);
                var hits = resultDocs.ScoreDocs;
                List<Document> documents = new List<Document>();
                int last = 0;

                if (pageSize == -1)
                {
                    last = hits.Length;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        documents.Add(searcher.Doc(hits[i].doc));
                    }
                }
                else
                {
                    int first = pageIndex * pageSize;
                    last = (pageIndex + 1) * pageSize;
                    for (int i = first; i < last && i < hits.Length; i++)
                    {
                        documents.Add(searcher.Doc(hits[i].doc));
                    }
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

        public List<SearchResult> RunGlobalSearch(string keyword, List<DocumentType> documentTypes, int pageIndex, int pageSize)
        {
            List<Directory> directories = new List<Directory>();
            List<Searcher> searchers = new List<Searcher>();
            MultiSearcher searcher = null;
            try
            {
                foreach (var docType in documentTypes)
                {
                    Directory directory = new FolderManager().GetDirectory(docType.Id);
                    if (!IndexExists(directory))
                    {
                        continue;
                    }

                    IndexReader indexReader = IndexReader.Open(directory, true);
                    Searcher indexSearch = new IndexSearcher(indexReader);
                    searchers.Add(indexSearch);
                    directories.Add(directory);
                }

                if (searchers.Count > 0)
                {
                    searcher = new MultiSearcher(searchers.ToArray());
                    Query query = new QueryManager().BuildGlobalSearchQuery(documentTypes, keyword);
                    var sort = new Sort(new SortField(_fieldSort, SortField.LONG, true));
                    var filter = new QueryWrapperFilter(query);
                    TopDocs resultDocs = searcher.Search(query, filter, searcher.MaxDoc(), sort);
                    var hits = resultDocs.ScoreDocs;
                    List<Document> documents = new List<Document>();
                    int last = 0;

                    if (pageSize == -1)
                    {
                        last = hits.Length;
                        for (int i = 0; i < hits.Length; i++)
                        {
                            documents.Add(searcher.Doc(hits[i].doc));
                        }
                    }
                    else
                    {
                        int first = pageIndex * pageSize;
                        last = (pageIndex + 1) * pageSize;
                        for (int i = first; i < last && i < hits.Length; i++)
                        {
                            documents.Add(searcher.Doc(hits[i].doc));
                        }
                   }

                    return CreateSearchResult(documents, documentTypes, pageIndex, last < hits.Length);
                }

                return new List<SearchResult>();
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Close();
                }

                foreach (Searcher searcherItem in searchers)
                {
                    searcherItem.Close();
                }

                foreach (Directory directory in directories)
                {
                    directory.Close();
                }
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
                
                Query query = new QueryManager().BuildContentSearchQuery(docType, text);
                indexReader = IndexReader.Open(directory, true);
                searcher = new IndexSearcher(indexReader);
                TopDocs resultDocs = searcher.Search(query, indexReader.MaxDoc());
                var hits = resultDocs.ScoreDocs;
                List<Document> documents = new List<Document>();
                int last = 0;

                if (pageSize == -1)
                {
                    last = hits.Length;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        documents.Add(searcher.Doc(hits[i].doc));
                    }
                }
                else
                {
                    int first = pageIndex * pageSize;
                    last = (pageIndex + 1) * pageSize;
                    for (int i = first; i < last && i < hits.Length; i++)
                    {
                        documents.Add(searcher.Doc(hits[i].doc));
                    }
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
            AddStaticFields(document, lucDocument);
            AddDynamicFields(lucDocument, document);
            return lucDocument;
        }

        private static void AddStaticFields(Domain.Document document, Document lucDocument)
        {
            long sortValue = document.CreatedDate.Ticks;

           /* var docTypeId = new NumericField(_fieldDocTypeId, Field.Store.YES, true);
            docTypeId.SetLongValue(document.DocTypeID);
            lucDocument.Add(docTypeId);

            var id = new NumericField(_fieldId, Field.Store.YES, true);
            id.SetLongValue(document.ID);
            lucDocument.Add(id);*/
            lucDocument.Add(new Field(_fieldDocTypeId, document.DocTypeId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            lucDocument.Add(new Field(_fieldId, document.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED));

            lucDocument.Add(new Field(_fieldCreatedDate, document.CreatedDate.ToString(_staticDateTimeValueFormat), Field.Store.YES,
                                      Field.Index.NOT_ANALYZED));
            lucDocument.Add(new Field(_fieldCreatedBy, document.CreatedBy, Field.Store.YES, Field.Index.ANALYZED));

            if (!string.IsNullOrEmpty(document.ModifiedBy))
            {
                lucDocument.Add(new Field(_fieldModifiedDate, document.ModifiedDate.Value.ToString(_staticDateTimeValueFormat),
                                          Field.Store.YES, Field.Index.NOT_ANALYZED));
                lucDocument.Add(new Field(_fieldModifiedBy, document.ModifiedBy, Field.Store.YES, Field.Index.ANALYZED));
                sortValue = document.ModifiedDate.Value.Ticks;
            }

            var content = GetContentFromPages(document.Pages).ToLower().Replace("\n", " ").Replace("\r"," ");
            lucDocument.Add(new Field(_fieldContent, content, Field.Store.COMPRESS, Field.Index.ANALYZED));//, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));

            var version = new NumericField(_fieldVersion, Field.Store.YES, true);
            version.SetLongValue(document.Version);
            lucDocument.Add(version);

            var pageCount = new NumericField(_fieldPageCount, Field.Store.YES, true);
            pageCount.SetLongValue(document.PageCount);
            lucDocument.Add(pageCount);

            lucDocument.Add(new Field(_fieldBinaryType, document.BinaryType, Field.Store.YES, Field.Index.ANALYZED));

            var sortField = new NumericField(_fieldSort, Field.Store.YES, true);
            sortField.SetLongValue(sortValue);
            lucDocument.Add(sortField);
        }

        private void AddDynamicFields(Document lucDocument, Dom.Document document)
        {
            foreach (var fieldValue in document.FieldValues)
            {
                if (string.IsNullOrEmpty(fieldValue.Value))
                {
                    continue;
                }

                switch (fieldValue.FieldMetaData.DataTypeEnum)
                {
                    case FieldDataType.Date:
                        DateTime value = DateTime.Parse(fieldValue.Value);
                        lucDocument.Add(new Field(fieldValue.FieldMetaData.Id.ToString(), value.ToString(_dateTimeValueFormat), Field.Store.YES, Field.Index.NOT_ANALYZED));
                        break;
                    case FieldDataType.Decimal:
                        var decimalField = new NumericField(fieldValue.FieldMetaData.Id.ToString(), Field.Store.YES, true);
                        decimalField.SetDoubleValue(double.Parse(fieldValue.Value));
                        lucDocument.Add(decimalField);
                        break;
                    case FieldDataType.Integer:
                        var intField = new NumericField(fieldValue.FieldMetaData.Id.ToString(), Field.Store.YES, true);
                        intField.SetLongValue(long.Parse(fieldValue.Value));
                        lucDocument.Add(intField);
                        break;
                    case FieldDataType.Picklist:
                    case FieldDataType.String:
                    case FieldDataType.Boolean:
                        lucDocument.Add(new Field(fieldValue.FieldMetaData.Id.ToString(), fieldValue.Value, Field.Store.YES, Field.Index.ANALYZED));
                        break;
                    default:
                        break;
                }
            }
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

        private List<SearchResult> CreateSearchResult(IEnumerable<Document> documents, List<DocumentType> docTypes, int pageIndex, bool hasMoreResult)
        {
            List<SearchResult> results = new List<SearchResult>();
            foreach (Document lucDocument in documents)
            {
                var document = new Dom.Document();
                ExtractStaticFields(lucDocument, document);
                DocumentType docType = docTypes.First(p => p.Id == document.DocTypeId);
                ExtractDynamicFields(lucDocument, document, docType);

                SearchResult result = results.FirstOrDefault(p => p.DocumentType == docType);
                if (result == null)
                {
                    result = new SearchResult { DocumentType = docType, HasMoreResult = hasMoreResult, PageIndex = pageIndex };
                    results.Add(result);
                }

                result.Documents.Add(document);
            }

            return results;
        }

        private Dom.Document CreateDocument(Document lucDocument, DocumentType docType)
        {
            var document = new Dom.Document();
            ExtractStaticFields(lucDocument, document);
            ExtractDynamicFields(lucDocument, document, docType);
            return document;
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
                FieldMetaData metaField = docType.FieldMetaDatas.FirstOrDefault(p => p.Id.ToString() == fieldName);
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

        private static string GetContentFromPages(List<Page> pages)
        {
            string content = string.Empty;
            if (pages != null && pages.Count > 0)
            {
                foreach (Page page in pages)
                {
                    if (page.Content == null)
                    {
                        continue;
                    }

                    content += page.Content;
                }
            }

            return content;


        }
    }
}