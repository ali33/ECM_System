-- Copyright (c) Microsoft Corporation.  All rights reserved.

--==========================================================================
-- Sample Tracking Database
--==========================================================================

set ansi_nulls on;
go

set quoted_identifier on;
go

if not exists (select * from sys.schemas where name = N'Workflow.Tracking')
	exec ('create schema [Workflow.Tracking]')
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[WorkflowInstanceEventsTable]') and type in (N'U'))
	drop table [Workflow.Tracking].[WorkflowInstanceEventsTable]
go

create table [Workflow.Tracking].[WorkflowInstanceEventsTable]
(
	[Id] int identity(1,1) not null,
	[WorkflowInstanceId] uniqueidentifier not null,
	[WorkflowActivityDefinition] nvarchar(256) null,
	[RecordNumber] bigint not null,
	[State] nvarchar(128) null,
	[TraceLevelId] tinyint null,
	[Reason] nvarchar(2048) null,
	[ExceptionDetails] nvarchar (max) null,
	[SerializedAnnotations] nvarchar(max) null,
	[TimeCreated] [datetime] not null,
	constraint [PK_WorkflowInstanceEventsTable_Id] primary key ([Id]),
);

go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[ActivityInstanceEventsTable]') and type in (N'U'))
	drop table [Workflow.Tracking].[ActivityInstanceEventsTable]
go

create table [Workflow.Tracking].[ActivityInstanceEventsTable]
(
	[Id] int identity(1,1) not null,
	[WorkflowInstanceId] uniqueidentifier not null,
	[RecordNumber] bigint not null,
	[State] nvarchar(128) null,
	[TraceLevelId] tinyint null,
	[ActivityRecordType] nvarchar(128) not null,
	[ActivityName] nvarchar(1024) null,
	[ActivityId] nvarchar(256) null,
	[ActivityInstanceId] nvarchar (256) null,
	[ActivityType] nvarchar(2048) null,
	[SerializedArguments] nvarchar(max) null,
	[SerializedVariables] nvarchar(max) null,
        [SerializedAnnotations] nvarchar(max) null,
	[TimeCreated] datetime not null,
	
	constraint [PK_ActivityInstanceEventsTable_Id] primary key ([Id]),
);

go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[ExtendedActivityEventsTable]') and type in (N'U'))
	drop table [Workflow.Tracking].[ExtendedActivityEventsTable]
go

create table [Workflow.Tracking].[ExtendedActivityEventsTable]
(
	[Id] int identity(1,1) not null,
	[WorkflowInstanceId] uniqueidentifier not null,
	[RecordNumber] bigint null,
	[TraceLevelId] tinyint null,
	[ActivityRecordType] nvarchar(128) not null,
	[ActivityName] nvarchar(1024) null,
	[ActivityId] nvarchar(256) null,
	[ActivityInstanceId] nvarchar (256) null,
	[ActivityType] nvarchar(2048) null,
	[ChildActivityName] nvarchar(1024) null,
	[ChildActivityId] nvarchar(256) null,
	[ChildActivityInstanceId] nvarchar (256) null,
	[ChildActivityType] nvarchar(2048) null,
	[FaultDetails]	    nvarchar(max) null,
	[FaultHandlerActivityName] nvarchar(1024) null,
	[FaultHandlerActivityId] nvarchar(256) null,
	[FaultHandlerActivityInstanceId] nvarchar (256) null,
	[FaultHandlerActivityType] nvarchar(2048) null,
	[SerializedAnnotations] nvarchar(max) null,
	[TimeCreated] datetime not null,
	
	constraint [PK_ExtendedActivityInstanceEventsTable_Id] primary key ([Id]),
);

go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[BookmarkResumptionEventsTable]') and type in (N'U'))
	drop table [Workflow.Tracking].[BookmarkResumptionEventsTable]
go

