CREATE TABLE [Delfus].[Templates]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[FileName] VARCHAR (255) NOT NULL,
	[FilePath] varchar(255) NOT NULL,
	[ModifiedById] NVARCHAR (128) NOT NULL,
	[ModifiedOn] datetime NOT NULL,
	[Status] NVARCHAR(250) NOT NULL, 
    CONSTRAINT [PK_Templates] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
	CONSTRAINT [FK_Users_UploadedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Profile].[Users] ([Id])
)
