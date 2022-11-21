using Ecm.AppHelper;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Helper;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Resources;
using System.Reflection;
using Ecm.ContentViewer.View;

namespace Ecm.ContentViewer.ViewModel
{
    public class MainViewerViewModel : ComponentViewModel
    {
        private ObservableCollection<ContentItem> _items;
        private ObservableCollection<BatchTypeModel> _batchTypes;
        private ObservableCollection<CommentModel> _comments;
        private ObservableCollection<AmbiguousDefinitionModel> _ambiguousDefinitions;
        private ObservableCollection<FieldValueModel> _fieldValues;
        private bool _enabledOcrClient;
        private bool _enabledBarcodeClient;
        private string _appName;
        private BatchTypeModel _selectedBatchType;
        private bool _isReopenDialog;
        private bool _isAutoCreateBatch;
        private bool _isShowScannerDialogMenu;

        private RelayCommand _showThumbnialCommand;
        private RoutedCommand _showBatchIndexCommand;
        private RelayCommand _showIndexCommand;
        private RelayCommand _showCommentCommand;
        private RelayCommand _newDocumentFromSelectedCommand;
        private RelayCommand _newDocumentStartingHereCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _changeContentTypeCommand;
        private RelayCommand _changeBatchTypeCommand;
        private RelayCommand _combineDocumentCommand;
        private RelayCommand _rotateRightCommand;
        private RelayCommand _rotateLeftCommand;
        private RelayCommand _indexCommand;
        private RelayCommand _batchIndexCommand;
        private RelayCommand _indexNextDocumentCommand;
        private RelayCommand _indexPreviousDocumentCommand;
        private RelayCommand _replaceByFileSystemCommand;
        private RelayCommand _replaceByScannerCommand;
        private RelayCommand _replaceByCameraCommand;
        private RelayCommand _insertBeforeByFileSystemCommand;
        private RelayCommand _insertBeforeByScannerCommand;
        private RelayCommand _insertBeforeByCameraCommand;
        private RelayCommand _insertAfterByFileSystemCommand;
        private RelayCommand _insertAfterByScannerCommand;
        private RelayCommand _insertAfterByCameraCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _createNewBatchCommand;
        private RelayCommand _rejectCommand;
        private RelayCommand _approveCommand;
        private RelayCommand _unRejectCommand;
        private RelayCommand _sendLinkCommand;
        private RelayCommand _submitBatchCommand;
        private RelayCommand _commentCommand;
        private RelayCommand _setLanguageCommand;
        private RelayCommand _changeNameCommand;

        private RelayCommand _scanCommand;
        private RelayCommand _importCommand;
        private RelayCommand _cameraCommand;
        private RelayCommand _chooseDefaultScannerMenuCommand;
        private RelayCommand _chooseDefaultCameraMenuCommand;
        private RelayCommand _chooseDefaultMicrophoneMenuCommand;

        public MainViewerViewModel()
        {
        }

        private List<ContentItem> IndexedItems { get; private set; }

        public ObservableCollection<ContentItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes
        {
            get { return _batchTypes; }
            set
            {
                _batchTypes = value;
                OnPropertyChanged("BatchTypes");
            }
        }

