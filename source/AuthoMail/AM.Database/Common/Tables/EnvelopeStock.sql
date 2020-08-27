CREATE TABLE [Common].[EnvelopeStock]
(
	[EnvelopeId] INT NOT NULL, 
    [ShipperId] INT NOT NULL, 
    [CurrentQuantity] INT NOT NULL, 
    [ReOrderDuration] INT NOT NULL, 
    CONSTRAINT [FK_EnvelopeStock_Envelope] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes]([Id]), 
    CONSTRAINT [FK_EnvelopeStock_ShipperInformation] FOREIGN KEY ([ShipperId]) REFERENCES [Common].[ShipperInformation]([Id])
)
