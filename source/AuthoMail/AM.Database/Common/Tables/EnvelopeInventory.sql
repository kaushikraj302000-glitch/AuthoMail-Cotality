CREATE TABLE [Common].[EnvelopeInventory] (
    [Id]               INT            IDENTITY (10000, 1) NOT NULL,
    [EnvelopeId]       VARCHAR (255)  NOT NULL,
    [Description]      VARCHAR (2048) NULL,
    [ReceivedOn]       DATETIME       NULL,
    [ReceivedQuantity] INT            NOT NULL,
    [UnitPrice]        DECIMAL (5, 2) NULL,
    [CreatedBy]        NVARCHAR (128) NOT NULL,
    [ModifiedBy]       NVARCHAR (128) NOT NULL,
    [CreatedOn]        DATETIME       NOT NULL,
    [ModifiedOn]       DATETIME       NOT NULL,
    CONSTRAINT [PK_EnvelopeInventory] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CreatedBy_EnvelopeInventory_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_EnvelopeId_EnvelopeInventory_Users] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes] ([Id]),
    CONSTRAINT [FK_ModifiedBy_EnvelopeInventory_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id])
);

