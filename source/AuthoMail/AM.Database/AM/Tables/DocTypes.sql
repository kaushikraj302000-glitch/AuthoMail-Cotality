CREATE TABLE [AM].[DocTypes] (
    [Id]                                  INT            IDENTITY (1, 1) NOT NULL,
    [DocTypeCode]                         VARCHAR (255)  NOT NULL,
    [ClientId]                            INT            NOT NULL,
    [PrintSize]                           VARCHAR (255)  NOT NULL,
    [ContentScalePercent]                 FLOAT (53)     NOT NULL,
    [PrintFormat]                         VARCHAR (255)  NULL,
    [QCPercent]                           FLOAT (53)     NULL,
    [ShippingCarrier]                     VARCHAR (255)  NULL,
    [ShippingServiceType]                 VARCHAR (255)  NULL,
    [ShippingAccountCode]                 VARCHAR (255)  NULL,
    [IsReturnInfoSpecified]               BIT            NULL,
    [RetRcptPrefix]                       VARCHAR (255)  NULL,
    [RetRcptFirstName]                    VARCHAR (255)  NULL,
    [RetRcptMiddleName]                   VARCHAR (255)  NULL,
    [RetRcptLastName]                     VARCHAR (255)  NULL,
    [RetRcptSuffix]                       VARCHAR (255)  NULL,
    [RetRcptPhone]                        VARCHAR (255)  NULL,
    [RetRcptOrganization]                 VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine1AboveStreet] VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine2AboveStreet] VARCHAR (255)  NULL,
    [RetRcptAddressStreet]                VARCHAR (255)  NULL,
    [RetRcptAddressAptOrSuiteOrOther]     VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine1BelowStreet] VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine2BelowStreet] VARCHAR (255)  NULL,
    [RetRcptAddressCity]                  VARCHAR (255)  NULL,
    [RetRcptAddressState]                 VARCHAR (255)  NULL,
    [RetRcptAddressZIP]                   VARCHAR (255)  NULL,
    [ClientEmails]                        VARCHAR (1024) NULL,
    [ClientContactNo]                     VARCHAR (255)  NULL,
    [SupportEmails]                       VARCHAR (1024) NULL,
    [SupportContactNo]                    VARCHAR (255)  NULL,
    [IsActive]                            BIT            NOT NULL,
    [ModifiedOn]                          DATETIME       NULL,
    [CreatedOn]                           DATETIME       NULL,
    [CreatedBy]                           NVARCHAR (128) NULL,
    [ModifiedBy]                          NVARCHAR (128) NULL,
    [Description]                         VARCHAR (512)  DEFAULT ('desc') NOT NULL,
    [BizdayCutOff]                        VARCHAR (255)  NULL,
    [BizdaySLAType]                       VARCHAR (255)  NULL,
    [BizdayTimeSpan]                      VARCHAR (255)  NULL,
    [SatCutOff]                           VARCHAR (255)  NULL,
    [SatSLAType]                          VARCHAR (255)  NULL,
    [SatTimeSpan]                         VARCHAR (255)  NULL,
    [SunCutOff]                           VARCHAR (255)  NULL,
    [SunSLAType]                          VARCHAR (255)  NULL,
    [SunTimeSpan]                         VARCHAR (255)  NULL,
    [ShippingSpecialService]              VARCHAR (255)  NULL,
    [IsBRE]                               BIT            CONSTRAINT [DF_DocTypes_IsBRE] DEFAULT ((0)) NOT NULL,
    [EnvelopeId]                          VARCHAR (255)  NULL,
    [BREEnvelopeId]                       VARCHAR (255)  NULL,
    [NoOfCopies]                          INT            NOT NULL,
    CONSTRAINT [PK_DocTypes] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_CreatedBy_DocTypes_User] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_DocTypes_BREEnvelope] FOREIGN KEY ([BREEnvelopeId]) REFERENCES [Common].[Envelopes] ([Id]),
    CONSTRAINT [FK_DocTypes_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_DocTypes_Envelope] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes] ([Id]),
    CONSTRAINT [FK_ModifiedBy_DocTypes_User] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DocTypes_ClientId_Description]
    ON [AM].[DocTypes]([ClientId] ASC, [Description] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DocTypes_ClientId_DocTypeCode]
    ON [AM].[DocTypes]([ClientId] ASC, [DocTypeCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_DocTypes_ClientId]
    ON [AM].[DocTypes]([ClientId] ASC) WITH (FILLFACTOR = 90);

