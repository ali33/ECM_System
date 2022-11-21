using System;
using System.Activities.Tracking;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;

namespace Ecm.Workflow.WorkflowExtension
{
    public class SqlTrackingParticipant : TrackingParticipant
    {
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                _connectionString = GetConnectionString(value);
            }
        }

        internal static String GetConnectionString(String rawConnectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                                                     {
                                                         ConnectionString = rawConnectionString,
                                                         AsynchronousProcessing = true,
                                                         Enlist = false
                                                     };
            return builder.ToString();
        }

        // The track method is called when a tracking record is emitted by the workflow runtime
        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            TrackingCommand trackingCommand = new TrackingCommand(record);

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        foreach (SqlParameter parameter in trackingCommand.Parameters)
                        {
                            sqlCommand.Parameters.Add(parameter);
                        }

                        int sqlTimeout = (int)timeout.TotalSeconds;
                        if (sqlTimeout > 0)
                        {
                            sqlCommand.CommandTimeout = sqlTimeout;
                        }

                        // Wrap the command in a transaction since the command may contain multiple statements. 
                        sqlCommand.Transaction = sqlConnection.BeginTransaction();
                        sqlCommand.CommandText = trackingCommand.Procedure;
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.Transaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "SqlTrackingParticipant: Exception in Track {0}", e.StackTrace));
            }
        }

        private string _connectionString;
    }

    internal class TrackingCommand
    {
        public TrackingCommand()
        {
            _parameters = new List<SqlParameter>();
            _dataSerializer = new NetDataContractSerializer();
        }

        public TrackingCommand(TrackingRecord record)
            : this()
        {
            ActivityScheduledRecord activityScheduledRecord = record as ActivityScheduledRecord;
            if (activityScheduledRecord != null)
            {
                CreateTrackingCommand(activityScheduledRecord);
                return;
            }

            CancelRequestedRecord cancelRequestedRecord = record as CancelRequestedRecord;
            if (cancelRequestedRecord != null)
            {
                CreateTrackingCommand(cancelRequestedRecord);
                return;
            }

            FaultPropagationRecord faultPropagationRecord = record as FaultPropagationRecord;
            if (faultPropagationRecord != null)
            {
                CreateTrackingCommand(faultPropagationRecord);
                return;
            }

            ActivityStateRecord activityStateRecord = record as ActivityStateRecord;
            if (activityStateRecord != null)
            {
                CreateTrackingCommand(activityStateRecord);
                return;
            }

            WorkflowInstanceRecord workflowInstanceRecord = record as WorkflowInstanceRecord;
            if (workflowInstanceRecord != null)
            {
                CreateTrackingCommand(workflowInstanceRecord);
                return;
            }

            BookmarkResumptionRecord bookmarkResumptionRecord = record as BookmarkResumptionRecord;
            if (bookmarkResumptionRecord != null)
            {
                CreateTrackingCommand(bookmarkResumptionRecord);
                return;
            }

            CustomTrackingRecord customTrackingRecord = record as CustomTrackingRecord;
            if (customTrackingRecord != null)
            {
                CreateTrackingCommand(customTrackingRecord);
                return;
            }
        }

        public SqlParameter[] Parameters
        {
            get
            {
                return _parameters.ToArray();
            }
        }

        public string Procedure { get; protected set; }

        private void CreateTrackingCommand(WorkflowInstanceRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertWorkflowInstanceEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@WorkflowActivityDefinition", SqlDbType.NVarChar, 256, record.ActivityDefinitionId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@State", SqlDbType.NVarChar, 128, record.State));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));

            if (record is WorkflowInstanceUnhandledExceptionRecord)
            {
                _parameters.Add(CreateTrackingCommandParameter("@ExceptionDetails", SqlDbType.NVarChar, null,
                                ((WorkflowInstanceUnhandledExceptionRecord)record).UnhandledException.ToString()));
            }
            else
            {
                _parameters.Add(CreateTrackingCommandParameter("@ExceptionDetails", SqlDbType.NVarChar, null, DBNull.Value));
            }

            if (record is WorkflowInstanceTerminatedRecord)
            {
                _parameters.Add(CreateTrackingCommandParameter("@Reason", SqlDbType.NVarChar, null, ((WorkflowInstanceTerminatedRecord)record).Reason));
            }
            else
            {
                if (record is WorkflowInstanceAbortedRecord)
                {
                    _parameters.Add(CreateTrackingCommandParameter("@Reason", SqlDbType.NVarChar, null, ((WorkflowInstanceAbortedRecord)record).Reason));
                }
                else if (record is WorkflowInstanceSuspendedRecord)
                {
                    _parameters.Add(CreateTrackingCommandParameter("@Reason", SqlDbType.NVarChar, null, ((WorkflowInstanceSuspendedRecord)record).Reason));
                }
                else
                {
                    _parameters.Add(CreateTrackingCommandParameter("@Reason", SqlDbType.NVarChar, null, DBNull.Value));
                }
            }
        }

        private void CreateTrackingCommand(ActivityStateRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertActivityInstanceEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@State", SqlDbType.NVarChar, 128, record.State));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityRecordType", SqlDbType.NVarChar, 128, "ActivityState"));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityName", SqlDbType.NVarChar, 256, record.Activity.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityId", SqlDbType.NVarChar, 256, record.Activity.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityInstanceId", SqlDbType.NVarChar, 256, record.Activity.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityType", SqlDbType.NVarChar, 2048, record.Activity.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@ArgumentsXml", SqlDbType.NVarChar, null, SerializeData(record.Arguments)));
            _parameters.Add(CreateTrackingCommandParameter("@VariablesXml", SqlDbType.NVarChar, null, SerializeData(record.Variables)));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));
        }

        private void CreateTrackingCommand(ActivityScheduledRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertActivityScheduledEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityRecordType", SqlDbType.NVarChar, 128, "ActivityScheduled"));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityName", SqlDbType.NVarChar, 1024, record.Activity == null ? null : record.Activity.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityId", SqlDbType.NVarChar, 256, record.Activity == null ? null : record.Activity.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityInstanceId", SqlDbType.NVarChar, 256,
                                                           record.Activity == null ? null : record.Activity.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityType", SqlDbType.NVarChar, 2048, record.Activity == null ? null : record.Activity.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityName", SqlDbType.NVarChar, 1024, record.Child.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityId", SqlDbType.NVarChar, 256, record.Child.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityInstanceId", SqlDbType.NVarChar, 256, record.Child.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityType", SqlDbType.NVarChar, 2048, record.Child.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));
        }

        private void CreateTrackingCommand(CancelRequestedRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertActivityCancelRequestedEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityRecordType", SqlDbType.NVarChar, 128, "ActivityScheduled"));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityName", SqlDbType.NVarChar, 1024,
                                                           record.Activity == null ? string.Empty : record.Activity.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityId", SqlDbType.NVarChar, 256, record.Activity == null ? string.Empty : record.Activity.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityInstanceId", SqlDbType.NVarChar, 256,
                                                           record.Activity == null ? string.Empty : record.Activity.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityType", SqlDbType.NVarChar, 2048,
                                                           record.Activity == null ? string.Empty : record.Activity.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityName", SqlDbType.NVarChar, 1024, record.Child.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityId", SqlDbType.NVarChar, 256, record.Child.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityInstanceId", SqlDbType.NVarChar, 256, record.Child.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ChildActivityType", SqlDbType.NVarChar, 2048, record.Child.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));
        }

        private void CreateTrackingCommand(FaultPropagationRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertFaultPropagationEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityRecordType", SqlDbType.NVarChar, 128, "FaultPropagation"));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityName", SqlDbType.NVarChar, 256, record.FaultSource.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityId", SqlDbType.NVarChar, 256, record.FaultSource.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityInstanceId", SqlDbType.NVarChar, 256, record.FaultSource.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityType", SqlDbType.NVarChar, 2048, record.FaultSource.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@FaultDetails", SqlDbType.NVarChar, null, record.Fault.ToString()));
            _parameters.Add(CreateTrackingCommandParameter("@FaultHandlerActivityName", SqlDbType.NVarChar, null,
                                                           record.FaultHandler == null ? null : record.FaultHandler.Name));
            _parameters.Add(CreateTrackingCommandParameter("@FaultHandlerActivityId", SqlDbType.NVarChar, 256,
                                                           record.FaultHandler == null ? null : record.FaultHandler.Id));
            _parameters.Add(CreateTrackingCommandParameter("@FaultHandlerActivityInstanceId", SqlDbType.NVarChar, 256,
                                                           record.FaultHandler == null ? null : record.FaultHandler.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@FaultHandlerActivityType", SqlDbType.NVarChar, 2048,
                                                           record.FaultHandler == null ? null : record.FaultHandler.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));
        }

        private void CreateTrackingCommand(BookmarkResumptionRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertBookmarkResumptionEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@BookmarkName", SqlDbType.NVarChar, 1024, record.BookmarkName));
            _parameters.Add(CreateTrackingCommandParameter("@BookmarkScope", SqlDbType.UniqueIdentifier, null, record.BookmarkScope));
            _parameters.Add(CreateTrackingCommandParameter("@OwnerActivityName", SqlDbType.NVarChar, 256, record.Owner.Name));
            _parameters.Add(CreateTrackingCommandParameter("@OwnerActivityId", SqlDbType.NVarChar, 256, record.Owner.Id));
            _parameters.Add(CreateTrackingCommandParameter("@OwnerActivityInstanceId", SqlDbType.NVarChar, 256, record.Owner.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@OwnerActivityType", SqlDbType.NVarChar, 2048, record.Owner.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));
        }

        private void CreateTrackingCommand(CustomTrackingRecord record)
        {
            Procedure = "[Workflow.Tracking].[InsertCustomTrackingEvent]";

            _parameters.Add(CreateTrackingCommandParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier, null, record.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@RecordNumber", SqlDbType.BigInt, null, record.RecordNumber));
            _parameters.Add(CreateTrackingCommandParameter("@TraceLevelId", SqlDbType.TinyInt, null, record.Level));
            _parameters.Add(CreateTrackingCommandParameter("@CustomRecordName", SqlDbType.NVarChar, 1024, record.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityName", SqlDbType.NVarChar, 256, record.Activity.Name));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityId", SqlDbType.NVarChar, 256, record.Activity.Id));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityInstanceId", SqlDbType.NVarChar, 256, record.Activity.InstanceId));
            _parameters.Add(CreateTrackingCommandParameter("@ActivityType", SqlDbType.NVarChar, 2048, record.Activity.TypeName));
            _parameters.Add(CreateTrackingCommandParameter("@CustomRecordDataXml", SqlDbType.NVarChar, null, SerializeData(record.Data)));
            _parameters.Add(CreateTrackingCommandParameter("@AnnotationsXml", SqlDbType.NVarChar, null, SerializeData(record.Annotations)));
            _parameters.Add(CreateTrackingCommandParameter("@TimeCreated", SqlDbType.DateTime, null, record.EventTime));
        }

        private SqlParameter CreateTrackingCommandParameter(string name, SqlDbType type, int? size, object value)
        {
            SqlParameter parameter;

            if (size.HasValue)
            {
                parameter = new SqlParameter(name, type, size.Value);
            }
            else
            {
                parameter = new SqlParameter(name, type);
            }

            if (value == null && type != SqlDbType.Structured)
            {
                parameter.IsNullable = true;
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }

            return parameter;
        }

        private string SerializeData<TKey, TValue>(IDictionary<TKey, TValue> data)
        {
            if (data.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                if (_dataSerializer == null)
                {
                    _dataSerializer = new NetDataContractSerializer();
                }
                try
                {
                    _dataSerializer.WriteObject(writer, data);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Exception during serialization of data: {0}", e.Message));
                }

                writer.Flush();
                return builder.ToString();
            }
        }

        private readonly List<SqlParameter> _parameters;
        private NetDataContractSerializer _dataSerializer;
    }
}