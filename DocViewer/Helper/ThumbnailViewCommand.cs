using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;
using Ecm.Mvvm;
using System.IO;
using System.Resources;
using System.Reflection;

namespace Ecm.DocViewer.Helper
{
    internal class ThumbnailViewCommand
    {
        public ThumbnailViewCommand(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            IndexedItems = new List<ContentItem>();
            Initialize();
        }

        public void PopulateIndexPanel()
        {
            var fieldValues = new List<FieldValueModel>();
            var indexedItemCount = 0;
            var selectedItems = ViewerContainer.ThumbnailSelector.SelectedItems;
            IndexedItems = new List<ContentItem>();

            if (selectedItems.Count > 0)
            {
                if (selectedItems.Any(p => p.ItemType == ContentItemType.Batch))
                {
                    if (ViewerContainer.DocViewerMode != DocViewerMode.Capture)
                    {
                        if (selectedItems[0].Children.Count(p => p.ItemType == ContentItemType.Document) > 0)
                        {
                            var docItem = selectedItems[0].Children.First(p => p.ItemType == ContentItemType.Document);
                            fieldValues = docItem.DocumentData.FieldValues;
                            indexedItemCount = 1;
                            IndexedItems.Add(docItem);
                        }
                    }
                }
                else if (!selectedItems.Any(p => p.ItemType == ContentItemType.Page && p.Parent.ItemType == ContentItemType.Batch))
                {
                    // Get first document
                    ContentItem firstDocItem;
                    if (selectedItems[0].ItemType == ContentItemType.Document)
                    {
                        firstDocItem = selectedItems[0];
                    }
                    else
                    {
                        firstDocItem = selectedItems[0].Parent;
                    }

                    var allSelectedDocItems = selectedItems.Where(p => p.ItemType == ContentItemType.Document &&
                                                                       p != firstDocItem &&
                                                                       p.DocumentData.DocumentType.Id == firstDocItem.DocumentData.DocumentType.Id).ToList();
                    var allSelectedDocOfPageItems = selectedItems.Where(p => p.ItemType == ContentItemType.Page &&
                                                                             p.Parent != firstDocItem &&
                                                                             !allSelectedDocItems.Contains(p.Parent) &&
                                                                             p.Parent.DocumentData.DocumentType.Id == firstDocItem.DocumentData.DocumentType.Id).ToList();
                    IndexedItems.Add(firstDocItem);
                    IndexedItems.AddRange(allSelectedDocItems);
                    IndexedItems.AddRange(allSelectedDocOfPageItems);
                    indexedItemCount = IndexedItems.Count;
                    fieldValues = IndexedItems[0].DocumentData.FieldValues;
                }
            }

            if (indexedItemCount > 0)
            {
                ViewerContainer.FieldValues = new ObservableCollection<FieldValueModel>();
                foreach (FieldValueModel fieldValue in fieldValues)
                {
                    if (fieldValue.Field.IsSystemField)
                    {
                        continue;
                    }

                    var clonedFieldValue = new FieldValueModel
                    {
                        Field = fieldValue.Field,
                        ShowMultipleUpdate = indexedItemCount > 1,
                        Value = fieldValue.Value,
                        SnippetImage = fieldValue.SnippetImage,
                        TableValues = fieldValue.TableValues,
                        CanSeeRetrictedField = ViewerContainer.PermissionManager.CanSeeRetrictedField()
                    };

                    clonedFieldValue.CanViewRetrictedField = !clonedFieldValue.Field.IsRestricted || clonedFieldValue.CanSeeRetrictedField;

                    ViewerContainer.FieldValues.Add(clonedFieldValue);

                    // Register event to listen the value of cloned index and then update back the selected documents/batches
                    clonedFieldValue.PropertyChanged += FieldValuePropertyChanged;

                    if (clonedFieldValue.TableValues != null && clonedFieldValue.Field.DataType == FieldDataType.Table)
                    {
                        clonedFieldValue.TableValues.CollectionChanged += TableValues_CollectionChanged;
                    }
                }

                ViewerContainer.IndexViewer.CanUpdateIndexValue = ViewerContainer.PermissionManager.CanModifyIndex();
            }
            else
            {
                ViewerContainer.FieldValues.Clear();
                IndexedItems.Clear();
            }

            ViewerContainer.ShowIndexViewer(true);
        }
        public void PopulateLinkDocumentPanel()
        {
            var linkDocuments = new ObservableCollection<LinkDocumentModel>();
            var indexedItemCount = 0;
            var selectedItems = ViewerContainer.ThumbnailSelector.SelectedItems;
            IndexedItems = new List<ContentItem>();

            if (selectedItems.Count > 0)
            {
                if (selectedItems.Any(p => p.ItemType == ContentItemType.Batch))
                {
                    if (ViewerContainer.DocViewerMode != DocViewerMode.Capture)
                    {
                        if (selectedItems[0].Children.Count(p => p.ItemType == ContentItemType.Document) > 0)
                        {
                            var docItem = selectedItems[0].Children.First(p => p.ItemType == ContentItemType.Document);
                            linkDocuments = docItem.DocumentData.LinkDocuments;//.Select(p => p.LinkedDocument).ToList();
                            indexedItemCount = 1;
                            IndexedItems.Add(docItem);
                        }
                    }
                }
                else if (!selectedItems.Any(p => p.ItemType == ContentItemType.Page && p.Parent.ItemType == ContentItemType.Batch))
                {
                    // Get first document
                    ContentItem firstDocItem;
                    if (selectedItems[0].ItemType == ContentItemType.Document)
                    {
                        firstDocItem = selectedItems[0];
                    }
                    else
                    {
                        firstDocItem = selectedItems[0].Parent;
                    }

                    var allSelectedDocItems = selectedItems.Where(p => p.ItemType == ContentItemType.Document &&
                                                                       p != firstDocItem &&
                                                                       p.DocumentData.DocumentType.Id == firstDocItem.DocumentData.DocumentType.Id).ToList();
                    var allSelectedDocOfPageItems = selectedItems.Where(p => p.ItemType == ContentItemType.Page &&
                                                                             p.Parent != firstDocItem &&
                                                                             !allSelectedDocItems.Contains(p.Parent) &&
                                                                             p.Parent.DocumentData.DocumentType.Id == firstDocItem.DocumentData.DocumentType.Id).ToList();
                    IndexedItems.Add(firstDocItem);
                    IndexedItems.AddRange(allSelectedDocItems);
                    IndexedItems.AddRange(allSelectedDocOfPageItems);
                    indexedItemCount = IndexedItems.Count;
                    linkDocuments = IndexedItems[0].DocumentData.LinkDocuments;//.Select(p=>p.LinkedDocument).ToList();
                }
            }

            if (indexedItemCount > 0)
            {
                ViewerContainer.LinkDocuments = new ObservableCollection<LinkDocumentModel>();
                ViewerContainer.SelectedDocument = IndexedItems[0].DocumentData;
                ViewerContainer.SelectedDocumentType = IndexedItems[0].DocumentData.DocumentType;

                foreach (LinkDocumentModel linkDoc in linkDocuments)
                {
                    var clonedLinkDoc = new LinkDocumentModel
                    {
                        DocumentId = linkDoc.DocumentId,
                        Id = linkDoc.Id,
                        LinkedDocumentId = linkDoc.LinkedDocumentId,
                        LinkedDocument = linkDoc.LinkedDocument,
                        Notes = linkDoc.Notes,
                        RootDocument = linkDoc.RootDocument
                    };

                    ViewerContainer.LinkDocuments.Add(clonedLinkDoc);
                }

            }
            else
            {
                ViewerContainer.LinkDocuments.Clear();
                IndexedItems.Clear();
            }

            ViewerContainer.LinkDocuments.CollectionChanged += LinkDocuments_CollectionChanged;
            ViewerContainer.ShowLinkDocumentViewer(true);
        }

