-- =============================================
-- Author:		 Milan Patel
-- Create date:  01-Mar-18
-- Modifiy date: 01-Mar-18
-- Exec: [dbo].[uspweb_DailyDetailOperationsReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',NULL,NULL,NULL
-- Description:	 Get Batch by parameters clientId AND (date or batchid)
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_DailyDetailOperationsReport]
	
	@clientId INT,
	@startDate DATETIME,
	@endDate DATETIME,
	@batchId INT,
	@channel VARCHAR(128),
	@shippingStatus VARCHAR(255)
AS
BEGIN

SELECT *
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [dbo].[v_DocTypes] d ON m.DocTypeId=d.Id AND m.PnSConfigDetailId = d.PnSConfigDetailId
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND ISNULL(m.ShippingStatus, '') = ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, ''))
END

