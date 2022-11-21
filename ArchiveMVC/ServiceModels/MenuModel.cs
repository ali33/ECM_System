using Ecm.Mvvm;
using System.Collections.Generic;


namespace ArchiveMVC.Models
{
    public class MenuModel 
    {
        private string _menuText;
        private string _imageUrl;
        private bool _isSelected;
        private ComponentViewModel _viewModel;
        private List<MenuModel> _menuItems = new List<MenuModel>();
        private string _pageTitle;
        public MenuModel()
        {
        }

        public MenuModel(string text, string imagePath, ComponentViewModel viewModel, bool isSelected, string pageTitle) 
        {
            _menuText = text;
            _imageUrl = imagePath;
            _isSelected = isSelected;
            _viewModel = viewModel;
            _pageTitle = pageTitle;
        }

        public ComponentViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
        }

        public string MenuText
        {
            get { return _menuText; }
            set
            {
                _menuText = value;
            }
        }

        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
            }
        }

        public List<MenuModel> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
            }
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
            }
        }

        public int Id { get; set; }

        public bool IsUserGroup { get; set; }

        public bool IsDocType { get; set; }

        public int ParentId { get; set; }

    }
}
