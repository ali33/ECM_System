using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureModel
{
    public class SearchQueryExpressionModel : BaseDependencyProperty
    {
        private Guid _id;
        private Guid _searchQueryId;
        private SearchConjunction _condition;
        private SearchOperator _operator;
        private string _operatorText;
        private string _value1;
        private string _value2;
        private FieldModel _field;

        private DateTime? _dateValue1;
        private DateTime? _dateValue2;

        public DateTime? DateValue1
        {
            get { return _dateValue1; }
            set
            {
                _dateValue1 = value;
                OnPropertyChanged("DateValue1");
            }
        }

        public DateTime? DateValue2
        {
            get { return _dateValue2; }
            set
            {
                _dateValue2 = value;
                OnPropertyChanged("DateValue2");
            }
        }

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public Guid SearchQueryId
        {
            get { return _searchQueryId; }
            set
            {
                _searchQueryId = value;
                OnPropertyChanged("SearchQueryId");
            }
        }

        public SearchConjunction Condition
        {
            get { return _condition; }
            set
            {
                _condition = value;
                OnPropertyChanged("Condition");
            }
        }

        public string OperatorText
        {
            get
            {
                return _operatorText;
            }
            set
            {
                _operatorText = value;
                OnPropertyChanged("OperatorText");
                var tmpOperator = MapOperator(value);
                if (tmpOperator != _operator)
                {
                    Operator = tmpOperator;
                }
            }
        }

        public SearchOperator Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                var tmpOperatorText = MapOperator(value);
                if (tmpOperatorText != _operatorText)
                {
                    OperatorText = tmpOperatorText;
                }
            }
        }

        public string Value1
        {
            get { return _value1; }
            set
            {
                _value1 = (value + string.Empty).Trim();
                OnPropertyChanged("Value1");
            }
        }

        public string Value2
        {
            get { return _value2; }
            set
            {
                _value2 = (value + string.Empty).Trim();
                OnPropertyChanged("Value2");
            }
        }

        public FieldModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
                OnPropertyChanged("Field");
            }
        }

        private string MapOperator(SearchOperator op)
        {
            switch (op)
            {
                case SearchOperator.Equal:
                    return Common.EQUAL;
                case SearchOperator.GreaterThan:
                    return Common.GREATER_THAN;
                case SearchOperator.GreaterThanOrEqualTo:
                    return Common.GREATER_THAN_OR_EQUAL_TO;
                case SearchOperator.LessThan:
                    return Common.LESS_THAN;
                case SearchOperator.LessThanOrEqualTo:
                    return Common.LESS_THAN_OR_EQUAL_TO;
                case SearchOperator.InBetween:
                    return Common.IN_BETWEEN;
                case SearchOperator.Contains:
                    return Common.CONTAINS;
                case SearchOperator.NotContains:
                    return Common.NOT_CONTAINS;
                case SearchOperator.NotEqual:
                    return Common.NOT_EQUAL;
                case SearchOperator.StartsWith:
                    return Common.STARTS_WITH;
                case SearchOperator.EndsWith:
                    return Common.ENDS_WITH;
            }

            return Common.EQUAL;
        }

        private SearchOperator MapOperator(string op)
        {
            switch (op)
            {
                case Common.EQUAL:
                    return SearchOperator.Equal;
                case Common.GREATER_THAN:
                    return SearchOperator.GreaterThan;
                case Common.GREATER_THAN_OR_EQUAL_TO:
                    return SearchOperator.GreaterThanOrEqualTo;
                case Common.LESS_THAN:
                    return SearchOperator.LessThan;
                case Common.LESS_THAN_OR_EQUAL_TO:
                    return SearchOperator.LessThanOrEqualTo;
                case Common.IN_BETWEEN:
                    return SearchOperator.InBetween;
                case Common.CONTAINS:
                    return SearchOperator.Contains;
                case Common.NOT_CONTAINS:
                    return SearchOperator.NotContains;
                case Common.NOT_EQUAL:
                    return SearchOperator.NotEqual;
                case Common.STARTS_WITH:
                    return SearchOperator.StartsWith;
                case Common.ENDS_WITH:
                    return SearchOperator.EndsWith;
            }

            return SearchOperator.Equal;
        }
    }
}
