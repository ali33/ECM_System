using System;
//using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ecm.Model;
using Ecm.Admin.ViewModel;
using System.Threading;
//using Ecm.Model.DataProvider;
//using Ecm.WorkflowDesigner;
using Button = System.Windows.Controls.Button;
using ListView = System.Windows.Controls.ListView;
//using Ecm.Domain;

namespace Ecm.Admin.View
{
    public partial class DocumentTypeView
    {
        public DocumentTypeView()
        {
            InitializeComponent();
            Loaded += DocumentTypeViewLoaded;
        }

        private void DocumentTypeViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (DocumentTypeViewModel)DataContext;
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DocType")
            {
                _viewModel.CreateEditedDocType();
                Dispatcher.BeginInvoke((ThreadStart)(() => txtDocTypeName.Focus()));
            }

            if (e.PropertyName == "EditDocType")
            {
                Dispatcher.BeginInvoke((ThreadStart)(() => txtDocTypeName.Focus()));
            }
        }

        private void LvlFieldSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var field = ((ListView)sender).SelectedItem as FieldMetaDataModel;
            if (field != null && _viewModel != null)
            {
                _viewModel.LoadFieldViewModel(field);
            }
        }

        private void LvlFieldMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var field = ((ListView)sender).SelectedItem as FieldMetaDataModel;

            if (field != null)
            {
                _viewModel.LoadFieldViewModel(field);
                _viewModel.EditFieldCommand.Execute(null);
            }
        }

        private void LvlFieldTargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateGridView(sender as ListView);
        }

        private void LvlFieldEndSortingProcess(object sender, EventArgs e)
        {
            LvlFieldTargetUpdated(sender, null);
        }

        private void BtnConfigureLookupClick(object sender, RoutedEventArgs e)
        {
            var field = ((FrameworkElement)sender).DataContext as FieldMetaDataModel;
            if (field != null)
            {
                _viewModel.LookupCommand.Execute(field);
            }
        }

        private void BtnDeleteLookupClick(object sender, RoutedEventArgs e)
        {
            var field = ((FrameworkElement)sender).DataContext as FieldMetaDataModel;
            if (field != null)
            {
                _viewModel.DeleteLookupCommand.Execute(field);
            }
        }

        private void UpdateGridView(ListView listView)
        {
            if (listView != null)
            {
                var gridView = listView.View as GridView;

                if(gridView == null || gridView.Columns.Count < 1)
                {
                    return;
                }

                foreach (GridViewColumn column in gridView.Columns)
                {
                    column.Width = 0;
                    column.Width = double.NaN;
                }
            }
        }

        private void ConfigureOCRButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.DocType = (DocumentTypeModel)((Button)sender).Tag;
            _viewModel.ConfigOcrTemplate();
        }

        private void DeleteOCRButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.DocType = (DocumentTypeModel)((Button)sender).Tag;
            _viewModel.DeleteOCRTemplate();
        }

        private void ConfigBarcodeButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.DocType = (DocumentTypeModel)((Button)sender).Tag;
            _viewModel.ConfigBarcode();
        }

        private void DeleteBarcodeButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.DocType = (DocumentTypeModel)((Button)sender).Tag;
            _viewModel.DeleteBarcode();
        }

        //private void ConfigWorkflowButtonClick(object sender, RoutedEventArgs e)
        //{
        //    _viewModel.DocType = (DocumentTypeModel)((Button)sender).Tag;

        //    DocumentType docType = ObjectMapper.GetDocumentType(_viewModel.DocType);
        //    WorkflowDefinition workflowDefinition = _viewModel.GetWorkflowDefinition(docType.Id);
        //    List<HumanStepPermission> permissions;
        //    if (workflowDefinition == null)
        //    {
        //        workflowDefinition = new WorkflowDefinition
        //                                 {
        //                                     OwnerId = docType.Id,
        //                                     Definition = string.Empty,
        //                                     WorkflowDefinitionID = Guid.Empty
        //                                 };

        //        permissions = new List<HumanStepPermission>();
        //    }
        //    else
        //    {
        //        permissions = _viewModel.GetHumanStepPermissions(workflowDefinition.WorkflowDefinitionID);
        //        if (permissions == null)
        //        {
        //            permissions = new List<HumanStepPermission>();
        //        }
        //    }

        //    DesignerContainer workflowDesigner = new DesignerContainer
        //                                             {
        //                                                 HandleExceptionAction = ProcessHelper.ProcessException,
        //                                                 LoginUser = ObjectMapper.GetUser(LoginViewModel.LoginUser),
        //                                                 UserGroups = _viewModel.GetUserGroups(),
        //                                                 DocumentType = docType,
        //                                                 HumanStepPermissions = permissions,
        //                                                 WorkflowDefinition = workflowDefinition
        //                                             };

        //    workflowDesigner.SaveWorkflow += WorkflowDesignerSaveWorkflow;

        //    DialogBaseView dialog = new DialogBaseView
        //                                {
        //                                    Text = "Workflow designer",
        //                                    Size = new System.Drawing.Size(1200, 700),
        //                                    EnableToResize = true,
        //                                    WpfContent = workflowDesigner
        //                                };
        //    dialog.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        //    dialog.ShowDialog();
        //}

        //private void WorkflowDesignerSaveWorkflow(long docTypeId, string workflowDefinition, List<HumanStepPermission> humanStepPermissions)
        //{
        //    Guid workflowDefinitionId = Guid.NewGuid();
        //    WorkflowDefinition wfDefinition = new WorkflowDefinition
        //                                          {
        //                                              WorkflowDefinitionID = workflowDefinitionId,
        //                                              OwnerId = docTypeId,
        //                                              Definition = workflowDefinition
        //                                          };

        //    _viewModel.SaveWorkflow(docTypeId, wfDefinition, humanStepPermissions);
        //    _viewModel.DocType.WorkflowDefinitionID = workflowDefinitionId;

        //}

        private DocumentTypeViewModel _viewModel;

        private void EditPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((ThreadStart)(() => txtDocTypeName.Focus()));
        }
    }
}
