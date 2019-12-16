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
		@mailpieceid int,
		@code varchar(50),
		@existjobid int,
		@jobName varchar(512),
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
		select @minDate= MIN(StartDate) from #dallasBatchesTimes
			
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
			if(not exists(select 1 from PnS.MRDFRequest where jobid=@jobid and MailPieceStatus='InProgress'))
			begin
			--Update Batchsummary pipeline status when all mailpieces's response is received
				update PnS.BatchSummary 
				set PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() 
				where PrintBatch = @jobid

				if(not exists(select 1 from [PnS].[MRDFRequest] where MailPieceStatus='InProgress' and PrintTrailBatchId=@PrintTrailBatchId))
				begin
					-- Update in PrintTrailBatch of mrdfreceivedon when all response is received.
					update PnS.PrintTrailBatches 
					set MRDFReceivedOn=GETUTCDATE()
					where Id=@PrintTrailBatchId					
				end
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
