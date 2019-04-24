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
		@PrintTrailBatchId int = null
	
	BEGIN TRY
	
		set @trackno = replace(@trackno, 'AUT','')
		set @clientCode = replace(@clientCode, 'AUT','')
		--Get Print Location name
		select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
		--SET @printlocationcl=(select Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL')
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		if(@btName is null)
		begin 
			select 'Invalid' as Code, NULL as TrackingNo, 'Batch name can not be null' as [Message], NULL as [JobName]
			return
		end
		select @mailpieceid= Id,@pieceid=PieceId,@jobid=JobId,@mrdfreqdate=MRDFReqDate,@ispullbackrequested =IsPullBackRequested,@isqcrequested=IsQCRequested,@totalsheet=TotalSheets,@pdffilename=PDFFileName,@jobName=JobName,@serviceType=ShippingServiceType,@clientAppCode=AppCode,@clientName=ClientName,@bt=BatchName,@PrintTrailBatchId=PrintTrailBatchId
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where TrackingNo=@trackno and PrintLocation=@printlocationcl and IsSubmitted=1 and ShippingStatus='InProgress'

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
RETURNS TABLE 
AS
RETURN
(
with db as
(
 select * from [dbo].[ufn_GetDallasBatches](@EstDate)
),
q as
(
SELECT m.Id,m.BusinessFriendlyIdValue as LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,--b.SubmittedOn,
req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] as MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, res.ProcessingTimeStamp,
d.ShippingServiceType,d.ShippingCarrier,d.IsBRE,
c.AppCode,c.Name as ClientName,(select BatchName from db where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType  AND AppCode=c.AppCode) BatchName
FROM [AM].[MailPieces] m
LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
INNER JOIN Common.Clients c ON m.ClientId=c.Id
WHERE m.PrintLocation=(select Name from Common.PrintVendors where VendorCode='CL_DALLAS') 
AND m.EffectiveReceivedOnEst >=(select MIN(StartDate) from db)
)
select * from q where BatchName is not null 
)

go
print '[dbo].[ufn_GetDallasBatchDetails] created'