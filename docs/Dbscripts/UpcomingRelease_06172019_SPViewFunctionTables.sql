
PRINT 'Added two new column in Common.Envelopes table'
ALTER TABLE Common.Envelopes
ADD ReorderDuration INT NULL--,MinStock INT NULL

PRINT 'Added new column in AM.MailPieces,PnS.MRDFRequest,PnS.PrintTrailBatches table'
ALTER TABLE AM.MailPieces
ADD PDFFileSize BIGINT NULL
ALTER TABLE PnS.MRDFRequest
ADD PDFFileSize BIGINT NULL
ALTER TABLE PnS.PrintTrailBatches
ADD ZipFileSize BIGINT NULL

PRINT 'Added new table PnS.EnvelopeUsage'
CREATE TABLE [PnS].[EnvelopeUsage](
	[PrintTrailBatchId] [int] NULL,
	[EnvelopeId] [varchar](255) NULL,
	[Quantity] [int] NULL,
	[InsertedOn] [datetime] NULL
) ON [PRIMARY]
GO

ALTER TABLE [PnS].[EnvelopeUsage]  WITH CHECK ADD  CONSTRAINT [FK_EnvelopeUsage_Envelopes] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE [PnS].[EnvelopeUsage] CHECK CONSTRAINT [FK_EnvelopeUsage_Envelopes]
GO

ALTER TABLE [PnS].[EnvelopeUsage]  WITH CHECK ADD  CONSTRAINT [FK_EnvelopeUsage_PrintTrailBatches] FOREIGN KEY([PrintTrailBatchId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])
GO

ALTER TABLE [PnS].[EnvelopeUsage] CHECK CONSTRAINT [FK_EnvelopeUsage_PrintTrailBatches]
GO

CREATE INDEX Idx_EnvelopeUsage_PrintTrailBatchId
ON [PnS].[EnvelopeUsage] (PrintTrailBatchId);
GO


PRINT 'Added new table AM.ErrorNotifications'

