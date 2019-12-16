CREATE TABLE [Common].[ClientDomains] (
    [ClientId]      INT           NOT NULL,
    [AllowedDomain] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_ClientDomains] PRIMARY KEY CLUSTERED ([AllowedDomain] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ClientDomains_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]) ON DELETE CASCADE
);

