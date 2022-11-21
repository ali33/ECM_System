using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.Workflow.Activities.CustomActivityModel;
using System.Windows.Input;
using Ecm.Workflow.Activities.LookupConfiguration.View;
using System.Resources;
using System.Reflection;
using Ecm.Utility;
using Ecm.Workflow.Activities.CustomActivityDomain;
using Ecm.Workflow.Activities.CustomActivityModel.DataProvider;
using Ecm.CaptureDomain;
using Ecm.LookupDomain;

namespace Ecm.Workflow.Activities.LookupConfiguration.ViewModel
{
    public class BatchTypeViewModel : ComponentViewModel
    {
        private RelayCommand _saveCommand;
        private RelayCommand _configureLookupCommand;
        private RelayCommand _deleteLookupCommand;
        private ObservableCollection<FieldModel> _batchFields = new ObservableCollection<FieldModel>();
        private DialogViewer _dialog;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.BatchTypeView", Assembly.GetExecutingAssembly());
        private LookupProvider _lookupProvider = new LookupProvider();
        private List<Guid> _deleteLookupClient = new List<Guid>();
        private User _user;

        public Guid ActivityId { get; set; }

        public BatchTypeViewModel(BatchTypeModel batchType, string xml, User user, Guid activityId)
        {
            ActivityId = activityId;
            LookupXml = xml;
            BatchType = batchType;
            DocumentTypeFieldViewModels = new ObservableCollection<DocumentFieldViewModel>();
            _user = user;
            InitializeData();
        }

        public Action SaveCompleted { get; set; }

        public string LookupXml { get; set; }

        public LookupConfigurationModel LookupConfiguration { get; set; }

        public BatchTypeModel BatchType { get; set; }

