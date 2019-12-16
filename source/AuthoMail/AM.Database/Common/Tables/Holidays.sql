CREATE TABLE [Common].[Holidays] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [HolidayDate] DATE          NULL,
    [Description] VARCHAR (200) NULL,
    CONSTRAINT [PK_Holidays] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

