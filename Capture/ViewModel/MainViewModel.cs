using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Text;
using Ecm.Capture.View;
using Ecm.AppHelper;
using Ecm.CaptureModel.DataProvider;

namespace Ecm.Capture.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        #region Private members

        private ComponentViewModel _viewModel;
        private string _welcomeText;
        private bool _hasWorkItem;
        private bool _hasWorkItemOpening;
        private string _currentWorkItemName;

        private RelayCommand _captureViewCommand;
        private RelayCommand _taskViewCommand;
        private RelayCommand _logoutCommand;
        private RelayCommand _changePasswordCommand;
        private RelayCommand _aboutViewCommand;
        private RelayCommand _closeTaskCommand;
        private RelayCommand _closeOtherTasksCommand;
        private RelayCommand _closeAllTasksCommand;

        private readonly CaptureViewModel _captureViewModel;
        private readonly AssignedTaskViewModel _assignedTaskViewModel;
        private readonly ObservableCollection<WorkItemViewModel> _workItemViewModels = new ObservableCollection<WorkItemViewModel>();
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.MainView", Assembly.GetExecutingAssembly());
        private Dispatcher _dispatcher;
        private DialogBaseView _dialog;

        #endregion

        #region Public properties

        public ComponentViewModel ContentViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged("ContentViewModel");

                foreach (var workItemViewModel in WorkItemViewModels)
                {
                    if (value != workItemViewModel)
                    {
                        workItemViewModel.IsActivated = false;
                    }
                }

                if (value is WorkItemViewModel)
                {
                    CurrentWorkItemName = (value as WorkItemViewModel).WorkItemName;
                    HasWorkItemOpening = true;
                }
                else
                {
                    HasWorkItemOpening = false;
                }
            }
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

        public string CurrentWorkItemName
        {
            get { return _currentWorkItemName; }
            set
            {
                _currentWorkItemName = value;
                OnPropertyChanged("CurrentWorkItemName");
            }
        }

        public ObservableCollection<WorkItemViewModel> WorkItemViewModels
        {
            get { return _workItemViewModels; }
        }

        public bool HasCapturePermission { get; private set; }

        public bool HasTaskPermission { get; private set; }

        public bool HasWorkItem
        {
            get { return _hasWorkItem; }
            set
            {
                _hasWorkItem = value;
                OnPropertyChanged("HasWorkItem");
            }
        }

        public bool HasWorkItemOpening
        {
            get { return _hasWorkItemOpening; }
            set
            {
                _hasWorkItemOpening = value;
                OnPropertyChanged("HasWorkItemOpening");
            }
        }

        public CaptureViewModel CaptureViewModel
        {
            get { return _captureViewModel; }
        }

        public AssignedTaskViewModel AssignedTaskViewModel
        {
            get { return _assignedTaskViewModel; }
        }

        public ICommand LogoutCommand
        {
            get { return _logoutCommand ?? (_logoutCommand = new RelayCommand(p => Logout())); }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                return _changePasswordCommand ?? (_changePasswordCommand = new RelayCommand(p => ChangePassword(), p => CanChangePassword()));
            }
        }

        public ICommand CaptureViewCommand
        {
            get
            {
                return _captureViewCommand ?? (_captureViewCommand = new RelayCommand(p => OpenCaptureView(), p => CanOpenCaptureView()));
            }
        }

        public ICommand TaskViewCommand
        {
            get
            {
                return _taskViewCommand ?? (_taskViewCommand = new RelayCommand(p => OpenAssignedTaskView(), p => CanOpenAssignedTaskView()));
            }
        }

        public ICommand AboutViewCommand
        {
            get { return _aboutViewCommand ?? (_aboutViewCommand = new RelayCommand(p => ShowAboutView())); }
        }

        public ICommand CloseTaskCommand
        {
            get { return _closeTaskCommand ?? (_closeTaskCommand = new RelayCommand(p => CloseTask(), p => CanCloseTask())); }
        }

        public ICommand CloseOtherTasksCommand
        {
            get { return _closeOtherTasksCommand ?? (_closeOtherTasksCommand = new RelayCommand(p => CloseOtherTask(), p => CanCloseOtherTask())); }
        }

        public ICommand CloseAllTasksCommand
        {
            get { return _closeAllTasksCommand ?? (_closeAllTasksCommand = new RelayCommand(p => CloseAllTask())); }
        }

        #endregion

        #region Public methods

        public MainViewModel(Dispatcher dispatcher)
        {
            try
            {
                _dispatcher = dispatcher;
                WelcomeText = LoginViewModel.LoginUser.Fullname;

                _captureViewModel = new CaptureViewModel(OnSaveWorkItemCompleted);
                HasCapturePermission = _captureViewModel.BatchTypes.Count > 0;
                HasCapturePermission = true;

                _assignedTaskViewModel = new AssignedTaskViewModel(this, dispatcher);
                HasTaskPermission = true;

                //if (HasCapturePermission)
                //{
                //    ContentViewModel = _captureViewModel;
                //}
                //else
                //{
                ContentViewModel = _assignedTaskViewModel;
                //}

                WorkItemViewModels.CollectionChanged += WorkItemViewModelsCollectionChanged;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public MainViewModel(Dispatcher dispatcher, string workItemId)
        {
            try
            {
                _dispatcher = dispatcher;
                WelcomeText = LoginViewModel.LoginUser.Fullname;

                _captureViewModel = new CaptureViewModel(OnSaveWorkItemCompleted);
                HasCapturePermission = _captureViewModel.BatchTypes.Count > 0;
                HasCapturePermission = true;

                _assignedTaskViewModel = new AssignedTaskViewModel(this, dispatcher);
                HasTaskPermission = true;

                BatchModel batch = new WorkItemProvider().GetWorkItem(Guid.Parse(workItemId));

                if (batch != null)
                {
                    WorkItemViewModel workItemViewModel = new WorkItemViewModel(batch, this);
                }

                WorkItemViewModels.CollectionChanged += WorkItemViewModelsCollectionChanged;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
        #endregion

        #region Private methods

        private void CloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void ShowAboutView()
        {
            ContentViewModel = new AboutViewModel();
        }

        private bool CanChangePassword()
        {
            return true;
        }

        private void ChangePassword()
        {
            var viewModel = new ChangePasswordViewModel(CloseDialog);
            viewModel.UserName = LoginViewModel.LoginUser.Username;
            var view = new ChangePasswordView(viewModel);
            view.MinWidth = 310;
            _dialog = new DialogBaseView(view) { Text = _resource.GetString("tbChangePassword") };
            _dialog.SizeToContent();
            _dialog.ShowDialog();
        }

        private void Logout()
        {
            CloseAllTask();

            LoginViewModel.LoginUser = new UserModel();
            NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
        }

        private bool CanOpenCaptureView()
        {
            return !(ContentViewModel is CaptureViewModel) && HasCapturePermission;
        }

        private void OpenCaptureView()
        {
            ContentViewModel = _captureViewModel;
        }

        private bool CanOpenAssignedTaskView()
        {
            return !(ContentViewModel is AssignedTaskViewModel) && HasTaskPermission;
        }

        private void OpenAssignedTaskView()
        {
            _assignedTaskViewModel.Initialize();
            //_assignedTaskViewModel.RefreshSearchResult();
            _assignedTaskViewModel.ReloadCommand.Execute(null);
            ContentViewModel = _assignedTaskViewModel;
        }

        private void WorkItemViewModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasWorkItem = WorkItemViewModels.Count > 0;
            if (!HasWorkItem)
            {
                ContentViewModel = AssignedTaskViewModel;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                WorkItemViewModel newWorkItem = e.NewItems[0] as WorkItemViewModel;
                if (newWorkItem != null)
                {
                    newWorkItem.SaveCompletedAction = OnSaveWorkItemCompleted;
                }
            }
        }

        private void OnSaveWorkItemCompleted()
        {
            _assignedTaskViewModel.ReloadCommand.Execute(null);
        }

        private bool CanCloseTask()
        {
            return ContentViewModel is WorkItemViewModel;
        }

        private void CloseTask()
        {
            if (ContentViewModel is WorkItemViewModel)
            {
                WorkItemViewModel viewModel = ContentViewModel as WorkItemViewModel;

                if (viewModel.IsChanged && viewModel.WorkItem.LockedBy == LoginViewModel.LoginUser.Username)
                {
                    if (!viewModel.CheckTransactionId())
                    {
                        DialogService.ShowErrorDialog(_resource.GetString("uiWorkitemInvalidTransactionId"));
                    }
                    else
                    {
                        var result = DialogService.ShowTwoStateConfirmDialog(string.Format(_resource.GetString("uiSaveConfirmation"), viewModel.WorkItem.BatchName));
                        if (result == DialogServiceResult.Yes)
                        {
                            viewModel.InternalSave();
                        }
                    }
                }

                (ContentViewModel as WorkItemViewModel).Close(false);
            }
        }

        private void CloseAllTask()
        {
            ContentViewModel = AssignedTaskViewModel;
            StringBuilder viewModelName = new StringBuilder();
            bool isChanged = false;

            foreach (WorkItemViewModel viewModel in WorkItemViewModels)
            {
                if (viewModel.IsChanged && viewModel.WorkItem.LockedBy == LoginViewModel.LoginUser.Username)
                {
                    isChanged = true;
                    viewModelName.AppendLine(viewModel.WorkItemName);
                }
            }

            if (isChanged)
            {
                var result = DialogService.ShowTwoStateConfirmDialog(string.Format(_resource.GetString("uiSaveConfirmation"), viewModelName));
                if (result == DialogServiceResult.Yes)
                {
                    foreach (WorkItemViewModel viewModel in WorkItemViewModels)
                    {
                        if (viewModel.IsChanged && viewModel.WorkItem.LockedBy == LoginViewModel.LoginUser.Username)
                        {
                            viewModel.InternalSave();
                        }
                    }
                }
            }

            WorkItemViewModels.Clear();
            WorkingFolder.Delete(WorkingFolder.UndeletedFiles);
        }

        private bool CanCloseOtherTask()
        {
            return WorkItemViewModels.Count > 1 && ContentViewModel is WorkItemViewModel;
        }

        private void CloseOtherTask()
        {
            var closedDocuments = WorkItemViewModels.Where(p => p != ContentViewModel).ToList();
            StringBuilder viewModelName = new StringBuilder();
            bool isChanged = false;

            foreach (WorkItemViewModel viewModel in closedDocuments)
            {
                if (viewModel.IsChanged && viewModel.WorkItem.LockedBy == LoginViewModel.LoginUser.Username)
                {
                    isChanged = true;
                    viewModelName.AppendLine(viewModel.WorkItemName);
                }
            }

            if (isChanged)
            {
                var result = DialogService.ShowTwoStateConfirmDialog(string.Format(_resource.GetString("uiSaveConfirmation"), viewModelName));
                if (result == DialogServiceResult.Yes)
                {
                    foreach (WorkItemViewModel viewModel in WorkItemViewModels)
                    {
                        if (viewModel.IsChanged && viewModel.WorkItem.LockedBy == LoginViewModel.LoginUser.Username)
                        {
                            viewModel.InternalSave();
                        }
                    }
                }
            }
            foreach (var closedDocument in closedDocuments)
            {
                WorkItemViewModels.Remove(closedDocument);
            }
        }

        #endregion
    }
}
