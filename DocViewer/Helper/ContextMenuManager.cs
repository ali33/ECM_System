using System.Linq;
using System.Windows.Controls;
using Ecm.DocViewer.Model;
using System.Windows.Input;
using System.Resources;
using System.Reflection;

namespace Ecm.DocViewer.Helper
{
    internal class ContextMenuManager
    {
        ResourceManager _resource = new ResourceManager("Ecm.DocViewer.ViewerContainer", Assembly.GetExecutingAssembly());
        public ContextMenuManager(ViewerContainer viewerContainer, ContextMenu contextMenu)
        {
            ViewerContainer = viewerContainer;
            ContextMenu = contextMenu;
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public ContextMenu ContextMenu { get; private set; }

        public void InitializeMenuItem()
        {
            ContextMenu.Items.Clear();
            if (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Batch) && ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {
                ApplyForBatchOnly();
            }
            else if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                     !ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.Document))
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                {
                    ApplyForOCRTemplateDocumentOnly();
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
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanCapture())
            {
                var scanItem = new MenuItem { Header = _resource.GetString("mnScan") };
                var importItem = new MenuItem { Header = _resource.GetString("mnImport") };
                var cameraItem = new MenuItem { Header = _resource.GetString("mnCamera") };

                ContextMenu.Items.Add(scanItem);
                ContextMenu.Items.Add(importItem);
                AddCaptureSubMenuItems(scanItem, ViewerContainer.ScanManager.ScanCommand, ViewerContainer.ScanManager.ScanToDocumentTypeCommand);
                AddCaptureSubMenuItems(importItem, ViewerContainer.ImportManager.ImportCommand, ViewerContainer.ImportManager.ImportToDocumentTypeCommand);

                if (ViewerContainer.CameraManager.HasVideoInputDevice)
                {
                    ContextMenu.Items.Add(cameraItem);
                    AddCaptureSubMenuItems(cameraItem, ViewerContainer.CameraManager.CaptureContentCommand, ViewerContainer.CameraManager.CaptureToDocumentTypeCommand);
                }
            }

            if (permission.CanDelete())
            {
                var deleteMenuItem = new MenuItem
                                         {
                                             Header = _resource.GetString("mnDelete"),
                                             Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand
                                         };
                ContextMenu.Items.Add(deleteMenuItem);
            }

            InsertSeparatorBeforeMenuItem("Delete");
            InsertSeparatorAfterMenuItem("Delete");
        }

