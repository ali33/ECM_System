using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Core.Presentation.Factories;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Toolbox;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.Contract;
using Ecm.WorkflowDesigner.Model;
using System.Windows.Input;
using System.Activities.Presentation.Validation;
using Ecm.WorkflowDesigner.ViewModel;

namespace Ecm.WorkflowDesigner
{
    /// <summary>
    /// Interaction logic for DesignerContainer.xaml
    /// </summary>
    public partial class DesignerContainer
    {
        //private ValidateDesignerError _validateError = new ValidateDesignerError();
        
        //[ImportMany]
        //private IEnumerable<StoppableActivityContract> _activities { get; set; }

        //[ImportMany]
        //private IEnumerable<ActivityWithResult> _Xactivities { get; set; }

        //#region Dependency properties
        //public static readonly DependencyProperty WorkflowDefinitionProperty =
        //    DependencyProperty.Register("WorkflowDefinition", typeof(WorkflowDefinition), typeof(DesignerContainer),
        //                    new FrameworkPropertyMetadata(null, WorkflowDefinitionChangedCallback));

        //private static void WorkflowDefinitionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var designer = d as DesignerContainer;
        //    if (designer != null)
        //    {
        //        //designer.LoadWorkflow();
        //    }
        //}

        //public static readonly DependencyProperty LoginUserProperty =
        //    DependencyProperty.Register("LoginUser", typeof(User), typeof(DesignerContainer));

        //public static readonly DependencyProperty UserGroupsProperty =
        //    DependencyProperty.Register("UserGroups", typeof(List<UserGroup>), typeof(DesignerContainer));

        //public static readonly DependencyProperty DocTypesProperty =
        //    DependencyProperty.Register("DocTypes", typeof(List<DocumentType>), typeof(DesignerContainer));

        //public static readonly DependencyProperty BatchTypeProperty =
        //    DependencyProperty.Register("BatchType", typeof(BatchType), typeof(DesignerContainer));

        //public static readonly DependencyProperty HumanStepPermissionsProperty =
        //    DependencyProperty.Register("HumanStepPermissions", typeof(List<HumanStepPermission>), typeof(DesignerContainer));

        //public static readonly DependencyProperty CustomActivitySettingsProperty =
        //    DependencyProperty.Register("CustomActivitySettings", typeof(List<CustomActivitySetting>), typeof(DesignerContainer));

        //public RoutedCommand SaveCommand;

        //#endregion

        //public DesignerContainer()
        //{
        //    InitializeComponent();

        //    Loaded += DesignerContainerLoaded;
        //    Unloaded += DesignerContainerUnloaded;
        //}

        //public void Save()
        //{
        //    HandleViewStateData(false);

        //    try
        //    {
        //        _workflowDesigner.Flush();

        //        if (!string.IsNullOrEmpty(_workflowDesigner.Text))
        //        {
        //            if (SaveWorkflow != null)
        //            {
        //                SaveWorkflow(BatchType.Id, _workflowDesigner.Text, HumanStepPermissions, CustomActivitySettings);
        //            }
        //        }

        //        IsChanged = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleExceptionAction(ex);
        //    }
        //    finally
        //    {
        //        HandleViewStateData(true);
        //    }
        //}

        //public bool ValidateWorkflow()
        //{
        //    ModelItem root = _workflowDesigner.Context.Services.GetService<ModelService>().Root;

        //    // if we want to access advanced properties of the wf then seach the properties from root.Properties
        //    // ModelItemCollection col = root.Properties["Properties"].Collection;

        //    // explicitly access the root of the workflow
        //    ActivityBuilder rootBuilder = (ActivityBuilder)root.GetCurrentValue();
            
        //    if (rootBuilder.Implementation == null)
        //    {
        //        return false;
        //    }

        //    // find argument (rootBuilder.Properties)
        //    DynamicActivityProperty sysInfoArg = rootBuilder.Properties.SingleOrDefault(p => p.Name == WorkflowConstant.RuntimeData &&
        //                                                                                     p.Type == typeof(InArgument<WorkflowRuntimeData>));
        //    return sysInfoArg != null;

        //}


        //public Action<Exception> HandleExceptionAction;

        //public event SaveWorkflowEventHandler SaveWorkflow;

        //public bool IsChanged { get; internal set; }

        //public User LoginUser
        //{
        //    get { return GetValue(LoginUserProperty) as User; }
        //    set { SetValue(LoginUserProperty, value); }
        //}

        //public List<UserGroup> UserGroups
        //{
        //    get { return GetValue(UserGroupsProperty) as List<UserGroup>; }
        //    set { SetValue(UserGroupsProperty, value); }
        //}

