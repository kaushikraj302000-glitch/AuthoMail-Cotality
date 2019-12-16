CREATE TABLE [PnS].[EnvelopeUsage] (
    [PrintTrailBatchId] INT      NULL,
    [Quantity]          INT      NULL,
    [InsertedOn]        DATETIME NULL,
    [EnvelopeId]        INT      NULL,
    CONSTRAINT [FK_EnvelopeUsage_PrintTrailBatches] FOREIGN KEY ([PrintTrailBatchId]) REFERENCES [PnS].[PrintTrailBatches] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [Idx_EnvelopeUsage_PrintTrailBatchId]
    ON [PnS].[EnvelopeUsage]([PrintTrailBatchId] ASC);

