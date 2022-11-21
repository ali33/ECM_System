using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.DurableInstancing;
using System.Xaml;

namespace Ecm.Workflow.WorkflowCore
{
    public delegate void CoreWorkflowCompletedHandler(Guid workflowID, bool hasError);

    public delegate void CoreWorkflowBookmarkedHandler(Guid workflowID, string activityName, string bookmarkName);

    public delegate void CoreWorkflowAbortedHandler(Guid workflowID, Exception reason);

    public delegate void CoreWorkflowUnloadedHandler(Guid workflowID);

    public class Core
    {
        private readonly Activity _workflow;
        private Guid _workflowInstanceID;
        private string _bookmarkName;

        public Core(string xamlWorkflowDefinition)
        {
            if (string.IsNullOrEmpty(xamlWorkflowDefinition))
            {
                throw new ArgumentNullException(xamlWorkflowDefinition);
            }

            try
            {
                _workflow = LoadWorkflow(xamlWorkflowDefinition);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while loading workflow definition.", ex);
            }
        }

        public event CoreWorkflowBookmarkedHandler WorkflowBookmarked;

        public event CoreWorkflowCompletedHandler WorkflowCompleted;

        public event CoreWorkflowAbortedHandler WorkflowAborted;

        public event CoreWorkflowUnloadedHandler WorkflowUnloaded;

        public object[] Extensions { get; set; }

        public InstanceStore WorkflowInstanceStore { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public object ResumeData { get; set; }

        public void Run()
        {
            WorkflowApplication app = CreateWorkflowInstance(_workflow, true);

            _workflowInstanceID = app.Id;
            app.Run();
        }

        public void Resume(Guid workflowInstanceID, string bookmarkName)
        {
            _workflowInstanceID = workflowInstanceID;

            if (_workflowInstanceID == Guid.Empty)
            {
                throw new ArgumentNullException("Resuming workflow requires workflow instance ID.");
            }

            _bookmarkName = bookmarkName;
            if (string.IsNullOrEmpty(_bookmarkName))
            {
                throw new ArgumentNullException("Resuming workflow requires bookmark name.");
            }

            // note that when we reload the workflow then we have to create the workflow without parameter
            WorkflowApplication app = CreateWorkflowInstance(_workflow, false);
            app.Load(_workflowInstanceID);

            

            app.ResumeBookmark(_bookmarkName, ResumeData);
        }

        private WorkflowApplication CreateWorkflowInstance(Activity workflow, bool includeParameter)
        {
            WorkflowApplication app;

            if (includeParameter)
            {
                app = new WorkflowApplication(workflow, Parameters);
            }
            else
            {
                app = new WorkflowApplication(workflow);
            }

            app.InstanceStore = WorkflowInstanceStore;

            if (Extensions != null && Extensions.Length > 0)
            {
                foreach (object item in Extensions)
                {
                    app.Extensions.Add(item);
                }
            }

            // note that we should not handle the app.Aborted because it makes the mainthread(WCF thread) be killed
            app.OnUnhandledException = delegate(WorkflowApplicationUnhandledExceptionEventArgs e)
            {
                if (WorkflowAborted != null)
                {
                    WorkflowAborted(e.InstanceId, e.UnhandledException);
                }
                return UnhandledExceptionAction.Abort;
            };

            app.Completed = delegate(WorkflowApplicationCompletedEventArgs e)
            {
                // Get the output arguments here by calling IDictionary<string, object> result = e.Outputs; 

                if (WorkflowCompleted != null)
                {
                    WorkflowCompleted(_workflowInstanceID, e.CompletionState == ActivityInstanceState.Faulted);
                }
            };

            app.PersistableIdle = delegate(WorkflowApplicationIdleEventArgs e)
            {
                if (e.Bookmarks.Count > 0)
                {
                    _bookmarkName = e.Bookmarks[0].BookmarkName;

                    if (WorkflowBookmarked != null)
                    {
                        WorkflowBookmarked(_workflowInstanceID, e.Bookmarks[0].OwnerDisplayName, _bookmarkName);
                    }
                }

                // Uunload workflow from the memory after it is idled and has been persisted
                return PersistableIdleAction.Unload;
            };

            app.Unloaded = delegate(WorkflowApplicationEventArgs e)
            {
                if (WorkflowUnloaded != null)
                {
                    WorkflowUnloaded(e.InstanceId);
                }
            };

            return app;
        }

        private Activity LoadWorkflow(string workflowXAML)
        {
            XamlXmlReaderSettings readerSettings = new XamlXmlReaderSettings { LocalAssembly = Assembly.GetExecutingAssembly() };

            using (XamlXmlReader reader = new XamlXmlReader(new StringReader(workflowXAML), readerSettings))
            {
                Activity workflow = ActivityXamlServices.Load(reader);
                return workflow;
            }
        }
    }
}