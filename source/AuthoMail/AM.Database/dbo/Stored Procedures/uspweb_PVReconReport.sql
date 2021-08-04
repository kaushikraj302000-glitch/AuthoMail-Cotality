CREATE PROCEDURE [dbo].[uspweb_PVReconReport]
	@pvCode VARCHAR(255) = NULL,
	@startDate DATETIME,
	@endDate DATETIME
AS
BEGIN

	SELECT C.AppCode, B.BatchName AS BatchName, P.UniqueId,P.BusinessFriendlyIdValue AS LoanNumber
	,P.RcptFirstName AS [FirstName],P.RcptLastName AS [LastName]
	,P.RcptAddressStreet AS [Address],P.RcptAddressCity AS [City],P.RcptAddressState AS [State],P.RcptAddressZIP AS [ZIP]
	,S.Code AS Channel ,P.DocumentOrPackageType AS [DocType]
	,B.ReceivedOn, P.ShippedOn, P.PulledBackOn, P.PrintLocation
	,P.ShippingStatus AS [Status], P.TrackingNo, P.[FileName]
	,P.TotalPages, P.TotalSheets,ISNULL(MPP.LETTER,0) AS [LetterPageCount],ISNULL(MPP.LEGAL,0) AS [LegalPageCount], P.TotalBlankPages
	,P.IsEDeliveryRequested, PCD.ShippingCarrier + ' / ' +PCD.ShippingServiceType AS [ShippingMethod]
	,ISNULL(REPLACE(REPLACE(P.ErrorDesc, CHAR(13), ''), CHAR(10), ''),'') AS ErrorDesc
	,P.Inserts,PT.PrintBatchName, PT.SubmittedOn, P.DeliveryStatus, P.DeliveryDate, P.ShippingCost, E.EnvelopeName
	FROM AM.MailPieces P
	INNER JOIN AM.DocTypes D ON P.DocTypeId=D.Id 
	INNER JOIN Common.PnSConfigDetail PCD ON P.PnSConfigDetailId=PCD.Id
	INNER JOIN Common.Clients C ON P.ClientId=C.Id
	INNER JOIN AM.MailBatches B ON P.MailBatchId=B.Id
	INNER JOIN Common.SFTChannels S ON B.SFTChannelId=S.Id
	INNER JOIN PnS.MRDFRequest REQ ON P.UniqueId=REQ.UniqueId
	INNER JOIN PnS.PrintTrailBatches PT ON REQ.PrintTrailBatchId=PT.Id
	INNER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
	LEFT OUTER JOIN 
	(
		SELECT *
		FROM 
		(
			SELECT 
			[MailPieceId],[PageSize],[PageCount]
			FROM [Common].[MailPiecePageInfo]
		) AS s
		PIVOT
		(
			MAX([PageCount])
			FOR [PageSize] IN ([LETTER],[LEGAL])
		)AS pvt2
	)MPP on MPP.MailPieceId=P.Id
	WHERE PT.SubmittedOn BETWEEN @startDate AND @endDate
	AND PT.PrintTrailId IN (SELECT Id FROM Common.PrintTrails WHERE PrintVendorCode=ISNULL(@pvCode,PrintVendorCode))
	ORDER BY P.Id
END

