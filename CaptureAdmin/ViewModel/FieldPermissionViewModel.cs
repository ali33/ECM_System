using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.CaptureModel;
using System.Windows.Input;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class FieldPermissionViewModel : ComponentViewModel
    {
        private DocumentFieldPermissionModel _fieldPermission;
        private bool _allPermission;
        private RelayCommand _checkAllCommand;

        public FieldPermissionViewModel()
        {
        }

        public string FieldName { get; set; }

        public DocumentFieldPermissionModel FieldPermission
        {
            get { return _fieldPermission; }
            set
            {
                _fieldPermission = value;
                OnPropertyChanged("FieldPermission");
                if (value != null)
                {
                    value.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(value_PropertyChanged);
                }
            }
        }

        void value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanRead" || e.PropertyName == "CanWrite")
            {
                AllPermission = FieldPermission.CanRead && FieldPermission.CanWrite;
            }
        }

        public bool AllPermission
        {
            get { return _allPermission; }
            set
            {
                _allPermission = value;
                OnPropertyChanged("AllPermission");

                //FieldPermission.CanRead = FieldPermission.CanWrite = FieldPermission.Hidden = value;
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
            FieldPermission.CanRead = FieldPermission.CanWrite = AllPermission;
        }
    }
}
