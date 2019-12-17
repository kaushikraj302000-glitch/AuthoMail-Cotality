CREATE PROCEDURE [dbo].[uspweb_ErrorReportSLA]
	@clientId INT,
	@startDateRec DATETIME,
	@endDateRec DATETIME,
	@startDateDue DATETIME,
	@endDateDue DATETIME,
	@batchId INT,
	@docType VARCHAR(128)
AS
BEGIN

	SELECT *	
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch, [Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [dbo].[v_DocTypes] d ON m.DocTypeId=d.Id AND m.PnSConfigDetailId = d.PnSConfigDetailId
	WHERE m.MailBatchId=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId --AND m.SLADays < 0
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDateRec, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDateRec, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.DueDate,0) BETWEEN ISNULL(@startDateDue, ISNULL(m.DueDate, 0)) AND ISNULL(@endDateDue, ISNULL(m.DueDate, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(d.Id, '') = ISNULL(@docType, ISNULL(d.Id, ''))
	--AND m.ShippingStatus IN ('InProgress','Shipped')
	--AND ISNULL(m.ShippingStatus, '') IN (ISNULL('''InProgress'',''Shipped''', ISNULL(m.ShippingStatus, '')))
	END

