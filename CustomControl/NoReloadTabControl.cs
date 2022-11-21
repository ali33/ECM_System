using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ecm.CustomControl
{
    [TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
    public class NoReloadTabControl : TabControl
    {
        public NoReloadTabControl()
        {
            // this is necessary so that we get the initial databound selected item
            ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
        }

        public static T FindVisualChildByName<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                var controlName = child.GetValue(NameProperty) as string;
                if (controlName == childName)
                {
                    return child as T;
                }

                var result = FindVisualChildByName<T>(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _itemsHolder = FindVisualChildByName<Panel>(this, "PART_ItemsHolder");
            UpdateSelectedItem();
        }

        protected TabItem GetSelectedTabItem()
        {
            object selectedItem = SelectedItem;
            if (selectedItem == null)
            {
                return null;
            }

            var item = selectedItem as TabItem;
            if (item == null)
            {
                item = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem;
            }

            return item;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (_itemsHolder == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _itemsHolder.Children.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (object item in e.OldItems)
                        {
                            var cp = FindChildContentPresenter(item);
                            if (cp != null)
                            {
                                _itemsHolder.Children.Remove(cp);
                            }
                        }
                    }

                    // don't do anything with new items because we don't want to
                    // create visuals that aren't being shown

                    UpdateSelectedItem();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Replace not implemented yet");
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            UpdateSelectedItem();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateSelectedItem();
        }

        private void CreateChildContentPresenter(object item)
        {
            if (item == null)
            {
                return;
            }

            var cp = FindChildContentPresenter(item);

            if (cp != null)
            {
                return;
            }

            // the actual child to be added.  cp.Tag is a reference to the TabItem
            cp = new ContentPresenter
                     {
                         Content = (item is TabItem) ? (item as TabItem).Content : item,
                         ContentTemplate = SelectedContentTemplate,
                         ContentTemplateSelector = SelectedContentTemplateSelector,
                         ContentStringFormat = SelectedContentStringFormat,
                         Visibility = Visibility.Collapsed,
                         Tag = (item is TabItem) ? item : (ItemContainerGenerator.ContainerFromItem(item))
                     };

            _itemsHolder.Children.Add(cp);
        }

        private ContentPresenter FindChildContentPresenter(object data)
        {
            if (data is TabItem)
            {
                data = (data as TabItem).Content;
            }

            if (data == null)
            {
                return null;
            }

            if (_itemsHolder == null)
            {
                return null;
            }

            return _itemsHolder.Children.Cast<ContentPresenter>().FirstOrDefault(cp => cp.Content == data);
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                ItemContainerGenerator.StatusChanged -= ItemContainerGeneratorStatusChanged;
                UpdateSelectedItem();
            }
        }

        private void UpdateSelectedItem()
        {
            if (_itemsHolder == null)
            {
                return;
            }

            // generate a ContentPresenter if necessary
            var item = GetSelectedTabItem();
            if (item != null)
            {
                CreateChildContentPresenter(item);
            }

            // show the right child
            foreach (ContentPresenter child in _itemsHolder.Children)
            {
                var tabItem = child.Tag as TabItem;
                child.Visibility = tabItem != null && (tabItem.IsSelected) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Panel _itemsHolder;
    }
}