using Ecm.ContentViewer.Helper;
using Ecm.ContentViewer.Model;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ecm.ContentViewer.ViewModel
{
    public class ThumbnailViewerViewModel : BaseDependencyProperty
    {
        private bool _contextMenuOpen;

        public ThumbnailViewerViewModel(MainViewerViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public MainViewerViewModel MainViewModel { get; set; }

        public ObservableCollection<ContentItem> Items { get; set; }

        public Action<ContentItem> ContextMenuShow { get; set; }

        public Action<ContentItem> GetFocus { get; set; }

        public bool ContextMenuOpen
        {
            get { return _contextMenuOpen; }
            set
            {
                _contextMenuOpen = value;
                OnPropertyChanged("ContextMenuOpen");
            }
        }

        public void Focus(ContentItem item)
        {
            if (GetFocus != null)
            {
                GetFocus(item);
            }
        }

        public void ShowContextMenu(ContentItem item)
        {
            if (ContextMenuShow != null)
            {
                ContextMenuShow(item);
            }
        }

    }
}
