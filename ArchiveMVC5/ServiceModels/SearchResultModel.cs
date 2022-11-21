using System.Collections.Generic;
using System.Data;
using System.Linq;

using ArchiveMVC5.Models.DataProvider;

namespace ArchiveMVC5.Models
{
    public class SearchResultModel
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
            }
        }

        public int ResultCount
        {
            get { return _resultCount; }
            set
            {
                _resultCount = value;
            }
        }

        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
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
            }
        }

        public PagingModel Paging { get; set; }
    }
}
