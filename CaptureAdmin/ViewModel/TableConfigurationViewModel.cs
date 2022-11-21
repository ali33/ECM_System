using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Ecm.Mvvm;
using Ecm.CaptureModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class TableConfigurationViewModel : ComponentViewModel
    {
        public event EventHandler ColumnChanged;
        private readonly ObservableCollection<TableColumnViewModel> _columnItems = new ObservableCollection<TableColumnViewModel>();
        private List<Guid> _removeColumns = new List<Guid>();
        private RelayCommand _addColumnCommand;
        private RelayCommand _closeCommand;
        private RelayCommand _saveCommand;
        private FieldModel _parentField;


        public Action CloseDialog { get; set; }

        public TableConfigurationViewModel(FieldModel field)
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

        public ICommand SaveTableCommand
        {
            get
            {
                return _addColumnCommand ?? new RelayCommand(SaveTable, CanSaveTable);
            }
        }

        private bool CanSaveTable(object obj)
        {
            return !_columnItems.Any(h => h.HasError);
        }

        private void SaveTable(object obj)
        {
            //_parentField.Children = _columnItems;
            var listColumn = new List<TableColumnModel>(_columnItems.Count);

            foreach (var tableColumn in _columnItems)
            {
                listColumn.Add(new TableColumnModel()
                {
                    ColumnGuid = tableColumn.ColumnGuid,
                    ColumnName = tableColumn.ColumnName.Trim(),
                    DataType = tableColumn.DataType,
                    DefaultValue = tableColumn.DefaultValue,
                    MaxLength = tableColumn.MaxLength.HasValue ? tableColumn.MaxLength.Value : 0,
                    UseCurrentDate = tableColumn.UseCurrentDate,
                    ParentFieldId = tableColumn.ParentFieldId,
                    FieldId = tableColumn.FieldId,
                    DocTypeId = tableColumn.DocTypeId,
                    IsRestricted = tableColumn.IsRestricted,
                    IsRequired = tableColumn.IsRequired
                });
            }

            _parentField.Children = new ObservableCollection<TableColumnModel>(listColumn);
            Close();
        }

        private void Close()
        {
            CloseDialog();
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
                    TableColumnViewModel column = new TableColumnViewModel(CheckExistedColumnName)
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
                        IsRestricted = tableColumn.IsRestricted,
                        IsRequired = tableColumn.IsRequired
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
                        MaxLength = column.MaxLength.HasValue ? column.MaxLength.Value : 0,
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
            TableColumnViewModel tableColumn = new TableColumnViewModel(CheckExistedColumnName)
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
            TableColumnViewModel tableColumn = new TableColumnViewModel(CheckExistedColumnName)
            {
                ColumnGuid = Guid.NewGuid(),
                ColumnName = "New column"
            };

            tableColumn.RequestRemove += OnRemoveColumn;
            tableColumn.PropertyChanged += TableColumnPropertyChanged;
            _columnItems.Add(tableColumn);
        }

        private bool CheckExistedColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            var count = _columnItems.Count(h => columnName.Equals(string.Format("{0}",h.ColumnName).Trim(), StringComparison.CurrentCultureIgnoreCase));
            if (count > 1)
            {
                return true;
            }

            return false;
        }
    }
}
