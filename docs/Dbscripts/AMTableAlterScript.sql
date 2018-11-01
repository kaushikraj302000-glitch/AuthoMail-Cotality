CREATE TABLE Common.PrintVendors ( Id int NOT NULL  IDENTITY(1,1),Name varchar(255) null CONSTRAINT [PK_PrintVendors_Id] PRIMARY KEY CLUSTERED 
(
 [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 

ALter Table Common.PrintVendors
Add SFTChannelId int null;

ALTER TABLE Common.PrintVendors  WITH CHECK ADD  CONSTRAINT [FK_PrintVendors_SFTChannels] FOREIGN KEY([SFTChannelId])
REFERENCES [Common].[SFTChannels] ([Id])

CREATE TABLE Common.PrintTrails (Id int NOT NULL  IDENTITY(1,1), TrailName varchar(255) NULL, PrintVendorId int NOT NULL CONSTRAINT [PK_PrintTrails_Id] PRIMARY KEY CLUSTERED 
(
 [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 

ALTER TABLE Common.PrintTrails  WITH CHECK ADD  CONSTRAINT [FK_PrintTrails_PrintVendors] FOREIGN KEY([PrintVendorId])
REFERENCES [Common].[PrintVendors] ([Id])


CREATE TABLE Common.DocTypePrintTrailMaps (DocTypeId int NOT NULL, PrintTrailId int NOT NULL, BizDayStartTime time NOT NULL, BizDayEndTime time NOT NULL,SatStartTime time NOT NULL,SatEndTime time NOT NULL,SunStartTime time NOT NULL,SunEndTime time NOT NULL CONSTRAINT [PK_DocTypeTrailMapsId] PRIMARY KEY CLUSTERED 
(
 [DocTypeId] ASC,
 [PrintTrailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 

ALTER TABLE Common.DocTypePrintTrailMaps  WITH CHECK ADD  CONSTRAINT [FK_DocTypeTrailMaps_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])


ALTER TABLE Common.DocTypePrintTrailMaps  WITH CHECK ADD  CONSTRAINT [FK_DocTypeTrailMaps_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])


ALter Table AM.MailPieces
Add PrintTrailId int null;

ALTER TABLE AM.MailPieces  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])


CREATE TABLE PnS.PrintTrailBatches (Id int NOT NULL IDENTITY(1,1),PrintBatchName varchar(255) NULL,SubmittedOn datetime NOT NULL,MRDFReceivedOn datetime NULL,PrintTrailId int NOT NULL CONSTRAINT [PK_PrintTrailBatches_Id] PRIMARY KEY CLUSTERED 
(
 [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 

ALTER TABLE PnS.PrintTrailBatches  WITH CHECK ADD  CONSTRAINT [FK_PrintTrailBatches_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])

ALter Table PnS.MRDFResponse
Add PrintTrailBatchId int null;

ALTER TABLE PnS.MRDFResponse  WITH CHECK ADD  CONSTRAINT [FK_MRDFResponse_PrintTrailBatches] FOREIGN KEY([PrintTrailBatchId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])

ALter Table PnS.MRDFRequest
Add PrintTrailBatchId int null;

ALTER TABLE PnS.MRDFRequest  WITH CHECK ADD  CONSTRAINT [FK_MRDFRequest_PrintTrailBatches] FOREIGN KEY([PrintTrailBatchId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])

ALter Table PnS.BatchSummary
Add PrintTrailId int null;

ALTER TABLE PnS.BatchSummary  WITH CHECK ADD  CONSTRAINT [FK_BatchSummary_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])


ALTER TABLE AM.PrintBatches
ADD PrintTrailId INT NULL;


ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_PrintTrails]
GO

insert into Common.PrintVendors Values ('Fort Mill, SC',1)
insert into Common.PrintVendors Values ('Dallas CL',1)

insert into Common.PrintTrails  Values ('ALASKA-GENERIC-9X12',1)
insert into Common.PrintTrails  Values ('ALASKA-GENERIC-PRI',1)
insert into Common.PrintTrails  Values ('CITIVM-GENERIC-PRI',1)
insert into Common.PrintTrails  Values ('CITIVM-GENERIC-9X12',1)
insert into Common.PrintTrails  Values ('HSBC-GENERICBRE-9X12',1)
insert into Common.PrintTrails  Values ('HSBC-GENERIC-9X12',1)
insert into Common.PrintTrails  Values ('HSBC-GENERICBRE-PRI',1)
insert into Common.PrintTrails  Values ('HSBC-GENERIC-PRI',1)
insert into Common.PrintTrails  Values ('CONDOSAFE-Generic-FEDEX',1)
insert into Common.PrintTrails  Values ('CONDOSAFE-Generic_U-PRI',1)
insert into Common.PrintTrails  Values ('DALLAS-90days-CERT',1)
insert into Common.PrintTrails  Values ('DALLAS-GENERIC-10',1)
insert into Common.PrintTrails  Values ('DALLAS-Statements-10',1)
insert into Common.PrintTrails  Values ('RPS-GENERIC-6X9',1)
insert into Common.PrintTrails  Values ('RPS-GENERIC-9X12',1)
insert into Common.PrintTrails  Values ('USAA-GENERICBRE-10',1)
insert into Common.PrintTrails  Values ('USAA-GENBRESAME-10',1)
insert into Common.PrintTrails  Values ('USAA-GENERIC-FEDEX',1)
insert into Common.PrintTrails  Values ('USAA-GENERIC-PRI',1)




