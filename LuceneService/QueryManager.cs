using System;
using System.Collections.Generic;
using Ecm.Domain;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;
using System.IO;
using Lucene.Net.Analysis.Tokenattributes;

namespace Ecm.LuceneService
{
    public class QueryManager
    {
        /// <summary>
        /// String: Contains, StartsWith, EndsWith, Equal, NotContains, NotEqual
        /// Integer: Equal, GreaterThan, GreaterThanOrEqualTo, InBetween, LessThan, LessThanOrEqualTo, NotEqual
        /// Decimal: Equal, GreaterThan, GreaterThanOrEqualTo, InBetween, LessThan, LessThanOrEqualTo, NotEqual
        /// Picklist: Contains, StartsWith, EndsWith, Equal, NotContains, NotEqual
        /// Boolean: Equal, NotEqual
        /// Date: Equal, GreaterThan, GreaterThanOrEqualTo, InBetween, LessThan, LessThanOrEqualTo, NotEqual
        /// </summary>
        public Query BuildAdvanceSearchQuery(DocumentType docType, SearchQuery searchQuery)
        {
            BooleanQuery query = new BooleanQuery();
            
            //Edit
            //query.Add(NumericRangeQuery.NewLongRange(IndexManager._fieldDocTypeId, docType.ID, docType.ID, true, true), BooleanClause.Occur.MUST);
            query.Add((Query)(new TermQuery(new Term(IndexManager._fieldDocTypeId, docType.Id.ToString()))), BooleanClause.Occur.MUST);           
            

            foreach(var expression in searchQuery.SearchQueryExpressions)
            {
                string fieldId = expression.FieldMetaData.Id.ToString();

                switch (expression.FieldMetaData.Name)
                {
                    case "Page count":
                        fieldId = IndexManager._fieldPageCount;
                        break;
                    case "Created by":
                        fieldId = IndexManager._fieldCreatedBy;
                        break;
                    case "Created on":
                        fieldId = IndexManager._fieldCreatedDate;
                        break;
                    case "Modified by":
                        fieldId = IndexManager._fieldModifiedBy;
                        break;
                    case "Modified on":
                        fieldId = IndexManager._fieldModifiedDate;
                        break;
                }

                BooleanClause.Occur occur = (string.IsNullOrEmpty(expression.Condition) || expression.Condition == "AND") ? BooleanClause.Occur.MUST : BooleanClause.Occur.SHOULD;
                Query subQuery = null;
                string indexValue1 = (expression.Value1 + string.Empty).ToLower();
                string indexValue2 = (expression.Value2 + string.Empty).ToLower();
                switch (expression.OperatorEnum)
                {
                    case SearchOperator.Contains:
                        subQuery = new WildcardQuery(new Term(fieldId, string.Format("*{0}*", indexValue1)));
                        break;
                    case SearchOperator.StartsWith:
                        subQuery = new PrefixQuery(new Term(fieldId, string.Format("{0}", indexValue1)));
                        break;
                    case SearchOperator.EndsWith:
                        subQuery = new WildcardQuery(new Term(fieldId, string.Format("*{0}", indexValue1)));
                       break;
                    case SearchOperator.Equal:
                        subQuery = GetEqualQuery(indexValue1, expression.FieldMetaData);
                        break;
                    case SearchOperator.GreaterThan:
                        subQuery = GetGreaterThanQuery(indexValue1, expression.FieldMetaData, false);
                        break;
                    case SearchOperator.GreaterThanOrEqualTo:
                        subQuery = GetGreaterThanQuery(indexValue1, expression.FieldMetaData, true);
                        break;
                    case SearchOperator.InBetween:
                        subQuery = GetBetweenQuery(indexValue1, indexValue2, expression.FieldMetaData);
                        break;
                    case SearchOperator.LessThan:
                        subQuery = GetLessThanQuery(indexValue1, expression.FieldMetaData, false);
                        break;
                    case SearchOperator.LessThanOrEqualTo:
                        subQuery = GetLessThanQuery(indexValue1, expression.FieldMetaData, true);
                        break;
                    case SearchOperator.NotContains:
                        subQuery = GetNotContainQuery(indexValue1, expression.FieldMetaData);
                        break;
                    case SearchOperator.NotEqual:
                        subQuery = GetNotEqualQuery(indexValue1, expression.FieldMetaData);
                        break;
                }

                query.Add(subQuery, occur);
            }

            return query;
        }

