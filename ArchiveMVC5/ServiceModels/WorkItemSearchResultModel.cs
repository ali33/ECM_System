using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace ArchiveMVC5.Models
{
    public class WorkItemSearchResultModel
    {
        private bool _isSelected;
        private DataTable _dataResult;
        private bool _internalChanged;
        private DocumentTypeModel _documentType;
        private bool _hasMoreResult;
        private string _documentTypeName;
        private string _resultCount;

        public string DocumentTypeName
        {
            get { return _documentTypeName; }
            set
            {
                _documentTypeName = value;
            }
        }

        public string ResultCount
        {
            get { return _resultCount; }
            set
            {
                _resultCount = value;
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
