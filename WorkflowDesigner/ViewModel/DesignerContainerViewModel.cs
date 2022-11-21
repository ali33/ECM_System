using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Activities.Presentation;
using System.ComponentModel.Composition;
using Ecm.Workflow.Activities.Contract;
using System.Activities;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System.Activities.Core.Presentation;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.IO;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Toolbox;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Input;
using Ecm.WorkflowDesigner.Model;
using System.Activities.Presentation.Validation;
using System.Activities.Core.Presentation.Factories;
using System.Resources;

namespace Ecm.WorkflowDesigner.ViewModel
{
    public class DesignerContainerViewModel : BaseDependencyProperty
    {
        private UIElement _designerViewer;
        private UIElement _propertyInspectorView;
        private System.Activities.Presentation.WorkflowDesigner _workflowDesigner = new System.Activities.Presentation.WorkflowDesigner();
        private ToolboxControl _toolboxControl;
        private RelayCommand _saveCommand;
        private ValidateDesignerError _validateError = new ValidateDesignerError();
        private List<ResourceReader> _resourceReaderList;

        public DesignerContainerViewModel(string xml)
        {
            XML = xml;
            Initialize();
            AddToolbox();
        }

        public string XML { get; set; }

        public string WorkflowText { get; set; }

        [ImportMany]
        private IEnumerable<StoppableActivityContract> _activities { get; set; }

        [ImportMany]
        private IEnumerable<ActivityWithResult> _Xactivities { get; set; }

        public event SaveWorkflowEventHandler SaveWorkflow;

        public ToolboxControl ToolboxControl
        {
            get { return _toolboxControl; }
            set
            {
                if (value == _toolboxControl)
                    return;

                _toolboxControl = value;

                OnPropertyChanged("ToolboxControl");
            }
        }

        public Action<Exception> HandleExceptionAction;

        public UIElement DesignerViewer
        {
            get { return _designerViewer; }
            set
            {
                if (value == _designerViewer)
                    return;

                _designerViewer = value;

                OnPropertyChanged("DesignerViewer");
            }
        }

        public UIElement PropertyInspectorView
        {
            get { return _propertyInspectorView; }
            set
            {
                if (value == _propertyInspectorView)
                    return;

                _propertyInspectorView = value;

                OnPropertyChanged("PropertyInspectorView");
            }
        }

        public bool IsChanged { get; internal set; }

        public List<UserGroup> UserGroups { get; set; }

        public User LoginUser { get; set; }

        public BatchType BatchType { get; set; }

        public List<DocumentType> DocTypes { get; set; }

        //public List<HumanStepPermission> HumanStepPermissions { get; set; }

