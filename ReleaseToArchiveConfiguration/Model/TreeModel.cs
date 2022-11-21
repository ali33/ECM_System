using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.Model
{
    public class TreeModel : BaseDependencyProperty
    {
        private string _menuText;
        private string _imageUrl;
        private bool _isSelected;
        private ComponentViewModel _viewModel;
        private ObservableCollection<TreeModel> _menuItems = new ObservableCollection<TreeModel>();
        private string _pageTitle;
        private bool _haveMapping;
        public TreeModel()
        {
        }

        public TreeModel(string text, string imagePath, ComponentViewModel viewModel, bool isSelected, string pageTitle)
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

        public ObservableCollection<TreeModel> MenuItems
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

        public bool IsArchive { get; set; }

        public bool IsCapture { get; set; }

        public Guid ParentId { get; set; }

        public bool HaveMapping
        {
            get { return _haveMapping; }
            set
            {
                _haveMapping = value;
                OnPropertyChanged("HaveMapping");
            }
        }
    }
}
