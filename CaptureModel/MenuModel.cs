using System.Collections.ObjectModel;
using Ecm.Mvvm;
using System;

namespace Ecm.CaptureModel
{
    public class MenuModel : BaseDependencyProperty 
    {
        private string _menuText;
        private string _imageUrl;
        private bool _isSelected;
        //private readonly ComponentViewModel _viewModel;
        private ObservableCollection<MenuModel> _menuItems = new ObservableCollection<MenuModel>();
        private string _pageTitle;

        public MenuModel()
        {
        }

        public MenuModel(string text, string imagePath, bool isSelected, string pageTitle, string menuName) 
        {
            _menuText = text;
            _imageUrl = imagePath;
            _isSelected = isSelected;
            //_viewModel = viewModel;
            _pageTitle = pageTitle;
            MenuName = menuName;
        }

        //public ComponentViewModel ViewModel
        //{
        //    get
        //    {
        //        return _viewModel;
        //    }
        //}

        public string MenuText
        {
            get { return _menuText; }
            set
            {
                _menuText = value;
                OnPropertyChanged("MenuText");
            }
        }

        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                OnPropertyChanged("ImageUrl");
            }
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

        public ObservableCollection<MenuModel> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                OnPropertyChanged("MenuItems");
            }
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnPropertyChanged("PageTitle");
            }
        }

        public Guid Id { get; set; }

        public bool IsUserGroup { get; set; }

        public bool IsDocType { get; set; }

        public Guid ParentId { get; set; }

        public string MenuName { get; set; }

    }
}
