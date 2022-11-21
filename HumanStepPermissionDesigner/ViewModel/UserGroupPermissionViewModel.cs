using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Ecm.CaptureDomain;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityDomain;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel
{
    public class UserGroupPermissionViewModel : ComponentViewModel
    {
        #region Private members

        private bool _isDataModified;
        //private readonly HumanStepUserGroupPermission _userGroupPermission;
        private readonly UserGroupPermission _userGroupPermission;
        private RelayCommand _checkCommand;

        #endregion
        public UserGroupPermissionViewModel(UserGroupPermission userGroupPermission, List<DocumentType> docTypes)//HumanStepUserGroupPermission userGroupPermission, List<DocumentType> docTypes)
        {
            _userGroupPermission = userGroupPermission;
            DocTypes = docTypes;

            Initialize();
        }

        public bool CanAnnotate
        {
            get { return _userGroupPermission.CanAnnotate; }
            set
            {
                _userGroupPermission.CanAnnotate = value;
                OnPropertyChanged("CanAnnotate");
            }
        }

        public bool CanDelete
        {
            get { return _userGroupPermission.CanDelete; }
            set
            {
                _userGroupPermission.CanDelete = value;
                OnPropertyChanged("CanDelete");
            }
        }

        public bool CanDownloadFilesOnDemand
        {
            get { return _userGroupPermission.CanDownloadFilesOnDemand; }
            set
            {
                _userGroupPermission.CanDownloadFilesOnDemand = value;
                OnPropertyChanged("CanDownloadFilesOnDemand");
            }
        }

        public bool CanEmail
        {
            get { return _userGroupPermission.CanEmail; }
            set
            {
                _userGroupPermission.CanEmail = value;
                OnPropertyChanged("CanEmail");
            }
        }

        public bool CanModifyDocument
        {
            get { return _userGroupPermission.CanModifyDocument; }
            set
            {
                _userGroupPermission.CanModifyDocument = value;
                OnPropertyChanged("CanModifyDocument");
            }
        }

        public bool CanModifyIndexes
        {
            get { return _userGroupPermission.CanModifyIndexes; }
            set
            {
                _userGroupPermission.CanModifyIndexes = value;
                OnPropertyChanged("CanModifyIndexes");
            }
        }

        public bool CanPrint
        {
            get { return _userGroupPermission.CanPrint; }
            set
            {
                _userGroupPermission.CanPrint = value;
                OnPropertyChanged("CanPrint");
            }
        }

        public bool CanReject
        {
            get { return _userGroupPermission.CanReject; }
            set
            {
                _userGroupPermission.CanReject = value;
                OnPropertyChanged("CanReject");
            }
        }

        public bool CanReleaseLoosePage
        {
            get { return _userGroupPermission.CanReleaseLoosePage; }
            set
            {
                _userGroupPermission.CanReleaseLoosePage = value;
                OnPropertyChanged("CanReleaseLoosePage");
            }
        }

        public bool CanSendLink
        {
            get { return _userGroupPermission.CanSendLink; }
            set
            {
                _userGroupPermission.CanSendLink = value;
                OnPropertyChanged("CanSendLink");
            }
        }

        public bool CanViewOtherItems
        {
            get { return _userGroupPermission.CanViewOtherItems; }
            set
            {
                _userGroupPermission.CanViewOtherItems = value;
                OnPropertyChanged("CanViewOtherItems");
            }
        }

        public UserGroupPermission UserGroupPermission
        {
            get { return _userGroupPermission; }
        }

        //public HumanStepUserGroupPermission UserGroupPermission
        //{
        //    get { return _userGroupPermission; }
        //}

        public List<DocumentType> DocTypes { get; private set; }

        //public ObservableCollection<DocumentPermissionViewModel> DocumentPermissionViewModels { get; set; }

        public bool IsDataModified
        {
            get { return _isDataModified; }
            set
            {
                _isDataModified = value;
                OnPropertyChanged("IsDataModified");
            }
        }

        public ICommand CheckCommand
        {
            get
            {
                return _checkCommand ?? (_checkCommand = new RelayCommand(p => CheckAction()));
            }
        }

        #region Private methods

        private void CheckAction()
        {
            IsDataModified = true;
        }

        private new void Initialize()
        {
            //DocumentPermissionViewModels = new ObservableCollection<DocumentPermissionViewModel>();

            foreach (DocumentType docType in DocTypes)
            {
                //BaseHumanStepDocTypePermission docTypePermission = UserGroupPermission.DocTypePermissions.FirstOrDefault(p => p.DocTypeId == docType.Id);
                //if (docTypePermission == null)
                //{
                //    docTypePermission = new BaseHumanStepDocTypePermission
                //    {
                //        DocTypeId = docType.Id,
                //        DocTypeName = docType.Name,
                //        CanAccess = true
                //    };

                //    UserGroupPermission.DocTypePermissions.Add(docTypePermission);
                //}
                //else
                //{
                //    docTypePermission.DocTypeName = docType.Name;
                //}

                //var documentPermission = new DocumentPermissionViewModel(docTypePermission);
                //documentPermission.PropertyChanged += DocumentPermissionPropertyChanged;

                //DocumentPermissionViewModels.Add(documentPermission);
            }
        }

        private void DocumentPermissionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDataModified")
            {
                IsDataModified = true;
            }
        }

        #endregion

    }
}
