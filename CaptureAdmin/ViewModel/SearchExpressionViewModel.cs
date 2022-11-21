using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.CaptureModel;
using System.Collections.ObjectModel;
using Ecm.CaptureAdmin.Model;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class SearchExpressionViewModel : BaseDependencyProperty
    {
        private SearchModel _searchModel;
        
        public SearchExpressionViewModel()
        {
            SearchConditions = new ObservableCollection<string> { { Common.AND}, { Common.OR} };
            SearchOperators = new ObservableCollection<string>();
        }

        public SearchModel Search
        {
            get { return _searchModel; }
            set
            {
                _searchModel = value;
                OnPropertyChanged("Search");
                if (value != null)
                {
                    GetSearchOperators(value.DataType);
                    _searchModel.Operator = SearchOperators[0];
                }
            }
        }

        public ObservableCollection<string> SearchOperators { get; set; }
        public ObservableCollection<string> SearchConditions { get; set; }


        private void GetSearchOperators(string dataType)
        {
            SearchOperators.Clear();
            switch (dataType)
            {
                case "String":
                    SearchOperators.Add(Common.CONTAINS);
                    SearchOperators.Add(Common.ENDS_WITH);
                    SearchOperators.Add(Common.EQUAL);
                    SearchOperators.Add(Common.NOT_CONTAINS);
                    SearchOperators.Add(Common.NOT_EQUAL);
                    SearchOperators.Add(Common.STARTS_WITH);
                    break;
                case "Date":
                    SearchOperators.Add(Common.EQUAL);
                    SearchOperators.Add(Common.GREATER_THAN);
                    SearchOperators.Add(Common.GREATER_THAN_OR_EQUAL_TO);
                    SearchOperators.Add(Common.IN_BETWEEN);
                    SearchOperators.Add(Common.LESS_THAN);
                    SearchOperators.Add(Common.LESS_THAN_OR_EQUAL_TO);
                    SearchOperators.Add(Common.NOT_EQUAL);
                    break;
            }
        }

    }
}
