using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ecm.Model;
using Ecm.Audit.ViewModel;
using System;

namespace Ecm.Audit.View
{
    /// <summary>
    /// Interaction logic for HistoryDetailView.xaml
    /// </summary>
    public partial class HistoryDetailView
    {
        public HistoryDetailView()
        {
            InitializeComponent();
            Loaded+=HistoryDetailView_Loaded;
        }

        void HistoryDetailView_Loaded(object sender, RoutedEventArgs e)
        {
            docRow.Width = new GridLength(75, GridUnitType.Star);
            expanderRow.Width = new GridLength(25, GridUnitType.Star);
        }

        private void lvDocVersion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var documentVersion = (sender as ListBox).SelectedItem as DocumentVersionModel;
            HistoryDetailViewModel viewModel = DataContext as HistoryDetailViewModel;

            if (viewModel != null && documentVersion != null)
            {
                viewModel.OpenDocumentVersion(documentVersion.VersionId);
            }

        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            docRow.Width = new GridLength(75, GridUnitType.Star);
            expanderRow.Width = new GridLength(25, GridUnitType.Star);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            docRow.Width = new GridLength(1, GridUnitType.Star);
            expanderRow.Width = new GridLength(30);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 22)
            {
                expanderRow.Width = new GridLength(22);
                pnProperties.Visibility = System.Windows.Visibility.Collapsed;
                pnProperties.Visibility = Visibility.Collapsed;
                pnRightHeader.Visibility = Visibility.Collapsed;
                txtPropertyHeader.Visibility = System.Windows.Visibility.Visible;
                btnExpandRight.IsChecked = true;
            }
            else
            {
                pnProperties.Visibility = Visibility.Visible;
                pnRightHeader.Visibility = Visibility.Visible;
                btnExpandRight.IsChecked = false;
                txtPropertyHeader.Visibility = System.Windows.Visibility.Hidden;
                pnRight.Width = Double.NaN;
            }
        }

        private void BtnExpandRightClick(object sender, RoutedEventArgs e)
        {
            if (btnExpandRight.IsChecked.Value)
            {
                pnProperties.Visibility = Visibility.Hidden;
                pnRightHeader.Visibility = Visibility.Hidden;
                txtPropertyHeader.Visibility = System.Windows.Visibility.Visible;
                expanderRow.Width = new GridLength(22);
            }
            else
            {
                pnProperties.Visibility = Visibility.Visible;
                txtPropertyHeader.Visibility = System.Windows.Visibility.Hidden;
                pnRightHeader.Visibility = Visibility.Visible;
                docRow.Width = new GridLength(75, GridUnitType.Star);
                expanderRow.Width = new GridLength(25, GridUnitType.Star);
                pnRight.Width = Double.NaN;
            }

        }

    }
}
