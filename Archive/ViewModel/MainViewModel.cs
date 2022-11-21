using System;
using System.Collections.Specialized;
using System.Linq;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.Model;
using Ecm.Archive.View;
using System.Collections.ObjectModel;
using System.Resources;
using System.Reflection;
using Ecm.AppHelper;
using System.Collections.Generic;
using System.IO;

namespace Ecm.Archive.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        #region Private members

        private RelayCommand _searchViewCommand;
        private RelayCommand _captureViewCommand;
        private RelayCommand _logoutCommand;
        private RelayCommand _changePasswordCommand;
        private RelayCommand _closeCommand;
        private RelayCommand _closeAllCommand;
        private RelayCommand _closeOtherCommand;
        private RelayCommand _aboutViewCommand;
        private RelayCommand _helpViewCommand;
        private ComponentViewModel _viewModel;
        private readonly SearchViewModel _searchViewModel;
        private readonly CaptureViewModel _captureViewModel;
        private readonly ObservableCollection<DocumentViewModel> _documentViewModels = new ObservableCollection<DocumentViewModel>();
        private DialogBaseView _dialog;
        private bool _hasDocument;
        private string _currentDocumentName;
        private bool _hasDocumentOpening;
        private string _welcomeText;
        ResourceManager _resource = new ResourceManager("Ecm.Archive.MainView", Assembly.GetExecutingAssembly());
        #endregion

        #region Public properties
        public ComponentViewModel ContentViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged("ContentViewModel");

                foreach (var documentTabViewModel in DocumentViewModels)
                {
                    if (value != documentTabViewModel)
                    {
                        documentTabViewModel.IsActivated = false;
                    }
                }

                if (value is DocumentViewModel)
                {
                    CurrentDocumentName = (value as DocumentViewModel).DocumentName;
                    HasDocumentOpening = true;
                }
                else
                {
                    HasDocumentOpening = false;
                }

                // Work item
                /*foreach (var workItemViewModel in WorkItemViewModels)
                {
                    if (value != workItemViewModel)
                    {
                        workItemViewModel.IsActivated = false;
                    }
                }

                if (value is WorkItemViewModel)
                {
                    CurrentDocumentName = (value as WorkItemViewModel).WorkItemName;
                    HasWorkItemOpening = true;
                }
                else
                {
                    HasWorkItemOpening = false;
                }*/
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

        public ObservableCollection<DocumentViewModel> DocumentViewModels
        {
            get { return _documentViewModels; }
        }

        public SearchViewModel SearchViewModel
        {
            get { return _searchViewModel; }
        }

        public CaptureViewModel CaptureViewModel
        {
            get { return _captureViewModel; }
        }

        public bool HasSearchPermission { get; private set; }

        public bool HasCapturePermission { get; private set; }

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

        public ICommand SearchViewCommand
        {
            get
            {
                return _searchViewCommand ?? (_searchViewCommand = new RelayCommand(p => OpenSearchView(), p => CanOpenSearchView()));
            }
        }

        public ICommand CaptureViewCommand
        {
            get
            {
                return _captureViewCommand ?? (_captureViewCommand = new RelayCommand(p => OpenCaptureView(), p => CanOpenCaptureView()));
            }
        }

        public ICommand LogoutCommand
        {
            get { return _logoutCommand ?? (_logoutCommand = new RelayCommand(p => Logout())); }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                return _changePasswordCommand ?? (_changePasswordCommand = new RelayCommand(p => ChangePassword()));
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

        public ICommand AboutViewCommand
        {
            get { return _aboutViewCommand ?? (_aboutViewCommand = new RelayCommand(p => ShowAboutView())); }
        }

        public ICommand HelpViewCommand
        {
            get { return _helpViewCommand ?? (_helpViewCommand = new RelayCommand(p => ShowHelp())); }
        }


        #endregion

        #region Public methods

        public MainViewModel()
        {
            try
            {
                WelcomeText = LoginViewModel.LoginUser.Fullname;

                _searchViewModel = new SearchViewModel(this);
                HasSearchPermission = _searchViewModel.DocumentTypes != null && _searchViewModel.DocumentTypes.Count > 0;

                _captureViewModel = new CaptureViewModel();
                HasCapturePermission = _captureViewModel.DocumentTypes.Count > 0;

                if (HasSearchPermission)
                {
                    ContentViewModel = _searchViewModel;
                }
                else if (HasCapturePermission)
                {
                    ContentViewModel = _captureViewModel;
                }

                DocumentViewModels.CollectionChanged += DocumentViewModelsCollectionChanged;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void RunGlobalSearch(string query)
        {
            if (!(ContentViewModel is SearchViewModel))
            {
                ContentViewModel = _searchViewModel;
            }

            _searchViewModel.RunGlobalSearch(query);
        }

        public void OpenLinkDocument(Guid docId)
        {
            new DocumentViewModel(docId, this);
        }

        #endregion

        #region Private methods

        private void ShowHelp()
        {
            DialogBaseView dialog = new DialogBaseView(new HelpView());
            dialog.Text = _resource.GetString("Page.WindowTitle");
            dialog.MaximizeBox = true;
            dialog.MinimizeBox = true;
            dialog.Size = new System.Drawing.Size(800, 600);
            dialog.Show();
        }

        private void ShowAboutView()
        {
            ContentViewModel = new AboutViewModel();
        }

        private bool CanOpenCaptureView()
        {
            return !(ContentViewModel is CaptureViewModel) && HasCapturePermission;
        }

        private void OpenCaptureView()
        {
            ContentViewModel = _captureViewModel;
        }

        private bool CanOpenSearchView()
        {
            return !(ContentViewModel is SearchViewModel) && HasSearchPermission;
        }

        private void OpenSearchView()
        {
            ContentViewModel = _searchViewModel;
        }

        private void ChangePassword()
        {
            var viewModel = new ChangePasswordViewModel(CloseDialog);
            var view = new ChangePasswordView(viewModel);
            viewModel.UserName = LoginViewModel.LoginUser.Username;
            _dialog = new DialogBaseView(view) { Size = new System.Drawing.Size(310, 275), Text = _resource.GetString("tbChangePassword") };
            _dialog.ShowDialog();
        }

        private void CloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void Logout()
        {
            CloseAllDocument();

            LoginViewModel.LoginUser = new UserModel();
            NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
        }

        private void DocumentViewModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasDocument = DocumentViewModels.Count > 0;
            if (DocumentViewModels.Count == 0)
            {
                ContentViewModel = SearchViewModel;
            }
        }

        private bool CanCloseDocument()
        {
            return ContentViewModel is DocumentViewModel;
        }

        private void CloseDocument()
        {
            if (ContentViewModel is DocumentViewModel)
            {
                var viewModel = ContentViewModel as DocumentViewModel;

                var result = DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiSaveConfirmCurrent"));
                if (result == DialogServiceResult.Yes)
                {
                    viewModel.Save();
                }
                else
                {
                    (viewModel).Close();
                    DeleteOutlookTempFiles(viewModel.Document.EmbeddedPictures.Keys.ToList());
                }
            }
        }


        private void CloseAllDocument()
        {
            var result = DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiSaveConfirmAll"));
            if (result == DialogServiceResult.Yes)
            {
                foreach (var closedDocument in DocumentViewModels)
                {
                    closedDocument.Save();
                }
                ContentViewModel = SearchViewModel;
            }
            else
            {

                List<string> deleteFiles = new List<string>();
                DocumentViewModels.ToList().ForEach(p => deleteFiles.AddRange(p.Document.EmbeddedPictures.Keys.ToList()));
                DocumentViewModels.Clear();

                DeleteOutlookTempFiles(deleteFiles);
                WorkingFolder.Delete(WorkingFolder.UndeletedFiles);
            }


        }

        private bool CanCloseOtherDocument()
        {
            return DocumentViewModels.Count > 1 && ContentViewModel is DocumentViewModel;
        }

        private void CloseOtherDocument()
        {
            var closedDocuments = DocumentViewModels.Where(p => p != ContentViewModel).ToList();

            var result = DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiSaveConfirmOther"));
            if (result == DialogServiceResult.Yes)
            {
                foreach (var closedDocument in closedDocuments)
                {
                    closedDocument.Save();
                    DocumentViewModels.Remove(closedDocument);
                }
            }
            else
            {
                foreach (var closedDocument in closedDocuments)
                {
                    DocumentViewModels.Remove(closedDocument);
                    DeleteOutlookTempFiles(closedDocument.Document.EmbeddedPictures.Keys.ToList());
                }
            }


        }

        public void DeleteOutlookTempFiles(List<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                string filePath = Path.Combine(new WorkingFolder("ECMAddin").Dir, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        #endregion
    }
}