        //public List<DocumentType> DocTypes
        //{
        //    get { return GetValue(DocTypesProperty) as List<DocumentType>; }
        //    set { SetValue(DocTypesProperty, value); }
        //}

        //public BatchType BatchType
        //{
        //    get { return GetValue(BatchTypeProperty) as BatchType; }
        //    set { SetValue(BatchTypeProperty, value); }
        //}

        //public List<HumanStepPermission> HumanStepPermissions
        //{
        //    get { return GetValue(HumanStepPermissionsProperty) as List<HumanStepPermission>; }
        //    set { SetValue(HumanStepPermissionsProperty, value); }
        //}

        //public List<CustomActivitySetting> CustomActivitySettings
        //{
        //    get { return GetValue(CustomActivitySettingsProperty) as List<CustomActivitySetting>; }
        //    set { SetValue(CustomActivitySettingsProperty, value); }
        //}

        //public WorkflowDefinition WorkflowDefinition
        //{
        //    get { return GetValue(WorkflowDefinitionProperty) as WorkflowDefinition; }
        //    set { SetValue(WorkflowDefinitionProperty, value); }
        //}

        ////internal WorkflowCommandManager WorkflowCommandManager { get; private set; }

        //private void DesignerContainerUnloaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void DesignerContainerLoaded(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        designerCol.Width = new GridLength(75, GridUnitType.Star);
        //        clProperties.Width = new GridLength(25, GridUnitType.Star);

        //        InitializeDesigner();
        //        RegisterCommands();
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleExceptionAction(ex);
        //    }
        //}

        //private void RegisterCommands()
        //{
        //    var gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
        //    SaveCommand = new RoutedCommand("SaveCommand", typeof(DesignerContainer), gesture);
        //    var commandBinding = new CommandBinding(SaveCommand, Save, CanSave);
        //    this.CommandBindings.Add(commandBinding);

        //    ButtonSave.Command = SaveCommand;
        //}

        //private void InitializeDesigner()
        //{
        //    try
        //    {
        //        DesignerMetadata designerMetadata = new DesignerMetadata();
        //        designerMetadata.Register();

        //        AddToolbox();
        //        LoadWorkflow();
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleExceptionAction(ex);
        //    }
        //}

        //private void AddToolbox()
        //{
        //    ToolboxControl toolboxControl = new ToolboxControl();
        //    toolboxControl.Loaded += ToolboxControlLoaded;
        //    toolboxControl.BorderThickness = new Thickness(0);
        //    toolboxControl.Background = Brushes.Transparent;
        //    toolboxControl.Categories.Add(GetStandardToolboxCategory());
        //    toolboxArea.Child = toolboxControl;

        //    try
        //    {
        //        AddCustomActivitiesToToolbox();
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleExceptionAction(ex);
        //    }
        //}

        //private void ToolboxControlLoaded(object sender, RoutedEventArgs e)
        //{
        //    ToolboxControl toolbox = sender as ToolboxControl;
        //    if (toolbox != null)
        //    {
        //        TextBox searchBox = toolbox.Template.FindName("PART_SearchBox", toolbox) as TextBox;

        //        if (searchBox != null)
        //        {
        //            searchBox.Visibility = Visibility.Collapsed;
        //            Grid grid = VisualTreeHelper.GetParent(searchBox) as Grid;

        //            if (grid != null)
        //            {
        //                foreach (UIElement item in grid.Children)
        //                {
        //                    if (item is TextBlock)
        //                    {
        //                        item.Visibility = Visibility.Collapsed;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private ToolboxCategory GetStandardToolboxCategory()
        //{
        //    ToolboxCategory category = new ToolboxCategory("Standard Activities")
        //    {
        //        new ToolboxItemWrapper(typeof (Assign)),
        //        new ToolboxItemWrapper(typeof (DoWhile)),
        //        new ToolboxItemWrapper(typeof (Flowchart)),
        //        new ToolboxItemWrapper(typeof (FlowDecision)),
        //        new ToolboxItemWrapper(typeof (FlowSwitch<>), "FlowSwitch<T>"),
        //        new ToolboxItemWrapper(typeof (ForEachWithBodyFactory<>), "ForEach<T>"),
        //        new ToolboxItemWrapper(typeof (If)),
        //        new ToolboxItemWrapper(typeof (TryCatch)),
        //        new ToolboxItemWrapper(typeof (Sequence)),
        //        new ToolboxItemWrapper(typeof (Switch<>), "Switch<T>"),
        //        new ToolboxItemWrapper(typeof (While))
        //    };

