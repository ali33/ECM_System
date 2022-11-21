using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Data;

namespace Ecm.CaptureModel
{
    public class WorkItemSearchResultModel : BaseDependencyProperty
    {
        private bool _isSelected;
        private DataTable _dataResult;
        private bool _internalChanged;
        private BatchTypeModel _batchType;
        private bool _hasMoreResult;
        private string _batchTypeName;
        private string _resultCount;

        public string BatchTypeName
        {
            get { return _batchTypeName; }
            set
            {
                _batchTypeName = value;
                OnPropertyChanged("BatchTypeName");
            }
        }

        public string ResultCount
        {
            get { return _resultCount; }
            set
            {
                _resultCount = value;
                OnPropertyChanged("ResultCount");
            }
        }

        public BatchTypeModel BatchType
        {
            get { return _batchType; }
            set
            {
                _batchType = value;
                if (value != null)
                {
                    BatchTypeName = _batchType.Name;
                }
            }
        }

        public DataTable DataResult
        {
            get { return _dataResult; }
            set
            {
                _dataResult = value;
                OnPropertyChanged("DataResult");
                _dataResult.RowChanged += SearchResultRowChanged;
                _dataResult.RowDeleted += SearchResultRowDeleted;
                _dataResult.TableNewRow += SearchResultRowAdded;
                UpdateResultCount();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");

                if (!_internalChanged)
                {
                    _internalChanged = true;
                    foreach (DataRow row in DataResult.Rows)
                    {
                        row[Common.COLUMN_SELECTED] = value;
                    }

                    _internalChanged = false;
                }
            }
        }

        public int PageIndex { get; set; }

        public bool HasMoreResult
        {
            get { return _hasMoreResult; }
            set
            {
                _hasMoreResult = value;
                OnPropertyChanged("HasMoreResult");
            }
        }

        public SearchQueryModel SearchQuery { get; set; }

        private void SearchResultRowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Change && !_internalChanged)
            {
                e.Row.AcceptChanges();
                RefreshChanges();
            }
            else if (e.Action == DataRowAction.Add)
            {
                UpdateResultCount();
            }
        }

        private void SearchResultRowDeleted(object sender, DataRowChangeEventArgs e)
        {
            DataResult.AcceptChanges();
            RefreshChanges();
            UpdateResultCount();
        }

        private void SearchResultRowAdded(object sender, DataTableNewRowEventArgs e)
        {
            UpdateResultCount();
        }

        private void RefreshChanges()
        {
            _internalChanged = true;

            List<DataRow> rows = DataResult.Rows.OfType<DataRow>().ToList();
            IsSelected = rows.Count > 0 && rows.All(p => (bool)p[Common.COLUMN_SELECTED]);

            _internalChanged = false;
        }

        private void UpdateResultCount()
        {
            if (DataResult != null)
            {
                ResultCount = string.Format("{0} work item(s)", DataResult.Rows.Count);
            }
        }
    }
}
