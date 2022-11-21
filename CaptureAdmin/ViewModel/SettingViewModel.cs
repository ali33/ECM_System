using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureModel;
using System;
using System.ComponentModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class SettingViewModel : ComponentViewModel
    {
        private RelayCommand _saveCommand;
        private SettingModel _setting;
        private SettingProvider _settingProvider;
        private bool _isChanged;

        public SettingViewModel()
        {
            Initialize();
        }

        public SettingModel Setting
        {
            get { return _setting; }
            set
            {
                _setting = value;
                OnPropertyChanged("Setting");
                if (value != null)
                {
                    Setting.PropertyChanged += new PropertyChangedEventHandler(Setting_PropertyChanged);
                }
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => SaveSetting(), p => CanSave());
                }

                return _saveCommand;
            }
        }

        private bool CanSave()
        {
            return Setting != null && _isChanged;
        }

        private void SaveSetting()
        {
            IsProcessing = true;
            BackgroundWorker save = new BackgroundWorker();
            save.DoWork += new DoWorkEventHandler(save_DoWork);
            save.RunWorkerCompleted += new RunWorkerCompletedEventHandler(save_RunWorkerCompleted);
            save.RunWorkerAsync();
        }

        void save_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                IsProcessing = false;
            }
        }

        void save_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _settingProvider.WriteSetting(Setting);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        public sealed override void Initialize()
        {
            _settingProvider = new SettingProvider();
            Setting = _settingProvider.GetSettings();
        }

        private void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _isChanged = true;
        }

    }
}