        public Query BuildContentSearchQuery(DocumentType docType, string text)
        {
            BooleanQuery query = new BooleanQuery();
            query.Add(NumericRangeQuery.NewLongRange(IndexManager._fieldDocTypeId, docType.Id, docType.Id, true, true), BooleanClause.Occur.MUST);
            BooleanClause.Occur occur = BooleanClause.Occur.MUST;
            WildcardQuery subQuery = new WildcardQuery(new Term(IndexManager._fieldContent, string.Format("*{0}*", text)));//GetPhraseQuery(new LowerCaseAnalyzer(), IndexManager._fieldContent, text);//
            query.Add(subQuery, BooleanClause.Occur.MUST);
            
            return query;
        }

        public Query BuildGlobalSearchQuery(List<DocumentType> docTypes, string keyword)
        {
            keyword = (keyword + string.Empty).ToLower();
            BooleanQuery query = GetGlobalSearchQueryForStaticFields(keyword);
            foreach(DocumentType docType in docTypes)
            {
                foreach (FieldMetaData field in docType.FieldMetaDatas)
                {
                    if (field.DataTypeEnum == FieldDataType.Integer)
                    {
                        long longValue;
                        if (long.TryParse(keyword, out longValue))
                        {
                            query.Add(NumericRangeQuery.NewLongRange(field.Id.ToString(), longValue, longValue, true, true), BooleanClause.Occur.SHOULD);
                        }
                    }
                    else if (field.DataTypeEnum == FieldDataType.Decimal)
                    {
                        double doubleValue;
                        if (double.TryParse(keyword, out doubleValue))
                        {
                            query.Add(NumericRangeQuery.NewDoubleRange(field.Id.ToString(), doubleValue, doubleValue, true, true), BooleanClause.Occur.SHOULD);
                        }
                    }
                    else if (field.DataTypeEnum == FieldDataType.Date)
                    {
                        DateTime dateValue;
                        if (DateTime.TryParse(keyword, out dateValue))
                        {
                            query.Add(new TermQuery(new Term(field.Id.ToString(), dateValue.ToString(IndexManager._dateTimeValueFormat))), BooleanClause.Occur.SHOULD);
                        }
                    }
                    else
                    {
                        query.Add(new WildcardQuery(new Term(field.Id.ToString(), string.Format("*{0}*", keyword))), BooleanClause.Occur.SHOULD);
                    }
                }
            }

            return query;
        }

        private Query GetEqualQuery(string fieldValue, FieldMetaData field, Analyzer standard)
        {
            Query subQuery;
            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value = GetLong(fieldValue);
                subQuery = NumericRangeQuery.NewLongRange(field.Id.ToString(), value, value, true, true);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value = GetDouble(fieldValue);
                subQuery = NumericRangeQuery.NewDoubleRange(field.Id.ToString(), value, value, true, true);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {
                DateTime value = GetDateTime(fieldValue);
                subQuery = new TermQuery(new Term(field.Id.ToString(), value.ToString(IndexManager._dateTimeValueFormat)));
            }
            else
            {
                subQuery = new TermQuery(new Term(field.Id.ToString(), fieldValue));//GetPhraseQuery(standard, field.FieldUniqueId, fieldValue);//
            }

            return subQuery;
        }