create table [Workflow.Tracking].[BookmarkResumptionEventsTable]
( 
    [Id] int identity(1,1) not null,
    [WorkflowInstanceId] uniqueidentifier not null,
    [RecordNumber] bigint null,
    [TraceLevelId] tinyint null,
    [BookmarkName] nvarchar(1024),
    [BookmarkScope] uniqueidentifier null,
    [OwnerActivityName] nvarchar(256) null,
    [OwnerActivityId] nvarchar(256) null,
    [OwnerActivityInstanceId] nvarchar(256) null,
    [OwnerActivityType] nvarchar(256) null,
    [SerializedAnnotations] nvarchar(max) null,
    [TimeCreated] datetime not null,

    constraint [PK_BookmarkResumptionkEventsTable_Id] primary key ([Id])
);
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[CustomTrackingEventsTable]') and type in (N'U'))
	drop table [Workflow.Tracking].[CustomTrackingEventsTable]
go

create table [Workflow.Tracking].[CustomTrackingEventsTable]
(
	[Id] int identity(1,1) not null,
	[WorkflowInstanceId] uniqueidentifier not null,
	[RecordNumber] bigint null,
	[TraceLevelId] tinyint null,
	[CustomRecordName] nvarchar(2048) null,
	[ActivityName] nvarchar(2048) null,
	[ActivityId] nvarchar (256) null,
	[ActivityInstanceId] nvarchar (256) null,
	[ActivityType] nvarchar (256) null,
	[SerializedData] nvarchar(max) null,
	[SerializedAnnotations] nvarchar(max) null,
	[TimeCreated] [datetime] not null,

	constraint [PK_CustomTrackingEventsTable_Id] primary key ([Id]),
);

go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertWorkflowInstanceEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertWorkflowInstanceEvent]
go

create procedure [Workflow.Tracking].[InsertWorkflowInstanceEvent]	
(	
													 @WorkflowInstanceId				uniqueidentifier	
													,@WorkflowActivityDefinition		nvarchar(256)
													,@RecordNumber					bigint
													,@State						nvarchar(128)
													,@TraceLevelId                      	tinyint
													,@Reason						nvarchar(max)
													,@ExceptionDetails				nvarchar(max)
													,@AnnotationsXml				nvarchar(2048)
													,@TimeCreated                       datetime
																			
)
as
 begin
	set nocount on


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret			smallint


	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[WorkflowInstanceEventsTable] (
				[WorkflowInstanceId]
				,[WorkflowActivityDefinition]
				,[RecordNumber]
				,[State]
				,[TraceLevelId]
				,[ExceptionDetails]
				,[Reason]
				,[SerializedAnnotations] 
				,[TimeCreated]
		) values (
				@WorkflowInstanceId
				,@WorkflowActivityDefinition
				,@RecordNumber
				,@State
				,@TraceLevelId
				,@ExceptionDetails
				,@Reason
				,@AnnotationsXml
				,@TimeCreated
				
		)
		if (@@ROWCOUNT <> 1)
        begin
         --   exec @errorMsg=[System.Globalization].[SessionsString] '4CED8A16-944D-4340-BC6A-09E7C3AC2ADF';
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

 end
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertActivityInstanceEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertActivityInstanceEvent]
go

