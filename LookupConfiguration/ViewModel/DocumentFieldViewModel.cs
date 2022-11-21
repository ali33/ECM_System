using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel;
using System.Windows.Input;
using Ecm.Workflow.Activities.LookupConfiguration.View;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.LookupConfiguration.ViewModel
{
    public class DocumentFieldViewModel : ComponentViewModel
    {
        private DocumentTypeModel _documentType;
        private RelayCommand _configureLookupCommand;
        private RelayCommand _deleteLookupCommand;

        private DialogViewer _dialog;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.BatchTypeView", Assembly.GetExecutingAssembly());

        public DocumentFieldViewModel()
        {
        }

        public DocumentTypeModel DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                OnPropertyChanged("DocumentType");
            }
        }

        public DocumentLookupInfoModel DocumentLookupInfo { get; set; }

        public List<Guid> DeletedLookupIds { get; set; }

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
        private void ConfigureLookup(object para)
        {
            var field = para as FieldModel;
            var lookupInfo = DocumentLookupInfo.LookupInfos.SingleOrDefault(p => p.FieldId == field.Id);

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

            var viewModel = new LookupConfigurationViewModel(workingLookupInfoModel, DocumentType.Fields, field.IsLookup);
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

        private void DeleteLookup(object para)
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("msgConfirmDeleteLookup")) == DialogServiceResult.Yes)
            {
                var field = para as FieldModel;
                var deletingLookup = DocumentLookupInfo.LookupInfos.SingleOrDefault(p => p.FieldId == field.Id);
                if (deletingLookup != null)
                {
                    if (DeletedLookupIds == null)
                    {
                        DeletedLookupIds = new List<Guid>();
                    }

                    DeletedLookupIds.Add(deletingLookup.FieldId);
                    DocumentLookupInfo.LookupInfos.Remove(deletingLookup);

                    var lookupField = DocumentType.Fields.SingleOrDefault(p => p.Id == deletingLookup.FieldId);

                    if (lookupField != null)
                    {
                        lookupField.IsLookup = false;
                    }

                }
            }
        }

        private void CloseLookupDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
                _dialog = null;
            }
        }

        private void SaveConfigurationComplete(LookupInfoModel lookupInfo)
        {
            var isExisted = false;
            for (int i = 0; i < DocumentLookupInfo.LookupInfos.Count; i++)
            {
                if (DocumentLookupInfo.LookupInfos[i].FieldId == lookupInfo.FieldId)
                {
                    isExisted = true;
                    DocumentLookupInfo.LookupInfos[i] = lookupInfo;
                    break;
                }
            }

            if (!isExisted)
            {
                DocumentLookupInfo.LookupInfos.Add(lookupInfo);
            }

            if (_dialog != null)
            {
                _dialog.Close();
            }

            var lookupField = DocumentType.Fields.SingleOrDefault(p => p.Id == lookupInfo.FieldId);

            if (lookupField != null)
            {
                lookupField.IsLookup = true;
            }
        }
    }
}
