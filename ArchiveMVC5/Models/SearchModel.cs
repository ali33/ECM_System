using Ecm.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Models
{
    public class SearchModel
    {
        
        public SearchModel()
        {
            listDocumentTypeModel = new Dictionary<string, DocumentTypeModel>();
        }
        public Dictionary<string, DocumentTypeModel> listDocumentTypeModel = new Dictionary<string, DocumentTypeModel>();
        public string DataTypeJson { get; set; }
        public string DataTypeItemJson { get; set; }
        public string DocTypeJson { get; set; }
        public string ConjunctionJson { get; set; }
        public Guid DocTypeIDFirst { get; set; }
       // public IList<SearchQueryModel> QueryModel { get; set; }
        public string QueryNameFirst { get; set; }// Lay cac query name cua DocType dau tien , add by TrietHo
        public string ExpressionsFirst { get; set; }

        
        public IList<DocumentTypeModel> ListDocType { get; set; }
        public IList<SearchQueryModel> ListQueryModel { get; set; }
        public Guid DocumentTypeId { get; set; }
        public string CacheDocType { get; set; }

        //FieldID trong khung chọn Search
        public Guid FieldID { get; set; }

        public SearchOperator SearchOperator { get; set; }
        public string ValueControlId { get; set; }
        public Guid QueryId { get; set; }
        public string  ContentSearch { get; set; }
        
        
    }
}