        private Query GetEqualQuery(string fieldValue, FieldMetaData field)
        {
            Query subQuery;
            string fieldId = field.Id.ToString();

            switch (field.Name)
            {
                case "Page count":
                    fieldId = IndexManager._fieldPageCount;
                    break;
                case "Created by":
                    fieldId = IndexManager._fieldCreatedBy;
                    break;
                case "Created on":
                    fieldId = IndexManager._fieldCreatedDate;
                    break;
                case "Modified by":
                    fieldId = IndexManager._fieldModifiedBy;
                    break;
                case "Modified on":
                    fieldId = IndexManager._fieldModifiedDate;
                    break;
            }


            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value = GetLong(fieldValue);
                subQuery = NumericRangeQuery.NewLongRange(fieldId, value, value, true, true);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value = GetDouble(fieldValue);
                subQuery = NumericRangeQuery.NewDoubleRange(fieldId, value, value, true, true);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {

                var stringValue1 = Convert.ToDateTime(fieldValue + " 00:00:00").ToString(IndexManager._dateTimeValueFormat);
                var stringValue2 = Convert.ToDateTime(fieldValue + " 23:59:59").ToString(IndexManager._dateTimeValueFormat);

                subQuery = new TermRangeQuery(fieldId, stringValue1, stringValue2, true, true);
            }
            else
            {
                subQuery = new TermQuery(new Term(fieldId, fieldValue));
            }

            return subQuery;
        }

        private Query GetGreaterThanQuery(string fieldValue, FieldMetaData field, bool hasEqual)
        {
            Query subQuery;
            string fieldId = field.Id.ToString();

            switch (field.Name)
            {
                case "Page count":
                    fieldId = IndexManager._fieldPageCount;
                    break;
                case "Created by":
                    fieldId = IndexManager._fieldCreatedBy;
                    break;
                case "Created on":
                    fieldId = IndexManager._fieldCreatedDate;
                    break;
                case "Modified by":
                    fieldId = IndexManager._fieldModifiedBy;
                    break;
                case "Modified on":
                    fieldId = IndexManager._fieldModifiedDate;
                    break;
            }

            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value = GetLong(fieldValue);
                subQuery = NumericRangeQuery.NewLongRange(fieldId, value, long.MaxValue, hasEqual, false);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value = GetDouble(fieldValue);
                subQuery = NumericRangeQuery.NewDoubleRange(fieldId, value, double.MaxValue, hasEqual, false);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {
                var hour = " 00:00:00";

                if (!hasEqual)
                {
                    hour = " 23:59:59";
                }

                string stringValue = Convert.ToDateTime(fieldValue + hour).ToString(IndexManager._dateTimeValueFormat);

                subQuery = new TermRangeQuery(fieldId, stringValue, 
                                              DateTime.MaxValue.ToString(IndexManager._dateTimeValueFormat), hasEqual, false);
            }
            else
            {
                string oper = hasEqual ? "Greater than or equal to" : "Greater than";
                throw new NotSupportedException("Operator '" + oper + "' is not supported on field type: " + field.DataTypeEnum);
            }

            return subQuery;
        }

        private Query GetLessThanQuery(string fieldValue, FieldMetaData field, bool hasEqual)
        {
            Query subQuery;
            string fieldId = field.Id.ToString();

            switch (field.Name)
            {
                case "Page count":
                    fieldId = IndexManager._fieldPageCount;
                    break;
                case "Created by":
                    fieldId = IndexManager._fieldCreatedBy;
                    break;
                case "Created on":
                    fieldId = IndexManager._fieldCreatedDate;
                    break;
                case "Modified by":
                    fieldId = IndexManager._fieldModifiedBy;
                    break;
                case "Modified on":
                    fieldId = IndexManager._fieldModifiedDate;
                    break;
            }

            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value = GetLong(fieldValue);
                subQuery = NumericRangeQuery.NewLongRange(fieldId, long.MinValue, value, false, hasEqual);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value = GetDouble(fieldValue);
                subQuery = NumericRangeQuery.NewDoubleRange(fieldId, long.MinValue, value, false, hasEqual);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {
                var hour = " 00:00:00";

                if (hasEqual)
                {
                    hour = " 23:59:59";
                }

                string stringValue = Convert.ToDateTime(fieldValue + hour).ToString(IndexManager._dateTimeValueFormat);

                subQuery = new TermRangeQuery(fieldId, DateTime.MinValue.ToString(IndexManager._dateTimeValueFormat),
                                              stringValue, false, hasEqual);
            }
            else
            {
                string oper = hasEqual ? "Less than or equal to" : "Less than";
                throw new NotSupportedException("Operator " + oper + " is not supported on field type: " + field.DataTypeEnum);
            }

            return subQuery;
        }

