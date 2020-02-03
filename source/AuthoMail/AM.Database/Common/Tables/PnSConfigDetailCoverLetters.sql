CREATE TABLE [Common].[PnSConfigDetailCoverLetters] (
    [PnSConfigDetailId]       INT            NOT NULL,
    [SeqId]                   INT            NOT NULL,
    [CoverLetterTemplatePath] VARCHAR (1024) NULL,
    CONSTRAINT [PK_PnSConfigDetailCoverLetters] PRIMARY KEY CLUSTERED ([PnSConfigDetailId] ASC, [SeqId] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_PnSConfigDetailCoverLetters_PnSConfigDetail] FOREIGN KEY ([PnSConfigDetailId]) REFERENCES [Common].[PnSConfigDetail] ([Id])
);

