using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent for each criteria (expression) for the <see cref="SearchQuery"/> object.
    /// </summary>
    [DataContract]
    public class SearchQueryExpression
    {
        /// <summary>
        /// Identifier of the criteria
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="SearchQuery"/> object
        /// </summary>
        [DataMember]
        public Guid SearchQueryId { get; set; }

        /// <summary>
        /// Condition of the expression: None, And, Or
        /// </summary>
        [DataMember]
        public string Condition { get; set; }

        /// <summary>
        /// Identifier of the <see cref="FieldMetaData"/> object that is the left operand of the expression
        /// </summary>
        [DataMember]
        public Guid FieldId { get; set; }

        /// <summary>
        /// The operator of the criteria: Equal, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, InBetween, Contains, NotContains, NotEqual, StartsWith, EndsWith
        /// </summary>
        [DataMember]
        public string Operator { get; set; }

        /// <summary>
        /// This property is used internally
        /// </summary>
        public SearchOperator OperatorEnum
        {
            get
            {
                return (SearchOperator)Enum.Parse(typeof(SearchOperator), Operator);
            }
        }

        /// <summary>
        /// The right operand of the expression
        /// </summary>
        [DataMember]
        public string Value1 { get; set; }

        /// <summary>
        /// In case of <see cref="Operator"/> = InBetween, the <see cref="Value1"/> and <see cref="Value2"/> will be a start value and end value.
        /// </summary>
        [DataMember]
        public string Value2 { get; set; }

        /// <summary>
        /// The <see cref="FieldMetaData"/> object.
        /// </summary>
        [DataMember]
        public FieldMetaData FieldMetaData { get; set; }

        /// <summary>
        /// Identify this search condition apply to advansearch with additional condition
        /// </summary>
        [DataMember]
        public bool IsAdditionalField { get; set; }

        /// <summary>
        /// ID of additional field
        /// </summary>
        [DataMember]
        public string FieldUniqueId { get; set; }
    }
}