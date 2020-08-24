CREATE TABLE [Common].[EnvelopeStock]
(
	[EnvelopeId] INT NOT NULL, 
    [ShippedId] INT NOT NULL, 
    [CurrentQuantity] INT NOT NULL, 
    [ReOrderDuration] INT NOT NULL, 
    CONSTRAINT [FK_EnvelopeStock_Envelope] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes]([Id]), 
    CONSTRAINT [FK_EnvelopeStock_ShipperInformation] FOREIGN KEY ([ShippedId]) REFERENCES [Common].[ShipperInformation]([Id])
)
