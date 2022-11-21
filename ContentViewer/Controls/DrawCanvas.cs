using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ecm.ContentViewer.Helper;
using Ecm.ContentViewer.Model;

namespace Ecm.ContentViewer.Controls
{
    public class DrawCanvas : Canvas
    {
        public static readonly DependencyProperty EnableReadModeProperty = DependencyProperty.Register("EnableReadMode", typeof(bool), typeof(DrawCanvas),
                                new FrameworkPropertyMetadata(true, EnableReadModeChangedCallback));

        private static void EnableReadModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layoutCanvas = d as DrawCanvas;
            if (layoutCanvas != null)
            {
                layoutCanvas.SetEnableReadMode((bool)e.NewValue);
            }
        }

        public static readonly DependencyProperty CurrentZoomRatioProperty = DependencyProperty.Register("CurrentZoomRatio", typeof(double), typeof(DrawCanvas),
                                new FrameworkPropertyMetadata(1d, CurrentZoomRatioChangedCallback));

        private static void CurrentZoomRatioChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = d as DrawCanvas;
            if (canvas != null)
            {
                canvas.Zoom();
            }
        }

        public static readonly DependencyProperty IsChangedProperty = DependencyProperty.Register(
                                "IsChanged", typeof(bool), typeof(DrawCanvas));

        public event EventHandler ContentChanged;

        public event EventHandler CurrentItemsOnViewChanged;

        public event EventHandler DoubleClickOnAnnotation;

        public event EventHandler DoubleClickOnWorkspace;

        public List<CanvasElement> ChildItems
        {
            get
            {
                return _items;
            }
        }

        public List<int> CurrentItemsOnView { get; private set; }

        public List<ContentItem> DocumentItems { get; private set; }

        public bool EnableReadMode
        {
            get
            {
                return (bool)GetValue(EnableReadModeProperty);
            }
            set
            {
                SetValue(EnableReadModeProperty, value);
            }
        }

        public double CurrentZoomRatio
        {
            get
            {
                return (double)GetValue(CurrentZoomRatioProperty);
            }
            set
            {
                SetValue(CurrentZoomRatioProperty, value);
            }
        }

        public bool IsChanged
        {
            get
            {
                return (bool)GetValue(IsChangedProperty);
            }
            set
            {
                SetValue(IsChangedProperty, value);
            }
        }

        public ScrollViewer ParentScrollViewer
        {
            get { return _parentScrollViewer ?? (_parentScrollViewer = Parent as ScrollViewer); }
        }

        public void ArrangeLayout()
        {
            var itemsLine = new List<CanvasElement>();
            double lineTop = 0;
            double scrollViewerWidth = ParentScrollViewer.ActualWidth;
            double verticalThumbWidth = GetVerticalScrollbarWidth();

            if (scrollViewerWidth - verticalThumbWidth > _space)
            {
                Width = scrollViewerWidth - verticalThumbWidth;

                foreach (CanvasElement item in _items)
                {
                    // Ensure each line has at least 1 item
                    if (itemsLine.Count < 1)
                    {
                        itemsLine.Add(item);
                        continue;
                    }

                    double desiredItemWidth = CalDesiredWidth(itemsLine);
                    double newDesiredWidth = desiredItemWidth + item.ActualItemSize.Width + _space;

                    if (newDesiredWidth + _space >= Width - _space)
                    {
                        DrawLine(lineTop, itemsLine);
                        lineTop += CalMaxHeight(itemsLine);

                        itemsLine.Clear();
                    }

                    itemsLine.Add(item);
                }

                if (itemsLine.Count > 0) // Draw items still not drawn in above loop
                {
                    DrawLine(lineTop, itemsLine);
                    lineTop += CalMaxHeight(itemsLine);
                }

                Height = lineTop;
                CurrentItemsOnView = GetCurrentShownItems();
                if (CurrentItemsOnViewChanged != null)
                {
                    CurrentItemsOnViewChanged(this, EventArgs.Empty);
                }

                if (Height < ((FrameworkElement)(Parent)).ActualHeight)
                {
                    Height = ((FrameworkElement)(Parent)).ActualHeight;
                }
            }
        }

        public bool CanMoveNext()
        {
            return GetFirstItemOfNextLine() != null;
        }

        public bool CanMovePrevious()
        {
            return GetFirstItemOfPreviousLine() != null;
        }

        public void FitHeight()
        {
            if (CurrentItemsOnView != null && CurrentItemsOnView.Count > 0)
            {
                _fitHeight = true;
                CanvasElement item = _items[CurrentItemsOnView[0]];
                CurrentZoomRatio = CalZoomRatioToFitHeight(item);
            }
        }

        public void FitWidth()
        {
            if (CurrentItemsOnView != null && CurrentItemsOnView.Count > 0)
            {
                _fitHeight = false;
                CanvasElement item = _items[CurrentItemsOnView[0]];
                CurrentZoomRatio = CalZoomRatioToFitWidth(item);
            }
        }

        public void FitToWindow()
        {
            if (CurrentItemsOnView != null && CurrentItemsOnView.Count > 0)
            {
                CanvasElement item = _items[CurrentItemsOnView[0]];
                double zoomRatioToFitHeight = CalZoomRatioToFitHeight(item);
                double zoomRatioToFitWidth = CalZoomRatioToFitWidth(item);

                if (zoomRatioToFitHeight >= 1 && zoomRatioToFitWidth >= 1)
                {
                    if (zoomRatioToFitHeight >= zoomRatioToFitWidth)
                    {
                        _fitHeight = false;
                        CurrentZoomRatio = zoomRatioToFitWidth;
                    }
                    else
                    {
                        _fitHeight = true;
                        CurrentZoomRatio = zoomRatioToFitHeight;
                    }
                }
                else if (zoomRatioToFitHeight < 1 && zoomRatioToFitWidth < 1)
                {
                    if (zoomRatioToFitHeight >= zoomRatioToFitWidth)
                    {
                        _fitHeight = false;
                        CurrentZoomRatio = zoomRatioToFitWidth;
                    }
                    else
                    {
                        _fitHeight = true;
                        CurrentZoomRatio = zoomRatioToFitHeight;
                    }
                }
                else if (zoomRatioToFitHeight >= 1)
                {
                    _fitHeight = false;
                    CurrentZoomRatio = zoomRatioToFitWidth;
                }
                else
                {
                    _fitHeight = true;
                    CurrentZoomRatio = zoomRatioToFitHeight;
                }
            }
        }

        public void GoToItem(int pageNumber)
        {
            if (pageNumber > _items.Count)
            {
                pageNumber = _items.Count;
            }

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (!Double.IsNaN(Width))
            {
                ParentScrollViewer.ScrollToVerticalOffset(GetTop(_items[pageNumber - 1].ItemContainer) - _space);
                ParentScrollViewer.ScrollToHorizontalOffset((Width - ParentScrollViewer.ActualWidth)/2);
            }
        }

        public void Initialize(List<ContentItem> items)
        {
            _items = (from p in items select p.Image).ToList();
            DocumentItems = items;
            Children.Clear();

            foreach (CanvasElement item in _items)
            {
                item.MouseDoubleClick -= ItemCanvasMouseDoubleClick;
                item.MouseDoubleClick += ItemCanvasMouseDoubleClick;

                item.StartSelection -= ItemStartSelection;
                item.StartSelection += ItemStartSelection;

                if (item.ItemContainer.Parent != null && item.ItemContainer.Parent is Panel)
                {
                    (item.ItemContainer.Parent as Panel).Children.Remove(item.ItemContainer);
                }

                Children.Add(item.ItemContainer);
            }

            ArrangeLayout();
        }

        public void MoveNext()
        {
            CanvasElement item = GetFirstItemOfNextLine();

            if (item != null)
            {
                ParentScrollViewer.ScrollToVerticalOffset(GetTop(item.ItemContainer) - _space);
            }
        }

        public void MovePrevious()
        {
            CanvasElement item = GetFirstItemOfPreviousLine();
            if (item != null)
            {
                ParentScrollViewer.ScrollToVerticalOffset(GetTop(item.ItemContainer) - _space);
            }
        }

        public void OnContentChanged()
        {
            IsChanged = true;
            if (ContentChanged != null)
            {
                ContentChanged(this, EventArgs.Empty);
            }
        }

        public void RefreshCurrentZoom()
        {
            Zoom();
        }

        public void ZoomIn()
        {
            if (CurrentZoomRatio < _maxZoomRatio)
            {
                double newZoomRatio = CurrentZoomRatio + _mouseWheelZoomStep;
                newZoomRatio = newZoomRatio > _maxZoomRatio ? _maxZoomRatio : newZoomRatio;
                CanvasElement focusItem = GetFocusItem();
                CurrentZoomRatio = newZoomRatio;

                if (focusItem != null)
                {
                    KeepFocusWhenZoom(focusItem);
                }
            }
        }

        public void ZoomOut()
        {
            CanvasElement itemCanvas = _items[CurrentItemsOnView[0]];
            double minZoomRatio = CalZoomRatioToFitHeight(itemCanvas);
            if (CurrentZoomRatio > minZoomRatio)
            {
                double newZoomRatio = itemCanvas.CurrentZoomRatio - _mouseWheelZoomStep;
                newZoomRatio = newZoomRatio < minZoomRatio ? minZoomRatio : newZoomRatio;
                CanvasElement focusItem = GetFocusItem();
                CurrentZoomRatio = newZoomRatio;

                if (focusItem != null)
                {
                    KeepFocusWhenZoom(focusItem);
                }
            }
        }

        protected void LayoutCanvasLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isContentLoaded)
            {
                Focusable = false;
                ParentScrollViewer.SizeChanged += ParentSizeChanged;
                ParentScrollViewer.ScrollChanged += ParentScrollChanged;
                SetEnableReadMode(true);
                _isContentLoaded = true;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += LayoutCanvasLoaded;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (DoubleClickOnWorkspace != null)
                {
                    DoubleClickOnWorkspace(this, e);
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                Focusable = true;
                Focus();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            SetHandCursor();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            SetHandCursor();
        }

        private double CalDesiredWidth(List<CanvasElement> items)
        {
            if (items.Count == 1)
            {
                return items[0].ActualItemSize.Width;
            }

            return items.Sum(p => p.ActualItemSize.Width + _space); // space between items
        }

        private void Zoom()
        {
            foreach (CanvasElement item in _items)
            {
                item.Zoom(CurrentZoomRatio);
            }

            ArrangeLayout();
        }

        /// <summary>
        ///   Calculate the max height of items at one line
        /// </summary>
        private double CalMaxHeight(IEnumerable<CanvasElement> items)
        {
            return items.Max(p => p.ActualItemSize.Height + 2 * _space);
        }

        internal double CalZoomRatioToFitWidth(CanvasElement item)
        {
            if (item.PageInfo == null)
            {
                return 1.0;
            }

            double originalWidth = item.OriginalItemSize.Width;
            double desiredWidth = ParentScrollViewer.ActualWidth - GetVerticalScrollbarWidth() - 2 * _space;
            return desiredWidth / originalWidth;
        }

        internal double CalZoomRatioToFitHeight(CanvasElement item)
        {
            if (item.PageInfo == null)
            {
                return 1.0;
            }

            double originalHeight = item.OriginalItemSize.Height + 4;
            double desiredHeight = ParentScrollViewer.ActualHeight - 2 * _space;
            return desiredHeight / originalHeight;
        }

        internal void KeepFocusWhenZoom(CanvasElement item)
        {
            double oldWidth = item.ActualWidth;
            double oldHeight = item.ActualHeight;
            Point focusPoint = Mouse.GetPosition(item);
            ParentScrollViewer.ScrollToVerticalOffset(GetTop(item.ItemContainer) - _space + (focusPoint.Y - focusPoint.Y));
            UpdateLayout();
            var newFocusPoint = new Point((item.ActualWidth / oldWidth) * focusPoint.X, (item.ActualHeight / oldHeight) * focusPoint.Y);
            var currentMousePosition = Mouse.GetPosition(item);
            ParentScrollViewer.ScrollToVerticalOffset(GetTop(item.ItemContainer) - _space + (newFocusPoint.Y - currentMousePosition.Y));
        }

        private void DrawLine(double top, List<CanvasElement> itemsLine)
        {
            double desiredWidth = CalDesiredWidth(itemsLine); // Actual width and space between items only
            if (itemsLine.Count > 1)
            {
                desiredWidth -= _space;
            }

            double left = (Width - desiredWidth) / 2;
            left = Math.Max(left, _space);

            // Set the width of canvas in case of line items larger the canvas. 
            // Occurs only when line items has only 1 item
            if ((int)desiredWidth + left + _space > Width)
            {
                Width = desiredWidth + left + _space;
            }

            foreach (CanvasElement item in itemsLine)
            {
                SetTop(item.ItemContainer, top + _space);
                SetLeft(item.ItemContainer, left);
                left += item.ActualItemSize.Width + _space;
            }
        }

        private List<int> GetCurrentShownItems()
        {
            double viewPortTop = ParentScrollViewer.VerticalOffset;
            double viewPortBottom = viewPortTop + ParentScrollViewer.ViewportHeight;

            if (ParentScrollViewer.ViewportHeight == 0d)
            {
                viewPortBottom = viewPortTop + ParentScrollViewer.ActualHeight;
            }

            // Get the items in the current view
            // There are 4 cases:
            // - Bottom of item fall in viewport
            // - Top of item fall in viewport
            // - Whole item fall in viewport
            // - Middle of item  fall in viewport
            List<CanvasElement> onViewItems = (from p in _items
                                            where
                                                (GetTop(p.ItemContainer) + p.ActualItemSize.Height > viewPortTop
                                                 && GetTop(p.ItemContainer) + p.ActualItemSize.Height <= viewPortBottom)
                                                || // Bottom of item fall in viewport
                                                (GetTop(p.ItemContainer) >= viewPortTop
                                                 && GetTop(p.ItemContainer) < viewPortBottom)
                                                || // Top of item fall in viewport
                                                (GetTop(p.ItemContainer) >= viewPortTop
                                                 && GetTop(p.ItemContainer) + p.ActualItemSize.Height <= viewPortBottom)
                                                || // Whole item fall in viewport
                                                (GetTop(p.ItemContainer) < viewPortTop
                                                 && GetTop(p.ItemContainer) + p.ActualItemSize.Height > viewPortBottom)
                                            // Middle of item fall in viewport
                                            select p).Distinct().ToList();

            return onViewItems.Select(item => _items.IndexOf(item)).ToList();
        }

        private CanvasElement GetFocusItem()
        {
            Point mousePos = Mouse.GetPosition(this);

            return (from item in ChildItems let itemRect = VisualTreeHelper.GetDescendantBounds(item) let itemBounds = item.TransformToAncestor(this).TransformBounds(itemRect) where itemBounds.Contains(mousePos) select item).FirstOrDefault();
        }

        private CanvasElement GetFirstItemOfNextLine()
        {
            double viewPortTop = ParentScrollViewer.VerticalOffset;
            double viewPortBottom = viewPortTop + ParentScrollViewer.ViewportHeight;

            if (ParentScrollViewer.ViewportHeight == 0d)
            {
                viewPortBottom = viewPortTop + ParentScrollViewer.ActualHeight;
            }

            // First item of next line is first item of: 
            // Bottom below viewport AND Top fall in viewport AND Height inside the viewport
            // OR has Top below viewport
            return (from p in _items
                    where
                        GetTop(p.ItemContainer) > viewPortBottom
                        ||
                        (GetTop(p.ItemContainer) + p.ActualItemSize.Height > viewPortBottom
                         && GetTop(p.ItemContainer) <= viewPortBottom && GetTop(p.ItemContainer) > viewPortTop
                         && p.ActualItemSize.Height < ParentScrollViewer.ActualHeight)
                    select p).FirstOrDefault();
        }

        private CanvasElement GetFirstItemOfPreviousLine()
        {
            double viewPortTop = ParentScrollViewer.VerticalOffset;
            double viewPortBottom = viewPortTop + ParentScrollViewer.ViewportHeight;

            if (ParentScrollViewer.ViewportHeight == 0d)
            {
                viewPortBottom = viewPortTop + ParentScrollViewer.ActualHeight;
            }

            // First item of previous line is first item of: 
            // Bottom + viewport height fall in viewport && Top + viewport height fall in viewport
            // OR Top above viewport AND Bottom fall into the viewport
            CanvasElement item = (from p in _items
                               where (GetTop(p.ItemContainer) + p.ActualItemSize.Height + ParentScrollViewer.ViewportHeight <= viewPortBottom &&
                                      GetTop(p.ItemContainer) + p.ActualItemSize.Height + ParentScrollViewer.ViewportHeight >= viewPortTop && 
                                      GetTop(p.ItemContainer) + ParentScrollViewer.ViewportHeight <= viewPortBottom && 
                                      GetTop(p.ItemContainer) + ParentScrollViewer.ViewportHeight >= viewPortTop) ||
                                     (GetTop(p.ItemContainer) < viewPortTop && GetTop(p.ItemContainer) + p.ActualItemSize.Height <= viewPortBottom && 
                                      GetTop(p.ItemContainer) + p.ActualItemSize.Height > viewPortTop)
                               select p).FirstOrDefault() ??
                              (from p in _items where GetTop(p.ItemContainer) + p.ActualItemSize.Height <= viewPortTop select p).LastOrDefault();

            return item;
        }

        private double GetVerticalScrollbarWidth()
        {
            if (ParentScrollViewer.Style != null)
            {
                if (ParentScrollViewer.Style.Resources.Contains(SystemParameters.VerticalScrollBarWidthKey))
                {
                    return (double) ParentScrollViewer.Style.Resources[SystemParameters.VerticalScrollBarWidthKey];
                }

                if (ParentScrollViewer.Style.Resources.Contains(SystemParameters.ScrollWidthKey))
                {
                    return (double) ParentScrollViewer.Style.Resources[SystemParameters.ScrollWidthKey];
                }
            }

            return SystemParameters.ScrollWidth;
        }

        private void ItemCanvasMouseDoubleClick(object sender, EventArgs e)
        {
            var arg = e as MouseButtonEventArgs;
            var canvas = sender as CanvasElement;
            if (canvas != null && arg != null)
            {
                AnnotationControl annotation = canvas.GetAnnotationShape(arg.GetPosition(canvas));
                if (annotation == null)
                {
                    if (DoubleClickOnWorkspace != null)
                    {
                        DoubleClickOnWorkspace(canvas, e);
                    }

                    if (!canvas.IsNonImagePreview)
                    {
                        _fitHeight = !_fitHeight;
                        CurrentZoomRatio = _fitHeight ? CalZoomRatioToFitHeight(canvas) : CalZoomRatioToFitWidth(canvas);
                        KeepFocusWhenZoom(canvas);
                    }
                }
                else
                {
                    if (DoubleClickOnAnnotation != null)
                    {
                        DoubleClickOnAnnotation(annotation, e);
                    }
                }

                arg.Handled = true;
            }
        }

        private void ItemStartSelection(object sender, EventArgs e)
        {
            var currentItem = sender as CanvasElement;

            foreach (CanvasElement item in _items)
            {
                if (item != currentItem)
                {
                    item.DeselectAll();
                }
            }
        }

        private void ParentScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            CurrentItemsOnView = GetCurrentShownItems();

            if (CurrentItemsOnViewChanged != null)
            {
                CurrentItemsOnViewChanged(this, EventArgs.Empty);
            }
        }

        private void ParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_items.Count > 0)
            {
                ArrangeLayout();
            }
        }

        private void SetEnableReadMode(bool enable)
        {
            if (_readToolHeloper == null)
            {
                _readToolHeloper = new PanToolHelper(this);
            }

            if (enable)
            {
                _readToolHeloper.Enable();
            }
            else
            {
                _readToolHeloper.Remove();
            }
        }

        private void SetHandCursor()
        {
            Cursor = new Cursor(Assembly.GetAssembly(typeof(DrawCanvas)).GetManifestResourceStream("Ecm.ContentViewer.Resources.hand.cur"));
        }

        public const double _space = 10f; // space of items and canvas edge. space between items

        private bool _isContentLoaded;
        private List<CanvasElement> _items = new List<CanvasElement>();
        private ScrollViewer _parentScrollViewer;
        private PanToolHelper _readToolHeloper;
        private bool _fitHeight = true;
        private const double _maxZoomRatio = 5d; // 500%
        private const double _mouseWheelZoomStep = 0.05d;
    }

    public class ZoomRatioEventArgs : EventArgs
    {
        public double ZoomRatio { get; set; }
    }
}