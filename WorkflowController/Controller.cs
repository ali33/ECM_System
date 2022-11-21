using System;
using System.Activities.DurableInstancing;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Runtime.DurableInstancing;

using Ecm.CaptureDomain;
using Ecm.Workflow.WorkflowCore;
using Ecm.Workflow.WorkflowExtension;

namespace Ecm.Workflow.WorkflowController
{
    public delegate void WorkflowBookmarkedHandler(WorkflowRuntimeData runtimeData, Guid workflowId, string activityName, string bookmarkName, bool hasError);

    public delegate void WorkflowAbortedHandler(WorkflowRuntimeData runtimeData, Guid workflowId, Exception reason);

    public delegate void WorkflowCompletedHandler(WorkflowRuntimeData runtimeData, Guid workflowId, bool hasError);

    public class Controller
    {

        private readonly WorkflowRuntimeData _runtimeData;
        private readonly Core _wfCore;

        private readonly string _workflowDefinition;
        private bool _hasError;
        private string _lastErrorMessage;
        private bool _autoResume;
        private string _lastBookmarkName;
        private object _resumeData;
        private string _connectionString;
        private static InstanceStore _staticInstanceStore;
        private static InstanceStore _staticInstanceStoreResume;

        public Controller(WorkflowRuntimeData runtimeData,
                          string workflowDefinition,
                          string connectionString)
        {
            _runtimeData = runtimeData;
            _workflowDefinition = workflowDefinition;
            _connectionString = connectionString;
            _hasError = false;

            try
            {
                _wfCore = new Core(_workflowDefinition);
            }
            catch (Exception ex)
            {
                _hasError = true;
                LogError(ex);
            }

            if (!_hasError)
            {
                _wfCore.WorkflowCompleted += WorkflowController_WorkflowCompleted;
                _wfCore.WorkflowAborted += WorkflowController_WorkflowAborted;
                _wfCore.WorkflowBookmarked += WorkflowController_WorkflowBookmarked;
                _wfCore.WorkflowUnloaded += WorkflowController_WorkflowUnloaded;

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(WorkflowConstant.RuntimeData, runtimeData); // match the argument name in workflow designer
                _wfCore.Parameters = parameters;

                _wfCore.Extensions = new[] { CreateTrackingExtension() };
            }
        }

        public void StartWorkflow()
        {
            _hasError = false;

            try
            {
                if (_staticInstanceStore == null)
                {
                    _staticInstanceStore = new SqlWorkflowInstanceStore(_connectionString);
                    InstanceHandle instanceHandle = _staticInstanceStore.CreateInstanceHandle();
                    InstanceView view = _staticInstanceStore.Execute(instanceHandle, new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                    _staticInstanceStore.DefaultInstanceOwner = view.InstanceOwner;
                }
                _wfCore.WorkflowInstanceStore = _staticInstanceStore;

                _wfCore.Run();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void AutoResumeLastStop(Guid workflowInstanceId)
        {
            ResumeWorkflow(workflowInstanceId, _lastBookmarkName);
            _autoResume = false;
            _lastBookmarkName = string.Empty;
        }

        public void ResumeWorkflow(Guid workflowInstanceId, string bookmarkName)
        {
            _hasError = false;
            try
            {
                if (_staticInstanceStoreResume == null)
                {
                    _staticInstanceStoreResume = new SqlWorkflowInstanceStore(_connectionString);
                    InstanceHandle instanceHandle = _staticInstanceStoreResume.CreateInstanceHandle();
                    InstanceView view = _staticInstanceStoreResume.Execute(instanceHandle, new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                    _staticInstanceStoreResume.DefaultInstanceOwner = view.InstanceOwner;
                }
                _wfCore.WorkflowInstanceStore = _staticInstanceStoreResume;
                _wfCore.ResumeData = _resumeData;
                _wfCore.Resume(workflowInstanceId, bookmarkName);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public object ResumeData
        {
            set
            {
                _resumeData = value;
            }
        }

        public bool HasError
        {
            get
            {
                return _hasError;
            }
        }

        public string LastErrorMessage
        {
            get
            {
                return _lastErrorMessage;
            }
        }

        public WorkflowBookmarkedHandler WorkflowBookmarked;
        public WorkflowCompletedHandler WorkflowCompleted;
        public WorkflowAbortedHandler WorkflowAborted;

        private object CreateTrackingExtension()
        {
            SqlTrackingParticipant sqltrackingExtension = new SqlTrackingParticipant { ConnectionString = _connectionString };
            TrackingProfile profile = new TrackingProfile { ActivityDefinitionId = "*" };

            WorkflowInstanceQuery wfQuery = new WorkflowInstanceQuery { States = { "*" } };
            ActivityStateQuery activityQuery = new ActivityStateQuery { States = { "*" } };
            ActivityScheduledQuery activityScheduledQuery = new ActivityScheduledQuery { ActivityName = "*", ChildActivityName = "*" };
            FaultPropagationQuery faultPropagationQuery = new FaultPropagationQuery { FaultSourceActivityName = "*", FaultHandlerActivityName = "*" };
            BookmarkResumptionQuery bookmarkResumptionQuery = new BookmarkResumptionQuery { Name = "*" };

            profile.Queries.Add(wfQuery);
            profile.Queries.Add(activityQuery);
            profile.Queries.Add(activityScheduledQuery);
            profile.Queries.Add(faultPropagationQuery);
            profile.Queries.Add(bookmarkResumptionQuery);

            sqltrackingExtension.TrackingProfile = profile;

            return sqltrackingExtension;
        }

        private void WorkflowController_WorkflowAborted(Guid workflowId, Exception reason)
        {
            _hasError = true;
            _lastErrorMessage = reason.Message;
            if (WorkflowAborted != null)
            {
                WorkflowAborted(_runtimeData, workflowId, reason);
            }
        }

        private void WorkflowController_WorkflowBookmarked(Guid workflowId, string activityName, string bookmarkName)
        {
            if (WorkflowBookmarked != null)
            {
                WorkflowBookmarked(_runtimeData, workflowId, activityName, bookmarkName, HasError);
            }

            if (bookmarkName.ToUpper() == "AUTORESUME")
            {
                _lastBookmarkName = bookmarkName;
                _autoResume = true;
            }
            else
            {
                _autoResume = false;
            }
        }

        private void WorkflowController_WorkflowCompleted(Guid workflowId, bool hasError)
        {
            _hasError = hasError;

            if (WorkflowCompleted != null)
            {
                WorkflowCompleted(_runtimeData, workflowId, HasError);
            }
        }

        private void WorkflowController_WorkflowUnloaded(Guid workflowId)
        {
            if (_autoResume && !string.IsNullOrEmpty(_lastBookmarkName))
            {
                AutoResumeLastStop(workflowId);
            }
        }

        private void LogError(Exception ex)
        {
            _hasError = true;
            _lastErrorMessage = ex.Message;
        }
    }
}