using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Ecm.AppHelper;
using Ecm.ContentViewer.Controls;
using Ecm.Mvvm;
using System.Windows.Media;
using Ecm.ContentViewer.Converter;
using Ecm.Utility;
using Ecm.ContentViewer.Model;
using System.Resources;
using System.Reflection;
using Ecm.ContentViewer.Helper;
using Ecm.ContentViewer.View;

namespace Ecm.ContentViewer.Model
{
    public class ContentItem : BaseDependencyProperty
    {
        private string _title1;
        private string _titleBig1;
        private string _title2;
        private string _title2Trans;
        private string _title3;
        private bool _isSelected;
        private double _thumbnailWidth;
        private double _thumbnailHeight;
        private bool _isValid;
        private bool _rejected;
        private BatchModel _batchData;
        private ContentModel _documentData;
        private PageModel _pageData;
        private CanvasElement _itemCanvas;
        private ChangeType _changeType;
        private Brush _thumbnailCollasped;
        private Brush _thumbnailExpanded;
        private bool _isLoaded;
        private bool _isChanged;

        public ContentItem(BatchModel batch)
        {
            BatchData = batch;
            ItemType = ContentItemType.Batch;
            Initialize();
            SetupTitles();
        }

        public ContentItem(ContentModel document)
        {
            DocumentData = document;
            ItemType = ContentItemType.ContentModel;
            Initialize();
        }

        public ContentItem(string filePath, string originalFileName)
        {
            ItemType = ContentItemType.Page;
            FilePath = filePath;
            PageData = new PageModel { FileExtension = GetFileExtension(FilePath), OriginalFileName = originalFileName };
            PageData.FileFormat = (FileFormatModel)new FileFormatConverter().Convert(PageData.FileExtension, null, null, null);
            PageData.FileType = (FileTypeModel)new FileTypeConverter().Convert(PageData.FileExtension, null, null, null);
        }

        public ContentItem(PageModel page, byte[] binary)
        {
            ItemType = ContentItemType.Page;
            PageData = page;
            Binary = binary;
            PageData.FileFormat = (FileFormatModel)new FileFormatConverter().Convert(PageData.FileExtension, null, null, null);
            PageData.FileType = (FileTypeModel)new FileTypeConverter().Convert(PageData.FileExtension, null, null, null);
        }

        public ContentItem(PageModel page, string filePath)
        {
            ItemType = ContentItemType.Page;
            FilePath = filePath;
            PageData = page ?? new PageModel();
        }

        public void SetParent(ContentItem parent)
        {
            Parent = parent;
        }

        public void Load(MainViewer mainViewer, WorkingFolder workingFolder)
        {
            if (!_isLoaded)
            {
                MainViewer = mainViewer;
                if (Image == null)
                {
                    CreateThumbnail();
                }

                if (FilePath == null && Binary != null && PageData.FileType != FileTypeModel.Image)
                {
                    FilePath = workingFolder.Save(Binary, "native_" + Guid.NewGuid().GetHashCode().ToString() + "." + PageData.FileExtension);
                }

                SetupTitles();
                _isLoaded = true;
            }
        }

        public void ResetStatus()
        {
            ChangeType = ChangeType.None;
            IsChanged = false;
            if (Children != null && Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    child.ResetStatus();
                }
            }
        }

        public void Clean()
        {
            if (ThumbnailCollasped != null)
            {
                var brush = ThumbnailCollasped as VisualBrush;
                if (brush != null)
                {
                    brush.Visual = null;
                }

                ThumbnailCollasped = null;
            }

            ThumbnailExpanded = null;
            if (Image != null)
            {
                Image.Clean();
                Image = null;
            }

            if (!string.IsNullOrEmpty(FilePath))
            {
                WorkingFolder.GlobalDelete(FilePath);
            }

            if (Children != null)
            {
                foreach (ContentItem item in Children)
                {
                    item.Clean();
                }

                Children.Clear();
            }
        }

        public void SetDocumentData(ContentModel document)
        {
            if (ItemType == ContentItemType.ContentModel)
            {
                DocumentData = document;
                DocumentData.FieldValues.ForEach(p =>
                {
                    p.PropertyChanged += IndexPropertyChanged;
                });

                IsValid = !DocumentData.FieldValues.Any(p => !p.IsValid);
                SetupTitles();
            }
        }

        public void SetBatchData(BatchModel batch)
        {
            if (ItemType == ContentItemType.Batch)
            {
                BatchData = batch;
                BatchData.FieldValues.ForEach(p =>
                {
                    p.PropertyChanged += IndexPropertyChanged;
                });
            }
        }

        public byte[] GetBinary(ViewerMode mode)
        {
            if (mode == ViewerMode.LightCapture || mode == ViewerMode.ContentModel)
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    using (var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return stream.ReadAllBytes();
                    }
                }

