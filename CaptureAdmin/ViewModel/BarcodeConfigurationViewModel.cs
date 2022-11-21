using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System.Reflection;
using System.Resources;
using Ecm.BarcodeDomain;
using Ecm.CaptureModel;
using Ecm.CaptureAdmin.View;
using Ecm.CaptureModel.DataProvider;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class BarcodeConfigurationViewModel : ComponentViewModel
    {
        private bool _isModified;
        private readonly BatchTypeModel _batchType;
        private readonly List<DocTypeModel> _docTypes;
        private const string _unknown = "UNKNOWN (*)";
        private BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.BarcodeConfigurationView", Assembly.GetExecutingAssembly());

        private RelayCommand _saveCommand;
        private RelayCommand _addSeparationActionCommand;
        private RelayCommand _editSeparationActionCommand;
        private RelayCommand _deleteSeparationActionCommand;
        private RelayCommand _addReadActionCommand;
        private RelayCommand _editReadActionCommand;
        private RelayCommand _deleteReadActionCommand;

        private SeparationActionModel _selectedSeparationAction;
        private ReadActionModel _selectedReadAction;
        private BarcodeConfigurationModel _currentBarcodeConfiguration;
        //private string _xml;

        public BarcodeConfigurationViewModel(BatchTypeModel batchType, List<DocTypeModel> docTypes)
        {
            _batchType = batchType;
            _docTypes = docTypes;
            //_xml = batchType.BarcodeConfigurationXml;
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

        public BarcodeConfigurationModel CurrentBarcodeConfiguration
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
            return IsModified || (CurrentBarcodeConfiguration.SeparationActions.Any() || CurrentBarcodeConfiguration.ReadActions.Any());
        }

        public override void Initialize()
        {
            if (_batchType.BarcodeConfiguration != null)
            {
                CurrentBarcodeConfiguration = _batchType.BarcodeConfiguration;

            }
            else
            {
                CurrentBarcodeConfiguration = new BarcodeConfigurationModel();
            }

            if (CurrentBarcodeConfiguration != null)
            {
                BuildReadActionModels(CurrentBarcodeConfiguration.ReadActions);
                BuildSeparationActionModels(CurrentBarcodeConfiguration.SeparationActions);
            }
        }

        //Private methods

        private void SaveBarcodeConfiguration()
        {
            // 2015/05/06 - HungLe - Start
            // Adding code to fix when delete all barcode not generate barcode config xml

            /* Old block code
            //List<SeparationAction> separationActions = _separationActionModels.Select(p => GetSeparationAction(p)).ToList();
            //List<ReadAction> readActions = _readActionModels.Select(p => GetReadAction(p)).ToList();
            BatchBarcodeConfiguration barcodeConfig = ObjectMapper.GetBarcodeConfiguration(CurrentBarcodeConfiguration);
            BarcodeConfigurationXml = Utility.UtilsSerializer.Serialize<BatchBarcodeConfiguration>(barcodeConfig);
            */

            BarcodeConfigurationXml = null;
            if (CurrentBarcodeConfiguration != null)
            {
                var kindOfBarcode = 2;
                if (CurrentBarcodeConfiguration.ReadActions == null
                    || CurrentBarcodeConfiguration.ReadActions.Count == 0)
                {
                    kindOfBarcode--;
                }
                if (CurrentBarcodeConfiguration.SeparationActions == null
                    || CurrentBarcodeConfiguration.SeparationActions.Count == 0)
                {
                    kindOfBarcode--;
                }

                if (kindOfBarcode != 0)
                {
                    var barcodeConfig = ObjectMapper.GetBarcodeConfiguration(CurrentBarcodeConfiguration);
                    BarcodeConfigurationXml
                        = Utility.UtilsSerializer.Serialize<BatchBarcodeConfiguration>(barcodeConfig);
                }
            }
            // 2015/05/06 - HungLe - End

            try
            {
                _batchTypeProvider.SaveBarcodeConfiguration(BarcodeConfigurationXml, _batchType.Id);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void BuildSeparationActionModels(ObservableCollection<SeparationActionModel> separationActions)
        {
            foreach (SeparationActionModel separationAction in separationActions)
            {
                if (separationAction.HasSpecifyDocumentType)
                {
                    DocTypeModel docType = _docTypes.FirstOrDefault(p => p.Id == separationAction.DocTypeId);
                    separationAction.DocTypeName = docType != null ? docType.Name : _unknown;
                }
            }
        }

        private void BuildReadActionModels(ObservableCollection<ReadActionModel> readActions)
        {
            foreach (ReadActionModel readAction in readActions)
            {
                if (readAction.IsDocIndex)
                {
                    DocTypeModel docType = _docTypes.FirstOrDefault(p => p.Id == readAction.DocTypeId);
                    readAction.TargetTypeName = docType != null ? docType.Name : _unknown;

                    //    if (docType != null)
                    //    {
                    //        int maxPosition = -1;
                    //        if (readAction.CopyValueToFields.Count > 0)
                    //        {
                    //            maxPosition = readAction.CopyValueToFields.Max(p => p.Position);
                    //        }

                    //        for (int position = 0; position <= maxPosition; position++)
                    //        {
                    //            CopyValueToFieldModel toIndex = readAction.CopyValueToFields.FirstOrDefault(p => p.Position == position);
                    //            if (toIndex != null)
                    //            {
                    //                FieldModel field = docType.Fields.FirstOrDefault(p => p.UniqueId == toIndex.FieldGuid);
                    //                readAction.CopyValueToFields.Add(new CopyValueToFieldModel
                    //                {
                    //                    FieldGuid = toIndex.FieldGuid,
                    //                    FieldName = field != null ? field.Name : string.Empty,
                    //                    Position = toIndex.Position
                    //                });
                    //            }
                    //            else
                    //            {
                    //                readAction.CopyValueToFields.Add(new CopyValueToFieldModel
                    //                {
                    //                    FieldGuid = string.Empty,
                    //                    FieldName = string.Empty,
                    //                    Position = position
                    //                });
                    //            }
                    //        }
                    //    }
                }
                else
                {
                    readAction.TargetTypeName = _batchType.Name;

                    //    int maxPosition = -1;
                    //    if (readAction.CopyValueToFields.Count > 0)
                    //    {
                    //        maxPosition = readAction.CopyValueToFields.Max(p => p.Position);
                    //    }

                    //    for (int position = 0; position <= maxPosition; position++)
                    //    {
                    //        CopyValueToFieldModel toField = readAction.CopyValueToFields.FirstOrDefault(p => p.Position == position);
                    //        if (toField != null)
                    //        {
                    //            FieldModel field = _batchType.Fields.FirstOrDefault(p => p.UniqueId == toField.FieldGuid);
                    //            readAction.CopyValueToFields.Add(new CopyValueToFieldModel
                    //            {
                    //                FieldGuid = toField.FieldGuid,
                    //                FieldName = field != null ? field.Name : string.Empty,
                    //                Position = toField.Position
                    //            });
                    //        }
                    //        else
                    //        {
                    //            readAction.CopyValueToFields.Add(new CopyValueToFieldModel
                    //            {
                    //                FieldGuid = string.Empty,
                    //                FieldName = string.Empty,
                    //                Position = position
                    //            });
                    //        }
                    //    }
                }
            }
        }

        private void AddSeparationAction()
        {
            DialogBaseView dialog = new DialogBaseView
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
                CurrentBarcodeConfiguration.SeparationActions.Add(actionModel);
                IsModified = true;
            }
        }

        private bool CanEditSeparationAction()
        {
            return SelectedSeparationAction != null;
        }

        private void EditSeparationAction()
        {
            DialogBaseView dialog = new DialogBaseView
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
                int index = CurrentBarcodeConfiguration.SeparationActions.IndexOf(SelectedSeparationAction);
                CurrentBarcodeConfiguration.SeparationActions.RemoveAt(index);
                CurrentBarcodeConfiguration.SeparationActions.Insert(index, actionModel);
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
                CurrentBarcodeConfiguration.SeparationActions.Remove(SelectedSeparationAction);
                SelectedSeparationAction = null;
                IsModified = true;
            }
        }

        private void AddReadAction()
        {
            DialogBaseView dialog = new DialogBaseView
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
                CurrentBarcodeConfiguration.ReadActions.Add(actionModel);
                IsModified = true;
            }
        }

        private bool CanEditReadAction()
        {
            return SelectedReadAction != null;
        }

        private void EditReadAction()
        {
            DialogBaseView dialog = new DialogBaseView
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
                int index = CurrentBarcodeConfiguration.ReadActions.IndexOf(SelectedReadAction);
                CurrentBarcodeConfiguration.ReadActions.RemoveAt(index);
                CurrentBarcodeConfiguration.ReadActions.Insert(index, actionModel);
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
                CurrentBarcodeConfiguration.ReadActions.Remove(SelectedReadAction);
                SelectedReadAction = null;
                IsModified = true;
            }
        }

    }
}
