using System.Collections.ObjectModel;
using System.Linq;
using Ecm.Mvvm;
using System;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class MenuItemModel : BaseDependencyProperty
    {
        private bool _isSelected;
        private ObservableCollection<MenuItemModel> _menuItems;
        private bool _isExpand;

        public string DisplayText { get; set; }

        public Guid Id { get; set; }

        public MenuItemType Type { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
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

        public ObservableCollection<MenuItemModel> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                OnPropertyChanged("MenuItems");
            }
        }

        public MenuItemModel Parent { get; set; }

    }
}
