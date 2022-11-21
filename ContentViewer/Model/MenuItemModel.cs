using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Ecm.ContentViewer.Model
{
    public class MenuItemModel : BaseDependencyProperty
    {
        private string _text;
        private List<MenuItemModel> _children;
        private BitmapImage _icon;
        private bool _isSeparator;
        private bool _isChecked;
        private bool _isCheckable;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public List<MenuItemModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        public BitmapImage Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        public bool IsSeparator
        {
            get { return _isSeparator; }
            set
            {
                _isSeparator = value;
                OnPropertyChanged("IsSeparator");
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        public bool IsCheckable
        {
            get { return _isCheckable; }
            set
            {
                _isCheckable = value;
                OnPropertyChanged("IsCheckable");
            }
        }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }
    }
}
