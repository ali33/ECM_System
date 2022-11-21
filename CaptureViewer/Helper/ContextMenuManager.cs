using System.Linq;
using System.Windows.Controls;
using Ecm.CaptureViewer.Model;
using System.Windows.Input;
using System.Resources;
using System.Reflection;
using System;

namespace Ecm.CaptureViewer.Helper
{
    internal class ContextMenuManager
    {
        ResourceManager _resource = new ResourceManager("Ecm.CaptureViewer.ViewerContainer", Assembly.GetExecutingAssembly());

        public ContextMenuManager(ViewerContainer viewerContainer, ContextMenu contextMenu)
        {
            ViewerContainer = viewerContainer;
            ContextMenu = contextMenu;
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public ContextMenu ContextMenu { get; private set; }

        public void InitializeMenuData()
        {
            ContextMenu.Items.Clear();
            if (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Batch))
            {
                if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
                {
                    ApplyForBatchOnly();
                }
            }
            else if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                     !ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.Document))
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    ApplyForOCRTemplateContentType();
                }
                else
                {
                    ApplyForDocumentOnly();
                }
            }
            else if (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Document) &&
                     ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page))
            {
                if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
                {
                    ApplyForDocumentAndPage();
                }
            }
            else if (ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                     ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page)
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    ApplyForOCRTemplatePage();
                }
                else
                {
                    ApplyForPage();
                }
            }
            else if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 1 &&
                     !ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.Page))
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    ApplyForOCRTemplatePages();
                }
                else
                {
                    ApplyForPages();
                }
            }
        }

        private void ApplyForBatchOnly()
        {
            try
            {
                var permission = ViewerContainer.PermissionManager;
                bool hasChangeBatchTypeMenu = false;
                bool hasDeleteMenu = false;
                bool hasSendLinkMenu = false;

                if (permission.CanCapture())
                {
                    var scanItem = new MenuItem { Header = _resource.GetString("mnScan"), IsEnabled = !ViewerContainer.ReadOnly };
                    var importItem = new MenuItem { Header = _resource.GetString("mnImport"), IsEnabled = !ViewerContainer.ReadOnly };
                    var cameraItem = new MenuItem { Header = _resource.GetString("mnCamera"), IsEnabled = !ViewerContainer.ReadOnly };

                    ContextMenu.Items.Add(scanItem);
                    ContextMenu.Items.Add(importItem);
                    AddCaptureSubMenuItems(scanItem, ViewerContainer.ScanManager.ScanCommand, ViewerContainer.ScanManager.ScanToDocumentTypeCommand);
                    AddCaptureSubMenuItems(importItem, ViewerContainer.ImportManager.ImportFileSystemCommand, ViewerContainer.ImportManager.ImportFileSystemToDocumentTypeCommand);

                    if (ViewerContainer.CameraManager.HasVideoInputDevice)
                    {
                        ContextMenu.Items.Add(cameraItem);
                        AddCaptureSubMenuItems(cameraItem, ViewerContainer.CameraManager.CaptureContentCommand, ViewerContainer.CameraManager.CaptureContentToDocumentTypeCommand);
                    }
                }


                if (permission.CanChangeBatchType())
                {
                    var changeBatchTypeMenuItem = new MenuItem
                    {
                        Header = _resource.GetString("mnChangeBatchType"),
                        Command = ViewerContainer.ThumbnailCommandManager.ChangeBatchTypeCommand,
                        IsEnabled = !ViewerContainer.ReadOnly
                    };

                    ContextMenu.Items.Add(changeBatchTypeMenuItem);
                    hasChangeBatchTypeMenu = true;
                }

                // HungLe - 2014/07/18 - Adding menu rename content - Start
                var renameContent = new MenuItem
                {
                    Header = _resource.GetString("mnRenameBatch"),
                    Command = ViewerContainer.ThumbnailCommandManager.RenameBatchCommand,
                    IsEnabled = !ViewerContainer.ReadOnly
                };
                ContextMenu.Items.Add(renameContent);
                hasChangeBatchTypeMenu = true;
                // HungLe - 2014/07/18 - Adding menu rename content - End


                if (permission.CanDelete())
                {
                    var deleteMenuItem = new MenuItem
                                             {
                                                 Header = _resource.GetString("mnDelete"),
                                                 Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand,
                                                 IsEnabled = !ViewerContainer.ReadOnly
                                             };
                    ContextMenu.Items.Add(deleteMenuItem);
                    hasDeleteMenu = true;

                }

                InsertRejectMenuItem();
                InsertUnRejectMenuItem();

                if (ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Count > 0 &&
                    ViewerContainer.PermissionManager.CanSendLink() && ViewerContainer.DocViewerMode == DocViewerMode.WorkItem)
                {
                    var sendLinkMenuItem = new MenuItem { Header = _resource.GetString("mnSendLink"), Command = ViewerContainer.ThumbnailCommandManager.SendLinkCommand, IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(sendLinkMenuItem);
                    hasSendLinkMenu = true;
                }

                var submitMenuItem = new MenuItem { Header = _resource.GetString("mnSubmit"), Command = ViewerContainer.ThumbnailCommandManager.SaveCommand, IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(submitMenuItem);

                if (hasChangeBatchTypeMenu)
                {
                    InsertSeparatorBefore(_resource.GetString("mnChangeBatchType"));
                }
                if (hasDeleteMenu)
                {
                    InsertSeparatorBefore(_resource.GetString("mnDelete"));
                    InsertSeparatorAfter(_resource.GetString("mnDelete"));
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ApplyForDocumentOnly()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanChangeDocumentType())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnChangeContentType"), IsEnabled = !ViewerContainer.ReadOnly };
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.ChangeContentTypeCommand);
                ContextMenu.Items.Add(menuItem);
            }

            var renameMenuItem = new MenuItem
            {
                Header = string.Format(_resource.GetString("mnRename"), _resource.GetString("uiContentText")),
                Command = ViewerContainer.ThumbnailCommandManager.ChangeNameCommand,
                IsEnabled = !ViewerContainer.ReadOnly
            };

            ContextMenu.Items.Add(renameMenuItem);

            if (!ViewerContainer.ThumbnailSelector.Cursor.Children.Any(p => p.PageData.FileType != CaptureModel.FileTypeModel.Image) && (ViewerContainer.DocViewerMode == DocViewerMode.Capture || ViewerContainer.DocViewerMode == DocViewerMode.WorkItem))
            {
                var lanMenuItem = new MenuItem { Header = _resource.GetString("mnContentLanguage"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(lanMenuItem);
                AddLanguageMenu(lanMenuItem, ViewerContainer.ThumbnailCommandManager.SetLanguageCommand, ViewerContainer.ThumbnailSelector.Cursor);
            }

            var deleteMenuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand, IsEnabled = !ViewerContainer.ReadOnly };
            ContextMenu.Items.Add(deleteMenuItem);

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 1)
            {
                if (permission.CanCombineDocument())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnCombine"),
                                           Command = ViewerContainer.ThumbnailCommandManager.CombineDocumentCommand,
                                           IsEnabled = !ViewerContainer.ReadOnly
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }
            else
            {
                if (permission.CanInsert())
                {
                    var menuItem = new MenuItem { Header = _resource.GetString("mnAppend"), IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                    AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                           ViewerContainer.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                           ViewerContainer.ThumbnailCommandManager.InsertAfterByCameraCommand);
                }
            }

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (ViewerContainer.ThumbnailSelector.SelectedItems.Where(p=>p.DocumentData.DocumentType != null).GroupBy(p => p.DocumentData.DocumentType.Id).Count() == 1)
            {
                if (permission.CanModifyIndex())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnIndex"),
                                           Command = ViewerContainer.ThumbnailCommandManager.IndexCommand,
                                           IsEnabled = !ViewerContainer.ReadOnly
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
            InsertSeparatorBefore("Index");
        }

        private void ApplyForDocumentAndPage()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromSelectedPage"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (ViewerContainer.ThumbnailSelector.Cursor != null &&
                    ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page &&
                    (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                     (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                      ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(
                          ViewerContainer.ThumbnailSelector.Cursor) > 0)))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromHere"), IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                    AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            var deleteMenuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand, IsEnabled = !ViewerContainer.ReadOnly };
            ContextMenu.Items.Add(deleteMenuItem);

            var docItem = ViewerContainer.ThumbnailSelector.SelectedItems.First(p => p.ItemType == ContentItemType.Document);
            if (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Document && p != docItem) ||
                                                                         (p.ItemType == ContentItemType.Page && p.Parent != docItem)))
            {
                if (permission.CanCombineDocument())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnCombine"),
                                           Command = ViewerContainer.ThumbnailCommandManager.CombineDocumentCommand,
                                           IsEnabled = !ViewerContainer.ReadOnly
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Page &&
                                                                           p.Parent.ItemType == ContentItemType.Document && p.Parent.DocumentData.DocumentType != null &&
                                                                           p.Parent.DocumentData.DocumentType.Id != docItem.DocumentData.DocumentType.Id) ||
                                                                          (p.ItemType == ContentItemType.Document && p.DocumentData.DocumentType != null &&
                                                                           p.DocumentData.DocumentType.Id != docItem.DocumentData.DocumentType.Id)))
            {
                if (permission.CanModifyIndex())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnIndex"),
                                           Command = ViewerContainer.ThumbnailCommandManager.IndexCommand,
                                           IsEnabled = !ViewerContainer.ReadOnly
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
            InsertSeparatorBefore("Index");
        }

        private void ApplyForPage()
        {
            var permission = ViewerContainer.PermissionManager;

            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromSelectedPage"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (ViewerContainer.ThumbnailSelector.Cursor.PageData.FileType == CaptureModel.FileTypeModel.Image && (ViewerContainer.DocViewerMode== DocViewerMode.Capture || ViewerContainer.DocViewerMode== DocViewerMode.WorkItem))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnContentLanguage"), IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                    AddLanguageMenu(menuItem, ViewerContainer.ThumbnailCommandManager.SetLanguageCommand, ViewerContainer.ThumbnailSelector.Cursor);
                }

                if (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                    (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                     ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(ViewerContainer.ThumbnailSelector.Cursor) > 0))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromHere"), IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                    AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            bool hasDelete = false;
            if (permission.CanDelete())
            {
                hasDelete = true;
                var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand, IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
            }

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            bool hasReplace = false;
            if (permission.CanReplace())
            {
                hasReplace = true;
                var menuItem = new MenuItem { Header = _resource.GetString("mnReplace"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.ReplaceByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByFileSystemCommand,
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByCameraCommand);
            }

            if (permission.CanInsert())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnInsertBefore"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertBeforeByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertBeforeByFileSystemCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertBeforeByCameraCommand);

                menuItem = new MenuItem { Header = _resource.GetString("mnInsertAfter"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertAfterByCameraCommand);
            }

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (permission.CanModifyIndex())
            {
                if (ViewerContainer.ThumbnailSelector.SelectedItems[0].Parent.ItemType == ContentItemType.Document && ViewerContainer.ThumbnailSelector.SelectedItems[0].Parent.DocumentData.DocumentType != null)
                {
                    var menuItem = new MenuItem { Header = _resource.GetString("mnIndex"), Command = ViewerContainer.ThumbnailCommandManager.IndexCommand, IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            if (hasDelete)
            {
                InsertSeparatorBefore("Delete");
            }
            else if (hasReplace)
            {
                InsertSeparatorBefore("Replace");
            }

            if (hasReplace)
            {
                InsertSeparatorAfter("Replace");
            }
            else
            {
                InsertSeparatorAfter("Delete");
            }

            InsertSeparatorBefore("Index");
        }

        private void ApplyForPages()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromSelectedPage"), IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (ViewerContainer.ThumbnailSelector.Cursor.PageData.FileType == CaptureModel.FileTypeModel.Image && (ViewerContainer.DocViewerMode == DocViewerMode.Capture || ViewerContainer.DocViewerMode == DocViewerMode.WorkItem))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnContentLanguage"), IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                    AddLanguageMenu(menuItem, ViewerContainer.ThumbnailCommandManager.SetLanguageCommand,ViewerContainer.ThumbnailSelector.SelectedItems);
                }

                if (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                    (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                     ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(ViewerContainer.ThumbnailSelector.Cursor) > 0))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromHere"), IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                    AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            if (permission.CanModifiedDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand, IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(menuItem);
            }

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (permission.CanModifyIndex())
            {
                if (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.Parent.ItemType == ContentItemType.Batch) &&
                    !ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.Parent.DocumentData.DocumentType == null) &&
                    ViewerContainer.ThumbnailSelector.SelectedItems.GroupBy(p => p.Parent.DocumentData.DocumentType.Id).Count() == 1)
                {
                    var menuItem = new MenuItem { Header = _resource.GetString("mnIndex"), Command = ViewerContainer.ThumbnailCommandManager.IndexCommand, IsEnabled = !ViewerContainer.ReadOnly };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
            InsertSeparatorBefore("Index");
        }

        private void ApplyForOCRTemplateContentType()
        {
            var documentType = ViewerContainer.BatchTypes.First(p => p.Id == ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocTypes[0];

            var scanItem = new MenuItem { Header = _resource.GetString("mnScan"), Command = ViewerContainer.ThumbnailCommandManager.ScanOCRTemplateCommand };
            var importItem = new MenuItem { Header = _resource.GetString("mnImport"), Command = ViewerContainer.ThumbnailCommandManager.ImportOCRTemplateCommand};
            var cameraItem = new MenuItem { Header = _resource.GetString("mnCamera"), Command = ViewerContainer.ThumbnailCommandManager.CameraScanOCRTemplateCommand};

            ContextMenu.Items.Add(scanItem);
            ContextMenu.Items.Add(importItem);

            if (ViewerContainer.CameraManager.HasVideoInputDevice)
            {
                ContextMenu.Items.Add(cameraItem);
            }
        }

        private void ApplyForOCRTemplatePage()
        {

            var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
            ContextMenu.Items.Add(menuItem);

            menuItem = new MenuItem { Header = _resource.GetString("mnReplace") };
            ContextMenu.Items.Add(menuItem);
            AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.ReplaceByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByFileSystemCommand,
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByCameraCommand);

            menuItem = new MenuItem { Header = _resource.GetString("mnInsertBefore") };
            ContextMenu.Items.Add(menuItem);
            AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertBeforeByScannerCommand,
                                    ViewerContainer.ThumbnailCommandManager.InsertBeforeByFileSystemCommand,
                                    ViewerContainer.ThumbnailCommandManager.InsertBeforeByCameraCommand);

            menuItem = new MenuItem { Header = _resource.GetString("mnInsertAfter") };
            ContextMenu.Items.Add(menuItem);
            AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                    ViewerContainer.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                    ViewerContainer.ThumbnailCommandManager.InsertAfterByCameraCommand);

            AddRotationMenuItem();
            
            InsertSeparatorBefore("Delete");
            InsertSeparatorBefore("Replace");
            InsertSeparatorAfter("Replace");
            InsertSeparatorAfter("Delete");
        }

        private void ApplyForOCRTemplatePages()
        {
            var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
            ContextMenu.Items.Add(menuItem);
            AddRotationMenuItem();

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
        }

        private void AddCaptureSubMenuItems(MenuItem menuItem, ICommand unClassifyCommand, ICommand newDocumentCommand)
        {
            var documentTypes = ViewerContainer.BatchTypes.First(p => p.Id == ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocTypes;

            if (documentTypes.Count > 0)
            {
                menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnClassifyLater"), Command = unClassifyCommand, IsEnabled = !ViewerContainer.ReadOnly });
                var newDocumentMenuItem = new MenuItem { Header = _resource.GetString("mnNewDocument"), IsEnabled = !ViewerContainer.ReadOnly };
                menuItem.Items.Add(newDocumentMenuItem);


                foreach (var documentType in documentTypes)
                {
                    newDocumentMenuItem.Items.Add(new MenuItem
                                                      {
                                                          Header = documentType.Name,
                                                          Command = newDocumentCommand,
                                                          CommandParameter = documentType,
                                                          Icon = ViewerContainer.CreateDocumentIcon(documentType),
                                                          IsEnabled = !ViewerContainer.ReadOnly
                                                      });
                }
            }
        }

        private void AddCaptureSubMenuItems(MenuItem menuItem, RoutedCommand scanCommand, RoutedCommand importCommand, RoutedCommand cameraCommand)
        {
            menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnScan"), Command = scanCommand, IsEnabled = !ViewerContainer.ReadOnly });
            menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnFileImport"), Command = importCommand, IsEnabled = !ViewerContainer.ReadOnly });

            if (ViewerContainer.CameraManager.HasVideoInputDevice)
            {
                menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnCamera"), Command = cameraCommand, IsEnabled = !ViewerContainer.ReadOnly });
            }
        }

        private void AddDocumentTypeItems(MenuItem menuItem, ICommand menuCommand)
        {
            var documentTypes = ViewerContainer.BatchTypes.First(p => p.Id == ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocTypes;
            foreach (var documentType in documentTypes)
            {
                menuItem.Items.Add(new MenuItem
                                       {
                                           Header = documentType.Name,
                                           Command = menuCommand,
                                           CommandParameter = documentType,
                                           Icon = ViewerContainer.CreateDocumentIcon(documentType),
                                           IsEnabled = !ViewerContainer.ReadOnly
                                       });
            }
        }

        private void AddRotationMenuItem()
        {
            var menuItem = new MenuItem { Header = _resource.GetString("mnRotateRight"), Command = ViewerContainer.ThumbnailCommandManager.RotateRightCommand, IsEnabled = !ViewerContainer.ReadOnly };
            ContextMenu.Items.Add(menuItem);
            menuItem = new MenuItem { Header = _resource.GetString("mnRotateLeft"), Command = ViewerContainer.ThumbnailCommandManager.RotateLeftCommand, IsEnabled = !ViewerContainer.ReadOnly };
            ContextMenu.Items.Add(menuItem);
        }

        private void InsertSeparatorAfter(string headerName)
        {
            var menuItemCount = ContextMenu.Items.OfType<MenuItem>().Count();
            var menuItem = ContextMenu.Items.OfType<MenuItem>().FirstOrDefault(p => p.Header != null && p.Header.ToString().ToLower() == headerName.ToLower());
            if (menuItem != null)
            {
                var index = ContextMenu.Items.IndexOf(menuItem);
                if (index < menuItemCount - 1)
                {
                    if (!(ContextMenu.Items[index + 1] is Separator))
                    {
                        ContextMenu.Items.Insert(index + 1, new Separator());
                    }
                }
            }
        }

        private void InsertSeparatorBefore(string headerName)
        {
            var menuItem = ContextMenu.Items.OfType<MenuItem>().FirstOrDefault(p => p.Header != null && p.Header.ToString().ToLower() == headerName.ToLower());
            if (menuItem != null)
            {
                var index = ContextMenu.Items.IndexOf(menuItem);
                if (index > 0)
                {
                    if (!(ContextMenu.Items[index - 1] is Separator))
                    {
                        ContextMenu.Items.Insert(index, new Separator());
                    }
                }
            }
        }

        private void InsertRejectMenuItem()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => !p.Rejected ||
                                                                  (p.ItemType == ContentItemType.Document && p.Children.Any(q => !q.Rejected)) ||
                                                                  (p.ItemType == ContentItemType.Batch && p.Children.Any(q => !q.Rejected || (q.ItemType == ContentItemType.Document && q.Children.Any(r => !r.Rejected))))) &&
                ViewerContainer.PermissionManager.CanReject())
            {
                var item = new MenuItem { Header = _resource.GetString("mnReject"), Command = ViewerContainer.ThumbnailCommandManager.RejectCommand, IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(item);
            }
        }

        private void InsertUnRejectMenuItem()
        {
            bool hasReject = ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.Rejected ||
                                                                               (p.ItemType == ContentItemType.Document && p.Children.Any(q => q.Rejected)) ||
                                                                               (p.ItemType == ContentItemType.Batch && p.Children.Any(q => q.Rejected ||
                                                                                                                                     (q.ItemType == ContentItemType.Document && q.Children.Any(r => r.Rejected)))));
            if (hasReject)
            {
                var item = new MenuItem { Header = _resource.GetString("mnUnreject"), Command = ViewerContainer.ThumbnailCommandManager.UnRejectCommand, IsEnabled = !ViewerContainer.ReadOnly };
                ContextMenu.Items.Add(item);
            }
        }

        private void AddLanguageMenu(MenuItem menuItem, ICommand menuCommand, ContentItem item)
        {
            var subMenuItem = new MenuItem
            {
                Header = _resource.GetString("mnEnglish"),
                Command = menuCommand,
                CommandParameter = "eng",
                IsCheckable = true,
                IsChecked = item.ItemType == ContentItemType.Document ? !item.Children.Any(p => !p.PageData.ContentLanguageCode.Equals("vie")) : item.PageData.ContentLanguageCode.Equals("eng")//ViewerContainer.SelectedLanguageMenu == null ? true : ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnEnglish").ToLower()) ? true : false
            };

            menuItem.Items.Add(subMenuItem);

            subMenuItem = new MenuItem
            {
                Header = _resource.GetString("mnVietnamese"),
                Command = menuCommand,
                CommandParameter = "vie",
                IsCheckable = true,
                IsChecked = item.ItemType == ContentItemType.Document ? !item.Children.Any(p => !p.PageData.ContentLanguageCode.Equals("eng")) : item.PageData.ContentLanguageCode.Equals("vie")//ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnVietnamese").ToLower()) ? true : false
            };

            menuItem.Items.Add(subMenuItem);

        }

        private void AddLanguageMenu(MenuItem menuItem, ICommand menuCommand, SingleItemList<ContentItem> items)
        {
            var subMenuItem = new MenuItem
            {
                Header = _resource.GetString("mnEnglish"),
                Command = menuCommand,
                CommandParameter = "eng",
                IsCheckable = true,
                IsChecked = !items.Any(p=>!p.PageData.ContentLanguageCode.Equals("vie"))//ViewerContainer.SelectedLanguageMenu == null ? true : ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnEnglish").ToLower()) ? true : false
            };

            menuItem.Items.Add(subMenuItem);

            subMenuItem = new MenuItem
            {
                Header = _resource.GetString("mnVietnamese"),
                Command = menuCommand,
                CommandParameter = "vie",
                IsCheckable = true,
                IsChecked = !items.Any(p => !p.PageData.ContentLanguageCode.Equals("eng"))//ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnVietnamese").ToLower()) ? true : false
            };

            menuItem.Items.Add(subMenuItem);

        }
    }
}
