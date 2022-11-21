using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureViewer.Helper;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Controls.Primitives;

namespace Ecm.CaptureViewer
{
    public partial class DocThumbnail
    {
        private readonly Thumbnail _dragDrop;
        private Point _lastMouseDown;
        private DragDropEffects _allowDrop;
        private bool _autoShowMenuContext;

        #region Dependency properties

        public static readonly DependencyProperty ItemsProperty =
           DependencyProperty.Register("Items", typeof(ObservableCollection<ContentItem>), typeof(DocThumbnail),
               new FrameworkPropertyMetadata(null));

        #endregion

        #region Public methods and constructors

        public ObservableCollection<ContentItem> Items
        {
            get { return GetValue(ItemsProperty) as ObservableCollection<ContentItem>; }
            set
            {
                SetValue(ItemsProperty, value);

            }
        }

        public ViewerContainer ViewerContainer { get; set; }

        public DocThumbnail()
        {
            InitializeComponent();
            _dragDrop = new Thumbnail(this);
        }

        public void Focus(ContentItem item)
        {
            if (item != null)
            {
                var treeViewItem = GetContainer(item);
                if (treeViewItem != null)
                {
                    treeViewItem.BringIntoView();
                    treeViewItem.Focus();
                }
            }
        }

        public void ShowContextMenu(ContentItem item)
        {
            tvwThumbnail.UpdateLayout();
            TreeViewItem uiItem = GetContainer(item);
            if (uiItem != null)
            {
                _autoShowMenuContext = true;
                ContextMenuEventArgs args = (ContextMenuEventArgs)FormatterServices.GetUninitializedObject(typeof(ContextMenuEventArgs));
                args.RoutedEvent = ContextMenuOpeningEvent;
                args.Source = uiItem;
                tvwThumbnail.RaiseEvent(args);
                tvwThumbnail.ContextMenu.Placement = PlacementMode.Right;
                tvwThumbnail.ContextMenu.PlacementTarget = uiItem;
                tvwThumbnail.ContextMenu.HorizontalOffset = -(uiItem.ActualWidth / 2);
                tvwThumbnail.ContextMenu.VerticalOffset = uiItem.ActualHeight / 2;
                tvwThumbnail.ContextMenu.IsOpen = true;
            }
        }

        #endregion

        #region Event handlers

