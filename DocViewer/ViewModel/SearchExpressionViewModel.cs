using Ecm.Domain;
using Ecm.Model;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Ecm.DocViewer.ViewModel
{
    public class SearchExpressionViewModel : BaseDependencyProperty
    {
        private SearchQueryExpressionModel _searchQueryExpression;
        private ResourceManager _resource = new ResourceManager("Ecm.DocViewer.Resources", Assembly.GetExecutingAssembly());

        private ObservableCollection<KeyValuePair<SearchOperator, string>> _searchOperators;

        public SearchExpressionViewModel()
        {
            SearchConditions = new Dictionary<string, SearchConjunction> { { Common.AND, SearchConjunction.And }, { Common.OR, SearchConjunction.Or }, { Common.NONE, SearchConjunction.None } };
            AvailableOperands = new ObservableCollection<string>();
        }

        public Dictionary<string, SearchConjunction> SearchConditions { get; private set; }

        public ObservableCollection<KeyValuePair<SearchOperator, string>> SearchOperators
        {
            get { return _searchOperators; }
            set
            {
                _searchOperators = value;
                OnPropertyChanged("SearchOperators");
            }
        }

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
            SearchOperators = new ObservableCollection<KeyValuePair<SearchOperator, string>>();
            switch (dataType)
            {
                case FieldDataType.String:
                case FieldDataType.Picklist:
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.Contains, _resource.GetString("CONTAINS")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.EndsWith, _resource.GetString("ENDS_WITH")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.Equal, _resource.GetString("EQUAL")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.NotContains, _resource.GetString("NOT_CONTAINS")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.NotEqual, _resource.GetString("NOT_EQUAL")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.StartsWith, _resource.GetString("STARTS_WITH")));
                    break;
                case FieldDataType.Date:
                case FieldDataType.Integer:
                case FieldDataType.Decimal:
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.Equal, _resource.GetString("EQUAL")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.InBetween, _resource.GetString("IN_BETWEEN")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.NotEqual, _resource.GetString("NOT_EQUAL")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.GreaterThan, "GREATER_THAN"));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.GreaterThanOrEqualTo, "GREATER_THAN_OR_EQUAL_TO"));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.LessThan, "LESS_THAN"));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.LessThanOrEqualTo, "LESS_THAN_OR_EQUAL_TO"));
                    break;
                case FieldDataType.Boolean:
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.Equal, _resource.GetString("EQUAL")));
                    SearchOperators.Add(new KeyValuePair<SearchOperator, string>(SearchOperator.NotEqual, _resource.GetString("NOT_EQUAL")));
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

        private void GetAvailableOperands(FieldDataType dataType, FieldMetaDataModel field)
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
