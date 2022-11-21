using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System.Resources;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Interop;
using Ecm.Utility;
using System.IO;

namespace Ecm.CaptureViewer.Helper
{
    internal class ThumbnailViewCommand
    {
        ResourceManager _resource = new ResourceManager("Ecm.CaptureViewer.ViewerContainer", Assembly.GetExecutingAssembly());

        public ThumbnailViewCommand(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
            IndexedItems = new List<ContentItem>();
            Initialize();
        }

        public void PopulateIndexPanel(ContentItem item)
        {
            var fieldValues = new List<FieldValueModel>();
            var indexedItemCount = 0;
            var selectedItems = ViewerContainer.ThumbnailSelector.SelectedItems;
            IndexedItems = new List<ContentItem>();
            DocumentPermissionModel documentPermission = null;
            if (item != null)
            {

                // Get first document
                ContentItem firstDocItem = null;

                if (item.ItemType == ContentItemType.Batch)
                {
                    if (selectedItems[0].Children.Count > 0)
                    {
                        firstDocItem = selectedItems[0].Children[0];
                    }
                }
                else
                {
                    if (item.ItemType == ContentItemType.Document)
                    {
                        firstDocItem = selectedItems[0];
                    }
                    else
                    {
                        firstDocItem = selectedItems[0].Parent;
                    }
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
                documentPermission = IndexedItems[0].DocumentData.DocumentPermission;
            }

            ShowIndexPanel(fieldValues, indexedItemCount, documentPermission);
        }

        public void PopulateBatchIndexPanel(ContentItem item)
        {
            var fieldValues = new List<FieldValueModel>();
            var indexedItemCount = 0;
            var selectedItems = ViewerContainer.ThumbnailSelector.SelectedItems;
            IndexedItems = new List<ContentItem>();

            if (item != null)
            {
                if (item.ItemType == ContentItemType.Batch)
                {
                    fieldValues = item.BatchData.FieldValues;
                }
                else
                {
                    if (item.ItemType == ContentItemType.Page)
                    {
                        if (item.Parent.ItemType == ContentItemType.Document)
                        {
                            fieldValues = item.Parent.Parent.BatchData.FieldValues;
                        }
                        else
                        {
                            fieldValues = item.Parent.BatchData.FieldValues;
                        }
                    }
                    else
                    {
                        fieldValues = item.Parent.BatchData.FieldValues;
                    }
                }
                indexedItemCount = 1;
                IndexedItems.Add(item.BatchItem);
            }

            ShowIndexPanel(fieldValues, indexedItemCount, null);
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public List<ContentItem> IndexedItems { get; private set; }

        public RoutedCommand NewDocumentFromSelectedCommand;

        public RoutedCommand NewDocumentStartingHereCommand;

        public RoutedCommand DeleteCommand;

        public RoutedCommand ChangeContentTypeCommand;

        public RoutedCommand ChangeBatchTypeCommand;

        // HungLe - 2014/07/18 - Adding rename batch - Start
        public RoutedCommand RenameBatchCommand;
        // HungLe - 2014/07/18 - Adding rename batch - Start

        public RoutedCommand CombineDocumentCommand;

        public RoutedCommand RotateRightCommand;

        public RoutedCommand RotateLeftCommand;

        public RoutedCommand IndexCommand;

        public RoutedCommand BatchIndexCommand;

        public RoutedCommand IndexNextDocumentCommand;

        public RoutedCommand IndexPreviousDocumentCommand;

        public RoutedCommand ReplaceByFileSystemCommand;

        public RoutedCommand ReplaceByScannerCommand;

        public RoutedCommand ReplaceByCameraCommand;

        public RoutedCommand InsertBeforeByFileSystemCommand;

        public RoutedCommand InsertBeforeByScannerCommand;

        public RoutedCommand InsertBeforeByCameraCommand;

        public RoutedCommand InsertAfterByFileSystemCommand;

        public RoutedCommand InsertAfterByScannerCommand;

        public RoutedCommand InsertAfterByCameraCommand;

        public RoutedCommand SaveCommand;

        public RoutedCommand CreateNewBatchCommand;

        public RoutedCommand RejectCommand;

        public RoutedCommand ApproveCommand;

        public RoutedCommand UnRejectCommand;

        public RoutedCommand SendLinkCommand;

        public RoutedCommand SubmitBatchCommand;

        public RoutedCommand CommentCommand;

        public RoutedCommand SetLanguageCommand;

        public RoutedCommand ChangeNameCommand;

        public RoutedCommand ImportOCRTemplateCommand;

        public RoutedCommand ScanOCRTemplateCommand;

        public RoutedCommand CameraScanOCRTemplateCommand;

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

            ChangeContentTypeCommand = new RoutedCommand("ChangeContentType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ChangeContentTypeCommand, ChangeContentType, CanChangeContentType);
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

            gesture = new InputGestureCollection { new KeyGesture(Key.Enter, ModifierKeys.Control, "Ctrl + Enter") };
            IndexCommand = new RoutedCommand("Index", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(IndexCommand, Index, CanIndex);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Enter, ModifierKeys.Alt, "Alt + Enter") };
            BatchIndexCommand = new RoutedCommand("BatchIndex", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(BatchIndexCommand, BatchIndex, CanBatchIndex);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) };
            IndexNextDocumentCommand = new RoutedCommand("IndexNextDoc", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(IndexNextDocumentCommand, IndexNextDoc, CanIndexNextDoc);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.B, ModifierKeys.Control) };
            IndexPreviousDocumentCommand = new RoutedCommand("IndexPreviousDoc", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(IndexPreviousDocumentCommand, IndexPreviousDoc, CanIndexPreviousDoc);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D1, ModifierKeys.Control) };
            ReplaceByScannerCommand = new RoutedCommand("ReplaceByScan", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ReplaceByScannerCommand, ReplaceByScanner, CanReplace);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.D2, ModifierKeys.Control) };
            ReplaceByFileSystemCommand = new RoutedCommand("ReplaceByImport", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(ReplaceByFileSystemCommand, ReplaceByFileSystem, CanReplace);
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

            gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            SaveCommand = new RoutedCommand("SaveCommand", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(SaveCommand, Save, CanSave);
            ViewerContainer.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Alt) };
            CreateNewBatchCommand = new RoutedCommand("CreateBatchCommand", typeof(ViewerContainer), gesture);
            commandBinding = new CommandBinding(CreateNewBatchCommand, CreateNewBatch, CanCreateNewBatch);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ChangeBatchTypeCommand = new RoutedCommand("ChangeBatchType", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ChangeBatchTypeCommand, ChangeBatchType, CanChangeBatchType);
            ViewerContainer.CommandBindings.Add(commandBinding);

            // HungLe - 2014/07/18 - Adding rename batch command - Start
            RenameBatchCommand = new RoutedCommand("RenameBatch", typeof(ViewerContainer));
            commandBinding = new CommandBinding(RenameBatchCommand, RenameBatch, CanRenameBatch);
            ViewerContainer.CommandBindings.Add(commandBinding);
            // HungLe - 2014/07/18 - Adding rename batch command - End

            RejectCommand = new RoutedCommand("Reject", typeof(ViewerContainer));
            commandBinding = new CommandBinding(RejectCommand, Reject, CanReject);
            ViewerContainer.CommandBindings.Add(commandBinding);

            UnRejectCommand = new RoutedCommand("UnReject", typeof(ViewerContainer));
            commandBinding = new CommandBinding(UnRejectCommand, UnReject, CanUnReject);
            ViewerContainer.CommandBindings.Add(commandBinding);

            SendLinkCommand = new RoutedCommand("SendLink", typeof(ViewerContainer));
            commandBinding = new CommandBinding(SendLinkCommand, SendLink, CanSendLink);
            ViewerContainer.CommandBindings.Add(commandBinding);

            SubmitBatchCommand = new RoutedCommand("Submit", typeof(ViewerContainer));
            commandBinding = new CommandBinding(SubmitBatchCommand, SubmitBatch, CanSubmitBatch);
            ViewerContainer.CommandBindings.Add(commandBinding);

            CommentCommand = new RoutedCommand("Comment", typeof(ViewerContainer));
            commandBinding = new CommandBinding(CommentCommand, DisplayComment, CanShowComment);
            ViewerContainer.CommandBindings.Add(commandBinding);

            SetLanguageCommand = new RoutedCommand("SetLanguageCommand", typeof(ViewerContainer));
            commandBinding = new CommandBinding(SetLanguageCommand, SetContentLanguage, CanSetContentLanguage);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ApproveCommand = new RoutedCommand("Approve", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ApproveCommand, Approve, CanApprove);
            ViewerContainer.CommandBindings.Add(commandBinding);

            ChangeNameCommand = new RoutedCommand("ChangeName", typeof(ViewerContainer));
            commandBinding = new CommandBinding(ChangeNameCommand, ChangeName, CanChangeName);
            ViewerContainer.CommandBindings.Add(commandBinding);
        }

        private void ShowIndexPanel(List<FieldValueModel> fieldValues, int indexedItemCount, DocumentPermissionModel documentPermission)
        {
            if (indexedItemCount > 0)
            {
                ViewerContainer.FieldValues = new ObservableCollection<FieldValueModel>();
                foreach (FieldValueModel fieldValue in fieldValues)
                {
                    if (fieldValue.Field.IsSystemField)
                    {
                        continue;
                    }

                    if (fieldValue.Field.IsRestricted && (documentPermission != null && !documentPermission.CanSeeRestrictedField))
                    {
                        continue;
                    }

                    DocumentFieldPermissionModel fieldPermission = null;
                    if (documentPermission != null && documentPermission.FieldPermissions != null)
                    {
                        fieldPermission = documentPermission.FieldPermissions.SingleOrDefault(p => p.FieldId == fieldValue.Field.Id);
                    }
                    else
                    {
                        fieldPermission = new DocumentFieldPermissionModel
                        {
                            CanRead = true,
                            CanWrite = true,
                            FieldId = fieldValue.Field.Id,
                        };
                    }

                    var isHidden = false;
                    if (fieldPermission == null)
                    {
                        isHidden = true;
                    }
                    else
                    {
                        if (fieldPermission.Hidden)
                        {
                            isHidden = true;
                        }
                        else if (fieldValue.Field.IsRestricted)
                        {
                            isHidden = documentPermission == null ? false : !documentPermission.CanSeeRestrictedField;
                        }
                    }

                    var clonedFieldValue = new FieldValueModel
                    {
                        Field = fieldValue.Field,
                        ShowMultipleUpdate = indexedItemCount > 1,
                        Value = fieldValue.Value,
                        SnippetImage = fieldValue.SnippetImage,
                        IsHidden = isHidden,
                        IsReadOnly = fieldPermission == null ? false : fieldPermission.CanRead && !fieldPermission.CanWrite && !fieldValue.Field.IsRequired,
                        IsWrite = fieldPermission == null ? false : fieldPermission.CanWrite,
                        TableValues = fieldValue.TableValues,
                        MultipleUpdate = fieldValue.MultipleUpdate,
                        DocPermission = documentPermission
                    };

                    ViewerContainer.FieldValues.Add(clonedFieldValue);

                    // Register event to listen the value of cloned index and then update back the selected documents/batches
                    clonedFieldValue.PropertyChanged += FieldValuePropertyChanged;
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

        private void CreateNewBatch(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DialogViewer viewer = new DialogViewer();
                ResourceManager resource = new ResourceManager("Ecm.CaptureViewer.BatchTypeSelection", Assembly.GetExecutingAssembly());

                viewer.Text = resource.GetString("Title");

                BatchTypeSelection batchTypeSelection = new BatchTypeSelection
                {
                    Dialog = viewer,
                    SelectedBatchType = ViewerContainer.SelectedBatchType,
                    IsReopenDialog = ViewerContainer.IsReopenDialog,
                    IsAutoCreateBatch = ViewerContainer.IsAutoCreateBatch,
                    BatchTypes = ViewerContainer.BatchTypes
                };

                viewer.WpfContent = batchTypeSelection;

                if (ViewerContainer.SelectedBatchType != null)
                {
                    ViewerContainer.SelectedBatchType.IsSelected = true;
                }

                if (viewer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ViewerContainer.SelectedBatchType = batchTypeSelection.SelectedBatchType;
                    ViewerContainer.IsAutoCreateBatch = batchTypeSelection.IsAutoCreateBatch;
                    ViewerContainer.IsReopenDialog = batchTypeSelection.IsReopenDialog;
                    ViewerContainer.ContentItemManager.CreateBatch(batchTypeSelection.SelectedBatchType);
                    ViewerContainer.ThumbnailSelector.LeftMouseClick(ViewerContainer.Items.Last());
                    ViewerContainer.DocThumbnail.ShowContextMenu(ViewerContainer.Items.Last());
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanCreateNewBatch(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.BatchTypes != null && ViewerContainer.BatchTypes.Count > 0 && ViewerContainer.DocViewerMode != DocViewerMode.WorkItem;
        }

        private void CanNewDocumentFromSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page) &&
                           ViewerContainer.PermissionManager.CanCreateDocument() &&
                           (!ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page &&
                                                                                      p.Parent.ItemType == ContentItemType.Document) ||
                            ViewerContainer.PermissionManager.CanSplitDocument()) && ViewerContainer.PermissionManager.CanClassify();
        }

        private void NewDocumentFromSelected(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as DocTypeModel;
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
                             ViewerContainer.PermissionManager.CanSplitDocument())) && ViewerContainer.PermissionManager.CanClassify();

        }

        private void NewDocumentStartingHere(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as DocTypeModel;
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
            e.CanExecute = ViewerContainer.PermissionManager.CanDelete();// && ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate;
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            ResourceManager resource = new ResourceManager("Ecm.CaptureViewer.ViewerContainer", Assembly.GetExecutingAssembly());
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
                                ViewerContainer.ThumbnailSelector.RemoveContentItem(item);
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
                            item.BatchItem.BatchData.Documents.Remove(item.Parent.DocumentData);
                            ViewerContainer.ThumbnailSelector.RemoveContentItem(item.Parent);
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
                                item.Parent.BatchData.Documents.Remove(item.DocumentData);

                                if (item.DocumentData.Id != Guid.Empty)
                                {
                                    item.Parent.BatchData.DeletedDocuments.Add(item.DocumentData.Id);
                                }
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
                            ViewerContainer.ThumbnailSelector.RemoveContentItem(item);
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

                    if (ViewerContainer.Items.Count == 0)
                    {
                        if (ViewerContainer.CommentView != null
                            && ViewerContainer.CommentView.Comments != null)
                        {
                            ViewerContainer.CommentView.Comments.Clear();

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanChangeContentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanChangeDocumentType() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                           !ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.Document);
        }

        private void ChangeContentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as DocTypeModel;
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

        private void CanChangeBatchType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanChangeBatchType() && ViewerContainer.PermissionManager.CanClassify();
        }

        private void ChangeBatchType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (ViewerContainer.ThumbnailSelector.Cursor.BatchItem.Children.Any(p => p.ItemType == ContentItemType.Document))
                {
                    if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiChangeBatchTypeConfirmation")) != DialogServiceResult.Yes)
                    {
                        return;
                    }
                }

                var dialog = new DialogViewer { Text = new ResourceManager("Ecm.CaptureViewer.BatchTypeSelection", Assembly.GetExecutingAssembly()).GetString("tbTitle") };
                var batchTypeSelection = new BatchTypeSelection
                {
                    Dialog = dialog,
                    BatchTypes = new ObservableCollection<BatchTypeModel>(ViewerContainer.BatchTypes.Where(p => p != ViewerContainer.ThumbnailSelector.Cursor.BatchData.BatchType))
                };

                if (ViewerContainer.SelectedBatchType != null)
                {
                    ViewerContainer.SelectedBatchType.IsSelected = true;
                }

                dialog.WpfContent = batchTypeSelection;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var batchType = batchTypeSelection.SelectedBatchType;

                    BatchModel batch = new BatchModel(Guid.NewGuid(), DateTime.Now, ViewerContainer.UserName, batchType) { Permission = new BatchPermissionModel() };
                    batch.Permission.CanAnnotate = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanChangeDocumentType = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanDelegateItems = false;
                    batch.Permission.CanDelete = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanEmail = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanInsertDocument = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanModifyDocument = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanModifyIndexes = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanPrint = batchType.BatchTypePermission.CanCapture;
                    batch.Permission.CanReject = false;
                    batch.Permission.CanReleaseLoosePage = true;
                    batch.Permission.CanSendLink = false;
                    batch.Permission.CanSplitDocument = batchType.BatchTypePermission.CanCapture;

                    ViewerContainer.ThumbnailSelector.Cursor.SetBatchData(batch);//new BatchModel(Guid.Empty, DateTime.Now, ViewerContainer.UserName, batchTypeSelection.SelectedBatchType));
                    ViewerContainer.SelectedBatchType = batchTypeSelection.SelectedBatchType;
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        // HungLe - 2014/07/18 - Adding rename batch command - Start
        private void CanRenameBatch(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // ViewerContainer.PermissionManager.CanChangeBatchType() && ViewerContainer.PermissionManager.CanClassify();
        }

        private void RenameBatch(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var contentNameDialog = new DialogViewer();
                contentNameDialog.Text = ContentNameInputResouces.BatchNameTitle;
                var contentNameInput = new ContentNameInput()
                {
                    Dialog = contentNameDialog,
                    ContentName = string.Format("{0}", ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchData.BatchName),
                    MinWidth = 350,
                    MaxWidth = 350
                };
                contentNameDialog.WpfContent = contentNameInput;
                var contentName = string.Empty;
                contentNameDialog.Shown += contentNameDialog_Shown;
                contentNameDialog.SizeToContent();
                if (contentNameDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchData.BatchName = contentNameInput.ContentName;
                    ViewerContainer.ThumbnailSelector.SelectedItems[0].Title1 = contentNameInput.ContentName;
                }
                contentNameDialog.Dispose();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        /// <summary>
        /// Focus input content name text box and select all text.
        /// </summary>
        /// <param name="sender">Type of DialogViewer</param>
        /// <param name="e"></param>
        void contentNameDialog_Shown(object sender, EventArgs e)
        {
            var dialog = sender as DialogViewer;
            var contentNameInput = dialog.WpfContent as ContentNameInput;
            contentNameInput.Focus();
            contentNameInput.txtName.SelectAll();
        }
        // HungLe - 2014/07/18 - Adding rename batch command - End

        private void CanCombineDocument(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.PermissionManager.CanCombineDocument())
            {
                var docItem = ViewerContainer.ThumbnailSelector.SelectedItems.FirstOrDefault(p => p.ItemType == ContentItemType.Document);
                e.CanExecute = docItem != null &&
                               (ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Document && p != docItem) ||
                                                                                         (p.ItemType == ContentItemType.Page && p.Parent != docItem)));
                //&& ViewerContainer.PermissionManager.CanClassify();
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
                        ViewerContainer.ThumbnailSelector.RemoveContentItem(item.Parent);
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
                //&& ViewerContainer.PermissionManager.CanClassify();
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

        private void CanIndex(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.ThumbnailSelector != null && ViewerContainer.ThumbnailSelector.Cursor != null && ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page && ViewerContainer.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = (ViewerContainer.ThumbnailSelector != null && ViewerContainer.ThumbnailSelector.Cursor != null &&
                    ((ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Document && ViewerContainer.ThumbnailSelector.Cursor.DocumentData.DocumentType != null) ||
                    (ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page && ViewerContainer.ThumbnailSelector.Cursor.Parent.DocumentData.DocumentType != null) ||
                    (ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch && ViewerContainer.ThumbnailSelector.Cursor.Children.Count > 0 && ViewerContainer.ThumbnailSelector.Cursor.DocumentData != null && ViewerContainer.ThumbnailSelector.Cursor.DocumentData.DocumentType != null))) &&
                    ViewerContainer.PermissionManager.CanIndex();
            }
        }

        private void CanBatchIndex(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ThumbnailSelector != null && ViewerContainer.ThumbnailSelector.Cursor != null && ViewerContainer.PermissionManager.CanIndex();
        }

        private void Index(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                PopulateIndexPanel(ViewerContainer.ThumbnailSelector.Cursor);
                ViewerContainer.ButtonNextPage.Visibility = System.Windows.Visibility.Visible;
                ViewerContainer.ButtonPreviousPage.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void BatchIndex(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                PopulateBatchIndexPanel(ViewerContainer.ThumbnailSelector.Cursor);
                ViewerContainer.ButtonNextPage.Visibility = System.Windows.Visibility.Hidden;
                ViewerContainer.ButtonPreviousPage.Visibility = System.Windows.Visibility.Hidden;
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

                do
                {
                    if (maxIndex >= allDocs.Count - 1)
                    {
                        e.CanExecute = false;
                        break;
                    }
                    else if (!allDocs[maxIndex + 1].DocumentData.DocumentType.DocTypePermission.CanAccess)
                    {
                        maxIndex += 1;
                    }
                    else
                    {
                        e.CanExecute = ViewerContainer.PermissionManager.CanIndex();

                        break;
                    }
                } while (true);

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

                do
                {
                    if (!allDocs[maxIndex + 1].DocumentData.DocumentType.DocTypePermission.CanAccess)
                    {
                        maxIndex += 1;
                    }
                    else
                    {
                        ViewerContainer.ThumbnailSelector.LeftMouseClick(allDocs[maxIndex + 1]);
                        PopulateIndexPanel(ViewerContainer.ThumbnailSelector.Cursor);
                        break;
                    }
                } while (true);


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

                do
                {
                    if (minIndex <= 0)
                    {
                        e.CanExecute = false;
                        break;
                    }
                    else if (!allDocs[minIndex - 1].DocumentData.DocumentType.DocTypePermission.CanAccess)
                    {
                        minIndex -= 1;
                    }
                    else
                    {
                        e.CanExecute = ViewerContainer.PermissionManager.CanIndex();
                        break;
                    }
                } while (true);
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

                do
                {
                    if (!allDocs[minIndex - 1].DocumentData.DocumentType.DocTypePermission.CanAccess)
                    {
                        minIndex -= 1;
                    }
                    else
                    {
                        ViewerContainer.ThumbnailSelector.LeftMouseClick(allDocs[minIndex - 1]);
                        PopulateIndexPanel(ViewerContainer.ThumbnailSelector.Cursor);
                        break;
                    }
                } while (true);

            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanReplace(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {
                e.CanExecute = ViewerContainer.PermissionManager.CanReplace() &&
                               ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                               ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page;
                //&& ViewerContainer.PermissionManager.CanClassify();
            }
            else
            {
                e.CanExecute = true;
            }
        }

        private void ReplaceByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ScanManager.ReplaceContent(ViewerContainer.ThumbnailSelector.SelectedItems[0],
                    () =>
                    {
                        foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                        {
                            item.Rejected = false;
                            item.ChangeType |= ChangeType.UnReject;

                            RejectAndUnreject(item, false);
                        }
                    });
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
                ViewerContainer.ImportManager.ReplaceContent(ViewerContainer.ThumbnailSelector.SelectedItems[0],
                    () =>
                    {
                        foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                        {
                            item.Rejected = false;
                            item.ChangeType |= ChangeType.UnReject;

                            RejectAndUnreject(item, false);
                        }
                    });
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanReplaceByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {
                e.CanExecute = ViewerContainer.PermissionManager.CanReplace() &&
                               ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                               ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page &&
                               ViewerContainer.CameraManager.HasVideoInputDevice;
                //&& ViewerContainer.PermissionManager.CanClassify();
            }
            else
            {
                e.CanExecute = true;
            }
        }

        private void ReplaceByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.CameraManager.Replace(ViewerContainer.ThumbnailSelector.SelectedItems[0],
                    () =>
                    {
                        foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                        {
                            item.Rejected = false;
                            item.ChangeType |= ChangeType.UnReject;

                            RejectAndUnreject(item, false);
                        }
                    });
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanInsertBefore(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {

                e.CanExecute = ViewerContainer.PermissionManager.CanInsert() &&
                               ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                               ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page;
                //&& ViewerContainer.PermissionManager.CanClassify();
            }
            else
            {
                e.CanExecute = true;
            }
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
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {
                e.CanExecute = ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                               ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page &&
                               ViewerContainer.CameraManager.HasVideoInputDevice && ViewerContainer.PermissionManager.CanClassify();
            }
            else
            {
                e.CanExecute = true;
            }
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
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {

                e.CanExecute = ViewerContainer.PermissionManager.CanInsert() &&
                               ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                               (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page ||
                                ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document);// && ViewerContainer.PermissionManager.CanClassify();
            }
            else
            {
                e.CanExecute = true;
            }
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

        private void ImportOCRTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            string[] templateFile = Browse();

            if (templateFile != null && templateFile.Length > 0)
            {
                ViewerContainer.ImportOCRTemplate(templateFile);
            }
        }

        private void ScanOCRTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            string[] templateFile = ViewerContainer.ScanManager.ScanOcrTemplate();

            if (templateFile != null && templateFile.Length > 0)
            {
                ViewerContainer.ImportOCRTemplate(templateFile);
            }
        }

        private void CameraScanOCRTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Count() > 0)
            {
                ViewerContainer.CameraManager.InsertAfter(ViewerContainer.ThumbnailSelector.SelectedItems[0].Children.Last());
            }
            else
            {
                ViewerContainer.CameraManager.CaptureOcrTemplate();
            }
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
            if (ViewerContainer.DocViewerMode != DocViewerMode.OCRTemplate)
            {
                e.CanExecute = ViewerContainer.PermissionManager.CanInsert() &&
                                           ViewerContainer.ThumbnailSelector.SelectedItems.Count == 1 &&
                                           (ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page ||
                                            ViewerContainer.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Document) &&
                                           ViewerContainer.CameraManager.HasVideoInputDevice;
                //&& ViewerContainer.PermissionManager.CanClassify();
            }
            else
            {
                e.CanExecute = true;
            }
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
            if (ViewerContainer == null || ViewerContainer.Items == null)
            {
                e.CanExecute = false;
                return;
            }

            bool valid = true;

            foreach (ContentItem item in ViewerContainer.Items)
            {
                valid &= item.IsValid;
                foreach (ContentItem doc in item.Children)
                {
                    try
                    {
                        if (doc.DocumentData != null)
                        {
                            valid &= !doc.DocumentData.FieldValues.Any(p => !p.IsValid);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            if (ViewerContainer.DocViewerMode == DocViewerMode.WorkItem)
            {
                // This case for work item
                e.CanExecute = (ViewerContainer.IsChanged || ViewerContainer.DocViewerMode == DocViewerMode.WorkItem) &&
                               !ViewerContainer.IsProcessing &&
                               ViewerContainer.Items != null &&
                               ViewerContainer.Items.Count != 0 && valid &&
                               !ViewerContainer.Items.Any(p => p.BatchData.IsCompleted ||
                                                                 p.Children.Count == 0 ||
                                                                 (p.Children.Any(q => q.ItemType == ContentItemType.Page) && !p.BatchData.Permission.CanReleaseLoosePage) ||
                                                                 p.Children.Any(q => q.ItemType == ContentItemType.Document && !q.IsValid));
            }
            else
            {
                e.CanExecute = ViewerContainer.Items != null &&
                               ViewerContainer.Items.Count > 0 &&
                               ViewerContainer.Items.Any(p => p.IsChanged) && valid &&
                               (ViewerContainer.PermissionManager.CanReleaseWithLoosePage() ||
                                !ViewerContainer.Items.Any(p => p.Children.Any(q => q.ItemType == ContentItemType.Page)));
            }
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.SaveAllBatches();
                if (ViewerContainer.CommentView != null
                    && ViewerContainer.CommentView.Comments != null)
                {
                    ViewerContainer.CommentView.Comments.Clear();
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanSubmitBatch(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewerContainer.DocViewerMode == DocViewerMode.WorkItem)
            {
                // This case for work item
                e.CanExecute = (ViewerContainer.IsChanged || ViewerContainer.DocViewerMode == DocViewerMode.WorkItem) &&
                               !ViewerContainer.IsProcessing &&
                               ViewerContainer.Items != null &&
                               ViewerContainer.Items.Count != 0 &&
                               !ViewerContainer.Items.Any(p => p.BatchData.IsCompleted ||
                                                                 p.Children.Count == 0 || !p.IsValid ||
                                                                 (p.Children.Any(q => q.ItemType == ContentItemType.Page) && !p.BatchData.Permission.CanReleaseLoosePage) ||
                                                                 p.Children.Any(q => q.ItemType == ContentItemType.Document && !q.IsValid));
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

        private void SubmitBatch(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.SubmitBatch();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanApprove(object sender, CanExecuteRoutedEventArgs e)
        {
            bool valid = true;

            foreach (ContentItem item in ViewerContainer.Items)
            {
                valid &= item.IsValid;
                foreach (ContentItem doc in item.Children)
                {
                    if (doc.DocumentData != null)
                    {
                        valid &= !doc.DocumentData.FieldValues.Any(p => !p.IsValid);
                    }
                }
            }

            e.CanExecute = (ViewerContainer.IsChanged || ViewerContainer.DocViewerMode == DocViewerMode.WorkItem) &&
                           !ViewerContainer.IsProcessing &&
                           ViewerContainer.Items != null &&
                           ViewerContainer.Items.Count != 0 && valid &&
                           !ViewerContainer.Items.Any(p => p.BatchData.IsCompleted || p.Rejected ||
                                                             p.Children.Count == 0 ||
                                                             (p.Children.Any(q => q.ItemType == ContentItemType.Page) && !p.BatchData.Permission.CanReleaseLoosePage) ||
                                                             p.Children.Any(q => q.ItemType == ContentItemType.Document && !q.IsValid));
        }

        private void Approve(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerContainer.ApproveBatch();
        }

        private void CanReject(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanReject() &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => !p.Rejected ||
                                                                             (p.ItemType == ContentItemType.Document && p.Children.Any(q => !q.Rejected)) ||
                                                                             (p.ItemType == ContentItemType.Batch && p.Children.Any(q => !q.Rejected || (q.ItemType == ContentItemType.Document && q.Children.Any(r => !r.Rejected)))));
        }

        private void Reject(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                {
                    item.Rejected = true;
                    item.ChangeType |= ChangeType.Reject;

                    RejectAndUnreject(item, true);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }

        }

        private void CanUnReject(object sender, CanExecuteRoutedEventArgs e)
        {
            bool hasReject = ViewerContainer.ThumbnailSelector.SelectedItems.Any(p => p.Rejected ||
                                                                               (p.ItemType == ContentItemType.Document && p.Children.Any(q => q.Rejected)) ||
                                                                               (p.ItemType == ContentItemType.Batch && p.Children.Any(q => q.Rejected ||
                                                                                                                                     (q.ItemType == ContentItemType.Document && q.Children.Any(r => r.Rejected)))));
            e.CanExecute = hasReject;
        }

        private void UnReject(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                {
                    item.Rejected = false;
                    item.ChangeType |= ChangeType.UnReject;

                    RejectAndUnreject(item, false);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanSendLink(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.Children.Count > 0 &&
                           ViewerContainer.PermissionManager.CanSendLink();
        }

        private void SendLink(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string url = BrowserInteropHelper.Source.AbsoluteUri;
                if (BrowserInteropHelper.Source.Query != string.Empty)
                {
                    url = BrowserInteropHelper.Source.AbsoluteUri.Replace(BrowserInteropHelper.Source.Query, string.Empty);
                }

                const string queryTemplate = "mode=workitem&username={0}&workitemid={1}";
                string newLine = Environment.NewLine;
                string body = string.Empty;

                var encodedUri = new Uri("http://localhost/index.html?" + string.Format(queryTemplate, ViewerContainer.UserName, ViewerContainer.ThumbnailSelector.Cursor.BatchData.Id.ToString()));
                body += newLine + url + encodedUri.Query;

                if (body != string.Empty)
                {
                    body = _resource.GetString("uiEmailBody") + newLine + newLine + body.Substring(newLine.Length);
                    var mapi = new UtilsMapi();
                    mapi.SendMailPopup(_resource.GetString("uiEmailSubject"), body);
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanShowComment(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.ThumbnailSelector.SelectedItems.Count > 0 &&
                           ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.Children.Count > 0;
        }

        private void DisplayComment(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ViewerContainer.ShowCommentViewer(true);
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void CanSetContentLanguage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
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

        // HungLe - 2014/07/18 - Adding rename batch command - Start
        private void CanChangeName(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewerContainer.PermissionManager.CanModifiedDocument();
        }

        private void ChangeName(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var contentNameDialog = new DialogViewer();
                contentNameDialog.Text = ContentNameInputResouces.DocNameTitle;
                var contentNameInput = new ContentNameInput()
                {
                    Dialog = contentNameDialog,
                    ContentName = string.Format("{0}", ViewerContainer.ThumbnailSelector.SelectedItems[0].DocumentData.DocName),
                    MinWidth = 350,
                    MaxWidth = 350
                };
                if (ViewerContainer.ThumbnailSelector.SelectedItems.Count > 1)
                {
                    contentNameInput.ContentName = string.Empty;
                }

                contentNameDialog.WpfContent = contentNameInput;
                var contentName = string.Empty;
                contentNameDialog.Shown += contentNameDialog_Shown;
                contentNameDialog.SizeToContent();
                if (contentNameDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var item in ViewerContainer.ThumbnailSelector.SelectedItems)
                    {
                        item.DocumentData.DocName = contentNameInput.ContentName;
                        item.Title1 = (item.Parent.Children.Where(p => p.ItemType == ContentItemType.Document).ToList().IndexOf(item) + 1) +
                                      ". " + contentNameInput.ContentName;
                    }
                }
                contentNameDialog.Dispose();
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }
        // HungLe - 2014/07/18 - Adding rename batch command - End

        #region Helper methods

        private void CreateDocument(IEnumerable<ContentItem> pageItems, DocTypeModel documentType)
        {
            var document = new DocumentModel(DateTime.Now, ViewerContainer.UserName, documentType);
            var docItem = new ContentItem(document);
            var batchItem = ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem;
            batchItem.Children.Add(docItem);
            pageItems = ViewerContainer.ContentItemManager.SortPages(pageItems.ToList());
            foreach (var pageItem in pageItems)
            {
                if (pageItem.Parent.ItemType == ContentItemType.Document && pageItem.Parent.Children.Count == 1)
                {
                    pageItem.Parent.Parent.Children.Remove(pageItem.Parent);
                    ViewerContainer.ThumbnailSelector.RemoveContentItem(pageItem.Parent);
                }

                pageItem.Parent.Children.Remove(pageItem);
                docItem.Children.Add(pageItem);
                docItem.DocumentData.Pages.Add(pageItem.PageData);
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
                            fieldValue.MultipleUpdate = clonedFieldValue.MultipleUpdate;
                            fieldValue.ShowMultipleUpdate = clonedFieldValue.ShowMultipleUpdate;

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

        private void RejectAndUnreject(ContentItem item, bool isReject)
        {
            ChangeType changeStatus = ChangeType.Reject;

            if (!isReject)
            {
                changeStatus = ChangeType.UnReject;
            }

            if (item.ItemType == ContentItemType.Page)
            {
                item.PageData.IsRejected = isReject;

                if (!isReject)
                {
                    if (item.Parent.DocumentData.Pages.Any(p => p.IsRejected))
                    {
                        item.Parent.DocumentData.IsRejected = true;
                    }
                    else
                    {
                        item.Parent.DocumentData.IsRejected = isReject;
                    }
                }
                else
                {
                    item.Parent.DocumentData.IsRejected = isReject;
                }

                item.Parent.Parent.BatchData.IsRejected = isReject;

            }
            else if (item.ItemType == ContentItemType.Document)
            {
                item.DocumentData.IsRejected = isReject;

                if (!isReject)
                {
                    if (item.Parent.BatchData.Documents.Any(p => p.IsRejected))
                    {
                        item.Parent.BatchData.IsRejected = true;
                    }
                    else
                    {
                        item.Parent.BatchData.IsRejected = false;
                    }
                }

                //item.Parent.BatchData.IsRejected = isReject;

                foreach (var subItem in item.Children)
                {
                    subItem.PageData.IsRejected = isReject;
                }
            }
            else
            {
                item.BatchData.IsRejected = isReject;

                foreach (var childItem in item.Children)
                {
                    childItem.DocumentData.IsRejected = isReject;

                    foreach (var subItem in childItem.Children)
                    {
                        subItem.PageData.IsRejected = isReject;
                    }
                }

            }

            if (item.Parent != null)
            {
                if (isReject)
                {
                    item.Parent.Rejected = true;
                    item.Parent.ChangeType |= ChangeType.Reject;
                    item.BatchItem.Rejected = true;
                    item.BatchItem.ChangeType |= ChangeType.Reject;
                }
                else
                {
                    if (!item.Parent.Children.Any(p => p.Rejected))
                    {
                        item.Parent.Rejected = false;
                        item.Parent.ChangeType |= ChangeType.UnReject;
                        if (!item.BatchItem.Children.Any(p => p.Rejected))
                        {
                            item.BatchItem.Rejected = false;
                            item.BatchItem.ChangeType |= ChangeType.UnReject;
                        }
                    }
                }
            }

            if (item.Children != null)
            {
                foreach (var subItem in item.Children)
                {
                    subItem.Rejected = isReject;
                    item.ChangeType |= changeStatus;
                    if (subItem.Children != null)
                    {
                        foreach (var pageItem in subItem.Children)
                        {
                            pageItem.Rejected = isReject;
                            pageItem.ChangeType |= changeStatus;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
