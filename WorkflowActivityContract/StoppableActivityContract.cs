using System;
using System.Activities;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.Contract
{
    [InheritedExport(typeof(StoppableActivityContract))]
    public abstract class StoppableActivityContract : NativeActivity
    {
        #region design time properties

        [Browsable(false)]
        public User User { get; set; }

        //[Browsable(false)]
        //public string UserName { get; set; }

        //[Browsable(false)]
        //public string PasswordHash { get; set; }

        [Browsable(false)]
        public Guid UniqueID { get; set; }

        [Browsable(false)]
        public object ObjectID { get; set; }

        //[Editor(typeof(BoundedTextEditor), typeof(PropertyValueEditor))]
        //public string Description { get; set; }

        #endregion

        protected sealed override void Execute(NativeActivityContext context)
        {
            ExecutionBody(context);

            string bookmarkName;

            if (BookmarkWorkflowAfterExecution)
            {
                bookmarkName = UniqueID.ToString();// +System.Char.ConvertFromUtf32(164) + Description;
            }
            else
            {
                // if we change this value (AUTORESUME) then we have to update the new value at WorkflowManager\WorkflowManager\WorkflowController_WorkflowBookmarked

                bookmarkName = "AUTORESUME";
            }

            context.CreateBookmark(bookmarkName, WorkflowResumeBody);
        }

        protected sealed override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected virtual bool BookmarkWorkflowAfterExecution
        {
            get
            {
                return false;
            }
        }

        protected virtual void WorkflowResumeBody(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            var a = obj;
        }
        
        protected abstract void ExecutionBody(NativeActivityContext context);

        protected WorkflowRuntimeData GetWorkflowRuntimeData(NativeActivityContext context)
        {
            if (_runtimeData == null)
            {
                _runtimeData = ContractHelper.GetRuntimeData(context);
            }
            return _runtimeData;
        }

        private WorkflowRuntimeData _runtimeData;
    }

    //public class BoundedTextEditor : PropertyValueEditor
    //{
    //    public BoundedTextEditor()
    //    {
    //        this.InlineEditorTemplate = new DataTemplate();

    //        FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            
    //        FrameworkElementFactory textb = new FrameworkElementFactory(typeof(TextBox));
    //        Binding textBinding = new Binding("Value");
    //        textb.SetValue(TextBox.TextProperty, textBinding);
    //        textb.SetValue(TextBox.MaxLengthProperty, 5);

    //        stack.AppendChild(textb);

    //        this.InlineEditorTemplate.VisualTree = stack;
    //    }

    //}
}
