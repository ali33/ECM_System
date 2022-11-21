using System;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using Ecm.Model.DataProvider;

namespace Ecm.Admin.ViewModel
{
    public class SettingViewModel : ComponentViewModel
    {
        private SettingModel _setting;
        private RelayCommand _saveCommand;
        private RelayCommand _configureAutoCorrectCommand;
        private bool _hasChanged;
        private readonly SettingProvider _settingProvider = new SettingProvider();

        public SettingViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public sealed override void Initialize()
        {
            try
            {
                Setting = _settingProvider.GetSettings();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public MainViewModel MainViewModel { get; set; }

        public SettingModel Setting
        {
            get { return _setting; }
            set
            {
                _setting = value;
                OnPropertyChanged("Setting");

                if (value != null)
                {
                    _setting.PropertyChanged += SettingPropertyChanged;
                }
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(p => SaveSetting(), p => CanSaveSetting()));
            }
        }

        public ICommand ConfigureAutoCorrectCommand
        {
            get
            {
                if (_configureAutoCorrectCommand == null)
                {
                    _configureAutoCorrectCommand = new RelayCommand(p => ConfigureAutoCorrect());
                }

                return _configureAutoCorrectCommand;
            }
        }

        //Private methods
        private void ConfigureAutoCorrect()
        {
            AmbiguousDefinitionViewModel ambiguousViewModel = new AmbiguousDefinitionViewModel(MainViewModel);
            MainViewModel.ViewModel = ambiguousViewModel;

        }

        private bool CanSaveSetting()
        {
            return _hasChanged;
        }

        private void SaveSetting()
        {
            try
            {
                _settingProvider.WriteSetting(Setting);
                _hasChanged = false;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void SettingPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _hasChanged = true;
        }
    }
}
