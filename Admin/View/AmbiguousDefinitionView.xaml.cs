using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ecm.Admin.ViewModel;
using Ecm.Model;
using Ecm.CustomControl;
using System.Threading;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for e.xaml
    /// </summary>
    public partial class AmbiguousDefinitionView
    {
        public AmbiguousDefinitionView()
        {
            InitializeComponent();
            Loaded += AmbiguousDefinitionViewLoaded;
        }

        private void AmbiguousDefinitionViewLoaded(object sender, RoutedEventArgs e)
        {
            AmbiguousDefinitionViewModel viewModel = DataContext as AmbiguousDefinitionViewModel;
            viewModel.ResetListView = ResetListView;
            if (viewModel != null && viewModel.Languages != null)
            {
                viewModel.PropertyChanged += ViewModelPropertyChanged;
                if (viewModel.Languages.Count > 0)
                {
                    cboLanguage.SelectedIndex = 0;
                }
            }
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AmbiguousDefinition")
            {
                Dispatcher.BeginInvoke((ThreadStart)(() => txtText.Focus()));
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AmbiguousDefinitionViewModel viewModel = DataContext as AmbiguousDefinitionViewModel;
            LanguageModel lang = (sender as ComboBox).SelectedItem as LanguageModel;
            if (viewModel != null && lang != null)
            {

                viewModel.Language = lang;
                viewModel.SelectedAmbiguousDefinition = null;
                viewModel.LoadData(lang.Id);
            }
        }

        private void ResetListView()
        {
            lvAmbiguous.SelectedIndex = -1;
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((ThreadStart)(() => txtText.Focus()));
        }
    }
}
