CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId INT,
	@startDate DATETIME,
	@endDate DATETIME,
	@channel VARCHAR(128)
AS
BEGIN
		SELECT m.UniqueId,b.BatchName,m.BusinessFriendlyIdValue AS LoanNumber,m.[FileName],m.RcptFirstName AS FirstName,m.RcptLastName AS LastName
	,m.RcptAddressStreet AS [Address],m.RcptAddressCity AS City, m.RcptAddressState AS [State],m.RcptAddressZIP AS Zip,
	b.ReceivedOn,m.PrintLocation,m.ShippingStatus AS [Status],m.TrackingNo,m.TotalPages,m.TotalBlankPages,m.PrintedOn,m.ShippedOn,
	m.PickedUpOn,PCD.ShippingCarrier,m.ErrorDesc,m.IsEDeliveryRequested,
	d.DocTypeName AS DocType,PCD.ShippingCarrier +' '+PCD.ShippingServiceType AS ShippingMethod,
	ch.Title AS Channel,c.AppCode,
	r.JobId,r.PieceId
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	LEFT OUTER JOIN [PnS].[MRDFRequest] r ON m.UniqueId = r.UniqueId AND r.IsPDFReceived=1
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND m.EffectiveReceivedOnEst  BETWEEN @startDate AND @endDate
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
END

