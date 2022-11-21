using System.Linq;
using System.Windows.Controls;
using Ecm.ContentViewer.Model;
using System.Windows.Input;
using System.Resources;
using System.Reflection;
using Ecm.ContentViewer.ViewModel;
using System.Collections.Generic;

namespace Ecm.ContentViewer.Helper
{
    internal class ContextMenuManager
    {
        ResourceManager _resource = new ResourceManager("Ecm.ContentViewer.MainViewer", Assembly.GetExecutingAssembly());

        public ContextMenuManager(MainViewerViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public MainViewerViewModel MainViewModel { get; private set; }

        private List<MenuItemModel> MenuItems { get; set; }

        public ContextMenu ContextMenu { get; private set; }

        public List<MenuItemModel> InitializeMenuData()
        {
            MenuItems = new List<MenuItemModel>();

            if (MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Batch))
            {
                ApplyForBatchOnly();
            }
            else if (MainViewModel.ThumbnailSelector.SelectedItems.Count > 0 &&
                     !MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.ContentModel))
            {
                ApplyForDocumentOnly();
            }
            else if (MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.ContentModel) &&
                     MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page))
            {
                ApplyForDocumentAndPage();
            }
            else if (MainViewModel.ThumbnailSelector.SelectedItems.Count == 1 &&
                     MainViewModel.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page)
            {
                ApplyForPage();
            }
            else if (MainViewModel.ThumbnailSelector.SelectedItems.Count > 1 &&
                     !MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.Page))
            {
                ApplyForPages();
            }


            return MenuItems;
        }

        private void ApplyForBatchOnly()
        {
            var permission = MainViewModel.PermissionManager;
            bool hasChangeBatchTypeMenu = false;
            bool hasDeleteMenu = false;
            bool hasSendLinkMenu = false;

            if (permission.CanCapture())
            {
                var scanItem = new MenuItemModel { Text = _resource.GetString("mnScan") };
                var importItem = new MenuItemModel { Text = _resource.GetString("mnImport") };
                var cameraItem = new MenuItemModel { Text = _resource.GetString("mnCamera") };

                MenuItems.Add(scanItem);
                MenuItems.Add(importItem);

                AddCaptureSubMenuItems(scanItem, MainViewModel.ScanManager.ScanCommand, MainViewModel.ScanManager.ScanToDocumentTypeCommand);
                AddCaptureSubMenuItems(importItem, MainViewModel.ImportManager.ImportFileSystemCommand, MainViewModel.ImportManager.ImportFileSystemToDocumentTypeCommand);

                if (MainViewModel.CameraManager.HasVideoInputDevice)
                {
                    MenuItems.Add(cameraItem);
                    AddCaptureSubMenuItems(cameraItem, MainViewModel.CameraManager.CaptureContentCommand, MainViewModel.CameraManager.CaptureContentToDocumentTypeCommand);
                }
            }


            if (permission.CanChangeBatchType())
            {
                var changeBatchTypeMenuItem = new MenuItemModel
                {
                    Text = _resource.GetString("mnChangeBatchType"),
                    Command = MainViewModel.ThumbnailCommandManager.ChangeBatchTypeCommand
                };

                MenuItems.Add(changeBatchTypeMenuItem);
                hasChangeBatchTypeMenu = true;
            }

            if (permission.CanDelete())
            {
                var deleteMenuItem = new MenuItemModel
                                         {
                                             Text = _resource.GetString("mnDelete"),
                                             Command = MainViewModel.ThumbnailCommandManager.DeleteCommand
                                         };
                MenuItems.Add(deleteMenuItem);
                hasDeleteMenu = true;

            }

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            if (MainViewModel.ThumbnailSelector.SelectedItems[0].Children.Count > 0 &&
                MainViewModel.PermissionManager.CanSendLink() && MainViewModel.ViewerMode == ViewerMode.WorkItem)
            {
                var sendLinkMenuItem = new MenuItemModel { Text = _resource.GetString("mnSendLink"), Command = MainViewModel.ThumbnailCommandManager.SendLinkCommand };
                MenuItems.Add(sendLinkMenuItem);
                hasSendLinkMenu = true;
            }

            var submitMenuItem = new MenuItemModel { Text = _resource.GetString("mnSubmit"), Command = MainViewModel.ThumbnailCommandManager.SaveCommand };
            MenuItems.Add(submitMenuItem);

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

        private void ApplyForDocumentOnly()
        {
            var permission = MainViewModel.PermissionManager;
            if (permission.CanChangeDocumentType())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnChangeContentType") };
                AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.ChangeContentTypeCommand);
                MenuItems.Add(menuItem);
            }

            var renameMenuItem = new MenuItemModel
            {
                Text = string.Format(_resource.GetString("mnRename"), _resource.GetString("uiContentText")),
                Command = MainViewModel.ThumbnailCommandManager.ChangeNameCommand
            };

            MenuItems.Add(renameMenuItem);

            if (!MainViewModel.ThumbnailSelector.Cursor.Children.Any(p => p.PageData.FileType != ContentModel.FileTypeModel.Image) && (MainViewModel.ViewerMode == ViewerMode.Capture || MainViewModel.ViewerMode == ViewerMode.WorkItem))
            {
                var lanMenuItem = new MenuItemModel { Text = _resource.GetString("mnContentLanguage") };
                MenuItems.Add(lanMenuItem);
                AddLanguageMenu(lanMenuItem, MainViewModel.ThumbnailCommandManager.SetLanguageCommand, MainViewModel.ThumbnailSelector.Cursor);
            }

            if (permission.CanDelete())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnDelete"), Command = MainViewModel.ThumbnailCommandManager.DeleteCommand };
                MenuItems.Add(menuItem);
            }

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            if (MainViewModel.ThumbnailSelector.SelectedItems.Count > 1)
            {
                if (permission.CanCombineDocument())
                {
                    var menuItem = new MenuItemModel
                                       {
                                           Text = _resource.GetString("mnCombine"),
                                           Command = MainViewModel.ThumbnailCommandManager.CombineDocumentCommand
                                       };
                    MenuItems.Add(menuItem);
                }
            }
            else
            {
                if (permission.CanInsert())
                {
                    var menuItem = new MenuItemModel { Text = _resource.GetString("mnAppend") };
                    MenuItems.Add(menuItem);
                    AddCaptureSubMenuItems(menuItem, MainViewModel.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                           MainViewModel.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                           MainViewModel.ThumbnailCommandManager.InsertAfterByCameraCommand);
                }
            }

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (MainViewModel.ThumbnailSelector.SelectedItems.GroupBy(p => p.DocumentData.DocumentType.Id).Count() == 1)
            {
                if (permission.CanModifyIndex())
                {
                    var menuItem = new MenuItemModel
                                       {
                                           Text = _resource.GetString("mnIndex"),
                                           Command = MainViewModel.ThumbnailCommandManager.IndexCommand
                                       };
                    MenuItems.Add(menuItem);
                }
            }

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
            InsertSeparatorBefore("Index");
        }

        private void ApplyForDocumentAndPage()
        {
            var permission = MainViewModel.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocumentFromSelectedPage") };
                MenuItems.Add(menuItem);
                AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (MainViewModel.ThumbnailSelector.Cursor != null &&
                    MainViewModel.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page &&
                    (MainViewModel.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                     (MainViewModel.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.ContentModel &&
                      MainViewModel.ThumbnailSelector.Cursor.Parent.Children.IndexOf(
                          MainViewModel.ThumbnailSelector.Cursor) > 0)))
                {
                    menuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocumentFromHere") };
                    MenuItems.Add(menuItem);
                    AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            if (permission.CanDelete())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnDelete"), Command = MainViewModel.ThumbnailCommandManager.DeleteCommand };
                MenuItems.Add(menuItem);
            }

            var docItem = MainViewModel.ThumbnailSelector.SelectedItems.First(p => p.ItemType == ContentItemType.ContentModel);
            if (MainViewModel.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.ContentModel && p != docItem) ||
                                                                         (p.ItemType == ContentItemType.Page && p.Parent != docItem)))
            {
                if (permission.CanCombineDocument())
                {
                    var menuItem = new MenuItemModel
                                       {
                                           Text = _resource.GetString("mnCombine"),
                                           Command = MainViewModel.ThumbnailCommandManager.CombineDocumentCommand
                                       };
                    MenuItems.Add(menuItem);
                }
            }

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (!MainViewModel.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Page &&
                                                                           p.Parent.ItemType == ContentItemType.ContentModel &&
                                                                           p.Parent.DocumentData.DocumentType.Id != docItem.DocumentData.DocumentType.Id) ||
                                                                          (p.ItemType == ContentItemType.ContentModel &&
                                                                           p.DocumentData.DocumentType.Id != docItem.DocumentData.DocumentType.Id)))
            {
                if (permission.CanModifyIndex())
                {
                    var menuItem = new MenuItemModel
                                       {
                                           Text = _resource.GetString("mnIndex"),
                                           Command = MainViewModel.ThumbnailCommandManager.IndexCommand
                                       };
                    MenuItems.Add(menuItem);
                }
            }

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
            InsertSeparatorBefore("Index");
        }

        private void ApplyForPage()
        {
            var permission = MainViewModel.PermissionManager;

            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocumentFromSelectedPage") };
                MenuItems.Add(menuItem);
                AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (MainViewModel.ThumbnailSelector.Cursor.PageData.FileType == ContentModel.FileTypeModel.Image && (MainViewModel.ViewerMode== ViewerMode.Capture || MainViewModel.ViewerMode== ViewerMode.WorkItem))
                {
                    menuItem = new MenuItemModel { Text = _resource.GetString("mnContentLanguage") };
                    MenuItems.Add(menuItem);
                    AddLanguageMenu(menuItem, MainViewModel.ThumbnailCommandManager.SetLanguageCommand, MainViewModel.ThumbnailSelector.Cursor);
                }

                if (MainViewModel.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                    (MainViewModel.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.ContentModel &&
                     MainViewModel.ThumbnailSelector.Cursor.Parent.Children.IndexOf(MainViewModel.ThumbnailSelector.Cursor) > 0))
                {
                    menuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocumentFromHere") };
                    MenuItems.Add(menuItem);
                    AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            bool hasDelete = false;
            if (permission.CanDelete())
            {
                hasDelete = true;
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnDelete"), Command = MainViewModel.ThumbnailCommandManager.DeleteCommand };
                MenuItems.Add(menuItem);
            }

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            bool hasReplace = false;
            if (permission.CanReplace())
            {
                hasReplace = true;
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnReplace") };
                MenuItems.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, MainViewModel.ThumbnailCommandManager.ReplaceByScannerCommand,
                                       MainViewModel.ThumbnailCommandManager.ReplaceByFileSystemCommand,
                                       MainViewModel.ThumbnailCommandManager.ReplaceByCameraCommand);
            }

            if (permission.CanInsert())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnInsertBefore") };
                MenuItems.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, MainViewModel.ThumbnailCommandManager.InsertBeforeByScannerCommand,
                                       MainViewModel.ThumbnailCommandManager.InsertBeforeByFileSystemCommand,
                                       MainViewModel.ThumbnailCommandManager.InsertBeforeByCameraCommand);

                menuItem = new MenuItemModel { Text = _resource.GetString("mnInsertAfter") };
                MenuItems.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, MainViewModel.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                       MainViewModel.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                       MainViewModel.ThumbnailCommandManager.InsertAfterByCameraCommand);
            }

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (permission.CanModifyIndex())
            {
                if (MainViewModel.ThumbnailSelector.SelectedItems[0].Parent.ItemType == ContentItemType.ContentModel)
                {
                    var menuItem = new MenuItemModel { Text = _resource.GetString("mnIndex"), Command = MainViewModel.ThumbnailCommandManager.IndexCommand };
                    MenuItems.Add(menuItem);
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
            var permission = MainViewModel.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocumentFromSelectedPage") };
                MenuItems.Add(menuItem);
                AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (MainViewModel.ThumbnailSelector.Cursor.PageData.FileType == ContentModel.FileTypeModel.Image && (MainViewModel.ViewerMode == ViewerMode.Capture || MainViewModel.ViewerMode == ViewerMode.WorkItem))
                {
                    menuItem = new MenuItemModel { Text = _resource.GetString("mnContentLanguage") };
                    MenuItems.Add(menuItem);
                    AddLanguageMenu(menuItem, MainViewModel.ThumbnailCommandManager.SetLanguageCommand,MainViewModel.ThumbnailSelector.SelectedItems);
                }

                if (MainViewModel.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                    (MainViewModel.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.ContentModel &&
                     MainViewModel.ThumbnailSelector.Cursor.Parent.Children.IndexOf(MainViewModel.ThumbnailSelector.Cursor) > 0))
                {
                    menuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocumentFromHere") };
                    MenuItems.Add(menuItem);
                    AddDocumentTypeItems(menuItem, MainViewModel.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            if (permission.CanDelete())
            {
                var menuItem = new MenuItemModel { Text = _resource.GetString("mnDelete"), Command = MainViewModel.ThumbnailCommandManager.DeleteCommand };
                MenuItems.Add(menuItem);
            }

            InsertRejectMenuItem();
            InsertUnRejectMenuItem();

            if (permission.CanRotate())
            {
                AddRotationMenuItem();
            }

            if (permission.CanModifyIndex())
            {
                if (!MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.Parent.ItemType == ContentItemType.Batch) &&
                    MainViewModel.ThumbnailSelector.SelectedItems.GroupBy(p => p.Parent.DocumentData.DocumentType.Id).Count() == 1)
                {
                    var menuItem = new MenuItemModel { Text = _resource.GetString("mnIndex"), Command = MainViewModel.ThumbnailCommandManager.IndexCommand };
                    MenuItems.Add(menuItem);
                }
            }

            InsertSeparatorBefore("Delete");
            InsertSeparatorAfter("Delete");
            InsertSeparatorBefore("Index");
        }

        private void AddCaptureSubMenuItems(MenuItemModel menuItem, ICommand unClassifyCommand, ICommand newDocumentCommand)
        {
            var documentTypes = MainViewModel.BatchTypes.First(p => p.Id == MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocTypes;

            if (documentTypes.Count > 0)
            {
                menuItem.Children.Add(new MenuItemModel { Text = _resource.GetString("mnClassifyLater"), Command = unClassifyCommand });
                var newDocumentMenuItem = new MenuItemModel { Text = _resource.GetString("mnNewDocument") };
                menuItem.Children.Add(newDocumentMenuItem);


                foreach (var documentType in documentTypes)
                {
                    newDocumentMenuItem.Children.Add(new MenuItemModel
                                                      {
                                                          Text = documentType.Name,
                                                          Command = newDocumentCommand,
                                                          CommandParameter = documentType,
                                                          Icon = MainViewModel.CreateDocumentIcon(documentType)
                                                      });
                }
            }
        }

        private void AddCaptureSubMenuItems(MenuItemModel menuItem, RoutedCommand scanCommand, RoutedCommand importCommand, RoutedCommand cameraCommand)
        {
            menuItem.Children.Add(new MenuItemModel { Text = _resource.GetString("mnScan"), Command = scanCommand });
            menuItem.Children.Add(new MenuItemModel { Text = _resource.GetString("mnFileImport"), Command = importCommand });

            if (MainViewModel.CameraManager.HasVideoInputDevice)
            {
                menuItem.Children.Add(new MenuItemModel { Text = _resource.GetString("mnCamera"), Command = cameraCommand });
            }
        }

        private void AddDocumentTypeItems(MenuItemModel menuItem, ICommand menuCommand)
        {
            var documentTypes = MainViewModel.BatchTypes.First(p => p.Id == MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocTypes;
            foreach (var documentType in documentTypes)
            {
                menuItem.Children.Add(new MenuItemModel
                                       {
                                           Text = documentType.Name,
                                           Command = menuCommand,
                                           CommandParameter = documentType,
                                           Icon = MainViewModel.CreateDocumentIcon(documentType)
                                       });
            }
        }

        private void AddRotationMenuItem()
        {
            var menuItem = new MenuItemModel { Text = _resource.GetString("mnRotateRight"), Command = MainViewModel.ThumbnailCommandManager.RotateRightCommand };
            MenuItems.Add(menuItem);
            menuItem = new MenuItemModel { Text = _resource.GetString("mnRotateLeft"), Command = MainViewModel.ThumbnailCommandManager.RotateLeftCommand };
            MenuItems.Add(menuItem);
        }

        private void InsertSeparatorAfter(string headerName)
        {
            var menuItemCount = MenuItems.OfType<MenuItemModel>().Count();
            var menuItem = MenuItems.OfType<MenuItemModel>().FirstOrDefault(p => p.Text != null && p.Text.ToString().ToLower() == headerName.ToLower());
            if (menuItem != null)
            {
                var index = MenuItems.IndexOf(menuItem);
                if (index < menuItemCount - 1)
                {
                    if (!(MenuItems[index + 1].IsSeparator))
                    {
                        MenuItems.Insert(index, new MenuItemModel { IsSeparator = true });
                    }
                }
            }
        }

        private void InsertSeparatorBefore(string headerName)
        {
            var menuItem = MenuItems.OfType<MenuItemModel>().FirstOrDefault(p => p.Text != null && p.Text.ToString().ToLower() == headerName.ToLower());
            if (menuItem != null)
            {
                var index = MenuItems.IndexOf(menuItem);
                if (index > 0)
                {
                    if (!(MenuItems[index - 1].IsSeparator))
                    {
                        MenuItems.Insert(index, new MenuItemModel { IsSeparator = true });
                    }
                }
            }
        }

        private void InsertRejectMenuItem()
        {
            if (MainViewModel.ThumbnailSelector.SelectedItems.Any(p => !p.Rejected ||
                                                                  (p.ItemType == ContentItemType.ContentModel && p.Children.Any(q => !q.Rejected)) ||
                                                                  (p.ItemType == ContentItemType.Batch && p.Children.Any(q => !q.Rejected || (q.ItemType == ContentItemType.ContentModel && q.Children.Any(r => !r.Rejected))))) &&
                MainViewModel.PermissionManager.CanReject())
            {
                var item = new MenuItemModel { Text = _resource.GetString("mnReject"), Command = MainViewModel.ThumbnailCommandManager.RejectCommand };
                MenuItems.Add(item);
            }
        }

        private void InsertUnRejectMenuItem()
        {
            bool hasReject = MainViewModel.ThumbnailSelector.SelectedItems.Any(p => p.Rejected ||
                                                                               (p.ItemType == ContentItemType.ContentModel && p.Children.Any(q => q.Rejected)) ||
                                                                               (p.ItemType == ContentItemType.Batch && p.Children.Any(q => q.Rejected ||
                                                                                                                                     (q.ItemType == ContentItemType.ContentModel && q.Children.Any(r => r.Rejected)))));
            if (hasReject)
            {
                var item = new MenuItemModel { Text = _resource.GetString("mnUnreject"), Command = MainViewModel.ThumbnailCommandManager.UnRejectCommand };
                MenuItems.Add(item);
            }
        }

        private void AddLanguageMenu(MenuItemModel menuItem, ICommand menuCommand, ContentItem item)
        {
            var subMenuItem = new MenuItemModel
            {
                Text = _resource.GetString("mnEnglish"),
                Command = menuCommand,
                CommandParameter = "eng",
                IsCheckable = true,
                IsChecked = item.ItemType == ContentItemType.ContentModel ? !item.Children.Any(p => !p.PageData.ContentLanguageCode.Equals("vie")) : item.PageData.ContentLanguageCode.Equals("eng")//ViewerContainer.SelectedLanguageMenu == null ? true : ViewerContainer.SelectedLanguageMenu.Text.ToString().ToLower().Equals(_resource.GetString("mnEnglish").ToLower()) ? true : false
            };

            menuItem.Children.Add(subMenuItem);

            subMenuItem = new MenuItemModel
            {
                Text = _resource.GetString("mnVietnamese"),
                Command = menuCommand,
                CommandParameter = "vie",
                IsCheckable = true,
                IsChecked = item.ItemType == ContentItemType.ContentModel ? !item.Children.Any(p => !p.PageData.ContentLanguageCode.Equals("eng")) : item.PageData.ContentLanguageCode.Equals("vie")//ViewerContainer.SelectedLanguageMenu.Text.ToString().ToLower().Equals(_resource.GetString("mnVietnamese").ToLower()) ? true : false
            };

            menuItem.Children.Add(subMenuItem);

        }

        private void AddLanguageMenu(MenuItemModel menuItem, ICommand menuCommand, SingleItemList<ContentItem> items)
        {
            var subMenuItem = new MenuItemModel
            {
                Text = _resource.GetString("mnEnglish"),
                Command = menuCommand,
                CommandParameter = "eng",
                IsCheckable = true,
                IsChecked = !items.Any(p=>!p.PageData.ContentLanguageCode.Equals("vie"))//ViewerContainer.SelectedLanguageMenu == null ? true : ViewerContainer.SelectedLanguageMenu.Text.ToString().ToLower().Equals(_resource.GetString("mnEnglish").ToLower()) ? true : false
            };

            menuItem.Children.Add(subMenuItem);

            subMenuItem = new MenuItemModel
            {
                Text = _resource.GetString("mnVietnamese"),
                Command = menuCommand,
                CommandParameter = "vie",
                IsCheckable = true,
                IsChecked = !items.Any(p => !p.PageData.ContentLanguageCode.Equals("eng"))//ViewerContainer.SelectedLanguageMenu.Text.ToString().ToLower().Equals(_resource.GetString("mnVietnamese").ToLower()) ? true : false
            };

            menuItem.Children.Add(subMenuItem);

        }
    }
}