create procedure [Workflow.Tracking].[InsertActivityInstanceEvent]	
(	
													 @WorkflowInstanceId	uniqueidentifier
													 ,@RecordNumber         bigint
													 ,@State                nvarchar(128)
													 ,@TraceLevelId         tinyint
	                                                 						 ,@ActivityRecordType   nvarchar(128)
	                                                 						 ,@ActivityName         nvarchar(1024)
													 ,@ActivityId           nvarchar(256)
													 ,@ActivityInstanceId   nvarchar (256)
													 ,@ActivityType         nvarchar (2048)
													 ,@ArgumentsXml	        nvarchar(max)
													 ,@VariablesXml	        nvarchar(max)
													 ,@AnnotationsXml       nvarchar(max)
													 ,@TimeCreated          datetime												 
																			
)
as
 begin
	set NOCOUNT ON


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret		int



	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[ActivityInstanceEventsTable] (
				[WorkflowInstanceId]
				,[RecordNumber]
				,[State]
				,[TraceLevelId]
				,[ActivityRecordType] 
	            		,[ActivityName] 
	            		,[ActivityId] 
	            		,[ActivityInstanceId]
	            		,[ActivityType]
                    		,[SerializedArguments]
	            		,[SerializedVariables]
	            		,[SerializedAnnotations]
				,[TimeCreated]
		) values (
				 @WorkflowInstanceId
	            		,@RecordNumber         
				,@State                
				,@TraceLevelId         
	            		,@ActivityRecordType   
	            		,@ActivityName         
	            		,@ActivityId           
			    	,@ActivityInstanceId   
			    	,@ActivityType
			    	,@ArgumentsXml
			    	,@VariablesXml
			    	,@AnnotationsXml  
			    	,@TimeCreated 
			       
				
		)
		
		if (@@ROWCOUNT <> 1)
        begin
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

 end
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertActivityScheduledEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertActivityScheduledEvent]
go

create procedure [Workflow.Tracking].[InsertActivityScheduledEvent]	
(	
													 @WorkflowInstanceId	uniqueidentifier
													 ,@RecordNumber         bigint
													 ,@TraceLevelId         tinyint
	                                                 						 ,@ActivityRecordType   nvarchar(128)
	                                                 						 ,@ActivityName         nvarchar(1024)
													 ,@ActivityId           nvarchar(256)
													 ,@ActivityInstanceId   nvarchar (256)
													 ,@ActivityType         nvarchar (2048)
													 ,@ChildActivityName         nvarchar(1024)
													 ,@ChildActivityId           nvarchar(256)
													 ,@ChildActivityInstanceId   nvarchar (256)
													 ,@ChildActivityType         nvarchar (2048)
													 ,@AnnotationsXml			nvarchar(max)
													 ,@TimeCreated          datetime
													 
																			
)
as
 begin
	set NOCOUNT ON


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret		int



	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[ExtendedActivityEventsTable] (
				[WorkflowInstanceId]
				,[RecordNumber]
				,[TraceLevelId]
				,[ActivityRecordType] 
	            		,[ActivityName] 
	            		,[ActivityId] 
	            		,[ActivityInstanceId]
	            		,[ActivityType]
				,[ChildActivityName] 
	            		,[ChildActivityId] 
	            		,[ChildActivityInstanceId]
	            		,[ChildActivityType]
	            		,[SerializedAnnotations]
				,[TimeCreated]
		) values (
				 @WorkflowInstanceId
	            		,@RecordNumber                 
				,@TraceLevelId         
	            		,@ActivityRecordType   
	            		,@ActivityName         
	            		,@ActivityId           
			    	,@ActivityInstanceId   
			    	,@ActivityType
			    	,@ChildActivityName         
	            		,@ChildActivityId          
			    	,@ChildActivityInstanceId   
			    	,@ChildActivityType
			    	,@AnnotationsXml     
				,@TimeCreated 
				       
				
		)
		
		if (@@ROWCOUNT <> 1)
        begin
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

 end
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertActivityCancelRequestedEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertActivityCancelRequestedEvent]
go

