-- =============================================
-- Author:		Milan Patel
-- Create date: 18-Aug-18
-- Description:	<Description,,>
-- [dbo].[SP_USPSScan] 1007,'9205500000000000000018'
-- =============================================
CREATE PROCEDURE [dbo].[SP_USPSScan]
	@mailPieceId int,
	@trackNo varchar(255)
AS
BEGIN
	declare
		@mpIdOld int,
		@trackIdOld varchar(255),
		@printlocationcl varchar(1024),
		@serviceType varchar(255),
		@ispullbackrequested bit,
		@isqcrequested bit,
		@mailpiecestatus varchar(255),
		@jobid int,
		@pieceid int,
		@inprogresscnt int

	BEGIN TRY
		--Get Print Location name. (Not Needed)
		SET @printlocationcl=(select Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL')
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		select @mpIdOld=m.Id, @ispullbackrequested=m.IsPullBackRequested, @isqcrequested=m.IsQCRequested,  @trackIdOld=m.TrackingNo, @serviceType=d.ShippingServiceType, @pieceid=r.PieceId,@jobid=r.JobId,@mailpiecestatus=r.MailPieceStatus
		from AM.MailPieces m
		inner join AM.DocTypes d on m.DocTypeId=d.Id
		inner join [PnS].[MRDFRequest] r ON m.UniqueId=r.UniqueId
		where m.Id=@mailPieceId  and m.PrintLocation=@printlocationcl
		
		--Validate MailPiece found or not, ServiceType is PRIORITY_EXPRESS or not
		if(@mpIdOld IS NULL OR @serviceType <> 'PRIORITY_EXPRESS')
		begin 
			select 'Invalid' as Code,@mailPieceId as MailPieceId, @trackNo as TrackingNo,replace('Document with barcode mailPieceId could not be found.','mailPieceId',@mailPieceId) as [Message]
			return
		end		
		-- Validate tracking no is 22 digit or not. Verify
		if(LEN(@trackNo) <> 22)
		begin
			select 'Invalid' as Code,@mailPieceId as MailPieceId, @trackNo as TrackingNo, 'Invalid Tracking No' as [Message]
			return
		end
		-- Validate Response received or not.
		if((select count(*) from PnS.MRDFResponse where JobId=@jobid and PieceId=@pieceid)=0)
		begin
			select 'RESNotFound' as Code,@mailPieceId as MailPieceId, @trackNo as TrackingNo, NULL as [Message]
			return
		end
		-- Validate for Rescan.(Response is received by above validation) 		
		if (@mailPieceStatus <> 'InProgress')
		begin
			select 'Rescan' as Code,@mailPieceId as MailPieceId, @trackNo as TrackingNo, NULL as [Message]
			return
		end
		-- Validate for Rescan. Already tracking no registered 		
		if exists(select 1 from [AM].[MailPieces] where trackingno=@trackNo)
		begin
			select 'Rescan' as Code,@mailPieceId as MailPieceId, @trackNo as TrackingNo, 'Tracking No already exists.' as [Message]
			return
		end
		-- set mailpiece status if ispullbackrequested then update otherwise not
		SET @mailpiecestatus='Shipped'
		if(@ispullbackrequested = 1) SET @mailpiecestatus='PulledBack'

		-- Start Transaction
		BEGIN TRANSACTION
			-- Update Tracking no with new trackingno
			update AM.MailPieces set TrackingNo=@trackNo where TrackingNo=cast(@mailPieceId as varchar)
			
			-- Update MRDFRequest MailPieceStatus
			Update [PnS].[MRDFRequest] SET MailPieceStatus = @mailpiecestatus
			where JobId=@jobid and PieceId = @pieceid

			-- Check if we are yet to received response for any mail pieces
			select @inprogresscnt= count(*) from [PnS].[MRDFRequest] where MailPieceStatus='InProgress' and JobId=@jobid
			
			-- If responses are received for entire batch, then update batch status to received
			if(@inprogresscnt = 0)
			begin
				update PnS.BatchSummary 
				set PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() where PrintBatch = @jobid
			end
		COMMIT
		--Return Message
		select 'Valid' as Code,@mailPieceId as MailPieceId, @trackNo as TrackingNo, NULL as [Message]
			return
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH
END