using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Data;

namespace CaptureMVC.Models
{
    public class ConditionModel
    {
        public Guid Id { get; set; }
        public SearchConjunction Conjunction { get; set; }
        public Guid FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldDisplay { get; set; }
        public FieldDataType FieldDataType { get; set; }
        public SearchOperator SearchOperator { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string ControlInputId { get; set; }
        public bool IsSystemField { get; set; }
    }

    public class SavedQueryModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class AssignedMenuItemModel
    {
        public BatchStatus Status { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }

    public class AssignedMenuModel
    {
        public Guid BatchTypeId { get; set; }
        public string BatchTypeName { get; set; }
        public List<AssignedMenuItemModel> Items { get; set; }
        public string BatchTypeIcon { get; set; }
        public bool IsExpand { get; set; }

        public AssignedMenuModel()
        {
            Items = new List<AssignedMenuItemModel>();
        }
    }

    public class SearchResultModel
    {
        public Guid BatchTypeId { get; set; }
        public string BatchTypeName { get; set; }
        public DataTable DataResult { get; set; }
        public int TotalCount { get; set; }
        public int ResultCount { get; set; }
        public bool HasMoreResult { get; set; }
        public int PageIndex { get; set; }
        public string JsonSearchExpressions { get; set; }
        public bool SearchByAdvance { get; set; }
        public BatchStatus BatchStatus { get; set; }
        public int IndexActivityName { get; set; }
    }
}