create procedure [Workflow.Tracking].[InsertActivityCancelRequestedEvent]	
(	
													 @WorkflowInstanceId	uniqueidentifier
													 ,@RecordNumber         bigint
													 ,@TraceLevelId         tinyint
	                                                 						 ,@ActivityRecordType   nvarchar(128)
	                                                 						 ,@ActivityName         nvarchar(1024)
													 ,@ActivityId           nvarchar(256)
													 ,@ActivityInstanceId   nvarchar (256)
													 ,@ActivityType         nvarchar (2048)
													 ,@ChildActivityName         nvarchar(1024)
													 ,@ChildActivityId           nvarchar(256)
													 ,@ChildActivityInstanceId   nvarchar (256)
													 ,@ChildActivityType         nvarchar (2048)
													 ,@AnnotationsXml nvarchar(max)
													 ,@TimeCreated          datetime
													 
																			
)
as
 begin
	set NOCOUNT ON


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret		int



	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[ExtendedActivityEventsTable] (
				[WorkflowInstanceId]
				,[RecordNumber]
				,[TraceLevelId]
				,[ActivityRecordType] 
	           	        ,[ActivityName] 
	            		,[ActivityId] 
	            		,[ActivityInstanceId]
	            		,[ActivityType]
				,[ChildActivityName] 
	            		,[ChildActivityId] 
	            		,[ChildActivityInstanceId]
	            		,[ChildActivityType]
	            		,[SerializedAnnotations]
				,[TimeCreated]
		) values (
				 @WorkflowInstanceId
	            		,@RecordNumber                 
				,@TraceLevelId         
	            		,@ActivityRecordType   
	            		,@ActivityName         
	            		,@ActivityId           
			    	,@ActivityInstanceId   
			    	,@ActivityType
			    	,@ChildActivityName         
	            		,@ChildActivityId          
			    	,@ChildActivityInstanceId   
			    	,@ChildActivityType
			    	,@AnnotationsXml    
				,@TimeCreated 
				       
				
		)
		
		if (@@ROWCOUNT <> 1)
        begin
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

 end
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertFaultPropagationEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertFaultPropagationEvent]
go

create procedure [Workflow.Tracking].[InsertFaultPropagationEvent]	
(	
													 @WorkflowInstanceId	uniqueidentifier
													 ,@RecordNumber         bigint
													 ,@TraceLevelId         tinyint
	                                                 						 ,@ActivityRecordType   nvarchar(128)
	                                                						 ,@ActivityName         nvarchar(1024)
													 ,@ActivityId           nvarchar(256)
													 ,@ActivityInstanceId   nvarchar (256)
													 ,@ActivityType         nvarchar (2048)
													 ,@FaultDetails		nvarchar (max)
													 ,@FaultHandlerActivityName         nvarchar(1024)
													 ,@FaultHandlerActivityId           nvarchar(256)
													 ,@FaultHandlerActivityInstanceId   nvarchar (256)
													 ,@FaultHandlerActivityType         nvarchar (2048)
													 ,@AnnotationsXml nvarchar(max)
													 ,@TimeCreated          datetime
													 
																			
)
as
 begin
	set NOCOUNT ON


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret		int



	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[ExtendedActivityEventsTable] (
				[WorkflowInstanceId]
				,[RecordNumber]
				,[TraceLevelId]
				,[ActivityRecordType] 
	            		,[ActivityName] 
	            		,[ActivityId] 
	            		,[ActivityInstanceId]
	            		,[ActivityType]
				,[FaultDetails]
				,[FaultHandlerActivityName] 
	            		,[FaultHandlerActivityId] 
	            		,[FaultHandlerActivityInstanceId]
	            		,[FaultHandlerActivityType]
	            		,[SerializedAnnotations]
				,[TimeCreated]
		) values (
				 @WorkflowInstanceId
	            		,@RecordNumber                 
				,@TraceLevelId         
	            		,@ActivityRecordType   
	            		,@ActivityName         
	            		,@ActivityId           
			    	,@ActivityInstanceId   
			    	,@ActivityType
				,@FaultDetails
			    	,@FaultHandlerActivityName         
	            		,@FaultHandlerActivityId          
			    	,@FaultHandlerActivityInstanceId   
			    	,@FaultHandlerActivityType
			    	,@AnnotationsXml    
				,@TimeCreated 
				       
				
		)
		
		if (@@ROWCOUNT <> 1)
        begin
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

 end
go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertBookmarkResumptionEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertBookmarkResumptionEvent]
go

