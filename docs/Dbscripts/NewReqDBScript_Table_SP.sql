create table [AM].[ProjectRequest]
(
	Id [int] identity(1,1) not null primary key,
	ClientName nvarchar(255),	
	SubmittedOn datetime,
	SubmittedBy varchar(255),
	UATStartOn datetime null,
	ProdStartOn datetime null,
	Description nvarchar(2048),
	FileTransmittedVia nvarchar(1024),
	NoOfEstSweepTimes varchar(255),
	ESTSweepTime nvarchar(1024),
	TypeOfFiles nvarchar(1024),
	TransmittedTime nvarchar(255),
	ServiceLevel varchar(512),
	EstimatedPagesPerPackage int,
	EstimatedPackagesDaily int,
	EstimatedPackagesMonthly int,
	EstimatedPackagesAnnually int,
	DailyPrintSchedule varchar(100), -- PipeLine seperated
	PaperPrintRequirements nvarchar(2048),
	PackagesSentToCustomer nvarchar(2048),
	IsSampleEnvelopeAttached bit,
	Envelopes nvarchar(2048),
	IsBRERequired bit,
	BusinessReplyMailEnvelope nvarchar(2048),
	IsCustomerSupplyEnvelope bit,
	IsCustomerProvideArtworkEnvelope bit,
	IsClientPostageAccount bit,
	IsScanInAuthoMailDallas bit,
	FilesTransmittedOther nvarchar(2048),
	TypeOfFilesOther nvarchar(2048),
	PaperAndPrintDetails nvarchar(2048),
	ShippingMailingDetail nvarchar(2048),
	MailingEnvelopeDetail nvarchar(2048),
	BusinessReplyMailEnvelopeDetail nvarchar(2048),
	ListOfDocTypesAndPages nvarchar(2048),
	PDFSample nvarchar(2048),
	InsertRequired bit,
	BookletRequired bit,
	BusinessReplyMailEnvelopeRequired bit,
	OtherMaterialRequired bit,
	InsertMaterialOrderedBy varchar(255),--Corelogic or Client
	AdditionalInfo nvarchar(max),
	CreatedOn datetime not null,
	CreatedBy nvarchar(128) not null,
	ModifiedOn datetime not null,	
	ModifiedBy nvarchar(128) not null,
	CONSTRAINT CreatedBy_ProjectRequest_Users FOREIGN KEY (CreatedBy)
    REFERENCES [Profile].[users](Id),
	CONSTRAINT ModifiedBy_ProjectRequest_Users FOREIGN KEY (ModifiedBy)
    REFERENCES [Profile].[users](Id)
) 

Go
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 
GO
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus  from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 or (rq.MailPieceStatus='Shipped' and mp.IsPickedUp <> 1 and mp.DueDate < GETUTCDATE() and d.ShippingCarrier='FedEx')
END

Go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_MonthlyReconReport')
DROP PROCEDURE [dbo].[uspweb_MonthlyReconReport]
GO
CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128),
	@shippingStatus varchar(255)
AS
	select m.Id as MailPieceId,m.BusinessFriendlyIdValue as BusinessIdValue,m.[FileName],m.RcptFirstName as FirstName,m.RcptLastName as LastName
	,m.RcptAddressStreet as [Address],m.RcptAddressCity as City, m.RcptAddressState as [State],m.RcptAddressZIP as Zip,
	b.ReceivedOn,m.PrintLocation,m.ShippingStatus as [Status],m.TrackingNo,m.TotalPages,m.PrintedOn,CASE WHEN d.ShippingCarrier ='FedEx' THEN m.PickedUpOn ELSE m.ShippedOn END as ShippedOn,
	d.DocTypeCode as DocType,d.ShippingServiceType as ShippingMethod,
	ch.Title as Channel,c.AppCode,
	r.JobId,r.PieceId
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	LEFT OUTER JOIN [PnS].[MRDFRequest] r ON m.UniqueId = r.UniqueId AND r.IsPDFReceived=1
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND b.ReceivedOn BETWEEN @startDate and @endDate
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND m.ShippingStatus=@shippingStatus

	Go