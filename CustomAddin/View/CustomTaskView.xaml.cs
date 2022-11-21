using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ecm.CustomAddin.ViewModel;
using Microsoft.Office.Tools;

namespace Ecm.CustomAddin.View
{
    /// <summary>
    /// Interaction logic for CustomTaskView.xaml
    /// </summary>
    public partial class CustomTaskView : Window
    {
        public CustomTaskView(CustomTaskPaneCollection customTaskPanes)
        {
            InitializeComponent();
            DataContext = new CustomTaskViewModel(customTaskPanes, CloseView);
        }

        private void CloseView()
        {
            this.Close();
        }
    }
}
