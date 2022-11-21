using System.Collections.Specialized;
using System.Windows.Input;
using System.Linq;

using Ecm.ContentViewer.Model;
using Ecm.Mvvm;
using System.Collections.Generic;
using Ecm.ContentViewer.View;

namespace Ecm.ContentViewer.Helper
{
    class ThumbnailSelector : BaseDependencyProperty
    {
        private ContentItem _cursor;

        public ContentItem Cursor
        {
            get { return _cursor; } 
            private set
            {
                _cursor = value;
                OnPropertyChanged("Cursor");
            }
        }

        public MainViewer MainViewer { get; private set; }

        public SingleItemList<ContentItem> SelectedItems { get; private set; }

        public bool IsCtrlPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }
        }

        public bool IsShiftPressed
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            }
        }

        public ThumbnailSelector(MainViewer viewerContainer)
        {
            SelectedItems = new SingleItemList<ContentItem>();
            SelectedItems.CollectionChanged += SelectedItemsCollectionChanged;
            MainViewer = viewerContainer;
        }

        public void InitializeSelection()
        {
            if (MainViewer.Items != null)
            {
                if (MainViewer.Items.Count > 0)
                {
                    SetDefaultSelection();
                }
                else
                {
                    MainViewer.Items.CollectionChanged += ContentItemsCollectionChanged;
                }
            }
            else
            {
                Cursor = null;
                SelectedItems.Clear();
            }
        }

        public void RightMouseClick(ContentItem item)
        {
            if (!IsShiftPressed && !IsCtrlPressed && !SelectedItems.Contains(item))
            {
                Cursor = item;
                SelectedItems.Clear();
                SelectedItems.Add(item);
            }
        }

        public void LeftMouseClick(ContentItem item)
        {
            if (item.ItemType == ContentItemType.Batch)
            {
                SelectBatch(item);
            }
            else
            {
                SelectChildrenItem(item);
            }
        }

        public SingleItemList<ContentItem> GetItems()
        {
            var items = new SingleItemList<ContentItem>();
            foreach (var item in MainViewer.Items)
            {
                items.Add(item);
                var cursor = item;
                while (cursor.Rear != null)
                {
                    items.Add(cursor.Rear);
                    cursor = cursor.Rear;
                }
            }

            return items;
        }

        public bool MoveSelectedSelection(Key key)
        {
            if (MainViewer.Items == null || MainViewer.Items.Count == 0)
            {
                return false;
            }

            var handled = false;
            ContentItem tpCursor = null;
            switch (key)
            {
                case Key.Up:
                    tpCursor = GetPreviousContentItem();
                    handled = true;
                    break;
                case Key.Down:
                    tpCursor = GetNextContentItem();
                    handled = true;
                    break;
            }
            
            if (tpCursor != null && Cursor != tpCursor)
            {
                if (IsShiftPressed)
                {
                    if (SelectedItems.Any(p => p.ItemType == ContentItemType.Batch) || tpCursor.ItemType == ContentItemType.Batch)
                    {
                        SelectedItems.Clear();
                    }

                    SelectedItems.Add(tpCursor);
                    if (SelectedItems.IndexOf(tpCursor) < SelectedItems.Count - 1) // In case of cursor go back
                    {
                        SelectedItems.RemoveAt(SelectedItems.Count - 1);
                    }

                    Cursor = tpCursor;
                }
                else
                {
                    SelectedItems.Clear();
                    SelectedItems.Add(tpCursor);
                    Cursor = tpCursor;
                }
            }

            return handled;
        }

        public void MoveNextDoc()
        {
            
        }

        public void MovePreviousContent()
        {
            if (MainViewer.Items == null || MainViewer.Items.Count == 0)
            {
                return;
            }

            if (Cursor == null)
            {
                
            }
        }

        public void RemoveContentItem(ContentItem item)
        {
            if (Cursor == item || (item.ItemType != ContentItemType.Page && item.Children.Contains(Cursor)))
            {
                Cursor = null;
            }

            if (SelectedItems.Contains(item))
            {
                SelectedItems.Remove(item);
            }

            if (item.ItemType != ContentItemType.Page)
            {
                var tmpItems = new SingleItemList<ContentItem>(SelectedItems);
                foreach (var tmpItem in tmpItems)
                {
                    if (item.Children.Contains(tmpItem))
                    {
                        SelectedItems.Remove(tmpItem);
                    }
                }
            }
        }

        public void MoveSelection()
        {
            if (Cursor == null)
            {
                return;
            }

            if (Cursor.ItemType == ContentItemType.Batch)
            {
                if (MainViewer.Items.Count > 1)
                {
                    var cursorIndex = MainViewer.Items.IndexOf(Cursor);
                    Cursor = MainViewer.Items[cursorIndex == MainViewer.Items.Count - 1 ? cursorIndex - 1 : cursorIndex + 1];
                    SelectedItems.Add(Cursor);
                }
            }
            else
            {
                if (Cursor.Parent.Children.Count == 1)
                {
                    if (Cursor.Parent.ItemType == ContentItemType.Batch)
                    {
                        Cursor = Cursor.Parent;
                    }
                    else
                    {
                        var parentIndex = Cursor.Parent.Parent.Children.IndexOf(Cursor.Parent);
                        if (Cursor.Parent.Parent.Children.Count > 1)
                        {
                            Cursor = Cursor.Parent.Parent.Children[parentIndex == Cursor.Parent.Parent.Children.Count - 1 ? parentIndex - 1 : parentIndex + 1];
                        }
                        else
                        {
                            Cursor = Cursor.Parent.Parent;
                        }
                    }
                }
                else
                {
                    var cursorIndex = Cursor.Parent.Children.IndexOf(Cursor);
                    Cursor = Cursor.Parent.Children[cursorIndex == Cursor.Parent.Children.Count - 1 ? cursorIndex - 1 : cursorIndex + 1];
                }

                SelectedItems.Add(Cursor);
            }
        }

        private ContentItem GetNextContentItem()
        {
            if (Cursor.Rear != null)
            {
                return Cursor.Rear;
            }

            var curentBatchIndex = MainViewer.Items.IndexOf(Cursor.BatchItem);
            if (curentBatchIndex == MainViewer.Items.Count - 1)
            {
                return null;
            }

            return MainViewer.Items[curentBatchIndex + 1];
        }

        private ContentItem GetPreviousContentItem()
        {
            if (Cursor.Front != null)
            {
                return Cursor.Front;
            }

            var curentBatchIndex = MainViewer.Items.IndexOf(Cursor.BatchItem);
            if (curentBatchIndex == 0)
            {
                return null;
            }

            var previousBatch = MainViewer.Items[curentBatchIndex - 1];
            if (previousBatch.Children.Count > 0)
            {
                var lastChild = previousBatch.Children[previousBatch.Children.Count - 1];
                if (lastChild.ItemType == ContentItemType.Page)
                {
                    return lastChild;
                }

                return lastChild.Children[lastChild.Children.Count - 1];
            }

            return null;
        }

        private void ContentItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SetDefaultSelection();
            }
        }

        private void SetDefaultSelection()
        {
            foreach(var item in MainViewer.Items)
            {
                item.IsSelected = false;
                foreach(var child in item.Children)
                {
                    child.IsSelected = false;
                    if (child.ItemType != ContentItemType.Page)
                    {
                        foreach(var page in child.Children)
                        {
                            page.IsSelected = false;
                        }
                    }
                }
            }

            if (MainViewer.DocViewerMode != DocViewerMode.ContentModel && 
                MainViewer.DocViewerMode != DocViewerMode.WorkItem)
            {
                LeftMouseClick(MainViewer.Items[0]);
            }
            else
            {
                if (MainViewer.Items[0].Children.Count > 0)
                {
                    LeftMouseClick(MainViewer.Items[0].Children[0]);
                }
            }
        }

        private void SelectBatch(ContentItem item)
        {
            if (Cursor != item)
            {
                SelectedItems.Clear();
            }

            Cursor = item;
            SelectedItems.Add(item);
        }

        private void SelectChildrenItem(ContentItem item)
        {
            if (Cursor != null && (Cursor.BatchItem != item.BatchItem || Cursor.ItemType == ContentItemType.Batch))
            {
                SelectedItems.Clear();
                Cursor = item;
                SelectedItems.Add(item);
            }
            else
            {
                List<FieldValueModel> fieldValues = null;

                if (Cursor != null && Cursor.DocumentData != null && Cursor.DocumentData.FieldValues != null)
                {
                    fieldValues = Cursor.DocumentData.FieldValues.ToList();
                }

                if (IsCtrlPressed)
                {
                    if (SelectedItems.Contains(item))
                    {
                        SelectedItems.Remove(item);
                    }
                    else
                    {
                        SelectedItems.Add(item);
                    }

                    Cursor = item;
                }
                else if (IsShiftPressed)
                {
                    if (Cursor == null)
                    {
                        Cursor = item;
                        SelectedItems.Add(item);
                    }
                    else
                    {
                        SelectedItems.Clear();
                        var flattenItems = GetItems();
                        var from = flattenItems.IndexOf(Cursor);
                        var to = flattenItems.IndexOf(item);
                        if (from > to)
                        {
                            var tmp = from; from = to; to = tmp;
                        }

                        for(var i = from; i <= to; i++)
                        {
                            SelectedItems.Add(flattenItems[i]);
                        }
                    }
                }
                else
                {
                    if (!SelectedItems.Contains(item))
                    {
                        SelectedItems.Clear();
                    }

                    SelectedItems.Add(item);
                    Cursor = item;
                }

                if (fieldValues != null)
                {
                    foreach (FieldValueModel fieldValue in fieldValues)
                    {
                        if (fieldValue.MultipleUpdate)
                        {
                            var newValue = Cursor.DocumentData.FieldValues.SingleOrDefault(p => p.Field.Id == fieldValue.Field.Id);

                            if (newValue != null)
                            {
                                newValue.Value = fieldValue.Value;
                                newValue.MultipleUpdate = true;
                            }
                        }
                    }
                }
            }
        }

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ContentItem item in e.NewItems)
                {
                    item.IsSelected = true;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ContentItem item in e.OldItems)
                {
                    item.IsSelected = false;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                var oldItem = e.OldItems[0] as ContentItem;
                var newItem = e.NewItems[0] as ContentItem;
                if (oldItem != null)
                {
                    oldItem.IsSelected = false;
                }

                if (newItem != null)
                {
                    newItem.IsSelected = true;
                }
            }
        }
    }
}