        public ObservableCollection<CommentModel> Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                OnPropertyChanged("Comments");
            }
        }

        public ObservableCollection<AmbiguousDefinitionModel> AmbiguousDefinitions
        {
            get { return _ambiguousDefinitions; }
            set
            {
                _ambiguousDefinitions = value;
                OnPropertyChanged("AmbiguousDefinitions");
            }
        }

        public ObservableCollection<FieldValueModel> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                _fieldValues = value;
                OnPropertyChanged("FieldValues");
            }
        }

        public BatchTypeModel SelectedBatchType
        {
            get { return _selectedBatchType; }
            set
            {
                _selectedBatchType = value;
                OnPropertyChanged("SelectedBatchType");
            }
        }

        public bool IsReopenDialog
        {
            get { return _isReopenDialog; }
            set
            {
                _isReopenDialog = value;
                OnPropertyChanged("IsReopenDialog");
            }
        }

        public bool IsAutoCreateBatch
        {
            get { return _isAutoCreateBatch; }
            set
            {
                _isAutoCreateBatch = value;
                OnPropertyChanged("IsAutoCreateBatch");
            }
        }

        public bool IsShowScannerDialogMenu
        {
            get { return _isShowScannerDialogMenu; }
            set
            {
                _isShowScannerDialogMenu = value;
                OnPropertyChanged("IsShowScannerDialogMenu");
            }
        }

        public ViewerMode ViewerMode { get; set; }

        public bool EnabledOcrClient
        {
            get { return _enabledOcrClient; }
            set
            {
                _enabledOcrClient = value;
                OnPropertyChanged("EnabledOcrClient");
            }
        }

        public bool EnabledBarcodeClient
        {
            get { return _enabledBarcodeClient; }
            set
            {
                _enabledBarcodeClient = value;
                OnPropertyChanged("EnabledBarcodeClient");
            }
        }

        public string AppName
        {
            get { return _appName; }
            set
            {
                _appName = value;
                OnPropertyChanged("AppName");
            }
        }

        public string UserName { get; set; }

        public bool IsChanged { get; set; }

        internal PermissionManager PermissionManager { get; set; }

        internal ThumbnailSelector ThumbnailSelector { get; set; }

        internal ContentItem OpeningContainerItem { get; set; }

        public Func<FieldModel, string, DataTable> GetLookupData;

        public BaseDependencyProperty ContentViewModel { get; set; }

        public BaseDependencyProperty DataViewModel { get; set; }

        public WorkingFolder WorkingFolder { get; set; }

        internal ContentItemManager ContentItemManager { get; private set; }

        public ScanningManager ScanningManager { get; set; }

        public FileSystemImportManager ImportManager { get; set; }

        public OCRHelper OCRHelper { get; set; }

        public ICommand ShowThumbnailCommand
        {
            get
            {
                if (_showThumbnialCommand == null)
                {
                    _showThumbnialCommand = new RelayCommand(p => ShowThumbnail());
                }
                return _showThumbnialCommand;
            }
        }

        private void ShowThumbnail(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public ICommand ShowBatchIndexCommand
        {
            get
            {
                if (_showBatchIndexCommand== null)
                {
                    _showBatchIndexCommand = new RelayCommand(p => ShowBatchIndex(), p=> CanShowBatchIndex());
                }
                return _showBatchIndexCommand;
            }
        }

        public ICommand ShowIndexCommand
        {
            get
            {
                if (_showIndexCommand== null)
                {
                    _showIndexCommand = new RelayCommand(p => ShowIndex(), p => CanShowIndex());
                }
                return _showIndexCommand;
            }
        }

        public ICommand ShowCommentCommand
        {
            get
            {
                if (_showCommentCommand== null)
                {
                    _showCommentCommand = new RelayCommand(p => ShowComment());
                }
                return _showCommentCommand;
            }
        }

        public ICommand NewDocumentFromSelectedCommand
        {
            get
            {
                if (_newDocumentFromSelectedCommand == null)
                {
                    _newDocumentFromSelectedCommand = new RelayCommand(p => NewDocumentFromSelected(), p => CanNewDocumentFromSelected());
                }

                return _newDocumentFromSelectedCommand;
            }
        }

        public ICommand NewDocumentStartingHereCommand;
        public ICommand DeleteCommand;
        public ICommand ChangeContentTypeCommand;
        public ICommand ChangeBatchTypeCommand;
        public ICommand CombineDocumentCommand;
        public ICommand RotateRightCommand;
        public ICommand RotateLeftCommand;
        public ICommand IndexCommand;
        public ICommand BatchIndexCommand;
        public ICommand IndexNextDocumentCommand;
        public ICommand IndexPreviousDocumentCommand;
        public ICommand ReplaceByFileSystemCommand;
        public ICommand ReplaceByScannerCommand;
        public ICommand ReplaceByCameraCommand;
        public ICommand InsertBeforeByFileSystemCommand;
        public ICommand InsertBeforeByScannerCommand;
        public ICommand InsertBeforeByCameraCommand;
        public ICommand InsertAfterByFileSystemCommand;
        public ICommand InsertAfterByScannerCommand;
        public ICommand InsertAfterByCameraCommand;
        public ICommand SaveCommand;
        public ICommand CreateNewBatchCommand;
        public ICommand RejectCommand;
        public ICommand ApproveCommand;
        public ICommand UnRejectCommand;
        public ICommand SendLinkCommand;
        public ICommand SubmitBatchCommand;
        public ICommand CommentCommand;
        public ICommand SetLanguageCommand;
        public ICommand ChangeNameCommand;

        //Private methods
        private void ShowComment()
        {
            ContentViewModel = new CommentViewModel();
        }

        private void ShowIndex()
        {
            IndexViewerViewModel indexViewModel = new IndexViewerViewModel(this);
            DataViewModel = indexViewModel;
            indexViewModel.PopulateIndexPanel(ThumbnailSelector.Cursor);
        }

        private bool CanShowIndex()
        {
            if (ThumbnailSelector != null && ThumbnailSelector.Cursor != null && ThumbnailSelector.Cursor.ItemType == ContentItemType.Page && ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch)
            {
                return false;
            }
            else
            {
                return (ThumbnailSelector != null && ThumbnailSelector.Cursor != null &&
                    (ThumbnailSelector.Cursor.ItemType == ContentItemType.ContentModel || ThumbnailSelector.Cursor.ItemType == ContentItemType.Page ||
                    (ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch && ThumbnailSelector.Cursor.Children.Count > 0 && ThumbnailSelector.Cursor.DocumentData != null)));
            }
        }

        private bool CanShowBatchIndex()
        {
            return (ThumbnailSelector != null && ThumbnailSelector.Cursor != null);
        }

        private void ShowBatchIndex()
        {
            IndexViewerViewModel indexViewModel = new IndexViewerViewModel(this);
            DataViewModel = indexViewModel;
            indexViewModel.PopulateBatchIndexPanel(ThumbnailSelector.Cursor);
        }

        private void ShowThumbnail()
        {
            ThumbnailViewerViewModel thumbnailViewModel = new ThumbnailViewerViewModel(this);
            thumbnailViewModel.Items = Items;
            DataViewModel = thumbnailViewModel;
        }

        public void ShowHideScannerDialog()
        {
            IsShowScannerDialogMenu = !IsShowScannerDialogMenu;
        }

#region Command method
        private void ShowIndexPanel(List<FieldValueModel> fieldValues, int indexedItemCount, List<ContentFieldPermissionModel> fieldPermissions)
        {
            if (indexedItemCount > 0)
            {
                this.FieldValues = new ObservableCollection<FieldValueModel>();
                foreach (FieldValueModel fieldValue in fieldValues)
                {
                    if (fieldValue.Field.IsSystemField)
                    {
                        continue;
                    }
                    
                    ContentFieldPermissionModel fieldPermission = null;
                    if (fieldPermissions != null)
                    {
                        fieldPermission = fieldPermissions.SingleOrDefault(p => p.FieldId == fieldValue.Field.Id);
                    }
                    else
                    {
                        fieldPermission = new ContentFieldPermissionModel
                        {
                            CanRead = true,
                            CanWrite = true,
                            Hidden = false,
                            FieldId = fieldValue.Field.Id
                        };
                    }

                    var clonedFieldValue = new FieldValueModel
                    {
                        Field = fieldValue.Field,
                        ShowMultipleUpdate = indexedItemCount > 1,
                        Value = fieldValue.Value,
                        SnippetImage = fieldValue.SnippetImage,
                        IsHidden = fieldPermission == null ? true : (fieldPermission.Hidden && fieldValue.Field.IsRestricted),
                        IsReadOnly = fieldPermission == null ? false : fieldPermission.CanRead && !fieldPermission.CanWrite && !fieldValue.Field.IsRequired,
                        IsWrite = fieldPermission == null ? false : fieldPermission.CanWrite,
                        TableValues = fieldValue.TableValues,
                        MultipleUpdate = fieldValue.MultipleUpdate
                    };

                    this.FieldValues.Add(clonedFieldValue);

                    // Register event to listen the value of cloned index and then update back the selected documents/batches
                    clonedFieldValue.PropertyChanged += FieldValuePropertyChanged;
                }
                IndexViewerViewModel indexViewModel = new IndexViewerViewModel(this);
                DataViewModel = indexViewModel;

                indexViewModel.CanUpdateIndexValue = this.PermissionManager.CanModifyIndex();
            }
            else
            {
                this.FieldValues.Clear();
                IndexedItems.Clear();
            }

            this.ShowIndexViewer(true);
        }

        private void CreateNewBatch()
        {
            try
            {
                DialogViewer viewer = new DialogViewer();
                ResourceManager resource = new ResourceManager("Ecm.ContentViewer.BatchTypeSelection", Assembly.GetExecutingAssembly());

                viewer.Text = resource.GetString("Title");

                BatchTypeSelection batchTypeSelection = new BatchTypeSelection
                {
                    Dialog = viewer,
                    SelectedBatchType = SelectedBatchType,
                    IsReopenDialog = this.IsReopenDialog,
                    IsAutoCreateBatch = this.IsAutoCreateBatch,
                    BatchTypes = this.BatchTypes
                };

                viewer.WpfContent = batchTypeSelection;

                if (this.SelectedBatchType != null)
                {
                    this.SelectedBatchType.IsSelected = true;
                }

                if (viewer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.SelectedBatchType = batchTypeSelection.SelectedBatchType;
                    this.IsAutoCreateBatch = batchTypeSelection.IsAutoCreateBatch;
                    this.IsReopenDialog = batchTypeSelection.IsReopenDialog;
                    this.ContentItemManager.CreateBatch(batchTypeSelection.SelectedBatchType);
                    this.ThumbnailSelector.LeftMouseClick(this.Items.Last());

                    ThumbnailViewerViewModel thumbnialViewModel = new ThumbnailViewerViewModel(this);
                    DataViewModel = thumbnialViewModel;
                    thumbnialViewModel.ShowContextMenu(this.Items.Last());
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private bool CanCreateNewBatch()
        {
            return this.BatchTypes != null && this.BatchTypes.Count > 0 && this.ViewerMode != ViewerMode.WorkItem;
        }

        private bool CanNewDocumentFromSelected()
        {
            return this.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page) &&
                           this.PermissionManager.CanCreateDocument() &&
                           (!this.ThumbnailSelector.SelectedItems.Any(p => p.ItemType == ContentItemType.Page &&
                                                                    p.Parent.ItemType == ContentItemType.ContentModel) ||
                            this.PermissionManager.CanSplitDocument());
        }

        private void NewDocumentFromSelected()
        {
            try
            {
                var documentType = e.Parameter as ContentTypeModel;
                if (documentType != null)
                {
                    var pageItems = this.ThumbnailSelector.SelectedItems.Where(p => p.ItemType == ContentItemType.Page).ToList();
                    CreateDocument(pageItems, documentType);
                    this.DisplayItem();
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanNewDocumentStartingHere(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanCreateDocument() &&
                           this.ThumbnailSelector.Cursor != null &&
                           this.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page &&
                           (this.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch ||
                            (this.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.ContentModel &&
                             this.ThumbnailSelector.Cursor.Parent.Children.IndexOf(this.ThumbnailSelector.Cursor) > 0 &&
                             this.PermissionManager.CanSplitDocument()));

        }

        private void NewDocumentStartingHere(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as ContentTypeModel;
                if (documentType != null)
                {
                    // Get all pages from the position of cursor in a container (batch or document)
                    var cursorIndex = this.ThumbnailSelector.Cursor.Parent.Children.IndexOf(this.ThumbnailSelector.Cursor);
                    var movedPages = this.ThumbnailSelector.Cursor.Parent.Children.Skip(cursorIndex).Where(p => p.ItemType == ContentItemType.Page).ToList();
                    CreateDocument(movedPages, documentType);
                    this.DisplayItem();
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanDelete() && this.DocViewerMode != DocViewerMode.OCRTemplate;
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Delete the selected item and move cursor to other item and then set its selection
                if (DialogService.ShowTwoStateConfirmDialog("Are you sure you want to delete the opening document?") == DialogServiceResult.Yes)
                {
                    var tmpItems = new SingleItemList<ContentItem>(this.ThumbnailSelector.SelectedItems);
                    var cleanUpItems = new List<ContentItem>(this.ThumbnailSelector.SelectedItems);
                    foreach (var item in tmpItems)
                    {
                        if (item.ItemType == ContentItemType.Batch)
                        {
                            if (this.DocViewerMode == DocViewerMode.Capture)
                            {
                                this.ThumbnailSelector.MoveSelection();
                                this.Items.Remove(item);
                                this.ThumbnailSelector.RemoveContentItem(item);
                                this.DocThumbnail.Focus(this.ThumbnailSelector.Cursor);
                            }
                            else
                            {
                                cleanUpItems.Remove(item);
                                cleanUpItems.AddRange(item.Children);
                                item.Children.Clear();
                            }

                            this.DeleteBatch(item);
                        }
                        else if (item.Parent.ItemType == ContentItemType.ContentModel && item.Parent.Children.Count == 1)
                        {
                            item.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                            item.BatchItem.DeletedDocuments.Add(item.Parent);
                            if (this.DocViewerMode == DocViewerMode.ContentModel)
                            {
                                this.DeleteDocument(item.Parent);
                            }

                            this.ThumbnailSelector.MoveSelection();
                            item.BatchItem.Children.Remove(item.Parent);
                            this.ThumbnailSelector.RemoveContentItem(item.Parent);
                        }
                        else
                        {
                            if (item.ItemType == ContentItemType.ContentModel)
                            {
                                item.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                                item.BatchItem.DeletedDocuments.Add(item);
                                if (this.DocViewerMode == DocViewerMode.ContentModel)
                                {
                                    this.DeleteDocument(item.Parent);
                                }
                            }
                            else if (item.ItemType == ContentItemType.Page)
                            {
                                item.Parent.ChangeType |= ChangeType.DeletePage;
                                item.Parent.DeletedPages.Add(item);
                            }

                            this.ThumbnailSelector.MoveSelection();
                            item.Parent.Children.Remove(item);
                            this.ThumbnailSelector.RemoveContentItem(item);
                        }
                    }

                    this.DocThumbnail.Focus(this.ThumbnailSelector.Cursor);

                    // Clean up items
                    foreach (var item in cleanUpItems)
                    {
                        if (item.ItemType == ContentItemType.Batch)
                        {
                            if (this.DocViewerMode == DocViewerMode.Capture)
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

                    this.CollectGarbage();
                }
                
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanChangeContentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanChangeDocumentType() &&
                           this.ThumbnailSelector.SelectedItems.Count > 0 &&
                           !this.ThumbnailSelector.SelectedItems.Any(p => p.ItemType != ContentItemType.ContentModel);
        }

        private void ChangeContentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var documentType = e.Parameter as ContentTypeModel;
                if (documentType != null)
                {
                    var document = new ContentModel(DateTime.Now, this.UserName, documentType);
                    foreach (var docItem in this.ThumbnailSelector.SelectedItems)
                    {
                        docItem.SetDocumentData(document);
                        docItem.ChangeType |= ChangeType.ChangeDocumentType;
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanChangeBatchType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanChangeBatchType();
        }

        private void ChangeBatchType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (this.ThumbnailSelector.Cursor.BatchItem.Children.Any(p => p.ItemType == ContentItemType.ContentModel))
                {
                    if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiChangeBatchTypeConfirmation")) != DialogServiceResult.Yes)
                    {
                        return;
                    }
                }

                var dialog = new DialogViewer { Text = new ResourceManager("Ecm.ContentViewer.BatchTypeSelection", Assembly.GetExecutingAssembly()).GetString("Title") };
                var batchTypeSelection = new BatchTypeSelection
                {
                    Dialog = dialog,
                    BatchTypes = new ObservableCollection<BatchTypeModel>(this.BatchTypes.Where(p => p != this.ThumbnailSelector.Cursor.BatchData.BatchType))
                };

                if (this.SelectedBatchType!= null)
                {
                    this.SelectedBatchType.IsSelected = true;
                }

                dialog.WpfContent = batchTypeSelection;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.ThumbnailSelector.Cursor.SetBatchData(new BatchModel(Guid.Empty, DateTime.Now,this.UserName, batchTypeSelection.SelectedBatchType));
                    this.SelectedBatchType = batchTypeSelection.SelectedBatchType;
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanCombineDocument(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.PermissionManager.CanCombineDocument())
            {
                var docItem = this.ThumbnailSelector.SelectedItems.FirstOrDefault(p => p.ItemType == ContentItemType.ContentModel);
                e.CanExecute = docItem != null &&
                               (this.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.ContentModel && p != docItem) ||
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
                var docItem = this.ThumbnailSelector.SelectedItems.First(p => p.ItemType == ContentItemType.ContentModel);
                var movedItems = this.ThumbnailSelector.SelectedItems.Where(p => p.ItemType == ContentItemType.Page && p.Parent != docItem).ToList();
                var movedDocs = this.ThumbnailSelector.SelectedItems.Where(p => p.ItemType == ContentItemType.ContentModel && p != docItem).ToList();
                foreach (var movedDoc in movedDocs)
                {
                    movedItems.AddRange(movedDoc.Children);
                }

                movedItems = this.ContentItemManager.SortPages(movedItems).ToList();
                foreach (var item in movedItems)
                {
                    if (item.Parent.ItemType == ContentItemType.ContentModel && item.Parent.Children.Count == 1)
                    {
                        item.BatchItem.Children.Remove(item.Parent);
                        this.ThumbnailSelector.RemoveContentItem(item.Parent);
                        if (this.DocViewerMode == DocViewerMode.ContentModel)
                        {
                            this.DeleteDocument(item.Parent);
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
                this.HandleException(ex);
            }
        }

        private void CanRotate(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanRotate() &&
                           this.ThumbnailSelector.SelectedItems.Any(p => (p.ItemType == ContentItemType.Page && p.PageData.FileType == FileTypeModel.Image) ||
                                                                                    (p.ItemType == ContentItemType.ContentModel && p.Children.Any(q => q.PageData.FileType == FileTypeModel.Image)));
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
                foreach (var item in this.ThumbnailSelector.SelectedItems)
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
                        case ContentItemType.ContentModel:
                            item.Children.ToList().ForEach(q =>
                            {
                                // Ignore the item if it is already in the SelectedItems collection, if not, it will be rotated twice
                                if (!this.ThumbnailSelector.SelectedItems.Any(i => i == q))
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

                this.ArrangeImageViewerLayout();
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }
        
        private void CanIndex(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.ThumbnailSelector != null && this.ThumbnailSelector.Cursor != null && this.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page && this.ThumbnailSelector.Cursor.Parent.ItemType == ContentItemType.Batch)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = (this.ThumbnailSelector != null && this.ThumbnailSelector.Cursor != null &&
                    (this.ThumbnailSelector.Cursor.ItemType == ContentItemType.ContentModel || this.ThumbnailSelector.Cursor.ItemType == ContentItemType.Page || 
                    (this.ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch && this.ThumbnailSelector.Cursor.Children.Count > 0 && this.ThumbnailSelector.Cursor.DocumentData != null)));
            }
        }

        private void CanBatchIndex(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.ThumbnailSelector != null && this.ThumbnailSelector.Cursor != null);// && this.ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch);
        }

        private void Index(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                PopulateIndexPanel(this.ThumbnailSelector.Cursor);
                this.ButtonNextPage.Visibility = System.Windows.Visibility.Visible;
                this.ButtonPreviousPage.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void BatchIndex(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                PopulateBatchIndexPanel(this.ThumbnailSelector.Cursor);
                this.ButtonNextPage.Visibility = System.Windows.Visibility.Hidden;
                this.ButtonPreviousPage.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
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
                this.ThumbnailSelector.LeftMouseClick(allDocs[maxIndex + 1]);
                PopulateIndexPanel(this.ThumbnailSelector.Cursor);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
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
                this.ThumbnailSelector.LeftMouseClick(allDocs[minIndex - 1]);
                PopulateIndexPanel(this.ThumbnailSelector.Cursor);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanReplace(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanReplace() &&
                           this.ThumbnailSelector.SelectedItems.Count == 1 &&
                           this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page;
        }

        private void ReplaceByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.ScanManager.ReplaceContent(this.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void ReplaceByFileSystem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.ImportManager.ReplaceContent(this.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanReplaceByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanReplace() &&
                           this.ThumbnailSelector.SelectedItems.Count == 1 &&
                           this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page &&
                           this.CameraManager.HasVideoInputDevice;
        }

        private void ReplaceByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.CameraManager.Replace(this.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanInsertBefore(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanInsert() &&
                           this.ThumbnailSelector.SelectedItems.Count == 1 &&
                           this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page;
        }

        private void InsertBeforeByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.ScanManager.InsertContentBefore(this.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void InsertBeforeByFileSystem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.ImportManager.InsertContentBefore(this.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanInsertBeforeByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ThumbnailSelector.SelectedItems.Count == 1 &&
                           this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page &&
                           this.CameraManager.HasVideoInputDevice;
        }

        private void InsertBeforeByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.CameraManager.InsertBefore(this.ThumbnailSelector.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanInsertAfter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanInsert() &&
                           this.ThumbnailSelector.SelectedItems.Count == 1 &&
                           (this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page ||
                            this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.ContentModel);
        }

        private void InsertAfterByScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.ContentModel)
                {
                    this.ScanManager.InsertContentAfter(this.ThumbnailSelector.SelectedItems[0].Children.Last());
                }
                else
                {
                    this.ScanManager.InsertContentAfter(this.ThumbnailSelector.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void InsertAfterByFileSystem(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.ContentModel)
                {
                    this.ImportManager.InsertContentAfter(this.ThumbnailSelector.SelectedItems[0].Children.Last());
                }
                else
                {
                    this.ImportManager.InsertContentAfter(this.ThumbnailSelector.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanInsertAfterByCamera(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanInsert() &&
                           this.ThumbnailSelector.SelectedItems.Count == 1 &&
                           (this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.Page ||
                            this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.ContentModel) &&
                           this.CameraManager.HasVideoInputDevice;
        }

        private void InsertAfterByCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (this.ThumbnailSelector.SelectedItems[0].ItemType == ContentItemType.ContentModel)
                {
                    this.CameraManager.InsertAfter(this.ThumbnailSelector.SelectedItems[0].Children.Last());
                }
                else
                {
                    this.CameraManager.InsertAfter(this.ThumbnailSelector.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            bool valid = true;

            foreach (ContentItem item in this.Items)
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
                    catch(Exception ex)
                    {
                    }
                }
            }

            if (this.DocViewerMode == DocViewerMode.WorkItem)
            {
                // This case for work item
                e.CanExecute = (this.IsChanged || this.DocViewerMode == DocViewerMode.WorkItem) &&
                               !this.IsProcessing &&
                               this.Items != null &&
                               this.Items.Count != 0 && valid &&
                               !this.Items.Any(p => p.BatchData.IsCompleted ||
                                                                 p.Children.Count == 0 ||
                                                                 (p.Children.Any(q => q.ItemType == ContentItemType.Page) && !p.BatchData.Permission.CanReleaseLoosePage) ||
                                                                 p.Children.Any(q => q.ItemType == ContentItemType.ContentModel && !q.IsValid));
            }
            else
            {
                e.CanExecute = this.Items != null &&
                               this.Items.Count > 0 &&
                               this.Items.Any(p => p.IsChanged) && valid &&
                               (this.PermissionManager.CanReleaseWithLoosePage() ||
                                !this.Items.Any(p => p.Children.Any(q => q.ItemType == ContentItemType.Page)));
            }
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.SaveAllBatches();
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanSubmitBatch(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DocViewerMode == DocViewerMode.WorkItem)
            {
                // This case for work item
                e.CanExecute = (this.IsChanged || this.DocViewerMode == DocViewerMode.WorkItem) &&
                               !this.IsProcessing &&
                               this.Items != null &&
                               this.Items.Count != 0 &&
                               !this.Items.Any(p => p.BatchData.IsCompleted ||
                                                                 p.Children.Count == 0 || !p.IsValid ||
                                                                 (p.Children.Any(q => q.ItemType == ContentItemType.Page) && !p.BatchData.Permission.CanReleaseLoosePage) ||
                                                                 p.Children.Any(q => q.ItemType == ContentItemType.ContentModel && !q.IsValid));
            }
            else
            {
                e.CanExecute = this.Items != null &&
                               this.Items.Count > 0 &&
                               this.Items.Any(p => p.IsChanged && p.IsValid) &&
                               (this.PermissionManager.CanReleaseWithLoosePage() ||
                                !this.Items.Any(p => p.Children.Any(q => q.ItemType == ContentItemType.Page)));
            }
        }

        private void SubmitBatch(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.SaveBatch(this.ThumbnailSelector.SelectedItems[0].BatchItem);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanApprove(object sender, CanExecuteRoutedEventArgs e)
        {
            bool valid = true;

            foreach (ContentItem item in this.Items)
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

            e.CanExecute = (this.IsChanged || this.DocViewerMode == DocViewerMode.WorkItem) &&
                           !this.IsProcessing &&
                           this.Items != null &&
                           this.Items.Count != 0 && valid &&
                           !this.Items.Any(p => p.BatchData.IsCompleted || p.Rejected ||
                                                             p.Children.Count == 0 ||
                                                             (p.Children.Any(q => q.ItemType == ContentItemType.Page) && !p.BatchData.Permission.CanReleaseLoosePage) ||
                                                             p.Children.Any(q => q.ItemType == ContentItemType.ContentModel && !q.IsValid));
        }

        private void Approve(object sender, ExecutedRoutedEventArgs e)
        {
            this.ApproveBatch();
        }

        private void CanReject(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PermissionManager.CanReject() &&
                           this.ThumbnailSelector.SelectedItems.Count > 0 &&
                           this.ThumbnailSelector.SelectedItems.Any(p => !p.Rejected ||
                                                                             (p.ItemType == ContentItemType.ContentModel && p.Children.Any(q => !q.Rejected)) ||
                                                                             (p.ItemType == ContentItemType.Batch && p.Children.Any(q => !q.Rejected || (q.ItemType == ContentItemType.ContentModel && q.Children.Any(r => !r.Rejected)))));
        }

        private void Reject(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                foreach (var item in this.ThumbnailSelector.SelectedItems)
                {
                    item.Rejected = true;
                    item.ChangeType |= ChangeType.Reject;

                    RejectAndUnreject(item, true);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }

        }

        private void CanUnReject(object sender, CanExecuteRoutedEventArgs e)
        {
            bool hasReject = this.ThumbnailSelector.SelectedItems.Any(p => p.Rejected ||
                                                                               (p.ItemType == ContentItemType.ContentModel && p.Children.Any(q => q.Rejected)) ||
                                                                               (p.ItemType == ContentItemType.Batch && p.Children.Any(q => q.Rejected ||
                                                                                                                                     (q.ItemType == ContentItemType.ContentModel && q.Children.Any(r => r.Rejected)))));
            e.CanExecute = hasReject;
        }

        private void UnReject(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                foreach (var item in this.ThumbnailSelector.SelectedItems)
                {
                    item.Rejected = false;
                    item.ChangeType |= ChangeType.UnReject;

                    RejectAndUnreject(item, false);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanSendLink(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ThumbnailSelector.SelectedItems.Count > 0 &&
                           this.ThumbnailSelector.SelectedItems[0].BatchItem.Children.Count > 0 &&
                           this.PermissionManager.CanSendLink();
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

                var encodedUri = new Uri("http://localhost/index.html?" + string.Format(queryTemplate, this.UserName, this.ThumbnailSelector.Cursor.BatchData.Id.ToString()));
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
                this.HandleException(ex);
            }
        }

        private void CanShowComment(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ThumbnailSelector.SelectedItems.Count > 0 &&
                           this.ThumbnailSelector.SelectedItems[0].BatchItem.Children.Count > 0;
        }

        private void DisplayComment(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                this.ShowCommentViewer(true);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void CanSetContentLanguage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SetContentLanguage(object sender, ExecutedRoutedEventArgs e)
        {
            var languageCode = e.Parameter as string;

            if (this.ThumbnailSelector.SelectedItems.Count > 0)
            {
                foreach (ContentItem item in this.ThumbnailSelector.SelectedItems)
                {
                    if (item.ItemType == ContentItemType.ContentModel)
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

        private void CreateDocument(IEnumerable<ContentItem> pageItems, ContentTypeModel documentType)
        {
            var document = new ContentModel(DateTime.Now, ViewerContainer.UserName, documentType);
            var docItem = new ContentItem(document);
            var batchItem = ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem;
            batchItem.Children.Add(docItem);
            pageItems = ViewerContainer.ContentItemManager.SortPages(pageItems.ToList());
            foreach (var pageItem in pageItems)
            {
                if (pageItem.Parent.ItemType == ContentItemType.ContentModel && pageItem.Parent.Children.Count == 1)
                {
                    pageItem.Parent.Parent.Children.Remove(pageItem.Parent);
                    ViewerContainer.ThumbnailSelector.RemoveContentItem(pageItem.Parent);
                }

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
                        FieldValueModel fieldValue = p.ItemType == ContentItemType.ContentModel ? p.DocumentData.FieldValues.FirstOrDefault(q => q.Field.Name == clonedFieldValue.Field.Name) :
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
                        where subItem.ItemType == ContentItemType.ContentModel
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
        //Public methods
        public void HandleException(Exception ex)
        {
        }
    }
}