        //    return category;
        //}

        //private void AddCustomActivitiesToToolbox()
        //{
        //    LoadCustomActivities();

        //    ToolboxControl toolboxControl = toolboxArea.Child as ToolboxControl;
        //    Dictionary<string, int> categoryIndexCollection = new Dictionary<string, int>();

        //    ToolboxCategory toolboxCategory = new ToolboxCategory("ECM Activities");
        //    toolboxControl.Categories.Add(toolboxCategory);

        //    if (_activities != null && _activities.Count() > 0)
        //    {
        //        foreach (var item in _activities)
        //        {
        //            //object[] customAttributes = item.GetType().Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        //            //currentItemName = ((AssemblyTitleAttribute)customAttributes[0]).Title;

        //            //if (categoryIndexCollection.ContainsKey(currentItemName))
        //            //{
        //            //    toolboxCategory = toolboxControl.Categories[categoryIndexCollection[currentItemName]];
        //            //}
        //            //else
        //            //{
        //            //    toolboxCategory = new ToolboxCategory(currentItemName);
        //            //    toolboxControl.Categories.Add(toolboxCategory);
        //            //    categoryIndexCollection.Add(currentItemName, toolboxControl.Categories.Count - 1);
        //            //}

        //            toolboxCategory.Add(new ToolboxItemWrapper(item.GetType()));
        //        }
        //    }

        //    if (_Xactivities != null && _Xactivities.Count() > 0)
        //    {
        //        foreach (var item in _Xactivities)
        //        {
        //            //currentItemName = item.GetType().Assembly.GetName().Name;
        //            //if (categoryIndexCollection.ContainsKey(currentItemName))
        //            //{
        //            //    toolboxCategory = toolboxControl.Categories[categoryIndexCollection[currentItemName]];
        //            //}
        //            //else
        //            //{
        //            //    toolboxCategory = new ToolboxCategory(currentItemName);
        //            //    toolboxControl.Categories.Add(toolboxCategory);
        //            //    categoryIndexCollection.Add(currentItemName, toolboxControl.Categories.Count - 1);
        //            //}

        //            toolboxCategory.Add(new ToolboxItemWrapper(item.GetType()));
        //        }
        //    }
        //}

        //private void LoadCustomActivities()
        //{
        //    string directory = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
        //    var catalog = new DirectoryCatalog(directory);
        //    var container = new CompositionContainer(catalog);
        //    var batch = new CompositionBatch();
        //    batch.AddPart(this);
        //    container.Compose(batch);
        //}

        //internal void LoadWorkflow()
        //{
        //    if (IsLoaded)
        //    {
        //        _workflowDesigner = new System.Activities.Presentation.WorkflowDesigner();
        //        _workflowDesigner.Context.Services.Publish<IValidationErrorService>(_validateError);
        //        LoadWorkflow(WorkflowDefinition.DefinitionXML);

        //        Grid grid = _workflowDesigner.View as Grid;
        //        if (grid != null)
        //        {
        //            grid.Background = Brushes.Transparent;
        //        }
        //        designerArea.Child = _workflowDesigner.View;

        //        Panel panel = _workflowDesigner.PropertyInspectorView as Panel;
        //        if (panel != null)
        //        {
        //            panel.Background = Brushes.Transparent;
        //        }
        //        propertiesArea.Child = _workflowDesigner.PropertyInspectorView;

        //        _workflowDesigner.Context.Services.GetService<DesignerView>().WorkflowShellBarItemVisibility = ShellBarItemVisibility.Variables | ShellBarItemVisibility.Zoom | ShellBarItemVisibility.MiniMap;
                
        //        // attach the workflow permission into the ActivityBuilder so the activity could access this at design time
        //        HandleViewStateData(true);

        //        ModelService modelService = _workflowDesigner.Context.Services.GetService<ModelService>();
        //        modelService.ModelChanged += ModelServiceModelChanged;
        //    }
        //}

        //private void LoadWorkflow(string workflowXamlDefinition)
        //{
        //    if (string.IsNullOrEmpty(workflowXamlDefinition))
        //    {
        //        ActivityBuilder acBuilder = new ActivityBuilder { Name = "Workflow for batch type" };

        //        Flowchart rootNode = new Flowchart();
        //        acBuilder.Implementation = rootNode;
        //        _workflowDesigner.Load(acBuilder);

        //        // add an argument RuntimeInfo (InArgument<WorkflowRuntimeData>) for the workflow - this will be used when executing the workflow
        //        var property = new DynamicActivityProperty
        //        {
        //            Name = WorkflowConstant.RuntimeData,
        //            Type = typeof(InArgument<WorkflowRuntimeData>),
        //        };

