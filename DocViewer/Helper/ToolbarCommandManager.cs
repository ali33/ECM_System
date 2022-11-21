using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.DocViewer.Controls;
using Ecm.DocViewer.Model;
using Ecm.Model;
using Ecm.Mvvm;
using Ecm.Utility;

namespace Ecm.DocViewer.Helper
{
    internal class ToolbarCommandManager
    {
        public ToolbarCommandManager(ViewerContainer viewerContainer)
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
            e.CanExecute = ViewerContainer.OpeningItems != null && ViewerContainer.OpeningItems.Count > 0;
        }

        private void CanPrint(object sender, CanExecuteRoutedEventArgs e)
        {
            ContentViewerPermission permission = ViewerContainer.PermissionManager.GetViewerPermission(null);
            e.CanExecute = ViewerContainer.OpeningItems != null && ViewerContainer.OpeningItems.Count > 0 && permission != null && permission.CanPrint;
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
            ContentViewerPermission permission = ViewerContainer.PermissionManager.GetViewerPermission(null);
            e.CanExecute = permission != null && permission.CanEmail;
        }

        private void Email(object sender, ExecutedRoutedEventArgs e)
        {
            //try
            //{
            //    var emailHelper = new SendMailHelper(ViewerContainer.WorkingFolder);
            //    emailHelper.StartProcess += EmailHelperStartProcess;
            //    emailHelper.EndProcess += EmailHelperEndProcess;

            //    if (ViewerContainer.OpeningItem.PageData.FileType == FileTypeModel.Image)
            //    {
            //        var dialogViewer = new DialogViewer { Width = 500, Height = 250, Text = "Select pages for email" };
            //        var rangeSelector = new RangeSelector
            //                                {
            //                                    Dialog = dialogViewer
            //                                };
            //        dialogViewer.WpfContent = rangeSelector;
            //        if (dialogViewer.ShowDialog() == DialogResult.OK)
            //        {
            //            string filename = Guid.NewGuid().ToString();
            //            var items = new List<CanvasElement>();
            //            var canvasItems = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).Select(p => p.Image).ToList();
            //            if (rangeSelector.SelectedPageIndexes == null || rangeSelector.SelectedPageIndexes.Count == 0)
            //            {
            //                foreach (CanvasElement item in canvasItems)
            //                {
            //                    CanvasElement clonedItem = item.Clone();
            //                    clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
            //                    items.Add(clonedItem);
            //                }
            //            }
            //            else
            //            {
            //                int itemCount = canvasItems.Count;
            //                foreach (int pageIndex in rangeSelector.SelectedPageIndexes)
            //                {
            //                    if (pageIndex <= itemCount)
            //                    {
            //                        CanvasElement clonedItem = canvasItems[pageIndex - 1].Clone();
            //                        clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
            //                        items.Add(clonedItem);
            //                    }
            //                }
            //            }

            //            emailHelper.AddAttachment(items, filename);
            //            emailHelper.SendMail();
            //        }
            //    }
            //    else
            //    {
            //        using (var stream = new FileStream(ViewerContainer.OpeningItem.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //        {
            //            byte[] binary = stream.ReadAllBytes();
            //            emailHelper.AddAttachment(binary, Guid.NewGuid().ToString(), Path.GetExtension(ViewerContainer.OpeningItem.FilePath));
            //        }

            //        emailHelper.SendMail();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ViewerContainer.HandleException(ex);
            //}
            ExportFileHelper export = new ExportFileHelper(ViewerContainer);
            export.SendMail();
        }

        private void CanSaveAs(object sender, CanExecuteRoutedEventArgs e)
        {
            ContentViewerPermission permission = ViewerContainer.PermissionManager.GetViewerPermission(null);
            e.CanExecute = permission != null && permission.CanEmail;
        }

        private void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            //try
            //{
            //    if (ViewerContainer.OpeningItem.PageData.FileType == FileTypeModel.Image)
            //    {
            //        var dialogViewer = new DialogViewer { Width = 500, Height = 250, Text = "Select pages for saving" };
            //        var rangeSelector = new RangeSelector
            //        {
            //            Dialog = dialogViewer
            //        };
            //        dialogViewer.WpfContent = rangeSelector;
            //        if (dialogViewer.ShowDialog() == DialogResult.OK)
            //        {
            //            string filename = Guid.NewGuid().ToString();
            //            var items = new List<CanvasElement>();
            //            var canvasItems = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).Select(p => p.Image).ToList();
            //            if (rangeSelector.SelectedPageIndexes == null || rangeSelector.SelectedPageIndexes.Count == 0)
            //            {
            //                foreach (CanvasElement item in canvasItems)
            //                {
            //                    CanvasElement clonedItem = item.Clone();
            //                    clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
            //                    items.Add(clonedItem);
            //                }
            //            }
            //            else
            //            {
            //                int itemCount = canvasItems.Count;
            //                foreach (int pageIndex in rangeSelector.SelectedPageIndexes)
            //                {
            //                    if (pageIndex <= itemCount)
            //                    {
            //                        CanvasElement clonedItem = canvasItems[pageIndex - 1].Clone();
            //                        clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
            //                        items.Add(clonedItem);
            //                    }
            //                }
            //            }

            //            string extension = ".xps";
            //            DownloadFileHelper downloadHelper = new DownloadFileHelper(ViewerContainer.WorkingFolder);
            //            downloadHelper.FileName = DialogService.ShowSaveFileDialog(string.Format("{0} documents |*{1}", extension.Replace(".", string.Empty).ToUpper(), extension),
            //                                                                       filename + extension);
            //            if (!string.IsNullOrWhiteSpace(downloadHelper.FileName))
            //            {
            //                downloadHelper.Add(items, filename);
            //                downloadHelper.Save();
            //            }
            //        }
            //    }
            //    else
            //    {
            //        using (var stream = new FileStream(ViewerContainer.OpeningItem.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //        {
            //            byte[] binary = stream.ReadAllBytes();
            //            string extension = Path.GetExtension(ViewerContainer.OpeningItem.FilePath).Replace(".", "");
            //            string defaultName = Guid.NewGuid().ToString() + "." + extension;
            //            string filePath = DialogService.ShowSaveFileDialog(string.Format("{0}|*.{1}", extension.ToUpper(), extension.ToLower()), defaultName);
            //            if (!string.IsNullOrEmpty(filePath))
            //            {
            //                File.WriteAllBytes(filePath, binary);
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ViewerContainer.HandleException(ex);
            //}
            ExportFileHelper export = new ExportFileHelper(ViewerContainer);
            export.SaveFile();
        }

        //private void EmailHelperEndProcess(object sender, EventArgs e)
        //{
        //    ViewerContainer.IsProcessing = false;
        //    ViewerContainer.CollectGarbage();
        //}

        //private void EmailHelperStartProcess(object sender, EventArgs e)
        //{
        //    ViewerContainer.IsProcessing = true;
        //}

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
