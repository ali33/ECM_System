using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using Ecm.CaptureDomain;
using System.Windows.Input;

namespace Ecm.CaptureViewer
{
    public partial class CommentView
    {
        private ContentItem _currentBatch;

        public RoutedCommand SaveCommand;

        public static readonly DependencyProperty CommentsProperty =
            DependencyProperty.Register("Comments", typeof(ObservableCollection<CommentModel>), typeof(CommentView),
                new FrameworkPropertyMetadata(new ObservableCollection<CommentModel>()));

        public static readonly DependencyProperty AllowAddCommentProperty =
            DependencyProperty.Register("AllowAddComment", typeof(bool), typeof(CommentView));

        public static readonly DependencyProperty HasMessageProperty =
            DependencyProperty.Register("HasMessage", typeof(bool), typeof(CommentView));

        public static readonly DependencyProperty SortByDescProperty =
            DependencyProperty.Register("SortByDesc", typeof(bool), typeof(CommentView),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty TotalMessageProperty =
            DependencyProperty.Register("TotalMessage", typeof(int), typeof(CommentView));

        public static readonly DependencyProperty RemainCharsProperty =
            DependencyProperty.Register("RemainChars", typeof(int), typeof(CommentView));

        public static readonly DependencyProperty NewMessageProperty =
            DependencyProperty.Register("NewMessage", typeof(string), typeof(CommentView),
               new FrameworkPropertyMetadata(string.Empty, NewMessageChangedCallback));

        private static void NewMessageChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commentView = d as CommentView;
            if (commentView != null)
            {
                commentView.RemainChars = commentView.customText.MaxTextLength - (string.IsNullOrEmpty(e.NewValue + string.Empty) ? 0 : commentView.GetXamlTextLength(e.NewValue + string.Empty));
                //commentView.btnSaveButton.IsEnabled = commentView.RemainChars < commentView.customText.MaxTextLength;
            }
        }

        public CommentView()
        {
            InitializeComponent();
            RemainChars = customText.MaxTextLength;
            InitCommand();
            SortMessages();
        }

        public void InitCommand()
        {
            SaveCommand = new RoutedCommand("Save", typeof(CommentView));
            var commandBinding = new CommandBinding(SaveCommand, Save, CanSave);
            this.CommandBindings.Add(commandBinding);

            btnSaveButton.Command = SaveCommand;
        }

        public ObservableCollection<CommentModel> Comments
        {
            get { return GetValue(CommentsProperty) as ObservableCollection<CommentModel>; }
            set 
            { 
                SetValue(CommentsProperty, value);
                if (value != null)
                {
                    TotalMessage = value.Count;
                    HasMessage = TotalMessage > 0;
                }
                else
                {
                    TotalMessage = 0;
                    HasMessage = false;
                }
            }
        }

        public bool AllowAddComment
        {
            get { return (bool)GetValue(AllowAddCommentProperty); }
            set { SetValue(AllowAddCommentProperty, value); }
        }

        public int TotalMessage
        {
            get { return (int)GetValue(TotalMessageProperty); }
            private set { SetValue(TotalMessageProperty, value); }
        }

        public string NewMessage
        {
            get { return GetValue(NewMessageProperty) as string; }
            set { SetValue(NewMessageProperty, value); }
        }

        public int RemainChars
        {
            get { return (int)GetValue(RemainCharsProperty); }
            set { SetValue(RemainCharsProperty, value); }
        }

        public bool HasMessage
        {
            get { return (bool)GetValue(HasMessageProperty); }
            set { SetValue(HasMessageProperty, value); }
        }

        public bool SortByDesc
        {
            get { return (bool)GetValue(SortByDescProperty); }
            set { SetValue(SortByDescProperty, value); }
        }

        public ContentItem CurrentBatch
        {
            get 
            {
                if (ViewerContainer.ThumbnailSelector.Cursor != null)
                {
                    ContentItem item = ViewerContainer.ThumbnailSelector.Cursor;
                    if (item.ItemType == ContentItemType.Batch)
                    {
                        return item;
                    }
                    else if (item.ItemType == ContentItemType.Document)
                    {
                        return item.Parent;
                    }
                    else
                    {
                        return item.Parent.Parent;
                    }
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _currentBatch = value;
                SortMessages();
            }
        }

        public ViewerContainer ViewerContainer { get; set; }

        private int GetXamlTextLength(string xamlString)
        {
            var textBox = new RichTextBox();
            var range = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
            byte[] byteArray = Encoding.UTF8.GetBytes(xamlString);
            var stream = new MemoryStream(byteArray);

            range.Load(stream, DataFormats.Xaml);
            string regularText = range.Text.Replace(Environment.NewLine, string.Empty);
            return regularText.Length;
        }

        public void SortMessages()
        {
            if (SortByDesc)
            {
                Comments = new ObservableCollection<CommentModel>(Comments.OrderByDescending(p => p.CreatedDate));
                //SortButton.Content = "Newest on top";
            }
            else
            {
                Comments = new ObservableCollection<CommentModel>(Comments.OrderBy(p => p.CreatedDate));
                //SortButton.Content = "Oldest on top";
            }
        }

        private void SortButtonClick(object sender, RoutedEventArgs e)
        {
            if (Comments != null)
            {
                SortByDesc = !SortByDesc;
                SortMessages();
            }

        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            if (Comments == null)
            {
                Comments = new ObservableCollection<CommentModel>();
            }

            var newComment = new CommentModel { CreatedDate = DateTime.Now, Note = NewMessage, CreatedBy = ViewerContainer.UserName };
            CurrentBatch.BatchData.Comments.Add(newComment);
            Comments.Add(newComment);
            CurrentBatch.ChangeType |= ChangeType.AddComment;
            NewMessage = string.Empty;
            SortMessages();
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrWhiteSpace(NewMessage) && this.RemainChars < this.customText.MaxTextLength;
        }
    }
}
