using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using ExcelImport.Model;
using System.Collections.ObjectModel;
using Ecm.CustomAddin.ViewModel;
using Microsoft.Office.Tools;
using System.Windows.Input;

namespace Ecm.CustomAddin.ViewModel
{
    public class CustomTaskViewModel : ComponentViewModel
    {
        private ObservableCollection<CustomTask> _customTasks = new ObservableCollection<CustomTask>();
        private CustomTaskPaneCollection _customTaskPanes;
        private RelayCommand _okCommand;
        private Action _closeView;

        public CustomTaskViewModel(CustomTaskPaneCollection customTaskPanes, Action closeView)
        {
            _customTaskPanes = customTaskPanes;
            LoadData(customTaskPanes);
            _closeView = closeView;
        }

        public ObservableCollection<CustomTask> CustomTasks
        {
            get { return _customTasks; }
            set
            {
                _customTasks = value;
                OnPropertyChanged("CustomTasks");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => DisplayCustomView(), p => CanDisplayCustomView());
                }
                return _okCommand;
            }
        }

        private bool CanDisplayCustomView()
        {
            return CustomTasks.Count > 0;
        }

        private void DisplayCustomView()
        {
            foreach (CustomTask cusTask in CustomTasks)
            {
                CustomTaskPane pane = _customTaskPanes.SingleOrDefault(p => p.Control.Name == cusTask.ControlName);
                pane.Visible = cusTask.IsChecked;
            }

            if (_closeView != null)
            {
                _closeView();
            }
        }

        private void LoadData(CustomTaskPaneCollection customTaskPanes)
        {
            CustomTasks.Clear();
            foreach (CustomTaskPane task in customTaskPanes)
            {
                CustomTask cusTask = new CustomTask { CustomTaskTitle = task.Title, IsChecked = task.Visible, ControlName = task.Control.Name };
                CustomTasks.Add(cusTask);
            }
        }
    }
}