        //        _workflowDesigner.Context.Services.GetService<ModelService>().Root.Properties["Properties"].Collection.Add(property);
        //    }
        //    else
        //    {
        //        string workflowPath = SaveWorkflowToTempFile(workflowXamlDefinition);
        //        _workflowDesigner.Load(workflowPath);
        //        File.Delete(workflowPath);
        //    }
        //}

        //private void HandleViewStateData(bool storeViewState)
        //{
        //    ModelItem root = _workflowDesigner.Context.Services.GetService<ModelService>().Root;
        //    ViewStateService vss = root.GetEditingContext().Services.GetService<ViewStateService>();

        //    if (storeViewState)
        //    {
        //        vss.StoreViewState(root, "_vsAvailableUserGroups", UserGroups);
        //        vss.StoreViewState(root, "_vsAvailableBatchTypes", BatchType);
        //        vss.StoreViewState(root, "_vsAvailableDocTypes", DocTypes);
        //        vss.StoreViewState(root, "_vsWorkflowPermission", HumanStepPermissions);
        //        vss.StoreViewState(root, "_vsWorkflowCustomActivitySetting", CustomActivitySettings);
        //        vss.StoreViewState(root, "_vsAvailableLoginUser", LoginUser);
        //    }
        //    else
        //    {
        //        vss.RemoveViewState(root, "_vsAvailableUserGroups");
        //        vss.RemoveViewState(root, "_vsAvailableBatchTypes");
        //        vss.RemoveViewState(root, "_vsAvailableDocTypes");
        //        vss.RemoveViewState(root, "_vsWorkflowPermission");
        //        vss.RemoveViewState(root, "_vsWorkflowCustomActivitySetting");
        //        vss.RemoveViewState(root, "_vsAvailableLoginUser");
        //    }
        //}

        //private void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        //{
        //    if (e.ItemsAdded != null && e.ItemsAdded.Count() == 1)
        //    {
        //        var addedItem = e.ItemsAdded.First();
        //        if (addedItem.GetCurrentValue() is StoppableActivityContract)
        //        {
        //            AssignSystemInfoToActivity(addedItem.GetCurrentValue() as StoppableActivityContract);
        //        }
        //        else if (addedItem.Content != null &&
        //                 addedItem.Content.ComputedValue != null &&
        //                 addedItem.Content.ComputedValue is StoppableActivityContract)
        //        {
        //            AssignSystemInfoToActivity(addedItem.Content.ComputedValue as StoppableActivityContract);
        //        }
        //    }
        //    else if (e.ItemsRemoved != null)
        //    {
        //        foreach (var item in e.ItemsRemoved)
        //        {
        //            if (item.ItemType == typeof(FlowStep))
        //            {
        //                RemoveActivityData(item.Content.ComputedValue as Activity);
        //            }
        //            else
        //            {
        //                RemoveActivityData(item.GetCurrentValue() as Activity);
        //            }
        //        }
        //    }
        //    else if (e.PropertiesChanged != null)
        //    {
        //        var changedItem = e.PropertiesChanged.First();

        //        if (changedItem != null &&
        //                                changedItem.ComputedValue != null &&
        //                                changedItem.ComputedValue is StoppableActivityContract)
        //        {
        //            AssignSystemInfoToActivity(changedItem.ComputedValue as StoppableActivityContract);
        //        }
        //        else
        //        {
        //            if (changedItem.Parent.GetCurrentValue() is ActivityBuilder)
        //            {
        //                if (changedItem.Value == null && changedItem.Name.ToLower() == "implementation")
        //                {
        //                    HumanStepPermissions.Clear();
        //                    CustomActivitySettings.Clear();
        //                }
        //            }
        //        }
        //    }
        //    IsChanged = true;
        //}

        //private void AssignSystemInfoToActivity(StoppableActivityContract activity)
        //{
        //    //activity.UserID = LoginUser.Id;
        //    //activity.UserName = LoginUser.UserName;
        //    //activity.PasswordHash = LoginUser.Password;
        //    activity.User = LoginUser;
        //    activity.ObjectID = BatchType.Id;

        //    activity.UniqueID = Guid.NewGuid();
        //}

        //private void RemoveActivityData(Activity activity)
        //{
        //    if (activity is StoppableActivityContract)
        //    {
        //        int actIndex = HumanStepPermissions.FindIndex(a => a.HumanStepId == ((StoppableActivityContract)activity).UniqueID);

        //        if (actIndex >= 0)
        //        {
        //            HumanStepPermissions.RemoveAt(actIndex);
        //        }

