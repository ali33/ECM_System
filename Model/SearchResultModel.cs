using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;

namespace Ecm.Model
{
    public class SearchResultModel : BaseDependencyProperty
    {
        private bool _isSelected;
        private bool _isChecked;
        private DataTable _dataResult;
        private bool _internalChanged;
        private DocumentTypeModel _documentType;
        private bool _hasMoreResult;
        private string _documentTypeName;
        private int _resultCount;
        private int _totalCount;
        private bool _isGlobalSearch;

        public string DocumentTypeName
        {
            get { return _documentTypeName; }
            set
            {
                _documentTypeName = value;
                OnPropertyChanged("DocumentTypeName");
            }
        }

        public int ResultCount
        {
            get { return _resultCount; }
            set
            {
                _resultCount = value;
                OnPropertyChanged("ResultCount");
            }
        }

        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
                OnPropertyChanged("TotalCount");
            }
        }

        public DocumentTypeModel DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                if (value != null)
                {
                    DocumentTypeName = _documentType.Name;
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

                //if (!_internalChanged)
                //{
                //    _internalChanged = true;
                //    foreach (DataRow row in DataResult.Rows)
                //    {
                //        row[Common.COLUMN_SELECTED] = value;
                //    }

                //    _internalChanged = false;
                //}
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");

                if (!_internalChanged)
                {
                    _internalChanged = true;
                    foreach (DataRow row in DataResult.Rows)
                    {
                        row[Common.COLUMN_CHECKED] = value;
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

        public string GlobalSearchText { get; set; }

        public bool IsGlobalSearch
        {
            get { return _isGlobalSearch; }
            set
            {
                _isGlobalSearch = value;
                OnPropertyChanged("IsGlobalSearch");
            }
        }

        public void LoadMoreResult()
        {
            if (HasMoreResult)
            {
                if (!IsGlobalSearch)
                {
                    SearchResultModel searchResult = new SearchProvider().RunAdvanceSearch(++PageIndex, DocumentType.Id,
                                                                                           SearchQuery);
                    if (searchResult != null)
                    {
                        HasMoreResult = searchResult.HasMoreResult;
                        foreach (DataRow row in searchResult.DataResult.Rows)
                        {
                            DataResult.ImportRow(row);
                        }
                    }
                }
            }
        }

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
                ResultCount = DataResult.Rows.Count;
            }
            else
            {
                ResultCount = 0;
            }
        }
    }
}
