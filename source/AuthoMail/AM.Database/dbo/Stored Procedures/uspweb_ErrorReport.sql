-- =============================================
-- Author:		 Milan Patel
-- Create date:  28-Feb-18
-- Modifiy date: 03-Mar-18
-- Exec [dbo].[uspweb_ErrorReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null,null
-- Description:	 Get Batch by parameters
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_ErrorReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@batchId int,
	@channel varchar(128),
	@shippingStatus varchar(255)
AS
BEGIN

select *
	from [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	left outer join [AM].[DocTypes] d on m.DocTypeId=d.Id
	where m.MailBatchId=b.Id and c.Id=b.ClientId and ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND ISNULL(m.ShippingStatus, '') = (ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, '')))
	END








