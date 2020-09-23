CREATE TABLE [API].[AMRequestLogs]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PackageId] UNIQUEIDENTIFIER NOT NULL, 
    [IsPrimaryRequest] BIT NOT NULL, 
    [Status] VARCHAR(255) NOT NULL, 
    [HTTPStatus] INT NULL, 
    [APIResponse] NVARCHAR(2048) NULL, 
    [ReceivedOn] DATETIME NOT NULL, 
    [ProcessedOn] DATETIME NULL, 
    [ErrorLog] NVARCHAR(MAX) NULL,
    [SizeCheck] DECIMAL(7, 2) NULL, 
    [Download] DECIMAL(7, 2) NULL, 
    [Validate] DECIMAL(7, 2) NULL, 
    [Merge] DECIMAL(7, 2) NULL, 
    [PostBatch] DECIMAL(7, 2) NULL, 
    [Cleanup] DECIMAL(7, 2) NULL, 
    [TotalTime] DECIMAL(7, 2) NULL
)

GO

CREATE INDEX [IX_AMRequestLogs_PackageId] ON [API].[AMRequestLogs] ([PackageId])
