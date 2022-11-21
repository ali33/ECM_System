using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace Ecm.CustomControl
{
    public class SortableGridViewColumn : GridViewColumn
    {

        public ListSortDirection Direction
        {
            get
            {
                return (ListSortDirection)GetValue(DirectionProperty);
            }
            set
            {
                SetValue(DirectionProperty, value);
            }
        }

        public bool IsDefaultSortColumn
        {
            get
            {
                return (bool)GetValue(IsDefaultSortColumnProperty);
            }
            set
            {
                SetValue(IsDefaultSortColumnProperty, value);
            }
        }

        public string SortPropertyName
        {
            get
            {
                return (string)GetValue(SortPropertyNameProperty);
            }
            set
            {
                SetValue(SortPropertyNameProperty, value);
            }
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction",
            typeof(ListSortDirection),
            typeof(SortableGridViewColumn),
            new UIPropertyMetadata(ListSortDirection.Ascending));

        public static readonly DependencyProperty IsDefaultSortColumnProperty =
            DependencyProperty.Register(
                "IsDefaultSortColumn", typeof(bool), typeof(SortableGridViewColumn), new UIPropertyMetadata(false));

        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.Register(
                "SortPropertyName", typeof(string), typeof(SortableGridViewColumn), new UIPropertyMetadata(""));

    }
}
