using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Resources;
using System.Reflection;
using ArchiveModel = Ecm.Model;
using CaptureModel = Ecm.CaptureModel;
using CaptureDomain = Ecm.CaptureDomain;
using Domain = Ecm.Domain;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.Model;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.Domain;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel.DataProvider;
using Ecm.Workflow.Activities.CustomActivityDomain;
using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.View;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ViewModel
{
    public class ConfigurationMainViewModel : ComponentViewModel
    {
        private RelayCommand _testCommand;
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _clearCommand;
        private RelayCommand _mappingSelectedCommand;
        private RelayCommand _configColumnCommand;

        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ConfigurationView", Assembly.GetExecutingAssembly());
        private bool _testOk;
        public CloseDialog CloseDialog;
        private ArchiveProvider _archiveProvider;
        private bool _isLogged = false;
        private bool _isChanged;
        private string _xml;
        private ObservableCollection<FieldMetaDataModel> _archiveFields = new ObservableCollection<FieldMetaDataModel>();
        private ObservableCollection<TreeModel> _captureDocumentTypeMenus = new ObservableCollection<TreeModel>();
        private ObservableCollection<MappingFieldModel> _mappingFields = new ObservableCollection<MappingFieldModel>();
        private MappingModel _mapping;
        private ReleaseInfoModel _currentReleaseInfo;
        private ArchiveModel.DocumentTypeModel _selectedDocumentType;
        private CaptureModel.DocTypeModel _selectedCaptureDocumentType;

        public ConfigurationMainViewModel(List<CaptureDomain.DocumentType> docTypes, string xml)
        {
            _xml = xml;
            InitData(docTypes);
        }

        public string ReleaseXml { get; set; }

        public ReleaseInfoModel CurrentReleaseInfo
        {
            get { return _currentReleaseInfo; }
            set
            {
                _currentReleaseInfo = value;
                OnPropertyChanged("CurrentReleaseInfo");
                if (value != null)
                {
                    value.LoginInfoModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(LoginInfoModel_PropertyChanged);
                }
            }
        }

        void LoginInfoModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _testOk = false;
            IsLogged = false;
            _isChanged = true;
        }

        public User LoginUser { get; set; }

        public ObservableCollection<TreeModel> CaptureDocumentTypeMenus
        {
            get { return _captureDocumentTypeMenus; }
            set
            {
                _captureDocumentTypeMenus = value;
                OnPropertyChanged("CaptureDocumentTypeMenus");
            }
        }

        public ObservableCollection<CaptureModel.DocTypeModel> CaptureDocumentTypes { get; set; }

        public CaptureModel.DocTypeModel SelectedCaptureDocumentType
        {
            get { return _selectedCaptureDocumentType; }
            set
            {
                _selectedCaptureDocumentType = value;
                OnPropertyChanged("SelectedCaptureDocumentType");
            }
        }

        public ObservableCollection<ArchiveModel.DocumentTypeModel> DocumentTypes { get; set; }

        public ArchiveModel.DocumentTypeModel SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");
            }
        }

        public ObservableCollection<CaptureModel.FieldModel> CaptureFields
        {
            get { return SelectedCaptureDocumentType.Fields; }
        }

        public MappingModel Mapping
        {
            get { return _mapping; }
            set
            {
                _mapping = value;
                OnPropertyChanged("Mapping");
            }
        }

        public ObservableCollection<MappingFieldModel> MappingFields
        {
            get { return _mappingFields; }
            set
            {
                _mappingFields = value;
                OnPropertyChanged("MappingFields");
            }
        }

        public ObservableCollection<FieldMetaDataModel> ArchiveFields
        {
            get { return _archiveFields; }
            set
            {
                _archiveFields = value;
                OnPropertyChanged("ArchiveFields");
            }
        }

        public bool IsLogged
        {
            get { return _isLogged; }
            set
            {
                _isLogged = value;
                OnPropertyChanged("IsLogged");
            }
        }

        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                OnPropertyChanged("IsChanged");
            }
        }

        public ICommand TestCommand
        {
            get
            {
                if (_testCommand == null)
                {
                    _testCommand = new RelayCommand(p => TestConnection(), p => CanTest());
                }
                return _testCommand;
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => SaveInfo(), p => CanSave());
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
                    _cancelCommand = new RelayCommand(p => CancelInfo());
                }
                return _cancelCommand;
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new RelayCommand(p => ClearMapping(), p => CanClearMapping());
                }
                return _clearCommand;
            }
        }

        private void ClearMapping()
        {
            foreach (var item in Mapping.FieldMaps)
            {
                item.ArchiveFieldId = Guid.Empty;
            }
        }

        private bool CanClearMapping()
        {
            return SelectedDocumentType != null;
        }

        public ICommand MappingSelectedCommand
        {
            get
            {
                if (_mappingSelectedCommand == null)
                {
                    _mappingSelectedCommand = new RelayCommand(p => LoadMappingData(p as TreeModel));
                }

                return _mappingSelectedCommand;
            }
        }

        public ICommand ConfigColumnCommand
        {
            get
            {
                if (_configColumnCommand == null)
                {
                    _configColumnCommand = new RelayCommand(p => MapColumn(p));
                }

                return _configColumnCommand;
            }
        }


        //Private methods
        private void InitData(List<CaptureDomain.DocumentType> docTypes)
        {
            CaptureDocumentTypes = Ecm.CaptureModel.DataProvider.ObjectMapper.GetDocTypeModels(docTypes);
            MappingFields = new ObservableCollection<MappingFieldModel>();
            CurrentReleaseInfo = LoadReleaseInfoModelData();

            if (CurrentReleaseInfo == null)
            {
                CurrentReleaseInfo = new ReleaseInfoModel
                {
                    LoginInfoModel = new LoginInfoModel(),
                    MappingInfos = new ObservableCollection<MappingModel>()
                };
            }
            else
            {
                IsEditMode = true;
                _archiveProvider = new ArchiveProvider(CurrentReleaseInfo.LoginInfoModel.UserName, CurrentReleaseInfo.LoginInfoModel.Password, CurrentReleaseInfo.LoginInfoModel.ArchiveEndPoint);
                LoginUser = _archiveProvider.LoginToArchive();
                InitializeData();
            }
        }

        private void CancelInfo()
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

        private bool CanSave()
        {
            return _testOk;
        }

        private bool CanTest()
        {
            return CurrentReleaseInfo.LoginInfoModel != null &&
                !string.IsNullOrEmpty(CurrentReleaseInfo.LoginInfoModel.ArchiveEndPoint) &&
                !string.IsNullOrEmpty(CurrentReleaseInfo.LoginInfoModel.UserName) &&
                !string.IsNullOrEmpty(CurrentReleaseInfo.LoginInfoModel.Password) && IsChanged;
        }

        private void SaveInfo()
        {
            //CurrentReleaseInfo.LoginInfoModel.Password = LoginUser.EncryptedPassword;
            ReleaseXml = Utility.UtilsSerializer.Serialize<ReleaseInfo>(GetReleaseInfoData(CurrentReleaseInfo));
        }

        private void TestConnection()
        {
            //string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(CurrentReleaseInfo.LoginInfoModel.Password);
            _archiveProvider = new ArchiveProvider(CurrentReleaseInfo.LoginInfoModel.UserName, CurrentReleaseInfo.LoginInfoModel.Password, CurrentReleaseInfo.LoginInfoModel.ArchiveEndPoint);

            LoginUser = _archiveProvider.LoginToArchive();

            if (DocumentTypes != null)
            {
                DocumentTypes.Clear();
            }

            CaptureDocumentTypeMenus = null;

            if (LoginUser == null)
            {
                _testOk = false;
                IsLogged = false;
                MessageBox.Show(_resource.GetString("errorLoginFail"), _resource.GetString("uiErrorTitle"));
            }
            else
            {
                _testOk = true;
                IsLogged = true;
                IsChanged = false;
                //CurrentReleaseInfo.LoginInfoModel.Password = LoginUser.EncryptedPassword;
                InitializeData();
                MessageBox.Show(_resource.GetString("LoginSuccess"), _resource.GetString("uiErrorTitle"));
            }
        }

        private void InitializeData()
        {
            try
            {
                _archiveProvider.ConfigUserInfo(LoginUser.UserName, LoginUser.Password);
                DocumentTypes = _archiveProvider.GetDocumentTypes();
                _testOk = true;
                LoadMenuData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadMenuData()
        {
            if (CaptureDocumentTypeMenus == null)
            {
                CaptureDocumentTypeMenus = new ObservableCollection<TreeModel>();
            }

            foreach (var captureDocType in CaptureDocumentTypes)
            {
                var node = new TreeModel
                {
                    Id = captureDocType.Id,
                    MenuText = captureDocType.Name,
                    IsCapture = true,
                    HaveMapping = true
                };

                CaptureDocumentTypeMenus.Add(node);

                if (DocumentTypes != null && DocumentTypes.Count > 0)
                {
                    node.MenuItems = new ObservableCollection<TreeModel>(
                        (from p in DocumentTypes
                         select new TreeModel { Id = p.Id, ParentId = node.Id, MenuText = p.Name, IsArchive = true }).ToList());

                    // Check doc type have mapping
                    foreach (var doc in node.MenuItems)
                    {
                        List<CaptureModel.FieldModel> availableFields = captureDocType.Fields.ToList().Where(p => !p.IsSystemField).ToList();

                        var mapping = CurrentReleaseInfo.MappingInfos.FirstOrDefault(p => p.CaptureDocumentTypeId == node.Id && p.ReleaseDocumentTypeId == doc.Id);
                        if (mapping == null)
                        {
                            continue;
                        }

                        foreach (CaptureModel.FieldModel captureField in availableFields)
                        {
                            var fieldMap = mapping.FieldMaps.FirstOrDefault(q => q.CaptureFieldId == captureField.Id);

                            if (fieldMap != null && fieldMap.ArchiveFieldId != Guid.Empty)
                            {
                                doc.HaveMapping = true;
                                break;
                            }
                        }

                    }
                }
            }
        }

        private void LoadMappingData(TreeModel menu)
        {
            if (menu.IsArchive)
            {
                Guid captureDocTypeId = menu.ParentId;
                Guid archiveDocTypeId = menu.Id;
                Mapping = CurrentReleaseInfo.MappingInfos.FirstOrDefault
                        (p => p.CaptureDocumentTypeId == captureDocTypeId && p.ReleaseDocumentTypeId == archiveDocTypeId);

                if (Mapping == null)
                {
                    Mapping = new MappingModel
                    {
                        CaptureDocumentTypeId = captureDocTypeId,
                        ReleaseDocumentTypeId = archiveDocTypeId,
                        FieldMaps = new ObservableCollection<MappingFieldModel>()
                    };

                    CurrentReleaseInfo.MappingInfos.Add(Mapping);
                }


                SelectedCaptureDocumentType = CaptureDocumentTypes.SingleOrDefault(p => p.Id == captureDocTypeId);
                SelectedDocumentType = DocumentTypes.SingleOrDefault(p => p.Id == archiveDocTypeId);
                var archiveFields = SelectedDocumentType.Fields.Where(h => !h.IsSystemField).OrderBy(p => p.Name).ToList();
                archiveFields.Insert(0, new FieldMetaDataModel() { Name = string.Empty, Id = Guid.Empty });
                ArchiveFields = new ObservableCollection<FieldMetaDataModel>(archiveFields);


                LoadMappingFieldData(captureDocTypeId, archiveDocTypeId);
            }
        }

        private void MapColumn(object p)
        {
            MappingFieldModel mappingField = p as MappingFieldModel;

            TableMappingViewModel columnViewModel = new TableMappingViewModel(mappingField, SelectedCaptureDocumentType, SelectedDocumentType);

            TableMappingView columnView = new TableMappingView(columnViewModel);

            DialogViewer dialog = new DialogViewer(columnView)
            {
                Size = new System.Drawing.Size(600, 600),
                Text = _resource.GetString("uiConfigColumnTitle")
            };

            columnView.Dialog = dialog;
            dialog.ShowDialog();
        }

        private void LoadMappingFieldData(Guid captureDocumentTypeId, Guid archiveDocumentTypeId)
        {
            ObservableCollection<MappingFieldModel> mappingFields = new ObservableCollection<MappingFieldModel>();

            List<CaptureModel.FieldModel> availableFields = SelectedCaptureDocumentType.Fields.ToList().Where(p => !p.IsSystemField).OrderBy(h => h.DisplayOrder).ThenBy(h => h.Name).ToList();

            foreach (CaptureModel.FieldModel captureField in availableFields)
            {
                MappingFieldModel mappingFieldModel = new MappingFieldModel();

                var mappingModel = Mapping.FieldMaps.FirstOrDefault(q => q.CaptureFieldId == captureField.Id);

                if (mappingModel == null)
                {
                    //mappingFieldModel.ArchiveField = string.Empty;
                    mappingFieldModel.ArchiveFieldId = Guid.Empty;
                }
                else
                {
                    //mappingFieldModel.ArchiveField = archiveField.ArchiveField;
                    mappingFieldModel.ArchiveFieldId = mappingModel.ArchiveFieldId;
                    mappingFieldModel.ColumnMappings = mappingModel.ColumnMappings;
                }

                mappingFieldModel.CaptureField = captureField.Name;
                mappingFieldModel.CaptureFieldId = captureField.Id;
                
                mappingFields.Add(mappingFieldModel);
            }

            Mapping.FieldMaps = mappingFields;
        }

        private ReleaseInfoModel LoadReleaseInfoModelData()
        {
            if (!string.IsNullOrEmpty(_xml))
            {
                ReleaseInfo releaseInfo = Utility.UtilsSerializer.Deserialize<ReleaseInfo>(_xml);

                ReleaseInfoModel releaseInfoModel = new ReleaseInfoModel
                {
                    LoginInfoModel = new LoginInfoModel
                    {
                        ArchiveEndPoint = releaseInfo.LoginInfo.ArchiveEndPoint,
                        Password = Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(releaseInfo.LoginInfo.Password),
                        UserName = releaseInfo.LoginInfo.UserName
                    }
                };

                foreach (Mapping mapping in releaseInfo.MappingInfos)
                {
                    MappingModel mappingModel = new MappingModel
                    {
                        CaptureDocumentTypeId = mapping.CaptureDocumentTypeId,
                        ReleaseDocumentTypeId = mapping.ReleaseDocumentTypeId
                    };

                    foreach (MappingField mappingField in mapping.FieldMaps)
                    {
                        ObservableCollection<MappingFieldModel> mappingFields = new ObservableCollection<MappingFieldModel>();
                        mappingField.MappingFields.ForEach(p => mappingFields.Add(new MappingFieldModel
                        {
                            ArchiveFieldId = p.ArchiveFieldId,
                            CaptureFieldId = p.CaptureFieldId
                        }));
                        mappingModel.FieldMaps.Add(new MappingFieldModel { CaptureFieldId = mappingField.CaptureFieldId, ArchiveFieldId = mappingField.ArchiveFieldId, ColumnMappings = mappingFields });
                    }

                    releaseInfoModel.MappingInfos.Add(mappingModel);
                }

                return releaseInfoModel;
            }

            return null;
        }

        private ReleaseInfo GetReleaseInfoData(ReleaseInfoModel releaseInfoModel)
        {
            ReleaseInfo releaseInfo = new ReleaseInfo
            {
                LoginInfo = new LoginInfo
                {
                    ArchiveEndPoint = releaseInfoModel.LoginInfoModel.ArchiveEndPoint,
                    Password = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(releaseInfoModel.LoginInfoModel.Password),
                    UserName = releaseInfoModel.LoginInfoModel.UserName
                }
            };

            releaseInfo.MappingInfos = new List<Mapping>();

            foreach (MappingModel mappingModel in releaseInfoModel.MappingInfos)
            {
                Mapping mapping = new Mapping
                {
                    CaptureDocumentTypeId = mappingModel.CaptureDocumentTypeId,
                    ReleaseDocumentTypeId = mappingModel.ReleaseDocumentTypeId
                };

                mapping.FieldMaps = new List<MappingField>();

                foreach (MappingFieldModel mappingFieldModel in mappingModel.FieldMaps)
                {
                    if (mappingFieldModel.ArchiveFieldId == Guid.Empty)
                    {
                        continue;
                    }

                    List<MappingField> mappingFields = new List<MappingField>();

                    if (mappingFieldModel.ColumnMappings != null)
                    {
                        foreach (var tableFieldModel in mappingFieldModel.ColumnMappings.ToList())
                        {
                            if (tableFieldModel.ArchiveFieldId == Guid.Empty)
                            {
                                continue;
                            }

                            mappingFields.Add(new MappingField
                            {
                                ArchiveFieldId = tableFieldModel.ArchiveFieldId,
                                CaptureFieldId = tableFieldModel.CaptureFieldId
                            });
                        }
                    }

                    mapping.FieldMaps.Add(new MappingField
                    {
                        CaptureFieldId = mappingFieldModel.CaptureFieldId,
                        ArchiveFieldId = mappingFieldModel.ArchiveFieldId,
                        MappingFields = mappingFields
                    });
                }

                if (mapping.FieldMaps.Count > 0)
                {
                    releaseInfo.MappingInfos.Add(mapping);
                }
            }

            return releaseInfo;
        }
    }
}
