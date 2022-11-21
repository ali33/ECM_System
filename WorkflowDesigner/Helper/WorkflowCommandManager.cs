using System;
using System.Windows.Input;

namespace Ecm.WorkflowDesigner.Helper
{
    internal class WorkflowCommandManager
    {
        public WorkflowCommandManager(DesignerContainer designerContainer)
        {
            DesignerContainer = designerContainer;
            Initialize();
        }

        public DesignerContainer DesignerContainer { get; private set; }

        public RoutedCommand SaveCommand;

        private void Initialize()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            SaveCommand = new RoutedCommand("SaveCommand", typeof(DesignerContainer), gesture);
            var commandBinding = new CommandBinding(SaveCommand, Save, CanSave);
            DesignerContainer.CommandBindings.Add(commandBinding);
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DesignerContainer.IsChanged && DesignerContainer.ValidateWorkflow() && DesignerContainer.;
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DesignerContainer.Save();
            }
            catch (Exception ex)
            {
                DesignerContainer.HandleExceptionAction(ex);
            }
        }
    }
}
