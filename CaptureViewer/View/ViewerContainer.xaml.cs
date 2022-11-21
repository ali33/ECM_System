using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Ecm.AppHelper;
using Ecm.CustomControl;
using Ecm.CaptureViewer.Helper;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using System.Timers;
using Ecm.CaptureViewer.Converter;
using System.Resources;
using System.Reflection;
using Ecm.Mvvm;

namespace Ecm.CaptureViewer
{
    public partial class ViewerContainer:UserControl
    {
        #region Private members
        private ResourceManager _resource = new ResourceManager("Ecm.CaptureViewer.ViewerContainer", Assembly.GetExecutingAssembly());
        private Timer _cleanupTimer;
        private bool _isLoaded;
        private MenuItem scanPageConfigurationMenu;
        private MenuItem selectedMenuItem;
        public bool _isNativeMode;
        #endregion

        #region Dependency properties

        public static readonly DependencyProperty ItemsProperty =
           DependencyProperty.Register("Items", typeof(ObservableCollection<ContentItem>), typeof(ViewerContainer),
               new FrameworkPropertyMetadata(null, ItemsChangedCallback));

        private static void ItemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ViewerContainer;
            if (viewer != null)
            {
                viewer.InitializeDefaultSelection();
                viewer.RefreshItems();
            }
        }

        public static readonly DependencyProperty OpeningItemsProperty =
           DependencyProperty.Register("OpeningItems", typeof(ObservableCollection<ContentItem>), typeof(ViewerContainer));

        public static readonly DependencyProperty BatchTypesProperty =
           DependencyProperty.Register("BatchTypes", typeof(ObservableCollection<BatchTypeModel>), typeof(ViewerContainer),
               new FrameworkPropertyMetadata(null, BatchTypesChangedCallback));

        public static readonly DependencyProperty SelectedBatchTypeProperty =
           DependencyProperty.Register("SelectedBatchType", typeof(BatchTypeModel), typeof(ViewerContainer));

