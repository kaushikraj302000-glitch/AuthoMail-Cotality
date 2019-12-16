CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128)
AS
	select m.UniqueId,b.BatchName,m.BusinessFriendlyIdValue as LoanNumber,m.[FileName],m.RcptFirstName as FirstName,m.RcptLastName as LastName
	,m.RcptAddressStreet as [Address],m.RcptAddressCity as City, m.RcptAddressState as [State],m.RcptAddressZIP as Zip,
	b.ReceivedOn,m.PrintLocation,m.ShippingStatus as [Status],m.TrackingNo,m.TotalPages,m.PrintedOn,m.ShippedOn,
	m.PickedUpOn,d.ShippingCarrier,m.ErrorDesc,
	d.DocTypeCode as DocType,d.ShippingCarrier +' '+d.ShippingServiceType as ShippingMethod,
	ch.Title as Channel,c.AppCode,
	r.JobId,r.PieceId
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	LEFT OUTER JOIN [PnS].[MRDFRequest] r ON m.UniqueId = r.UniqueId AND r.IsPDFReceived=1
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND m.EffectiveReceivedOnEst BETWEEN @startDate and @endDate
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	ORDER BY m.Id
