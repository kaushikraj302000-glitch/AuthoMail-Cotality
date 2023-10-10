CREATE TABLE [AM].[SPELogs]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ClientId] INT NULL, 
    [CreatedBy] INT NULL, 
    [CreatedOn] DATETIME NULL, 
    [Status] VARCHAR(50) NULL, 
    [ErrorDesc] VARCHAR(MAX) NULL
)
