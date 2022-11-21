using Ecm.Mvvm;
using Ecm.Domain;
using System;

namespace ArchiveMVC.Models
{
    public class SearchQueryExpressionModel
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
            }
        }

        public Guid SearchQueryId
        {
            get { return _searchQueryId; }
            set
            {
                _searchQueryId = value;
            }
        }

        public SearchConjunction Condition
        {
            get { return _condition; }
            set
            {
                _condition = value;
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
            }
        }

        public SearchOperator Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
            }
        }

        public string Value1
        {
            get { return _value1; }
            set
            {
                _value1 = (value + string.Empty).Trim();
            }
        }

        public string Value2
        {
            get { return _value2; }
            set
            {
                _value2 = (value + string.Empty).Trim();
            }
        }

        public FieldMetaDataModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
            }
        }

        public string FieldUniqueId { get; set; }
    }
}
