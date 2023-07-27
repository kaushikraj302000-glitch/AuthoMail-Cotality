CREATE TABLE [AM].[USPSLabelCutOffDetails]
(
	[Id] INT PRIMARY KEY IDENTITY (1, 1) NOT NULL,
    [USPSUserId] VARCHAR(30) NOT NULL, 
    [BizStartTime] TIME NULL, 
    [BizEndTime] TIME NULL, 
    [SatStartTime] TIME NULL, 
    [SatEndTime] TIME NULL, 
    [PrintVendorCode] VARCHAR(255) NOT NULL, 
    [PrintVendorLocation] VARCHAR(255) NOT NULL
)
