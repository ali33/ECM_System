using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Input;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.Mvvm;

namespace Ecm.Capture.ViewModel
{
    public class WorkItemListViewModel : ComponentViewModel
    {
        #region Private members

        private WorkItemSearchResultModel _searchResult;
        private bool _enableOpen;
        private bool _enableApprove;
        private bool _enableUnLock;
        private bool _enableResume;
        private bool _enableEmailAsLink;
        private bool _enableReject;
        private bool _enableDelete;
        private bool _enableDelegate;
        private bool _hasPermission;
        private readonly Action<Guid> _openWorkItem;

        #endregion

        #region Public properties

        public WorkItemSearchResultModel SearchResult
        {
            get { return _searchResult; }
            set
            {
                _searchResult = value;
                OnPropertyChanged("SearchResult");

                ResetContextMenuPermission();

                if (value != null)
                {
                    _searchResult.PropertyChanged += SearchResultPropertyChanged;
                }
            }
        }

        public bool EnableOpen
        {
            get
            {
                return _enableOpen;
            }
            set
            {
                _enableOpen = value;
                OnPropertyChanged("EnableOpen");
            }
        }

        public bool EnableApprove
        {
            get
            {
                return _enableApprove;
            }
            set
            {
                _enableApprove = value;
                OnPropertyChanged("EnableApprove");
            }
        }

        public bool EnableUnLock
        {
            get
            {
                return _enableUnLock;
            }
            set
            {
                _enableUnLock = value;
                OnPropertyChanged("EnableUnLock");
            }
        }

        public bool EnableResume
        {
            get
            {
                return _enableResume;
            }
            set
            {
                _enableResume = value;
                OnPropertyChanged("EnableResume");
            }
        }

        public bool EnableEmailAsLink
        {
            get
            {
                return _enableEmailAsLink;
            }
            set
            {
                _enableEmailAsLink = value;
                OnPropertyChanged("EnableEmailAsLink");
            }
        }

        public bool EnableReject
        {
            get
            {
                return _enableReject;
            }
            set
            {
                _enableReject = value;
                OnPropertyChanged("EnableReject");
            }
        }

        public bool EnableDelete
        {
            get
            {
                return _enableDelete;
            }
            set
            {
                _enableDelete = value;
                OnPropertyChanged("EnableDelete");
            }
        }

        public bool EnableDelegate
        {
            get { return _enableDelegate; }
            set
            {
                _enableDelegate = value;
                OnPropertyChanged("EnableDelegate");
            }
        }

        public bool HasPermission
        {
            get { return _hasPermission; }
            set
            {
                _hasPermission = value;
                OnPropertyChanged("HasPermission");
            }
        }

        public ICommand OpenCommand { get; set; }

        public ICommand ApproveCommand { get; set; }

        public ICommand UnLockCommand { get; set; }

        public ICommand ResumeCommand { get; set; }

        public ICommand EmailAsLinkCommand { get; set; }

        public ICommand RejectCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public ICommand DelegateCommand { get; set; }

        #endregion

        #region Public methods

        public WorkItemListViewModel(Action<Guid> openWorkItem)
        {
            _openWorkItem = openWorkItem;
        }

        public List<Guid> GetSelectedWorkItemIds()
        {
            var selectedDocIds = new List<Guid>();

            try
            {
                var selectedRows = new List<DataRow>();
                selectedRows.AddRange(SearchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_SELECTED]));

                if (selectedRows.Count > 0)
                {
                    selectedDocIds.AddRange(selectedRows.Select(searchRow => (Guid)searchRow[Common.COLUMN_BATCH_ID]));
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return selectedDocIds;
        }

        public void OpenWorkItem(DataRow dataRow)
        {
            Guid id = (Guid)dataRow[Common.COLUMN_BATCH_ID];
            if (_openWorkItem != null)
            {
                _openWorkItem(id);
            }
        }

        #endregion

        #region Private methods

        private void SearchResultPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                ResetContextMenuPermission();

                List<DataRow> selectedRows = SearchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_SELECTED]).ToList();

                if (selectedRows.Count > 0)
                {
                    EnableApprove = true;
                    EnableReject = true;
                    EnableEmailAsLink = true;
                    EnableOpen = true;
                    EnableUnLock = EnableResume = LoginViewModel.LoginUser.IsAdmin;
                    EnableDelete = true;
                    EnableDelegate = LoginViewModel.LoginUser.IsAdmin;

                    foreach (DataRow row in selectedRows)
                    {
                        BatchPermission permission = row[Common.COLUMN_PERMISSION] as BatchPermission;
                        bool isCompleted = (bool)row[Common.COLUMN_IS_COMPLETED];
                        bool isProcessing = (bool)row[Common.COLUMN_IS_PROCESSING];
                        bool hasError = (bool)row[Common.COLUMN_HAS_ERROR];
                        string lockedBy = row[Common.COLUMN_LOCKED_BY] as string;
                        bool isOwner = string.IsNullOrEmpty(lockedBy) || lockedBy == LoginViewModel.LoginUser.Username;// || LoginViewModel.LoginUser.IsAdmin;
                        bool isRejected = (bool)row[Common.COLUMN_IS_REJECTED];

                        EnableResume = EnableResume && !isCompleted && hasError && LoginViewModel.LoginUser.IsAdmin;

                        EnableUnLock = EnableUnLock && !isCompleted && !hasError && !isProcessing
                                        && !string.IsNullOrEmpty(lockedBy)
                                        && (lockedBy.Equals(LoginViewModel.LoginUser.Username) 
                                            || LoginViewModel.LoginUser.IsAdmin);

                        EnableReject = EnableReject && !isCompleted && !hasError && !isProcessing
                                        && !isRejected && permission.CanReject
                                        && (string.IsNullOrEmpty(lockedBy) 
                                            || lockedBy.Equals(LoginViewModel.LoginUser.Username));

                        EnableApprove = EnableApprove && !isCompleted && !hasError && !isProcessing
                                        && (string.IsNullOrEmpty(lockedBy)
                                            || lockedBy.Equals(LoginViewModel.LoginUser.Username));

                        EnableOpen = EnableOpen && !isProcessing;

                        EnableDelete = EnableDelete && !isProcessing && permission.CanDelete;

                        EnableEmailAsLink = EnableEmailAsLink && !isProcessing && permission.CanSendLink;

                        EnableDelegate = false;// Temp disable until next version !isProcessing && permission.CanDelegateItems;

                        HasPermission = HasPermission || (EnableApprove || EnableReject || EnableEmailAsLink || EnableResume || EnableOpen || EnableUnLock || EnableDelete || EnableDelegate);
                    }

                }
            }
        }

        private void ResetContextMenuPermission()
        {
            EnableApprove = false;
            EnableReject = false;
            EnableEmailAsLink = false;
            EnableOpen = false;
            EnableUnLock = false;
            EnableResume = false;
            EnableDelete = false;
        }

        #endregion
    }
}
