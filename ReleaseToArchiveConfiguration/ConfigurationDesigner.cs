using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Activities.Presentation.View;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Converters;
using System.IO;
using System.Windows.Markup;
using System.Windows;
using System.Reflection;
using System.Resources;

using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.View;
using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ViewModel;
using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.Model;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration
{
    public class ConfigurationDesigner : DialogPropertyValueEditor
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ConfigurationView", Assembly.GetExecutingAssembly());
        private DialogViewer dialog;

        public ConfigurationDesigner()
        {
            const string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
                                        <DockPanel LastChildFill='True'>
                                            <pe:EditModeSwitchButton TargetEditMode='Dialog' Name='EditButton' DockPanel.Dock='Right'>...</pe:EditModeSwitchButton>
                                            <TextBlock Margin='2,0,0,0' IsHitTestVisible='False' VerticalAlignment='Center' Foreground='Gray'><Italic>Click to configure release setting</Italic></TextBlock>
                                        </DockPanel>
                                    </DataTemplate>";

            using (MemoryStream sr = new MemoryStream(Encoding.UTF8.GetBytes(template)))
            {
                InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
            }
        }

        public override void ShowDialog(PropertyValue propertyValue, System.Windows.IInputElement commandSource)
        {
            var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
            ModelItem root = activityItem.Root;
            ViewStateService vss = root.GetEditingContext().Services.GetService<ViewStateService>();

            List<DocumentType> docTypes = vss.RetrieveViewState(root, "_vsAvailableDocTypes") as List<DocumentType>;
            List<CustomActivitySetting> CustomActivitySettings = vss.RetrieveViewState(root, "_vsWorkflowCustomActivitySetting") as List<CustomActivitySetting>;
            Guid activityId = (Guid)activityItem.Properties["UniqueID"].ComputedValue;
            int currentPermissionIndex = CustomActivitySettings.FindIndex(p => p.ActivityId == activityId);
            CustomActivitySetting activitySetting;

            if (currentPermissionIndex < 0)
            {
                activitySetting = new CustomActivitySetting
                {
                    ActivityId = activityId
                };

            }
            else
            {
                activitySetting = CustomActivitySettings[currentPermissionIndex].Clone();
            }


            ConfigurationMainViewModel viewModel = new ConfigurationMainViewModel(docTypes, activitySetting.Value);
            ConfigurationMainView view = new ConfigurationMainView(viewModel);

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
                    activitySetting.Value = viewModel.ReleaseXml;

                    if (currentPermissionIndex < 0)
                    {
                        CustomActivitySettings.Add(activitySetting);
                    }
                    else
                    {
                        CustomActivitySettings[currentPermissionIndex] = activitySetting;
                    }
                    propertyValue.Value = Guid.NewGuid();
                }
            };

            view.Dialog = dialog;
            dialog.ShowDialog();
        }
    }
}