        public ObservableCollection<DocumentFieldViewModel> DocumentTypeFieldViewModels { get; set; }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => Save(), p => CanSave());
                }

                return _saveCommand;
            }
        }

        public ICommand ConfigureLookupCommand
        {
            get
            {
                if (_configureLookupCommand == null)
                {
                    _configureLookupCommand = new RelayCommand(p => ConfigureLookup(p));
                }

                return _configureLookupCommand;
            }
        }

        public ICommand DeleteLookupCommand
        {
            get
            {
                if (_deleteLookupCommand == null)
                {
                    _deleteLookupCommand = new RelayCommand(p => DeleteLookup(p));
                }

                return _deleteLookupCommand;
            }
        }

        //Private methods
        private void InitializeData()
        {
            if (!string.IsNullOrEmpty(LookupXml))
            {
                CustomActivityDomain.LookupConfigurationInfo lookupConfiguration = (CustomActivityDomain.LookupConfigurationInfo)UtilsSerializer.Deserialize<CustomActivityDomain.LookupConfigurationInfo>(LookupXml);

                if (lookupConfiguration == null)
                {
                    LookupConfiguration = new LookupConfigurationModel();
                }

                LookupConfiguration = Mapper.GetLookupConfigurationModel(lookupConfiguration);
            }
            else
            {
                LookupConfiguration = new LookupConfigurationModel();
            }

            foreach (LookupInfoModel info in LookupConfiguration.BatchLookups)
            {
                var lookupField = BatchType.Fields.SingleOrDefault(p => p.Id == info.FieldId);

                if (lookupField != null)
                {
                    lookupField.IsLookup = true;
                }
            }

            foreach (DocumentTypeModel docType in BatchType.DocTypes)
            {
                var docLookup = LookupConfiguration.DocumentLookups.SingleOrDefault(p => p.DocumentTypeId == docType.Id);

                var documentFieldViewModel = new DocumentFieldViewModel
                {
                    DocumentType = docType,
                    DocumentLookupInfo = docLookup == null ? new DocumentLookupInfoModel { DocumentTypeId = docType.Id } : docLookup
                };

                foreach (LookupInfoModel info in documentFieldViewModel.DocumentLookupInfo.LookupInfos)
                {
                    var docLookupField = documentFieldViewModel.DocumentType.Fields.SingleOrDefault(p => p.Id == info.FieldId);

                    if (docLookupField != null)
                    {
                        docLookupField.IsLookup = true;
                    }
                }

                DocumentTypeFieldViewModels.Add(documentFieldViewModel);
            }

        }

        private void DeleteLookup(object para)
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("msgConfirmDeleteLookup")) == DialogServiceResult.Yes)
            {
                var field = para as FieldModel;
                var deletingLookup = LookupConfiguration.BatchLookups.SingleOrDefault(p => p.FieldId == field.Id);

                if (deletingLookup != null)
                {
                    _deleteLookupClient.Add(field.Id);
                    LookupConfiguration.BatchLookups.Remove(deletingLookup);

                    var lookupField = BatchType.Fields.SingleOrDefault(p => p.Id == deletingLookup.FieldId);

                    if (lookupField != null)
                    {
                        lookupField.IsLookup = false;
                    }

                }
            }
        }

        private void ConfigureLookup(object para)
        {
            var field = para as FieldModel;
            var lookupInfo = LookupConfiguration.BatchLookups.SingleOrDefault(p => p.FieldId == field.Id);

            LookupInfoModel workingLookupInfoModel;
            if (lookupInfo == null)
            {
                workingLookupInfoModel = new LookupInfoModel();
                workingLookupInfoModel.FieldId = field.Id;
            }
            else
            {
                workingLookupInfoModel = (LookupInfoModel)lookupInfo.Clone();
            }

            var viewModel = new LookupConfigurationViewModel(workingLookupInfoModel, BatchType.Fields, field.IsLookup);

            viewModel.CloseDialog += CloseLookupDialog;
            viewModel.SaveLookupComplete = SaveConfigurationComplete;

            var view = new LookupConfigurationView(viewModel);

            _dialog = new DialogViewer();
            _dialog.Width = 800;
            _dialog.Height = 700;
            _dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            _dialog.MaximizeBox = false;
            _dialog.MinimizeBox = false;
            _dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            _dialog.Text = _resource.GetString("uiDialogTitle");
            view.Dialog = _dialog;
            _dialog.WpfContent = view;
            _dialog.ShowDialog();
        }

        private void CloseLookupDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
                _dialog = null;
            }
        }

        private bool CanSave()
        {
            return (BatchType != null && BatchType.Fields != null && BatchType.Fields.Count > 0) ||
                (DocumentTypeFieldViewModels != null && DocumentTypeFieldViewModels.Count > 0);
        }

        private void Save()
        {
            if (DocumentTypeFieldViewModels != null && DocumentTypeFieldViewModels.Count > 0)
            {
                foreach (var documentLookupViewModel in DocumentTypeFieldViewModels)
                {
                    if (documentLookupViewModel.DocumentType.Fields.Any(p => p.IsLookup))
                    {
                        var existedLookup = LookupConfiguration.DocumentLookups.SingleOrDefault(p => p.DocumentTypeId == documentLookupViewModel.DocumentType.Id);

                        if (existedLookup == null)
                        {
                            LookupConfiguration.DocumentLookups.Add(documentLookupViewModel.DocumentLookupInfo);
                        }
                        else
                        {
                            existedLookup = documentLookupViewModel.DocumentLookupInfo;
                        }

                        foreach (var lookupInfoModel in documentLookupViewModel.DocumentLookupInfo.LookupInfos)
                        {
                            CustomActivityDomain.LookupInfo lookupInfo = Mapper.GetLookupInfo(lookupInfoModel);

                            if (lookupInfoModel.ApplyLookupClient)
                            {
                                LookupDomain.LookupInfo lookupInfoDomain = GetLookupInfoDomain(lookupInfo);
                                lookupInfoDomain.ActivityId = ActivityId;
                                _lookupProvider.UpdateDocumentLookup(lookupInfo.FieldId,
                                                                     UtilsSerializer.Serialize<LookupDomain.LookupInfo>(lookupInfoDomain),
                                                                     _user,
                                                                     new Nullable<Guid>(ActivityId));
                            }
                            else
                            {
                                _lookupProvider.UpdateDocumentLookup(lookupInfo.FieldId, string.Empty, _user, null);
                            }
                        }

                    }

                    if (documentLookupViewModel.DeletedLookupIds != null)
                    {
                        foreach (Guid id in documentLookupViewModel.DeletedLookupIds)
                        {
                            _lookupProvider.UpdateDocumentLookup(id, string.Empty, _user, null);
                        }
                    }

                }
            }

            foreach (var lookupInfoModel in LookupConfiguration.BatchLookups)
            {
                CustomActivityDomain.LookupInfo lookupInfo = Mapper.GetLookupInfo(lookupInfoModel);
                if (lookupInfoModel.ApplyLookupClient)
                {
                    LookupDomain.LookupInfo lookupInfoDomain = GetLookupInfoDomain(lookupInfo);
                    lookupInfoDomain.ActivityId = ActivityId;
                    _lookupProvider.UpdateBatchLookup(lookupInfo.FieldId, UtilsSerializer.Serialize<LookupDomain.LookupInfo>(lookupInfoDomain), _user, new Nullable<Guid>(ActivityId));
                }
                else
                {
                    _lookupProvider.UpdateBatchLookup(lookupInfo.FieldId, string.Empty, _user, null);
                }
            }

            foreach (Guid id in _deleteLookupClient)
            {
                _lookupProvider.UpdateBatchLookup(id, string.Empty, _user, null);
            }

            CustomActivityDomain.LookupConfigurationInfo lookup = Mapper.GetLookupConfiguration(LookupConfiguration);
            LookupXml = UtilsSerializer.Serialize<CustomActivityDomain.LookupConfigurationInfo>(lookup);

            if (SaveCompleted != null)
            {
                SaveCompleted();
            }
        }


        private void SaveConfigurationComplete(LookupInfoModel lookupInfo)
        {
            var isExisted = false;
            for (int i = 0; i < LookupConfiguration.BatchLookups.Count; i++)
            {
                if (LookupConfiguration.BatchLookups[i].FieldId == lookupInfo.FieldId)
                {
                    isExisted = true;
                    LookupConfiguration.BatchLookups[i] = lookupInfo;
                    break;
                }
            }

            if (!isExisted)
            {
                LookupConfiguration.BatchLookups.Add(lookupInfo);
            }

            if (_dialog != null)
            {
                _dialog.Close();
            }

            var lookupField = BatchType.Fields.SingleOrDefault(p => p.Id == lookupInfo.FieldId);

            if (lookupField != null)
            {
                lookupField.IsLookup = true;
            }

        }

        private LookupDomain.LookupInfo GetLookupInfoDomain(CustomActivityDomain.LookupInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new LookupDomain.LookupInfo
            {
                ConnectionInfo = new LookupDomain.ConnectionInfo
                {
                    DatabaseName = info.LookupConnection.DatabaseName,
                    DbType = (LookupDomain.DatabaseType)info.LookupConnection.DbType,
                    Host = info.LookupConnection.Host,
                    Password = info.LookupConnection.Password,
                    Port = info.LookupConnection.Port,
                    ProviderType = (LookupDomain.ProviderType)info.LookupConnection.ProviderType,
                    Schema = info.LookupConnection.Schema,
                    Username = info.LookupConnection.Username
                },
                FieldId = info.FieldId,
                LookupObjectName = info.SourceName,
                LookupType = (LookupDomain.LookupType)info.LookupType,
                LookupWhenTabOut = info.LookupAtLostFocus,
                MinPrefixLength = info.MinPrefixLength,
                QueryCommand = info.SqlCommand,
                LookupMapping = GetMappings(info.Mappings),
                RuntimeMappingInfo = GetMappings(info.Mappings),
                MaxLookupRow = info.MaxLookupRow,
                LookupMaps = info.Mappings.Select(GetLookupMap).ToList(),
                LookupColumn = info.LookupColumn,
                LookupOperator = info.LookupOperator
            };
        }

        private LookupMap GetLookupMap(LookupMapping mapping)
        {
            if (mapping == null)
            {
                return null;
            }

            return new LookupMap
            {
                DataColumn = mapping.DataColumn,
                FieldId = mapping.FieldId,
                FieldName = mapping.FieldName
            };
        }

        private List<string> GetMappings(List<LookupMapping> mappings)
        {
            List<string> mappingStrings = new List<string>();
            foreach (LookupMapping mapping in mappings)
            {
                mappingStrings.Add(mapping.FieldName);
            }

            return mappingStrings;
        }

    }
}