                return Binary;
            }

            // TODO: add code to burn image
            return null;
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ContentItem BatchItem
        {
            get
            {
                if (ItemType == ContentItemType.ContentModel)
                {
                    return Parent;
                }

                if (ItemType == ContentItemType.Batch)
                {
                    return this;
                }

                if (Parent.ItemType == ContentItemType.ContentModel)
                {
                    return Parent.Parent;
                }

                return Parent;
            }
        }

        public ContentItem Front
        {
            get
            {
                if (ItemType == ContentItemType.Batch)
                {
                    return null;
                }

                int currentIndex = Parent.Children.IndexOf(this);
                if (currentIndex == 0)
                {
                    return Parent;
                }

                ContentItem front = Parent.Children[currentIndex - 1];
                if (front.ItemType == ContentItemType.Page)
                {
                    return front;
                }

                return front.Children[front.Children.Count - 1];
            }
        }

        public ContentItem Rear
        {
            get
            {
                if (ItemType != ContentItemType.Page)
                {
                    if (Children.Count > 0)
                    {
                        return Children[0];
                    }

                    return null;
                }

                int currentIndex = Parent.Children.IndexOf(this);
                if (currentIndex < Parent.Children.Count - 1)
                {
                    return Parent.Children[currentIndex + 1];
                }

                if (Parent.ItemType == ContentItemType.Batch)
                {
                    return null;
                }

                currentIndex = Parent.Parent.Children.IndexOf(Parent);
                if (currentIndex == Parent.Parent.Children.Count - 1)
                {
                    return null;
                }

                ContentItem rear = Parent.Parent.Children[currentIndex + 1];
                if (rear.ItemType == ContentItemType.Page)
                {
                    return rear;
                }

                return rear.Children[0];
            }
        }

        public ContentItem Parent { get; private set; }

        public ChangeType ChangeType
        {
            get { return _changeType; }
            set
            {
                _changeType = value;
                OnPropertyChanged("ChangeType");

                if (value != ChangeType.None)
                {
                    IsChanged = true;
                    if (Parent != null)
                    {
                        Parent.IsChanged = true;
                        if (Parent.Parent != null)
                        {
                            Parent.Parent.IsChanged = true;
                        }
                    }
                }
            }
        }

        public ObservableCollection<ContentItem> Children { get; private set; }

        public ContentItemType ItemType { get; private set; }

        public Brush ThumbnailCollasped
        {
            get { return _thumbnailCollasped; }
            private set
            {
                _thumbnailCollasped = value;
                OnPropertyChanged("ThumbnailCollasped");
            }
        }

        public Brush ThumbnailExpanded
        {
            get { return _thumbnailExpanded; }
            private set
            {
                _thumbnailExpanded = value;
                OnPropertyChanged("ThumbnailExpanded");
            }
        }

        public double ThumbnailHeight
        {
            get
            {
                return _thumbnailHeight;
            }
            set
            {
                _thumbnailHeight = value;
                OnPropertyChanged("ThumbnailHeight");
            }
        }

        public double ThumbnailWidth
        {
            get
            {
                return _thumbnailWidth;
            }
            set
            {
                _thumbnailWidth = value;
                OnPropertyChanged("ThumbnailWidth");
            }
        }

        public string Title1
        {
            get { return _title1; }
            set
            {
                _title1 = value;
                OnPropertyChanged("Title1");
            }
        }

        public string TitleBig1
        {
            get { return _titleBig1; }
            set
            {
                _titleBig1 = value;
                OnPropertyChanged("TitleBig1");
            }
        }

        public string Title2
        {
            get { return _title2; }
            set
            {
                _title2 = value;
                OnPropertyChanged("Title2");
            }
        }

        public string Title2Trans
        {
            get { return _title2Trans; }
            set
            {
                _title2Trans = value;
                OnPropertyChanged("Title2Trans");
            }
        }

        public string Title3
        {
            get { return _title3; }
            set
            {
                _title3 = value;
                OnPropertyChanged("Title3");
            }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        public bool IsChanged
        {
            get { return _isChanged; }
            internal set
            {
                _isChanged = value;
                OnPropertyChanged("IsChanged");
            }
        }

        public bool Rejected
        {
            get { return _rejected; }
            set
            {
                _rejected = value;
                OnPropertyChanged("Rejected");
            }
        }

        public string FilePath { get; private set; }

        public byte[] Binary { get; private set; }

        public MainViewer MainViewer { get; private set; }

        public CanvasElement Image
        {
            get { return _itemCanvas; }
            private set
            {
                _itemCanvas = value;
                if (value != null)
                {
                    value.ContentChanged += ImageContentChanged;
                }

                OnPropertyChanged("Image");
            }
        }

        public BatchModel BatchData // Only has value if ItemType = Batch
        {
            get { return _batchData; }
            set
            {
                _batchData = value;
                OnPropertyChanged("BatchData");
            }
        }

        public ContentModel DocumentData
        {
            get { return _documentData; }
            set
            {
                _documentData = value;
                OnPropertyChanged("DocumentData");
            }
        }

        public PageModel PageData
        {
            get { return _pageData; }
            set
            {
                _pageData = value;
                OnPropertyChanged("PageData");
            }
        }

        public List<ContentItem> DeletedDocuments { get; private set; }

        public List<ContentItem> DeletedPages { get; private set; }

        public PermissionManager PermissionManager { get; set; }

        private void SetupTitles()
        {
            if (ItemType == ContentItemType.Batch)
            {
                Title1 = BatchData.BatchType.Name;
                Title2 = "Created on: " + BatchData.CreatedDate.ToString(Properties.Resources.LongDateTimeFormat);
                Title3 = "Created by: " + BatchData.CreatedBy;
            }
            else if (ItemType == ContentItemType.ContentModel)
            {
                Title1 = (Parent.Children.Where(p => p.ItemType == ContentItemType.ContentModel).ToList().IndexOf(this) + 1) +
                         ". " + DocumentData.DocumentType.Name;
                Title2 = "Pages: " + Children.Count;
            }
            else
            {
                TitleBig1 = (Parent.Children.IndexOf(this) + 1).ToString();
                if (PageData.FileType == FileTypeModel.Image && Image != null)
                {
                    Image.ShowPageText(Parent.Children.IndexOf(this), Parent.ItemType == ContentItemType.ContentModel ? Parent.Children.Count : 0);
                    Title2Trans = (Math.Round(Image.DpiX)).ToString() + " dpi";
                }
            }
        }

        private void Initialize()
        {
            if (ItemType == ContentItemType.Batch || ItemType == ContentItemType.ContentModel)
            {
                DeletedPages = new List<ContentItem>();
                Children = new ObservableCollection<ContentItem>();
                Children.CollectionChanged += ChildrenCollectionChanged;

                if (ItemType == ContentItemType.Batch)
                {
                    DeletedDocuments = new List<ContentItem>();
                    BatchData.FieldValues.ForEach(p =>
                    {
                        p.PropertyChanged += IndexPropertyChanged;
                    });

                    IsValid = !BatchData.FieldValues.Any(p => !p.IsValid) &&
                              Children.Count > 0 &&
                              !Children.Any(p => p.ItemType == ContentItemType.Page) &&
                              !Children.Any(p => p.ItemType == ContentItemType.ContentModel && p.DocumentData.FieldValues.Any(q => !q.IsValid));
                }
                else
                {
                    DocumentData.FieldValues.ForEach(p =>
                    {
                        p.PropertyChanged += IndexPropertyChanged;
                    });
                }
            }
        }

        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (ContentItem item in e.NewItems)
                {
                    item.Parent = this;
                    if (item.ItemType == ContentItemType.ContentModel)
                    {
                        item.IsValid = !item.DocumentData.FieldValues.Any(p => !p.IsValid);
                    }
                    else if (item.PageData.FileType == FileTypeModel.Image && item.Image != null)// && ViewerContainer != null)
                    {
                        item.Image.SetPermission(PermissionManager.GetContentViewerPermission(item));
                    }
                }
            }

            SetupTitles();
            foreach (ContentItem item in Children)
            {
                item.SetupTitles();
            }

            if (ItemType == ContentItemType.ContentModel && Children.Count > 0)
            {
                var fileTypes = Children.Select(p => p.PageData.FileType).Distinct().ToList();
                if (fileTypes.Count > 1)
                {
                    DocumentData.BinaryType = FileTypeModel.Compound;
                }
                else
                {
                    DocumentData.BinaryType = fileTypes[0];
                }
            }

            if (ItemType == ContentItemType.Batch)
            {
                List<ContentItem> allDocs = Children.Where(p => p.ItemType == ContentItemType.ContentModel).ToList();
                if (allDocs.Count > 0)
                {
                    IsValid = !BatchData.FieldValues.Any(p => !p.IsValid) && !allDocs.Any(p => !p.IsValid) && !Children.Any(p => p.ItemType == ContentItemType.Page);
                }
                else
                {
                    IsValid = false;
                }
            }
        }

        private void CreateThumbnail()
        {
            if (ItemType == ContentItemType.ContentModel)
            {
                ThumbnailCollasped = GetSysThumbnail("doc_collasped.png");
                ThumbnailExpanded = GetSysThumbnail("doc_expanded.png");
                ThumbnailWidth = 70;
                ThumbnailHeight = 80;
            }
            else if (ItemType == ContentItemType.Batch)
            {
                ThumbnailCollasped = GetSysThumbnail("batch_collasped.png");
                ThumbnailExpanded = GetSysThumbnail("batch_expanded.png");
                ThumbnailWidth = 100;
                ThumbnailHeight = 90;
            }
            else
            {
                CreatePageThumbnail();
            }
        }

        private void CreatePageThumbnail()
        {
            ThumbnailWidth = 70;
            ThumbnailHeight = 80;

            if (PageData.FileType == FileTypeModel.Image)
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    Image = new CanvasElement(Binary, PageData, PermissionManager.GetContentViewerPermission(this), MainViewer);
                }
                else
                {
                    Image = new CanvasElement(FilePath, PageData, PermissionManager.GetContentViewerPermission(this), MainViewer);
                }

                Image.ShowPageText(Parent.Children.IndexOf(this), Parent.Children.Count);
                CreateThumbnailForImagePage();
                return;
            }

            Image = new CanvasElement(new Uri("pack://application:,,,/ContentViewer;component/Resources/Images/unknow-page.png"), MainViewer) { IsNonImagePreview = true };
            if (PageData.FileType == FileTypeModel.Media)
            {
                ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("media.png");
            }
            else
            {
                switch (PageData.FileFormat)
                {
                    case FileFormatModel.Doc:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("word.png");
                        break;
                    case FileFormatModel.Pdf:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("pdf.png");
                        break;
                    case FileFormatModel.Xls:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("excel.png");
                        break;
                    case FileFormatModel.Html:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("html.png");
                        break;
                    case FileFormatModel.Ppt:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("ppt.png");
                        break;
                    case FileFormatModel.Txt:
                    case FileFormatModel.Log:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("text.png");
                        break;
                    case FileFormatModel.Xml:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("xml.png");
                        break;
                    case FileFormatModel.Zip:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("zip.png");
                        break;
                    case FileFormatModel.Xps:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("xps.png");
                        break;
                    case FileFormatModel.Rtf:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("rtf.png");
                        break;
                    case FileFormatModel.Unknown:
                        ThumbnailCollasped = ThumbnailExpanded = GetSysThumbnail("unknow.png");
                        break;
                }
            }
        }

        private Brush GetSysThumbnail(string fileName)
        {
            return new ImageBrush(BitmapFrame.Create(new Uri("pack://application:,,,/ContentViewer;component/Resources/Images/" + fileName)));
        }

        private string GetFileExtension(string fileName)
        {
            var lastDotIndex = fileName.LastIndexOf(".");
            return fileName.Substring(lastDotIndex + 1);
        }

        private void CreateThumbnailForImagePage()
        {
            var cloneItem = Image.Clone();
            // Want to make the view make sense
            if (Image.Width > Image.Height)
            {
                var tempWidth = ThumbnailWidth;
                ThumbnailWidth = ThumbnailHeight;
                ThumbnailHeight = tempWidth;
            }

            var sizeOfControl = new Size(cloneItem.Width, cloneItem.Height);
            cloneItem.Measure(sizeOfControl);
            cloneItem.Arrange(new Rect(sizeOfControl));
            if (ThumbnailCollasped != null)
            {
                var visualBrush = ThumbnailCollasped as VisualBrush;
                if (visualBrush != null)
                {
                    visualBrush.Visual = null;
                }
            }

            ThumbnailCollasped = new VisualBrush(cloneItem);
            ThumbnailExpanded = ThumbnailCollasped;
        }

        private void ImageContentChanged(object sender, EventArgs e)
        {
            CreateThumbnailForImagePage();
            Parent.ChangeType |= ChangeType.AnnotatePage;
        }

        private void IndexPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                if (ItemType == ContentItemType.ContentModel)
                {
                    IsValid = !DocumentData.FieldValues.Any(p => !p.IsValid);
                }
                else
                {
                    IsValid = !BatchData.FieldValues.Any(p => !p.IsValid);
                }
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == "IsValid")
            {
                if (ItemType == ContentItemType.ContentModel)
                {
                    List<ContentItem> allDocs = Parent.Children.Where(p => p.ItemType == ContentItemType.ContentModel).ToList();
                    if (allDocs.Count > 0)
                    {
                        Parent.IsValid = !Parent.BatchData.FieldValues.Any(p => !p.IsValid) && !allDocs.Any(p => !p.IsValid) && !Parent.Children.Any(p => p.ItemType == ContentItemType.Page);
                    }
                    else
                    {
                        Parent.IsValid = false;
                    }
                }
            }
            else if (propertyName == "ChangeType" && (ChangeType & ChangeType.RotatePage) == ChangeType.RotatePage)
            {
                CreateThumbnailForImagePage();
            }
        }
    }
}