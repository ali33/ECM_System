using Ecm.Mvvm;
using Ecm.Domain;
using System;

namespace Ecm.Model
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
        private FieldMetaDataModel _field;
        private string _additionalFieldId;

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
            }
        }

        public SearchOperator Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                OnPropertyChanged("Operator");
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

        public FieldMetaDataModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
                OnPropertyChanged("Field");
            }
        }

        
        public string FieldUniqueId { get; set; }
        
    }
}
