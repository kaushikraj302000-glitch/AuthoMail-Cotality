CREATE TABLE [Common].[DocTypePrintTrailMaps] (
    [DocTypeId]       INT      NOT NULL,
    [PrintTrailId]    INT      NOT NULL,
    [BizDayStartTime] TIME (7) NULL,
    [BizDayEndTime]   TIME (7) NULL,
    [SatStartTime]    TIME (7) NULL,
    [SatEndTime]      TIME (7) NULL,
    [SunStartTime]    TIME (7) NULL,
    [SunEndTime]      TIME (7) NULL,
    CONSTRAINT [FK_DocTypePrintTrailMaps_DocTypes] FOREIGN KEY ([DocTypeId]) REFERENCES [AM].[DocTypes] ([Id]),
    CONSTRAINT [FK_DocTypePrintTrailMaps_PrintTrails] FOREIGN KEY ([PrintTrailId]) REFERENCES [Common].[PrintTrails] ([Id])
);

