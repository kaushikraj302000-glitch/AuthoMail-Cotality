-- =============================================
-- Author:		 Milan Patel
-- Create date:  28-Feb-18
-- Modifiy date: 05-Mar-18
-- Exec [dbo].[uspweb_SummaryControlReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',NULL
-- Description:	 Get Batch by parameters
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_SummaryControlReport]
	@clientId INT,
	@startDate DATETIME,
	@endDate DATETIME,
	@channel VARCHAR(128)
AS
BEGIN
	SELECT *
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch, [Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [dbo].[v_DocTypes] d ON m.DocTypeId=d.Id AND m.PnSConfigDetailId = d.PnSConfigDetailId
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	END

