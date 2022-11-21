using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Ecm.CustomControl
{
    public class PopupDialog : Popup
    {
        public void Show()
        {
            IsOpen = true;
        }

        public void Hide()
        {
            IsOpen = false;
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            Placement = PlacementMode.Center;
            Focusable = true;
        }

        protected override void  OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsOpen = false;
            }

            base.OnKeyDown(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "IsOpen")
            {
                var val = (bool)e.NewValue;
                if (val)
                {
                    MaskDialogUtil.ShowMaskLayout(this);
                }
                else
                {
                    MaskDialogUtil.HideMaskLayout(this);
                }
            }

            base.OnPropertyChanged(e);
        }
    }
}