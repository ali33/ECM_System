using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using Ecm.CaptureAdmin.ViewModel;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.WorkflowDesigner;
using ListView = System.Windows.Controls.ListView;
using System.Resources;
using System.Reflection;
using Ecm.WorkflowDesigner.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    public partial class BatchTypeView
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.BarcodeConfigurationView", Assembly.GetExecutingAssembly());
        private BatchTypeViewModel _viewModel;

        public BatchTypeView()
        {
            InitializeComponent();
            Loaded += DocumentTypeViewLoaded;
        }

        private void DocumentTypeViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (BatchTypeViewModel)DataContext;
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "BatchType")
                {
                    _viewModel.CreateEditedBatchType();
                    Dispatcher.BeginInvoke((ThreadStart) (() => txtBatchTypeName.Focus()));
                }
                else if (e.PropertyName == "EditBatchType")
                {
                    Dispatcher.BeginInvoke((ThreadStart) (() => txtBatchTypeName.Focus()));
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LvlFieldMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var field = ((ListView) sender).SelectedItem as FieldModel;

                if (field != null && _viewModel.EditBatchFieldCommand.CanExecute(null))
                {
                    _viewModel.EditBatchFieldCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LvlDocTypeMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var docType = ((ListView) sender).SelectedItem as DocTypeModel;

                if (docType != null)
                {
                    _viewModel.EditDocTypeCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        //private void ConfigWorkflowButtonClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        _viewModel.BatchType = (BatchTypeModel) ((Button) sender).Tag;

        //        BatchType batchType = ObjectMapper.GetBatchType(_viewModel.BatchType);
        //        List<DocumentType> documentTypes = ObjectMapper.GetDocTypes(_viewModel.DocTypes);
        //        WorkflowDefinition workflowDefinition = _viewModel.GetWorkflowDefinition(batchType.Id);
        //        List<HumanStepPermission> permissions;
        //        List<CustomActivitySetting> customActivitySettings;

        //        if (workflowDefinition == null)
        //        {
        //            workflowDefinition = new WorkflowDefinition
        //                                     {
        //                                         BatchTypeId = batchType.Id,
        //                                         DefinitionXML = string.Empty,
        //                                         Id = Guid.Empty
        //                                     };

        //            permissions = new List<HumanStepPermission>();
        //            customActivitySettings = new List<CustomActivitySetting>();
        //        }
        //        else
        //        {
        //            permissions = _viewModel.GetHumanStepPermissions(workflowDefinition.Id);
        //            customActivitySettings = _viewModel.GetCustomActivitySettings(workflowDefinition.Id);

        //            if (permissions == null)
        //            {
        //                permissions = new List<HumanStepPermission>();
        //            }

        //            if (customActivitySettings == null)
        //            {
        //                customActivitySettings = new List<CustomActivitySetting>();
        //            }
        //        }

        //        DesignerContainerViewModel viewModel = new DesignerContainerViewModel(workflowDefinition.DefinitionXML)
        //        {
        //            HandleExceptionAction = ProcessHelper.ProcessException,
        //            LoginUser = ObjectMapper.GetUser(LoginViewModel.LoginUser),
        //            UserGroups = _viewModel.GetUserGroups(),
        //            DocTypes = documentTypes,
        //            BatchType = batchType,
        //            HumanStepPermissions = permissions,
        //            CustomActivitySettings = customActivitySettings
        //        };

        //        DesignerContainer workflowDesigner = new DesignerContainer(viewModel);
        //        viewModel.SaveWorkflow += WorkflowDesignerSaveWorkflow;
        //        viewModel.LoadWorkflow();

        //        DialogBaseView dialog = new DialogBaseView
        //                                    {
        //                                        Text = "Workflow designer",
        //                                        Size = new System.Drawing.Size(1200, 700),
        //                                        EnableToResize = true,
        //                                        WpfContent = workflowDesigner,
        //                                        WindowState = System.Windows.Forms.FormWindowState.Maximized
        //                                    };

        //        dialog.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void WorkflowDesignerSaveWorkflow(Guid batchTypeId, string workflowDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings)
        //{
        //    try
        //    {
        //        WorkflowDefinition wfDefinition = new WorkflowDefinition
        //                                              {
        //                                                  BatchTypeId = batchTypeId,
        //                                                  DefinitionXML = workflowDefinition
        //                                              };

        //        Guid workflowDefinitionId = _viewModel.SaveWorkflow(batchTypeId, wfDefinition, humanStepPermissions, customActivitySettings);
        //        _viewModel.BatchType.WorkflowDefinitionId = workflowDefinitionId;
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void BtnConfigureOcrClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var docType = ((FrameworkElement) sender).DataContext as DocTypeModel;
        //        if (docType != null)
        //        {
        //            ConfigOCRTemplateViewModel ocrTemplateViewModel = new ConfigOCRTemplateViewModel(
        //                OCRTemplateMapper.GetDocumentTypeWithOcrTemplate(docType), OCRTemplateMapper.GetLanguages(_viewModel.GetLanguages()));
        //            ocrTemplateViewModel.SaveOcrTemplate += OcrTemplateViewModel_SaveOcrTemplate;

        //            ConfigOCRTemplateView view = new ConfigOCRTemplateView
        //                                             {
        //                                                 DataContext = ocrTemplateViewModel
        //                                             };

        //            DialogBaseView dialog = new DialogBaseView
        //                                        {
        //                                            Text = "Configure OCR template",
        //                                            Size = new System.Drawing.Size(900, 700),
        //                                            EnableToResize = true,
        //                                            WpfContent = view,
        //                                            WindowState = System.Windows.Forms.FormWindowState.Maximized
        //                                        };

        //            dialog.ShowDialog();
        //            _viewModel.Initialize();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void OcrTemplateViewModel_SaveOcrTemplate(Guid docTypeId, OCRTemplateModel ocrTemplate)
        //{
        //    DocTypeModel docType = _viewModel.DocTypes.FirstOrDefault(p => p.Id == docTypeId);
        //    if (docType != null)
        //    {
        //        _viewModel.SaveOcrTemplate(docType, ocrTemplate);
        //    }
        //}

        //private void BtnDeleteOcrClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var docType = ((FrameworkElement) sender).DataContext as DocTypeModel;
        //        if (docType != null)
        //        {
        //            _viewModel.DeleteOcrTemplate(docType);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void BtnConfigureBarcodeClick(object sender, RoutedEventArgs e)
        //{

        //}

        //private void ConfigBarcodeButtonClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //var docType = ((FrameworkElement)sender).DataContext as DocTypeModel;
        //        //if (docType != null)
        //        //{
        //        //    ConfigBarcodeViewModel barcodeViewModel = new ConfigBarcodeViewModel(docType);
        //        //    ConfigBarcodeView view = new ConfigBarcodeView
        //        //    {
        //        //        DataContext = barcodeViewModel
        //        //    };

        //        //    DialogBaseView dialog = new DialogBaseView
        //        //    {
        //        //        Text = "Configure barcodes",
        //        //        Size = new System.Drawing.Size(900, 700),
        //        //        EnableToResize = true,
        //        //        WpfContent = view,
        //        //        WindowState = System.Windows.Forms.FormWindowState.Maximized
        //        //    };

        //        //    dialog.ShowDialog();
        //        //}

        //        _viewModel.BatchType = (BatchTypeModel)((Button)sender).Tag;
        //        var viewModel = new BarcodeConfigurationViewModel(_viewModel.BatchType, _viewModel.DocTypes.ToList());
        //        var view = new BarcodeConfigurationView(viewModel);

        //        DialogBaseView dialog = new DialogBaseView(view);
        //        dialog.Width = 850;
        //        dialog.Height = 700;
        //        dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
        //        dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        //        dialog.MaximizeBox = false;
        //        dialog.MinimizeBox = false;
        //        dialog.Text = _resource.GetString("uiDialogTitle");
        //        view.Dialog = dialog;
        //        dialog.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void BtnDeleteBarcodeClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var docType = ((FrameworkElement) sender).DataContext as DocTypeModel;
        //        if (docType != null)
        //        {
        //            _viewModel.DeleteBarcode(docType);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        private void EditPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke((ThreadStart) (() => txtBatchTypeName.Focus()));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
    }
}