CREATE TABLE [AM].[ErrorNotifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [int] NOT NULL,
	[MailBatchId] [int] NOT NULL,
	[TotalMPsCount] [int] NOT NULL,
	[ErrorMPsCount] [int] NOT NULL,
	[ReceivedOn] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_ErrorNotifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE INDEX Idx_ErrorNotifications_MailBatchId
ON AM.ErrorNotifications (MailBatchId);
GO

CREATE INDEX Idx_ErrorNotifications_ClientId
ON AM.ErrorNotifications (ClientId);
GO

PRINT 'Added new view view_EnvelopeInventoryData'
CREATE VIEW [dbo].[view_EnvelopeInventoryData]
AS

WITH CTE AS
(
SELECT EnvelopeId,SUM(Quantity) AS TotalQuantityUsed
FROM [PnS].[EnvelopeUsage] EU
WHERE InsertedOn >=DATEADD(DAY,-30,GETUTCDATE())
GROUP BY EnvelopeId
)
SELECT CTE.*
,E.CurrentQuantity
,(CTE.TotalQuantityUsed/30) AS AvgQuantityPerDay
,((CTE.TotalQuantityUsed/30)*E.ReorderDuration) AS MinStockRequirement 
,(((CTE.TotalQuantityUsed/30)*E.ReorderDuration) * (30 / 100)) AS BufferStock
FROM CTE 
INNER JOIN Common.Envelopes E ON CTE.[EnvelopeId] = E.Id
GO

PRINT 'Added new sp	usp_InsertErrorNotificationData'


CREATE PROCEDURE [dbo].[usp_InsertErrorNotificationData]
@mailBatchId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @errorMPs INT,@errorMPsInNotify INT,@errorNotifyId INT

	select @errorMPs=count(Id) from am.MailPieces where mailbatchid=@mailBatchId and ShippingStatus='Error';
	if(@errorMPs>0)
	begin
		select @errorMPsInNotify=ErrorMPsCount,@errorNotifyId=Id from AM.ErrorNotifications WHERE MailBatchId=@mailBatchId;
		if(@errorMPsInNotify is null)
		begin 
			insert into AM.ErrorNotifications(ClientId, MailBatchId, ErrorMPsCount, ReceivedOn, CreatedOn)
			SELECT ClientId,Id,@errorMPs,ReceivedOn,GETUTCDATE() FROM AM.MailBatches WHERE Id=@mailBatchId
		end
		else
		begin
			if(@errorMPs > @errorMPsInNotify)
			begin
				delete from AM.ErrorNotifications where Id=@errorNotifyId;

				insert into AM.ErrorNotifications(ClientId, MailBatchId, ErrorMPsCount, ReceivedOn, CreatedOn)
				SELECT ClientId,Id,@errorMPs,ReceivedOn,GETUTCDATE() FROM AM.MailBatches WHERE Id=@mailBatchId
			end
		end
	end

END

PRINT 'Added new table [AM].[AlertSubscribers]'
CREATE TABLE [AM].[AlertSubscribers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClientId] [int] NOT NULL,
	[AlertType] [varchar](255) NOT NULL,
 CONSTRAINT [PK_AlertSubscribers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [AM].[AlertSubscribers]  WITH CHECK ADD  CONSTRAINT [FK_AlertSubscribers_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO

ALTER TABLE [AM].[AlertSubscribers] CHECK CONSTRAINT [FK_AlertSubscribers_Clients]
GO

ALTER TABLE [AM].[AlertSubscribers]  WITH CHECK ADD  CONSTRAINT [FK_AlertSubscribers_Users] FOREIGN KEY([UserId])
REFERENCES [Profile].[Users] ([Id])
GO

ALTER TABLE [AM].[AlertSubscribers] CHECK CONSTRAINT [FK_AlertSubscribers_Users]
GO

--------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_EnvelopeInventoryStockCalculation')
DROP PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
go

CREATE PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
	
AS
BEGIN
	declare @startDate as date,@endDate as date,@wstartDate as date
set @endDate = dateadd(day,-1,cast(getutcdate() as date));
set @startDate= dateadd(month,-1,getutcdate());
set @wstartDate=dateadd(day,-7,getutcdate());
;with monthAvg as
(
	select EnvelopeId,count(distinct(cast(SubmittedOn as date))) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	WHERE cast(SubmittedOn as date) BETWEEN @startDate AND @endDate
	GROUP BY EnvelopeId
),
weekAvg as
(
	select EnvelopeId,count(distinct(cast(SubmittedOn as date))) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	WHERE cast(SubmittedOn as date) BETWEEN @wstartDate AND @endDate
	GROUP BY EnvelopeId
),
mergeAvg as
(
	select m.EnvelopeId,case when m.TotalQuantityUsed>isnull(w.TotalQuantityUsed,0) then m.TotalQuantityUsed else w.TotalQuantityUsed end as TotalQuantityUsed,
	case when m.TotalDays>isnull(w.TotalDays,0) then m.TotalDays else w.TotalDays end as TotalDays
	from monthAvg m
	left outer join weekAvg w on m.EnvelopeId=w.EnvelopeId
)
select c.*,e.CurrentQuantity,e.ReorderDuration,CEILING(cast(TotalQuantityUsed as float)/TotalDays) as AvgRate,CEILING(cast(e.CurrentQuantity as float)/(TotalQuantityUsed/TotalDays)) as EnvDays
from mergeAvg c
inner join Common.Envelopes e on c.EnvelopeId=e.Id
END
go
-----------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit]
go
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mp.businessfriendlyidvalue as LoanNumber,mp.TrackingNo,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.DueDate, mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus,pt.TrailName as PrintTrail  
 from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 or (rq.MailPieceStatus='Shipped' and mp.IsPickedUp <> 1 and mp.DueDate < GETUTCDATE() and d.ShippingCarrier='FedEx' and mb.ReceivedOn>='2019-01-01 05:00:00')
END
---------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetEnvelopeInventoryUsage')
DROP PROCEDURE [dbo].[usp_GetEnvelopeInventoryUsage]
go
Create PROCEDURE [dbo].[usp_GetEnvelopeInventoryUsage]
	@Id varchar(500)
	AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		declare @startDate as date,@endDate as date
set @endDate = dateadd(day,-1,cast(getutcdate() as date));
set @startDate= dateadd(month,-1,getutcdate());--dateadd(day,-(DAY(EOMONTH(DATEADD(month,-1,@endDate)))+1),@endDate)

;with cte as
(
	select distinct(cast(SubmittedOn as date)) as Date ,SUM(Quantity) AS UsageStock
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	WHERE cast(SubmittedOn as date) BETWEEN @startDate AND @endDate and EU.EnvelopeId=@Id
	GROUP BY SubmittedOn 
)
select * from cte  order by Date desc

	END