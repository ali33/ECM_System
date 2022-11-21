using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Ecm.Mvvm;
using Ecm.Model;

namespace Ecm.Admin.ViewModel
{
    public class TableConfigurationViewModel : ComponentViewModel
    {
        public event EventHandler ColumnChanged;
        private readonly ObservableCollection<TableColumnViewModel> _columnItems = new ObservableCollection<TableColumnViewModel>();
        private List<Guid> _removeColumns = new List<Guid>();
        private RelayCommand _addColumnCommand;
        private RelayCommand _closeCommand;
        private RelayCommand _okCommand;
        private FieldMetaDataModel _parentField;

        public Action<bool,TableConfigurationViewModel> CloseDialog { get; set; }

        public TableConfigurationViewModel(FieldMetaDataModel field)
        {
            _parentField = field;
        }

        public ICommand AddColumnCommand
        {
            get
            {
                if (_addColumnCommand == null)
                {
                    _addColumnCommand = new RelayCommand(p => AddColumn());
                }

                return _addColumnCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(p => Close());
                }

                return _closeCommand;
            }

        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => Ok(),p=>CanOk());
                }

                return _okCommand;
            }

        }

        private bool CanOk()
        {
            return _columnItems != null && _columnItems.Count > 0;
        }

        private void Ok()
        {
            CloseDialog(true, this);
        }

        private void Close()
        {
            CloseDialog(false, this);
        }

        public ObservableCollection<TableColumnViewModel> ColumnItems
        {
            get
            {
                return _columnItems;
            }
        }

        public void Load(List<TableColumnModel> tableColumns)
        {
            //_columnItems.Clear();

            if (tableColumns != null)
            {
                foreach (TableColumnModel tableColumn in tableColumns)
                {
                    TableColumnViewModel column = new TableColumnViewModel
                    {
                        ColumnGuid = tableColumn.ColumnGuid,
                        ColumnName = tableColumn.ColumnName,
                        DataType = tableColumn.DataType,
                        DefaultValue = tableColumn.DefaultValue,
                        MaxLength = tableColumn.MaxLength,
                        UseCurrentDate = tableColumn.UseCurrentDate,
                        ParentFieldId = tableColumn.ParentFieldId,
                        FieldId = tableColumn.FieldId,
                        DocTypeId = tableColumn.DocTypeId,
                        IsRequired = tableColumn.IsRequired,
                        IsRestricted = tableColumn.IsRestricted

                    };

                    column.RequestRemove += OnRemoveColumn;
                    column.PropertyChanged += TableColumnPropertyChanged;

                    _columnItems.Add(column);
                }
            }

            if (_columnItems.Count == 0)
            {
                // Add a blank item
                CreateBlankColumn();
            }
        }

        public void Reset()
        {
            _columnItems.Clear();
        }

        public List<TableColumnModel> GetTableColumns()
        {
            List<TableColumnModel> tableColumns = new List<TableColumnModel>();

            foreach (TableColumnViewModel column in _columnItems)
            {
                if (!string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    TableColumnModel tableColumn = new TableColumnModel
                    {
                        ColumnGuid = column.ColumnGuid,
                        ColumnName = column.ColumnName,
                        DataType = column.DataType,
                        DefaultValue = column.DefaultValue,
                        MaxLength = column.MaxLength,
                        UseCurrentDate = column.UseCurrentDate,
                        ParentFieldId = column.ParentFieldId,
                        FieldId = column.FieldId,
                        DocTypeId = column.DocTypeId,
                        IsRequired = column.IsRequired,
                        IsRestricted = column.IsRestricted
                    };

                    tableColumns.Add(tableColumn);
                }
            }

            return tableColumns;
        }

        public List<Guid> GetRemoveColumns()
        {
            return _removeColumns;
        }

        public bool HasError
        {
            get
            {
                if (_columnItems.Count == 0)
                {
                    return false;
                }

                return _columnItems.Any(p => p.HasError);
            }
        }

        private void AddColumn()
        {
            TableColumnViewModel tableColumn = new TableColumnViewModel
            {
                ColumnGuid = Guid.NewGuid(),
                ColumnName = "New column"
            };

            tableColumn.RequestRemove += OnRemoveColumn;
            tableColumn.PropertyChanged += TableColumnPropertyChanged;
            _columnItems.Add(tableColumn);

            if (ColumnChanged != null)
            {
                ColumnChanged(this, null);
            }
        }

        private void OnRemoveColumn(object sender, EventArgs e)
        {
            TableColumnViewModel tableColumn = sender as TableColumnViewModel;

            if (tableColumn.FieldId != Guid.Empty)
            {
                _removeColumns.Add(tableColumn.FieldId);
            }

            _columnItems.Remove(tableColumn);

            if (_columnItems.Count == 0)
            {
                CreateBlankColumn();
            }

            if (ColumnChanged != null)
            {
                ColumnChanged(this, null);
            }
        }

        private void TableColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataType")
            {
                if (ColumnChanged != null)
                {
                    ColumnChanged(this, null);
                }
            }
        }

        private void CreateBlankColumn()
        {
            TableColumnViewModel tableColumn = new TableColumnViewModel
            {
                ColumnGuid = Guid.NewGuid(),
                ColumnName = "New column"
            };

            tableColumn.RequestRemove += OnRemoveColumn;
            tableColumn.PropertyChanged += TableColumnPropertyChanged;
            _columnItems.Add(tableColumn);
        }

    }
}
