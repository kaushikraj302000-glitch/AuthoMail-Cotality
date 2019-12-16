CREATE FUNCTION [dbo].[ufn_GetDallasBatchDetails]
(
@EstDate DATE
)
RETURNS @MPs TABLE 
(
	Id int,
	LoanNumber varchar(255) null,
	ClientId int null,
	PrintLocation varchar(255) null,
	EffectiveReceivedOnEst datetime null,
	TrackingNo varchar(255) null,
	IsPullBackRequested bit,
	IsQCRequested bit,
	TotalSheets int,
	DueDate datetime null,
	TotalPages int,
	ShippingStatus varchar(255) null,
	PrintBatch int,
	IsSubmitted bit,
	PipelineStatus varchar(512),
	PrintBatchName varchar(255),
	JobName varchar(512) null,
	SubmittedOn datetime null,
	PieceId int null,
	PrintTrailBatchId int null,
	JobId int null,
	MRDFReqDate datetime null,
	PDFFileName varchar(255)null,
	UniqueId uniqueidentifier null,
	MailPieceStatus varchar(255) null,
	ProcessingTimeStamp datetime null,
	ShippingServiceType varchar(255) null,
	ShippingCarrier varchar(255) null,
	IsBRE bit,
	AppCode varchar(128),
	ClientName varchar(255) null,
	BatchName varchar(128) null,
	FilePath varchar(1024) null
)
AS
begin

	declare @minDate datetime,@pl varchar(255);
	declare @dallasBatchesTimes Table
	(
		AppCode VARCHAR(255),
		BatchName varchar(255),
		StartDate datetime,
		EndDate datetime,
		ShippingCarrier varchar(255),
		ShippingServiceType varchar(255)
	)
	select @pl=Name from Common.PrintVendors where VendorCode='CL_DALLAS'
	--Get batches timings
	--select * into #dallasBatchesTimes from (select * from [dbo].[ufn_GetDallasBatches](@EstDate)) as t
	insert into @dallasBatchesTimes
	select * from [dbo].[ufn_GetDallasBatches](@EstDate)

	--Assign value to mindate
	select @minDate= MIN(StartDate) from @dallasBatchesTimes

	;with q1 as
	(
	SELECT m.Id,m.BusinessFriendlyIdValue as LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,
	m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
	b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,
	req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] as MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, 
	res.ProcessingTimeStamp,
	d.ShippingServiceType,d.ShippingCarrier,d.IsBRE,
	c.AppCode,c.Name as ClientName,
	(select BatchName from @dallasBatchesTimes where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType  AND AppCode=c.AppCode) BatchName,req.FilePath
	FROM [AM].[MailPieces] m
	LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
	LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
	LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
	LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
	INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
	INNER JOIN Common.Clients c ON m.ClientId=c.Id
	WHERE m.PrintLocation=@pl
	AND m.EffectiveReceivedOnEst >=@minDate
	)
	insert into @MPs
	select * from q1 where BatchName is not null 

	return
end
