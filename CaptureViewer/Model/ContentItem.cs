using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Ecm.AppHelper;
using Ecm.CaptureViewer.Controls;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using System.Windows.Media;
using Ecm.CaptureViewer.Converter;
using Ecm.CaptureModel;
using Ecm.Utility;
using Ecm.CaptureDomain;
using Ecm.CaptureViewer.Helper;
using System.Resources;
using System.Reflection;

namespace Ecm.CaptureViewer.Model
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
        private DocumentModel _documentData;
        private PageModel _pageData;
        private CanvasElement _itemCanvas;
        private ChangeType _changeType;
        private Brush _thumbnailCollasped;
        private Brush _thumbnailExpanded;
        private bool _isLoaded;
        private bool _isChanged;
        private bool _isVisible = true;

        public ContentItem(BatchModel batch)
        {
            BatchData = batch;
            Rejected = batch.IsRejected;
            ItemType = ContentItemType.Batch;
            Initialize();
            SetupTitles();
        }

        public ContentItem(DocumentModel document)
        {
            DocumentData = document;
            Rejected = document.IsRejected;
            ItemType = ContentItemType.Document;
            Initialize();
        }

        public ContentItem(string filePath)
        {
            ItemType = ContentItemType.Page;
            FilePath = filePath;
            PageData = new PageModel { FileExtension = GetFileExtension(FilePath) };
            PageData.FileFormat = (FileFormatModel)new FileFormatConverter().Convert(PageData.FileExtension, null, null, null);
            PageData.FileType = (FileTypeModel)new FileTypeConverter().Convert(PageData.FileExtension, null, null, null);
            PageData.OriginalFileName = new FileInfo(filePath).Name;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                PageData.FileBinary = fs.ReadAllBytes();
                fs.Close();
            }
        }

        public ContentItem(PageModel page, byte[] binary)
        {
            ItemType = ContentItemType.Page;
            PageData = page;
            Binary = binary;
            PageData.FileFormat = (FileFormatModel)new FileFormatConverter().Convert(PageData.FileExtension, null, null, null);
            PageData.FileType = (FileTypeModel)new FileTypeConverter().Convert(PageData.FileExtension, null, null, null);
            Rejected = page.IsRejected;
        }

        public ContentItem(PageModel page, string filePath)
        {
            ItemType = ContentItemType.Page;
            FilePath = filePath;
            PageData = page ?? new PageModel();
            Rejected = page.IsRejected;
        }

        public void SetParent(ContentItem parent)
        {
            Parent = parent;
        }

        public void Load(ViewerContainer viewerContainer)
        {
            if (!_isLoaded)
            {
                ViewerContainer = viewerContainer;
                if (Image == null)
                {
                    CreateThumbnail();
                }

                if (FilePath == null && Binary != null && PageData.FileType != FileTypeModel.Image)
                {
                    FilePath = ViewerContainer.WorkingFolder.Save(Binary, "native_" + Guid.NewGuid().GetHashCode().ToString() + "." + PageData.FileExtension);
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

        public void SetDocumentData(DocumentModel document)
        {
            if (ItemType == ContentItemType.Document)
            {
                DocumentData = document;
                DocumentData.FieldValues.ForEach(p =>
                {
                    p.PropertyChanged += IndexPropertyChanged;
                });

                IsValid = !DocumentData.FieldValues.Any(p => !p.IsValid);
                SetupTitles();

                ChangeType |= CaptureDomain.ChangeType.ChangeDocumentType;
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

                IsValid = !BatchData.FieldValues.Any(p => !p.IsValid);
                SetupTitles();
                ChangeType |= ChangeType.ChangeBatchType;

                List<ContentItem> items = Children.Where(p => p.ItemType == ContentItemType.Document).ToList();
                foreach (var contentItem in items)
                {
                    //var docProfile = BatchData.BatchType.Document.First();
                    DocumentModel document = new DocumentModel(DateTime.Now, ViewerContainer.UserName, BatchData.BatchType.DocTypes[0]);
                    contentItem.SetDocumentData(document);
                }
            }
        }

        public byte[] GetBinary()
        {
            if (PageData != null)
            {
                if (PageData.FileType == CaptureModel.FileTypeModel.Image && Image.IsChanged)
                {
                    var clonedItem = Image.Clone();
                    //clonedItem.IsInitialized.Clone();//.InitializeContent(false);
                    byte[] binary = FileHelper.CreateOnePageTiff(clonedItem);
                    return binary;
                }

                if (!string.IsNullOrEmpty(FilePath))
                {
                    using (var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return stream.ReadAllBytes();
                    }
                }

                return Binary;
            }

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
                if (ItemType == ContentItemType.Document)
                {
                    return Parent;
                }

                if (ItemType == ContentItemType.Batch)
                {
                    return this;
                }

                if (Parent.ItemType == ContentItemType.Document)
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

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
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

        public ViewerContainer ViewerContainer { get; private set; }

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

        public DocumentModel DocumentData
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

        private void SetupTitles()
        {
            ResourceManager _resource = new ResourceManager("Ecm.CaptureViewer.ViewerContainer", Assembly.GetExecutingAssembly());
            if (ItemType == ContentItemType.Batch)
            {
                // HungLe - 2014/07/18 - Editing show batch name instead of show batch type name in treeview - Start
                //Title1 = BatchData.BatchType.Name;
                if (!string.IsNullOrWhiteSpace(BatchData.BatchName))
                {
                    Title1 = BatchData.BatchName;
                }
                else
                {
                    Title1 = BatchData.BatchType.Name;
                }
                // HungLe - 2014/07/18 - Editing show batch name instead of show batch type name in treeview - End

                Title2 = _resource.GetString("uiCreatedOn") + BatchData.CreatedDate.ToString(Properties.Resources.LongDateTimeFormat);
                Title3 = _resource.GetString("uiCreatedBy") + BatchData.CreatedBy;
            }
            else if (ItemType == ContentItemType.Document)
            {
                // HungLe - 2014/07/18 - Editing show doc name if it exist in treeview - Start
                //Title1 = (Parent.Children.Where(p => p.ItemType == ContentItemType.Document).ToList().IndexOf(this) + 1) +
                //            ". " + DocumentData.DocumentType.Name;
                if (!string.IsNullOrWhiteSpace(DocumentData.DocName))
                {
                    Title1 = (Parent.Children.Where(p => p.ItemType == ContentItemType.Document).ToList().IndexOf(this) + 1) +
                             ". " + DocumentData.DocName;
                }
                else
                {
                    Title1 = (Parent.Children.Where(p => p.ItemType == ContentItemType.Document).ToList().IndexOf(this) + 1) +
                        ". " + (DocumentData.DocumentType == null ? string.Empty : DocumentData.DocumentType.Name);
                }
                // HungLe - 2014/07/18 - Editing show doc name if it exist in treeview - End

                Title2 = _resource.GetString("uiPages") + Children.Count;
            }
            else
            {
                TitleBig1 = (Parent.Children.IndexOf(this) + 1).ToString();
                if (PageData.FileType == FileTypeModel.Image && Image != null)
                {
                    Image.ShowPageText(Parent.Children.IndexOf(this), Parent.ItemType == ContentItemType.Document ? Parent.Children.Count : 0);
                    Title2Trans = (Math.Round(Image.DpiX)).ToString() + " dpi";
                }
            }
        }

        private void Initialize()
        {
            if (ItemType == ContentItemType.Batch || ItemType == ContentItemType.Document)
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

                    //foreach (ContentItem docData in BatchData)
                    //{ 
                        
                    //}

                    IsValid = !BatchData.FieldValues.Any(p => !p.IsValid) &&
                              Children.Count > 0 &&
                              !Children.Any(p => p.ItemType == ContentItemType.Page) &&
                              !Children.Any(p => p.ItemType == ContentItemType.Document && p.DocumentData.FieldValues.Any(q => !q.IsValid));
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
            if (ViewerContainer != null)
            {
                ViewerContainer.IsChanged = true;
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (ContentItem item in e.NewItems)
                {
                    item.Parent = this;

                    if (item.ItemType == ContentItemType.Document)
                    {
                        if (item.DocumentData.DocTypeId != Guid.Empty)
                        {
                            item.IsValid = !item.DocumentData.FieldValues.Any(p => !p.IsValid);
                        }
                        else
                        {
                            item.IsValid = true;
                        }

                        if (item.Parent.Children.Any(p => p.Rejected))
                        {
                            item.Parent.Rejected = true;
                        }
                        //item.DocumentData.IsRejected = false;
                    }
                    else if (item.ItemType == ContentItemType.Page && item.Parent.ItemType == ContentItemType.Batch)
                    {
                        item.IsValid = item.BatchItem.BatchData.Permission.CanReleaseLoosePage;
                    }
                    else if (item.ItemType == ContentItemType.Page)
                    {
                        if (item.PageData.FileType == FileTypeModel.Image && item.Image != null && ViewerContainer != null)
                        {
                            item.Image.SetPermission(ViewerContainer.PermissionManager.GetContentViewerPermission(item));
                        }

                        if (item.Parent.Children.Any(p => p.Rejected))
                        {
                            item.Parent.Rejected = true;
                            item.Parent.DocumentData.IsRejected = true;

                            item.Parent.Parent.Rejected = true;
                            item.Parent.Parent.BatchData.IsRejected = true;
                        }
                        //item.PageData.IsRejected = false;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (ItemType == ContentItemType.Document)
                {
                    if (!Children.Any(p => p.Rejected))
                    {
                        DocumentData.IsRejected = false;
                        Rejected = false;

                        if (!Parent.Children.Any(q => q.Rejected))
                        {
                            Parent.BatchData.IsRejected = false;
                            Parent.Rejected = false;
                        }
                    }
                }
                else if (ItemType == ContentItemType.Batch)
                {
                    if (!Children.Any(p => p.Rejected))
                    {
                        Rejected = false;
                        BatchData.IsRejected = false;
                    }
                }

            }

            SetupTitles();
            foreach (ContentItem item in Children)
            {
                item.SetupTitles();
            }

            if (ItemType == ContentItemType.Document && Children.Count > 0)
            {
                List<FileTypeModel> fileTypes = Children.Select(p => p.PageData.FileType).Distinct().ToList();
                if (fileTypes.Count > 1)
                {
                    DocumentData.BinaryType = FileTypeModel.Compound;
                }
                else
                {
                    DocumentData.BinaryType = fileTypes.FirstOrDefault();
                }
            }

            if (ItemType == ContentItemType.Batch)
            {
                List<ContentItem> allDocs = Children.Where(p => p.ItemType == ContentItemType.Document).ToList();
                List<ContentItem> allLoosePages = Children.Where(p => p.ItemType == ContentItemType.Page).ToList();
                IsValid = !BatchData.FieldValues.Any(p => !p.IsValid) && (allLoosePages.Count > 0 && !allLoosePages.Any(p => !p.IsValid) || (allDocs.Count > 0 && !allDocs.Any(p => !p.IsValid)));
            }
        }

        private void CreateThumbnail()
        {
            if (ItemType == ContentItemType.Document)
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
            try{
            ThumbnailWidth = 70;
            ThumbnailHeight = 80;

            if (PageData.FileType == FileTypeModel.Image)
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    Image = new CanvasElement(Binary, PageData, ViewerContainer.PermissionManager.GetContentViewerPermission(this), ViewerContainer);
                }
                else
                {
                    Image = new CanvasElement(FilePath, PageData, ViewerContainer.PermissionManager.GetContentViewerPermission(this), ViewerContainer);
                }

                Image.ShowPageText(Parent.Children.IndexOf(this), Parent.Children.Count);
                CreateThumbnailForImagePage();
                return;
            }

            Image = new CanvasElement(new Uri("pack://application:,,,/CaptureViewer;component/Resources/unknow-page.png"), ViewerContainer) { IsNonImagePreview = true };
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
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }

        }

        private Brush GetSysThumbnail(string fileName)
        {
            return new ImageBrush(BitmapFrame.Create(new Uri("pack://application:,,,/CaptureViewer;component/Resources/" + fileName)));
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
                if (ItemType == ContentItemType.Document)
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
                if (ItemType == ContentItemType.Document)
                {
                    List<ContentItem> allDocs = Parent.Children.Where(p => p.ItemType == ContentItemType.Document).ToList();
                    if (allDocs.Count > 0)
                    {
                        Parent.IsValid = !Parent.BatchData.FieldValues.Any(p => !p.IsValid) && !allDocs.Any(p => !p.IsValid);
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
            //else if (propertyName == "Rejected")
            //{
            //    if (Rejected)
            //    {
            //        IsValid = !Rejected;
            //    }
            //    else
            //    {
            //        if (ItemType == ContentItemType.Document)
            //        {
            //            List<ContentItem> allDocs = Parent.Children.Where(p => p.ItemType == ContentItemType.Document).ToList();
            //            if (allDocs.Count > 0)
            //            {
            //                IsValid = !Parent.BatchData.FieldValues.Any(p => !p.IsValid) && !allDocs.Any(p => p.DocumentData.FieldValues.Any(q=> !q.IsValid));
            //            }
            //            else
            //            {
            //                IsValid = false;
            //            }
            //        }
            //    }
            //}
        }


    }
}