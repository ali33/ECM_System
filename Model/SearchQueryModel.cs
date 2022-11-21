using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

namespace Ecm.Model
{
    public class SearchQueryModel : BaseDependencyProperty
    {
        private string _name;
        private ObservableCollection<SearchQueryExpressionModel> _searchQueryExpressions;
        private DocumentTypeModel _documentType;

        public SearchQueryModel()
        {
            DeletedExpressions = new List<Guid>();
            _searchQueryExpressions = new ObservableCollection<SearchQueryExpressionModel>();
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
                OnPropertyChanged("Name");
            }
        }

        public ObservableCollection<SearchQueryExpressionModel> SearchQueryExpressions
        {
            get { return _searchQueryExpressions; }
            set
            {
                _searchQueryExpressions = value;
                OnPropertyChanged("SearchQueryExpressions");
            }
        }

        public DocumentTypeModel DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                OnPropertyChanged("DocumentType");
            }
        }

        public List<Guid> DeletedExpressions { get; private set; }
    }
}
