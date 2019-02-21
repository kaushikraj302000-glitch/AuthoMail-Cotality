SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION


PRINT N'Start execution'
----------------------------------------------------------------------------
---- SETUP BOTW CLIENT

INSERT [Common].[Clients] ([Name], [AppCode], [BusinessUnit], [IsAMActive], [IsPnSActive], [TimeZone], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy]) VALUES (N'Bank Of The West', N'BOTW', 1, 0, 0, N'Eastern Standard Time', GETUTCDATE(), GETUTCDATE(), N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11', N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11')


-- CLIENT SFT CHANNEL MAP
INSERT INTO [Common].[ClientSFTChannels] (ClientId,SFTChannelId) VALUES ((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'),(SELECT ID FROM Common.SFTChannels WHERE Code='AMC_PROD'))

-- Client Domain
INSERT INTO [Common].[ClientDomains] ([ClientId],[AllowedDomain])VALUES((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'),'bankofthewest.com')

--AMC Customer Id
INSERT INTO [AM].[ClientAMCCustIdMap](AMCCustId,ClientId) VALUES('180',(SELECT Id FROM Common.Clients WHERE AppCode='BOTW'))

-- Client Business Identifier
INSERT INTO [AM].[ClientBusinessIdentifiers](ClientId, BusinessIdName) VALUES ((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'),'Loan Number')

-- EMAIL CHANNELS FOR CLIENT
INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('BOTW Download Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Download Summary','HTML',0)

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('BOTW Production Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Production Summary','HTML',0)

-- DELIVERABLES FOR CLIENT
INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'),'BOTW Error Report','Error','Batch','','\t','Error-{Now:yyyyMMdd}.csv',1,'/Reports',(SELECT Id FROM Common.EmailChannels WHERE Title='BOTW Download Reporting'),NULL)

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'),'BOTW Delivery Report','Delivery','Batch','','\t','Delivery-{Now:yyyyMMdd}.csv',1,'/Reports',(SELECT Id FROM Common.EmailChannels WHERE Title='BOTW Production Reporting'),NULL)

-- PRINT TRAIL FOR CLIENT
INSERT INTO [Common].[PrintTrails](PrintVendorId,SimulateTrail,TrailName) VALUES ((SELECT Id FROM Common.PrintVendors WHERE VendorCode='CL_DALLAS'),0,'BOTW-GENERIC')

--DOCUMENT TYPES ENTRY
INSERT [AM].[DocTypes] ([DocTypeCode], [ClientId], [PrintSize], [ContentScalePercent], [PrintFormat], [QCPercent], [ShippingCarrier], [ShippingServiceType], [ShippingAccountCode], [IsReturnInfoSpecified], [RetRcptPrefix], [RetRcptFirstName], [RetRcptMiddleName], [RetRcptLastName], [RetRcptSuffix], [RetRcptPhone], [RetRcptOrganization], [RetRcptAddrOptionalLine1AboveStreet], [RetRcptAddrOptionalLine2AboveStreet], [RetRcptAddressStreet], [RetRcptAddressAptOrSuiteOrOther], [RetRcptAddrOptionalLine1BelowStreet], [RetRcptAddrOptionalLine2BelowStreet], [RetRcptAddressCity], [RetRcptAddressState], [RetRcptAddressZIP], [ClientEmails], [ClientContactNo], [SupportEmails], [SupportContactNo], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [Description], [BizdayCutOff], [BizdaySLAType], [BizdayTimeSpan], [SatCutOff], [SatSLAType], [SatTimeSpan], [SunCutOff], [SunSLAType], [SunTimeSpan], [ShippingSpecialService], [IsBRE], [EnvelopeId], [BREEnvelopeId], [NoOfCopies]) VALUES (N'InitialDisclosures', (SELECT Id FROM Common.Clients WHERE AppCode='BOTW'), N'LETTER-IP', 100, N'Simplex', 0, N'USPS', N'IMB', NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE(), N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11', N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11', N'Initial Disclosures', N'19:30:00', N'FixedTime', N'00:23:59:59', N'14:00:00', N'FixedTime', N'00:23:59:59', NULL, N'NA', NULL, NULL,0,NULL,NULL,1)

INSERT [AM].[DocTypes] ([DocTypeCode], [ClientId], [PrintSize], [ContentScalePercent], [PrintFormat], [QCPercent], [ShippingCarrier], [ShippingServiceType], [ShippingAccountCode], [IsReturnInfoSpecified], [RetRcptPrefix], [RetRcptFirstName], [RetRcptMiddleName], [RetRcptLastName], [RetRcptSuffix], [RetRcptPhone], [RetRcptOrganization], [RetRcptAddrOptionalLine1AboveStreet], [RetRcptAddrOptionalLine2AboveStreet], [RetRcptAddressStreet], [RetRcptAddressAptOrSuiteOrOther], [RetRcptAddrOptionalLine1BelowStreet], [RetRcptAddrOptionalLine2BelowStreet], [RetRcptAddressCity], [RetRcptAddressState], [RetRcptAddressZIP], [ClientEmails], [ClientContactNo], [SupportEmails], [SupportContactNo], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [Description], [BizdayCutOff], [BizdaySLAType], [BizdayTimeSpan], [SatCutOff], [SatSLAType], [SatTimeSpan], [SunCutOff], [SunSLAType], [SunTimeSpan], [ShippingSpecialService], [IsBRE], [EnvelopeId], [BREEnvelopeId], [NoOfCopies]) VALUES (N'InitialDisclosures_Equity', (SELECT Id FROM Common.Clients WHERE AppCode='BOTW'), N'LETTER-IP', 100, N'Simplex', 0, N'USPS', N'IMB', NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE(), N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11', N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11', N'Initial Disclosures - Equity', N'19:30:00', N'FixedTime', N'00:23:59:59', N'14:00:00', N'FixedTime', N'00:23:59:59', NULL, N'NA', NULL, NULL,0,NULL,NULL,1)

-- COVER LETTERS ENTRY
INSERT [AM].[DocTypeCoverLetters] ([DocTypeId], [SeqId], [CoverLetterTemplatePath])
SELECT Id,1,'BOTW_USPS_Generic_1.pdf' FROM AM.DocTypes WHERE ClientId = (SELECT Id FROM Common.Clients WHERE AppCode='BOTW')

-- DOCUMENT PRINT TRAIL MAPS
INSERT INTO [Common].[DocTypePrintTrailMaps](DocTypeId, PrintTrailId, BizDayStartTime, BizDayEndTime, SatStartTime, SatEndTime, SunStartTime, SunEndTime)
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='BOTW-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL From AM.DocTypes WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='BOTW') AND DocTypeCode IN ('InitialDisclosures','InitialDisclosures_Equity')


INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('CL_S_9_12',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('CL_S_6_9',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('CL_S_10',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('USAA_S_10',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('HSBC_B_9_12',0,1,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('BOTW_F_9_12',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('BOTW_B_10_13',0,1,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())


--USAA
UPDATE AM.DocTypes
SET
EnvelopeId=(select Id from Common.Envelopes WHERE Id='USAA_S_10')
WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='USAA') AND ShippingCarrier='USPS' AND ShippingServiceType='IMB'

--CITI, ALASKA, HSBC
UPDATE AM.DocTypes
SET
EnvelopeId=(select Id from Common.Envelopes WHERE Id='CL_S_9_12')
WHERE ClientId IN ((SELECT Id FROM Common.Clients WHERE AppCode='CITIVM'),(SELECT Id FROM Common.Clients WHERE AppCode='ALASKA'),(SELECT Id FROM Common.Clients WHERE AppCode='HSBC'))

--HSBC
UPDATE AM.DocTypes
SET
BREEnvelopeId=CASE WHEN IsBRE=1 THEN (select Id from Common.Envelopes WHERE Id='HSBC_B_9_12') ELSE NULL END
WHERE ClientId IN ((SELECT Id FROM Common.Clients WHERE AppCode='HSBC'))

--RPS
UPDATE AM.DocTypes
SET
EnvelopeId=(select Id from Common.Envelopes WHERE Id='CL_S_6_9')
WHERE ClientId in ((SELECT Id FROM Common.Clients WHERE AppCode='RPS'))
--BOTW
UPDATE AM.DocTypes
SET EnvelopeId=(select Id from Common.Envelopes WHERE Id='BOTW_F_9_12'),
BREEnvelopeId=CASE WHEN IsBRE=1 THEN (select Id from Common.Envelopes WHERE Id='BOTW_B_10_13') ELSE NULL END
WHERE ClientId IN ((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'))



---- UPDATE CUTOFF TIMINGS FOR PRINT READY CLIENTS

UPDATE AM.DocTypes
SET
[ShippingCarrier]='USPS',
[ShippingServiceType]='IMB',
[BizdayCutOff]='20:00:00',
[BizdaySLAType]='FixedTime',
[BizdayTimeSpan]='00:23:59:59',
[SatCutOff]=NULL,
[SatSLAType]='NA',
[SatTimeSpan]=NULL,
[SunCutOff]=NULL,
[SunSLAType]='NA',
[SunTimeSpan]=NULL
WHERE ClientId in ((SELECT Id FROM Common.Clients WHERE AppCode='CITIVM'),(SELECT Id FROM Common.Clients WHERE AppCode='RPS'))

---- INSERT DELIVERABLES FOR PRINT READY CLIENTS

-- Email Channel Data for CITIVM

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('CITIVM Download Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Download Summary','HTML',0)

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('CITIVM Production Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Production Summary','HTML',0)

-- Deliverables Data for CITIVM

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='CITIVM'),'Error Report','Error','Batch','','\t','Error-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='CITIVM Download Reporting'),NULL)

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='CITIVM'),'Delivery Report','Delivery','Batch','','\t','Delivery-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='CITIVM Production Reporting'),NULL)


-- Email Channel Data for RPS

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('RPS Download Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Download Summary','HTML',0)

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('RPS Production Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Production Summary','HTML',0)


-- Deliverables Data for RPS

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='RPS'),'Error Report','Error','Batch','','\t','Error-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='RPS Download Reporting'),NULL)

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='RPS'),'Delivery Report','Delivery','Batch','','\t','Delivery-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='RPS Production Reporting'),NULL)



---- INSERT SFTCHANNEL DETAILD FOR PRINT READY CLIENTS

INSERT [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES ( N'PrintReady_Prod', NULL, N'Print ready client Process', N'unc', N'\\Slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS\PROD\AM_PRINT_READY', N'user', N'', N'/', NULL, N'', N'Delete', 1)

EXEC usp_MapClientSFTChannels 'CITIVM','PrintReady_Prod'
EXEC usp_MapClientSFTChannels 'RPS','PrintReady_Prod'


---- INSERT AND UPDATE PRINT VENDORS PATHS IN SFTCHANNELS

INSERT [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES ( N'PnSService_Prod', NULL, N'PnS Service Process', N'unc', N'\\slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS\PROD', N'user', N'', N'/', N'/', N'', N'Delete', 1)


---- UPDATE PRINT VENDORS SFTCHANNEL

UPDATE Common.PrintVendors
SET
SFTChannelId=(SELECT Id FROM Common.SFTChannels WHERE Code='PCI_Prod')
WHERE Id=(SELECT Id FROM Common.PrintVendors WHERE VENDORCode='PCI')
UPDATE Common.PrintVendors
SET
SFTChannelId=(SELECT Id FROM Common.SFTChannels WHERE Code='CL_DocSol_PnS_DALLAS_Prod')
WHERE Id=(SELECT Id FROM Common.PrintVendors WHERE VENDORCode='CL_DALLAS')


---- ADDED NEW SYSTEM SETTINGS
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_DirectReportBackChannelCodes', N'String', N'AMC,PrintReady', NULL, 0, N'AMC,PrintReady', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
DELETE FROM [Common].[SystemSettings] WHERE Id IN ('Core_DirectReportBackChannelIds','Core_FedExEndPageTemplatePath','Core_USPSEndPageTemplatePath')

---- DELETE EXTRA ENTRIES FROM AM.DocTypeCoverLetters
DELETE FROM AM.DocTypeCoverLetters WHERE DocTypeId IN (select Id from AM.DocTypes WHERE ShippingServiceType<>'IMB')

---- INSERT END PAGE In AM.DocTypeCoverLetters

-- USAA
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'USAA_FedEx_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT ID FROM COMMON.CLIENTS WHERE AppCode='USAA') AND Id IN (select DocTypeId FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC-FEDEX'))
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'USAA_USPS_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT ID FROM COMMON.CLIENTS WHERE AppCode='USAA') AND Id IN (select DocTypeId
FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC-PRI'))

-- HSBC
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'HSBC_USPS_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT ID FROM COMMON.CLIENTS WHERE AppCode='HSBC') AND Id IN (select DocTypeId
FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='HSBC-GENERIC'))

-- BOTW
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'BOTW_USPS_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT id FROM Common.Clients WHERE AppCode='BOTW') AND Id IN (select DocTypeId
FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='BOTW-GENERIC'))

UPDATE Common.Clients SET IsAMActive=1,IsPnSActive=1 WHERE Id in ((SELECT Id FROM Common.Clients WHERE AppCode='CITIVM'),(SELECT Id FROM Common.Clients WHERE AppCode='RPS'),(SELECT Id FROM Common.Clients WHERE AppCode='BOTW'))

------------------------------------------------------------------------------
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO


IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE 
begin
PRINT 'The database update failed'
end
GO
DROP TABLE #tmpErrors
GO