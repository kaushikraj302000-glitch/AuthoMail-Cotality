GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_RegisterScan')
DROP PROCEDURE [dbo].[SP_RegisterScan]
go
print '[dbo].[SP_RegisterScan] dropped'
GO

CREATE PROCEDURE [dbo].[SP_RegisterScan]
	-- Add the parameters for the stored procedure here
@trackno varchar(255),
@clientCode varchar(128),
@operatorid varchar(50),
@machineid varchar(255),
@btName varchar(255),-- 05PM, 06PM, 07PM, Backlog
@EstDate DATE
AS
BEGIN
declare	@jobid int,
		@pieceid int,
		@mrdfreqdate date,
		@ispullbackrequested bit,
		@isqcrequested bit,
		@disposition varchar(50),
		@mailpiecestatus varchar(255),
		@totalsheet int = null,
		@pdffilename varchar(255),
		@printlocationcl varchar(1024),
		@inprogresscnt int,
		@mailpieceid int,
		@code varchar(50),
		@existjobid int,
		@jobName varchar(512),
		@serviceType varchar(255),
		@clientAppCode varchar(128),
		@clientName varchar(255),
		@bt varchar(255),
		@PrintTrailBatchId int = null,
		@minDate datetime =null
	BEGIN TRY
	
		set @trackno = replace(@trackno, 'AUT','')
		set @clientCode = replace(@clientCode, 'AUT','')
		--Get Print Location name
		select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
		
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		if(@btName is null)
		begin 
			select 'Invalid' as Code, NULL as TrackingNo, 'Batch name can not be null' as [Message], NULL as [JobName]
			return
		end
		
		--Get batches timings
		select * into #dallasBatchesTimes from (select * from [dbo].[ufn_GetDallasBatches](@EstDate)) as t
		--initilize mindate
		select @minDate= MIN(StartDate) from [dbo].[ufn_GetDallasBatches](@EstDate)
			
		-- getdata from DB
		SELECT @mailpieceid=m.Id, @ispullbackrequested=m.[IsPullBackRequested],@isqcrequested=m.[IsQCRequested],
		@totalsheet=m.[TotalSheets],
		@jobName=b.[JobName],
		@pieceid=req.[PieceId],@PrintTrailBatchId=req.[PrintTrailBatchId], @jobid=req.[JobId],@mrdfreqdate=req.[Date],@pdffilename=req.[PDFFileName],
		@clientAppCode=c.[AppCode],@clientName=c.[Name],
		@bt=(select [BatchName] from #dallasBatchesTimes where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType AND AppCode=c.AppCode)
		FROM [AM].[MailPieces] m
		INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
		INNER JOIN Common.Clients c ON m.ClientId=c.Id
		left outer join PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
		left outer JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
		WHERE m.PrintLocation=@printlocationcl 
		AND m.EffectiveReceivedOnEst >=@minDate and IsSubmitted=1 and m.ShippingStatus='InProgress' and m.TrackingNo=@trackno
			
		--Drop Temp Table
		If(OBJECT_ID('tempdb..#dallasBatchesTimes') Is Not Null)
		Begin
		Drop Table #dallasBatchesTimes
		End


		if(@mailpieceid IS NULL) 
		begin
			select 'Invalid' as Code, NULL as TrackingNo, NULL as [Message], NULL as [JobName]
			return
		end

		--Validate tracking no is assign to this client
		if(@clientCode <> @clientAppCode)
		begin
			select 'Invalid' as Code, @trackno as TrackingNo, REPLACE('This tracking number belongs to @clientName client. Please check your document.','@clientName',@clientName) as [Message], NULL as [JobName]
			return
		end

		-- Validate this mailpiece is assign to this batch or not.
		if(@bt<>@btName)
		begin
			select 'Invalid' as Code, @trackno as TrackingNo, REPLACE('This document belongs to <span style="color:#000"> @bt </span> batch.','@bt',@bt) as [Message], NULL as [JobName]
			return
		end
		
		if exists(select 1 from PnS.MRDFResponse where JobId = @PrintTrailBatchId and PieceId = @pieceid) 
		begin
			select 'Rescan' as Code, @trackno as TrackingNo, NULL as [Message], @jobName as [JobName]
			return
		end

		-- set mailpiece status if ispullbackrequested then update otherwise not
		SET @mailpiecestatus='Shipped'

		if(@ispullbackrequested = 1) 
		begin
			SET @code = 'Pulledback'
			SET @disposition='4' 
			SET @mailpiecestatus='PulledBack'
		end
		else if(@isqcrequested=1)
		begin
			SET @disposition='8'
			SET @code = 'QC'
		end
		else
		begin
			SET @disposition='5'
			SET @code = 'Valid'
		end

		-- Start Transaction
		BEGIN TRANSACTION	
			-- insert data into MRDFResponse table
			INSERT INTO [PnS].[MRDFResponse] ([JobId],[PieceId],[Date],[Feeder2_Fed],[OperatorId],[MachineId],									[ProcessingTimeStamp],[DetailedErrorId],[Disposition],[Input_Fed],[Feeder3_Fed],[Feeder4_Fed],[Feeder5_Fed],[ComputedWeight],[MeterDate],[StatusSource],[PostageMetered]
			,[WeightMeasured],[ExitLocation],[TotalSheets],[PDFFileName],[PackageTrackingNumber])
			VALUES(@PrintTrailBatchId ,@pieceid,@mrdfreqdate,0,@operatorid,@machineid,GETUTCDATE(),'4691'
			,@disposition,'00','0','0','0','0.000','00000000', @printlocationcl,'0.000','0.000',@printlocationcl
			,@totalsheet,@pdffilename,NULL)				
			
				Update [PnS].[MRDFRequest] SET MailPieceStatus = @mailpiecestatus
				where PieceId = @pieceid and PrintTrailBatchId=@PrintTrailBatchId and JobId=@jobid

				-- Check if we are yet to received response for any mail pieces
			

			select @inprogresscnt= count(*) from [PnS].[MRDFRequest] where MailPieceStatus='InProgress' and PrintTrailBatchId=@PrintTrailBatchId
			if(@inprogresscnt = 0)
			begin
			-- Update in PrintTrailBatch of mrdfreceivedon when all response is received.
				update PnS.PrintTrailBatches 
				set MRDFReceivedOn=GETUTCDATE()
				where Id=@PrintTrailBatchId
				--Update Batchsummary pipeline status when all mailpieces's response is received
				update PnS.BatchSummary 
				set PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() 
				where PrintBatch in (select distinct JobId from PnS.MRDFRequest where PrintTrailBatchId=@PrintTrailBatchId)
			end
			
		--End Transaction
		COMMIT
		
		select @code as Code, @trackno as TrackingNo, NULL as [Message], @jobName as [JobName]
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH
END
go
print '[dbo].[SP_RegisterScan] created'
GO

--------------------function---------------------
DROP FUNCTION [dbo].[ufn_GetDallasBatchDetails]
go
print '[dbo].[ufn_GetDallasBatchDetails] dropped'
GO
CREATE FUNCTION [dbo].[ufn_GetDallasBatchDetails]
(
@EstDate DATE
)
RETURNS @MPs TABLE 
(
	Id int,
	LoanNumber varchar(255) null,
	ClientId int null,
	PrintLocation varchar(255) null,
	EffectiveReceivedOnEst datetime null,
	TrackingNo varchar(255) null,
	IsPullBackRequested bit,
	IsQCRequested bit,
	TotalSheets int,
	DueDate datetime null,
	TotalPages int,
	ShippingStatus varchar(255) null,
	PrintBatch int,
	IsSubmitted bit,
	PipelineStatus varchar(512),
	PrintBatchName varchar(255),
	JobName varchar(512) null,
	SubmittedOn datetime null,
	PieceId int null,
	PrintTrailBatchId int null,
	JobId int null,
	MRDFReqDate datetime null,
	PDFFileName varchar(255)null,
	UniqueId uniqueidentifier null,
	MailPieceStatus varchar(255) null,
	ProcessingTimeStamp datetime null,
	ShippingServiceType varchar(255) null,
	ShippingCarrier varchar(255) null,
	IsBRE bit,
	AppCode varchar(128),
	ClientName varchar(255) null,
	BatchName varchar(128) null
)
AS
begin

	declare @minDate datetime,@pl varchar(255);
	declare @dallasBatchesTimes Table
	(
		AppCode VARCHAR(255),
		BatchName varchar(255),
		StartDate datetime,
		EndDate datetime,
		ShippingCarrier varchar(255),
		ShippingServiceType varchar(255)
	)
	select @pl=Name from Common.PrintVendors where VendorCode='CL_DALLAS'
	--Get batches timings
	--select * into #dallasBatchesTimes from (select * from [dbo].[ufn_GetDallasBatches](@EstDate)) as t
	insert into @dallasBatchesTimes
	select * from [dbo].[ufn_GetDallasBatches](@EstDate)

	--Assign value to mindate
	select @minDate= MIN(StartDate) from @dallasBatchesTimes

	insert into @MPs
	SELECT m.Id,m.BusinessFriendlyIdValue as LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,
	m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
	b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,
	req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] as MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, 
	res.ProcessingTimeStamp,
	d.ShippingServiceType,d.ShippingCarrier,d.IsBRE,
	c.AppCode,c.Name as ClientName,(select BatchName from @dallasBatchesTimes where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType  AND AppCode=c.AppCode) BatchName
	FROM [AM].[MailPieces] m
	LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
	LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
	LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
	LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
	INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
	INNER JOIN Common.Clients c ON m.ClientId=c.Id
	WHERE m.PrintLocation=@pl
	AND m.EffectiveReceivedOnEst >=@minDate

	return
