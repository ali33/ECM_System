using System;
using Ecm.Mvvm;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ecm.ContentViewer.Model
{
    public class BarcodeConfigurationModel : BaseDependencyProperty, IDataErrorInfo
    {
        private ObservableCollection<ReadActionModel> _readActions;
        private ObservableCollection<SeparationActionModel> _separationActions;

        public Guid Id { get; set; }

        public Action<BarcodeConfigurationModel> ErrorChecked { get; set; }

        public Guid InternalId { get; set; }

        public string CheckErrorProperty { get; set; }

        public BarcodeConfigurationModel()
        {
            ReadActions = new ObservableCollection<ReadActionModel>();
            SeparationActions = new ObservableCollection<SeparationActionModel>();
        }

        public BarcodeConfigurationModel(Action<BarcodeConfigurationModel> action)
        {
            ErrorChecked = action;
        }

        public ObservableCollection<ReadActionModel> ReadActions
        {
            get { return _readActions; }
            set
            {
                _readActions = value;
                OnPropertyChanged("ReadActions");
            }
        }

        public ObservableCollection<SeparationActionModel> SeparationActions
        {
            get { return _separationActions; }
            set
            {
                _separationActions = value;
                OnPropertyChanged("SeparationActions");
            }
        }

        public bool HasError { get; set; }

        public Dictionary<string, string> ErrorMessages { get; set; }

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get 
            {
                
                HasError = false;
                string msg = string.Empty;
                CheckErrorProperty = columnName;
                if (columnName == "BarcodeType")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasError)
                    {
                        msg = ErrorMessages["uiDupplicateBarcodeConfig"];
                    }
                }

                if (columnName == "BarcodePosition")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }
                    if (HasError)
                    {
                        msg = ErrorMessages["uiDupplicateBarcodeConfig"];
                    }
                }

                if (columnName == "MapValueToFieldId")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }
                    if (HasError)
                    {
                        msg = ErrorMessages["uiFieldAlreadyMap"];
                    }

                }

                HasError = !string.IsNullOrEmpty(msg);
                return msg;
            }
        }

        #endregion
    }
}