        private void ThumbnailItemLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var treeViewItem = sender as TreeViewItem;
                if (treeViewItem != null)
                {
                    var item = treeViewItem.Header as ContentItem;
                    if (item != null)
                    {
                        item.Load(ViewerContainer);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var target = GetSelectedItem(e.OriginalSource as UIElement);
                if (target != null)
                {
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        ViewerContainer.ThumbnailSelector.LeftMouseClick(target);
                    }
                    else if (e.ChangedButton == MouseButton.Right)
                    {
                        ViewerContainer.ThumbnailSelector.RightMouseClick(target);
                    }
                }

                if (e.ChangedButton == MouseButton.Left && Items != null && Items.Count > 0)
                {
                    _allowDrop = DragDropEffects.None;
                    _dragDrop.IsDragDropProcessing = true;
                    _lastMouseDown = e.GetPosition(tvwThumbnail);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _allowDrop = DragDropEffects.None;
                _dragDrop.IsDragDropProcessing = false;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = ViewerContainer.ThumbnailSelector.MoveSelectedSelection(e.Key);
                if (ViewerContainer.ThumbnailSelector.Cursor != null && e.Handled)
                {
                    var container = GetContainer(ViewerContainer.ThumbnailSelector.Cursor);
                    if (container != null)
                    {
                        container.BringIntoView();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && _dragDrop.IsDragDropProcessing)
                {
                    var currentPosition = e.GetPosition(tvwThumbnail);
                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        DragDrop.DoDragDrop(tvwThumbnail, ViewerContainer.ThumbnailSelector.SelectedItems, DragDropEffects.Move);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailDragOver(object sender, DragEventArgs e)
        {
            try
            {
                _allowDrop = DragDropEffects.None;
                ScrollWhenDraging(e);
                var container = GetContainer(e.OriginalSource as UIElement);
                var source = e.Data.GetData(typeof(SingleItemList<ContentItem>)) as SingleItemList<ContentItem>;
                if (container != null && source != null)
                {
                    var target = (ContentItem)container.Header;

                    // Disable drop when 
                    // - source contains batch
                    // - source is dropped into itself or its children
                    if (source.Any(p => p.ItemType == ContentItemType.Batch) ||
                        source.Any(p => p == target) ||
                        source.Any(p => p.ItemType != ContentItemType.Page && p.Children.Any(r => r == target)) ||
                        !ViewerContainer.PermissionManager.CanReOrderPage())
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                    else
                    {
                        _allowDrop = DragDropEffects.Move;
                        e.Effects = DragDropEffects.Move;
                    }
                }
                else if (source == null)
                {
                    if (container != null)
                    {
                        var target = (ContentItem)container.Header;
                        if ((!ViewerContainer.PermissionManager.CanInsert() &&
                             (target.ItemType == ContentItemType.Document ||
                              (target.ItemType == ContentItemType.Page && target.Parent.ItemType == ContentItemType.Document))) ||
                            (!ViewerContainer.PermissionManager.CanCapture() &&
                             (target.ItemType == ContentItemType.Batch ||
                              target.ItemType == ContentItemType.Page && target.Parent.ItemType == ContentItemType.Batch)))
                        {
                            e.Effects = DragDropEffects.None;
                            e.Handled = true;
                        }
                        else
                        {
                            _allowDrop = DragDropEffects.Move;
                            e.Effects = DragDropEffects.Move;
                        }
                    }
                    else if (!ViewerContainer.PermissionManager.CanCapture())
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                    else
                    {
                        _allowDrop = DragDropEffects.Move;
                        e.Effects = DragDropEffects.Move;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailDrop(object sender, DragEventArgs e)
        {
            try
            {
                _dragDrop.IsDragDropProcessing = false;
                if (_allowDrop == DragDropEffects.Move)
                {
                    var container = GetContainer(e.OriginalSource as UIElement);
                    ContentItem target = null;

                    if (container != null)
                    {
                        target = (ContentItem) container.Header;
                        container.BringIntoView();
                    }

                    var source = e.Data.GetData(typeof (SingleItemList<ContentItem>)) as SingleItemList<ContentItem>;
                    if (source != null)
                    {
                        _dragDrop.DropContentItems(source, target);
                    }
                    else
                    {
                        var files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                        if (files != null)
                        {
                            if (target == null)
                            {
                                target = Items[0];
                            }

                            var insertIndex = 0;
                            if (target.ItemType == ContentItemType.Page)
                            {
                                insertIndex = target.Parent.Children.IndexOf(target) + 1;
                                target = target.Parent;
                            }

                            _dragDrop.DropFilesFromLocalMachine(files, target, insertIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (ViewerContainer._isNativeMode)
                {
                    ViewerContainer.NativeViewer.Visibility = System.Windows.Visibility.Collapsed;
                }

                var targetItem = GetSelectedItem(e.OriginalSource as UIElement);
                tvwThumbnail.ContextMenu.Visibility = Visibility.Collapsed;
                e.Handled = true;
                if (targetItem != null)
                {
                    var contextMenuBuilder = new ContextMenuManager(ViewerContainer, tvwThumbnail.ContextMenu);
                    contextMenuBuilder.InitializeMenuData();
                    if (tvwThumbnail.ContextMenu.HasItems)
                    {
                        if (!_autoShowMenuContext)
                        {
                            tvwThumbnail.ContextMenu.HorizontalOffset = 0;
                            tvwThumbnail.ContextMenu.VerticalOffset = 0;
                        }

                        tvwThumbnail.ContextMenu.Visibility = Visibility.Visible;
                        e.Handled = false;
                    }

                }
                _autoShowMenuContext = false;
            }
            catch(Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ThumbnailContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            if (ViewerContainer._isNativeMode)
            {
                ViewerContainer.NativeViewer.Visibility = System.Windows.Visibility.Visible;
            }

        }

        #endregion

        #region Helper methods

        private TreeViewItem GetContainer(ContentItem item)
        {
            var sequences = new List<ContentItem> { item };
            while (sequences[0].Parent != null)
            {
                sequences.Insert(0, sequences[0].Parent);
            }

            var treeViewItem = tvwThumbnail.ItemContainerGenerator.ContainerFromItem(sequences[0]) as TreeViewItem;
            for (var i = 1; i < sequences.Count; i++)
            {
                if (treeViewItem == null)
                {
                    break;
                }

                treeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(sequences[i]) as TreeViewItem;
            }

            return treeViewItem;
        }

        private TreeViewItem GetContainer(UIElement element)
        {
            var container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }

            return container;
        }

        private void ScrollWhenDraging(DragEventArgs e)
        {
            var container = GetContainer(e.OriginalSource as UIElement);
            if (container != null)
            {
                container.BringIntoView();
            }

            var mouseposition = e.GetPosition(myScrollViewer);
            if (mouseposition.Y < 40)
            {
                myScrollViewer.LineUp();
            }
            if (mouseposition.Y > myScrollViewer.RenderSize.Height - 40)
            {
                myScrollViewer.LineDown();
            }
        }

        private ContentItem GetSelectedItem(UIElement element)
        {
            var container = GetContainer(element);
            if (container != null)
            {
                return (ContentItem)container.Header;
            }

            return null;
        }

        #endregion

    }
}
