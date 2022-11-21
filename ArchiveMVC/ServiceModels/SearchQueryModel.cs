using Ecm.Mvvm;

using System.Collections.Generic;
using System;

namespace ArchiveMVC.Models
{
    public class SearchQueryModel
    {
        private string _name;
        private List<SearchQueryExpressionModel> _searchQueryExpressions;
        private DocumentTypeModel _documentType;

        public SearchQueryModel()
        {
            DeletedExpressions = new List<Guid>();
            _searchQueryExpressions = new List<SearchQueryExpressionModel>();
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid DocTypeId { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public List<SearchQueryExpressionModel> SearchQueryExpressions
        {
            get { return _searchQueryExpressions; }
            set
            {
                _searchQueryExpressions = value;
            }
        }

        public DocumentTypeModel DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
            }
        }

        public List<Guid> DeletedExpressions { get; private set; }
    }
}