        private void ApplyForDocumentOnly()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanChangeDocumentType())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnChangeContentType") };
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.ChangeDocumentTypeCommand);
                ContextMenu.Items.Add(menuItem);
            }
            
            if (!ViewerContainer.ThumbnailSelector.Cursor.Children.Any(p => p.PageData.FileType != Ecm.Model.FileTypeModel.Image) && (ViewerContainer.DocViewerMode== DocViewerMode.LightCapture || ViewerContainer.DocViewerMode== DocViewerMode.Document))
            {
                var lanMenuItem = new MenuItem { Header = _resource.GetString("mnContentLanguage") };
                ContextMenu.Items.Add(lanMenuItem);
                AddLanguageMenu(lanMenuItem, ViewerContainer.ThumbnailCommandManager.SetLanguageCommand, ViewerContainer.ThumbnailSelector.Cursor);
            }

            if (permission.CanDelete())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
                ContextMenu.Items.Add(menuItem);
            }

            if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 1)
            {
                if (permission.CanCombineDocument())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnCombine"),
                                           Command = ViewerContainer.ThumbnailCommandManager.CombineDocumentCommand
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }
            else
            {
                if (permission.CanInsert())
                {
                    var menuItem = new MenuItem { Header = _resource.GetString("mnAppend") };
                    ContextMenu.Items.Add(menuItem);
                    AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                           ViewerContainer.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                           ViewerContainer.ThumbnailCommandManager.InsertAfterByCameraCommand);
                }
            }

            if (permission.CanRotate())
            {
                AddRotationOptions();
            }

            if (ViewerContainer.ThumbnailSelector.SelectedItems.GroupBy(p => p.DocumentData.DocumentType.Id).Count() == 1)
            {
                if (permission.CanModifyIndex())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnIndex"),
                                           Command = ViewerContainer.ThumbnailCommandManager.IndexCommand
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            InsertSeparatorBeforeMenuItem("Delete");
            InsertSeparatorAfterMenuItem("Delete");
            InsertSeparatorBeforeMenuItem("Index");
        }

        private void ApplyForDocumentAndPage()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromSelectedPage") };
                ContextMenu.Items.Add(menuItem);
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (ViewerContainer.ThumbnailSelector.Cursor != null &&
                    ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page &&
                    (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                     (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                      ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(
                          ViewerContainer.ThumbnailSelector.Cursor) > 0)))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromHere") };
                    ContextMenu.Items.Add(menuItem);
                    AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            if (permission.CanDelete())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
                ContextMenu.Items.Add(menuItem);
            }

            var docItem = ViewerContainer.ThumbnailSelector.SelectedItems.First(p => p.ItemType == ContentItemType.Document);
            if (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Document && p != docItem) ||
                                                                         (p.ItemType == ContentItemType.Page && p.Parent != docItem)))
            {
                if (permission.CanCombineDocument())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnCombine"),
                                           Command = ViewerContainer.ThumbnailCommandManager.CombineDocumentCommand
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            if (permission.CanRotate())
            {
                AddRotationOptions();
            }

            if (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Page &&
                                                                           p.Parent.ItemType == ContentItemType.Document &&
                                                                           p.Parent.DocumentData.DocumentType.Id != docItem.DocumentData.DocumentType.Id) ||
                                                                          (p.ItemType == ContentItemType.Document &&
                                                                           p.DocumentData.DocumentType.Id != docItem.DocumentData.DocumentType.Id)))
            {
                if (permission.CanModifyIndex())
                {
                    var menuItem = new MenuItem
                                       {
                                           Header = _resource.GetString("mnIndex"),
                                           Command = ViewerContainer.ThumbnailCommandManager.IndexCommand
                                       };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            InsertSeparatorBeforeMenuItem("Delete");
            InsertSeparatorAfterMenuItem("Delete");
            InsertSeparatorBeforeMenuItem("Index");
        }

        private void ApplyForPage()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromSelectedPage") };
                ContextMenu.Items.Add(menuItem);
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (ViewerContainer.ThumbnailSelector.Cursor.PageData.FileType == Ecm.Model.FileTypeModel.Image && (ViewerContainer.DocViewerMode == DocViewerMode.LightCapture || ViewerContainer.DocViewerMode == DocViewerMode.Document))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnContentLanguage") };
                    ContextMenu.Items.Add(menuItem);
                    AddLanguageMenu(menuItem, ViewerContainer.ThumbnailCommandManager.SetLanguageCommand, ViewerContainer.ThumbnailSelector.Cursor);
                }

                if (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                    (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                     ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(ViewerContainer.ThumbnailSelector.Cursor) > 0))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromHere") };
                    ContextMenu.Items.Add(menuItem);
                    AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            bool hasDelete = false;
            if (permission.CanDelete())
            {
                hasDelete = true;
                var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
                ContextMenu.Items.Add(menuItem);
            }

            bool hasReplace = false;
            if (permission.CanReplace())
            {
                hasReplace = true;
                var menuItem = new MenuItem { Header = _resource.GetString("mnReplace") };
                ContextMenu.Items.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.ReplaceByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByImportCommand,
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByCameraCommand);
            }

            if (permission.CanInsert())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnInsertBefore") };
                ContextMenu.Items.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertBeforeByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertBeforeByFileSystemCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertBeforeByCameraCommand);

                menuItem = new MenuItem { Header = _resource.GetString("mnInsertAfter") };
                ContextMenu.Items.Add(menuItem);
                AddCaptureSubMenuItems(menuItem, ViewerContainer.ThumbnailCommandManager.InsertAfterByScannerCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertAfterByFileSystemCommand,
                                       ViewerContainer.ThumbnailCommandManager.InsertAfterByCameraCommand);
            }

            if (permission.CanRotate())
            {
                AddRotationOptions();
            }

            if (permission.CanModifyIndex())
            {
                if (ViewerContainer.ThumbnailSelector.SelectedItems[0].Parent.ItemType == ContentItemType.Document)
                {
                    var menuItem = new MenuItem { Header = _resource.GetString("mnIndex"), Command = ViewerContainer.ThumbnailCommandManager.IndexCommand };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            if (hasDelete)
            {
                InsertSeparatorBeforeMenuItem("Delete");
            }
            else if (hasReplace)
            {
                InsertSeparatorBeforeMenuItem("Replace");
            }

            if (hasReplace)
            {
                InsertSeparatorAfterMenuItem("Replace");
            }
            else
            {
                InsertSeparatorAfterMenuItem("Delete");
            }

            InsertSeparatorBeforeMenuItem("Index");
        }

        private void ApplyForPages()
        {
            var permission = ViewerContainer.PermissionManager;
            if (permission.CanCreateDocument())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromSelectedPage") };
                ContextMenu.Items.Add(menuItem);
                AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentFromSelectedCommand);

                if (ViewerContainer.ThumbnailSelector.Cursor.PageData.FileType == Ecm.Model.FileTypeModel.Image && (ViewerContainer.DocViewerMode == DocViewerMode.LightCapture || ViewerContainer.DocViewerMode == DocViewerMode.Document))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnContentLanguage") };
                    ContextMenu.Items.Add(menuItem);
                    AddLanguageMenu(menuItem, ViewerContainer.ThumbnailCommandManager.SetLanguageCommand, ViewerContainer.ThumbnailSelector.SelectedItems);
                }

                if (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                    (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                     ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(ViewerContainer.ThumbnailSelector.Cursor) > 0))
                {
                    menuItem = new MenuItem { Header = _resource.GetString("mnNewDocumentFromHere") };
                    ContextMenu.Items.Add(menuItem);
                    AddDocumentTypeItems(menuItem, ViewerContainer.ThumbnailCommandManager.NewDocumentStartingHereCommand);
                }
            }

            if (permission.CanDelete())
            {
                var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
                ContextMenu.Items.Add(menuItem);
            }

            if (permission.CanRotate())
            {
                AddRotationOptions();
            }

            if (permission.CanModifyIndex())
            {
                if (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.Parent.ItemType == ContentItemType.Batch) &&
                    ViewerContainer.ThumbnailSelector.SelectedItems.GroupBy(p => p.Parent.DocumentData.DocumentType.Id).Count() == 1)
                {
                    var menuItem = new MenuItem { Header = _resource.GetString("mnIndex"), Command = ViewerContainer.ThumbnailCommandManager.IndexCommand };
                    ContextMenu.Items.Add(menuItem);
                }
            }

            InsertSeparatorBeforeMenuItem("Delete");
            InsertSeparatorAfterMenuItem("Delete");
            InsertSeparatorBeforeMenuItem("Index");
        }

        private void ApplyForOCRTemplateDocumentOnly()
        {
            var documentType = ViewerContainer.BatchTypes.First(p => p.Id == ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocumentTypes[0];

            var scanItem = new MenuItem { Header = _resource.GetString("mnScan"), Command = ViewerContainer.ThumbnailCommandManager.ScanOCRTemplateCommand };
            var importItem = new MenuItem { Header = _resource.GetString("mnImport"), Command = ViewerContainer.ThumbnailCommandManager.ImportOCRTemplateCommand };
            var cameraItem = new MenuItem { Header = _resource.GetString("mnCamera"), Command = ViewerContainer.ThumbnailCommandManager.CameraScanOCRTemplateCommand };

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
                                       ViewerContainer.ThumbnailCommandManager.ReplaceByImportCommand,
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

            AddRotationOptions();

            InsertSeparatorBeforeMenuItem("Delete");
            InsertSeparatorBeforeMenuItem("Replace");
            InsertSeparatorAfterMenuItem("Replace");
            InsertSeparatorAfterMenuItem("Delete");
        }

        private void ApplyForOCRTemplatePages()
        {
            var menuItem = new MenuItem { Header = _resource.GetString("mnDelete"), Command = ViewerContainer.ThumbnailCommandManager.DeleteCommand };
            ContextMenu.Items.Add(menuItem);
            AddRotationOptions();

            InsertSeparatorBeforeMenuItem("Delete");
            InsertSeparatorAfterMenuItem("Delete");
        }

        private void AddCaptureSubMenuItems(MenuItem menuItem, ICommand unClassifyCommand, ICommand newDocumentCommand)
        {
            menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnClassifyLater"), Command = unClassifyCommand });
            var newDocumentMenuItem = new MenuItem { Header = _resource.GetString("mnNewDocument") };
            menuItem.Items.Add(newDocumentMenuItem);

            var documentTypes = ViewerContainer.BatchTypes.First(p => p.Id == ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocumentTypes;
            foreach (var documentType in documentTypes)
            {
                newDocumentMenuItem.Items.Add(new MenuItem
                                                  {
                                                      Header = documentType.Name,
                                                      Command = newDocumentCommand,
                                                      CommandParameter = documentType,
                                                      Icon = ViewerContainer.CreateDocumentIcon(documentType)
                                                  });
            }
        }

        private void AddCaptureSubMenuItems(MenuItem menuItem, RoutedCommand scanCommand, RoutedCommand importCommand, RoutedCommand cameraCommand)
        {
            menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnScan"), Command = scanCommand });
            menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnFileImport"), Command = importCommand });

            if (ViewerContainer.CameraManager.HasVideoInputDevice)
            {
                menuItem.Items.Add(new MenuItem { Header = _resource.GetString("mnCamera"), Command = cameraCommand });
            }
        }

        private void AddDocumentTypeItems(MenuItem menuItem, ICommand menuCommand)
        {
            var documentTypes = ViewerContainer.BatchTypes.First(p => p.Id == ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.Id).DocumentTypes;
            foreach (var documentType in documentTypes)
            {
                menuItem.Items.Add(new MenuItem
                                       {
                                           Header = documentType.Name,
                                           Command = menuCommand,
                                           CommandParameter = documentType,
                                           Icon = ViewerContainer.CreateDocumentIcon(documentType)
                                       });
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
                IsChecked = item.ItemType == ContentItemType.Document ? !item.Children.Any(p => p.PageData.ContentLanguageCode.Equals("vie")) : item.PageData.ContentLanguageCode.Equals("eng")//ViewerContainer.SelectedLanguageMenu == null ? true : ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnEnglish").ToLower()) ? true : false
            };

            menuItem.Items.Add(subMenuItem);

            subMenuItem = new MenuItem
            {
                Header = _resource.GetString("mnVietnamese"),
                Command = menuCommand,
                CommandParameter = "vie",
                IsCheckable = true,
                IsChecked = item.ItemType == ContentItemType.Document ? !item.Children.Any(p => p.PageData.ContentLanguageCode.Equals("eng")) : item.PageData.ContentLanguageCode.Equals("vie")//ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnVietnamese").ToLower()) ? true : false
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
                IsChecked = !items.Any(p => !p.PageData.ContentLanguageCode.Equals("vie"))//ViewerContainer.SelectedLanguageMenu == null ? true : ViewerContainer.SelectedLanguageMenu.Header.ToString().ToLower().Equals(_resource.GetString("mnEnglish").ToLower()) ? true : false
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

        private void AddRotationOptions()
        {
            var menuItem = new MenuItem { Header = _resource.GetString("mnRotateRight"), Command = ViewerContainer.ThumbnailCommandManager.RotateRightCommand };
            ContextMenu.Items.Add(menuItem);
            menuItem = new MenuItem { Header = _resource.GetString("mnRotateLeft"), Command = ViewerContainer.ThumbnailCommandManager.RotateLeftCommand };
            ContextMenu.Items.Add(menuItem);
        }

        private void InsertSeparatorAfterMenuItem(string headerName)
        {
            var menuItemCount = ContextMenu.Items.OfType<MenuItem>().Count();
            var menuItem = ContextMenu.Items.OfType<MenuItem>().FirstOrDefault(p => p.Header.ToString().ToLower() == headerName.ToLower());
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

        private void InsertSeparatorBeforeMenuItem(string headerName)
        {
            var menuItem = ContextMenu.Items.OfType<MenuItem>().FirstOrDefault(p => p.Header.ToString().ToLower() == headerName.ToLower());
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
    }
}
