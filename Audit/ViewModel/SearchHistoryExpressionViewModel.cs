using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using Ecm.Domain;
using System.Collections.ObjectModel;

namespace Ecm.Audit.ViewModel
{
    public class SearchHistoryExpressionViewModel : BaseDependencyProperty
    {
        private SearchQueryExpressionModel _searchQueryExpression;

        public SearchHistoryExpressionViewModel()
        {
            SearchConditions = new Dictionary<string, SearchConjunction> { { Common.AND, SearchConjunction.And }, { Common.OR, SearchConjunction.Or } };
            SearchOperators = new ObservableCollection<string>();
        }

        public Dictionary<string, SearchConjunction> SearchConditions { get; private set; }

        public ObservableCollection<string> SearchOperators { get; private set; }

        public SearchQueryExpressionModel SearchQueryExpression
        {
            get { return _searchQueryExpression; }
            set
            {
                _searchQueryExpression = value;
                if (value != null && value.Field != null && value.Field.DataType != FieldDataType.Table)
                {
                    GetSearchOperators(value.Field.DataType);
                    if (string.IsNullOrEmpty(_searchQueryExpression.OperatorText))
                    {
                        _searchQueryExpression.OperatorText = SearchOperators[0];
                    }
                }

                if (value != null)
                {
                    value.PropertyChanged += SearchQueryExpressionPropertyChanged;
                }
            }
        }

        public bool IsAdditionCondition { get; set; }

        private void GetSearchOperators(FieldDataType dataType)
        {
            SearchOperators.Clear();
            switch (dataType)
            {
                case FieldDataType.String:
                case FieldDataType.Picklist:
                    SearchOperators.Add(Common.CONTAINS);
                    SearchOperators.Add(Common.ENDS_WITH);
                    SearchOperators.Add(Common.EQUAL);
                    SearchOperators.Add(Common.NOT_CONTAINS);
                    SearchOperators.Add(Common.NOT_EQUAL);
                    SearchOperators.Add(Common.STARTS_WITH);
                    break;
                case FieldDataType.Date:
                case FieldDataType.Integer:
                case FieldDataType.Decimal:
                    SearchOperators.Add(Common.EQUAL);
                    SearchOperators.Add(Common.GREATER_THAN);
                    SearchOperators.Add(Common.GREATER_THAN_OR_EQUAL_TO);
                    SearchOperators.Add(Common.IN_BETWEEN);
                    SearchOperators.Add(Common.LESS_THAN);
                    SearchOperators.Add(Common.LESS_THAN_OR_EQUAL_TO);
                    SearchOperators.Add(Common.NOT_EQUAL);
                    break;
                case FieldDataType.Boolean:
                    SearchOperators.Add(Common.EQUAL);
                    SearchOperators.Add(Common.NOT_EQUAL);
                    break;
                case FieldDataType.Table:
                    break;
            }
        }

        private void SearchQueryExpressionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Field" && IsAdditionCondition)
            {
                SearchOperators.Clear();
                if (SearchQueryExpression != null && SearchQueryExpression.Field != null)
                {
                    GetSearchOperators(SearchQueryExpression.Field.DataType);
                }

                if (SearchQueryExpression != null)
                {
                    SearchQueryExpression.Operator = SearchOperator.Equal;
                }
            }
        }    }
}