end
go
print '[dbo].[ufn_GetDallasBatchDetails] created'
---------------------------------------------------------------------------------
go
DROP FUNCTION [dbo].[SP_GetScanStats]
go
print '[dbo].[SP_GetScanStats] dropped'
GO
CREATE PROCEDURE [dbo].[SP_GetScanStats]
	@appcode varchar(128),
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	declare @totalPending int, @scannedCount int, @clientid int
	
	-- Remove AUT from app code
	set @appcode = replace(@appcode, 'AUT','')

	-- Get client id
	select @clientid=Id from Common.Clients where AppCode=@appcode

	--Calculate Pending & Scan count

		select 
		@totalPending=count(case when MailPieceStatus = 'InProgress' and IsSubmitted=1 then 1 else null end),
		@scannedCount=count(case when MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate) then 1 else null end)
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where ClientId=@clientId and ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate)))

		-- Return result
	select @scannedCount as [ScanCount],@totalPending + @scannedCount as [TotalCount]

END
go
print '[dbo].[SP_GetScanStats] created'
go
------------------------------------------------------------------------------------

DROP FUNCTION [dbo].[SP_GetRecentBatchesStats]
go
print '[dbo].[SP_GetRecentBatchesStats] dropped'
GO
ALTER PROCEDURE [dbo].[SP_GetRecentBatchesStats] 
	@appcode varchar(128),
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	
	declare @clientid int

	--Get Client Id, and remove AUT from client appcode
	set @appcode=REPLACE(@appcode,'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;
	
	-- Get Pending & Scan count batch(sweep) wise
	with t as
	(
	select distinct BatchName,
		count(case when MailPieceStatus = 'InProgress' and IsSubmitted=1 then 1 else null end) as PendingCount,
		count(case when MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate) then 1 else null end) as ScannedCount
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where ClientId=@clientId and ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate)))
		group by BatchName
	),		
	sw as
	(
		select distinct BatchName from dbo.ufn_GetDallasBatches(@EstDate) where AppCode=@appcode
	)
		
	select	ISNULL(t.BatchName,sw.BatchName) as BatchName,
			IsNULL(t.ScannedCount,0) as ScannedLoans,
			(ISNULL(t.PendingCount,0) + ISNULL(t.ScannedCount,0)) as TotalLoans 
			from t
			full outer join sw on t.BatchName = sw.BatchName
			order by BatchName
END

go
print '[dbo].[SP_GetRecentBatchesStats] created'
go
