CREATE TABLE [Common].[PnSConfigDetailPrintTrailMaps] (
    [PnSConfigDetailId] INT      NOT NULL,
    [PrintTrailId]      INT      NOT NULL,
    [BizDayStartTime]   TIME (7) NULL,
    [BizDayEndTime]     TIME (7) NULL,
    [SatStartTime]      TIME (7) NULL,
    [SatEndTime]        TIME (7) NULL,
    [SunStartTime]      TIME (7) NULL,
    [SunEndTime]        TIME (7) NULL,
    CONSTRAINT [FK_PnSConfigDetailPrintTrailMaps_PnSConfigDetail] FOREIGN KEY ([PnSConfigDetailId]) REFERENCES [Common].[PnSConfigDetail] ([Id]),
    CONSTRAINT [FK_PnSConfigDetailPrintTrailMaps_PrintTrails] FOREIGN KEY ([PrintTrailId]) REFERENCES [Common].[PrintTrails] ([Id])
);

