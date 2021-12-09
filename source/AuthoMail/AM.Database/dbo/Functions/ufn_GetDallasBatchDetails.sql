CREATE FUNCTION [dbo].[ufn_GetDallasBatchDetails]
(
@EstDate DATE
)
RETURNS @MPs TABLE 
(
	Id INT,
	LoanNumber VARCHAR(255) NULL,
	ClientId INT NULL,
	PrintLocation VARCHAR(255) NULL,
	EffectiveReceivedOnEst DATETIME NULL,
	TrackingNo VARCHAR(255) NULL,
	IsPullBackRequested BIT,
	IsQCRequested BIT,
	TotalSheets INT,
	DueDate DATETIME NULL,
	TotalPages INT,
	ShippingStatus VARCHAR(255) NULL,
	PrintBatch INT,
	IsSubmitted BIT,
	PipelineStatus VARCHAR(512),
	PrintBatchName VARCHAR(255),
	JobName VARCHAR(512) NULL,
	SubmittedOn DATETIME NULL,
	PieceId INT NULL,
	PrintTrailBatchId INT NULL,
	JobId INT NULL,
	MRDFReqDate DATETIME NULL,
	PDFFileName VARCHAR(255)NULL,
	UniqueId UNIQUEIDENTIFIER NULL,
	MailPieceStatus VARCHAR(255) NULL,
	ProcessingTimeStamp DATETIME NULL,
	ShippingServiceType VARCHAR(255) NULL,
	ShippingCarrier VARCHAR(255) NULL,
	IsBRE BIT,
	AppCode VARCHAR(128),
	ClientName VARCHAR(255) NULL,
	BatchName VARCHAR(128) NULL,
	FilePath VARCHAR(1024) NULL
)
AS
BEGIN

	DECLARE @minDate DATETIME,@pl VARCHAR(255);
	DECLARE @dallasBatchesTimes Table
	(
		AppCode VARCHAR(255),
		BatchName VARCHAR(255),
		StartDate DATETIME,
		EndDate DATETIME,
		ShippingCarrier VARCHAR(255),
		ShippingServiceType VARCHAR(255)
	)
	SELECT @pl=Name FROM Common.PrintVendors WHERE VendorCode='CL_DALLAS'
	--Get batches timings
	--SELECT  into #dallasBatchesTimes FROM (SELECT  FROM [dbo].[ufn_GetDallasBatches](@EstDate)) AS t
	INSERT INTO @dallasBatchesTimes
	SELECT * FROM [dbo].[ufn_GetDallasBatches](@EstDate)

	--Assign value to mindate
	SELECT @minDate= MIN(StartDate) FROM @dallasBatchesTimes

	INSERT INTO @MPs
	SELECT m.Id,m.BusinessFriendlyIdValue AS LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,
	m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
	b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,
	req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] AS MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, 
	res.ProcessingTimeStamp,
	PCD.ShippingServiceType,PCD.ShippingCarrier
	,CASE WHEN m.Inserts IS NOT NULL AND LEN(RTRIM(LTRIM(m.Inserts)))>0 THEN CAST(1 AS BIT) WHEN d.IsBRE=1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END,
	c.AppCode,c.Name AS ClientName,(SELECT BatchName FROM @dallasBatchesTimes WHERE EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=PCD.ShippingCarrier AND ShippingServiceType=PCD.ShippingServiceType  AND AppCode=c.AppCode) BatchName,
	req.FilePath
	FROM [AM].[MailPieces] m
	LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
	LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
	LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
	LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
	INNER JOIN [AM].DocTypes d ON m.DocTypeId=d.Id
	INNER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId=PCD.Id
	INNER JOIN Common.Clients c ON m.ClientId=c.Id
	WHERE m.PrintLocation=@pl
	AND m.EffectiveReceivedOnEst >=@minDate

	RETURN
END