        public List<CustomActivitySetting> CustomActivitySettings { get; set; }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => Save(), p => CanSave());
                }
                return _saveCommand;
            }
        }

        //------ Priviate method

        private bool CanSave()
        {
            _workflowDesigner.Context.Services.GetService<ValidationService>().ValidateWorkflow();
            return IsChanged && ValidateWorkflow() && !_validateError.HasError;
        }

        private void Save()
        {
            HandleViewStateData(false);

            try
            {
                _workflowDesigner.Flush();

                if (!string.IsNullOrEmpty(_workflowDesigner.Text))
                {
                    if (SaveWorkflow != null)
                    {
                        SaveWorkflow(BatchType.Id, _workflowDesigner.Text, CustomActivitySettings);
                        //SaveWorkflow(BatchType.Id, _workflowDesigner.Text, HumanStepPermissions, CustomActivitySettings);
                    }
                }

                IsChanged = false;
            }
            catch (Exception ex)
            {
                HandleExceptionAction(ex);
            }
            finally
            {
                HandleViewStateData(true);
            }
        }

        private void AddToolbox()
        {
            ToolboxControl = new ToolboxControl();
            ToolboxControl.Loaded += ToolboxControlLoaded;
            ToolboxControl.BorderThickness = new Thickness(0);
            ToolboxControl.Background = Brushes.Transparent;
            ToolboxControl.CategoryItemStyle = ToolboxControl.TryFindResource("ToolboxStyle") as Style;

            try
            {
                AddCustomActivitiesToToolbox();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            foreach (var catagory in GetStandardToolboxCategory())
            {
                ToolboxControl.Categories.Add(catagory);
            }

        }

        private void ToolboxControlLoaded(object sender, RoutedEventArgs e)
        {
            ToolboxControl toolbox = sender as ToolboxControl;

            if (toolbox != null)
            {
                TextBox searchBox = toolbox.Template.FindName("PART_SearchBox", toolbox) as TextBox;

                if (searchBox != null)
                {
                    searchBox.Visibility = Visibility.Collapsed;
                    Grid grid = VisualTreeHelper.GetParent(searchBox) as Grid;

                    if (grid != null)
                    {
                        foreach (UIElement item in grid.Children)
                        {
                            if (item is TextBlock || item is TreeViewItem)
                            {
                                item.Visibility = Visibility.Collapsed;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void AddCustomActivitiesToToolbox()
        {
            LoadCustomActivities();

            //ToolboxControl toolboxControl = toolboxArea.Child as ToolboxControl;
            Dictionary<string, int> categoryIndexCollection = new Dictionary<string, int>();

            ToolboxCategory toolboxCategory = new ToolboxCategory("ECM Activities");
            ToolboxControl.Categories.Add(toolboxCategory);

            if (_activities != null && _activities.Count() > 0)
            {
                foreach (var item in _activities)
                {
                    toolboxCategory.Add(new ToolboxItemWrapper(item.GetType()));
                }
            }

            if (_Xactivities != null && _Xactivities.Count() > 0)
            {
                foreach (var item in _Xactivities)
                {
                    toolboxCategory.Add(new ToolboxItemWrapper(item.GetType()));
                }
            }
        }

        private void LoadCustomActivities()
        {
            string directory = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            var catalog = new DirectoryCatalog(directory);
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddPart(this);
            container.Compose(batch);
        }

        private List<ToolboxCategory> GetStandardToolboxCategory()
        {
            List<ToolboxCategory> toolboxCategories = new List<ToolboxCategory>
            {
                new ToolboxCategory("Control Flow")
                {
                    new ToolboxItemWrapper(typeof(DoWhile), "DoWhile", "DoWhile"),
                    new ToolboxItemWrapper(typeof(ForEachWithBodyFactory<>), "ForEach", "ForEach<>"),
                    new ToolboxItemWrapper(typeof(If), "If", "If"),
                    new ToolboxItemWrapper(typeof(Parallel), "Parallel", "Parallel"),
                    new ToolboxItemWrapper(typeof(ParallelForEach<>), "ParallelForEach", "ParallelForEach<T>"),              
                    new ToolboxItemWrapper(typeof(Pick), "Pick", "Pick"),
                    new ToolboxItemWrapper(typeof(PickBranch), "PickBranch", "PickBranch"),         
                    new ToolboxItemWrapper(typeof(Sequence), "Sequence", "Sequence"),
                    new ToolboxItemWrapper(typeof(Switch<>), "Switch", "Switch<T>"),
                    new ToolboxItemWrapper(typeof(While), "While", "While")
                },
                new ToolboxCategory("Flowchart")
                {
                    new ToolboxItemWrapper(typeof(Flowchart), "Flowchart", "Flowchart"),
                    new ToolboxItemWrapper(typeof(FlowSwitch<>), "FlowSwitch", "FlowSwitch<T>"),
                    new ToolboxItemWrapper(typeof(FlowDecision), "FlowDecision", "FlowDecision")
                },
                new ToolboxCategory("Runtime")
                {
                    new ToolboxItemWrapper(typeof(Persist), "Persist", "Persist"),
                    new ToolboxItemWrapper(typeof(TerminateWorkflow), "TerminateWorkflow", "TerminateWorkflow")
                },
                new ToolboxCategory("Primitives")
                {
                    new ToolboxItemWrapper(typeof(Assign), "Assign", "Assign"),
                    new ToolboxItemWrapper(typeof(Assign<>), "Assign", "Assign<T>"),
                    new ToolboxItemWrapper(typeof(Delay), "Delay", "Delay"),
                    new ToolboxItemWrapper(typeof(InvokeMethod), "InvokeMethod", "InvokeMethod"),
                    new ToolboxItemWrapper(typeof(WriteLine), "WriteLine", "WriteLine")
                },
                new ToolboxCategory("Transaction")
                {
                    new ToolboxItemWrapper(typeof(CancellationScope), "CancellationScope", "CancellationScope"),
                    new ToolboxItemWrapper(typeof(CompensableActivity), "CompensableActivity", "CompensableActivity"),
                    new ToolboxItemWrapper(typeof(Compensate), "Compensate", "Compensate"),
                    new ToolboxItemWrapper(typeof(Confirm), "Confirm", "Confirm"),
                    new ToolboxItemWrapper(typeof(TransactionScope), "TransactionScope", "TransactionScope")
                },
                new ToolboxCategory("Collection")
                {
                    new ToolboxItemWrapper(typeof(AddToCollection<>), "AddToCollection", "AddToCollection<T>"),
                    new ToolboxItemWrapper(typeof(ClearCollection<>), "ClearCollection", "ClearCollection<T>"),
                    new ToolboxItemWrapper(typeof(ExistsInCollection<>), "ExistsInCollection", "ExistsInCollection<T>"),
                    new ToolboxItemWrapper(typeof(RemoveFromCollection<>), "RemoveFromCollection", "RemoveFromCollection<T>")
                },
                new ToolboxCategory("Error Handling")
                {
                    new ToolboxItemWrapper(typeof(Rethrow), "Rethrow", "Rethrow"),
                    new ToolboxItemWrapper(typeof(Throw), "Throw", "Throw"),
                    new ToolboxItemWrapper(typeof(TryCatch), "TryCatch", "TryCatch")
                }
            };

            return toolboxCategories;
        }


        public void GetWorkflowContentText()
        {
            _workflowDesigner.Flush();
            XML = WorkflowText = _workflowDesigner.Text;
        }

        private void Initialize()
        {
            DesignerMetadata designerMetadata = new DesignerMetadata();
            designerMetadata.Register();
        }

        public bool ValidateWorkflow()
        {
            ModelItem root = _workflowDesigner.Context.Services.GetService<ModelService>().Root;

            // if we want to access advanced properties of the wf then seach the properties from root.Properties
            // ModelItemCollection col = root.Properties["Properties"].Collection;

            // explicitly access the root of the workflow
            ActivityBuilder rootBuilder = (ActivityBuilder)root.GetCurrentValue();

            if (rootBuilder.Implementation == null)
            {
                return false;
            }

            // find argument (rootBuilder.Properties)
            DynamicActivityProperty sysInfoArg = rootBuilder.Properties.SingleOrDefault(p => p.Name == WorkflowConstant.RuntimeData &&
                                                                                             p.Type == typeof(InArgument<WorkflowRuntimeData>));

            return sysInfoArg != null;
        }

        public void LoadWorkflow()
        {
            LoadWorkflow(XML);

            DesignerViewer = _workflowDesigner.View;

            PropertyInspectorView = _workflowDesigner.PropertyInspectorView;

            _workflowDesigner.Context.Services.GetService<DesignerView>().WorkflowShellBarItemVisibility = ShellBarItemVisibility.Variables | ShellBarItemVisibility.Zoom | ShellBarItemVisibility.MiniMap;

            // attach the workflow permission into the ActivityBuilder so the activity could access this at design time
            HandleViewStateData(true);

            ModelService modelService = _workflowDesigner.Context.Services.GetService<ModelService>();
            _workflowDesigner.Context.Services.Publish<IValidationErrorService>(_validateError);
            modelService.ModelChanged += ModelServiceModelChanged;
        }

        private void LoadWorkflow(string workflowXamlDefinition)
        {
            if (string.IsNullOrEmpty(workflowXamlDefinition))
            {
                ActivityBuilder acBuilder = new ActivityBuilder { Name = "Business Process" };

                Flowchart rootNode = new Flowchart();
                acBuilder.Implementation = rootNode;
                _workflowDesigner.Load(acBuilder);

                var property = new DynamicActivityProperty
                {
                    Name = WorkflowConstant.RuntimeData,
                    Type = typeof(InArgument<WorkflowRuntimeData>),
                };

                _workflowDesigner.Context.Services.GetService<ModelService>().Root.Properties["Properties"].Collection.Add(property);
            }
            else
            {
                string workflowPath = SaveWorkflowToTempFile(workflowXamlDefinition);
                _workflowDesigner.Load(workflowPath);
                File.Delete(workflowPath);
            }
        }

        private void HandleViewStateData(bool storeViewState)
        {
            ModelItem root = _workflowDesigner.Context.Services.GetService<ModelService>().Root;
            ViewStateService vss = root.GetEditingContext().Services.GetService<ViewStateService>();

            if (storeViewState)
            {
                vss.StoreViewState(root, "_vsAvailableUserGroups", UserGroups);
                vss.StoreViewState(root, "_vsAvailableBatchTypes", BatchType);
                vss.StoreViewState(root, "_vsAvailableDocTypes", DocTypes);
                //vss.StoreViewState(root, "_vsWorkflowPermission", HumanStepPermissions);
                vss.StoreViewState(root, "_vsWorkflowCustomActivitySetting", CustomActivitySettings);
                vss.StoreViewState(root, "_vsAvailableLoginUser", LoginUser);
            }
            else
            {
                vss.RemoveViewState(root, "_vsAvailableUserGroups");
                vss.RemoveViewState(root, "_vsAvailableBatchTypes");
                vss.RemoveViewState(root, "_vsAvailableDocTypes");
                //vss.RemoveViewState(root, "_vsWorkflowPermission");
                vss.RemoveViewState(root, "_vsWorkflowCustomActivitySetting");
                vss.RemoveViewState(root, "_vsAvailableLoginUser");
            }
        }

        private void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            if (e.ItemsAdded != null && e.ItemsAdded.Count() == 1)
            {
                var addedItem = e.ItemsAdded.First();
                if (addedItem.GetCurrentValue() is StoppableActivityContract)
                {
                    AssignSystemInfoToActivity(addedItem.GetCurrentValue() as StoppableActivityContract);
                }
                else if (addedItem.Content != null &&
                         addedItem.Content.ComputedValue != null &&
                         addedItem.Content.ComputedValue is StoppableActivityContract)
                {
                    AssignSystemInfoToActivity(addedItem.Content.ComputedValue as StoppableActivityContract);
                }
            }
            else if (e.ItemsRemoved != null)
            {
                foreach (var item in e.ItemsRemoved)
                {
                    if (item.ItemType == typeof(FlowStep))
                    {
                        RemoveActivityData(item.Content.ComputedValue as System.Activities.Activity);
                    }
                    else
                    {
                        RemoveActivityData(item.GetCurrentValue() as System.Activities.Activity);
                    }
                }
            }
            else if (e.PropertiesChanged != null)
            {
                var changedItem = e.PropertiesChanged.First();

                if (changedItem != null &&
                                        changedItem.ComputedValue != null &&
                                        changedItem.ComputedValue is StoppableActivityContract)
                {
                    AssignSystemInfoToActivity(changedItem.ComputedValue as StoppableActivityContract);
                }
                else
                {
                    if (changedItem.Parent.GetCurrentValue() is ActivityBuilder)
                    {
                        if (changedItem.Value == null && changedItem.Name.ToLower() == "implementation")
                        {
                            //HumanStepPermissions.Clear();
                            CustomActivitySettings.Clear();
                        }
                    }
                }
            }

            IsChanged = true;
        }

        private void AssignSystemInfoToActivity(StoppableActivityContract activity)
        {
            activity.User = LoginUser;
            activity.ObjectID = BatchType.Id;
            activity.UniqueID = Guid.NewGuid();
        }

        private void RemoveActivityData(System.Activities.Activity activity)
        {
            if (activity is StoppableActivityContract)
            {
                //int actIndex = HumanStepPermissions.FindIndex(a => a.HumanStepId == ((StoppableActivityContract)activity).UniqueID);

                //if (actIndex >= 0)
                //{
                //    HumanStepPermissions.RemoveAt(actIndex);
                //}

                int customActIndex = CustomActivitySettings.FindIndex(p => p.ActivityId == ((StoppableActivityContract)activity).UniqueID);

                if (customActIndex >= 0)
                {
                    CustomActivitySettings.RemoveAt(customActIndex);
                }

            }
            else if (activity is Sequence)
            {
                foreach (System.Activities.Activity act in (activity as Sequence).Activities)
                {
                    RemoveActivityData(act);
                }
            }
            else if (activity is Flowchart)
            {
                foreach (FlowStep step in (activity as Flowchart).Nodes)
                {
                    RemoveActivityData(step.Action);
                }
            }
        }

        private string SaveWorkflowToTempFile(string workflowXaml)
        {
            var path = Path.GetTempFileName();
            using (StreamWriter writer = File.CreateText(path))
            {
                writer.Write(workflowXaml);
                return path;
            }
        }
    }

}
