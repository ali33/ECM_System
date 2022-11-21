using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ecm.Audit.View;
using Ecm.Model.DataProvider;
using System.Resources;
using System.Reflection;

namespace Ecm.Audit.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        private ComponentViewModel _viewModel;
        private ObservableCollection<MenuModel> _menuItems = new ObservableCollection<MenuModel>();
        private RelayCommand _actionLogViewCommand;
        private RelayCommand _reportViewCommand;
        private RelayCommand _logoutCommand;
        private RelayCommand _changePasswordCommand;
        private RelayCommand _supportViewCommand;
        private RelayCommand _aboutViewCommand;
        private RelayCommand _historyViewCommand;
        private RelayCommand _deletedDocumentCommand;
        private DialogBaseView _dialog;
        private bool _hasDocument;
        private string _currentDocumentName;
        private bool _hasDocumentOpening;
        private HistoryViewModel _historyViewModel;
        private DeletedDocumentHistoryViewModel _deleteDocumentViewModel;
        private ActionLogViewModel _actionLogViewModel;
        private ReportViewModel _reportViewModel;
        private RelayCommand _closeCommand;
        private RelayCommand _closeAllCommand;
        private RelayCommand _closeOtherCommand;
        private string _welcomeText;
        private ObservableCollection<HistoryDetailViewModel> _historyDetailViewModels = new ObservableCollection<HistoryDetailViewModel>();
        private AuditPermissionModel _auditPermission;
        private PermissionProvider _permissionProvider = new PermissionProvider();

        public MainViewModel()
        {
            AuditPermission = _permissionProvider.GetAuditPermissionByUser(LoginViewModel.LoginUser);
            WelcomeText = LoginViewModel.LoginUser.Fullname;
            if (AuditPermission.AllowedAudit)
            {
                _historyViewModel = new HistoryViewModel(this);
                _deleteDocumentViewModel = new DeletedDocumentHistoryViewModel(this);
            }

            if (AuditPermission.AllowedViewLog)
            {
                _actionLogViewModel = new ActionLogViewModel();
            }

            if (AuditPermission.AllowedViewReport)
            {
                _reportViewModel = new ReportViewModel();
            }

            HistoryDetailViewModels.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(HistoryDetailViewModels_CollectionChanged);
        }

        public string WelcomeText
        {
            get { return _welcomeText; }
            set
            {
                _welcomeText = value;
                OnPropertyChanged("WelcomeText");
            }
        }

        public AuditPermissionModel AuditPermission
        {
            get { return _auditPermission; }
            set
            {
                _auditPermission = value;
                OnPropertyChanged("AuditPermission");
            }
        }

        public ComponentViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged("ViewModel");

                foreach (var documentTabViewModel in HistoryDetailViewModels)
                {
                    if (value != documentTabViewModel)
                    {
                        documentTabViewModel.IsActivated = false;
                    }
                }

                if (value is HistoryDetailViewModel)
                {
                    CurrentDocumentName = (value as HistoryDetailViewModel).DocumentName;
                    HasDocumentOpening = true;
                }
                else
                {
                    HasDocumentOpening = false;
                }

            }
        }

        public bool HasDocument
        {
            get { return _hasDocument; }
            set
            {
                _hasDocument = value;
                OnPropertyChanged("HasDocument");
            }
        }

        public string CurrentDocumentName
        {
            get { return _currentDocumentName; }
            set
            {
                _currentDocumentName = value;
                OnPropertyChanged("CurrentDocumentName");
            }
        }

        public bool HasDocumentOpening
        {
            get { return _hasDocumentOpening; }
            set
            {
                _hasDocumentOpening = value;
                OnPropertyChanged("HasDocumentOpening");
            }
        }

        public ActionLogViewModel ActionLogViewModel
        {
            get { return _actionLogViewModel; }
        }

        public ReportViewModel ReportViewModel
        {
            get { return _reportViewModel; }
        }

        public HistoryViewModel HistoryViewModel
        {
            get { return _historyViewModel; }
        }

        public DeletedDocumentHistoryViewModel DeletedDocumentViewModel
        {
            get { return _deleteDocumentViewModel; }
        }

        public ObservableCollection<HistoryDetailViewModel> HistoryDetailViewModels
        {
            get { return _historyDetailViewModels; }
        }

        public ICommand ActionLogViewCommand
        {
            get
            {
                if (_actionLogViewCommand == null)
                {
                    _actionLogViewCommand = new RelayCommand(p => ShowActionLogView(),p=> CanShowActionLogView());
                }

                return _actionLogViewCommand;
            }
        }

        public ICommand ReportViewCommand
        {
            get
            {
                if (_reportViewCommand == null)
                {
                    _reportViewCommand = new RelayCommand(p => ShowReportView(), p => CanShowReportView());
                }

                return _reportViewCommand;
            }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                if (_changePasswordCommand == null)
                {
                    _changePasswordCommand = new RelayCommand(p => ChangePassword(), p => CanChangePassword());
                }

                return _changePasswordCommand;
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                if (_logoutCommand == null)
                {
                    _logoutCommand = new RelayCommand(p => Logout());
                }
                return _logoutCommand;
            }
        }

        public ICommand SupportViewCommand
        {
            get
            {
                if (_supportViewCommand == null)
                {
                    _supportViewCommand = new RelayCommand(p => ShowSupportView());
                }

                return _supportViewCommand;
            }
        }

        public ICommand AboutViewCommand
        {
            get
            {
                if (_aboutViewCommand == null)
                {
                    _aboutViewCommand = new RelayCommand(p => ShowAboutView());
                }
                return _aboutViewCommand;
            }
        }

        public ICommand HistoryViewCommand
        {
            get
            {
                if (_historyViewCommand == null)
                {
                    _historyViewCommand = new RelayCommand(p => ShowHistoryView(), p=> CanViewHistory());
                }
                return _historyViewCommand;
            }
        }

        public ICommand DeletedDocumentCommand
        {
            get
            {
                if (_deletedDocumentCommand == null)
                {
                    _deletedDocumentCommand = new RelayCommand(p => ShowDeletedDocumentView(), p => CanViewDeletedHistory());
                }
                return _deletedDocumentCommand;
            }
        }

        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => CloseDocument(), p => CanCloseDocument())); }
        }

        public ICommand CloseAllCommand
        {
            get { return _closeAllCommand ?? (_closeAllCommand = new RelayCommand(p => CloseAllDocument())); }
        }

        public ICommand CloseOtherCommand
        {
            get { return _closeOtherCommand ?? (_closeOtherCommand = new RelayCommand(p => CloseOtherDocument(), p => CanCloseOtherDocument())); }
        }

        //Public methods
        public void RunGlobalSearch(string text)
        {
            if(!(ViewModel is ActionLogViewModel))
            {
                ViewModel = new ActionLogViewModel();
            }
            
            (ViewModel as ActionLogViewModel).RunGlobalSearch(text);
       }

        //Private methods

        private void ShowDeletedDocumentView()
        {
            ViewModel = _deleteDocumentViewModel;
        }

        private bool CanViewDeletedHistory()
        {
            return !(ViewModel is DeletedDocumentHistoryViewModel);
        }

        void HistoryDetailViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasDocument = HistoryDetailViewModels.Count > 0;

            if (HistoryDetailViewModels.Count == 0)
            {
                ViewModel = HistoryViewModel;
            }

        }

        private void ShowHistoryView()
        {
            ViewModel = _historyViewModel;
        }

        private bool CanViewHistory()
        {
            return !(ViewModel is HistoryViewModel);
        }

        private void ShowAboutView()
        {
            ViewModel = new AboutViewModel();
        }

        private bool CanShowActionLogView()
        {
            return !(ViewModel is ActionLogViewModel);
        }

        private void ShowActionLogView()
        {
            ViewModel =_actionLogViewModel;
        }

        private bool CanShowReportView()
        {
            return !(ViewModel is ReportViewModel);
        }

        private void ShowReportView()
        {
            ViewModel = _reportViewModel;
        }

        private bool CanChangePassword()
        {
            return !LoginViewModel.LoginUser.IsAdmin;
        }

        private void ChangePassword()
        {
            ChangePasswordView view = new ChangePasswordView(new ChangePasswordViewModel(CloseView));
            _dialog = new DialogBaseView(view) { Size = new System.Drawing.Size(310, 266), Text = "Change password" };
            _dialog.ShowDialog();
        }

        private void CloseView()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void Logout()
        {
            LoginViewModel.LoginUser = new UserModel();
            NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ShowSupportView()
        {
            var res = new ResourceManager("Ecm.Audit.MainView", Assembly.GetExecutingAssembly());
            DialogBaseView dialog = new DialogBaseView(new HelpView());
            dialog.Text = res.GetString("uiHelpTitle");
            dialog.MaximizeBox = true;
            dialog.MinimizeBox = true;
            dialog.Size = new System.Drawing.Size(800, 600);
            dialog.Show();
        }

        private void CloseAllDocument()
        {
            ViewModel = HistoryViewModel;
            HistoryDetailViewModels.Clear();
        }

        private bool CanCloseOtherDocument()
        {
            return HistoryDetailViewModels.Count > 1 && ViewModel is HistoryDetailViewModel;
        }

        private void CloseOtherDocument()
        {
            var closedDocuments = HistoryDetailViewModels.Where(p => p != ViewModel).ToList();
            foreach (var closedDocument in closedDocuments)
            {
                HistoryDetailViewModels.Remove(closedDocument);
            }
        }

        private bool CanCloseDocument()
        {
            return ViewModel is HistoryDetailViewModel;
        }

        private void CloseDocument()
        {
            if (ViewModel is HistoryDetailViewModel)
            {
                (ViewModel as HistoryDetailViewModel).Close();
            }
        }

    }
}
