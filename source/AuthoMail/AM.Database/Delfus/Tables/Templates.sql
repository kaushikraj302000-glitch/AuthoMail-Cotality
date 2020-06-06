CREATE TABLE [Delfus].[Templates]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[FileName] VARCHAR (255) NOT NULL,
	[FilePath] varchar(255) NOT NULL,
	[UploadedById] NVARCHAR (128) NOT NULL,
	[UploadedOn] datetime NOT NULL,
	[IsRemoved] bit NOT NULL,
	[RemovedById] NVARCHAR (128) NULL,
	[RemovedOn] datetime NULL,
	CONSTRAINT [PK_Templates] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
	CONSTRAINT [FK_Users_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [Profile].[Users] ([Id]),
	CONSTRAINT [FK_Users_RemovedById] FOREIGN KEY ([RemovedById]) REFERENCES [Profile].[Users] ([Id])
)
