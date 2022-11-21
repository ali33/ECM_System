using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.Workflow.Activities.CustomActivityDomain;
using Ecm.Workflow.Activities.BarcodeExecutorDesigner.View;
using System.Reflection;
using System.Resources;
using Ecm.BarcodeDomain;

namespace Ecm.Workflow.Activities.BarcodeExecutorDesigner.ViewModel
{
    public class BarcodeConfigurationViewModel : ComponentViewModel
    {
        private bool _isModified;
        private bool _transferBarcodeToClientProcessing;
        private readonly BatchType _batchType;
        private readonly List<DocumentType> _docTypes;
        private const string _unknown = "UNKNOWN (*)";
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.BarcodeExecutorDesigner.Resource", Assembly.GetExecutingAssembly());

        private RelayCommand _saveCommand;
        private RelayCommand _addSeparationActionCommand;
        private RelayCommand _editSeparationActionCommand;
        private RelayCommand _deleteSeparationActionCommand;
        private RelayCommand _addReadActionCommand;
        private RelayCommand _editReadActionCommand;
        private RelayCommand _deleteReadActionCommand;

        private ObservableCollection<SeparationActionModel> _separationActionModels = new ObservableCollection<SeparationActionModel>();
        private ObservableCollection<ReadActionModel> _readActionModels = new ObservableCollection<ReadActionModel>();
        private SeparationActionModel _selectedSeparationAction;
        private ReadActionModel _selectedReadAction;
        private BatchBarcodeConfigurationModel _currentBarcodeConfiguration;
        private string _xml;

