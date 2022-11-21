using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.Model;
using System.Windows.Input;

namespace Ecm.Admin.ViewModel
{
    public class FieldListViewModel : ComponentViewModel
    {
        public FieldListViewModel()
        {
            _fields = new ObservableCollection<FieldMetaDataModel>();
        }

        private FieldMetaDataModel _selectedField;

        private ObservableCollection<FieldMetaDataModel> _fields;

        public ObservableCollection<FieldMetaDataModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        public FieldMetaDataModel SelectedField
        {
            get { return _selectedField; }
            set
            {
                _selectedField = value;
                OnPropertyChanged("SelectedField");
            }
        }

        #region lookup

        private RelayCommand _lookupCommand;
        private RelayCommand _deleteLookupCommand;
        private LookupViewModel _lookupViewModel;
        private Ecm.Admin.View.DialogBaseView _dialog;

        //public ICommand LookupCommand
        //{
        //    get { return _lookupCommand ?? (_lookupCommand = new RelayCommand(Lookup)); }
        //}

        public ICommand DeleteLookupCommand
        {
            get { return _deleteLookupCommand ?? (_deleteLookupCommand = new RelayCommand(DeleteLookup)); }
        }

        //private void Lookup(object sender)
        //{
        //    FieldMetaDataModel field = sender as FieldMetaDataModel;
        //    bool isEdit;

        //    if (field != null)
        //    {
        //        SelectedField = field;

        //        if (SelectedField.LookupInfo == null || SelectedField.Maps == null || SelectedField.Maps == null)
        //        {
        //            SelectedField.LookupInfo = new LookupInfoModel();
        //            SelectedField.Maps = new ObservableCollection<LookupMapModel>();
        //            isEdit = false;
        //        }
        //        else
        //        {
        //            isEdit = true;
        //        }

        //        _lookupViewModel = new LookupViewModel(SaveLookup, _fields, SelectedField, isEdit);
        //        _lookupViewModel.CloseDialog += new CloseDialog(ViewModelCloseDialog);

        //        _dialog = new Ecm.Admin.View.DialogBaseView(new Ecm.Admin.View.LookupConfigurationView(_lookupViewModel));
        //        _dialog.Size = new System.Drawing.Size(700, 600);
        //        _dialog.Text = "Database lookup configuration";
        //        _dialog.ShowDialog();
        //    }
        //}

        //private void SaveLookup(FieldMetaDataModel field)
        //{
        //    SelectedField = field;
        //    FieldMetaDataModel editField = _fields.FirstOrDefault(f => f.Name == SelectedField.Name);

        //    if (!_lookupViewModel.IsEditMode)
        //    {
        //        editField.Maps = field.Maps;
        //        editField.LookupInfo = field.LookupInfo;
        //    }
        //    else
        //    {
        //        foreach (var lookupMap in field.Maps)
        //        {
        //            if (lookupMap.Id == Guid.Empty)
        //            {
        //                editField.Maps.Add(lookupMap);
        //            }
        //            else
        //            {
        //                LookupMapModel map = editField.Maps.FirstOrDefault(p => p.Id == lookupMap.Id);
        //                map = lookupMap;
        //            }
        //        }

        //        editField = SelectedField;
        //        editField.IsLookup = true;
        //    }

        //    if (_dialog != null)
        //    {
        //        _dialog.Close();
        //    }
        //}

        private void DeleteLookup(object sender)
        {
            FieldMetaDataModel field = sender as FieldMetaDataModel;
            SelectedField = field;
            SelectedField.LookupInfo = null;
            SelectedField.Maps = null;
            SelectedField.IsLookup = false;
        }

        void ViewModelCloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        #endregion
    }
}
