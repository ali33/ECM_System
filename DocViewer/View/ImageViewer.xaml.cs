using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Forms;
using Ecm.DocViewer.Controls;
using Ecm.DocViewer.Model;
using Ecm.Model;
using Ecm.DocViewer.Helper;
using Ecm.Mvvm;
using Ecm.Utility;

namespace Ecm.DocViewer
{
    public partial class ImageViewer
    {
        private List<CanvasElement> _canvasItems = new List<CanvasElement>();
        private bool _firstTimeOpenItems = true;
        private bool _isLoaded;
        private AnnotationControl _promptOCRZone;
        private CanvasElement _promptOCRPage;
        private ImageViewerCommandManager _commandManager;
        private RelayCommand _zoominCommand;
        private RelayCommand _zoomOutCommand;
        private RelayCommand _nextPageCommand;
        private RelayCommand _previousPageCommand;
        private RelayCommand _fitWidthCommand;
        private RelayCommand _fitHeightCommand;
        private RelayCommand _fitToViewerCommand;
        private RelayCommand _printCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _emailCommand;

        public static readonly DependencyProperty ItemsProperty =
           DependencyProperty.Register("Items", typeof(ObservableCollection<ContentItem>), typeof(ImageViewer),
                new FrameworkPropertyMetadata(new ObservableCollection<ContentItem>(), ItemsChangedCallback));

