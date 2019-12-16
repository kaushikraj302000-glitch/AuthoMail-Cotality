CREATE PROCEDURE [dbo].[uspweb_ErrorReportSLA]
	@clientId int,
	@startDateRec datetime,
	@endDateRec datetime,
	@startDateDue datetime,
	@endDateDue datetime,
	@batchId int,
	@docType varchar(128)
AS
BEGIN

	SELECT *	
	from [AM].[MailBatches] b, [Common].[SFTChannels] ch, [Common].[Clients] c,[AM].[MailPieces] m
	left outer join [AM].[DocTypes] d on m.DocTypeId=d.Id
	where m.MailBatchId=b.Id and c.Id=b.ClientId and ch.Id=b.SFTChannelId AND m.ClientId=@clientId --AND m.SLADays < 0
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDateRec, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDateRec, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.DueDate,0) BETWEEN ISNULL(@startDateDue, ISNULL(m.DueDate, 0)) AND ISNULL(@endDateDue, ISNULL(m.DueDate, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, ''))
	--AND m.ShippingStatus IN ('InProgress','Shipped')
	--AND ISNULL(m.ShippingStatus, '') IN (ISNULL('''InProgress'',''Shipped''', ISNULL(m.ShippingStatus, '')))
	END
