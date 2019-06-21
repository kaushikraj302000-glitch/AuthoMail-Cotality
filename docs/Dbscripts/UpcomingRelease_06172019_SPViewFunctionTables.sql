
PRINT 'Added two new column in Common.Envelopes table'
ALTER TABLE Common.Envelopes
ADD ReorderDuration INT NULL,MinStock INT NULL

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
