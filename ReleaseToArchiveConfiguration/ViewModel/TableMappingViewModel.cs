using Ecm.Model;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArchiveModel = Ecm.Model;
using CaptureModel = Ecm.CaptureModel;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ViewModel
{
    public class TableMappingViewModel : ComponentViewModel
    {
        private MappingFieldModel _mappingField;
        private ArchiveModel.DocumentTypeModel _selectedDocumentType;
        private CaptureModel.DocTypeModel _selectedCaptureDocumentType;
        private RelayCommand _okCommand;
        private RelayCommand _CancelCommand;
        private ObservableCollection<TableColumnModel> _ArchiveChildrenField;
        private ObservableCollection<CaptureModel.TableColumnModel> _CaptureChildrenField;

        public TableMappingViewModel(MappingFieldModel mappingField, CaptureModel.DocTypeModel selectedCaptureDocumentType, ArchiveModel.DocumentTypeModel selectedDocumentType)
        {
            _mappingField = mappingField;
            SelectedCaptureDocumentType = selectedCaptureDocumentType;
            SelectedDocumentType = selectedDocumentType;
            LoadMappingFieldData();
        }

        public Action CloseDialog { get; set; }

        public MappingFieldModel MappingField
        {
            get { return _mappingField; }
            set
            {
                _mappingField = value;
                OnPropertyChanged("MappingField");
            }
        }
        public CaptureModel.DocTypeModel SelectedCaptureDocumentType
        {
            get { return _selectedCaptureDocumentType; }
            set
            {
                _selectedCaptureDocumentType = value;
                OnPropertyChanged("SelectedCaptureDocumentType");
            }
        }
        public ArchiveModel.DocumentTypeModel SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");
            }
        }

        public ObservableCollection<TableColumnModel> ArchiveChildrenField
        {
            get { return _ArchiveChildrenField; }
            set
            {
                _ArchiveChildrenField = value;
                OnPropertyChanged("ArchiveChildrenField");
            }
        }

        public ObservableCollection<CaptureModel.TableColumnModel> CaptureChildrenField
        {
            get { return _CaptureChildrenField; }
            set
            {
                _CaptureChildrenField = value;
                OnPropertyChanged("CaptureChildrenField");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(Save, CanSave);
                }

                return _okCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(Cancel);
                }

                return _CancelCommand;
            }
        }

        private void LoadMappingFieldData()
        {
            ObservableCollection<MappingFieldModel> mappingFields = new ObservableCollection<MappingFieldModel>();

            ArchiveChildrenField = new ObservableCollection<TableColumnModel>(SelectedDocumentType.Fields.SingleOrDefault(q => q.Id == MappingField.ArchiveFieldId).Children.OrderBy(h => h.ColumnName).ToList());
            ArchiveChildrenField.Insert(0, new TableColumnModel() { ColumnName = string.Empty, FieldId = Guid.Empty });
            List<CaptureModel.TableColumnModel> availableFields = SelectedCaptureDocumentType.Fields.Where(p => !p.IsSystemField).FirstOrDefault(p => p.Id == MappingField.CaptureFieldId).Children.ToList();

            CaptureChildrenField = new ObservableCollection<CaptureModel.TableColumnModel>(availableFields);

            foreach (CaptureModel.TableColumnModel captureField in availableFields)
            {
                MappingFieldModel mappingFieldModel = new MappingFieldModel();

                var archiveField = MappingField.ColumnMappings.FirstOrDefault(q => q.CaptureFieldId == captureField.FieldId);

                if (archiveField == null)
                {
                    mappingFieldModel.ArchiveFieldId = Guid.Empty;
                    mappingFieldModel.ArchiveField = string.Empty;
                }
                else
                {
                    mappingFieldModel.ArchiveField = archiveField.ArchiveField;
                    mappingFieldModel.ArchiveFieldId = archiveField.ArchiveFieldId;
                }
                
                mappingFieldModel.CaptureField = captureField.ColumnName;
                mappingFieldModel.CaptureFieldId = captureField.FieldId;

                mappingFields.Add(mappingFieldModel);
            }

            MappingField.ColumnMappings = mappingFields;
        }

        private void Cancel(object obj)
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

        private bool CanSave(object obj)
        {
            return MappingField.ColumnMappings.Any(p => !string.IsNullOrEmpty(p.ArchiveField));
        }

        private void Save(object obj)
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

    }
}
