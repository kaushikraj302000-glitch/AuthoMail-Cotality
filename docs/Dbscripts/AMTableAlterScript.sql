--Create Schema_MRDFResponseHistory table for maintain History

CREATE TABLE [PnS].[Schema_MRDFResponseHistory](
	[Id] [int] NOT NULL,
	[FieldName] [nvarchar](255) NULL,
	[Type] [nvarchar](255) NULL,
	[StartPosition] [int] NOT NULL,
	[Length] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[SampleValue] [nvarchar](255) NULL,
	[DataType] [nvarchar](255) NULL,
	[DataFormatTemplate] [nvarchar](255) NULL,
 CONSTRAINT [PK_SchemaMRDFResponseHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
-- Insert data from table Schema_MRDFResponse to Schema_MRDFResponseHistory
-- INSERT INTO [PnS].[Schema_MRDFResponseHistory]
-- SELECT * FROM [PnS].[Schema_MRDFResponse]

-- delete data from table Schema_MRDFResponse after move into Schema_MRDFResponseHistory
-- delete  from [PnS].[Schema_MRDFResponse]


CREATE TABLE [Common].[PrintVendors](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[SFTChannelId] [int] NULL,
	[BusinessCode] [varchar](255) NULL,
 CONSTRAINT [PK_PrintVendors_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Common].[PrintVendors]  WITH CHECK ADD  CONSTRAINT [FK_PrintVendors_SFTChannels] FOREIGN KEY([SFTChannelId])
REFERENCES [Common].[SFTChannels] ([Id])

ALTER TABLE [Common].[PrintVendors] CHECK CONSTRAINT [FK_PrintVendors_SFTChannels]


CREATE TABLE [Common].[PrintTrails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TrailName] [varchar](255) NULL,
	[PrintVendorId] [int] NOT NULL,
	[SimulateTrail] BIT NULL,
 CONSTRAINT [PK_PrintTrails_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Common].[PrintTrails]  WITH CHECK ADD  CONSTRAINT [FK_PrintTrails_PrintVendors] FOREIGN KEY([PrintVendorId])
REFERENCES [Common].[PrintVendors] ([Id])
GO

ALTER TABLE [Common].[PrintTrails] CHECK CONSTRAINT [FK_PrintTrails_PrintVendors]
GO

CREATE TABLE [Common].[DocTypePrintTrailMaps](
	[DocTypeId] [int] NOT NULL,
	[PrintTrailId] [int] NOT NULL,
	[BizDayStartTime] [time](7) NULL,
	[BizDayEndTime] [time](7) NULL,
	[SatStartTime] [time](7) NULL,
	[SatEndTime] [time](7) NULL,
	[SunStartTime] [time](7) NULL,
	[SunEndTime] [time](7) NULL,
 CONSTRAINT [PK_DocTypeTrailMapsId] PRIMARY KEY CLUSTERED 
(
	[DocTypeId] ASC,
	[PrintTrailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Common].[DocTypePrintTrailMaps]  WITH CHECK ADD  CONSTRAINT [FK_DocTypePrintTrailMaps_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO

ALTER TABLE [Common].[DocTypePrintTrailMaps] CHECK CONSTRAINT [FK_DocTypePrintTrailMaps_DocTypes]
GO

ALTER TABLE [Common].[DocTypePrintTrailMaps]  WITH CHECK ADD  CONSTRAINT [FK_DocTypePrintTrailMaps_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])


CREATE TABLE [PnS].[PrintTrailBatches](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PrintBatchName] [varchar](255) NULL,
	[SubmittedOn] [datetime] NULL,
	[MRDFReceivedOn] [datetime] NULL,
	[PrintTrailId] [int] NOT NULL,
	[IsSubmitted]  AS (case when [SubmittedOn] IS NULL then (0) else (1) end),
	[IsMRDFReceived]  AS (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
	[SuspendedOn] [datetime] NULL,
	[IsSuspended]  AS (case when [SuspendedOn] IS NULL then (0) else (1) end), 
	[ValidationErrors] VARCHAR(255) NULL,
 CONSTRAINT [PK_PrintTrailBatches_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [PnS].[PrintTrailBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintTrailBatches_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [PnS].[PrintTrailBatches] CHECK CONSTRAINT [FK_PrintTrailBatches_PrintTrails]
GO


ALTER TABLE [AM].[MailPieces]
ADD [PrintTrailId] [int] NULL
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [AM].[MailPieces] CHECK CONSTRAINT [FK_MailPieces_PrintTrails]
GO

ALTER TABLE [AM].[PrintBatches]
ADD [PrintTrailId] [int] NULL, [MailBatchId] [int] NULL
ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_MailBatches] FOREIGN KEY([MailBatchId])
REFERENCES [AM].[MailBatches] ([Id])
GO

ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_MailBatches]
GO

ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_PrintTrails]
GO

ALTER TABLE [PnS].[BatchSummary]
ADD [PrintTrailId] [int] NULL

ALTER TABLE [PnS].[BatchSummary]  WITH CHECK ADD  CONSTRAINT [FK_BatchSummary_PrintTrailBatches] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [PnS].[BatchSummary] CHECK CONSTRAINT [FK_BatchSummary_PrintTrailBatches]
GO


ALTER TABLE [PnS].[MRDFRequest] 
ADD [PrintTrailBatchId] [int] NULL

ALTER TABLE [PnS].[MRDFRequest]  WITH CHECK ADD  CONSTRAINT [FK_MRDFRequest_PrintTrailBatches] FOREIGN KEY([PrintTrailBatchId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])
GO

ALTER TABLE [PnS].[MRDFRequest] CHECK CONSTRAINT [FK_MRDFRequest_PrintTrailBatches]
GO

/****** Object:  Index [PK_MRDFResponse]    Script Date: 11/1/2018 6:54:55 PM ******/
ALTER TABLE [PnS].[MRDFResponse] DROP CONSTRAINT [PK_MRDFResponse]
GO

/****** Object:  Index [PK_MRDFResponse]    Script Date: 11/1/2018 6:54:55 PM ******/
ALTER TABLE [PnS].[MRDFResponse] ADD  CONSTRAINT [PK_MRDFResponse] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

ALTER TABLE [PnS].[MRDFResponse]  WITH CHECK ADD  CONSTRAINT [FK_MRDFResponse_BatchSummary] FOREIGN KEY([JobId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])
GO

ALTER TABLE [PnS].[MRDFResponse] CHECK CONSTRAINT [FK_MRDFResponse_BatchSummary]
GO

ALTER TABLE [Common].[Clients] DROP CONSTRAINT [DF__Clients__Simulat__7E850515]
ALTER TABLE [Common].[Clients]
Drop COLUMN SimulatePCI

-- Mapping Data
INSERT INTO Common.PrintVendors([Name],[SFTChannelId],[BusinessCode]) Values ('Fort Mill, SC',1,'002416')
INSERT INTO Common.PrintVendors([Name],[SFTChannelId],[BusinessCode]) Values ('Dallas CL',1,'ADE')

INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('ALASKA-GENERIC-9X12',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('ALASKA-GENERIC-PRI',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('CITIVM-GENERIC-PRI',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('CITIVM-GENERIC-9X12',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('HSBC-GENERIC-9X12',2,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('HSBC-GENERIC-PRI',2,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('DALLAS-90days-CERT',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('DALLAS-GENERIC-10',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('DALLAS-Statements-10',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('RPS-GENERIC-6X9',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('RPS-GENERIC-9X12',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENERIC-10',1,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENERIC-FEDEX',2,0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENERIC-PRI',2,0)




