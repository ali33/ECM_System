using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel
{
    public class FieldPermissionViewModel : ComponentViewModel
    {
        private bool _allPermission;

        private DocumentFieldPermission _documentFieldPermission;

        private RelayCommand _checkAllCommand;

        public FieldPermissionViewModel(DocumentFieldPermission documentFieldPermission)
        {
            _documentFieldPermission = documentFieldPermission;
        }

        public string FieldName { get; set; }

        public Guid FieldId { get; set; }

        public bool AllPermission
        {
            get { return _allPermission; }
            set
            {
                _allPermission = value;
                OnPropertyChanged("AllPermission");
            }
        }

        public bool CanRead
        {
            get { return _documentFieldPermission.CanRead; }
            set
            {
                _documentFieldPermission.CanRead = value;
                OnPropertyChanged("CanRead");
                AllPermission = value && CanWrite;
            }
        }

        public bool CanWrite
        {
            get { return _documentFieldPermission.CanWrite; }
            set
            {
                _documentFieldPermission.CanWrite = value;
                OnPropertyChanged("CanWrite");
                AllPermission = value && CanRead;
            }
        }

        public bool Hidden
        {
            get { return !_documentFieldPermission.CanRead && _documentFieldPermission.CanWrite; }
        }

        public Guid DocTypeId
        {
            get { return _documentFieldPermission.DocTypeId; }
            set
            {
                _documentFieldPermission.DocTypeId = value;
            }
        }

        public Guid UserGroupId
        {
            get { return _documentFieldPermission.UserGroupId; }
            set
            {
                _documentFieldPermission.UserGroupId = value;
            }
        }

        public ICommand CheckAllCommand
        {
            get
            {
                if (_checkAllCommand == null)
                {
                    _checkAllCommand = new RelayCommand(p => CheckAll());
                }

                return _checkAllCommand;
            }
        }

        private void CheckAll()
        {
            CanRead = CanWrite = AllPermission;
        }

    }
}
