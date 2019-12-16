CREATE TABLE [AM].[LogBatchTimeLine] (
    [MailBatchId]                   INT      NOT NULL,
    [DownloadStart]                 DATETIME NULL,
    [DownloadEnd]                   DATETIME NULL,
    [ValidationStart]               DATETIME NULL,
    [ValidationEnd]                 DATETIME NULL,
    [AcknowledgeStart]              DATETIME NULL,
    [AcknowledgeEnd]                DATETIME NULL,
    [PreppedStart]                  DATETIME NULL,
    [PreppedEnd]                    DATETIME NULL,
    [SubmittedToShippingStart]      DATETIME NULL,
    [SubmittedToShippingEnd]        DATETIME NULL,
    [ShippingResponseReceivedStart] DATETIME NULL,
    [ShippingResponseReceivedEnd]   DATETIME NULL,
    [ReportPrepStart]               DATETIME NULL,
    [ReportPrepEnd]                 DATETIME NULL,
    [ReportBackStart]               DATETIME NULL,
    [ReportBackEnd]                 DATETIME NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_DownloadStart]
    ON [AM].[LogBatchTimeLine]([DownloadStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_DownloadEnd]
    ON [AM].[LogBatchTimeLine]([DownloadEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ValidationStart]
    ON [AM].[LogBatchTimeLine]([ValidationStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ValidationEnd]
    ON [AM].[LogBatchTimeLine]([ValidationEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_AcknowledgeStart]
    ON [AM].[LogBatchTimeLine]([AcknowledgeStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_AcknowledgeEnd]
    ON [AM].[LogBatchTimeLine]([AcknowledgeEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_PreppedStart]
    ON [AM].[LogBatchTimeLine]([PreppedStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_PreppedEnd]
    ON [AM].[LogBatchTimeLine]([PreppedEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_SubmittedToShippingStart]
    ON [AM].[LogBatchTimeLine]([SubmittedToShippingStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_SubmittedToShippingEnd]
    ON [AM].[LogBatchTimeLine]([SubmittedToShippingEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ShippingResponseReceivedStart]
    ON [AM].[LogBatchTimeLine]([ShippingResponseReceivedStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ShippingResponseReceivedEnd]
    ON [AM].[LogBatchTimeLine]([ShippingResponseReceivedEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportPrepStart]
    ON [AM].[LogBatchTimeLine]([ReportPrepStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportPrepEnd]
    ON [AM].[LogBatchTimeLine]([ReportPrepEnd] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportBackStart]
    ON [AM].[LogBatchTimeLine]([ReportBackStart] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportBackEnd]
    ON [AM].[LogBatchTimeLine]([ReportBackEnd] ASC) WITH (FILLFACTOR = 90);

