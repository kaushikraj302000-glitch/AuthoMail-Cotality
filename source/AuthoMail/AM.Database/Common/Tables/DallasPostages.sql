CREATE TABLE [Common].[DallasPostages]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MinPage] INT NULL, 
    [MaxPage] INT NULL, 
    [Weight] DECIMAL(7, 4) NULL, 
    [PostageCost] DECIMAL(7, 4) NULL
)
