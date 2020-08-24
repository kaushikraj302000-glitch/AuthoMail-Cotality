CREATE TABLE [Common].[Envelopes] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [EnvelopeName]    VARCHAR (255)  NOT NULL,
    [EnvelopeDesc]    VARCHAR (255)  NULL,
    [PartNumber]      VARCHAR (255)  NULL,
    [IsBREEnvelope]   BIT            NOT NULL,
    [CreatedBy]       NVARCHAR (128) NOT NULL,
    [CreatedOn]       DATETIME       NOT NULL,
    [ModifiedBy]      NVARCHAR (128) NOT NULL,
    [ModifiedOn]      DATETIME       NOT NULL,
    CONSTRAINT [PK_Envelopes] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CreatedBy_Envelopes_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_ModifiedBy_Envelopes_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [UQ_EnvelopeName] UNIQUE NONCLUSTERED ([EnvelopeName] ASC) WITH (FILLFACTOR = 90)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Envelopes_EnvelopeName_PartNumber]
    ON [Common].[Envelopes]([EnvelopeName] ASC, [PartNumber] ASC) WITH (FILLFACTOR = 90);

