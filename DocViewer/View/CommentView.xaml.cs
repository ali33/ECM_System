using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;

namespace Ecm.DocViewer
{
    public partial class CommentView
    {
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

        public event EventHandler ExpandProcess;

        private static void NewMessageChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commentView = d as CommentView;
            if (commentView != null)
            {
                commentView.RemainChars = commentView.customText.MaxTextLength - (string.IsNullOrEmpty(e.NewValue + string.Empty) ? 0 : commentView.GetXamlTextLength(e.NewValue + string.Empty));
                commentView.SaveButton.IsEnabled = commentView.RemainChars < commentView.customText.MaxTextLength;
            }
        }

        public CommentView()
        {
            InitializeComponent();
            RemainChars = customText.MaxTextLength;
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

        public ContentItem Document
        {
            get { return _document; }
            set
            {
                _document = value;
                Comments = new ObservableCollection<CommentModel>(_document.DocumentData.Comments);
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

        private void SortMessages()
        {
            if (SortByDesc)
            {
                Comments = new ObservableCollection<CommentModel>(Comments.OrderByDescending(p => p.CreateDate));
                SortButton.Content = "Newest on top";
            }
            else
            {
                Comments = new ObservableCollection<CommentModel>(Comments.OrderBy(p => p.CreateDate));
                SortButton.Content = "Oldest on top";
            }
        }

        private void SortButtonClick(object sender, RoutedEventArgs e)
        {
            if (Comments != null)
            {
                SortByDesc = !SortByDesc;
                SortMessages();
            }

            if (ExpanderToggleButton.IsChecked != null && !ExpanderToggleButton.IsChecked.Value)
            {
                ExpanderToggleButton.IsChecked = true;

                if (ExpandProcess!=null)
                {
                    ExpandProcess(null, null);
                }
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (Comments == null)
            {
                Comments = new ObservableCollection<CommentModel>();
            }

            var newComment = new CommentModel { CreateDate = DateTime.Now, Message = NewMessage, UserName = ViewerContainer.UserName };
            Document.DocumentData.Comments.Add(newComment);
            Document.ChangeType |= ChangeType.AddComment;
            NewMessage = string.Empty;
            SortMessages();
        }

        private ContentItem _document;
    }
}
