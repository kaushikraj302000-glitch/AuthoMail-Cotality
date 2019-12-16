CREATE TABLE [Common].[PrintTrails] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [TrailName]     VARCHAR (255) NULL,
    [PrintVendorId] INT           NOT NULL,
    [SimulateTrail] BIT           NULL,
    CONSTRAINT [PK_PrintTrails_Id] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_PrintTrails_PrintVendors] FOREIGN KEY ([PrintVendorId]) REFERENCES [Common].[PrintVendors] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PrintVendor_TrailName]
    ON [Common].[PrintTrails]([TrailName] ASC, [PrintVendorId] ASC) WITH (FILLFACTOR = 90);