        private Query GetBetweenQuery(string fieldValue, string indexValue2, FieldMetaData field)
        {
            Query subQuery;
            string fieldId = field.Id.ToString();

            switch (field.Name)
            {
                case "Page count":
                    fieldId = IndexManager._fieldPageCount;
                    break;
                case "Created by":
                    fieldId = IndexManager._fieldCreatedBy;
                    break;
                case "Created on":
                    fieldId = IndexManager._fieldCreatedDate;
                    break;
                case "Modified by":
                    fieldId = IndexManager._fieldModifiedBy;
                    break;
                case "Modified on":
                    fieldId = IndexManager._fieldModifiedDate;
                    break;
            }

            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value1 = GetLong(fieldValue);
                long value2 = GetLong(indexValue2);
                subQuery = NumericRangeQuery.NewLongRange(fieldId, value1, value2, true, true);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value1 = GetDouble(fieldValue);
                double value2 = GetDouble(indexValue2);
                subQuery = NumericRangeQuery.NewDoubleRange(fieldId, value1, value2, true, true);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {
                var stringValue1 = Convert.ToDateTime(fieldValue + " 00:00:00").ToString(IndexManager._dateTimeValueFormat);
                var stringValue2 =  Convert.ToDateTime(indexValue2 + " 23:59:59").ToString(IndexManager._dateTimeValueFormat);

                subQuery = new TermRangeQuery(fieldId, stringValue1, stringValue2, true, true);
            }
            else
            {
                throw new NotSupportedException("Operator 'Between' is not supported on field type: " + field.DataTypeEnum);
            }

            return subQuery;
        }

        private Query GetNotContainQuery(string fieldValue, FieldMetaData field, Analyzer standard)
        {
            if (field.DataTypeEnum == FieldDataType.String || field.DataTypeEnum == FieldDataType.Picklist)
            {
                var containsQuery = GetPhraseQuery(standard, field.Id.ToString(),fieldValue);//new WildcardQuery(new Term(field.FieldUniqueId, string.Format("*{0}*", fieldValue)));
                var notContainsQuery = new BooleanQuery();
                notContainsQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);
                notContainsQuery.Add(containsQuery, BooleanClause.Occur.MUST_NOT);
                return notContainsQuery;
            }

            throw new NotSupportedException("Operator 'Not contains' is not supported on field type: " + field.DataTypeEnum);
        }

