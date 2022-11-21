using System;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;

using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel;
using Ecm.Workflow.Activities.CustomActivityDomain;
using Ecm.Utility;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner
{
    public class PermissionDesigner : DialogPropertyValueEditor
    {
        public PermissionDesigner()
        {
            const string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
                                        <DockPanel LastChildFill='True'>
                                            <pe:EditModeSwitchButton TargetEditMode='Dialog' Name='EditButton' DockPanel.Dock='Right'>...</pe:EditModeSwitchButton>
                                            <TextBlock Margin='2,0,0,0' IsHitTestVisible='False' VerticalAlignment='Center' Foreground='Gray'><Italic>Click to configure permissions</Italic></TextBlock>
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

            // get the activity
            ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;

            // get the activity builder of the activity, which contains common data 
            ModelItem root = activityItem.Root;
            ViewStateService vss = root.GetEditingContext().Services.GetService<ViewStateService>();
            //List<HumanStepPermission> permissions = vss.RetrieveViewState(root, "_vsWorkflowPermission") as List<HumanStepPermission>;

            List<CustomActivitySetting> CustomActivitySettings = vss.RetrieveViewState(root, "_vsWorkflowCustomActivitySetting") as List<CustomActivitySetting>;
            Guid activityId = (Guid)activityItem.Properties["UniqueID"].ComputedValue;
            int currentSettingIndex = CustomActivitySettings.FindIndex(p => p.ActivityId == activityId);
            CustomActivitySetting activitySetting;
            ActivityPermission permission;

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

            if (string.IsNullOrEmpty(activitySetting.Value))
            {
                permission = new ActivityPermission();
            }
            else
            {
                permission = (ActivityPermission)UtilsSerializer.Deserialize<ActivityPermission>(activitySetting.Value);
            }

            List<UserGroup> userGroups = vss.RetrieveViewState(root, "_vsAvailableUserGroups") as List<UserGroup>;
            List<DocumentType> docTypes = vss.RetrieveViewState(root, "_vsAvailableDocTypes") as List<DocumentType>;

            // We are configuring permission for document type. If we configure permission on batch type, we have to define another form
            HumanStepPermissionViewModel viewModel = new HumanStepPermissionViewModel
                                                         {
                                                             UserGroups = userGroups,
                                                             DocTypes = docTypes,
                                                             Permission = permission
                                                         };
            viewModel.Initialize();

            HumanStepPermissionForm permissionForm = new HumanStepPermissionForm();
            permissionForm.Initialize(viewModel);

            if (permissionForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                activitySetting.Value = UtilsSerializer.Serialize<ActivityPermission>(viewModel.Permission);

                if (currentSettingIndex < 0)
                {
                    CustomActivitySettings.Add(activitySetting);
                }
                else
                {
                    CustomActivitySettings[currentSettingIndex] = activitySetting;
                }

                // trigger the event on workflow designer
                propertyValue.Value = Guid.NewGuid();
            }
        }

    }
}
