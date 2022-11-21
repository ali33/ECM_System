using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.CaptureDomain;
using System.Windows.Input;
using Ecm.Workflow.Activities.NotifyConfiguration.View;
using System.Resources;
using System.Reflection;
using Ecm.Workflow.Activities.CustomActivityDomain;
using System.Configuration;

namespace Ecm.Workflow.Activities.NotifyConfiguration.ViewModel
{
    public class SettingViewModel : ComponentViewModel
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.NotifyConfiguration.Resource", Assembly.GetExecutingAssembly());
        private NotifySettingsModel _notifySettings;
        private bool _isMail;
        private bool _isSms;
        private bool _isBoth;
        private string _xml;
        private User _loginUser;

        private RelayCommand _browseCommand;
        private RelayCommand _saveCommand;

        public SettingViewModel(string xml, User loginUser)
        {
            _xml = xml;
            _loginUser = loginUser;
            _notifySettings = new NotifySettingsModel();
            Initialize();
        }

        public NotifySettingsModel NotifySettings
        {
            get { return _notifySettings; }
            set
            {
                _notifySettings = value;
                OnPropertyChanged("NotifySettings");
            }
        }

        public bool IsMail
        {
            get { return _isMail; }
            set
            {
                _isMail = value;
                OnPropertyChanged("IsMail");
            }
        }

        public bool IsSms
        {
            get { return _isSms; }
            set
            {
                _isSms = value;
                OnPropertyChanged("IsSms");
            }
        }

        public bool IsBoth
        {
            get { return _isBoth; }
            set
            {
                _isBoth = value;
                OnPropertyChanged("IsBoth");
            }
        }

        public string SettingXml { get; set; }

        public ICommand BrowseCommand
        {
            get
            {
                if (_browseCommand == null)
                {
                    _browseCommand = new RelayCommand(p => SelectMail());
                }

                return _browseCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => Save(), p=> CanSave());
                }

