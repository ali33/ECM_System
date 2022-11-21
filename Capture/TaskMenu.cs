using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureDomain;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.CaptureModel;

namespace Ecm.Capture
{
    public class TaskMenu : BaseDependencyProperty
    {
        private BatchTypeModel _batchType;
        private bool _isSelected;
        private byte[] _icon;
        private ObservableCollection<TaskMenuItem> _menuItems;
        private bool _isExpand;

        public TaskMenu()
        {
            MenuItems = new ObservableCollection<TaskMenuItem>();
        }

        public BatchTypeModel BatchType
        {
            get { return _batchType; }
            set
            {
                _batchType = value;
                OnPropertyChanged("BatchType");
            }
        }

        public ObservableCollection<TaskMenuItem> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                OnPropertyChanged("MenuItems");
            }
        }

        public string Name
        {
            get { return _batchType.Name; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public byte[] Icon
        {
            get { return BatchType.Icon; }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        public bool IsExpand
        {
            get { return _isExpand; }
            set
            {
                _isExpand = value;
                OnPropertyChanged("IsExpand");
            }
        }
    }
}
