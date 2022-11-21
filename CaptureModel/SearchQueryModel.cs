using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

namespace Ecm.CaptureModel
{
    public class SearchQueryModel : BaseDependencyProperty
    {
        private string _name;
        private ObservableCollection<SearchQueryExpressionModel> _searchQueryExpressions;
        private BatchTypeModel _batchType;

        public SearchQueryModel()
        {
            DeletedExpressions = new List<Guid>();
            _searchQueryExpressions = new ObservableCollection<SearchQueryExpressionModel>();
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid BatchTypeId { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string SearchQueryString { get; set; }

        public ObservableCollection<SearchQueryExpressionModel> SearchQueryExpressions
        {
            get { return _searchQueryExpressions; }
            set
            {
                _searchQueryExpressions = value;
                OnPropertyChanged("SearchQueryExpressions");
            }
        }

        public BatchTypeModel BatchType
        {
            get { return _batchType; }
            set
            {
                _batchType = value;
                OnPropertyChanged("BatchType");
            }
        }

        public List<Guid> DeletedExpressions { get; private set; }
    }
}
