using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.ContentViewer.Controls;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Model;
using Ecm.Mvvm;
using Ecm.Utility;
using Ecm.ContentViewer;

namespace Ecm.ContentViewer.Helper
{
    public class ImageViewerCommandManager
    {
        public ImageViewerCommandManager(ImageViewer viewer)
        {
            Viewer = viewer;
        }

        public void Initialize()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Control) };
            Viewer.PrintCommand = new RoutedCommand("Print", typeof(ImageViewer), gesture);
            var commandBinding = new CommandBinding(Viewer.PrintCommand, Print, CanPrint);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
            Viewer.EmailCommand = new RoutedCommand("Email", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.EmailCommand, Email, CanEmail);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Control) };
            Viewer.SaveAsCommand = new RoutedCommand("SaveAs", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.SaveAsCommand, SaveAs, CanSaveAs);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection
                          {
                              new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl + +"),
                              new KeyGesture(Key.Add, ModifierKeys.Control, "Ctrl + +")
                          };
            Viewer.ZoomInCommand = new RoutedCommand("ZoomIn", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.ZoomInCommand, ZoomIn, CanDoAction);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection
                          {
                              new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl + -"),
                              new KeyGesture(Key.Subtract, ModifierKeys.Control, "Ctrl + -")
                          };
            Viewer.ZoomOutCommand = new RoutedCommand("ZoomOut", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.ZoomOutCommand, ZoomOut, CanDoAction);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.U, ModifierKeys.Control) };
            Viewer.PreviousPageCommand = new RoutedCommand("PreviousPage", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.PreviousPageCommand, PreviousPage, CanMovePreviousPage);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) };
            Viewer.NextPageCommand = new RoutedCommand("NextPage", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.NextPageCommand, NextPage, CanMoveNextPage);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) };
            Viewer.FitWidthCommand = new RoutedCommand("FitWidth", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.FitWidthCommand, FitWidth, CanDoAction);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.H, ModifierKeys.Control) };
            Viewer.FitHeightCommand = new RoutedCommand("FitHeight", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.FitHeightCommand, FitHeight, CanDoAction);
            Viewer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D0, ModifierKeys.Control) };
            Viewer.FitToViewerCommand = new RoutedCommand("FitToViewer", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(Viewer.FitToViewerCommand, FitToViewer, CanDoAction);
            Viewer.CommandBindings.Add(commandBinding);
        }

        public ImageViewer Viewer { get; private set; }

        private void CanDoAction(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Viewer.Items != null && Viewer.Items.Count > 0;
        }

        private void CanPrint(object sender, CanExecuteRoutedEventArgs e)
        {
            ContentViewerPermission permission = Viewer.Permission;
            e.CanExecute = Viewer.Items != null && Viewer.Items.Count > 0 && permission != null && permission.CanPrint;
        }

        private void Print(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.PrintFile();

        }

        private void CanEmail(object sender, CanExecuteRoutedEventArgs e)
        {
            ContentViewerPermission permission = Viewer.Permission;
            e.CanExecute = permission != null && permission.CanEmail;
        }

        private void Email(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.EmailFile();
        }

        private void CanSaveAs(object sender, CanExecuteRoutedEventArgs e)
        {
            ContentViewerPermission permission = Viewer.ViewerContainer.PermissionManager.GetContentViewerPermission(null);
            e.CanExecute = permission != null && permission.CanEmail;
        }

        private void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.SaveAs();
        }

        private void ZoomIn(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.ZoomIn();
        }

        private void ZoomOut(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.ZoomOut();
        }

        private void CanMovePreviousPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Viewer.MyLayoutCanvas.CanMovePrevious();
        }

        private void PreviousPage(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.MovePrevious();
        }

        private void CanMoveNextPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Viewer.MyLayoutCanvas.CanMoveNext();
        }

        private void NextPage(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.MoveNext();
        }

        private void FitWidth(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.FitWidth();
        }

        private void FitHeight(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.FitHeight();
        }

        private void FitToViewer(object sender, ExecutedRoutedEventArgs e)
        {
            Viewer.MyLayoutCanvas.FitToWindow();
        }
    }
}