                return _saveCommand;
            }
        }
        //Public methods
        public override void Initialize()
        {
            NotifySettings = LoadSetting();

            if (NotifySettings == null)
            {
                NotifySettings = new NotifySettingsModel
                {
                    NotifyType = NotifyType.Mail,
                    MailInfo = new MailInfoModel
                    {
                        SmtpHostName = ConfigurationManager.AppSettings["SMTPHost"].ToString(),
                        SmtpPortNumber = ConfigurationManager.AppSettings["PortNumber"].ToString(),
                        SmtpUserName = ConfigurationManager.AppSettings["NoReplyMail"].ToString(),
                        MailFrom = _loginUser.Email,
                    },
                    SmsInfo = new SmsInfoModel()
                };
            }

            IsBoth = NotifySettings.NotifyType == NotifyType.Both;
            IsMail = NotifySettings.NotifyType == NotifyType.Mail;
            IsSms = NotifySettings.NotifyType == NotifyType.SMS;
        }

        //Private methods
        private void SelectMail()
        {
            MailToSelectionViewModel viewModel = new MailToSelectionViewModel(_loginUser, NotifySettings.MailInfo.MailTos);

            MailToSelectionView view = new MailToSelectionView(viewModel);
            DialogViewer dialog = new DialogViewer(view);
            
            view.Dialog = dialog;
            dialog.Text = _resource.GetString("uiSelectMailTitle");
            dialog.Size = new System.Drawing.Size(500, 600);
            dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            dialog.FormClosed += (s, o) =>
            {
                if (dialog.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    NotifySettings.MailInfo.MailTos = viewModel.MailTos;
                }
            };

            dialog.ShowDialog();
        }

        private NotifySettingsModel LoadSetting()
        {
            NotifySettingsModel settingViewModel = null;

            if (!string.IsNullOrEmpty(_xml))
            {
                NotifySettings settingInfo = Utility.UtilsSerializer.Deserialize<NotifySettings>(_xml);

                if (settingInfo != null)
                {
                    settingViewModel = new NotifySettingsModel
                    {
                        NotifyType = (NotifyType)Enum.ToObject(typeof(NotifyType), settingInfo.NotifyType),
                        MailInfo = new MailInfoModel
                        {
                            Body = settingInfo.MailInfo.Body,
                            MailFrom = settingInfo.MailInfo.MailFrom,
                            MailTos = settingInfo.MailInfo.MailTos,
                            Subject = settingInfo.MailInfo.Subject,
                            SmtpHostName = settingInfo.MailInfo.SmtpHostName,
                            SmtpPassword = settingInfo.MailInfo.SmtpPassword,
                            SmtpPortNumber = settingInfo.MailInfo.SmtpPortNumber,
                            SmtpUserName = settingInfo.MailInfo.SmtpUserName
                        },
                        SmsInfo = new SmsInfoModel()
                    };
                }
                else
                {
                    settingViewModel = new NotifySettingsModel
                    {
                        NotifyType = NotifyType.Mail,
                        MailInfo = new MailInfoModel
                        {
                            Body = _resource.GetString("uiNotifyMailBodyTemplate"),
                            SmtpHostName = ConfigurationManager.AppSettings["SMTPHost"].ToString(),
                            SmtpPortNumber = ConfigurationManager.AppSettings["PortNumber"].ToString(),
                            SmtpUserName = ConfigurationManager.AppSettings["NoReplyMail"].ToString(),
                            MailFrom = _loginUser.Email,
                            Subject = _resource.GetString("uiNotifyMailSubjectTemplate")
                        },
                        SmsInfo = new SmsInfoModel()
                    };
                }
            }
            else
            {
                settingViewModel = new NotifySettingsModel
                {
                    NotifyType = NotifyType.Mail,
                    MailInfo = new MailInfoModel
                    {
                        Body = _resource.GetString("uiNotifyMailBodyTemplate"),
                        Subject = _resource.GetString("uiNotifyMailSubjectTemplate"),
                        SmtpHostName = ConfigurationManager.AppSettings["SMTPHost"].ToString(),
                        SmtpPortNumber = ConfigurationManager.AppSettings["PortNumber"].ToString(),
                        SmtpUserName = ConfigurationManager.AppSettings["NoReplyMail"].ToString(),
                        MailFrom = _loginUser.Email,
                    },
                    SmsInfo = new SmsInfoModel()
                };
            }

            return settingViewModel;
        }

        private bool CanSave()
        {
            return NotifySettings != null && NotifySettings.MailInfo != null &&
                !string.IsNullOrEmpty(NotifySettings.MailInfo.MailFrom) && !string.IsNullOrEmpty(NotifySettings.MailInfo.MailTos) &&
                !string.IsNullOrEmpty(NotifySettings.MailInfo.Subject) && !string.IsNullOrEmpty(NotifySettings.MailInfo.Body);
        }

        private void Save()
        {
            SettingXml = Utility.UtilsSerializer.Serialize<NotifySettings>(GetNotifySettings());
        }

        private NotifySettings GetNotifySettings()
        {
            if (NotifySettings == null)
            {
                return null;
            }

            return new NotifySettings
                {
                    MailInfo = new MailInfo
                    {
                        Body = NotifySettings.MailInfo.Body,
                        MailFrom = NotifySettings.MailInfo.MailFrom,
                        MailTos = NotifySettings.MailInfo.MailTos,
                        Subject = NotifySettings.MailInfo.Subject,
                        SmtpPassword = NotifySettings.MailInfo.SmtpPassword,
                        SmtpPortNumber = NotifySettings.MailInfo.SmtpPortNumber,
                        SmtpUserName = NotifySettings.MailInfo.SmtpUserName,
                        SmtpHostName = NotifySettings.MailInfo.SmtpHostName
                    },
                    NotifyType = (int)NotifySettings.NotifyType,
                    SmsInfo = new SmsInfo()
                };
        }

    }
}
