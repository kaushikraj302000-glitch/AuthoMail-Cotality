
-- =============================================
-- Author:		 Milan Patel
-- Create date:  07-Mar-18
-- Modifiy date: 07-Mar-18
-- Exec: [dbo].[usp_MailPieceSearch] 101,null,null,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null,null
--       [dbo].[usp_MailPieceSearch] 101,null,null,'2018-03-19 04:00:00',null,null,null,null,null
-- Description:	 Get Batch by parameters
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_MailPieceSearch]

	@clientId int,
	@businessFriendlyIdName varchar(255),
	@businessFriendlyIdValue varchar(255),
	@startDate datetime,
	@endDate datetime,
	@docType varchar(255),
	@shippingStatus varchar(255),
	@channel varchar(128),
	@clientBatchId varchar(255),
	@mailBatchId int,
	@trackingNo varchar(255)
AS
BEGIN

	 SELECT 
	 m.BusinessFriendlyIdValue as BusinessIdValue,m.ShippingStatus as [Status],m.UniqueId,
	 m.RcptFirstName as FirstName,m.RcptLastName as LastName,m.RcptAddressCity as [City],m.RcptAddressZip as ZIP,
	 m.RcptAddressState as [State],m.ShippedOn,m.TrackingNo,m.DueDate,m.ErrorCode,m.ErrorDesc,m.PrintLocation,
	 b.ReceivedOn,
	 d.DocTypeCode as DocType,d.ShippingServiceType as ShippingMethod,d.ShippingCarrier,
	 ch.Title as Channel
	 FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	 LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id	 
	 LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	 WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id AND m.ClientId=@clientId
	 AND ISNULL(m.BusinessFriendlyIdName, '') = ISNULL(@businessFriendlyIdName, ISNULL(m.BusinessFriendlyIdName, ''))
	 AND ISNULL(m.BusinessFriendlyIdValue, '') = ISNULL(@businessFriendlyIdValue, ISNULL(m.BusinessFriendlyIdValue, ''))
	 AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, ''))
	 AND ISNULL(m.ShippingStatus, '') = ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, ''))
	 AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	 AND ISNULL(b.ClientBatchId, '') = ISNULL(@clientBatchId, ISNULL(b.ClientBatchId, ''))
	 AND ISNULL(m.TrackingNo, '') = ISNULL(@trackingNo, ISNULL(m.TrackingNo, ''))
	 AND ISNULL(m.MailBatchId, '') = ISNULL(@mailBatchId, ISNULL(m.MailBatchId, ''))
	 AND b.ReceivedOn BETWEEN @startDate AND @endDate
END