        //        int customActIndex = CustomActivitySettings.FindIndex(p => p.ActivityId == ((StoppableActivityContract)activity).UniqueID);

        //        if (customActIndex >= 0)
        //        {
        //            CustomActivitySettings.RemoveAt(customActIndex);
        //        }

        //    }
        //    else if (activity is Sequence)
        //    {
        //        foreach (Activity act in (activity as Sequence).Activities)
        //        {
        //            RemoveActivityData(act);
        //        }
        //    }
        //    else if (activity is Flowchart)
        //    {
        //        foreach (FlowStep step in (activity as Flowchart).Nodes)
        //        {
        //            RemoveActivityData(step.Action);
        //        }
        //    }
        //}

        //private string SaveWorkflowToTempFile(string workflowXaml)
        //{
        //    var path = Path.GetTempFileName();
        //    using (StreamWriter writer = File.CreateText(path))
        //    {
        //        writer.Write(workflowXaml);
        //        return path;
        //    }
        //}

        //private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    _workflowDesigner.Context.Services.GetService<ValidationService>().ValidateWorkflow();
        //    e.CanExecute = IsChanged && ValidateWorkflow() && !_validateError.HasError;
        //}

        //private void Save(object sender, ExecutedRoutedEventArgs e)
        //{
        //    try
        //    {
        //        Save();
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleExceptionAction(ex);
        //    }
        //}

        //private System.Activities.Presentation.WorkflowDesigner _workflowDesigner;

        private void BtnExpandLeftClick(object sender, RoutedEventArgs e)
        {
            if (btnExpandLeft.IsChecked.Value)
            {
                pnToolbox.Visibility = Visibility.Collapsed;
                pnLeftHeader.Visibility = Visibility.Collapsed;
                pnLeft.Width = 22;
            }
            else
            {
                pnToolbox.Visibility = Visibility.Visible;
                pnLeftHeader.Visibility = Visibility.Visible;
                pnLeft.Width = pnLeft.MaxWidth;
            }
        }
        private DesignerContainerViewModel _viewModel;

        public DesignerContainer(DesignerContainerViewModel viewModel)
        {
            InitializeComponent();
            DataContext =_viewModel = viewModel;
            Loaded += DesignerContainerLoaded;
            Unloaded += DesignerContainerUnloaded;
        }

        private void DesignerContainerUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void DesignerContainerLoaded(object sender, RoutedEventArgs e)
        {
            designerCol.Width = new GridLength(75, GridUnitType.Star);
            clProperties.Width = new GridLength(25, GridUnitType.Star);
        }

        private void BtnExpandRightClick(object sender, RoutedEventArgs e)
        {
            if (btnExpandRight.IsChecked.Value)
            {
                pnProperties.Visibility = Visibility.Collapsed;
                pnRightHeader.Visibility = Visibility.Collapsed;
                pnActivityProperties.Visibility = System.Windows.Visibility.Visible;
                clProperties.Width = new GridLength(22);
            }
            else
            {
                pnProperties.Visibility = Visibility.Visible;
                pnRightHeader.Visibility = Visibility.Visible;
                designerCol.Width = new GridLength(75,GridUnitType.Star);
                clProperties.Width = new GridLength(25, GridUnitType.Star);
                pnActivityProperties.Visibility = System.Windows.Visibility.Hidden;
                pnRight.Width = Double.NaN;
            }

        }

        private void pnLeft_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pnLeft.ActualWidth <= 22)
            {
                pnLeftHeader.Visibility = Visibility.Collapsed;
                pnToolbox.Visibility = Visibility.Collapsed;
                pnLeft.Width = 22;
                btnExpandLeft.IsChecked = true;
            }
            else
            {
                pnLeftHeader.Visibility = Visibility.Visible;
                pnToolbox.Visibility = Visibility.Visible;
                btnExpandLeft.IsChecked = false;
            }

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 22)
            {
                clProperties.Width = new GridLength(22);
                pnProperties.Visibility = System.Windows.Visibility.Collapsed;
                pnProperties.Visibility = Visibility.Collapsed;
                pnRightHeader.Visibility = Visibility.Collapsed;
                pnActivityProperties.Visibility = System.Windows.Visibility.Visible;
                btnExpandRight.IsChecked = true;
            }
            else
            {
                pnProperties.Visibility = Visibility.Visible;
                pnRightHeader.Visibility = Visibility.Visible;
                pnActivityProperties.Visibility = System.Windows.Visibility.Hidden;
                btnExpandRight.IsChecked = false;
                pnRight.Width = Double.NaN;
            }
        }
        //end
    }
}