        public BarcodeConfigurationViewModel(BatchType batchType, List<DocumentType> docTypes, string xml)
        {
            _batchType = batchType;
            _docTypes = docTypes;
            _xml = xml;
            Initialize();
        }

        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                _isModified = value;
                OnPropertyChanged("IsModified");
            }
        }

        public bool TransferBarcodeToClientProcessing
        {
            get { return _transferBarcodeToClientProcessing; }
            set
            {
                _transferBarcodeToClientProcessing = value;
                OnPropertyChanged("TransferBarcodeToClientProcessing");
            }
        }

        public ObservableCollection<SeparationActionModel> SeparationActionModels
        {
            get { return _separationActionModels; }
            set
            {
                _separationActionModels = value;
                OnPropertyChanged("SeparationActionModels");
            }
        }

        public ObservableCollection<ReadActionModel> ReadActionModels
        {
            get { return _readActionModels; }
            set
            {
                _readActionModels = value;
                OnPropertyChanged("ReadActionModels");
            }
        }

        public SeparationActionModel SelectedSeparationAction
        {
            get { return _selectedSeparationAction; }
            set
            {
                _selectedSeparationAction = value;
                OnPropertyChanged("SelectedSeparationAction");
            }
        }

        public ReadActionModel SelectedReadAction
        {
            get { return _selectedReadAction; }
            set
            {
                _selectedReadAction = value;
                OnPropertyChanged("SelectedReadAction");
            }
        }

        public BatchBarcodeConfigurationModel CurrentBarcodeConfiguration
        {
            get { return _currentBarcodeConfiguration; }
            set
            {
                _currentBarcodeConfiguration = value;
                OnPropertyChanged("CurrentBarcodeConfiguration");
            }
        }

        public string BarcodeConfigurationXml { get; set; }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(p => SaveBarcodeConfiguration(), p => CanSaveBarcodeConfiguration()));
            }
        }

        public ICommand AddSeparationActionCommand
        {
            get { return _addSeparationActionCommand ?? (_addSeparationActionCommand = new RelayCommand(p => AddSeparationAction())); }
        }

        public ICommand EditSeparationActionCommand
        {
            get { return _editSeparationActionCommand ?? (_editSeparationActionCommand = new RelayCommand(p => EditSeparationAction(), p => CanEditSeparationAction())); }
        }

        public ICommand DeleteSeparationActionCommand
        {
            get { return _deleteSeparationActionCommand ?? (_deleteSeparationActionCommand = new RelayCommand(p => DeleteSeparationAction(), p => CanDeleteSeparationAction())); }
        }

        public ICommand AddReadActionCommand
        {
            get { return _addReadActionCommand ?? (_addReadActionCommand = new RelayCommand(p => AddReadAction())); }
        }

        public ICommand EditReadActionCommand
        {
            get { return _editReadActionCommand ?? (_editReadActionCommand = new RelayCommand(p => EditReadAction(), p => CanEditReadAction())); }
        }

        public ICommand DeleteReadActionCommand
        {
            get { return _deleteReadActionCommand ?? (_deleteReadActionCommand = new RelayCommand(p => DeleteReadAction(), p => CanDeleteReadAction())); }
        }

        private bool CanSaveBarcodeConfiguration()
        {
            return (IsModified || TransferBarcodeToClientProcessing) && (_separationActionModels.Any() || _readActionModels.Any());
        }

        public override void Initialize()
        {
            if (!string.IsNullOrEmpty(_xml))
            {
                BatchBarcodeConfiguration barcodeConfig = Ecm.Utility.UtilsSerializer.Deserialize<BatchBarcodeConfiguration>(_xml);
                TransferBarcodeToClientProcessing = barcodeConfig.TransferBarcodeToClientProcessing;
                if (barcodeConfig != null)
                {
                    BuildReadActionModels(barcodeConfig.ReadActions);
                    BuildSeparationActionModels(barcodeConfig.SeparationActions);
                }
            }
            else
            {
                BuildReadActionModels(new List<ReadAction>());
                BuildSeparationActionModels(new List<SeparationAction>());
            }
        }

        //Private methods

        private BatchBarcodeConfigurationModel GetBarcodeConfigurationModel(BatchBarcodeConfiguration barcodeConfig)
        {
            if (barcodeConfig == null)
            {
                return null;
            }

            return new BatchBarcodeConfigurationModel
            {
                ReadActions = barcodeConfig.ReadActions.Select(p => GetReadActionModel(p)).ToList(),
                SeparationActions = barcodeConfig.SeparationActions.Select(p => GetSeparationActionModel(p)).ToList()
            };
        }

        private ReadActionModel GetReadActionModel(ReadAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new ReadActionModel
            {
                BarcodePositionInDoc = action.BarcodePositionInDoc,
                BarcodeType = (BarcodeTypeModel)action.BarcodeType,
                CopyValueToFields = action.CopyValueToFields.Select(p => GetCopyToFieldModel(p)).ToList(),
                DocTypeId = action.DocTypeId,
                Id = action.Id,
                IsDocIndex = action.IsDocIndex,
                OverwriteFieldValue = action.OverwriteFieldValue,
                Separator = action.Separator,
                StartsWith = action.StartsWith
            };
        }

        private CopyValueToFieldModel GetCopyToFieldModel(CopyValueToField copyToField)
        {
            if (copyToField == null)
            {
                return null;
            }

            return new CopyValueToFieldModel
            {
                FieldGuid = copyToField.FieldGuid,
                FieldName = copyToField.FieldName,
                Position = copyToField.Position
            };
        }

        private SeparationActionModel GetSeparationActionModel(SeparationAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new SeparationActionModel
            {
                BarcodePositionInDoc = action.BarcodePositionInDoc,
                BarcodeType = (BarcodeTypeModel)action.BarcodeType,
                DocTypeId = action.DocTypeId,
                Id = action.Id,
                StartsWith = action.StartsWith,
                HasSpecifyDocumentType = action.HasSpecifyDocumentType,
                RemoveSeparatorPage = action.RemoveSeparatorPage
            };
        }

        private BatchBarcodeConfiguration GetBarcodeConfiguration(BatchBarcodeConfigurationModel barcodeConfigModel)
        {
            if (barcodeConfigModel == null)
            {
                return null;
            }

            return new BatchBarcodeConfiguration
            {
                ReadActions = barcodeConfigModel.ReadActions.Select(p => GetReadAction(p)).ToList(),
                SeparationActions = barcodeConfigModel.SeparationActions.Select(p => GetSeparationAction(p)).ToList()
            };
        }

        private ReadAction GetReadAction(ReadActionModel actionModel)
        {
            if (actionModel == null)
            {
                return null;
            }

            return new ReadAction
            {
                BarcodePositionInDoc = actionModel.BarcodePositionInDoc,
                BarcodeType = (int)actionModel.BarcodeType,
                CopyValueToFields = actionModel.CopyValueToFields.Select(p => GetCopyToField(p)).ToList(),
                DocTypeId = actionModel.DocTypeId,
                Id = actionModel.Id,
                IsDocIndex = actionModel.IsDocIndex,
                OverwriteFieldValue = actionModel.OverwriteFieldValue,
                Separator = actionModel.Separator,
                StartsWith = actionModel.StartsWith
            };
        }

        private CopyValueToField GetCopyToField(CopyValueToFieldModel copyToFieldModel)
        {
            if (copyToFieldModel == null)
            {
                return null;
            }

            return new CopyValueToField
            {
                FieldGuid = copyToFieldModel.FieldGuid,
                FieldName = copyToFieldModel.FieldName,
                Position = copyToFieldModel.Position
            };
        }

        private SeparationAction GetSeparationAction(SeparationActionModel actionModel)
        {
            if (actionModel == null)
            {
                return null;
            }

            return new SeparationAction
            {
                BarcodePositionInDoc = actionModel.BarcodePositionInDoc,
                BarcodeType = (int)actionModel.BarcodeType,
                DocTypeId = actionModel.DocTypeId,
                Id = actionModel.Id,
                StartsWith = actionModel.StartsWith,
                HasSpecifyDocumentType = actionModel.HasSpecifyDocumentType,
                RemoveSeparatorPage = actionModel.RemoveSeparatorPage
            };
        }

        private void SaveBarcodeConfiguration()
        {
            List<SeparationAction> separationActions = _separationActionModels.Select(p => GetSeparationAction(p)).ToList();

            //separationActions.AddRange(_separationActionModels.Select(separationActionModel => new SeparationAction
            //                                                                                       {
            //                                                                                           Id = separationActionModel.Id,
            //                                                                                           BarcodePositionInDoc = separationActionModel.BarcodePositionInDoc,
            //                                                                                           BarcodeType = separationActionModel.BarcodeType,
            //                                                                                           DocTypeId = separationActionModel.DocTypeId,
            //                                                                                           RemoveSeparatorPage = separationActionModel.RemoveSeparatorPage,
            //                                                                                           StartsWith = separationActionModel.StartsWith,
            //                                                                                           HasSpecifyDocumentType = separationActionModel.HasSpecifyDocumentType
            //                                                                                       }));

            List<ReadAction> readActions = _readActionModels.Select(p => GetReadAction(p)).ToList();

            //foreach (ReadActionModel readActionModel in _readActionModels)
            //{
            //    ReadAction readAction = new ReadAction
            //                                {
            //                                    Id = readActionModel.Id,
            //                                    BarcodePositionInDoc = readActionModel.BarcodePositionInDoc,
            //                                    BarcodeType = (int)readActionModel.BarcodeType,
            //                                    IsDocIndex = readActionModel.IsDocIndex,
            //                                    DocTypeId = readActionModel.DocTypeId,
            //                                    StartsWith = readActionModel.StartsWith,
            //                                    Separator = readActionModel.Separator,
            //                                    OverwriteFieldValue = readActionModel.OverwriteFieldValue
            //                                };


            //    foreach (CopyValueToFieldModel fieldModel in readActionModel.CopyValueToFields)
            //    {
            //        if (fieldModel.FieldGuid != Guid.Empty)
            //        {
            //            readAction.CopyValueToFields.Add(new CopyValueToField
            //                                                  {
            //                                                      FieldGuid = fieldModel.FieldGuid,
            //                                                      Position = fieldModel.Position,
            //                                                      FieldName = fieldModel.FieldName
            //                                                  });
            //        }
            //    }

            //    readActions.Add(readAction);
            //}
            BatchBarcodeConfiguration barcodeConfig = new BatchBarcodeConfiguration
            {
                ReadActions = readActions,
                SeparationActions = separationActions,
                TransferBarcodeToClientProcessing = TransferBarcodeToClientProcessing
            };

            BarcodeConfigurationXml = Utility.UtilsSerializer.Serialize<BatchBarcodeConfiguration>(barcodeConfig);
        }

        private void BuildSeparationActionModels(IEnumerable<SeparationAction> separationActions)
        {
            foreach (SeparationAction separationAction in separationActions)
            {
                SeparationActionModel actionModel = new SeparationActionModel
                                                        {
                                                            BarcodePositionInDoc = separationAction.BarcodePositionInDoc,
                                                            BarcodeType = (BarcodeTypeModel)separationAction.BarcodeType,
                                                            DocTypeId = separationAction.DocTypeId,
                                                            Id = separationAction.Id,
                                                            HasSpecifyDocumentType = separationAction.HasSpecifyDocumentType,
                                                            RemoveSeparatorPage = separationAction.RemoveSeparatorPage,
                                                            StartsWith = separationAction.StartsWith
                                                        };

                if (separationAction.HasSpecifyDocumentType)
                {
                    DocumentType docType = _docTypes.FirstOrDefault(p => p.Id == separationAction.DocTypeId);
                    actionModel.DocTypeName = docType != null ? docType.Name : _unknown;
                }

                _separationActionModels.Add(actionModel);
            }
        }

        private void BuildReadActionModels(List<ReadAction> readActions)
        {
            foreach (ReadAction readAction in readActions)
            {
                ReadActionModel actionModel = new ReadActionModel
                                                  {
                                                      Id = readAction.Id,
                                                      OverwriteFieldValue = readAction.OverwriteFieldValue,
                                                      BarcodePositionInDoc = readAction.BarcodePositionInDoc,
                                                      BarcodeType = (BarcodeTypeModel)readAction.BarcodeType,
                                                      DocTypeId = readAction.DocTypeId,
                                                      IsDocIndex = readAction.IsDocIndex,
                                                      StartsWith = readAction.StartsWith,
                                                      Separator = readAction.Separator
                                                  };

                if (readAction.IsDocIndex)
                {
                    DocumentType docType = _docTypes.FirstOrDefault(p => p.Id == readAction.DocTypeId);
                    actionModel.TargetTypeName = docType != null ? docType.Name : _unknown;

                    if (docType != null)
                    {
                        int maxPosition = -1;
                        if (readAction.CopyValueToFields.Count > 0)
                        {
                            maxPosition = readAction.CopyValueToFields.Max(p => p.Position);
                        }

                        for (int position = 0; position <= maxPosition; position++)
                        {
                            CopyValueToField toIndex = readAction.CopyValueToFields.FirstOrDefault(p => p.Position == position);
                            if (toIndex != null)
                            {
                                DocumentFieldMetaData field = docType.Fields.FirstOrDefault(p => p.UniqueId == toIndex.FieldGuid);
                                actionModel.CopyValueToFields.Add(new CopyValueToFieldModel
                                {
                                    FieldGuid = toIndex.FieldGuid,
                                    FieldName = field != null ? field.Name : string.Empty,
                                    Position = toIndex.Position
                                });
                            }
                            else
                            {
                                actionModel.CopyValueToFields.Add(new CopyValueToFieldModel
                                {
                                    FieldGuid = string.Empty,
                                    FieldName = string.Empty,
                                    Position = position
                                });
                            }
                        }
                    }
                }
                else
                {
                    actionModel.TargetTypeName = _batchType.Name;

                    int maxPosition = -1;
                    if (readAction.CopyValueToFields.Count > 0)
                    {
                        maxPosition = readAction.CopyValueToFields.Max(p => p.Position);
                    }

                    for (int position = 0; position <= maxPosition; position++)
                    {
                        CopyValueToField toField = readAction.CopyValueToFields.FirstOrDefault(p => p.Position == position);
                        if (toField != null)
                        {
                            BatchFieldMetaData field = _batchType.Fields.FirstOrDefault(p => p.UniqueId == toField.FieldGuid);
                            actionModel.CopyValueToFields.Add(new CopyValueToFieldModel
                            {
                                FieldGuid = toField.FieldGuid,
                                FieldName = field != null ? field.Name : string.Empty,
                                Position = toField.Position
                            });
                        }
                        else
                        {
                            actionModel.CopyValueToFields.Add(new CopyValueToFieldModel
                            {
                                FieldGuid = string.Empty,
                                FieldName = string.Empty,
                                Position = position
                            });
                        }
                    }
                }

                _readActionModels.Add(actionModel);
            }
        }

        private void AddSeparationAction()
        {
            DialogViewer dialog = new DialogViewer
            {
                Width = 500,
                Height = 500,
                Text = _resource.GetString("uiAddSeparationDataTitle"),
                MaximizeBox = false,
                MinimizeBox = false,
                FormBorderStyle = FormBorderStyle.Fixed3D,
                StartPosition = FormStartPosition.CenterParent

            };

            SeparationActionModel actionModel = new SeparationActionModel
                                                    {
                                                        BarcodeType = BarcodeTypeModel.CODE39,
                                                        BarcodePositionInDoc = 0,
                                                        Id = Guid.NewGuid(),
                                                        StartsWith = string.Empty,
                                                        HasSpecifyDocumentType = true
                                                    };

            SeparationActionView actionView = new SeparationActionView { Dialog = dialog };
            actionView.Initialize(actionModel, _docTypes);
            dialog.WpfContent = actionView;

            if (dialog.ShowDialog() == DialogResult.Yes)
            {
                _separationActionModels.Add(actionModel);
                IsModified = true;
            }
        }

        private bool CanEditSeparationAction()
        {
            return SelectedSeparationAction != null;
        }

        private void EditSeparationAction()
        {
            DialogViewer dialog = new DialogViewer
            {
                Width = 500,
                Height = 500,
                Text = _resource.GetString("uiEditSeparationDataTitle"),
                MaximizeBox = false,
                MinimizeBox = false
            };

            SeparationActionModel actionModel = SelectedSeparationAction.Clone();

            SeparationActionView actionView = new SeparationActionView { Dialog = dialog };
            actionView.Initialize(actionModel, _docTypes);
            dialog.WpfContent = actionView;

            if (dialog.ShowDialog() == DialogResult.Yes)
            {
                SelectedSeparationAction.IsSelected = false;
                int index = _separationActionModels.IndexOf(SelectedSeparationAction);
                _separationActionModels.RemoveAt(index);
                _separationActionModels.Insert(index, actionModel);
                IsModified = true;
            }
        }

        private bool CanDeleteSeparationAction()
        {
            return SelectedSeparationAction != null;
        }

        private void DeleteSeparationAction()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                SelectedSeparationAction.IsSelected = false;
                _separationActionModels.Remove(SelectedSeparationAction);
                SelectedSeparationAction = null;
                IsModified = true;
            }
        }

        private void AddReadAction()
        {
            DialogViewer dialog = new DialogViewer
            {
                Width = 500,
                Height = 500,
                Text = _resource.GetString("uiActionDataTitle"),
                MaximizeBox = false,
                MinimizeBox = false, 
                FormBorderStyle = FormBorderStyle.Fixed3D,
                StartPosition = FormStartPosition.CenterParent
            };

            ReadActionModel actionModel = new ReadActionModel
                                              {
                                                  BarcodeType = BarcodeTypeModel.CODE39,
                                                  BarcodePositionInDoc = 0,
                                                  Id = Guid.NewGuid(),
                                                  IsDocIndex = false,
                                                  StartsWith = string.Empty,
                                                  Separator = string.Empty,
                                                  OverwriteFieldValue = true
                                              };

            ReadActionView actionView = new ReadActionView { Dialog = dialog };
            actionView.Initialize(actionModel, _docTypes, _batchType);
            dialog.WpfContent = actionView;

            if (dialog.ShowDialog() == DialogResult.Yes)
            {
                _readActionModels.Add(actionModel);
                IsModified = true;
            }
        }

        private bool CanEditReadAction()
        {
            return SelectedReadAction != null;
        }

        private void EditReadAction()
        {
            DialogViewer dialog = new DialogViewer
            {
                Width = 500,
                Height = 500,
                Text = _resource.GetString("uiEditActionDataTitle"),
                MaximizeBox = false,
                MinimizeBox = false
            };

            ReadActionModel actionModel = SelectedReadAction.Clone();

            ReadActionView actionView = new ReadActionView { Dialog = dialog };
            actionView.Initialize(actionModel, _docTypes, _batchType);
            dialog.WpfContent = actionView;

            if (dialog.ShowDialog() == DialogResult.Yes)
            {
                SelectedReadAction.IsSelected = false;
                int index = _readActionModels.IndexOf(SelectedReadAction);
                _readActionModels.RemoveAt(index);
                _readActionModels.Insert(index, actionModel);
                IsModified = true;
            }
        }

        private bool CanDeleteReadAction()
        {
            return SelectedReadAction != null;
        }

        private void DeleteReadAction()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                SelectedReadAction.IsSelected = false;
                _readActionModels.Remove(SelectedReadAction);
                SelectedReadAction = null;
                IsModified = true;
            }
        }

    }
}
