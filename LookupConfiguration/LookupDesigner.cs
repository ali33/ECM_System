using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Resources;
using Ecm.Workflow.Activities.LookupConfiguration.View;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.LookupConfiguration.ViewModel;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.LookupConfiguration
{
    public class LookupDesigner : DialogPropertyValueEditor
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());
        private DialogViewer dialog;

        public LookupDesigner()
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
                                        <DockPanel LastChildFill='True'>
                                            <pe:EditModeSwitchButton TargetEditMode='Dialog' Name='EditButton' DockPanel.Dock='Right'>...</pe:EditModeSwitchButton>
                                            <TextBlock Margin='2,0,0,0' IsHitTestVisible='False' VerticalAlignment='Center' Foreground='Gray'><Italic>" + _resource.GetString("uiLookupConfigText") + @"</Italic></TextBlock>
                                        </DockPanel>
                                    </DataTemplate>";

            using (MemoryStream sr = new MemoryStream(Encoding.UTF8.GetBytes(template)))
            {
                InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
            }
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
            ModelItem root = activityItem.Root;
            ViewStateService vss = root.GetEditingContext().Services.GetService<ViewStateService>();

            User loginUser = vss.RetrieveViewState(root, "_vsAvailableLoginUser") as User;
            BatchType batchType = vss.RetrieveViewState(root, "_vsAvailableBatchTypes") as BatchType;
            List<CustomActivitySetting> CustomActivitySettings = vss.RetrieveViewState(root, "_vsWorkflowCustomActivitySetting") as List<CustomActivitySetting>;
            Guid activityId = (Guid)activityItem.Properties["UniqueID"].ComputedValue;
            int currentSettingIndex = CustomActivitySettings.FindIndex(p => p.ActivityId == activityId);
            CustomActivitySetting activitySetting;

            if (currentSettingIndex < 0)
            {
                activitySetting = new CustomActivitySetting
                {
                    ActivityId = activityId
                };

            }
            else
            {
                activitySetting = CustomActivitySettings[currentSettingIndex].Clone();
            }

            var batchTypeModel = new BatchTypeModel
            {
                Fields = Mapper.GetFieldModels(batchType.Fields),
                Id = batchType.Id,
                Name = batchType.Name,
                UniqueId = batchType.UniqueId,
                WorkflowDefinitionId = batchType.WorkflowDefinitionId,
                DocTypes = Mapper.GetDocumentTypeModels(batchType.DocTypes)
            };

            var viewModel = new BatchTypeViewModel(batchTypeModel, activitySetting.Value, loginUser, activitySetting.ActivityId);
            var view = new BatchTypeView(viewModel);

            dialog = new DialogViewer(view);
            dialog.Width = 800;
            dialog.Height = 700;
            dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            dialog.Text = _resource.GetString("uiDialogTitle");
            dialog.FormClosed += (s, o) =>
            {
                if (dialog.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    activitySetting.Value = viewModel.LookupXml;

                    if (currentSettingIndex < 0)
                    {
                        CustomActivitySettings.Add(activitySetting);
                    }
                    else
                    {
                        CustomActivitySettings[currentSettingIndex] = activitySetting;
                    }
                    propertyValue.Value = Guid.NewGuid();
                }
            };

            view.Dialog = dialog;
            dialog.ShowDialog();
        }

    }
}
