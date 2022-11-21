using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.CaptureModel;
using System.Collections.ObjectModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class DocumentFieldPermissionViewModel : ComponentViewModel
    {
        private DocTypePermissionModel _documentTypePermission;
        private ObservableCollection<FieldPermissionViewModel> _fieldViewModel = new ObservableCollection<FieldPermissionViewModel>();
        private bool _permissionChanged;

        public DocumentFieldPermissionViewModel()
        {
        }

        public DocTypeModel DocumentType { get; set; }

        public bool PermissionChanged
        {
            get { return _permissionChanged; }
            set
            {
                _permissionChanged = value;
                OnPropertyChanged("PermissionChanged");
            }
        }

        public DocTypePermissionModel DocumentTypePermission
        {
            get { return _documentTypePermission; }
            set
            {
                _documentTypePermission = value;
                OnPropertyChanged("DocumentTypePermission");

                if (value != null)
                {
                    value.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(value_PropertyChanged);
                }
            }
        }

        void value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanAccess")
            {
                PermissionChanged = true;
            }
        }
    }
}
