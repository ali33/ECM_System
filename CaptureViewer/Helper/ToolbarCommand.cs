using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.CaptureViewer.Controls;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using Ecm.Utility;

namespace Ecm.CaptureViewer.Helper
{
    internal class ToolbarCommand
    {
        public ToolbarCommand(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
        }

        public void Initialize()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Control) };
            PrintCommand = new RoutedCommand("Print", typeof(ImageViewer), gesture);
            var commandBinding = new CommandBinding(PrintCommand, Print, CanPrint);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
            EmailCommand = new RoutedCommand("Email", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(EmailCommand, Email, CanEmail);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Control) };
            SaveAsCommand = new RoutedCommand("SaveAs", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(SaveAsCommand, SaveAs, CanSaveAs);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection
                          {
                              new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl + +"),
                              new KeyGesture(Key.Add, ModifierKeys.Control, "Ctrl + +")
                          };
            ZoomInCommand = new RoutedCommand("ZoomIn", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(ZoomInCommand, ZoomIn, CanDoAction);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection
                          {
                              new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl + -"),
                              new KeyGesture(Key.Subtract, ModifierKeys.Control, "Ctrl + -")
                          };
            ZoomOutCommand = new RoutedCommand("ZoomOut", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(ZoomOutCommand, ZoomOut, CanDoAction);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.U, ModifierKeys.Control) };
            PreviousPageCommand = new RoutedCommand("PreviousPage", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(PreviousPageCommand, PreviousPage, CanMovePreviousPage);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) };
            NextPageCommand = new RoutedCommand("NextPage", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(NextPageCommand, NextPage, CanMoveNextPage);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) };
            FitWidthCommand = new RoutedCommand("FitWidth", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(FitWidthCommand, FitWidth, CanDoAction);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.H, ModifierKeys.Control) };
            FitHeightCommand = new RoutedCommand("FitHeight", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(FitHeightCommand, FitHeight, CanDoAction);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D0, ModifierKeys.Control) };
            FitToViewerCommand = new RoutedCommand("FitToViewer", typeof(ImageViewer), gesture);
            commandBinding = new CommandBinding(FitToViewerCommand, FitToViewer, CanDoAction);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public RoutedCommand PrintCommand;

        public RoutedCommand EmailCommand;

        public RoutedCommand SaveAsCommand;

        public RoutedCommand ZoomInCommand;

        public RoutedCommand ZoomOutCommand;

        public RoutedCommand NextPageCommand;

        public RoutedCommand PreviousPageCommand;

        public RoutedCommand FitWidthCommand;

        public RoutedCommand FitHeightCommand;

        public RoutedCommand FitToViewerCommand;

        private void CanDoAction(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = ViewerContainer.OpeningItems != null && ViewerContainer.OpeningItems.Count > 0;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanPrint(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                ContentViewerPermission permission = ViewerContainer.PermissionManager.GetContentViewerPermission(null);
                e.CanExecute = ViewerContainer.OpeningItems != null && ViewerContainer.OpeningItems.Count > 0 && permission != null && permission.CanPrint;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void Print(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var items = new List<CanvasElement>();
                var canvasItems = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).Select(p => p.Image).ToList();
                if (canvasItems.Count > 0)
                {
                    foreach (CanvasElement item in canvasItems)
                    {
                        CanvasElement clonedItem = item.Clone();
                        clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
                        items.Add(clonedItem);
                    }

                    var printHelper = new PrintHelper(canvasItems.Count, ViewerContainer.AppName, ViewerContainer.WorkingFolder) { HandleException = ViewerContainer.HandleException };
                    printHelper.StartPrint += PrintHelperStartPrint;
                    printHelper.EndPrint += PrintHelperEndPrint;
                    printHelper.Print(items);
                }

                ViewerContainer.AddActionLog(new ActionLogModel { Message = "Print document(s) by user: " + ViewerContainer.UserName });
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void PrintHelperStartPrint(object sender, EventArgs e)
        {
            ViewerContainer.IsProcessing = true;
        }

        private void PrintHelperEndPrint(object sender, EventArgs e)
        {
            ViewerContainer.IsProcessing = false;
            ViewerContainer.CollectGarbage();
        }

        private void CanEmail(object sender, CanExecuteRoutedEventArgs e)
        {
            try{
            ContentViewerPermission permission = ViewerContainer.PermissionManager.GetContentViewerPermission(null);
            e.CanExecute = permission != null && permission.CanEmail;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void Email(object sender, ExecutedRoutedEventArgs e)
        {
            ExportFileHelper export = new ExportFileHelper(ViewerContainer);
            export.SendMail();
        }

        private void CanSaveAs(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
            ContentViewerPermission permission = ViewerContainer.PermissionManager.GetContentViewerPermission(null);
            e.CanExecute = permission != null && permission.CanEmail;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            ExportFileHelper export = new ExportFileHelper(ViewerContainer);
            export.SaveFile();
        }

        private void ZoomIn(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.ZoomIn();
        }

        private void ZoomOut(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.ZoomOut();
        }

        private void CanMovePreviousPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ImageViewer.MyLayoutCanvas.CanMovePrevious();
        }

        private void PreviousPage(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.MovePrevious();
        }

        private void CanMoveNextPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ImageViewer.MyLayoutCanvas.CanMoveNext();
        }

        private void NextPage(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.MoveNext();
        }

        private void FitWidth(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.FitWidth();
        }

        private void FitHeight(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.FitHeight();
        }

        private void FitToViewer(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ImageViewer.MyLayoutCanvas.FitToWindow();
        }
    }
}