        private void LinkDocuments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ViewerContainer.IsChanged = true;
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public List<ContentItem> IndexedItems { get; private set; }

        public RoutedCommand NewDocumentFromSelectedCommand;

        public RoutedCommand NewDocumentStartingHereCommand;

        public RoutedCommand DeleteCommand;

        public RoutedCommand ChangeDocumentTypeCommand;

        public RoutedCommand CombineDocumentCommand;

        public RoutedCommand RotateRightCommand;

        public RoutedCommand RotateLeftCommand;

        public RoutedCommand IndexCommand;

        public RoutedCommand IndexNextDocCommand;

        public RoutedCommand IndexPreviousDocCommand;

        public RoutedCommand ReplaceByImportCommand;

        public RoutedCommand ReplaceByScannerCommand;

        public RoutedCommand ReplaceByCameraCommand;

        public RoutedCommand InsertBeforeByFileSystemCommand;

        public RoutedCommand InsertBeforeByScannerCommand;

        public RoutedCommand InsertBeforeByCameraCommand;

        public RoutedCommand InsertAfterByFileSystemCommand;

        public RoutedCommand InsertAfterByScannerCommand;

        public RoutedCommand InsertAfterByCameraCommand;

        public RoutedCommand SaveCommand;

        public RoutedCommand SetLanguageCommand;

