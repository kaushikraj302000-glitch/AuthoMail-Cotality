CREATE TABLE [Common].[Envelopes] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [EnvelopeName]    VARCHAR (255)  NOT NULL,
    [CurrentQuantity] INT            NULL,
    [IsBREEnvelope]   BIT            NOT NULL,
    [CreatedBy]       NVARCHAR (128) NOT NULL,
    [ModifiedBy]      NVARCHAR (128) NOT NULL,
    [CreatedOn]       DATETIME       NOT NULL,
    [ModifiedOn]      DATETIME       NOT NULL,
    [ReorderDuration] INT            NULL,
    [PartNumber]      VARCHAR (255)  NULL,
    [EnvelopeDesc]    VARCHAR (255)  NULL,
    CONSTRAINT [PK_Envelopes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CreatedBy_Envelopes_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_ModifiedBy_Envelopes_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [UQ_EnvelopeName] UNIQUE NONCLUSTERED ([EnvelopeName] ASC)
);

