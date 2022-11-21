using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.CaptureModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ecm.CaptureModel.DataProvider;

namespace Ecm.Capture.ViewModel
{
    public class DelegationUserViewModel : ComponentViewModel
    {
        private UserModel _delegationUser;
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private string _delegatedComment;

        public DelegationUserViewModel(Action<bool> saveCompleted)
        {
            SaveCompleted = saveCompleted;
            DelegationUsers = new UserProvider().GetAvailableUsersToDelegate();
        }

        public ObservableCollection<UserModel> DelegationUsers { get; private set; }

        private Action<bool> SaveCompleted { get; set; }

        public UserModel DelegationUser
        {
            get { return _delegationUser; }
            set
            {
                _delegationUser = value;
                OnPropertyChanged("DelegationUser");
            }
        }

        public string DelegatedComment
        {
            get { return _delegatedComment; }
            set
            {
                _delegatedComment = value;
                OnPropertyChanged("DelegatedComment");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => Save(), p => CanSave());
                }

                return _okCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => Cancel());
                }

                return _cancelCommand;
            }
        }

        private void Cancel()
        {
            if (SaveCompleted != null)
            {
                SaveCompleted(false);
            }
        }

        private bool CanSave()
        {
            return DelegationUser != null;
        }

        private void Save()
        {
            if (SaveCompleted != null)
            {
                SaveCompleted(true);
            }
        }
    }
}
