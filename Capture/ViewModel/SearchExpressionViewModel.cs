using System.Collections.Generic;
using System.Linq;
using Ecm.Mvvm;
using Ecm.CaptureModel;
using Ecm.CaptureDomain;
using System.Collections.ObjectModel;

namespace Ecm.Capture.ViewModel
{
    public class SearchExpressionViewModel : BaseDependencyProperty
    {
        private SearchQueryExpressionModel _searchQueryExpression;

        public SearchExpressionViewModel()
        {
            SearchConditions = new Dictionary<string, SearchConjunction> { { Common.AND, SearchConjunction.And }, { Common.OR, SearchConjunction.Or } };
            SearchOperators = new ObservableCollection<string>();
            AvailableOperands = new ObservableCollection<string>();
        }

        public Dictionary<string, SearchConjunction> SearchConditions { get; private set; }

        public ObservableCollection<string> SearchOperators { get; private set; }

        public ObservableCollection<string> AvailableOperands { get; private set; }

        public SearchQueryExpressionModel SearchQueryExpression
        {
            get { return _searchQueryExpression; }
            set
            {
                _searchQueryExpression = value;
                if (value != null && value.Field != null)
                {
                    GetSearchOperators(value.Field.DataType);
                    if (string.IsNullOrEmpty(_searchQueryExpression.OperatorText))
                    {
                        _searchQueryExpression.OperatorText = GetDefaultOperatorText(value.Field.DataType);
                    }

                    GetAvailableOperands(value.Field.DataType, value.Field);
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

        private string GetDefaultOperatorText(FieldDataType dataType)
        {
            if (dataType == FieldDataType.String || dataType == FieldDataType.Picklist)
            {
                return Common.CONTAINS;
            }

            return Common.EQUAL;
        }

        private void SearchQueryExpressionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Field" && IsAdditionCondition)
            {
                SearchOperators.Clear();
                if (SearchQueryExpression != null && SearchQueryExpression.Field != null)
                {
                    GetSearchOperators(SearchQueryExpression.Field.DataType);
                    SearchQueryExpression.Operator = GetDefaultOperator(SearchQueryExpression.Field.DataType);
                }
            }
        }

        private SearchOperator GetDefaultOperator(FieldDataType dataType)
        {
            if (dataType == FieldDataType.String || dataType == FieldDataType.Picklist)
            {
                return SearchOperator.Contains;
            }

            return SearchOperator.Equal;
        }

        private void GetAvailableOperands(FieldDataType dataType, FieldModel field)
        {
            if (dataType == FieldDataType.Boolean)
            {
                AvailableOperands.Clear();
                AvailableOperands.Add(string.Empty);
                AvailableOperands.Add("Yes");
                AvailableOperands.Add("No");
            }
            else if (dataType == FieldDataType.Picklist)
            {
                AvailableOperands.Clear();

                if (field.Picklists != null && field.Picklists.Count > 0)
                {
                    var picklistValues = field.Picklists.Select(p => p.Value).OrderBy(p => p).ToList();
                    picklistValues.ForEach(p => AvailableOperands.Add(p));
                }
            }
        }
    }
}
