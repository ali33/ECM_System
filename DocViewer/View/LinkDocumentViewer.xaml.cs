using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using Ecm.DocViewer.Model;
using Ecm.Model;
using System.Windows.Input;
using System;

namespace Ecm.DocViewer
{
    /// <summary>
    /// Interaction logic for LinkDocumentView.xaml
    /// </summary>
    public partial class LinkDocumentViewer
    {
        public static readonly DependencyProperty RemoveLinkCommandProperty =
            DependencyProperty.Register("RemoveLinkCommand", typeof(RoutedCommand), typeof(LinkDocumentViewer), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ViewLinkCommandProperty =
            DependencyProperty.Register("ViewLinkCommand", typeof(RoutedCommand), typeof(LinkDocumentViewer), new UIPropertyMetadata(null));
        public RoutedCommand RemoveLinkCommand
        {
            get { return (RoutedCommand)GetValue(RemoveLinkCommandProperty); }
            set { SetValue(RemoveLinkCommandProperty, value); }
        }

        public RoutedCommand ViewLinkCommand
        {
            get { return (RoutedCommand)GetValue(ViewLinkCommandProperty); }
            set { SetValue(ViewLinkCommandProperty, value); }
        }

        public static readonly DependencyProperty LinkDocumentsProperty =
            DependencyProperty.Register("LinkDocuments", typeof(ObservableCollection<LinkDocumentModel>), typeof(LinkDocumentViewer));

        public static readonly DependencyProperty LinkDocumentProperty =
            DependencyProperty.Register("LinkDocument", typeof(LinkDocumentModel), typeof(LinkDocumentViewer));

        public ObservableCollection<LinkDocumentModel> LinkDocuments
        {
            get { return GetValue(LinkDocumentsProperty) as ObservableCollection<LinkDocumentModel>; }
            set { SetValue(LinkDocumentsProperty, value); }
        }

        public LinkDocumentModel LinkDocument
        {
            get { return GetValue(LinkDocumentProperty) as LinkDocumentModel; }
            set { SetValue(LinkDocumentProperty, value); }
        }

        public ViewerContainer ViewerContainer { get; set; }

        public LinkDocumentViewer()
        {
            InitializeComponent();
            RemoveLinkCommand = new RoutedCommand("RemoveLinkCommand", typeof(SearchLinkDocumentView)); //new RoutedCommand("",);
            var commandBinding = new CommandBinding(RemoveLinkCommand, RemoveLink);
            this.CommandBindings.Add(commandBinding);

            ViewLinkCommand = new RoutedCommand("ViewLinkCommand", typeof(SearchLinkDocumentView)); //new RoutedCommand("",);
            commandBinding = new CommandBinding(ViewLinkCommand, ViewLink);
            this.CommandBindings.Add(commandBinding);

            Loaded += LinkDocumentViewer_Loaded;
        }

        private void ViewLink(object sender, ExecutedRoutedEventArgs e)
        {
            LinkDocumentModel linkDoc = e.Parameter as LinkDocumentModel;
            ViewerContainer.OpenLinkDocument(linkDoc.LinkedDocumentId);
        }


        private void RemoveLink(object sender, ExecutedRoutedEventArgs e)
        {
            DocumentModel document = ViewerContainer.Items[0].BatchItem.Children.SingleOrDefault(p => p.DocumentData.Id == ViewerContainer.SelectedDocument.Id).DocumentData;
            if (document != null)
            {
                LinkDocumentModel linkDoc = e.Parameter as LinkDocumentModel;
                document.LinkDocuments.Remove(linkDoc);
                document.DeletedLinkDocuments.Add(linkDoc.Id);
                ViewerContainer.LinkDocViewer.LinkDocuments.Remove(linkDoc);
            }

        }

        private void LinkDocumentViewer_Loaded(object sender, RoutedEventArgs e)
        {
            btnAddDoc.Command = ViewerContainer.ThumbnailCommandManager.OpenLinkDocumentCommand;
            //LinkDocuments = ViewerContainer.LinkDocuments;
        }

    }
}
