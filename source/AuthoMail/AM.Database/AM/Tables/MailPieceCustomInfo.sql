CREATE TABLE [AM].[MailPieceCustomInfo] (
    [MailPieceId] INT            NOT NULL,
    [Key]         VARCHAR (128)  NOT NULL,
    [DataType]    VARCHAR (128)  NULL,
    [Value]       VARCHAR (2048) NULL,
    CONSTRAINT [PK_MailPieceCustomInfo] PRIMARY KEY CLUSTERED ([MailPieceId] ASC, [Key] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_MailPieceCustomInfo_MailPieces] FOREIGN KEY ([MailPieceId]) REFERENCES [AM].[MailPieces] ([Id])
);