        private static void BatchTypesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ViewerContainer;
            if (viewer != null)
            {
                viewer.LoadOCRTemplate();
            }
        }

        public static readonly DependencyProperty FieldValuesProperty =
           DependencyProperty.Register("FieldValues", typeof(ObservableCollection<FieldValueModel>), typeof(ViewerContainer),
               new FrameworkPropertyMetadata(new ObservableCollection<FieldValueModel>()));

        public static readonly DependencyProperty AppNameProperty =
           DependencyProperty.Register("AppName", typeof(string), typeof(ViewerContainer));

        public static readonly DependencyProperty UserNameProperty =
           DependencyProperty.Register("UserName", typeof(string), typeof(ViewerContainer));

        public static readonly DependencyProperty IsProcessingProperty =
           DependencyProperty.Register("IsProcessing", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty ReadOnlyProperty =
           DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty IsChangedProperty =
           DependencyProperty.Register("IsChanged", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty DocViewerModeProperty =
           DependencyProperty.Register("DocViewerMode", typeof(DocViewerMode), typeof(ViewerContainer));

        public static readonly DependencyProperty OpenMediaFilePathProperty =
           DependencyProperty.Register("OpenMediaFilePath", typeof(string), typeof(ViewerContainer));

        public static readonly DependencyProperty OpenNativeFilePathProperty =
           DependencyProperty.Register("OpenNativeFilePath", typeof(string), typeof(ViewerContainer));

        public static readonly DependencyProperty OCRTemplateFilePathProperty =
           DependencyProperty.Register("OCRTemplateFilePath", typeof(string[]), typeof(ViewerContainer));

        public static readonly DependencyProperty AmbiguousDefinitionsProperty =
           DependencyProperty.Register("AmbiguousDefinitions", typeof(ObservableCollection<AmbiguousDefinitionModel>), typeof(ViewerContainer));

        public static readonly DependencyProperty IsAutoCreateBatchProperty =
            DependencyProperty.Register("IsAutoCreateBatch", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty IsReopenDialogProperty =
            DependencyProperty.Register("IsReopenDialog", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty CommentsProperty =
            DependencyProperty.Register("Comments", typeof(ObservableCollection<CommentModel>), typeof(ViewerContainer),
            new FrameworkPropertyMetadata(new ObservableCollection<CommentModel>()));

        public static readonly DependencyProperty EnabledOcrClientProperty =
            DependencyProperty.Register("EnabledOcrClient", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty EnabledBarcodeClientProperty =
            DependencyProperty.Register("EnabledBarcodeClient", typeof(bool), typeof(ViewerContainer));

        public static readonly DependencyProperty OpenTextFilePathProperty =
           DependencyProperty.Register("OpenTextFilePath", typeof(string), typeof(ViewerContainer));

        #endregion

        public ViewerContainer()
        {
            InitializeComponent();
            Loaded += ViewerContainerLoaded;
        }

        public void DisplayItem()
        {
            var pageItem = ThumbnailSelector.Cursor;

            if (ThumbnailSelector.Cursor == null || ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch)
            {
                if (DocViewerMode != DocViewerMode.Document)
                {
                    PanelInfo.Visibility = Visibility.Visible;
                    PanelImageViewer.Visibility = Visibility.Collapsed;
                    PanelNativeViewer.Visibility = Visibility.Collapsed;
                    PanelMediaPlayer.Visibility = Visibility.Collapsed;
                    PanelToolbar.Visibility = Visibility.Collapsed;
                    PanelComment.Visibility = Visibility.Collapsed;
                    OpenMediaFilePath = null;
                    OpenNativeFilePath = null;
                    OpeningItem = ThumbnailSelector.Cursor;
                    OpeningContainerItem = null;

                    return;
                }

                if (Items.Count > 0 && Items[0].Children.Count > 0)
                {
                    pageItem = Items[0].Children[0].Children[0];
                }
            }

            if (ThumbnailSelector.Cursor != null && ThumbnailSelector.Cursor.ItemType == ContentItemType.Document)
            {
                if (ThumbnailSelector.Cursor.Children.Count > 0)
                {
                    pageItem = ThumbnailSelector.Cursor.Children[0];
                }
            }

            if (pageItem == OpeningItem && OpeningItem != null && OpeningItem.PageData.FileType == FileTypeModel.Image)
            {
                if (PanelComment.Visibility == Visibility.Collapsed)
                {
                    ShowHideToolbarButton(pageItem);
                    return;
                }
            }

            PanelInfo.Visibility = Visibility.Collapsed;
            PanelImageViewer.Visibility = Visibility.Collapsed;
            PanelNativeViewer.Visibility = Visibility.Collapsed;
            PanelMediaPlayer.Visibility = Visibility.Collapsed;
            PanelToolbar.Visibility = Visibility.Collapsed;
            PanelComment.Visibility = Visibility.Collapsed;
            PanelTextViewer.Visibility = System.Windows.Visibility.Collapsed;
            OpenMediaFilePath = null;
            OpenNativeFilePath = null;
            OpeningContainerItem = null;
            _isNativeMode = false;

            if (pageItem != null && pageItem.ItemType == ContentItemType.Page)
            {
                pageItem.Load(this);
                PanelToolbar.Visibility = Visibility.Visible;
                switch (pageItem.PageData.FileType)
                {
                    case FileTypeModel.Image:
                        PanelImageViewer.Visibility = Visibility.Visible;
                        DisplayImages(pageItem);
                        break;
                    case FileTypeModel.Media:
                        PanelMediaPlayer.Visibility = Visibility.Visible;
                        OpenMediaFilePath = pageItem.FilePath;
                        break;
                    case FileTypeModel.Native:
                        PanelNativeViewer.Visibility = Visibility.Visible;
                        OpenNativeFilePath = pageItem.FilePath;
                        var fileFormat = (FileFormatModel)new FileFormatConverter().Convert(pageItem.PageData.FileExtension, null, null, null);

                        switch (fileFormat)
                        {
                            case FileFormatModel.Xls:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/excel.png"));
                                break;
                            case FileFormatModel.Doc:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/word.png"));
                                break;
                            case FileFormatModel.Html:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/html.png"));
                                break;
                            case FileFormatModel.Pdf:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/pdf.png"));
                                break;
                            case FileFormatModel.Ppt:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/ppt.png"));
                                break;
                            case FileFormatModel.Xml:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/xml.png"));
                                break;
                            case FileFormatModel.Xps:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/xps.png"));
                                break;
                            case FileFormatModel.Unknown:
                                nativeBackGround.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/unknow.png"));
                                break;

                        }
                        _isNativeMode = true;
                        break;
                    case FileTypeModel.Text:
                        PanelTextViewer.Visibility = Visibility.Visible;
                        OpenTextFilePath = pageItem.FilePath;
                        TextViewer.LoadTextDocument(OpenTextFilePath);
                        break;
                }

                OpeningItem = pageItem;
                OpeningContainerItem = pageItem.Parent;
                ShowHideToolbarButton(pageItem);
            }
        }

        public void ShowHideNativeView(bool hidden)
        {
            if (hidden)
            {
                NativeViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                NativeViewer.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void CollectGarbage()
        {
            if (_cleanupTimer == null)
            {
                _cleanupTimer = new Timer { Interval = 1000 };
                _cleanupTimer.Elapsed += CleanupTimerElapsed;
            }

            _cleanupTimer.Start();
        }

        public void ShowIndexViewer(bool show)
        {
            if (show)
            {
                IndexViewer.Visibility = Visibility.Visible;
                DocThumbnail.Visibility = Visibility.Collapsed;
                IndexViewer.UpdateTitle();
            }
            else
            {
                DocThumbnail.Visibility = Visibility.Visible;
                IndexViewer.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowCommentViewer(bool show)
        {
            if (show)
            {
                CommentView.AllowAddComment = ThumbnailSelector.Cursor != null &&
                                                (ThumbnailSelector.Cursor.BatchData != null && !ThumbnailSelector.Cursor.BatchData.IsCompleted) ||
                                                (ThumbnailSelector.Cursor.DocumentData != null && !ThumbnailSelector.Cursor.Parent.BatchData.IsCompleted) ||
                                                (ThumbnailSelector.Cursor.PageData != null && !ThumbnailSelector.Cursor.Parent.Parent.BatchData.IsCompleted);
                PanelInfo.Visibility = Visibility.Collapsed;
                PanelImageViewer.Visibility = Visibility.Collapsed;
                PanelNativeViewer.Visibility = Visibility.Collapsed;
                PanelMediaPlayer.Visibility = Visibility.Collapsed;
                PanelToolbar.Visibility = Visibility.Collapsed;
                PanelComment.Visibility = Visibility.Visible;
                OpenMediaFilePath = null;
                OpenNativeFilePath = null;
                OpeningContainerItem = null;
                CommentView.SortMessages();
            }
        }

        public void ArrangeImageViewerLayout()
        {
            ImageViewer.ArrangeLayout();
        }

        public void Clean()
        {
            if (DocViewerMode == DocViewerMode.Capture || DocViewerMode == Model.DocViewerMode.LightCapture)
            {
                if (Items != null && Items.Count > 0)
                {
                    foreach (var item in Items[0].Children)
                    {
                        item.Clean();
                    }

                    Items[0].Children.Clear();
                    ThumbnailSelector.LeftMouseClick(Items[0]);
                    Items[0].IsValid = false;
                    Items[0].ResetStatus();
                }

                Items = new ObservableCollection<ContentItem>();
                ShowIndexViewer(false);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public void DeleteBatch(ContentItem batchItem)
        {
            if (DeleteBatchItem != null)
            {
                DeleteBatchItem(batchItem);
            }
        }

        public void DeleteDocument(ContentItem docItem)
        {
            if (DeleteDocumentItem != null)
            {
                DeleteDocumentItem(docItem);
            }
        }

        public void SaveAllBatches()
        {
            if (SaveAll != null)
            {
                SaveAll();
            }
        }

        public void ImportOCRTemplate(string[] filePath)
        {
            if (DocViewerMode == DocViewerMode.OCRTemplate)
            {
                ContentItem docItem;
                if (Items[0].Children.Count == 0)
                {
                    var document = new DocumentModel(DateTime.Now, UserName, BatchTypes[0].DocTypes[0]);
                    docItem = new ContentItem(document);
                    Items[0].Children.Add(docItem);
                }
                else
                {
                    docItem = Items[0].Children[0];
                    docItem.Children.Clear();
                }

                ImportManager.ImportFromFiles(filePath, docItem, 0);
                ImportManager.ImportFileSystemCompleted += delegate
                {
                    ThumbnailSelector.LeftMouseClick(docItem);
                };

                OCRTemplateFilePath = filePath;
            }
        }

        public void SaveBatch(ContentItem batchItem)
        {
            if (Save != null)
            {
                Save(batchItem);
            }

        }

        public void SaveAllComplete()
        {
            if (Items.Count == 0)
            {
                if (IsAutoCreateBatch)
                {
                    ContentItemManager.CreateBatch(SelectedBatchType);
                    //Items.Add(new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, UserName, SelectedBatchType)));
                    ThumbnailSelector.LeftMouseClick(Items.Last());
                    DocThumbnail.ShowContextMenu(Items.Last());
                }
                else if (IsReopenDialog)
                {
                    ThumbnailCommandManager.CreateNewBatchCommand.Execute(null, null);
                }
            }
        }

        public void ApproveBatch()
        {
            if (ApproveAll != null)
            {
                ApproveAll();
            }
        }

        public void SubmitBatch()
        {
            if (Submit != null && CheckSubmitValid())
            {

                Submit();
            }
        }

        //public void ImportOCRTemplate(string filePath)
        //{
        //    if (DocViewerMode == DocViewerMode.OCRTemplate)
        //    {
        //        ContentItem docItem;
        //        if (Items[0].Children.Count == 0)
        //        {
        //            var document = new DocumentModel(DateTime.Now, UserName, BatchTypes[0].DocTypes[0]);
        //            docItem = new ContentItem(document);
        //            Items[0].Children.Add(docItem);
        //        }
        //        else
        //        {
        //            docItem = Items[0].Children[0];
        //            docItem.Children.Clear();
        //        }

        //        ImportManager.Import(new[] { filePath }, docItem, 0);
        //        ImportManager.ImportCompleted += delegate
        //                                             {
        //                                                 ThumbnailSelector.LeftClick(docItem);
        //                                             };
        //    }
        //}

        public void Backup()
        {
            if (DocViewerMode == DocViewerMode.Capture || DocViewerMode == DocViewerMode.LightCapture)
            {
                new RecoveryHelper(this).Backup();
            }
        }

        public ObservableCollection<ContentItem> Items
        {
            get { return GetValue(ItemsProperty) as ObservableCollection<ContentItem>; }
            set { SetValue(ItemsProperty, value); }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes
        {
            get { return GetValue(BatchTypesProperty) as ObservableCollection<BatchTypeModel>; }
            set { SetValue(BatchTypesProperty, value); }
        }

        public ObservableCollection<CommentModel> Comments
        {
            get { return GetValue(CommentsProperty) as ObservableCollection<CommentModel>; }
            set { SetValue(CommentsProperty, value); }
        }

        public BatchTypeModel SelectedBatchType
        {
            get { return GetValue(SelectedBatchTypeProperty) as BatchTypeModel; }
            set
            {
                SetValue(SelectedBatchTypeProperty, value);
                //InitializeDynamicCommands();
            }
        }

        public string OpenMediaFilePath
        {
            get { return GetValue(OpenMediaFilePathProperty) as string; }
            set { SetValue(OpenMediaFilePathProperty, value); }
        }

        public string OpenNativeFilePath
        {
            get { return GetValue(OpenNativeFilePathProperty) as string; }
            set { SetValue(OpenNativeFilePathProperty, value); }
        }

        public string OpenTextFilePath
        {
            get { return GetValue(OpenTextFilePathProperty) as string; }
            set { SetValue(OpenTextFilePathProperty, value); }
        }

        public string[] OCRTemplateFilePath
        {
            get { return GetValue(OCRTemplateFilePathProperty) as string[]; }
            set { SetValue(OCRTemplateFilePathProperty, value); }
        }

        public ContentItem OpeningContainerItem { get; set; }

        public ContentItem OpeningItem { get; set; }

        public ObservableCollection<ContentItem> OpeningItems
        {
            get { return GetValue(OpeningItemsProperty) as ObservableCollection<ContentItem>; }
            set { SetValue(OpeningItemsProperty, value); }
        }

        public ObservableCollection<FieldValueModel> FieldValues
        {
            get { return GetValue(FieldValuesProperty) as ObservableCollection<FieldValueModel>; }
            set { SetValue(FieldValuesProperty, value); }
        }

        public ObservableCollection<AmbiguousDefinitionModel> AmbiguousDefinitions
        {
            get { return GetValue(AmbiguousDefinitionsProperty) as ObservableCollection<AmbiguousDefinitionModel>; }
            set { SetValue(AmbiguousDefinitionsProperty, value); }
        }

        public string AppName
        {
            get { return GetValue(AppNameProperty) as string; }
            set { SetValue(AppNameProperty, value); }
        }

        public string UserName
        {
            get { return GetValue(UserNameProperty) as string; }
            set { SetValue(UserNameProperty, value); }
        }

        public bool IsProcessing
        {
            get { return (bool)GetValue(IsProcessingProperty); }
            set { SetValue(IsProcessingProperty, value); }
        }

        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        public bool IsChanged
        {
            get { return (bool)GetValue(IsChangedProperty); }
            set { SetValue(IsChangedProperty, value); }
        }

        public DocViewerMode DocViewerMode
        {
            get { return (DocViewerMode)GetValue(DocViewerModeProperty); }
            set { SetValue(DocViewerModeProperty, value); }
        }

        public bool IsReopenDialog
        {
            get { return (bool)GetValue(IsReopenDialogProperty); }
            set { SetValue(IsReopenDialogProperty, value); }
        }

        public bool IsAutoCreateBatch
        {
            get { return (bool)GetValue(IsAutoCreateBatchProperty); }
            set { SetValue(IsAutoCreateBatchProperty, value); }
        }

        public bool EnabledOcrClient
        {
            get { return (bool)GetValue(EnabledOcrClientProperty); }
            set { SetValue(EnabledOcrClientProperty, value); }
        }

        public bool EnabledBarcodeClient
        {
            get { return (bool)GetValue(EnabledBarcodeClientProperty); }
            set { SetValue(EnabledBarcodeClientProperty, value); }
        }

        public Func<FieldModel, string, DataTable> GetLookupData;

        internal ThumbnailSelector ThumbnailSelector { get; private set; }

        internal WorkingFolder WorkingFolder { get; private set; }

        internal FileSystemImportManager ImportManager { get; private set; }

        internal ContentItemManager ContentItemManager { get; private set; }

        internal ScanningManager ScanManager { get; private set; }

        internal CameraManager CameraManager { get; private set; }

        internal ThumbnailViewCommand ThumbnailCommandManager { get; private set; }

        internal PermissionManager PermissionManager { get; private set; }

        internal ToolbarCommand ToolbarCommandManager { get; private set; }

        internal OCRHelper OCRHelper { get; private set; }

        public Action<Exception> HandleException;

        public Action<Exception> LogException;

        public Action<ActionLogModel> AddActionLog;

        public event DeleteBatchEventHandler DeleteBatchItem;

        public event DeleteDocumentEventHandler DeleteDocumentItem;

        public event SaveAllEventHandler SaveAll;

        public event SaveEventHandler Save;

        public event ApproveEventHandler Approve;

        public event ApproveAllEventHandler ApproveAll;

        public event SubmitBatchEventHandler Submit;

        private void DropDownClosing(object sender, RoutedEventArgs e)
        {
            if (_isNativeMode)
            {
                this.PanelNativeViewer.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SettingsButton_DropDownOpenning(object sender, RoutedEventArgs e)
        {
            if (_isNativeMode)
            {
                this.PanelNativeViewer.Visibility = System.Windows.Visibility.Collapsed;
            }

        }

        #region Event handlers

        private void ViewerContainerLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    WorkingFolder = new WorkingFolder(DocViewerMode.ToString());
                    ThumbnailSelector = new ThumbnailSelector(this);
                    ImportManager = new FileSystemImportManager(this);
                    //if (DocViewerMode != DocViewerMode.OCRTemplate)
                    //{
                    ScanManager = new ScanningManager(this);
                    CameraManager = new CameraManager(this);
                    if (DocViewerMode == DocViewerMode.LightCapture || DocViewerMode == DocViewerMode.Capture)
                    {
                        OCRHelper = new OCRHelper(this);
                        string assemblyFolder = WorkingFolder.CreateTempFolder();
                        //BarcodeExtractor.Initialize(assemblyFolder);
                    }
                    //}

                    ContentItemManager = new ContentItemManager(this);
                    ThumbnailCommandManager = new ThumbnailViewCommand(this);
                    PermissionManager = new PermissionManager(this);
                    ToolbarCommandManager = new ToolbarCommand(this);
                    DocThumbnail.ViewerContainer = this;
                    ImageViewer.ViewerContainer = this;
                    CommentView.ViewerContainer = this;
                    ThumbnailSelector.PropertyChanged += ThumbnailSelectorPropertyChanged;
                    IndexViewer.ViewerContainer = this;
                    RegisterCommands();
                    InitializeDefaultSelection();
                    SetToolbarButtonCommand();
                    InvalidateUI();
                    LoadOCRTemplate();
                    //Restore();
                    _isLoaded = true;
                    btnIndex.Command = ThumbnailCommandManager.IndexCommand;
                    btnBatchIndex.Command = ThumbnailCommandManager.BatchIndexCommand;
                    btnComment.Command = ThumbnailCommandManager.CommentCommand;
                    if (DocViewerMode == Model.DocViewerMode.WorkItem || DocViewerMode == Model.DocViewerMode.Document ||
                        DocViewerMode == Model.DocViewerMode.LightCapture || DocViewerMode == Model.DocViewerMode.OCRTemplate)
                    {
                        btnCreateBatch.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        btnCreateBatch.Visibility = System.Windows.Visibility.Visible;
                    }
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void Restore()
        {
            //if (WorkingFolder != null && BatchTypes != null && (DocViewerMode == DocViewerMode.Capture || DocViewerMode == DocViewerMode.LightCapture))
            //{
            //    new RecoveryHelper(this).Restore();
            //}
        }

        private void ThumbnailSelectorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DisplayItem();
        }

        private void LeftPanelExpandCollapseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LeftPanelExpandCollapse.IsChecked != null && LeftPanelExpandCollapse.IsChecked.Value)
                {
                    CollaspeLeftPanel();
                }
                else
                {
                    ExpandLeftPanel();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void LeftPanelContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (LeftPanelContainer.Width < 50)
                {
                    CollaspeLeftPanel();
                    LeftPanelExpandCollapse.IsChecked = true;
                }
                else
                {
                    LeftPanelExpandCollapse.IsChecked = false;
                    LeftPanelContent.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void ThumbnailButtonClick(object sender, RoutedEventArgs e)
        {
            ExpandLeftPanel();
            ShowIndexViewer(false);
        }

        //private void IndexButtonClick(object sender, RoutedEventArgs e)
        //{
        //    ExpandLeftPanel();
        //    ThumbnailCommandManager.PopulateIndexPanel(ThumbnailSelector.Cursor);
        //}

        //private void BatchIndexButtonClick(object sender, RoutedEventArgs e)
        //{
        //    ExpandLeftPanel();
        //    ThumbnailCommandManager.PopulateIndexPanel(ThumbnailSelector.Cursor);
        //}

        private void ScanButtonDropDownOpenning(object sender, RoutedEventArgs e)
        {
            BuildContextMenuForButtons(ScanButton, ScanManager.ScanToDocumentTypeCommand);
            LoadScanPageConfigurationMenu(ScanButton);
        }

        private void CameraButtonDropDownOpenning(object sender, RoutedEventArgs e)
        {
            BuildContextMenuForButtons(CameraButton, CameraManager.CaptureContentToDocumentTypeCommand);
        }

        private void ImportButtonDropDownOpenning(object sender, RoutedEventArgs e)
        {
            BuildContextMenuForButtons(ImportButton, ImportManager.ImportFileSystemToDocumentTypeCommand);
        }

        private void BuildContextMenuForButtons(SplitButton button, ICommand menuCommand)
        {
            try
            {
                if (_isNativeMode)
                {
                    this.NativeViewer.Visibility = System.Windows.Visibility.Collapsed;
                }

                var selectedBatch = Items[0];
                if (ThumbnailSelector.SelectedItems.Count > 0)
                {
                    selectedBatch = ThumbnailSelector.SelectedItems[0].BatchItem;
                }

                // Get all document type of current batch
                var documentTypes = BatchTypes.First(p => p.Id == selectedBatch.BatchData.BatchType.Id).DocTypes;
                button.Items.Clear();
                foreach (var documentType in documentTypes)
                {
                    var icon = CreateDocumentIcon(documentType);
                    button.Items.Add(new MenuItem
                                     {
                                         Header = documentType.Name,
                                         Command = menuCommand,
                                         CommandParameter = documentType,
                                         Icon = icon
                                     });
                }

                button.ContextMenu.MinWidth = 250;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        internal Image CreateDocumentIcon(DocTypeModel documentType)
        {
            Image icon = null;
            if (documentType.Icon != null && documentType.Icon.Length > 0)
            {
                icon = new Image();
                icon.Width = icon.Height = 16;
                using (var me = new MemoryStream(documentType.Icon))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    image.StreamSource = me;
                    image.EndInit();
                    icon.Source = image;
                }
            }
            return icon;
        }

        private void LoadScanPageConfigurationMenu(SplitButton button)
        {
            if (scanPageConfigurationMenu == null)
            {
                scanPageConfigurationMenu = new MenuItem
                                                {
                                                    Header = "Scanning pages per document"
                                                };

                MenuItem defaultMenuItem = new MenuItem
                                                {
                                                    Header = "Not set",
                                                };

                defaultMenuItem.Click += DefaultMenuItemClick;
                scanPageConfigurationMenu.Items.Add(defaultMenuItem);

                for (int i = 1; i <= 10; i++)
                {
                    MenuItem subMenu = new MenuItem { Header = i.ToString() };
                    subMenu.Click += SubMenuClick;
                    scanPageConfigurationMenu.Items.Add(subMenu);
                }

                selectedMenuItem = (MenuItem)scanPageConfigurationMenu.Items[0];
                selectedMenuItem.IsChecked = true;
            }
            button.Items.Insert(0, scanPageConfigurationMenu);
        }

        private void DefaultMenuItemClick(object sender, RoutedEventArgs e)
        {
            SetSelectedMenuItem(sender);
            ScanManager.NumberOfPagesToCreateNewDocumentInNormalScanning = 0;
        }

        private void SubMenuClick(object sender, RoutedEventArgs e)
        {
            SetSelectedMenuItem(sender);
            ScanManager.NumberOfPagesToCreateNewDocumentInNormalScanning = int.Parse(selectedMenuItem.Header.ToString());
        }

        private void SetSelectedMenuItem(object sender)
        {
            selectedMenuItem.IsChecked = false;
            selectedMenuItem = (MenuItem)sender;
            selectedMenuItem.IsChecked = true;
        }

        #endregion

        #region Helper methods

        private void RegisterCommands()
        {
            //if (DocViewerMode != DocViewerMode.OCRTemplate)
            //{
            ImportButton.Command = ImportManager.ImportFileSystemCommand;
            ScanButton.Command = ScanManager.ScanCommand;
            CameraButton.Command = CameraManager.CaptureContentCommand;
            ChooseDefaultScannerMenu.Command = ScanManager.SelectDefaultScannerCommand;
            ChooseDefaultCameraMenu.Command = CameraManager.SelectDefaultCameraCommand;
            ChooseDefaultMicrophoneMenu.Command = CameraManager.SelectDefaultMicCommand;
            ShowScannerDialogMenu.Command = ScanManager.ShowHideScannerDialogCommand;
            //}

            // TODO: For workflow
            //if (DocViewerMode == DocViewerMode.LightCapture)
            //{
            //    LblButtonSave.Text = "Submit";
            //}

            if (DocViewerMode != Model.DocViewerMode.WorkItem)
            {
                ButtonSubmit.Visibility = System.Windows.Visibility.Hidden;
            }

            ButtonSave.Command = ThumbnailCommandManager.SaveCommand;
            ButtonSubmit.Command = ThumbnailCommandManager.SubmitBatchCommand;
            btnCreateBatch.Command = ThumbnailCommandManager.CreateNewBatchCommand;
        }

        private void InvalidateUI()
        {
            if (DocViewerMode == DocViewerMode.OCRTemplate)
            {
                //LeftPanelContainer.Visibility = Visibility.Collapsed;
                //LeftSplitter.Visibility = Visibility.Collapsed;
                leftButtonPanel.Visibility = Visibility.Collapsed;
                PanelInfo.Visibility = Visibility.Collapsed;
                PanelImageViewer.Visibility = Visibility.Visible;
                PanelToolbar.Visibility = Visibility.Visible;
                ShowHideToolbarButton(null);
                mainButtonPanel.Visibility = Visibility.Collapsed;
                ScanButton.Visibility = Visibility.Collapsed;
                CameraButton.Visibility = Visibility.Collapsed;
                ImportButton.Visibility = Visibility.Collapsed;
                SettingsButton.Visibility = Visibility.Collapsed;
            }
            else if (DocViewerMode == DocViewerMode.Document ||
                     DocViewerMode == DocViewerMode.WorkItem)
            {
                leftButtonPanel.Visibility = Visibility.Visible;
                mainButtonPanel.Visibility = ReadOnly ? Visibility.Collapsed : Visibility.Visible;
                ScanButton.Visibility = Visibility.Collapsed;
                CameraButton.Visibility = Visibility.Collapsed;
                ImportButton.Visibility = Visibility.Collapsed;
                SettingsButton.Visibility = Visibility.Collapsed;
            }

            if (BatchTypes != null &&
                !BatchTypes.Any(p => p.DocTypes.Any(q => q.DocTypePermission.CanAccess)))
            {
                SettingsButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ExpandLeftPanel()
        {
            LeftPanelContent.Visibility = Visibility.Visible;
            LeftPanelContainer.Width = 350;
        }

        private void CollaspeLeftPanel()
        {
            LeftPanelContent.Visibility = Visibility.Collapsed;
            LeftPanelContainer.Width = 25;
        }

        internal void InitializeDefaultSelection()
        {
            if (Items != null && Items.Count > 0 && IsLoaded)
            {
                ThumbnailSelector.InitializeSelection();
            }
        }

        private void DisplayImages(ContentItem pageItem)
        {
            try
            {
                ImageViewer.Permission = PermissionManager.GetContentViewerPermission(pageItem);
                if (pageItem.Parent.ItemType == ContentItemType.Batch)
                {
                    pageItem.Load(this);
                    OpeningItems = new ObservableCollection<ContentItem> { pageItem };
                }
                else
                {
                    if (OpeningItems == null || OpeningItems != pageItem.Parent.Children)
                    {
                        foreach (var item in pageItem.Parent.Children)
                        {
                            item.Load(this);
                        }

                        OpeningItems = pageItem.Parent.Children;
                    }

                    ImageViewer.GotoPage(OpeningItems.IndexOf(pageItem) + 1);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }

        private void ShowHideToolbarButton(ContentItem pageItem)
        {
            try
            {
                ContentViewerPermission permission = new ContentViewerPermission();
                if (DocViewerMode == DocViewerMode.Capture || DocViewerMode == DocViewerMode.LightCapture)
                {
                    permission = ContentViewerPermission.GetAllowAll();
                }
                else
                {
                    permission = PermissionManager.GetContentViewerPermission(pageItem);
                }
                ButtonEmail.Visibility = permission.CanEmail ? Visibility.Visible : Visibility.Collapsed;
                ButtonSaveAs.Visibility = ButtonEmail.Visibility;
                bool hasButtonVisible = permission.CanEmail;

                if (pageItem != null && pageItem.PageData.FileType == FileTypeModel.Image)
                {
                    ButtonPrint.Visibility = permission.CanPrint ? Visibility.Visible : Visibility.Collapsed;
                    if (ButtonSaveAs.Visibility == Visibility.Visible)
                    {
                        if (OpeningItems != null && OpeningItems.Select(p => p.PageData.FileType).Distinct().Count() > 1)
                        {
                            ButtonSaveAs.Visibility = Visibility.Collapsed;
                        }
                    }

                    ButtonPan.Visibility = Visibility.Visible;
                    hasButtonVisible = true;

                    if (permission.CanAddHighlight || permission.CanAddLine || permission.CanAddRedaction || permission.CanAddText ||
                        permission.CanDeleteHighlight || permission.CanDeleteLine || permission.CanDeleteRedaction || permission.CanDeleteText ||
                        permission.CanApplyOCRTemplate)
                    {
                        ButtonSelection.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ButtonSelection.Visibility = Visibility.Collapsed;
                    }

                    ButtonHideAnnotation.Visibility = permission.CanHideAnnotation ? Visibility.Visible : Visibility.Collapsed;
                    ButtonHightlight.Visibility = permission.CanAddHighlight ? Visibility.Visible : Visibility.Collapsed;
                    ButtonRedaction.Visibility = permission.CanAddRedaction ? Visibility.Visible : Visibility.Collapsed;
                    ButtonText.Visibility = permission.CanAddText ? Visibility.Visible : Visibility.Collapsed;
                    ButtonOCRZone.Visibility = permission.CanApplyOCRTemplate ? Visibility.Visible : Visibility.Collapsed;
                    ButtonZoomin.Visibility = Visibility.Visible;
                    ButtonZoomout.Visibility = Visibility.Visible;
                    //ButtonFitHeight.Visibility = Visibility.Visible;
                    //ButtonFitWidth.Visibility = Visibility.Visible;
                    ButtonFitToViewer.Visibility = Visibility.Visible;
                    ButtonRotateLeft.Visibility = ButtonRotateRight.Visibility = DocViewerMode == Model.DocViewerMode.OCRTemplate ? Visibility.Visible : (PermissionManager.CanRotate() ? Visibility.Visible : Visibility.Collapsed);
                    ButtonPreviousPage.Visibility = Visibility.Visible;
                    ButtonNextPage.Visibility = Visibility.Visible;

                    //ButtonHightlight.IsEnabled = ButtonRedaction.IsEnabled = ButtonText.IsEnabled = ButtonHideAnnotation.IsEnabled = ButtonOCRZone.IsEnabled = (OpeningItems != null && OpeningItems.Count > 0);
                }
                else
                {
                    ButtonPrint.Visibility = Visibility.Collapsed;
                    ButtonPan.Visibility = Visibility.Collapsed;
                    ButtonSelection.Visibility = Visibility.Collapsed;
                    ButtonHideAnnotation.Visibility = Visibility.Collapsed;
                    ButtonHightlight.Visibility = Visibility.Collapsed;
                    ButtonRedaction.Visibility = Visibility.Collapsed;
                    ButtonText.Visibility = Visibility.Collapsed;
                    ButtonOCRZone.Visibility = Visibility.Collapsed;
                    ButtonZoomin.Visibility = Visibility.Collapsed;
                    ButtonZoomout.Visibility = Visibility.Collapsed;
                    //ButtonFitHeight.Visibility = Visibility.Collapsed;
                    //ButtonFitWidth.Visibility = Visibility.Collapsed;
                    ButtonFitToViewer.Visibility = Visibility.Collapsed;
                    ButtonRotateLeft.Visibility = Visibility.Collapsed;
                    ButtonRotateRight.Visibility = Visibility.Collapsed;
                    ButtonPreviousPage.Visibility = Visibility.Collapsed;
                    ButtonNextPage.Visibility = Visibility.Collapsed;
                }

                PanelToolbar.Visibility = hasButtonVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }

        private void SetToolbarButtonCommand()
        {
            ToolbarCommandManager.Initialize();
            ButtonPrint.Command = ToolbarCommandManager.PrintCommand;
            ButtonEmail.Command = ToolbarCommandManager.EmailCommand;
            ButtonSaveAs.Command = ToolbarCommandManager.SaveAsCommand;
            ButtonZoomin.Command = ToolbarCommandManager.ZoomInCommand;
            ButtonZoomout.Command = ToolbarCommandManager.ZoomOutCommand;
            //ButtonFitHeight.Command = ToolbarCommandManager.FitHeightCommand;
            //ButtonFitWidth.Command = ToolbarCommandManager.FitWidthCommand;
            ButtonFitToViewer.Command = ToolbarCommandManager.FitToViewerCommand;
            ButtonRotateLeft.Command = ThumbnailCommandManager.RotateLeftCommand;
            ButtonRotateRight.Command = ThumbnailCommandManager.RotateRightCommand;
            ButtonPreviousPage.Command = ToolbarCommandManager.PreviousPageCommand;
            ButtonNextPage.Command = ToolbarCommandManager.NextPageCommand;
        }

        private void CleanupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _cleanupTimer.Stop();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void LoadOCRTemplate()
        {
            if (DocViewerMode == DocViewerMode.OCRTemplate && IsLoaded &&
                BatchTypes != null && BatchTypes.Count > 0 &&
                BatchTypes[0].DocTypes != null && BatchTypes[0].DocTypes.Count > 0)
            //&& BatchTypes[0].DocTypes[0].OCRTemplate != null && BatchTypes[0].DocTypes[0].OCRTemplate.DocTypeId != Guid.Empty)
            {
                var docType = BatchTypes[0].DocTypes.FirstOrDefault(p => p.IsSelected);
                ContentItem selectDoc = null;
                OCRTemplateModel ocrTemplate = docType.OCRTemplate;

                if (ocrTemplate != null)
                {
                    var document = new DocumentModel(DateTime.Now, UserName, docType);
                    var docItem = new ContentItem(document);

                    Items[0].DocumentData = document;
                    Items[0].Children.Add(docItem);
                    Items[0].BatchData.Documents.Add(document);

                    selectDoc = docItem;

                    var ocrTemplatePages = ocrTemplate.OCRTemplatePages.OrderBy(p => p.PageIndex).ToList();
                    foreach (var ocrTemplatePage in ocrTemplatePages)
                    {
                        var templateZones = ocrTemplatePage.OCRTemplateZones.ToList();
                        templateZones.ForEach(p => p.FieldMetaData = docType.Fields.FirstOrDefault(q => q.Id == p.FieldMetaDataId));
                        var annotations = (from p in templateZones
                                           select new AnnotationModel
                                           {
                                               CreatedBy = p.CreatedBy,
                                               CreatedOn = p.CreatedOn.Value,
                                               ModifiedBy = p.ModifiedBy,
                                               ModifiedOn = p.ModifiedOn,
                                               Height = p.Height,
                                               Top = p.Top,
                                               Width = p.Width,
                                               Left = p.Left,
                                               Type = AnnotationTypeModel.OCRZone,
                                               OCRTemplateZone = p,
                                               MetaFields = docType.Fields
                                           }).ToList();
                        var page = new PageModel { Annotations = annotations, FileExtension = ocrTemplatePage.FileExtension, RotateAngle = ocrTemplatePage.RotateAngle, Width = ocrTemplatePage.Width, Height = ocrTemplatePage.Height };
                        var pageItem = new ContentItem(page, ocrTemplatePage.Binary);

                        docItem.PageData = page;
                        docItem.Children.Add(pageItem);
                        docItem.DocumentData.Pages.Add(page);
                    }
                }
                else
                {
                    selectDoc = Items[0].Children[0];
                }

                ThumbnailSelector.LeftMouseClick(selectDoc);
            }
        }

        private void RefreshItems()
        {
            if (Items != null)
            {
                Items.CollectionChanged += ItemsCollectionChanged;
                ShowIndexViewer(false);
            }
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShowIndexViewer(false);
        }

        private bool CheckSubmitValid()
        {
            if (Items.Any(p => p.Rejected) && !PermissionManager.CanReject() && DocViewerMode == Model.DocViewerMode.WorkItem)
            {
                string message = string.Format(_resource.GetString("uiNoPermissionReject"), UserName);
                DialogService.ShowMessageDialog(message);
                return false;
            }

            return true;
        }
        #endregion

    }
}