        private Query GetNotContainQuery(string fieldValue, string field, Analyzer standard)
        {
            var containsQuery = GetPhraseQuery(standard, field, fieldValue);//new WildcardQuery(new Term(field, string.Format("*{0}*", fieldValue)));
            var notContainsQuery = new BooleanQuery();
            notContainsQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);
            notContainsQuery.Add(containsQuery, BooleanClause.Occur.MUST_NOT);
            return notContainsQuery;
        }

        private Query GetNotEqualQuery(string fieldValue, FieldMetaData field, Analyzer standard)
        {
            BooleanQuery subQuery = new BooleanQuery();
            subQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);
            string fieldId = field.Id.ToString();

            switch (field.Name)
            {
                case "Page count":
                    fieldId = IndexManager._fieldPageCount;
                    break;
                case "Created by":
                    fieldId = IndexManager._fieldCreatedBy;
                    break;
                case "Created on":
                    fieldId = IndexManager._fieldCreatedDate;
                    break;
                case "Modified by":
                    fieldId = IndexManager._fieldModifiedBy;
                    break;
                case "Modified on":
                    fieldId = IndexManager._fieldModifiedDate;
                    break;
            }


            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value = GetLong(fieldValue);
                subQuery.Add(NumericRangeQuery.NewLongRange(fieldId, value, value, true, true), BooleanClause.Occur.MUST_NOT);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value = GetDouble(fieldValue);
                subQuery.Add(NumericRangeQuery.NewDoubleRange(fieldId, value, value, true, true), BooleanClause.Occur.MUST_NOT);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {
                DateTime value = GetDateTime(fieldValue);
                subQuery.Add(new TermQuery(new Term(fieldId, value.ToString(IndexManager._dateTimeValueFormat))), BooleanClause.Occur.MUST_NOT);
            }
            else
            {
                //subQuery.Add(new WildcardQuery(new Term(field.FieldUniqueId, string.Format("*{0}*", fieldValue))), BooleanClause.Occur.MUST_NOT);
                subQuery.Add(GetPhraseQuery(standard, fieldId, fieldValue), BooleanClause.Occur.MUST_NOT);
            }

            return subQuery;
        }

        private Query GetNotEqualQuery(string fieldValue, string field, Analyzer standard)
        {
            BooleanQuery subQuery = new BooleanQuery();
            subQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);

            subQuery.Add(GetPhraseQuery(standard, field, fieldValue), BooleanClause.Occur.MUST_NOT);

            return subQuery;
        }
        
        private Query GetNotContainQuery(string fieldValue, FieldMetaData field)
        {
            if (field.DataTypeEnum == FieldDataType.String || field.DataTypeEnum == FieldDataType.Picklist)
            {
                var containsQuery = new WildcardQuery(new Term(field.Id.ToString(), string.Format("*{0}*", fieldValue)));
                var notContainsQuery = new BooleanQuery();
                notContainsQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);
                notContainsQuery.Add(containsQuery, BooleanClause.Occur.MUST_NOT);
                return notContainsQuery;
            }

            throw new NotSupportedException("Operator 'Not contains' is not supported on field type: " + field.DataTypeEnum);
        }

        private Query GetNotContainQuery(string fieldValue, string field)
        {
            var containsQuery = new WildcardQuery(new Term(field, string.Format("*{0}*", fieldValue)));
            var notContainsQuery = new BooleanQuery();
            notContainsQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);
            notContainsQuery.Add(containsQuery, BooleanClause.Occur.MUST_NOT);
            return notContainsQuery;
        }

        private Query GetNotEqualQuery(string fieldValue, FieldMetaData field)
        {
            BooleanQuery subQuery = new BooleanQuery();
            subQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);
            string fieldId = field.Id.ToString();

            switch (field.Name)
            {
                case "Page count":
                    fieldId = IndexManager._fieldPageCount;
                    break;
                case "Created by":
                    fieldId = IndexManager._fieldCreatedBy;
                    break;
                case "Created on":
                    fieldId = IndexManager._fieldCreatedDate;
                    break;
                case "Modified by":
                    fieldId = IndexManager._fieldModifiedBy;
                    break;
                case "Modified on":
                    fieldId = IndexManager._fieldModifiedDate;
                    break;
            }
            if (field.DataTypeEnum == FieldDataType.Integer)
            {
                long value = GetLong(fieldValue);
                subQuery.Add(NumericRangeQuery.NewLongRange(fieldId, value, value, true, true), BooleanClause.Occur.MUST_NOT);
            }
            else if (field.DataTypeEnum == FieldDataType.Decimal)
            {
                double value = GetDouble(fieldValue);
                subQuery.Add(NumericRangeQuery.NewDoubleRange(fieldId, value, value, true, true), BooleanClause.Occur.MUST_NOT);
            }
            else if (field.DataTypeEnum == FieldDataType.Date)
            {
                DateTime value = GetDateTime(fieldValue);
                subQuery.Add(new TermQuery(new Term(fieldId, value.ToString(IndexManager._dateTimeValueFormat))), BooleanClause.Occur.MUST_NOT);
            }
            else
            {
                subQuery.Add(new WildcardQuery(new Term(fieldId, string.Format("*{0}*", fieldValue))), BooleanClause.Occur.MUST_NOT);
            }

            return subQuery;
        }

        private Query GetNotEqualQuery(string fieldValue, string field)
        {
            BooleanQuery subQuery = new BooleanQuery();
            subQuery.Add(new MatchAllDocsQuery(), BooleanClause.Occur.MUST);

            subQuery.Add(new TermQuery(new Term(IndexManager._fieldContent, fieldValue)), BooleanClause.Occur.MUST_NOT);

            return subQuery;
        }

        private PhraseQuery GetPhraseQuery(Analyzer standard, string field, string value)
        {
            PhraseQuery phraseQuery = new PhraseQuery();
            phraseQuery.SetSlop(12);
            phraseQuery.SetBoost(5);

            TokenStream tokens = standard.TokenStream(field, new StringReader(value));
            List<Term> terms = new List<Term>();
            while (tokens.IncrementToken())
            {
                TermAttribute termAttribute = (TermAttribute)tokens.GetAttribute(typeof(TermAttribute));
                phraseQuery.Add(new Term(field, termAttribute.Term()));
            }

            return phraseQuery;
        }

        private BooleanQuery GetGlobalSearchQueryForStaticFields(string keyword)
        {
            BooleanQuery query = new BooleanQuery();
            query.Add(new WildcardQuery(new Term(IndexManager._fieldCreatedBy, string.Format("*{0}*", keyword))), BooleanClause.Occur.SHOULD);
            query.Add(new WildcardQuery(new Term(IndexManager._fieldModifiedBy, string.Format("*{0}*", keyword))), BooleanClause.Occur.SHOULD);
            
            DateTime dateValue;
            if (DateTime.TryParse(keyword, out dateValue))
            {
                query.Add(new TermQuery(new Term(IndexManager._fieldCreatedDate, dateValue.ToString(IndexManager._dateTimeValueFormat))), BooleanClause.Occur.SHOULD);
                query.Add(new TermQuery(new Term(IndexManager._fieldModifiedDate, dateValue.ToString(IndexManager._dateTimeValueFormat))), BooleanClause.Occur.SHOULD);
            }

            long numericValue;
            if (long.TryParse(keyword, out numericValue))
            {
                query.Add(NumericRangeQuery.NewLongRange(IndexManager._fieldPageCount, numericValue, numericValue, true, true), BooleanClause.Occur.SHOULD);
                query.Add(NumericRangeQuery.NewLongRange(IndexManager._fieldVersion, numericValue, numericValue, true, true), BooleanClause.Occur.SHOULD);
            }

            return query;
        }

        private long GetLong(string fieldValue)
        {
            long value;
            if (long.TryParse(fieldValue, out value))
            {
                return value;
            }

            throw new ArgumentException("The value in query is not numeric value.");
        }

        private double GetDouble(string fieldValue)
        {
            double value;
            if (double.TryParse(fieldValue, out value))
            {
                return value;
            }

            throw new ArgumentException("The value in query is not numeric value.");
        }

        private DateTime GetDateTime(string fieldValue)
        {
            DateTime value;
            if (DateTime.TryParse(fieldValue, out value))
            {
                return value;
            }

            throw new ArgumentException("The value in query is not date value.");
        }

    }
}