        private static void ItemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null) viewer.LoadItems();
        }

        public static readonly DependencyProperty EnableReadModeProperty =
           DependencyProperty.Register("EnableReadMode", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(true, EnableReadModeChangedCallback));

        private static void EnableReadModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(true, false, false, false, false, false, false);
                }
                else
                {
                    viewer.SwitchMode(false, viewer.EnableSelection, viewer.EnableRedaction, viewer.EnableHighlight,
                                      viewer.EnableText, viewer.EnableLine, viewer.EnableOCRZone);
                }
            }
        }

        public static readonly DependencyProperty EnableSelectionProperty =
           DependencyProperty.Register("EnableSelection", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableSelectionChangedCallback));

        private static void EnableSelectionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(false, true, false, false, false, false, false);
                }
                else
                {
                    viewer.SwitchMode(viewer.EnableReadMode, false, viewer.EnableRedaction, viewer.EnableHighlight, viewer.EnableText, viewer.EnableLine, viewer.EnableOCRZone);
                }
            }
        }

        public static readonly DependencyProperty EnableRedactionProperty =
           DependencyProperty.Register("EnableRedaction", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableRedactionChangedCallback));

        private static void EnableRedactionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(false, false, true, false, false, false, false);
                }
                else
                {
                    viewer.SwitchMode(viewer.EnableReadMode, viewer.EnableSelection, false, viewer.EnableHighlight, viewer.EnableText, viewer.EnableLine, viewer.EnableOCRZone);
                }
            }
        }

        public static readonly DependencyProperty EnableHideAnnotationProperty =
           DependencyProperty.Register("EnableHideAnnotation", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableHideAnnotationChangedCallback));

        private static void EnableHideAnnotationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                viewer.HideAnnotation((bool)e.NewValue);
            }
        }

        public static readonly DependencyProperty EnableHighlightProperty =
           DependencyProperty.Register("EnableHighlight", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableHighlightChangedCallback));

        private static void EnableHighlightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(false, false, false, true, false, false, false);
                }
                else
                {
                    viewer.SwitchMode(viewer.EnableReadMode, viewer.EnableSelection, viewer.EnableRedaction, false, viewer.EnableText, viewer.EnableLine, viewer.EnableOCRZone);
                }
            }
        }

        public static readonly DependencyProperty EnableTextProperty =
           DependencyProperty.Register("EnableText", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableTextChangedCallback));

        private static void EnableTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(false, false, false, false, true, false, false);
                }
                else
                {
                    viewer.SwitchMode(viewer.EnableReadMode, viewer.EnableSelection, viewer.EnableRedaction, viewer.EnableHighlight, false, viewer.EnableLine, viewer.EnableOCRZone);
                }
            }
        }

        public static readonly DependencyProperty EnableLineProperty =
           DependencyProperty.Register("EnableLine", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableLineChangedCallback));

        private static void EnableLineChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(false, false, false, false, false, true, false);
                }
                else
                {
                    viewer.SwitchMode(viewer.EnableReadMode, viewer.EnableSelection, viewer.EnableRedaction, viewer.EnableHighlight, viewer.EnableText, false, viewer.EnableOCRZone);
                }
            }
        }

        public static readonly DependencyProperty EnableOCRZoneProperty =
           DependencyProperty.Register("EnableOCRZone", typeof(bool), typeof(ImageViewer),
               new FrameworkPropertyMetadata(false, EnableOCRZoneChangedCallback));

        private static void EnableOCRZoneChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ImageViewer;
            if (viewer != null)
            {
                if ((bool)e.NewValue)
                {
                    viewer.SwitchMode(false, false, false, false, false, false, true);
                }
                else
                {
                    viewer.SwitchMode(viewer.EnableReadMode, viewer.EnableSelection, viewer.EnableRedaction, viewer.EnableHighlight, viewer.EnableText, viewer.EnableLine, false);
                }
            }
        }

        public static readonly DependencyProperty HasSelectedPageProperty =
           DependencyProperty.Register("HasSelectedPage", typeof(bool), typeof(ImageViewer));

        public static readonly DependencyProperty OriginalImageChangedProperty =
           DependencyProperty.Register("OriginalImageChanged", typeof(bool), typeof(ImageViewer));

        public static readonly DependencyProperty IsChangedProperty =
           DependencyProperty.Register("IsChanged", typeof(bool), typeof(ImageViewer));

        public static readonly DependencyProperty EmailFileNameProperty =
           DependencyProperty.Register("EmailFileName", typeof(string), typeof(ImageViewer));

        public static readonly DependencyProperty NeedToBeClosedProperty =
           DependencyProperty.Register("NeedToBeClosed", typeof(bool), typeof(ImageViewer));
        
        public static readonly DependencyProperty UserNameProperty =
           DependencyProperty.Register("UserName", typeof(string), typeof(ImageViewer));

        public static readonly DependencyProperty PermissionProperty =
           DependencyProperty.Register("Permission", typeof(ContentViewerPermission), typeof(ImageViewer));

        public ImageViewer()
        {
            InitializeComponent();
            Loaded += ViewerLoaded;
        }

        public void PrintFile()
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

        public void EmailFile()
        {
            ExportFileHelper export = new ExportFileHelper(ViewerContainer);
            export.SendMail();
        }

        public void SaveAs()
        {
            ExportFileHelper export = new ExportFileHelper(ViewerContainer);
            export.SaveFile();
        }

        private void PrintHelperEndPrint(object sender, EventArgs e)
        {
            ViewerContainer.IsProcessing = false;
            CollectGarbageHelper.CollectGarbage();
        }

        private void PrintHelperStartPrint(object sender, EventArgs e)
        {
            ViewerContainer.IsProcessing = true;
        }

        public void ArrangeLayout()
        {
            MyLayoutCanvas.ArrangeLayout();
        }

        public void GotoPage(int pageNumber)
        {
            MyLayoutCanvas.GoToItem(pageNumber);
        }

        public void PromptOCRZone(int pageNumber, OCRTemplateZoneModel zone, OCRTemplatePageModel page)
        {
            _promptOCRPage = null;
            _promptOCRZone = null;
            if (CanvasItems.Count >= pageNumber)
            {
                MyLayoutCanvas.GoToItem(pageNumber);
                CanvasElement focusPage = CanvasItems[pageNumber - 1];
                var info = new AnnotationModel
                {
                    Width = zone.Width,
                    Height = zone.Height, 
                    Top = zone.Top,
                    Left = zone.Left,
                    Type = AnnotationTypeModel.OCRZone,
                    OCRTemplateZone = zone,
                    CreatedBy = zone.CreatedBy,
                    CreatedOn = zone.CreatedOn,
                    ModifiedBy = zone.ModifiedBy,
                    ModifiedOn = zone.ModifiedOn
                };

                _promptOCRZone = new AnnotationControl(info, focusPage);
                _promptOCRZone.IsNew = false;
                _promptOCRZone.IsSelected = false;
                _promptOCRPage = focusPage;
                _promptOCRZone.BringIntoView();
                var blinkAnimation = new DoubleAnimationUsingKeyFrames
                                         {
                                             RepeatBehavior = new RepeatBehavior(5),
                                             Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100)),
                                             AutoReverse = true,
                                             KeyFrames = new DoubleKeyFrameCollection
                                                             {
                                                                 new DiscreteDoubleKeyFrame { Value = 0, KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 50)) },
                                                                 new DiscreteDoubleKeyFrame { Value = 1, KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 100)) }
                                                             }
                                         };
                _promptOCRZone.BeginAnimation(OpacityProperty, blinkAnimation);
            }
        }

        public void UnPromptOCRZone()
        {
            if (_promptOCRZone != null && _promptOCRPage != null)
            {
                _promptOCRPage.Children.Remove(_promptOCRZone);
            }
        }

        public ObservableCollection<ContentItem> Items
        {
            get { return GetValue(ItemsProperty) as ObservableCollection<ContentItem>; }
            set { SetValue(ItemsProperty, value); }
        }

        public List<CanvasElement> CanvasItems
        {
            get { return _canvasItems; }
            private set { _canvasItems = value; }
        }

        public bool EnableReadMode
        {
            get { return (bool)GetValue(EnableReadModeProperty); }
            set { SetValue(EnableReadModeProperty, value); }
        }

        public bool EnableSelection
        {
            get { return (bool)GetValue(EnableSelectionProperty); }
            set { SetValue(EnableSelectionProperty, value); }
        }

        public bool EnableRedaction
        {
            get { return (bool)GetValue(EnableRedactionProperty); }
            set { SetValue(EnableRedactionProperty, value); }
        }

        public bool EnableHideAnnotation
        {
            get { return (bool)GetValue(EnableHideAnnotationProperty); }
            set { SetValue(EnableHideAnnotationProperty, value); }
        }

        public bool EnableHighlight
        {
            get { return (bool)GetValue(EnableHighlightProperty); }
            set { SetValue(EnableHighlightProperty, value); }
        }

        public bool EnableText
        {
            get { return (bool)GetValue(EnableTextProperty); }
            set { SetValue(EnableTextProperty, value); }
        }

        public bool EnableLine
        {
            get { return (bool)GetValue(EnableLineProperty); }
            set { SetValue(EnableLineProperty, value); }
        }

        public bool EnableOCRZone
        {
            get { return (bool)GetValue(EnableOCRZoneProperty); }
            set { SetValue(EnableOCRZoneProperty, value); }
        }

        public bool HasSelectedPage
        {
            get { return (bool)GetValue(HasSelectedPageProperty); }
            set { SetValue(HasSelectedPageProperty, value); }
        }

        public bool IsChanged
        {
            get { return (bool)GetValue(IsChangedProperty); }
            set { SetValue(IsChangedProperty, value); }
        }

        public bool NeedToBeClosed
        {
            get { return (bool)GetValue(NeedToBeClosedProperty); }
            set { SetValue(NeedToBeClosedProperty, value); }
        }

        public string EmailFileName
        {
            get { return GetValue(EmailFileNameProperty) + ""; }
            set { SetValue(EmailFileNameProperty, value); }
        }

        public string UserName
        {
            get { return GetValue(UserNameProperty) + string.Empty; }
            set { SetValue(UserNameProperty, value); }
        }

        public ContentViewerPermission Permission
        {
            get { return GetValue(PermissionProperty) as ContentViewerPermission; }
            set { SetValue(PermissionProperty, value); }
        }

        public ViewerContainer ViewerContainer { get; set; }

        //public RoutedCommand PrintCommand;

        //public RoutedCommand EmailCommand;

        //public RoutedCommand SaveAsCommand;

        //public RoutedCommand ZoomInCommand;

        //public RoutedCommand ZoomOutCommand;

        //public RoutedCommand NextPageCommand;

        //public RoutedCommand PreviousPageCommand;

        //public RoutedCommand FitWidthCommand;

        //public RoutedCommand FitHeightCommand;

        //public RoutedCommand FitToViewerCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand(p => PrintFile(), p => CanPrint());
                }

                return _printCommand;
            }
        }

        public ICommand EmailCommand
        {
            get
            {
                if (_emailCommand == null)
                {
                    _emailCommand = new RelayCommand(p => EmailFile(), p => CanEmail());
                }

                return _emailCommand;
            }

        }

        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => SaveAs(), p => CanSaveAs());
                }

                return _saveCommand;
            }
        }

        //public RoutedCommand ZoomInCommand;
        public ICommand ZoomInCommand
        {
            get
            {
                if (_zoominCommand == null)
                {
                    _zoominCommand = new RelayCommand(p => ZoomIn());
                }

                return _zoominCommand;
            }
        }

        public ICommand ZoomOutCommand
        {
            get
            {
                if (_zoomOutCommand == null)
                {
                    _zoomOutCommand = new RelayCommand(p => ZoomOut());
                }

                return _zoomOutCommand;
            }
        }

        public ICommand NextPageCommand
        {
            get
            {
                if (_nextPageCommand == null)
                {
                    _nextPageCommand = new RelayCommand(p => MoveNext(), p => CanMoveNext());
                }

                return _nextPageCommand;
            }
        }

        public ICommand PreviousPageCommand
        {
            get
            {
                if (_previousPageCommand == null)
                {
                    _previousPageCommand = new RelayCommand(p => MovePrevious(), p => CanMovePrevious());
                }

                return _previousPageCommand;
            }

        }

        public ICommand FitWidthCommand
        {
            get
            {
                if (_fitWidthCommand == null)
                {
                    _fitWidthCommand = new RelayCommand(p => FitWidth());
                }

                return _fitWidthCommand;
            }
        }

        public ICommand FitHeightCommand
        {
            get
            {
                if (_fitHeightCommand == null)
                {
                    _fitHeightCommand = new RelayCommand(p => FitHeight());
                }

                return _fitHeightCommand;
            }
        }

        public ICommand FitToViewerCommand
        {
            get
            {
                if (_fitToViewerCommand == null)
                {
                    _fitToViewerCommand = new RelayCommand(p => FitToViewer());
                }

                return _fitToViewerCommand;
            }
        }
        private void ViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                MyLayoutCanvas.ContentChanged += OnContentChanged;
                MyLayoutCanvas.DoubleClickOnAnnotation += MyLayoutCanvasDoubleClickOnAnnotation;
                MyLayoutCanvas.DoubleClickOnWorkspace += MyLayoutCanvasDoubleClickOnWorkspace;

                // The event handler for this event is crashed in build if we declare in xmal. So it must be here
                //if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
                //{
                //    mnuAddHighlight.Visibility = System.Windows.Visibility.Collapsed;
                //    mnuAddText.Visibility = System.Windows.Visibility.Collapsed;
                //    mnuAddRedaction.Visibility = System.Windows.Visibility.Collapsed;
                //    mnuHideAnnotation.Visibility = System.Windows.Visibility.Collapsed;
                //    mnImageZone.Visibility = System.Windows.Visibility.Collapsed;
                //}
                //else
                //{
                //    if (Permission != null)
                //    {
                //        mnuAddHighlight.Visibility = Permission.CanAddHighlight ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //        mnuAddText.Visibility = Permission.CanAddText ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //        mnuAddRedaction.Visibility = Permission.CanAddRedaction ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //        mnuHideAnnotation.Visibility = Permission.CanHideAnnotation ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //        mnImageZone.Visibility = (Permission.CanAddHighlight || Permission.CanAddRedaction || Permission.CanAddText || Permission.CanHideAnnotation) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //    }
                //}

                LoadItems();
                InitCommand();
            }
        }

        private void InitCommand()
        {
            if (_commandManager == null)
            {
                _commandManager = new ImageViewerCommandManager(this);
            }

            _commandManager.Initialize();

            mnuPrint.Command = PrintCommand;
            mnuEmail.Command = EmailCommand;
            mnuSave.Command = SaveAsCommand;
            mnuZoomIn.Command = ZoomInCommand;
            mnuZoomOut.Command = ZoomOutCommand;
            mnuNextPage.Command = NextPageCommand;
            mnuPreviousPage.Command = PreviousPageCommand;
            mnuFitHeight.Command = FitHeightCommand;
            mnuFitWidth.Command = FitWidthCommand;
            mnuFitToViewer.Command = FitToViewerCommand;
        }

        private void MyLayoutCanvasDoubleClickOnWorkspace(object sender, EventArgs e)
        {
            var canvas = sender as CanvasElement;
            if (canvas != null && canvas.IsNonImagePreview)
            {
                var item = Items.First(p => p.Image == canvas);
                ViewerContainer.ThumbnailSelector.LeftClick(item);
                ViewerContainer.DocThumbnail.Focus(item);
            }
            else
            {
                if (!EnableReadMode)
                {
                    SwitchMode(true, false, false, false, false, false, false);
                }
            }
        }

        private void MyLayoutCanvasDoubleClickOnAnnotation(object sender, EventArgs e)
        {
            SwitchMode(false, true, false, false, false, false, false);

            // Change text annotation to edit mode
            var annotation = sender as AnnotationControl;
            if (annotation != null)
            {
                annotation.Select();
                if (annotation.AnnotationInfo.Type == AnnotationTypeModel.Text)
                {
                    annotation.ShowTextBox();
                }
            }
        }

        private void LoadItems()
        {
            if (Items != null && Items.Count > 0)
            {
                CanvasItems = (from p in Items select p.Image).ToList();
                CanvasItems.ForEach(p => p.ContentChanged += OnContentChanged);

                MyLayoutCanvas.Initialize(Items.ToList());
                SwitchMode(true, false, false, false, false, false, false);

                Items.CollectionChanged += ItemsCollectionChanged;
                MyLayoutCanvas.UpdateLayout();

                if (_firstTimeOpenItems)
                {
                    MyLayoutCanvas.FitHeight();
                    MyLayoutCanvas.UpdateLayout();
                    _firstTimeOpenItems = false;
                }
                else
                {
                    MyLayoutCanvas.RefreshCurrentZoom();
                }
            }
            else
            {
                CanvasItems.ForEach(p => p.ContentChanged -= OnContentChanged);
                CanvasItems.Clear();
                MyLayoutCanvas.ChildItems.Clear();
                MyLayoutCanvas.Children.Clear();
            }
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in Items)
            {
                item.Load(ViewerContainer);
            }

            CanvasItems = (from p in Items select p.Image).ToList();
            MyLayoutCanvas.Initialize(Items.ToList());
            SwitchMode(true, false, false, false, false, false, false);
            MyLayoutCanvas.RefreshCurrentZoom();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
                (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down)
            {
                if (e.Delta > 0)
                {
                    MyLayoutCanvas.ZoomIn();
                }
                else if (e.Delta < 0)
                {
                    MyLayoutCanvas.ZoomOut();
                }

                e.Handled = true;
            }
        }

        private void SwitchMode(bool read, bool selection, bool redaction, bool highlight, bool text, bool line, bool OCRZone)
        {
            try
            {
                MyLayoutCanvas.EnableReadMode = EnableReadMode = read;
                foreach (CanvasElement item in CanvasItems)
                {
                    if (!selection)
                    {
                        item.DeselectAll();
                    }

                    item.EnableSelection = EnableSelection = selection;
                    item.EnableRedaction = EnableRedaction = redaction;
                    item.EnableHighlight = EnableHighlight = highlight;
                    item.EnableText = EnableText = text;
                    item.EnableLine = EnableLine = line;
                    item.EnableOCRZone = EnableOCRZone = OCRZone;
                    item.EnableDrawingMode(redaction || text || highlight || line || OCRZone);
                }

                if (!selection && !redaction && !highlight && !text && !line && !OCRZone)
                {
                    EnableReadMode = true;
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void HideAnnotation(bool hide)
        {
            foreach (CanvasElement item in CanvasItems)
            {
                item.EnableHideAnnotation = hide;
            }
        }

        private void OnContentChanged(object sender, EventArgs e)
        {
            IsChanged = true;
            ViewerContainer.IsChanged = true;
        }

        private void MnuHideAnnotationClick(object sender, RoutedEventArgs e)
        {
            EnableHideAnnotation = true;
        }

        private void MnuAddRedactionClick(object sender, RoutedEventArgs e)
        {
            EnableRedaction = true;
        }

        private void MnuAddHighlightClick(object sender, RoutedEventArgs e)
        {
            EnableHighlight = true;
        }

        private void MnuAddTextClick(object sender, RoutedEventArgs e)
        {
            EnableText = true;
        }
        #region Command execution
        private bool CanPrint()
        {
            return Permission != null && Permission.CanPrint;
        }

        private bool CanSaveAs()
        {
            return Permission != null && Permission.CanEmail;
        }

        private bool CanEmail()
        {
            return Permission != null && Permission.CanEmail;
        }

        //public void PrintFile()
        //{
        //    try
        //    {
        //        var items = new List<CanvasElement>();
        //        var canvasItems = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).Select(p => p.Image).ToList();
        //        if (canvasItems.Count > 0)
        //        {
        //            foreach (CanvasElement item in canvasItems)
        //            {
        //                CanvasElement clonedItem = item.Clone();
        //                clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
        //                items.Add(clonedItem);
        //            }

        //            var printHelper = new PrintHelper(canvasItems.Count, ViewerContainer.AppName, ViewerContainer.WorkingFolder) { HandleException = ViewerContainer.HandleException };
        //            printHelper.StartPrint += PrintHelperStartPrint;
        //            printHelper.EndPrint += PrintHelperEndPrint;
        //            printHelper.Print(items);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewerContainer.HandleException(ex);
        //    }
        //}

        //public void EmailFile()
        //{
        //    ExportFileHelper export = new ExportFileHelper(ViewerContainer);
        //    export.SendMail();
        //}

        //public void SaveAs()
        //{
        //    ExportFileHelper export = new ExportFileHelper(ViewerContainer);
        //    export.SaveFile();
        //}

        private void ZoomIn()
        {
            this.MyLayoutCanvas.ZoomIn();
        }

        private void ZoomOut()
        {
            this.MyLayoutCanvas.ZoomOut();
        }

        private void MoveNext()
        {
            this.MyLayoutCanvas.MoveNext();
        }

        private bool CanMoveNext()
        {
            return this.MyLayoutCanvas.CanMoveNext();
        }

        private bool CanMovePrevious()
        {
            return this.MyLayoutCanvas.CanMovePrevious();
        }

        private void MovePrevious()
        {
            this.MyLayoutCanvas.MovePrevious();
        }

        private void FitWidth()
        {
            this.MyLayoutCanvas.FitWidth();
        }

        private void FitHeight()
        {
            this.MyLayoutCanvas.FitHeight();
        }

        private void FitToViewer()
        {
            this.MyLayoutCanvas.FitToWindow();
        }

        #endregion

    }
}