create procedure [Workflow.Tracking].[InsertBookmarkResumptionEvent]	
(	
													 @WorkflowInstanceId	uniqueidentifier
													 ,@RecordNumber         bigint
													 ,@TraceLevelId         tinyint
													 ,@BookmarkName         nvarchar(1024)
													 ,@BookmarkScope        uniqueidentifier
	                                                 						 ,@OwnerActivityName         nvarchar(1024)
													 ,@OwnerActivityId           nvarchar(256)
													 ,@OwnerActivityInstanceId   nvarchar (256)
													 ,@OwnerActivityType         nvarchar (2048)
													 ,@AnnotationsXml       nvarchar(max)
													 ,@TimeCreated          datetime
													 
																			
)
as
 begin
	set NOCOUNT ON


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret		int



	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[BookmarkResumptionEventsTable] (
				[WorkflowInstanceId]
				,[RecordNumber]
				,[TraceLevelId]
				,[BookmarkName]
				,[BookmarkScope]
	            		,[OwnerActivityName] 
	            		,[OwnerActivityId] 
	            		,[OwnerActivityInstanceId]
	            		,[OwnerActivityType]
	            		,[SerializedAnnotations]
				,[TimeCreated]
		) values (
				 @WorkflowInstanceId
	            		,@RecordNumber                        
				,@TraceLevelId         
	            		,@BookmarkName
	            		,@BookmarkScope
	            		,@OwnerActivityName         
	            		,@OwnerActivityId           
			    	,@OwnerActivityInstanceId   
			    	,@OwnerActivityType
			    	,@AnnotationsXml
				,@TimeCreated 
				       
				
		)

		if (@@ROWCOUNT <> 1)
        begin
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

end

go

if exists (select * from sys.objects where object_id = object_id(N'[Workflow.Tracking].[InsertCustomTrackingEvent]') and type in (N'P', N'PC'))
	drop procedure [Workflow.Tracking].[InsertCustomTrackingEvent]
go

create procedure [Workflow.Tracking].[InsertCustomTrackingEvent]	
(	
													 @WorkflowInstanceId	uniqueidentifier
													 ,@RecordNumber         bigint
													 ,@TraceLevelId         tinyint
													 ,@CustomRecordName     nvarchar(1024)
	                                                 						 ,@ActivityName         nvarchar(1024)
													 ,@ActivityId           nvarchar(256)
													 ,@ActivityInstanceId   nvarchar (256)
													 ,@ActivityType         nvarchar (2048)
													 ,@CustomRecordDataXml  nvarchar(max)
													 ,@AnnotationsXml       nvarchar(max)
													 ,@TimeCreated          datetime
													 
																			
)
as
 begin
	set NOCOUNT ON


	declare @local_tran		bit
			,@error			int
			,@errorMsg	nvarchar(256)
			,@ret		int



	if @@TRANCOUNT > 0
		set @local_tran = 0
	else
	 begin
		begin TRANSACTION
		set @local_tran = 1		
	 end
	
	select @error = @@ERROR

	begin try
	
		insert [Workflow.Tracking].[CustomTrackingEventsTable] (
				[WorkflowInstanceId]
				,[RecordNumber]
				,[TraceLevelId]
				,[CustomRecordName]
	            		,[ActivityName] 
	            		,[ActivityId] 
	            		,[ActivityInstanceId]
	            		,[ActivityType]
	            		,[SerializedData]
	            		,[SerializedAnnotations]
				,[TimeCreated]
		) values (
				 @WorkflowInstanceId
	            		,@RecordNumber                        
				,@TraceLevelId         
	            		,@CustomRecordName
	            		,@ActivityName         
	            		,@ActivityId           
			    	,@ActivityInstanceId   
			    	,@ActivityType
			    	,@CustomRecordDataXml
			    	,@AnnotationsXml    
				,@TimeCreated 
				       
				)
				
		if (@@ROWCOUNT <> 1)
        begin
            raiserror (@errorMsg, 16, -1);
        end 
     end try		
	 begin catch
            set @errorMsg = error_message();
            goto failed;
     end catch;

	if @local_tran = 1
		COMMIT TRANSACTION

	select	@ret = 0

	goto done

failed:
	if @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @errorMsg, 16, -1 )

	set @ret = -1
	goto done

done:
	return @ret

end

go