        public RoutedCommand ImportOCRTemplateCommand;

        public RoutedCommand ScanOCRTemplateCommand;

        public RoutedCommand CameraScanOCRTemplateCommand;

        public RoutedCommand OpenLinkDocumentCommand;

        private void Initialize()
        {
            NewDocumentFromSelectedCommand = new RoutedCommand("NewDocumentFromSelected", typeof(ViewerContainer));
            var commandBinding = new CommandBinding(NewDocumentFromSelectedCommand, NewDocumentFromSelected, CanNewDocumentFromSelected);
            ViewerContainer.CommandBindings.Add(commandBinding);

            NewDocumentStartingHereCommand = new RoutedCommand("NewDocumentStartingHere", typeof(ViewerContainer));
            commandBinding = new CommandBinding(NewDocumentStartingHereCommand, NewDocumentStartingHere, CanNewDocumentStartingHere);
            ViewerContainer.CommandBindings.Add(commandBinding);

            var gesture = new InputGestureCollection { new KeyGesture(Key.Delete, ModifierKeys.None) };
            DeleteCommand = new RoutedCommand("Delete", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(DeleteCommand, Delete, CanDelete);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ChangeDocumentTypeCommand = new RoutedCommand("ChangeDocumentType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ChangeDocumentTypeCommand, ChangeDocumentType, CanChangeDocumentType);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Control) };
            CombineDocumentCommand = new RoutedCommand("CombineDocument", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(CombineDocumentCommand, CombineDocument, CanCombineDocument);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) };
            RotateRightCommand = new RoutedCommand("RotateRight", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(RotateRightCommand, RotateRight, CanRotate);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.L, ModifierKeys.Control) };
            RotateLeftCommand = new RoutedCommand("RotateLeft", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(RotateLeftCommand, RotateLeft, CanRotate);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Enter, ModifierKeys.Control, "Ctrl+Enter") };
            IndexCommand = new RoutedCommand("Index", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(IndexCommand, Index);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) };
            IndexNextDocCommand = new RoutedCommand("IndexNextDoc", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(IndexNextDocCommand, IndexNextDoc, CanIndexNextDoc);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.B, ModifierKeys.Control) };
            IndexPreviousDocCommand = new RoutedCommand("IndexPreviousDoc", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(IndexPreviousDocCommand, IndexPreviousDoc, CanIndexPreviousDoc);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D1, ModifierKeys.Control) };
            ReplaceByScannerCommand = new RoutedCommand("ReplaceByScan", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ReplaceByScannerCommand, ReplaceByScanner, CanReplace);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D2, ModifierKeys.Control) };
            ReplaceByImportCommand = new RoutedCommand("ReplaceByImport", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ReplaceByImportCommand, ReplaceByFileSystem, CanReplace);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D3, ModifierKeys.Control) };
            ReplaceByCameraCommand = new RoutedCommand("ReplaceByCamera", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ReplaceByCameraCommand, ReplaceByCamera, CanReplaceByCamera);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D1, ModifierKeys.Alt) };
            InsertBeforeByScannerCommand = new RoutedCommand("InsertBeforeByScan", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(InsertBeforeByScannerCommand, InsertBeforeByScanner, CanInsertBefore);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D2, ModifierKeys.Alt) };
            InsertBeforeByFileSystemCommand = new RoutedCommand("InsertBeforeByImport", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(InsertBeforeByFileSystemCommand, InsertBeforeByFileSystem, CanInsertBefore);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D3, ModifierKeys.Alt) };
            InsertBeforeByCameraCommand = new RoutedCommand("InsertBeforeByCamera", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(InsertBeforeByCameraCommand, InsertBeforeByCamera, CanInsertBeforeByCamera);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Insert, ModifierKeys.None) };
            InsertAfterByScannerCommand = new RoutedCommand("InsertAfterByScan", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(InsertAfterByScannerCommand, InsertAfterByScanner, CanInsertAfter);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Insert, ModifierKeys.Control) };
            InsertAfterByFileSystemCommand = new RoutedCommand("InsertAfterByImport", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(InsertAfterByFileSystemCommand, InsertAfterByFileSystem, CanInsertAfter);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Insert, ModifierKeys.Shift) };
            InsertAfterByCameraCommand = new RoutedCommand("InsertAfterByCamera", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(InsertAfterByCameraCommand, InsertAfterByCamera, CanInsertAfterByCamera);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            SaveCommand = new RoutedCommand("SaveCommand", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SaveCommand, Save, CanSave);
            ViewerContainer.CommandBindings.Add(commandBinding);

            SetLanguageCommand = new RoutedCommand("SetLanguageCommand", typeof(ViewerContainer));
            commandBinding = new CommandBinding(SetLanguageCommand, SetContentLanguage, CanSetContentLanguage);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Insert, ModifierKeys.None) };
            ScanOCRTemplateCommand = new RoutedCommand("ScanOCRTemplate", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ScanOCRTemplateCommand, ScanOCRTemplate);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Insert, ModifierKeys.Control) };
            ImportOCRTemplateCommand = new RoutedCommand("ImportOCRTemplate", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ImportOCRTemplateCommand, ImportOCRTemplate);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Insert, ModifierKeys.Shift) };
            CameraScanOCRTemplateCommand = new RoutedCommand("CameraScanOCRTemplate", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(CameraScanOCRTemplateCommand, CameraScanOCRTemplate);
            ViewerContainer.CommandBindings.Add(commandBinding);

            OpenLinkDocumentCommand = new RoutedCommand("OpenLinkDocument", typeof(ViewerContainer));
            commandBinding = new CommandBinding(OpenLinkDocumentCommand, OpenLinkDocument);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void OpenLinkDocument(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.DisplayLinkDocumentView();
        }

        private void CanNewDocumentFromSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page) &&
                           ViewerContainer.PermissionManager.CanCreateDocument() &&
                           (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page &&
                                                                                      p.Parent.ItemType == ContentItemType.Document) ||
                            ViewerContainer.PermissionManager.CanSplitDocument());
        }

        private void NewDocumentFromSelected(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as DocumentTypeModel;
                if (documentType != null)
                {
                    var pageItems = ViewerContainer.ThumbnailSelector.SelectedItems.Where(p => p.ItemType == ContentItemType.Page).ToList();
                    CreateDocument(pageItems, documentType);
                    ViewerContainer.DisplayItem();
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanNewDocumentStartingHere(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanCreateDocument() &&
                           ViewerContainer.ThumbnailSelector.Cursor != null &&
                           ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page &&
                           (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                            (ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Document &&
                             ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(ViewerContainer.ThumbnailSelector.Cursor) > 0 &&
                             ViewerContainer.PermissionManager.CanSplitDocument()));

        }

        private void NewDocumentStartingHere(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as DocumentTypeModel;
                if (documentType != null)
                {
                    // Get all pages from the position of cursor in a container (batch or document)
                    var cursorIndex = ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.IndexOf(ViewerContainer.ThumbnailSelector.Cursor);
                    var movedPages = ViewerContainer.ThumbnailSelector.Cursor.Parent.Children.Skip(cursorIndex).Where(p => p.ItemType == ContentItemType.Page).ToList();
                    CreateDocument(movedPages, documentType);
                    ViewerContainer.DisplayItem();
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanDelete();
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            ResourceManager resource = new ResourceManager("Ecm.DocViewer.ViewerContainer", Assembly.GetExecutingAssembly());
            try
            {
                // Delete the selected item and move cursor to other item and then set its selection
                if (DialogService.ShowTwoStateConfirmDialog(resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
                {
                    var tmpItems = new SingleItemList<ContentItem>(ViewerContainer.ThumbnailSelector.SelectedItems);
                    var cleanUpItems = new List<ContentItem>(ViewerContainer.ThumbnailSelector.SelectedItems);
                    foreach (var item in tmpItems)
                    {
                        if (item.ItemType == ContentItemType.Batch)
                        {
                            if (ViewerContainer.DocViewerMode == DocViewerMode.Capture)
                            {
                                ViewerContainer.ThumbnailSelector.MoveSelection();
                                ViewerContainer.Items.Remove(item);
                                ViewerContainer.ThumbnailSelector.RemoveItem(item);
                                ViewerContainer.DocThumbnail.Focus(ViewerContainer.ThumbnailSelector.Cursor);
                            }
                            else
                            {
                                cleanUpItems.Remove(item);
                                cleanUpItems.AddRange(item.Children);
                                item.Children.Clear();
                            }

                            ViewerContainer.DeleteBatch(item);
                        }
                        else if (item.Parent.ItemType == ContentItemType.Document && item.Parent.Children.Count == 1 && ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
                        {
                            item.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                            item.BatchItem.DeletedDocuments.Add(item.Parent);
                            if (ViewerContainer.DocViewerMode == DocViewerMode.Document)
                            {
                                ViewerContainer.DeleteDocument(item.Parent);
                            }

                            ViewerContainer.ThumbnailSelector.MoveSelection();
                            item.BatchItem.Children.Remove(item.Parent);
                            ViewerContainer.ThumbnailSelector.RemoveItem(item.Parent);
                        }
                        else
                        {
                            if (item.ItemType == ContentItemType.Document)
                            {
                                item.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                                item.BatchItem.DeletedDocuments.Add(item);

                                if (ViewerContainer.DocViewerMode == DocViewerMode.Document)
                                {
                                    ViewerContainer.DeleteDocument(item.Parent);
                                }
                                item.Parent.Children.Remove(item);
                            }
                            else if (item.ItemType == ContentItemType.Page)
                            {
                                item.Parent.ChangeType |= ChangeType.DeletePage;
                                item.Parent.DeletedPages.Add(item);
                                item.Parent.Children.Remove(item);

                                if (item.Parent.DocumentData != null)
                                {
                                    item.Parent.DocumentData.Pages.Remove(item.PageData);

                                    if (item.PageData.Id != Guid.Empty)
                                    {
                                        item.Parent.DocumentData.DeletedPages.Add(item.PageData.Id);
                                    }
                                }
                            }

                            ViewerContainer.ThumbnailSelector.MoveSelection();
                            ViewerContainer.ThumbnailSelector.RemoveItem(item);
                        }
                    }

                    ViewerContainer.DocThumbnail.Focus(ViewerContainer.ThumbnailSelector.Cursor);

                    // Clean up items
                    foreach (var item in cleanUpItems)
                    {
                        if (item.ItemType == ContentItemType.Batch)
                        {
                            if (ViewerContainer.DocViewerMode == DocViewerMode.Capture)
                            {
                                item.Clean();
                            }
                            else
                            {
                                item.IsValid = false;
                                foreach (var child in item.Children)
                                {
                                    child.Clean();
                                }
                            }
                        }
                        else
                        {
                            item.Clean();
                        }
                    }

                    ViewerContainer.CollectGarbage();
                }

            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }


        private void CanChangeDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanChangeDocumentType() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                           !ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.Document);
        }

        private void ChangeDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as DocumentTypeModel;
                if (documentType != null)
                {
                    var document = new DocumentModel(DateTime.Now, ViewerContainer.UserName, documentType);
                    foreach (var docItem in ViewerContainer.ThumbnailSelector.SelectedItems)
                    {
                        docItem.SetDocumentData(document);
                        docItem.ChangeType |= ChangeType.ChangeDocumentType;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanCombineDocument(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.PermissionManager.CanCombineDocument())
            {
                var docItem = ViewerContainer.ThumbnailSelector.SelectedItems.FirstOrDefault(p => p.ItemType == ContentItemType.Document);
                e.CanExecute = docItem != null &&
                               (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Document && p != docItem) ||
                                                                                         (p.ItemType == ContentItemType.Page && p.Parent != docItem)));
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void CombineDocument(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var docItem = ViewerContainer.ThumbnailSelector.SelectedItems.First(p => p.ItemType == ContentItemType.Document);
                var movedItems = ViewerContainer.ThumbnailSelector.SelectedItems.Where(p => p.ItemType == ContentItemType.Page && p.Parent != docItem).ToList();
                var movedDocs = ViewerContainer.ThumbnailSelector.SelectedItems.Where(p => p.ItemType == ContentItemType.Document && p != docItem).ToList();
                foreach (var movedDoc in movedDocs)
                {
                    movedItems.AddRange(movedDoc.Children);
                }

                movedItems = ViewerContainer.ContentItemManager.SortPages(movedItems).ToList();
                foreach (var item in movedItems)
                {
                    if (item.Parent.ItemType == ContentItemType.Document && item.Parent.Children.Count == 1)
                    {
                        item.BatchItem.Children.Remove(item.Parent);
                        ViewerContainer.ThumbnailSelector.RemoveItem(item.Parent);
                        if (ViewerContainer.DocViewerMode == DocViewerMode.Document)
                        {
                            ViewerContainer.DeleteDocument(item.Parent);
                        }
                    }

                    if (!movedDocs.Contains(item.Parent))
                    {
                        item.Parent.ChangeType |= ChangeType.DeletePage;
                        item.Parent.DeletedPages.Add(item);
                    }

                    item.Parent.Children.Remove(item);
                    docItem.Children.Add(item);
                    docItem.ChangeType |= ChangeType.NewPage;
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanRotate(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {
                e.CanExecute = ViewerContainer.PermissionManager.CanRotate() &&
                               ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Page && p.PageData.FileType == FileTypeModel.Image) ||
                                                                                        (p.ItemType == ContentItemType.Document && p.Children.Any(q => q.PageData.FileType == FileTypeModel.Image)));
            }
            else
            {
                e.CanExecute = true;
            }
        }

        private void RotateRight(object sender, ExecutedRoutedEventArgs e)
        {
            Rotate(false);
        }

        private void RotateLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Rotate(true);
        }

        private void Rotate(bool left)
        {
            try
            {
                foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                {
                    switch (item.ItemType)
                    {
                        case ContentItemType.Page:
                            if (item.PageData.FileType == FileTypeModel.Image)
                            {
                                if (left)
                                {
                                    item.Image.RotateLeft();
                                }
                                else
                                {
                                    item.Image.RotateRight();
                                }

                                item.ChangeType |= ChangeType.RotatePage;
                            }
                            break;
                        case ContentItemType.Document:
                            item.Children.ToList().ForEach(q =>
                            {
                                // Ignore the item if it is already in the SelectedItems collection, if not, it will be rotated twice
                                if (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(i => i == q))
                                {
                                    if (q.PageData.FileType == FileTypeModel.Image)
                                    {
                                        if (left)
                                        {
                                            q.Image.RotateLeft();
                                        }
                                        else
                                        {
                                            q.Image.RotateRight();
                                        }

                                        q.ChangeType |= ChangeType.RotatePage;
                                    }
                                }
                            });
                            break;
                    }
                }

                ViewerContainer.ArrangeImageViewerLayout();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void Index(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                PopulateIndexPanel();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanIndexNextDoc(object sender, CanExecuteRoutedEventArgs e)
        {
            var allDocs = GetAllDocuments();
            if (allDocs != null && IndexedItems.Count > 0)
            {
                var maxIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Max();
                e.CanExecute = maxIndex < allDocs.Count - 1;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void IndexNextDoc(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var allDocs = GetAllDocuments();
                var maxIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Max();
                ViewerContainer.ThumbnailSelector.LeftClick(allDocs[maxIndex + 1]);
                PopulateIndexPanel();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanIndexPreviousDoc(object sender, CanExecuteRoutedEventArgs e)
        {
            var allDocs = GetAllDocuments();
            if (allDocs != null && IndexedItems.Count > 0)
            {
                var minIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Min();
                e.CanExecute = minIndex > 0;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void IndexPreviousDoc(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var allDocs = GetAllDocuments();
                var minIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Min();
                ViewerContainer.ThumbnailSelector.LeftClick(allDocs[minIndex - 1]);
                PopulateIndexPanel();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanReplace(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanReplace() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page;
        }

        private void ReplaceByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ScanManager.ReplaceContent(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void ReplaceByFileSystem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ImportManager.ReplaceContent(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanReplaceByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanReplace() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page &&
                           ViewerContainer.CameraManager.HasVideoInputDevice;
        }

        private void ReplaceByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.CameraManager.Replace(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanInsertBefore(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanInsert() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page;
        }

        private void InsertBeforeByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ScanManager.InsertContentBefore(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void InsertBeforeByFileSystem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ImportManager.InsertContentBefore(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanInsertBeforeByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page &&
                           ViewerContainer.CameraManager.HasVideoInputDevice;
        }

        private void InsertBeforeByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.CameraManager.InsertBefore(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanInsertAfter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanInsert() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                           (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page ||
                            ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document);
        }

        private void InsertAfterByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document)
                {
                    ViewerContainer.ScanManager.InsertContentAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());
                }
                else
                {
                    ViewerContainer.ScanManager.InsertContentAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void InsertAfterByFileSystem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document)
                {
                    ViewerContainer.ImportManager.InsertContentAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());
                }
                else
                {
                    ViewerContainer.ImportManager.InsertContentAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanInsertAfterByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanInsert() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                           (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page ||
                            ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document) &&
                           ViewerContainer.CameraManager.HasVideoInputDevice;
        }

        private void InsertAfterByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document)
                {
                    ViewerContainer.CameraManager.InsertAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());
                }
                else
                {
                    ViewerContainer.CameraManager.InsertAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.DocViewerMode == DocViewerMode.Document)
            {
                if (ViewerContainer.IsChanged)
                {
                    e.CanExecute = true;
                }
                else { 
                    // This case for work item
                    bool enabled = !ViewerContainer.PermissionManager.CanModifyIndex();

                    if (!enabled)
                    {
                        e.CanExecute = ViewerContainer.Items != null &&
                                       ViewerContainer.Items.Count > 0 &&
                                       ViewerContainer.Items.Any(p => p.IsChanged && p.IsValid) &&
                                       (ViewerContainer.PermissionManager.CanReleaseWithLoosePage() ||
                                        !ViewerContainer.Items.Any(p => p.Children.Any(q => q.ItemType == ContentItemType.Page)));
                    }
                    else
                    {
                        e.CanExecute = ViewerContainer.Items != null &&
                                       ViewerContainer.Items.Count > 0 &&
                                       ViewerContainer.Items.Any(p => p.IsChanged) &&
                                       (ViewerContainer.PermissionManager.CanReleaseWithLoosePage() ||
                                        !ViewerContainer.Items.Any(p => p.Children.Any(q => q.ItemType == ContentItemType.Page)));
                    }
                }
            }
            else
            {
                e.CanExecute = ViewerContainer.Items != null &&
                               ViewerContainer.Items.Count > 0 &&
                               ViewerContainer.Items.Any(p => p.IsChanged && p.IsValid) &&
                               (ViewerContainer.PermissionManager.CanReleaseWithLoosePage() ||
                                !ViewerContainer.Items.Any(p => p.Children.Any(q => q.ItemType == ContentItemType.Page)));
            }

        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.SaveAllBatches();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanSetContentLanguage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;// ViewerContainer.ThumbnailSelector.SelectedItems != null && ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page);
        }

        private void SetContentLanguage(object sender, ExecutedRoutedEventArgs e)
        {
            var languageCode = e.Parameter as string;

            if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0)
            {
                foreach (ContentItem item in ViewerContainer.ThumbnailSelector.SelectedItems)
                {
                    if (item.ItemType == ContentItemType.Document)
                    {
                        foreach (ContentItem childItem in item.Children)
                        {
                            childItem.PageData.ContentLanguageCode = languageCode;
                        }
                    }
                    else if (item.ItemType == ContentItemType.Page)
                    {
                        item.PageData.ContentLanguageCode = languageCode;
                    }
                }
            }
        }

        private void ImportOCRTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            string[] templateFile = Browse();
            ViewerContainer.ImportOCRTemplate(templateFile);
        }

        private void ScanOCRTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ScanManager.ScanOcrTemplate();
        }

        private void CameraScanOCRTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.CameraManager.CaptureOcrTemplate();
        }

        private string[] Browse()
        {
            string filter = "All Files (*.*) | *.*";

            if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
            {
                filter = "All Images|*.BMP;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG";
            }

            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                ValidateNames = true,
                Multiselect = true,
                CheckPathExists = true,
                Filter = filter,
                Title = ViewerContainer.AppName
            };

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileNames;
            }

            return null;
        }

        #region Helper methods

        private void CreateDocument(IEnumerable<ContentItem> pageItems, DocumentTypeModel documentType)
        {
            var document = new DocumentModel(DateTime.Now, ViewerContainer.UserName, documentType);
            var docItem = new ContentItem(document);
            var batchItem = ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem;
            batchItem.Children.Add(docItem);
            pageItems = ViewerContainer.ContentItemManager.SortPages(pageItems.ToList());
            foreach (var pageItem in pageItems.ToList())
            {
                if (pageItem.Parent.ItemType == ContentItemType.Document && pageItem.Parent.Children.Count == 1)
                {
                    pageItem.Parent.Parent.Children.Remove(pageItem.Parent);
                    ViewerContainer.ThumbnailSelector.RemoveItem(pageItem.Parent);
                }

                //switch (pageItem.Parent.ItemType)
                //{
                //    case ContentItemType.Batch:
                //        batchItem.Children.Remove(pageItem);
                //        break;
                //    case ContentItemType.Document:
                //        break;
                //    case ContentItemType.Page:
                //        break;
                //    default:
                //        break;
                //}

                pageItem.Parent.Children.Remove(pageItem);
                docItem.Children.Add(pageItem);
            }

            docItem.ChangeType |= ChangeType.NewDocument;
        }

        private void FieldValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // Update the value back the index of selection documetns/batches
                var clonedFieldValue = sender as FieldValueModel;
                if (clonedFieldValue != null && e.PropertyName == "Value")
                {
                    IndexedItems.ForEach(p =>
                    {
                        FieldValueModel fieldValue = p.ItemType == ContentItemType.Document ? p.DocumentData.FieldValues.FirstOrDefault(q => q.Field.Name == clonedFieldValue.Field.Name) :
                                                                                         p.BatchData.FieldValues.FirstOrDefault(q => q.Field.Name == clonedFieldValue.Field.Name);
                        if (fieldValue != null &&
                            fieldValue.Value + string.Empty != clonedFieldValue.Value + string.Empty)
                        {
                            fieldValue.Value = clonedFieldValue.Value;

                            if (fieldValue.Field.DataType == FieldDataType.Table && clonedFieldValue.TableValues.Count > 0)
                            {
                                fieldValue.TableValues = clonedFieldValue.TableValues;
                            }

                            ViewerContainer.IsChanged = true;
                            p.ChangeType |= ChangeType.UpdateIndex;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void TableValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ViewerContainer.IsChanged = true;
        }


        private List<ContentItem> GetAllDocuments()
        {
            if (ViewerContainer.Items != null && ViewerContainer.Items.Count > 0)
            {
                return (from item in ViewerContainer.Items
                        from subItem in item.Children
                        where subItem.ItemType == ContentItemType.Document
                        select subItem).ToList();
            }

            return null;
        }

        #endregion
    }
}
