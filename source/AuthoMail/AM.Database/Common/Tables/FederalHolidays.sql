CREATE TABLE [Common].[FederalHolidays] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [HolidayDate] DATE          NULL,
    [Description] VARCHAR (200) NULL,
    CONSTRAINT [PK_FederalHolidays] PRIMARY KEY CLUSTERED ([Id] ASC)
);

