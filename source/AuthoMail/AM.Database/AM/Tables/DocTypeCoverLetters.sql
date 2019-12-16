CREATE TABLE [AM].[DocTypeCoverLetters] (
    [DocTypeId]               INT            NOT NULL,
    [SeqId]                   INT            NOT NULL,
    [CoverLetterTemplatePath] VARCHAR (1024) NULL,
    CONSTRAINT [PK_DocTypeCoverLetters] PRIMARY KEY CLUSTERED ([DocTypeId] ASC, [SeqId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_DocTypeCoverLetters_DocTypes] FOREIGN KEY ([DocTypeId]) REFERENCES [AM].[DocTypes] ([Id])
);

