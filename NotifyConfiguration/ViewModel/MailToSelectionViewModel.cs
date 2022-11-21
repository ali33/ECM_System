using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.CustomActivityModel.DataProvider;
using System.Windows.Input;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.NotifyConfiguration.ViewModel
{
    public class MailToSelectionViewModel : ComponentViewModel
    {
        private ObservableCollection<UserGroupModel> _userGroups = new ObservableCollection<UserGroupModel>();
        private User _loginUser;
        private string _mailList;
        private RelayCommand _addCommand;
        private ObservableCollection<CheckBoxTreeModel> _menuItems = new ObservableCollection<CheckBoxTreeModel>();

        public MailToSelectionViewModel(User loginUser, string mailtos)
        {
            _mailList = mailtos;
            _loginUser = loginUser;
            InitializeData();
        }

        public ObservableCollection<UserGroupModel> UserGroups
        {
            get { return _userGroups; }
            set
            {
                _userGroups = value;
                OnPropertyChanged("UserGroups");
            }
        }

        public ObservableCollection<CheckBoxTreeModel> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                OnPropertyChanged("MenuItems");
            }
        }

        public string MailTos
        {
            get { return _mailList; }
            set
            {
                _mailList = value;
                OnPropertyChanged("MailTos");
            }
        }

        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand(p => AddToMailList());
                }
                return _addCommand;
            }
        }

        public void InitializeData()
        {
            try
            {
                var captureProvider = new CaptureProvider(_loginUser.UserName, _loginUser.EncryptedPassword);
                UserGroups = new ObservableCollection<UserGroupModel>(captureProvider.GetUserGroups());

                CheckBoxTreeModel root = new CheckBoxTreeModel
                {
                    DisplayText = "All",
                    Id = Guid.Empty,
                    IsInitiallySelected = true
                };
                List<CheckBoxTreeModel> children = new List<CheckBoxTreeModel>();

                foreach (var group in UserGroups)
                {
                    CheckBoxTreeModel menuItem = new CheckBoxTreeModel
                    {
                        DisplayText = group.Name,
                        Id = group.Id
                    };

                    foreach (var user in group.Users)
                    {
                        menuItem.Children.Add(new CheckBoxTreeModel
                        {
                            DisplayText = user.Fullname,
                            Id = user.Id,
                            Parent = menuItem,
                            Value = user.EmailAddress,
                            IsChecked = MenuIsCheck(user.EmailAddress)
                        });
                    }

                    children.Add(menuItem);
                }

                root.Children =children;
                MenuItems.Add(root);
            }
            catch (Exception ex)
            {
                CaptureProvider.ProcessException(ex);
            }
        }

        // Private methods
        private void AddToMailList()
        {
            MailTos = string.Empty;
            List<string> mailList = new List<string>();
            List<UserModel> selectedUser = new List<UserModel>();
            
            foreach (CheckBoxTreeModel root in MenuItems[0].Children)
            {
                foreach (CheckBoxTreeModel g in root.Children)
                {
                    if (g.Parent != null && g.Id != Guid.Empty && g.IsChecked.HasValue && g.IsChecked.Value)
                    {
                        mailList.Add(g.Value);
                    }
                }
            }

            mailList = mailList.Select(p=> p).Distinct().ToList();

            foreach (string mail in mailList)
            {
                MailTos += mail + ";";
            }

            if (MailTos != null && MailTos.EndsWith(";"))
            {
                MailTos = MailTos.Remove(MailTos.LastIndexOf(";"));
            }
        }

        private bool MenuIsCheck(string email)
        {
            if (string.IsNullOrEmpty(MailTos))
            {
                return false;
            }

            List<string> mails = MailTos.Split(';').ToList();

            return mails.Any(p => p.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

    }
}
