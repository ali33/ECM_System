using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ecm.Model.DataProvider;
using System.Resources;
using System.Reflection;
using System.ComponentModel;

namespace Ecm.Admin.ViewModel
{
    public class AmbiguousDefinitionViewModel : ComponentViewModel
    {
        private AmbiguousDefinitionModel _ambig;
        private AmbiguousDefinitionModel _editAmbig;
        private LanguageModel _language;
        private RelayCommand _addCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _removeCommand;
        private RelayCommand _closeCommand;

        private AmbiguousDefinitionProvider _ambiguousProvider = new AmbiguousDefinitionProvider();
        private ResourceManager _resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
        private ObservableCollection<AmbiguousDefinitionModel> _ambiguousDefinitions;

        public AmbiguousDefinitionViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            Initialize();
        }

        public MainViewModel MainViewModel { get; set; }

        public AmbiguousDefinitionModel SelectedAmbiguousDefinition
        {
            get { return _ambig; }
            set
            {
                _ambig = value;
                OnPropertyChanged("SelectedAmbiguousDefinition");
                if (value != null)
                {
                    EditPanelVisibled = true;
                    EditAmbiguousDefinition = new AmbiguousDefinitionModel
                    {
                        Text = value.Text,
                        IsUnicode = value.IsUnicode,
                        Id = value.Id,
                        Language = value.Language,
                        LanguageId = value.LanguageId
                    };
                }
            }
        }

        public AmbiguousDefinitionModel EditAmbiguousDefinition
        {
            get { return _editAmbig; }
            set
            {
                _editAmbig = value;
                OnPropertyChanged("EditAmbiguousDefinition");
            }
        }

        public LanguageModel Language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged("Language");
            }
        }

        public ObservableCollection<AmbiguousDefinitionModel> AmbiguousDefinitions
        {
            get { return _ambiguousDefinitions; }
            set
            {
                _ambiguousDefinitions = value;
                OnPropertyChanged("AmbiguousDefinitions");
            }
        }

        public ObservableCollection<LanguageModel> Languages { get; set; }

        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand(p => AddDefinition(), p=> CanAdd());
                }

                return _addCommand;
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new RelayCommand(p => RemoveDefinition(), p => CanRemove());
                }

                return _removeCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => SaveDefinition(), p => CanSave());
                }

                return _saveCommand;
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

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(p => Close());
                }

                return _closeCommand;
            }
        }

        public new Action ResetListView { get; set; }

        //Public methods

        private void Close()
        {
            SettingViewModel settingViewModel = new SettingViewModel(MainViewModel);
            settingViewModel.Initialize();
            MainViewModel.ViewModel = settingViewModel;
        }

        public override void Initialize()
        {
            base.Initialize();
            Languages = new ObservableCollection<LanguageModel>(new LanguageProvider().GetLanguages());
        }

        //Private methods

        private void Cancel()
        {
            //AmbiguousDefinition = null;
            EditPanelVisibled = false;
            if (ResetListView != null)
            {
                ResetListView();
            }
        }

        private bool CanSave()
        {
            return EditAmbiguousDefinition != null && Language != null && !string.IsNullOrEmpty(EditAmbiguousDefinition.Text) && !string.IsNullOrWhiteSpace(EditAmbiguousDefinition.Text);
        }

        private void SaveDefinition()
        {
            BackgroundWorker worker = new BackgroundWorker();
            IsProcessing = true;
            worker.DoWork += new DoWorkEventHandler(DoSave);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoSaveComplated);
            worker.RunWorkerAsync();

            LoadData(Language.Id);
        }

        void DoSaveComplated(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                LoadData(Language.Id);
                EditPanelVisibled = false;
                EditAmbiguousDefinition = null;
            }
        }

        void DoSave(object sender, DoWorkEventArgs e)
        {
            try
            {
                EditAmbiguousDefinition.LanguageId = Language.Id;
                _ambiguousProvider.Save(EditAmbiguousDefinition);
                EditPanelVisibled = false;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }            
        }

        private bool CanRemove()
        {
            return SelectedAmbiguousDefinition != null;
        }

        private void RemoveDefinition()
        {
            if(DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                try
                {
                    _ambiguousProvider.Delete(SelectedAmbiguousDefinition.Id);
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                EditPanelVisibled = false;
                LoadData(Language.Id);
            }
        }

        private void AddDefinition()
        {
            EditPanelVisibled = true;
            EditAmbiguousDefinition = new AmbiguousDefinitionModel();
        }

        private bool CanAdd()
        {
            return Language != null && AmbiguousDefinitions != null && AmbiguousDefinitions.Where(a => (a.IsUnicode || !a.IsUnicode) && a.LanguageId == Language.Id).Count() < 2;
        }

        public void LoadData(Guid languageId)
        {
            try
            {
                AmbiguousDefinitions = new ObservableCollection<AmbiguousDefinitionModel>(_ambiguousProvider.GetAmbiguousDefinitions(languageId));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
    }
}
