CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId INT,
	@startDate DATETIME,
	@endDate DATETIME,
	@channel VARCHAR(128)
AS
BEGIN

	SELECT A.AppCode, A.BatchName AS BatchName, A.UniqueId,A.BusinessFriendlyIdValue AS LoanNumber
	,A.RcptFirstName AS [FirstName],A.RcptLastName AS [LastName]
	,A.RcptAddressStreet AS [Address],A.RcptAddressCity AS [City],A.RcptAddressState AS [State],A.RcptAddressZIP AS [ZIP]
	,A.Channel ,A.DocumentOrPackageType AS [DocType]
	,A.ReceivedOn, A.ShippedOn, A.PrintLocation
	,A.ShippingStatus AS [Status], A.TrackingNo, A.[FileName]
	,A.TotalPages, A.TotalSheets,ISNULL(c.LETTER,0) AS [LetterPageCount],ISNULL(c.LEGAL,0) AS [LegalPageCount], A.TotalBlankPages
	,A.IsEDeliveryRequested, A.[ShippingMethod]
	,ISNULL(REPLACE(REPLACE(A.ErrorDesc, CHAR(13), ''), CHAR(10), ''),'') AS ErrorDesc
	,A.Inserts,A.PrintBatchName, A.SubmittedOn, A.DeliveryStatus, A.DeliveryDate, A.ShippingCost, A.EnvelopeName
	FROM 
	(
		SELECT B.BatchName,P.UniqueId, P.DocumentOrPackageType ,P.BusinessFriendlyIdValue, P.[FileName], P.[FilePattern], B.ReceivedOn
		,P.PrintedOn ,P.ShippedOn ,P.TotalPages ,P.RcptFirstName,P.RcptLastName ,P.RcptAddressStreet ,P.RcptAddressCity,P.RcptAddressState 
		,P.RcptAddressZIP,P.Id AS [MailPieceId],P.ShippingCost,P.ShippingStatus,P.ErrorDesc, PTB.PrintBatchName
		,PTB.SubmittedOn,P.IsEDeliveryRequested, S.Code AS Channel, P.PrintLocation 
		, PCD.ShippingCarrier + ' / ' +PCD.ShippingServiceType AS [ShippingMethod], P.TrackingNo, P.TotalSheets
		, P.TotalBlankPages, c.AppCode, E.EnvelopeName ,P.Inserts , P.DeliveryStatus, P.DeliveryDate
		FROM AM.MailBatches B
		INNER JOIN AM.MailPieces P ON P.MailBatchId=B.Id
		INNER JOIN Common.SFTChannels S ON B.SFTChannelId=S.Id
		INNER JOIN Common.Clients C ON P.ClientId=C.Id
		LEFT OUTER JOIN AM.DocTypes D ON D.Id=P.DocTypeId
		LEFT OUTER JOIN Common.PnSConfigDetail PCD ON P.PnSConfigDetailId=PCD.Id
		LEFT OUTER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
		LEFT OUTER JOIN PnS.MRDFRequest REQ ON P.UniqueId=REQ.UniqueId
		LEFT OUTER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
		LEFT OUTER JOIN PnS.PrintTrailBatches PTB ON REQ.PrintTrailBatchId = PTB.Id
		WHERE P.EffectiveReceivedOnEst BETWEEN @startDate AND @endDate
		AND B.ClientId=@ClientId
		AND ISNULL(S.Code, '') = ISNULL(@channel, ISNULL(S.Code, ''))
	)A
	LEFT OUTER JOIN 
	(
		SELECT *
		FROM 
		(
			SELECT 
				[MailPieceId],[PageSize],[PageCount]
			FROM [Common].[MailPiecePageInfo]
		) AS S
		PIVOT
		(
			MAX([PageCount])
			FOR [PageSize] IN ([LETTER],[LEGAL])
		)AS pvt2
	)C ON c.MailPieceId=A.MailPieceId
	ORDER BY A.MailPieceId
END

