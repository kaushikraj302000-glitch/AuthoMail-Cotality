CREATE TABLE [Common].[Envelopes] (
    [Id]              VARCHAR (255)  NOT NULL,
    [CurrentQuantity] INT            NULL,
    [IsBREEnvelope]   BIT            NOT NULL,
    [CreatedBy]       NVARCHAR (128) NOT NULL,
    [ModifiedBy]      NVARCHAR (128) NOT NULL,
    [CreatedOn]       DATETIME       NOT NULL,
    [ModifiedOn]      DATETIME       NOT NULL,
    CONSTRAINT [PK_Envelopes] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CreatedBy_Envelopes_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_ModifiedBy_Envelopes_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    UNIQUE NONCLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

