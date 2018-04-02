/****** Object:  StoredProcedure [dbo].[usp_TodayMailPieceChartData]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_TodayMailPieceChartData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_TodayMailPieceChartData]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMailPieceByBatchIdAndStatus]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMailPieceByBatchIdAndStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMailPieceByBatchIdAndStatus]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMailPieceByBatchId]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMailPieceByBatchId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMailPieceByBatchId]
GO
/****** Object:  View [dbo].[view_SummaryControlReport]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_SummaryControlReport]'))
DROP VIEW [dbo].[view_SummaryControlReport]
GO
/****** Object:  View [dbo].[view_MonthlyReconReport]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_MonthlyReconReport]'))
DROP VIEW [dbo].[view_MonthlyReconReport]
GO
/****** Object:  View [dbo].[view_MailPieceByParam]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_MailPieceByParam]'))
DROP VIEW [dbo].[view_MailPieceByParam]
GO
/****** Object:  View [dbo].[view_ErrorReport]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_ErrorReport]'))
DROP VIEW [dbo].[view_ErrorReport]
GO
/****** Object:  View [dbo].[view_DailyDetailOperationsReport]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_DailyDetailOperationsReport]'))
DROP VIEW [dbo].[view_DailyDetailOperationsReport]
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_GetWeeklyChartData]    Script Date: 3/28/2018 12:15:19 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufn_GetWeeklyChartData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ufn_GetWeeklyChartData]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_SummaryControlReport]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_SummaryControlReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_SummaryControlReport]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_MonthlyReconReport]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_MonthlyReconReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_MonthlyReconReport]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_MailPieceSearch]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_MailPieceSearch]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_MailPieceSearch]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_GetMailPieceByBatchIdAndStatus]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_GetMailPieceByBatchIdAndStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_GetMailPieceByBatchIdAndStatus]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_GetMailPieceByBatchId]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_GetMailPieceByBatchId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_GetMailPieceByBatchId]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_ErrorReportSLA]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_ErrorReportSLA]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_ErrorReportSLA]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_ErrorReport]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_ErrorReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_ErrorReport]
GO
/****** Object:  StoredProcedure [dbo].[uspweb_DailyDetailOperationsReport]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_DailyDetailOperationsReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[uspweb_DailyDetailOperationsReport]
GO
/****** Object:  StoredProcedure [dbo].[usp_RevertPipelineStatus]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RevertPipelineStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_RevertPipelineStatus]
GO
/****** Object:  StoredProcedure [dbo].[usp_MapClientSFTChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_MapClientSFTChannels]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_MapClientSFTChannels]
GO
/****** Object:  StoredProcedure [dbo].[usp_LogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_LogBatchTimeLine]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_LogBatchTimeLine]
GO
/****** Object:  StoredProcedure [dbo].[usp_InsertPullBackRequestsData]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_InsertPullBackRequestsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_InsertPullBackRequestsData]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetSubmittedBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetSubmittedBatches]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetSubmittedBatches]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetNextBatchIdByJobName]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetNextBatchIdByJobName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetNextBatchIdByJobName]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseSchema]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFResponseSchema]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFResponseSchema]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFResponseDataByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFResponseDataByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestSchema]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestSchema]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFRequestSchema]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestDataByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetFinalMRDFResponseDataByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetFinalMRDFResponseDataByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetDocTypeDetailsByAppId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetClientAppDetailsById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetClientAppDetailsById]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummarySchema]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummarySchema]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetBatchSummarySchema]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummaryById]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummaryById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetBatchSummaryById]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetAllClientAppDetail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetAllClientAppDetail]
GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteMRDFRequestByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_DeleteMRDFRequestByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_DeleteMRDFRequestByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_AMLogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_AMLogBatchTimeLine]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_AMLogBatchTimeLine]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_Users_Clients]') AND parent_object_id = OBJECT_ID(N'[Profile].[Users]'))
ALTER TABLE [Profile].[Users] DROP CONSTRAINT [FK_Users_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UsersRoles_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserRoles]'))
ALTER TABLE [Profile].[UserRoles] DROP CONSTRAINT [FK_UsersRoles_Users]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserRoles_Roles]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserRoles]'))
ALTER TABLE [Profile].[UserRoles] DROP CONSTRAINT [FK_UserRoles_Roles]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserLogins_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserLogins]'))
ALTER TABLE [Profile].[UserLogins] DROP CONSTRAINT [FK_UserLogins_Users]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_ReprintMRDFRequest_ReprintBatches]') AND parent_object_id = OBJECT_ID(N'[PnS].[ReprintMRDFRequest]'))
ALTER TABLE [PnS].[ReprintMRDFRequest] DROP CONSTRAINT [FK_ReprintMRDFRequest_ReprintBatches]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_MRDFResponse_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[MRDFResponse]'))
ALTER TABLE [PnS].[MRDFResponse] DROP CONSTRAINT [FK_MRDFResponse_BatchSummary]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_MRDFRequest_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[MRDFRequest]'))
ALTER TABLE [PnS].[MRDFRequest] DROP CONSTRAINT [FK_MRDFRequest_BatchSummary]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary] DROP CONSTRAINT [FK_BatchSummary_DocTypes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_Clients]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary] DROP CONSTRAINT [FK_BatchSummary_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_MailPiecePageInfo_MailPieces]') AND parent_object_id = OBJECT_ID(N'[Common].[MailPiecePageInfo]'))
ALTER TABLE [Common].[MailPiecePageInfo] DROP CONSTRAINT [FK_MailPiecePageInfo_MailPieces]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_EmailChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] DROP CONSTRAINT [FK_Deliverables_EmailChannel]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_Clients]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] DROP CONSTRAINT [FK_Deliverables_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_ClientSFTChannels_SFTChannels]') AND parent_object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]'))
ALTER TABLE [Common].[ClientSFTChannels] DROP CONSTRAINT [FK_ClientSFTChannels_SFTChannels]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_ClientSFTChannels_Clients]') AND parent_object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]'))
ALTER TABLE [Common].[ClientSFTChannels] DROP CONSTRAINT [FK_ClientSFTChannels_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Clients_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[Common].[Clients]'))
ALTER TABLE [Common].[Clients] DROP CONSTRAINT [FK_Clients_BusinessUnit]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PullBackRequests_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] DROP CONSTRAINT [FK_PullBackRequests_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_Users]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] DROP CONSTRAINT [FK_AM.PullBackRequests_Users]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_MailPieces]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] DROP CONSTRAINT [FK_AM.PullBackRequests_MailPieces]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] DROP CONSTRAINT [FK_AM.PullBackRequests_MailBatches]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatches_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatches]'))
ALTER TABLE [AM].[PrintBatches] DROP CONSTRAINT [FK_PrintBatches_MailBatches]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatches_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatches]'))
ALTER TABLE [AM].[PrintBatches] DROP CONSTRAINT [FK_PrintBatches_DocTypes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_Users]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] DROP CONSTRAINT [FK_MailPieces_Users]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_PrintBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] DROP CONSTRAINT [FK_MailPieces_PrintBatches]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] DROP CONSTRAINT [FK_MailPieces_MailBatches]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] DROP CONSTRAINT [FK_MailPieces_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieceCustomInfo_MailPieces]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]'))
ALTER TABLE [AM].[MailPieceCustomInfo] DROP CONSTRAINT [FK_MailPieceCustomInfo_MailPieces]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatches_SFTChannel]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatches]'))
ALTER TABLE [AM].[MailBatches] DROP CONSTRAINT [FK_MailBatches_SFTChannel]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatches_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatches]'))
ALTER TABLE [AM].[MailBatches] DROP CONSTRAINT [FK_MailBatches_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatchCustomInfo_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatchCustomInfo]'))
ALTER TABLE [AM].[MailBatchCustomInfo] DROP CONSTRAINT [FK_MailBatchCustomInfo_MailBatches]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypes_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypes]'))
ALTER TABLE [AM].[DocTypes] DROP CONSTRAINT [FK_DocTypes_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypeCoverLetters_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]'))
ALTER TABLE [AM].[DocTypeCoverLetters] DROP CONSTRAINT [FK_DocTypeCoverLetters_DocTypes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_ClientBusinessIdentifiers_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[ClientBusinessIdentifiers]'))
ALTER TABLE [AM].[ClientBusinessIdentifiers] DROP CONSTRAINT [FK_ClientBusinessIdentifiers_Clients]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Common].[DF_SFTChannels_IsActive]') AND type = 'D')
BEGIN
ALTER TABLE [Common].[SFTChannels] DROP CONSTRAINT [DF_SFTChannels_IsActive]
END

GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AM].[DF_MailPieces_IsQCPerformed]') AND type = 'D')
BEGIN
ALTER TABLE [AM].[MailPieces] DROP CONSTRAINT [DF_MailPieces_IsQCPerformed]
END

GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AM].[DF_MailPieces_IsQCRequested]') AND type = 'D')
BEGIN
ALTER TABLE [AM].[MailPieces] DROP CONSTRAINT [DF_MailPieces_IsQCRequested]
END

GO
/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_MRDFResponseData]'))
DROP VIEW [dbo].[view_MRDFResponseData]
GO
/****** Object:  Table [Profile].[Users]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Users]') AND type in (N'U'))
DROP TABLE [Profile].[Users]
GO
/****** Object:  Table [Profile].[UserRoles]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserRoles]') AND type in (N'U'))
DROP TABLE [Profile].[UserRoles]
GO
/****** Object:  Table [Profile].[UserLogins]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserLogins]') AND type in (N'U'))
DROP TABLE [Profile].[UserLogins]
GO
/****** Object:  Table [Profile].[UserClaims]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserClaims]') AND type in (N'U'))
DROP TABLE [Profile].[UserClaims]
GO
/****** Object:  Table [Profile].[Roles]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Roles]') AND type in (N'U'))
DROP TABLE [Profile].[Roles]
GO
/****** Object:  Table [PnS].[Schema_MRDFResponse]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_MRDFResponse]') AND type in (N'U'))
DROP TABLE [PnS].[Schema_MRDFResponse]
GO
/****** Object:  Table [PnS].[Schema_MRDFRequest]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_MRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[Schema_MRDFRequest]
GO
/****** Object:  Table [PnS].[Schema_BatchSummary]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_BatchSummary]') AND type in (N'U'))
DROP TABLE [PnS].[Schema_BatchSummary]
GO
/****** Object:  Table [PnS].[ReprintMRDFRequest]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[ReprintMRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[ReprintMRDFRequest]
GO
/****** Object:  Table [PnS].[ReprintBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[ReprintBatches]') AND type in (N'U'))
DROP TABLE [PnS].[ReprintBatches]
GO
/****** Object:  Table [PnS].[MRDFResponse]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[MRDFResponse]') AND type in (N'U'))
DROP TABLE [PnS].[MRDFResponse]
GO
/****** Object:  Table [PnS].[MRDFRequest]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[MRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[MRDFRequest]
GO
/****** Object:  Table [PnS].[LogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[LogBatchTimeLine]') AND type in (N'U'))
DROP TABLE [PnS].[LogBatchTimeLine]
GO
/****** Object:  Table [PnS].[BatchSummary]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[BatchSummary]') AND type in (N'U'))
DROP TABLE [PnS].[BatchSummary]
GO
/****** Object:  Table [Common].[SystemSettings]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SystemSettings]') AND type in (N'U'))
DROP TABLE [Common].[SystemSettings]
GO
/****** Object:  Table [Common].[SFTChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SFTChannels]') AND type in (N'U'))
DROP TABLE [Common].[SFTChannels]
GO
/****** Object:  Table [Common].[MailPiecePageInfo]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[MailPiecePageInfo]') AND type in (N'U'))
DROP TABLE [Common].[MailPiecePageInfo]
GO
/****** Object:  Table [Common].[EmailChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[EmailChannels]') AND type in (N'U'))
DROP TABLE [Common].[EmailChannels]
GO
/****** Object:  Table [Common].[Deliverables]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Deliverables]') AND type in (N'U'))
DROP TABLE [Common].[Deliverables]
GO
/****** Object:  Table [Common].[ClientSFTChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]') AND type in (N'U'))
DROP TABLE [Common].[ClientSFTChannels]
GO
/****** Object:  Table [Common].[Clients]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Clients]') AND type in (N'U'))
DROP TABLE [Common].[Clients]
GO
/****** Object:  Table [Common].[BusinessUnit]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[BusinessUnit]') AND type in (N'U'))
DROP TABLE [Common].[BusinessUnit]
GO
/****** Object:  Table [AM].[ShippingAccounts]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[ShippingAccounts]') AND type in (N'U'))
DROP TABLE [AM].[ShippingAccounts]
GO
/****** Object:  Table [AM].[SharedStrings]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[SharedStrings]') AND type in (N'U'))
DROP TABLE [AM].[SharedStrings]
GO
/****** Object:  Table [AM].[PullBackRequests]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PullBackRequests]') AND type in (N'U'))
DROP TABLE [AM].[PullBackRequests]
GO
/****** Object:  Table [AM].[PrintBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PrintBatches]') AND type in (N'U'))
DROP TABLE [AM].[PrintBatches]
GO
/****** Object:  Table [AM].[MailPieces]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPieces]') AND type in (N'U'))
DROP TABLE [AM].[MailPieces]
GO
/****** Object:  Table [AM].[MailPieceCustomInfo]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]') AND type in (N'U'))
DROP TABLE [AM].[MailPieceCustomInfo]
GO
/****** Object:  Table [AM].[MailBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailBatches]') AND type in (N'U'))
DROP TABLE [AM].[MailBatches]
GO
/****** Object:  Table [AM].[MailBatchCustomInfo]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailBatchCustomInfo]') AND type in (N'U'))
DROP TABLE [AM].[MailBatchCustomInfo]
GO
/****** Object:  Table [AM].[LogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[LogBatchTimeLine]') AND type in (N'U'))
DROP TABLE [AM].[LogBatchTimeLine]
GO
/****** Object:  Table [AM].[DocTypes]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[DocTypes]') AND type in (N'U'))
DROP TABLE [AM].[DocTypes]
GO
/****** Object:  Table [AM].[DocTypeCoverLetters]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]') AND type in (N'U'))
DROP TABLE [AM].[DocTypeCoverLetters]
GO
/****** Object:  Table [AM].[ClientBusinessIdentifiers]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[ClientBusinessIdentifiers]') AND type in (N'U'))
DROP TABLE [AM].[ClientBusinessIdentifiers]
GO
/****** Object:  Schema [Profile]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'Profile')
DROP SCHEMA [Profile]
GO
/****** Object:  Schema [PnS]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'PnS')
DROP SCHEMA [PnS]
GO
/****** Object:  Schema [Common]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'Common')
DROP SCHEMA [Common]
GO
/****** Object:  Schema [AM]    Script Date: 3/28/2018 3:47:23 PM ******/
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'AM')
DROP SCHEMA [AM]
GO
/****** Object:  Schema [AM]    Script Date: 3/28/2018 3:47:23 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'AM')
EXEC sys.sp_executesql N'CREATE SCHEMA [AM]'

GO
/****** Object:  Schema [Common]    Script Date: 3/28/2018 3:47:23 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Common')
EXEC sys.sp_executesql N'CREATE SCHEMA [Common]'

GO
/****** Object:  Schema [PnS]    Script Date: 3/28/2018 3:47:23 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'PnS')
EXEC sys.sp_executesql N'CREATE SCHEMA [PnS]'

GO
/****** Object:  Schema [Profile]    Script Date: 3/28/2018 3:47:23 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Profile')
EXEC sys.sp_executesql N'CREATE SCHEMA [Profile]'

GO
/****** Object:  Table [AM].[ClientBusinessIdentifiers]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[ClientBusinessIdentifiers]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[ClientBusinessIdentifiers](
	[ClientId] [int] NOT NULL,
	[BusinessIdName] [varchar](255) NOT NULL,
 CONSTRAINT [PK_ClientBusinessIdentifiers] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC,
	[BusinessIdName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[DocTypeCoverLetters]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[DocTypeCoverLetters](
	[DocTypeId] [int] NOT NULL,
	[SeqId] [int] NOT NULL,
	[CoverLetterTemplatePath] [varchar](1024) NULL,
 CONSTRAINT [PK_DocTypeCoverLetters] PRIMARY KEY CLUSTERED 
(
	[DocTypeId] ASC,
	[SeqId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[DocTypes]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[DocTypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[DocTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DocTypeCode] [varchar](255) NOT NULL,
	[Description] [varchar](1024) NULL,
	[ClientId] [int] NOT NULL,
	[PrintSize] [varchar](255) NOT NULL,
	[ContentScalePercent] [float] NOT NULL,
	[PrintFormat] [varchar](255) NULL,
	[QCPercent] [float] NULL,
	[SLADays] [int] NULL,
	[SLACutOffTime] [time](7) NULL,
	[ShippingCarrier] [varchar](255) NULL,
	[ShippingServiceType] [varchar](255) NULL,
	[ShippingAccountCode] [varchar](255) NULL,
	[IsReturnInfoSpecified] [bit] NULL,
	[RetRcptPrefix] [varchar](255) NULL,
	[RetRcptFirstName] [varchar](255) NULL,
	[RetRcptMiddleName] [varchar](255) NULL,
	[RetRcptLastName] [varchar](255) NULL,
	[RetRcptSuffix] [varchar](255) NULL,
	[RetRcptPhone] [varchar](255) NULL,
	[RetRcptOrganization] [varchar](255) NULL,
	[RetRcptAddrOptionalLine1AboveStreet] [varchar](255) NULL,
	[RetRcptAddrOptionalLine2AboveStreet] [varchar](255) NULL,
	[RetRcptAddressStreet] [varchar](255) NULL,
	[RetRcptAddressAptOrSuiteOrOther] [varchar](255) NULL,
	[RetRcptAddrOptionalLine1BelowStreet] [varchar](255) NULL,
	[RetRcptAddrOptionalLine2BelowStreet] [varchar](255) NULL,
	[RetRcptAddressCity] [varchar](255) NULL,
	[RetRcptAddressState] [varchar](255) NULL,
	[RetRcptAddressZIP] [varchar](255) NULL,
	[ClientEmails] [varchar](1024) NULL,
	[ClientContactNo] [varchar](255) NULL,
	[SupportEmails] [varchar](1024) NULL,
	[SupportContactNo] [varchar](255) NULL,
	[IsActive] [bit] NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[CreatedOn] [datetime] NULL,
	[ModifiedBy] [varchar](255) NULL,
	[CreatedBy] [varchar](255) NULL,
 CONSTRAINT [PK_DocTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[LogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[LogBatchTimeLine]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[LogBatchTimeLine](
	[MailBatchId] [int] NOT NULL,
	[DownloadStart] [datetime] NULL,
	[DownloadEnd] [datetime] NULL,
	[ValidationStart] [datetime] NULL,
	[ValidationEnd] [datetime] NULL,
	[AcknowledgeStart] [datetime] NULL,
	[AcknowledgeEnd] [datetime] NULL,
	[PreppedStart] [datetime] NULL,
	[PreppedEnd] [datetime] NULL,
	[SubmittedToShippingStart] [datetime] NULL,
	[SubmittedToShippingEnd] [datetime] NULL,
	[ShippingResponseReceivedStart] [datetime] NULL,
	[ShippingResponseReceivedEnd] [datetime] NULL,
	[ReportPrepStart] [datetime] NULL,
	[ReportPrepEnd] [datetime] NULL,
	[ReportBackStart] [datetime] NULL,
	[ReportBackEnd] [datetime] NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [AM].[MailBatchCustomInfo]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailBatchCustomInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[MailBatchCustomInfo](
	[MailBatchId] [int] NOT NULL,
	[Key] [varchar](128) NOT NULL,
	[DataType] [varchar](128) NULL,
	[Value] [varchar](2048) NULL,
 CONSTRAINT [PK_MailBatchCustomInfo] PRIMARY KEY CLUSTERED 
(
	[MailBatchId] ASC,
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[MailBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailBatches]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[MailBatches](
	[Id] [int] IDENTITY(1000,1) NOT NULL,
	[UniqueId] [uniqueidentifier] NOT NULL,
	[ClientId] [int] NULL,
	[SFTChannelId] [int] NULL,
	[ClientBatchId] [varchar](255) NULL,
	[BatchName] [varchar](255) NULL,
	[DocumentOrPackageType] [varchar](255) NULL,
	[IsReceived]  AS (case when [ReceivedOn] IS NULL then (0) else (1) end),
	[ReceivedOn] [datetime] NULL,
	[IsDownloaded]  AS (case when [DownloadedOn] IS NULL then (0) else (1) end),
	[DownloadedOn] [datetime] NULL,
	[IsValidated]  AS (case when [ValidatedOn] IS NULL then (0) else (1) end),
	[ValidatedOn] [datetime] NULL,
	[IsAcknowledged]  AS (case when [AcknowledgedOn] IS NULL then (0) else (1) end),
	[AcknowledgedOn] [datetime] NULL,
	[IsPrepped]  AS (case when [PreppedOn] IS NULL then (0) else (1) end),
	[PreppedOn] [datetime] NULL,
	[IsSubmittedToShipping]  AS (case when [SubmittedToShippingOn] IS NULL then (0) else (1) end),
	[SubmittedToShippingOn] [datetime] NULL,
	[IsShippingResponseReceived]  AS (case when [ShippingResponseReceivedOn] IS NULL then (0) else (1) end),
	[ShippingResponseReceivedOn] [datetime] NULL,
	[IsReportPrepped]  AS (case when [ReportPreppedOn] IS NULL then (0) else (1) end),
	[ReportPreppedOn] [datetime] NULL,
	[IsReportedBack]  AS (case when [ReportedBackOn] IS NULL then (0) else (1) end),
	[ReportedBackOn] [datetime] NULL,
	[Priority] [varchar](255) NULL,
	[BatchStatus] [varchar](255) NULL,
	[ValidationStatus] [varchar](255) NULL,
	[ValidationErrors] [varchar](max) NULL,
	[PipelineStatus] [varchar](255) NULL,
	[IsReturnInfoSpecified] [bit] NULL,
	[RetRcptPrefix] [varchar](255) NULL,
	[RetRcptFirstName] [varchar](255) NULL,
	[RetRcptMiddleName] [varchar](255) NULL,
	[RetRcptLastName] [varchar](255) NULL,
	[RetRcptSuffix] [varchar](255) NULL,
	[RetRcptPhone] [varchar](255) NULL,
	[RetRcptOrganization] [varchar](255) NULL,
	[RetRcptAddrOptionalLine1AboveStreet] [varchar](255) NULL,
	[RetRcptAddrOptionalLine2AboveStreet] [varchar](255) NULL,
	[RetRcptAddressStreet] [varchar](255) NULL,
	[RetRcptAddressAptOrSuiteOrOther] [varchar](255) NULL,
	[RetRcptAddrOptionalLine1BelowStreet] [varchar](255) NULL,
	[RetRcptAddrOptionalLine2BelowStreet] [varchar](255) NULL,
	[RetRcptAddressCity] [varchar](255) NULL,
	[RetRcptAddressState] [varchar](255) NULL,
	[RetRcptAddressZIP] [varchar](255) NULL,
	[PrintLocation] [varchar](255) NULL,
 CONSTRAINT [PK_Batch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[MailPieceCustomInfo]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[MailPieceCustomInfo](
	[MailPieceId] [int] NOT NULL,
	[Key] [varchar](128) NOT NULL,
	[DataType] [varchar](128) NULL,
	[Value] [varchar](2048) NULL,
 CONSTRAINT [PK_MailPieceCustomInfo] PRIMARY KEY CLUSTERED 
(
	[MailPieceId] ASC,
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[MailPieces]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPieces]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[MailPieces](
	[Id] [int] IDENTITY(1000,1) NOT NULL,
	[UniqueId] [uniqueidentifier] NULL,
	[ClientId] [int] NULL,
	[MailBatchId] [int] NOT NULL,
	[PrintBatchId] [int] NULL,
	[BusinessFriendlyIdName] [varchar](255) NULL,
	[BusinessFriendlyIdValue] [varchar](255) NULL,
	[FileName] [varchar](255) NULL,
	[FilePattern] [varchar](255) NULL,
	[DocumentOrPackageType] [varchar](255) NULL,
	[SLADays]  AS (datediff(second,isnull([ShippedOn],getutcdate()),[DueDate])),
	[DueDate] [datetime] NULL,
	[ShippingStatus] [varchar](255) NULL,
	[IsRcptInfoSpecified] [bit] NULL,
	[RcptPrefix] [varchar](255) NULL,
	[RcptFirstName] [varchar](255) NULL,
	[RcptMiddleName] [varchar](255) NULL,
	[RcptLastName] [varchar](255) NULL,
	[RcptSuffix] [varchar](255) NULL,
	[RcptPhone] [varchar](255) NULL,
	[RcptOrganization] [varchar](255) NULL,
	[CoRcptPrefix] [varchar](255) NULL,
	[CoRcptFirstName] [varchar](255) NULL,
	[CoRcptMiddleName] [varchar](255) NULL,
	[CoRcptLastName] [varchar](255) NULL,
	[CoRcptSuffix] [varchar](255) NULL,
	[CoRcptPhone] [varchar](255) NULL,
	[CoRcptOrganization] [varchar](255) NULL,
	[RcptAddrOptionalLine1AboveStreet] [varchar](255) NULL,
	[RcptAddrOptionalLine2AboveStreet] [varchar](255) NULL,
	[RcptAddressStreet] [varchar](255) NULL,
	[RcptAddressAptOrSuiteOrOther] [varchar](255) NULL,
	[RcptAddrOptionalLine1BelowStreet] [varchar](255) NULL,
	[RcptAddrOptionalLine2BelowStreet] [varchar](255) NULL,
	[RcptAddressCity] [varchar](255) NULL,
	[RcptAddressState] [varchar](255) NULL,
	[RcptAddressZIP] [varchar](255) NULL,
	[IsReturnInfoSpecified] [bit] NULL,
	[RetRcptPrefix] [varchar](255) NULL,
	[RetRcptFirstName] [varchar](255) NULL,
	[RetRcptMiddleName] [varchar](255) NULL,
	[RetRcptLastName] [varchar](255) NULL,
	[RetRcptSuffix] [varchar](255) NULL,
	[RetRcptPhone] [varchar](255) NULL,
	[RetRcptOrganization] [varchar](255) NULL,
	[RetRcptAddrOptionalLine1AboveStreet] [varchar](255) NULL,
	[RetRcptAddrOptionalLine2AboveStreet] [varchar](255) NULL,
	[RetRcptAddressStreet] [varchar](255) NULL,
	[RetRcptAddressAptOrSuiteOrOther] [varchar](255) NULL,
	[RetRcptAddrOptionalLine1BelowStreet] [varchar](255) NULL,
	[RetRcptAddrOptionalLine2BelowStreet] [varchar](255) NULL,
	[RetRcptAddressCity] [varchar](255) NULL,
	[RetRcptAddressState] [varchar](255) NULL,
	[RetRcptAddressZIP] [varchar](255) NULL,
	[TotalPages] [int] NOT NULL,
	[TotalSheets] [int] NOT NULL,
	[ErrorCode] [varchar](255) NULL,
	[ErrorDesc] [varchar](max) NULL,
	[TrackingNo] [varchar](255) NULL,
	[IsPullBackRequested]  AS (case when [PullBackRequestedOn] IS NULL then (0) else (1) end),
	[PullBackRequestedOn] [datetime] NULL,
	[IsPulledBack]  AS (case when [PulledBackOn] IS NULL then (0) else (1) end),
	[PulledBackOn] [datetime] NULL,
	[PulledBackRequestedBy] [nvarchar](128) NULL,
	[IsPrinted]  AS (case when [PrintedOn] IS NULL then (0) else (1) end),
	[PrintedOn] [datetime] NULL,
	[IsShipped]  AS (case when [ShippedOn] IS NULL then (0) else (1) end),
	[ShippedOn] [datetime] NULL,
	[ShippingCost] [decimal](6, 3) NULL,
	[Weight] [decimal](6, 3) NULL,
	[IsQCRequested] [bit] NULL,
	[IsQCPerformed] [bit] NULL,
	[DocTypeId] [int] NULL,
 CONSTRAINT [PK_MailPieces] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[PrintBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PrintBatches]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[PrintBatches](
	[Id] [int] IDENTITY(1000,1) NOT NULL,
	[UniqueId] [uniqueidentifier] NOT NULL,
	[MailBatchId] [int] NOT NULL,
	[AppCode] [varchar](255) NULL,
	[DocTypeId] [int] NULL,
	[BatchStatus] [varchar](255) NULL,
	[StatusDescription] [varchar](1024) NULL,
	[IsSubmittedToShipping]  AS (case when [SubmittedToShippingOn] IS NULL then (0) else (1) end),
	[SubmittedToShippingOn] [datetime] NULL,
	[IsShippingResponseReceived]  AS (case when [ShippingResponseReceivedOn] IS NULL then (0) else (1) end),
	[ShippingResponseReceivedOn] [datetime] NULL,
 CONSTRAINT [PK_PrintBatches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[PullBackRequests]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PullBackRequests]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[PullBackRequests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [int] NULL,
	[MailBatchId] [int] NOT NULL,
	[MailPieceId] [int] NOT NULL,
	[MailPieceUniqueId] [uniqueidentifier] NOT NULL,
	[Status] [varchar](255) NOT NULL,
	[RequestedOn] [datetime] NOT NULL,
	[RequestedBy] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_AM.PullBackRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[SharedStrings]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[SharedStrings]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[SharedStrings](
	[Key] [varchar](128) NOT NULL,
	[Value] [varchar](4096) NULL,
 CONSTRAINT [PK_SharedStrings] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[ShippingAccounts]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[ShippingAccounts]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[ShippingAccounts](
	[AccountCode] [varchar](128) NOT NULL,
	[WebAuthenticationKey] [varchar](255) NULL,
	[WebAuthenticationPassword] [varchar](255) NULL,
	[AccountNumber] [varchar](255) NULL,
	[MeterNumber] [varchar](255) NULL,
 CONSTRAINT [PK_ShippingAccounts] PRIMARY KEY CLUSTERED 
(
	[AccountCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[BusinessUnit]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[BusinessUnit]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[BusinessUnit](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BusinessUnit] [varchar](10) NOT NULL,
	[CustCode] [varchar](20) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_BusinessUnit] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[BusinessUnit] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[Clients]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Clients]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[Clients](
	[Id] [int] IDENTITY(100,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[AppCode] [varchar](128) NULL,
	[BusinessUnit] [int] NULL,
	[IsAMActive] [bit] NULL,
	[IsPnSActive] [bit] NULL,
	[TimeZone] [varchar](255) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](255) NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](255) NULL,
 CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_AppCode] UNIQUE NONCLUSTERED 
(
	[AppCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[ClientSFTChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[ClientSFTChannels](
	[ClientId] [int] NOT NULL,
	[SFTChannelId] [int] NOT NULL,
 CONSTRAINT [PK_ClientSFTChannels] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC,
	[SFTChannelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Common].[Deliverables]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Deliverables]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[Deliverables](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [int] NULL,
	[ReportName] [varchar](255) NULL,
	[ReportType] [varchar](255) NULL,
	[DeliveryType] [varchar](255) NULL,
	[ReportQuery] [varchar](max) NULL,
	[Delimeter] [varchar](255) NULL,
	[FileNameExpression] [varchar](255) NULL,
	[IsSFTUploadRequired] [bit] NULL,
	[UploadPath] [varchar](1024) NULL,
	[EmailChannelId] [int] NULL,
	[ScheduleId] [int] NULL,
 CONSTRAINT [PK_Deliverables] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[EmailChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[EmailChannels]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[EmailChannels](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](255) NULL,
	[To] [varchar](1024) NULL,
	[Cc] [varchar](1024) NULL,
	[Bcc] [varchar](1024) NULL,
	[Subject] [varchar](1024) NULL,
	[BodyTemplate] [varchar](max) NULL,
	[IsBodyHTML] [bit] NULL,
 CONSTRAINT [PK_EmailChannel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[MailPiecePageInfo]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[MailPiecePageInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[MailPiecePageInfo](
	[MailPieceId] [int] NOT NULL,
	[PageSize] [varchar](255) NOT NULL,
	[PageCount] [int] NULL,
 CONSTRAINT [PK_MailPiecePageInfo] PRIMARY KEY CLUSTERED 
(
	[MailPieceId] ASC,
	[PageSize] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[SFTChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SFTChannels]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[SFTChannels](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](128) NULL,
	[Description] [varchar](512) NULL,
	[Title] [varchar](255) NULL,
	[Protocol] [varchar](255) NULL,
	[Host] [varchar](255) NULL,
	[UserName] [varchar](255) NULL,
	[Password] [varchar](255) NULL,
	[ToADEDir] [varchar](1024) NULL,
	[FromADEDir] [varchar](1024) NULL,
	[ArchiveDir] [varchar](1024) NULL,
	[PostDownloadAction] [varchar](255) NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_SFTChannel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_SFTChannel] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[SystemSettings]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SystemSettings]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[SystemSettings](
	[Key] [varchar](128) NOT NULL,
	[DataType] [varchar](128) NULL,
	[Value] [varchar](1024) NULL,
	[Description] [varchar](2028) NULL,
	[IsSystem] [bit] NULL,
	[DefaultValue] [varchar](1024) NULL,
 CONSTRAINT [PK_SystemSettings] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[BatchSummary]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[BatchSummary]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[BatchSummary](
	[PrintBatch] [int] IDENTITY(1000,1) NOT NULL,
	[BatchIdentifier] [uniqueidentifier] NULL,
	[ClientId] [int] NULL,
	[JobName] [varchar](512) NULL,
	[PrintFileName] [varchar](255) NULL,
	[DueDate] [date] NULL,
	[SLADays] [int] NULL,
	[DocTypeId] [int] NULL,
	[PipelineStatus] [varchar](512) NULL,
	[ValidationStatus] [varchar](512) NULL,
	[OnValidationErrors] [varchar](512) NULL,
	[ValidationErrors] [varchar](max) NULL,
	[BatchStatus] [varchar](512) NULL,
	[IsReceived]  AS (case when [ReceivedOn] IS NULL then (0) else (1) end),
	[ReceivedOn] [datetime] NULL,
	[IsDownloaded]  AS (case when [DownloadedOn] IS NULL then (0) else (1) end),
	[DownloadedOn] [datetime] NULL,
	[IsPrepped]  AS (case when [PreppedOn] IS NULL then (0) else (1) end),
	[PreppedOn] [datetime] NULL,
	[IsSubmitted]  AS (case when [SubmittedOn] IS NULL then (0) else (1) end),
	[SubmittedOn] [datetime] NULL,
	[IsMRDFReceived]  AS (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
	[MRDFReceivedOn] [datetime] NULL,
	[IsReportPrepped]  AS (case when [ReportPreppedOn] IS NULL then (0) else (1) end),
	[ReportPreppedOn] [datetime] NULL,
	[IsReportedBack]  AS (case when [ReportedBackOn] IS NULL then (0) else (1) end),
	[ReportedBackOn] [datetime] NULL,
	[IsSuspended]  AS (case when [SuspendedOn] IS NULL then (0) else (1) end),
	[SuspendedOn] [datetime] NULL,
	[IsCancelled]  AS (case when [CancelledOn] IS NULL then (0) else (1) end),
	[CancelledOn] [datetime] NULL,
	[JulianDate] [int] NULL,
	[Images] [int] NULL,
 CONSTRAINT [PK_BatchSummary] PRIMARY KEY CLUSTERED 
(
	[PrintBatch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[LogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[LogBatchTimeLine]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[LogBatchTimeLine](
	[BatchId] [int] NOT NULL,
	[DownloadStart] [datetime] NULL,
	[DownloadEnd] [datetime] NULL,
	[PCIBatchPrepStart] [datetime] NULL,
	[PCIBatchPrepEnd] [datetime] NULL,
	[IEReportPrepStart] [datetime] NULL,
	[IEReportPrepEnd] [datetime] NULL,
	[SubmitToPCIStart] [datetime] NULL,
	[SubmitToPCIEnd] [datetime] NULL,
	[MRDFDownloadStart] [datetime] NULL,
	[MRDFDownloadEnd] [datetime] NULL,
	[ReportPrepStart] [datetime] NULL,
	[ReportPrepEnd] [datetime] NULL,
	[ReportBackStart] [datetime] NULL,
	[ReportBackEnd] [datetime] NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [PnS].[MRDFRequest]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[MRDFRequest]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[MRDFRequest](
	[JobId] [int] NOT NULL,
	[PieceId] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[TotalSheets] [int] NULL,
	[InputWeight] [varchar](255) NULL,
	[Feeder3_Sel] [varchar](255) NULL,
	[Feeder4_Sel] [varchar](255) NULL,
	[Feeder5_Sel] [varchar](255) NULL,
	[LowCap_Sel] [varchar](255) NULL,
	[NoSeal_Sel] [varchar](255) NULL,
	[Feeder2_Sel] [varchar](255) NULL,
	[ZipMark_Sel] [varchar](255) NULL,
	[Address1] [varchar](255) NULL,
	[Address2] [varchar](255) NULL,
	[Zip5] [varchar](255) NULL,
	[Zip4] [varchar](255) NULL,
	[Zip2] [varchar](255) NULL,
	[DivertVS2] [varchar](255) NULL,
	[DivertVS3] [varchar](255) NULL,
	[DivertVS4] [varchar](255) NULL,
	[ReturnLogoBitMap] [varchar](255) NULL,
	[ReturnLine1] [varchar](255) NULL,
	[ReturnLine2] [varchar](255) NULL,
	[ReturnLine3] [varchar](255) NULL,
	[ReturnLine4] [varchar](255) NULL,
	[ReturnLine5] [varchar](255) NULL,
	[ReturnLine6] [varchar](255) NULL,
	[GroupNumber] [varchar](255) NULL,
	[SocialSecNumber] [varchar](255) NULL,
	[CheckAmount] [varchar](255) NULL,
	[PassThru] [varchar](255) NULL,
	[Expansion] [varchar](255) NULL,
	[EndofRecord] [varchar](255) NULL,
	[UniqueId] [uniqueidentifier] NULL,
	[PDFFileName] [varchar](255) NULL,
	[ErrId] [int] NULL,
	[ErrDesc] [varchar](max) NULL,
	[MailPieceStatus] [varchar](255) NULL,
 CONSTRAINT [PK_MRDFRequest] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[MRDFResponse]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[MRDFResponse]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[MRDFResponse](
	[JobId] [int] NOT NULL,
	[PieceId] [int] NOT NULL,
	[Date] [date] NULL,
	[Feeder2_Fed] [int] NULL,
	[OperatorId] [varchar](50) NULL,
	[MachineId] [varchar](50) NULL,
	[ProcessingTimeStamp] [datetime] NULL,
	[DetailedErrorId] [varchar](50) NULL,
	[Disposition] [varchar](50) NULL,
	[Input_Fed] [varchar](50) NULL,
	[Feeder3_Fed] [varchar](50) NULL,
	[Feeder4_Fed] [varchar](50) NULL,
	[Feeder5_Fed] [varchar](50) NULL,
	[ComputedWeight] [decimal](4, 3) NULL,
	[MeterDate] [varchar](50) NULL,
	[StatusSource] [varchar](50) NULL,
	[PostageMetered] [decimal](4, 3) NULL,
	[WeightMeasured] [decimal](4, 3) NULL,
	[ExitLocation] [varchar](50) NULL,
	[TotalSheets] [int] NULL,
	[PDFFileName] [varchar](255) NULL,
 CONSTRAINT [PK_MRDFResponse] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[ReprintBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[ReprintBatches]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[ReprintBatches](
	[PrintBatch] [varchar](10) NOT NULL,
	[ParentPrintBatch] [int] NULL,
	[AppId] [int] NULL,
	[DocTypeCode] [varchar](29) NULL,
	[PipelineStatus] [varchar](512) NULL,
	[PrintFileName] [varchar](512) NULL,
	[IsSubmitted]  AS (case when [SubmittedOn] IS NULL then (0) else (1) end),
	[SubmittedOn] [datetime] NULL,
	[IsMRDFReceived]  AS (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
	[MRDFReceivedOn] [datetime] NULL,
 CONSTRAINT [PK_ReprintBatches] PRIMARY KEY CLUSTERED 
(
	[PrintBatch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[ReprintMRDFRequest]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[ReprintMRDFRequest]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[ReprintMRDFRequest](
	[JobId] [varchar](10) NOT NULL,
	[PieceId] [int] NOT NULL,
	[IsResponseReceived] [bit] NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[Schema_BatchSummary]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_BatchSummary]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[Schema_BatchSummary](
	[Id] [int] NOT NULL,
	[FieldName] [nvarchar](255) NULL,
	[StartPosition] [int] NOT NULL,
	[EndPosition] [int] NOT NULL,
	[Length] [int] NOT NULL,
	[Type] [nvarchar](255) NULL,
	[IsActive] [bit] NOT NULL,
	[Comments] [nvarchar](2000) NULL,
	[SampleValue] [nvarchar](255) NULL,
	[DataType] [nvarchar](255) NULL,
	[DataFormatTemplate] [nvarchar](255) NULL,
	[DefaultValue] [nvarchar](255) NULL,
 CONSTRAINT [PK_SchemaBatchSummary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [PnS].[Schema_MRDFRequest]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_MRDFRequest]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[Schema_MRDFRequest](
	[Id] [int] NOT NULL,
	[FieldName] [nvarchar](255) NULL,
	[Type] [nvarchar](255) NULL,
	[StartPosition] [int] NOT NULL,
	[Length] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[Comments] [nvarchar](2000) NULL,
	[SampleValue] [nvarchar](255) NULL,
	[DataType] [nvarchar](255) NULL,
	[DataFormatTemplate] [nvarchar](255) NULL,
	[DefaultValue] [nvarchar](255) NULL,
 CONSTRAINT [PK_SchemaMRDFRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [PnS].[Schema_MRDFResponse]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_MRDFResponse]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[Schema_MRDFResponse](
	[Id] [int] NOT NULL,
	[FieldName] [nvarchar](255) NULL,
	[Type] [nvarchar](255) NULL,
	[StartPosition] [int] NOT NULL,
	[Length] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[SampleValue] [nvarchar](255) NULL,
	[DataType] [nvarchar](255) NULL,
	[DataFormatTemplate] [nvarchar](255) NULL,
 CONSTRAINT [PK_SchemaMRDFResponse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[Roles]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Roles]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[Roles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK__Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[UserClaims]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserClaims]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[UserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK__UserClaim] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[UserLogins]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserLogins]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[UserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK__UserLogin] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[UserRoles]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[UserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK__UserRole] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[Users]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[Users](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[ClientId] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_MRDFResponseData]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[view_MRDFResponseData] AS
SELECT REQ.JobId
,REQ.PieceId
,ISNULL(REQ.ErrId,0) AS ErrId 
,REQ.ErrDesc
,RES.ProcessingTimeStamp AS ShippedOn 
,REQ.UniqueId
,REQ.PDFFileName
,REQ.MailPieceStatus
,ISNULL(RES.TotalSheets,0) AS TotalSheets
,REQ.DivertVS3
,RES.Disposition 
,ISNULL(RES.WeightMeasured,0.00) AS WeightMeasured 
,ISNULL(RES.PostageMetered,0.00) AS PostageCost 
FROM [PnS].MRDFRequest REQ
LEFT JOIN [PnS].MRDFResponse RES
ON  REQ.JobId =RES.JobId AND REQ.PieceId =RES.PieceId' 
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AM].[DF_MailPieces_IsQCRequested]') AND type = 'D')
BEGIN
ALTER TABLE [AM].[MailPieces] ADD  CONSTRAINT [DF_MailPieces_IsQCRequested]  DEFAULT ((0)) FOR [IsQCRequested]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AM].[DF_MailPieces_IsQCPerformed]') AND type = 'D')
BEGIN
ALTER TABLE [AM].[MailPieces] ADD  CONSTRAINT [DF_MailPieces_IsQCPerformed]  DEFAULT ((0)) FOR [IsQCPerformed]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Common].[DF_SFTChannels_IsActive]') AND type = 'D')
BEGIN
ALTER TABLE [Common].[SFTChannels] ADD  CONSTRAINT [DF_SFTChannels_IsActive]  DEFAULT ((0)) FOR [IsActive]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_ClientBusinessIdentifiers_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[ClientBusinessIdentifiers]'))
ALTER TABLE [AM].[ClientBusinessIdentifiers]  WITH CHECK ADD  CONSTRAINT [FK_ClientBusinessIdentifiers_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_ClientBusinessIdentifiers_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[ClientBusinessIdentifiers]'))
ALTER TABLE [AM].[ClientBusinessIdentifiers] CHECK CONSTRAINT [FK_ClientBusinessIdentifiers_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypeCoverLetters_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]'))
ALTER TABLE [AM].[DocTypeCoverLetters]  WITH CHECK ADD  CONSTRAINT [FK_DocTypeCoverLetters_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypeCoverLetters_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]'))
ALTER TABLE [AM].[DocTypeCoverLetters] CHECK CONSTRAINT [FK_DocTypeCoverLetters_DocTypes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypes_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypes]'))
ALTER TABLE [AM].[DocTypes]  WITH CHECK ADD  CONSTRAINT [FK_DocTypes_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypes_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypes]'))
ALTER TABLE [AM].[DocTypes] CHECK CONSTRAINT [FK_DocTypes_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatchCustomInfo_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatchCustomInfo]'))
ALTER TABLE [AM].[MailBatchCustomInfo]  WITH CHECK ADD  CONSTRAINT [FK_MailBatchCustomInfo_MailBatches] FOREIGN KEY([MailBatchId])
REFERENCES [AM].[MailBatches] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatchCustomInfo_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatchCustomInfo]'))
ALTER TABLE [AM].[MailBatchCustomInfo] CHECK CONSTRAINT [FK_MailBatchCustomInfo_MailBatches]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatches_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatches]'))
ALTER TABLE [AM].[MailBatches]  WITH CHECK ADD  CONSTRAINT [FK_MailBatches_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatches_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatches]'))
ALTER TABLE [AM].[MailBatches] CHECK CONSTRAINT [FK_MailBatches_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatches_SFTChannel]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatches]'))
ALTER TABLE [AM].[MailBatches]  WITH CHECK ADD  CONSTRAINT [FK_MailBatches_SFTChannel] FOREIGN KEY([SFTChannelId])
REFERENCES [Common].[SFTChannels] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailBatches_SFTChannel]') AND parent_object_id = OBJECT_ID(N'[AM].[MailBatches]'))
ALTER TABLE [AM].[MailBatches] CHECK CONSTRAINT [FK_MailBatches_SFTChannel]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieceCustomInfo_MailPieces]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]'))
ALTER TABLE [AM].[MailPieceCustomInfo]  WITH CHECK ADD  CONSTRAINT [FK_MailPieceCustomInfo_MailPieces] FOREIGN KEY([MailPieceId])
REFERENCES [AM].[MailPieces] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieceCustomInfo_MailPieces]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]'))
ALTER TABLE [AM].[MailPieceCustomInfo] CHECK CONSTRAINT [FK_MailPieceCustomInfo_MailPieces]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] CHECK CONSTRAINT [FK_MailPieces_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_MailBatches] FOREIGN KEY([MailBatchId])
REFERENCES [AM].[MailBatches] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] CHECK CONSTRAINT [FK_MailPieces_MailBatches]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_PrintBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_PrintBatches] FOREIGN KEY([PrintBatchId])
REFERENCES [AM].[PrintBatches] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_PrintBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] CHECK CONSTRAINT [FK_MailPieces_PrintBatches]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_Users]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_Users] FOREIGN KEY([PulledBackRequestedBy])
REFERENCES [Profile].[Users] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieces_Users]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieces]'))
ALTER TABLE [AM].[MailPieces] CHECK CONSTRAINT [FK_MailPieces_Users]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatches_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatches]'))
ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatches_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatches]'))
ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_DocTypes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatches_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatches]'))
ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_MailBatches] FOREIGN KEY([MailBatchId])
REFERENCES [AM].[MailBatches] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatches_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatches]'))
ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_MailBatches]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests]  WITH CHECK ADD  CONSTRAINT [FK_AM.PullBackRequests_MailBatches] FOREIGN KEY([MailBatchId])
REFERENCES [AM].[MailBatches] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_MailBatches]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] CHECK CONSTRAINT [FK_AM.PullBackRequests_MailBatches]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_MailPieces]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests]  WITH CHECK ADD  CONSTRAINT [FK_AM.PullBackRequests_MailPieces] FOREIGN KEY([MailPieceId])
REFERENCES [AM].[MailPieces] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_MailPieces]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] CHECK CONSTRAINT [FK_AM.PullBackRequests_MailPieces]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_Users]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests]  WITH CHECK ADD  CONSTRAINT [FK_AM.PullBackRequests_Users] FOREIGN KEY([RequestedBy])
REFERENCES [Profile].[Users] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_AM.PullBackRequests_Users]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] CHECK CONSTRAINT [FK_AM.PullBackRequests_Users]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PullBackRequests_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests]  WITH CHECK ADD  CONSTRAINT [FK_PullBackRequests_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PullBackRequests_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[PullBackRequests]'))
ALTER TABLE [AM].[PullBackRequests] CHECK CONSTRAINT [FK_PullBackRequests_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Clients_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[Common].[Clients]'))
ALTER TABLE [Common].[Clients]  WITH CHECK ADD  CONSTRAINT [FK_Clients_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [Common].[BusinessUnit] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Clients_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[Common].[Clients]'))
ALTER TABLE [Common].[Clients] CHECK CONSTRAINT [FK_Clients_BusinessUnit]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_ClientSFTChannels_Clients]') AND parent_object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]'))
ALTER TABLE [Common].[ClientSFTChannels]  WITH CHECK ADD  CONSTRAINT [FK_ClientSFTChannels_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_ClientSFTChannels_Clients]') AND parent_object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]'))
ALTER TABLE [Common].[ClientSFTChannels] CHECK CONSTRAINT [FK_ClientSFTChannels_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_ClientSFTChannels_SFTChannels]') AND parent_object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]'))
ALTER TABLE [Common].[ClientSFTChannels]  WITH CHECK ADD  CONSTRAINT [FK_ClientSFTChannels_SFTChannels] FOREIGN KEY([SFTChannelId])
REFERENCES [Common].[SFTChannels] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_ClientSFTChannels_SFTChannels]') AND parent_object_id = OBJECT_ID(N'[Common].[ClientSFTChannels]'))
ALTER TABLE [Common].[ClientSFTChannels] CHECK CONSTRAINT [FK_ClientSFTChannels_SFTChannels]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_Clients]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables]  WITH CHECK ADD  CONSTRAINT [FK_Deliverables_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_Clients]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] CHECK CONSTRAINT [FK_Deliverables_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_EmailChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables]  WITH CHECK ADD  CONSTRAINT [FK_Deliverables_EmailChannel] FOREIGN KEY([EmailChannelId])
REFERENCES [Common].[EmailChannels] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_EmailChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] CHECK CONSTRAINT [FK_Deliverables_EmailChannel]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_MailPiecePageInfo_MailPieces]') AND parent_object_id = OBJECT_ID(N'[Common].[MailPiecePageInfo]'))
ALTER TABLE [Common].[MailPiecePageInfo]  WITH CHECK ADD  CONSTRAINT [FK_MailPiecePageInfo_MailPieces] FOREIGN KEY([MailPieceId])
REFERENCES [AM].[MailPieces] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_MailPiecePageInfo_MailPieces]') AND parent_object_id = OBJECT_ID(N'[Common].[MailPiecePageInfo]'))
ALTER TABLE [Common].[MailPiecePageInfo] CHECK CONSTRAINT [FK_MailPiecePageInfo_MailPieces]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_Clients]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary]  WITH CHECK ADD  CONSTRAINT [FK_BatchSummary_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_Clients]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary] CHECK CONSTRAINT [FK_BatchSummary_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary]  WITH CHECK ADD  CONSTRAINT [FK_BatchSummary_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary] CHECK CONSTRAINT [FK_BatchSummary_DocTypes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_MRDFRequest_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[MRDFRequest]'))
ALTER TABLE [PnS].[MRDFRequest]  WITH CHECK ADD  CONSTRAINT [FK_MRDFRequest_BatchSummary] FOREIGN KEY([JobId])
REFERENCES [PnS].[BatchSummary] ([PrintBatch])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_MRDFRequest_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[MRDFRequest]'))
ALTER TABLE [PnS].[MRDFRequest] CHECK CONSTRAINT [FK_MRDFRequest_BatchSummary]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_MRDFResponse_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[MRDFResponse]'))
ALTER TABLE [PnS].[MRDFResponse]  WITH CHECK ADD  CONSTRAINT [FK_MRDFResponse_BatchSummary] FOREIGN KEY([JobId])
REFERENCES [PnS].[BatchSummary] ([PrintBatch])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_MRDFResponse_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[MRDFResponse]'))
ALTER TABLE [PnS].[MRDFResponse] CHECK CONSTRAINT [FK_MRDFResponse_BatchSummary]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_ReprintMRDFRequest_ReprintBatches]') AND parent_object_id = OBJECT_ID(N'[PnS].[ReprintMRDFRequest]'))
ALTER TABLE [PnS].[ReprintMRDFRequest]  WITH CHECK ADD  CONSTRAINT [FK_ReprintMRDFRequest_ReprintBatches] FOREIGN KEY([JobId])
REFERENCES [PnS].[ReprintBatches] ([PrintBatch])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_ReprintMRDFRequest_ReprintBatches]') AND parent_object_id = OBJECT_ID(N'[PnS].[ReprintMRDFRequest]'))
ALTER TABLE [PnS].[ReprintMRDFRequest] CHECK CONSTRAINT [FK_ReprintMRDFRequest_ReprintBatches]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserLogins_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserLogins]'))
ALTER TABLE [Profile].[UserLogins]  WITH CHECK ADD  CONSTRAINT [FK_UserLogins_Users] FOREIGN KEY([UserId])
REFERENCES [Profile].[Users] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserLogins_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserLogins]'))
ALTER TABLE [Profile].[UserLogins] CHECK CONSTRAINT [FK_UserLogins_Users]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserRoles_Roles]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserRoles]'))
ALTER TABLE [Profile].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [Profile].[Roles] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserRoles_Roles]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserRoles]'))
ALTER TABLE [Profile].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UsersRoles_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserRoles]'))
ALTER TABLE [Profile].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UsersRoles_Users] FOREIGN KEY([UserId])
REFERENCES [Profile].[Users] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UsersRoles_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserRoles]'))
ALTER TABLE [Profile].[UserRoles] CHECK CONSTRAINT [FK_UsersRoles_Users]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_Users_Clients]') AND parent_object_id = OBJECT_ID(N'[Profile].[Users]'))
ALTER TABLE [Profile].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_Users_Clients]') AND parent_object_id = OBJECT_ID(N'[Profile].[Users]'))
ALTER TABLE [Profile].[Users] CHECK CONSTRAINT [FK_Users_Clients]
GO
/****** Object:  StoredProcedure [dbo].[usp_AMLogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_AMLogBatchTimeLine]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_AMLogBatchTimeLine] AS' 
END
GO



ALTER PROCEDURE [dbo].[usp_AMLogBatchTimeLine]
	-- Add the parameters for the stored procedure here
	@BatchId int,
	@ActivityName varchar(25),
	@TimeStamp datetime
AS
BEGIN
	
	IF(@ActivityName = 'DownloadStart')
	BEGIN
		INSERT INTO [AM].[LogBatchTimeLine] ([MailBatchId],[DownloadStart]) VALUES (@BatchId,@TimeStamp)
	END
	ELSE
	BEGIN
		UPDATE [AM].[LogBatchTimeLine] SET
		[DownloadEnd] = (CASE WHEN @ActivityName = 'DownloadEnd' THEN @TimeStamp ELSE [DownloadEnd] END),
		[ValidationStart] = (CASE WHEN @ActivityName = 'ValidationStart' THEN @TimeStamp ELSE [ValidationStart] END),
		[ValidationEnd] = (CASE WHEN @ActivityName = 'ValidationEnd' THEN @TimeStamp ELSE [ValidationEnd]  END),
		[AcknowledgeStart] = (CASE WHEN @ActivityName = 'AcknowledgeStart' THEN @TimeStamp ELSE [AcknowledgeStart] END),
		[AcknowledgeEnd] = (CASE WHEN @ActivityName = 'AcknowledgeEnd' THEN @TimeStamp ELSE [AcknowledgeEnd] END),
		[PreppedStart] = (CASE WHEN @ActivityName = 'PreppedStart' THEN @TimeStamp ELSE [PreppedStart] END),
		[PreppedEnd] = (CASE WHEN @ActivityName = 'PreppedEnd' THEN @TimeStamp ELSE [PreppedEnd] END),
		[SubmittedToShippingStart] = (CASE WHEN @ActivityName = 'SubmittedToShippingStart' AND [SubmittedToShippingStart] IS NULL THEN @TimeStamp ELSE [SubmittedToShippingStart] END),
		[SubmittedToShippingEnd] = (CASE WHEN @ActivityName = 'SubmittedToShippingEnd' THEN @TimeStamp ELSE [SubmittedToShippingEnd] END),
		[ShippingResponseReceivedStart] = (CASE WHEN @ActivityName = 'ShippingResponseReceivedStart' AND [ShippingResponseReceivedStart] IS NULL THEN @TimeStamp ELSE [ShippingResponseReceivedStart] END),
		[ShippingResponseReceivedEnd] = (CASE WHEN @ActivityName = 'ShippingResponseReceivedEnd' THEN @TimeStamp ELSE [ShippingResponseReceivedEnd] END),
		[ReportPrepStart] = (CASE WHEN @ActivityName = 'ReportPrepStart' THEN @TimeStamp ELSE [ReportPrepStart] END),
		[ReportPrepEnd] = (CASE WHEN @ActivityName = 'ReportPrepEnd' THEN @TimeStamp ELSE [ReportPrepEnd] END),
		[ReportBackStart] = (CASE WHEN @ActivityName = 'ReportBackStart' THEN @TimeStamp ELSE [ReportBackStart] END),
		[ReportBackEnd] = (CASE WHEN @ActivityName = 'ReportBackEnd' THEN @TimeStamp ELSE [ReportBackEnd] END)
		WHERE MailBatchId = @BatchId
	END
END
























GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteMRDFRequestByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_DeleteMRDFRequestByJobId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_DeleteMRDFRequestByJobId] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_DeleteMRDFRequestByJobId]
	@jobId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DELETE FROM PnS.MRDFRequest WHERE JobId=@jobId
END






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetAllClientAppDetail]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetAllClientAppDetail] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetAllClientAppDetail]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT 
	C.Id, C.Name, C.AppCode, C.BusinessUnit, C.IsAMActive, C.IsPnSActive, C.ModifiedOn, C.ModifiedBy
	, C.CreatedOn, C.CreatedBy, C.TimeZone
	,B.Id As BUId
	,B.CustCode
	,B.[BusinessUnit]
  FROM [Common].[Clients] C INNER JOIN [Common].[BusinessUnit] B ON B.Id = C.BusinessUnit
END


/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 11/16/2017 17:20:19 ******/
SET ANSI_NULLS ON






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummaryById]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummaryById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetBatchSummaryById] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetBatchSummaryById]
	-- Add the parameters for the stored procedure here
	@batchId int
AS
BEGIN

	SELECT PrintBatch, ClientId, JobName, PrintFileName, BatchIdentifier, PipelineStatus, ValidationStatus, OnValidationErrors,
 ValidationErrors, BatchStatus, IsReceived, ReceivedOn, IsDownloaded, DownloadedOn, IsPrepped, PreppedOn, IsSubmitted,
 SubmittedOn, IsMRDFReceived, MRDFReceivedOn, IsReportPrepped, ReportPreppedOn, IsReportedBack, ReportedBackOn, IsSuspended, 
 SuspendedOn, IsCancelled, CancelledOn, JulianDate, Images, DocTypeId,SLADays,DueDate
  FROM [PnS].[BatchSummary] WHERE PrintBatch = @batchId
END

/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 11/16/2017 17:20:05 ******/
SET ANSI_NULLS ON






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummarySchema]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummarySchema]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetBatchSummarySchema] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetBatchSummarySchema]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	
	SELECT [Id]
      ,[Fieldname]
      ,[Startposition]
      ,[EndPosition]
      ,[Length]
      ,[Type]
      ,[IsActive]
      ,[Comments]
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
      ,[DefaultValue]
  FROM [PnS].[Schema_BatchSummary]	WHERE IsActive = 1
  ORDER BY Id
END





















GO
/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetClientAppDetailsById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetClientAppDetailsById] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetClientAppDetailsById]
	-- Add the parameters for the stored procedure here
	@appId int
AS
BEGIN
	SELECT C.[Id]
	  ,B.Id AS BUId
	  ,B.BusinessUnit
	  ,B.CustCode
      ,[AppCode]
      ,[Name]
	  ,[TimeZone]
      ,[IsAMActive]
	  ,[IsPnSActive]
      ,[ModifiedOn]
      ,[ModifiedBy]
      ,[CreatedOn]
      ,[CreatedBy]
  FROM [Common].[Clients] C
  INNER JOIN Common.BusinessUnit B ON B.Id=C.BusinessUnit
  WHERE C.Id = @appId
END


/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 11/16/2017 17:20:30 ******/
SET ANSI_NULLS ON





















GO
/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetDocTypeDetailsByAppId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
 @clientId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails]
 ,D.[Description] as DocTypeDesc,D.DocTypeCode,D.IsActive,D.ModifiedOn
 ,D.CreatedOn,D.Id AS DocTypeId
 FROM AM.DocTypes D
 INNER JOIN Common.Clients C ON C.Id=D.ClientId
 WHERE C.Id=@clientId
 
END


/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 11/16/2017 17:20:56 ******/
SET ANSI_NULLS ON






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetFinalMRDFResponseDataByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetFinalMRDFResponseDataByJobId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId]
@jobId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PostageCost, ErrId, ShippedOn, WeightMeasured, Disposition
 , PieceId, UniqueId, PDFFileName, ErrDesc, MailPieceStatus,TotalSheets,DivertVS3 
 FROM view_MRDFResponseData V WHERE V.JobId=@jobId
END
























GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestDataByJobId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByJobId] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetMRDFRequestDataByJobId]
@jobId int,
@mailPieceStatus varchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	If(@mailPieceStatus='ALL')
	BEGIN
		SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
		, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
		, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
		, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
		, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
		, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus 
		FROM [PnS].[MRDFRequest] WHERE JobId = @jobId
	END
	ELSE If(@mailPieceStatus='INTERNAL')--This will be used for getting only submitted records to PCI
	BEGIN
		SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
		, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
		, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
		, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
		, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
		, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus 
		FROM [PnS].[MRDFRequest] WHERE JobId = @jobId AND ErrId NOT IN (-1,-2,-3)
	END
	ELSE
	BEGIN
		SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
		, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
		, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
		, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
		, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
		, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus 
		FROM [PnS].[MRDFRequest] WHERE JobId = @jobId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus
	END
END






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestSchema]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestSchema]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetMRDFRequestSchema] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetMRDFRequestSchema]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT [Id]
      ,[Fieldname]
      ,[Type]
      ,[Startposition]
      ,[Length]
      ,[IsActive]
      ,[Description]
      ,[Comments]
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
      ,[DefaultValue]
  FROM [PnS].[Schema_MRDFRequest]	WHERE IsActive = 1
  ORDER BY Id
END





















GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByJobId]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFResponseDataByJobId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByJobId] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetMRDFResponseDataByJobId]
@jobId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PieceId, Date, Feeder2_Fed, OperatorId, MachineId, ProcessingTimeStamp, DetailedErrorId
 , Disposition, Input_Fed, Feeder3_Fed, Feeder4_Fed, Feeder5_Fed, ComputedWeight, MeterDate
 , StatusSource, PostageMetered, WeightMeasured, ExitLocation, TotalSheets, PDFFileName 
 FROM [Pns].[MRDFResponse] MS 
 WHERE MS.JobId=@jobId
END






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseSchema]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFResponseSchema]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetMRDFResponseSchema] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetMRDFResponseSchema]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT [Id]
      ,[Fieldname]
      ,[Type]
      ,[Startposition]
      ,[Length]
      ,[IsActive]
      ,[Description]
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
  FROM [PnS].[Schema_MRDFResponse]	WHERE IsActive = 1
  ORDER BY Id
END






















GO
/****** Object:  StoredProcedure [dbo].[usp_GetNextBatchIdByJobName]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetNextBatchIdByJobName]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetNextBatchIdByJobName] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetNextBatchIdByJobName] 
	-- Add the parameters for the stored procedure here
	@jobName varchar(50),
	@operation varchar(50)
AS
BEGIN
	
	IF (@jobName = 'PnSJobMonitor' AND @operation = 'ReportBack')
	BEGIN
		SELECT TOP 1 PrintBatch FROM [PnS].BatchSummary WHERE PipelineStatus = 'ReportPrepped' AND  IsReportPrepped = 1 AND IsReportedBack = 0
	END
    ELSE IF(@jobName = 'PnSCoreService')
    BEGIN
		IF(@operation = 'Prep')
		BEGIN
			SELECT TOP 1 PrintBatch AS BatchId FROM PnS.BatchSummary WHERE PipelineStatus = 'Downloaded' AND  IsDownloaded = 1 AND IsPrepped = 0 
		END
		ELSE IF(@operation = 'ProcessMRDF')
		BEGIN
			SELECT TOP 1 PrintBatch AS BatchId FROM PnS.BatchSummary WHERE PipelineStatus = 'MRDFReceived' AND  IsMRDFReceived = 1 AND IsReportPrepped = 0
		END
		ELSE
		BEGIN
			SELECT NULL AS BatchId
		END
    END
	ELSE IF(@jobName = 'PCIJobMonitor' AND @operation = 'Submit')
    BEGIN
		SELECT TOP 1 PrintBatch AS BatchId FROM PnS.BatchSummary WHERE PipelineStatus = 'Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 
    END
	ELSE
    BEGIN
       SELECT NULL AS BatchId
    END

END


























GO
/****** Object:  StoredProcedure [dbo].[usp_GetSubmittedBatches]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetSubmittedBatches]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetSubmittedBatches] AS' 
END
GO

ALTER PROCEDURE [dbo].[usp_GetSubmittedBatches]
AS
BEGIN

--SELECT * FROM dbo.BatchSummary
SELECT B.PrintBatch,DT.DocTypeCode,C.AppCode,BU.CustCode,CONVERT(VARCHAR(10),B.PrintBatch) as BatchCode 
FROM [PnS].BatchSummary B
INNER JOIN [AM].DocTypes DT ON DT.Id=B.DocTypeId 
INNER JOIN [Common].Clients C ON C.Id=B.ClientId
INNER JOIN [Common].BusinessUnit BU ON BU.Id=C.BusinessUnit
WHERE PipelineStatus='Submitted' AND C.IsPnSActive=1
UNION
SELECT RB.ParentPrintBatch as PrintBatch,RB.DocTypeCode,C.AppCode,BU.CustCode,RB.PrintBatch as BatchCode 
FROM [PnS].ReprintBatches RB
INNER JOIN [Common].Clients C ON C.Id=RB.AppId
INNER JOIN [Common].BusinessUnit BU ON BU.Id=C.BusinessUnit
WHERE PipelineStatus='Submitted' AND C.IsPnSActive=1

END





















GO
/****** Object:  StoredProcedure [dbo].[usp_InsertPullBackRequestsData]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_InsertPullBackRequestsData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_InsertPullBackRequestsData] AS' 
END
GO
-- =============================================
-- Author:		Akshar
-- Create date: 26-Mar-18
-- Description:	Insert Pullback requests data
-- Execute : usp_InsertPullBackRequestsData 'edd717f1-de00-41d1-92c1-febe34c52f9d' 
-- =============================================
ALTER PROCEDURE [dbo].[usp_InsertPullBackRequestsData]
	-- Add the parameters for the stored procedure here
	@MailPieceUniqueId uniqueidentifier,
	@ClientId int
AS
BEGIN

DECLARE @MailBatchId int, @MailPieceId int,@RequestedOn datetime,@RequestedBy nvarchar(128)

SELECT @MailBatchId = AMM.MailBatchId,@MailPieceId = AMM.Id,@MailPieceUniqueId = PM.[UniqueId],@RequestedOn =  AMM.PullBackRequestedOn,@RequestedBy = AMM.PulledBackRequestedBy FROM PnS.MRDFRequest PM INNER JOIN AM.MailPieces AMM ON PM.[UniqueId] = AMM.UniqueId WHERE PM.[UniqueId] = @MailPieceUniqueId

	INSERT INTO [AM].[PullBackRequests]
           ([MailBatchId]
		   ,[ClientId]
           ,[MailPieceId]
           ,[MailPieceUniqueId]
           ,[Status]
           ,[RequestedOn]
           ,[RequestedBy]
		   )
     VALUES
           (@MailBatchId
		   ,@ClientId
           ,@MailPieceId
           ,@MailPieceUniqueId
           ,'Requested'
           ,@RequestedOn
           ,@RequestedBy
		   )
END








GO
/****** Object:  StoredProcedure [dbo].[usp_LogBatchTimeLine]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_LogBatchTimeLine]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_LogBatchTimeLine] AS' 
END
GO

-- =============================================
-- Author:		Akshar
-- Create date: 5 Sep 2017
-- Description:	To track the batch timing history 
-- =============================================
ALTER PROCEDURE [dbo].[usp_LogBatchTimeLine]
	-- Add the parameters for the stored procedure here
	@BatchId int,
	@ActivityName varchar(25),
	@TimeStamp datetime
AS
BEGIN
	
	IF(@ActivityName = 'DownloadStart')
	BEGIN
		INSERT INTO [PnS].[LogBatchTimeLine] ([BatchId],[DownloadStart]) VALUES (@BatchId,@TimeStamp)
	END
	ELSE
	BEGIN
		UPDATE [PnS].[LogBatchTimeLine] SET
		[DownloadEnd] = (CASE WHEN @ActivityName = 'DownloadEnd' THEN @TimeStamp ELSE [DownloadEnd] END),
		[PCIBatchPrepStart] = (CASE WHEN @ActivityName = 'PCIBatchPrepStart' THEN @TimeStamp ELSE [PCIBatchPrepStart] END),
		[PCIBatchPrepEnd] = (CASE WHEN @ActivityName = 'PCIBatchPrepEnd' THEN @TimeStamp ELSE [PCIBatchPrepEnd]  END),
		[IEReportPrepStart] = (CASE WHEN @ActivityName = 'IEReportPrepStart' THEN @TimeStamp ELSE [IEReportPrepStart] END),
		[IEReportPrepEnd] = (CASE WHEN @ActivityName = 'IEReportPrepEnd' THEN @TimeStamp ELSE [IEReportPrepEnd] END),
		[SubmitToPCIStart] = (CASE WHEN @ActivityName = 'SubmitToPCIStart' THEN @TimeStamp ELSE [SubmitToPCIStart] END),
		[SubmitToPCIEnd] = (CASE WHEN @ActivityName = 'SubmitToPCIEnd' THEN @TimeStamp ELSE [SubmitToPCIEnd] END),
		[MRDFDownloadStart] = (CASE WHEN @ActivityName = 'MRDFDownloadStart' AND [MRDFDownloadStart] IS NULL THEN @TimeStamp ELSE [MRDFDownloadStart] END),
		[MRDFDownloadEnd] = (CASE WHEN @ActivityName = 'MRDFDownloadEnd' THEN @TimeStamp ELSE [MRDFDownloadEnd] END),
		[ReportPrepStart] = (CASE WHEN @ActivityName = 'ReportPrepStart' THEN @TimeStamp ELSE [ReportPrepStart] END),
		[ReportPrepEnd] = (CASE WHEN @ActivityName = 'ReportPrepEnd' THEN @TimeStamp ELSE [ReportPrepEnd] END),
		[ReportBackStart] = (CASE WHEN @ActivityName = 'ReportBackStart' THEN @TimeStamp ELSE [ReportBackStart] END),
		[ReportBackEnd] = (CASE WHEN @ActivityName = 'ReportBackEnd' THEN @TimeStamp ELSE [ReportBackEnd] END)
		WHERE BatchId = @BatchId
	END
END






















GO
/****** Object:  StoredProcedure [dbo].[usp_MapClientSFTChannels]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_MapClientSFTChannels]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_MapClientSFTChannels] AS' 
END
GO
-- =============================================
-- Author:		Akshar Vala
-- Create date: 27 Mar 2018
-- Description:	To map data in ClientSFTChannels table
-- =============================================
ALTER PROCEDURE [dbo].[usp_MapClientSFTChannels] 
	-- Add the parameters for the stored procedure here
	@AppCode varchar(128), 
	@SFTChannelCode varchar(128)
AS
BEGIN
	INSERT INTO [Common].[ClientSFTChannels]([ClientId],[SFTChannelId])
    SELECT (SELECT Id FROM Common.Clients WHERE AppCode = @AppCode) ClientId, 
	(SELECT Id FROM Common.SFTChannels WHERE Code = @SFTChannelCode)SFTChannelId 
END





GO
/****** Object:  StoredProcedure [dbo].[usp_RevertPipelineStatus]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RevertPipelineStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_RevertPipelineStatus] AS' 
END
GO



-- =============================================
-- Author:		Akshar Vala
-- Create date: 17 Oct 2017
-- Description:	Revert pipeline status from current status
-- =============================================
ALTER PROCEDURE [dbo].[usp_RevertPipelineStatus]
	@batchId int,
	@pipelineStatus varchar(512)
AS
BEGIN
	IF(@pipelineStatus = 'Downloaded')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Received',DownloadedOn = NULL,PreppedOn = NULL,SubmittedOn = NULL,
		 MRDFReceivedOn = NULL,ReportPreppedOn = NULL,ReportedBackOn = NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Prepped')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Downloaded',PreppedOn = NULL,SubmittedOn = NULL,MRDFReceivedOn = NULL,
		 ReportPreppedOn = NULL,ReportedBackOn = NULL,ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL
		 ,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Submitted')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Prepped',SubmittedOn = NULL,MRDFReceivedOn = NULL,ReportPreppedOn = NULL,
		 ReportedBackOn = NULL,ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'MRDFReceived')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Submitted',MRDFReceivedOn = NULL,ReportPreppedOn = NULL
		 ,ReportedBackOn = NULL,SuspendedOn=NULL,ValidationErrors=NULL
		 WHERE PrintBatch = @batchId
		 --This is needs to be done because if some of the MRDF has been received and mailpieces are updated 
		 --then need to update MailPieceStatus to InProgress
		 UPDATE [PnS].MRDFRequest
		 SET MailPieceStatus='InProgress',ErrId=0,ErrDesc=NULL
		 WHERE JobId=@batchId AND (ErrId=7 OR ErrId=0) AND MailPieceStatus!='Aborted'
		 --If we are reverting to Submitted status then need to delete mrdf response at batch level
		 DELETE FROM [PnS].MRDFResponse WHERE JobId=@batchId
	END
	ELSE IF(@pipelineStatus = 'ReportPrepped')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'MRDFReceived',ReportPreppedOn = NULL,ReportedBackOn = NULL
		 ,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'ReportedBack')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'ReportPrepped',ReportedBackOn = NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Reprint')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Submitted',MRDFReceivedOn = NULL,ReportPreppedOn = NULL,ReportedBackOn = NULL,
		 ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL
		 WHERE PrintBatch = @batchId
		 DELETE FROM [PnS].MRDFResponse WHERE Disposition LIKE '%7%'
	END
END






















GO
/****** Object:  StoredProcedure [dbo].[uspweb_DailyDetailOperationsReport]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_DailyDetailOperationsReport]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_DailyDetailOperationsReport] AS' 
END
GO
-- =============================================
-- Author:		 Milan Patel
-- Create date:  01-Mar-18
-- Modifiy date: 01-Mar-18
-- Exec: [dbo].[uspweb_DailyDetailOperationsReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null,null
-- Description:	 Get Batch by parameters clientId and (date or batchid)
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_DailyDetailOperationsReport]
	
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@batchId int,
	@channel varchar(128),
	@shippingStatus varchar(255)
AS
BEGIN

SELECT 
    b.Id as [BatchId],
	b.UniqueId as [BatchUniqueId],
	b.ClientBatchId,
	b.BatchName,
	b.[ReceivedOn],
	b.PrintLocation,
	m.Id as [MailPieceId],
	m.UniqueId as [MailPieceUniqueId],
	m.[DocumentOrPackageType],
	m.[BusinessFriendlyIdName],
	m.[BusinessFriendlyIdValue],
	m.[FileName],
	m.[PrintedOn],
	m.[ShippedOn] as [MailedOn],
	m.[TrackingNo],	
	m.[RcptFirstName],
	m.[RcptLastName],
	m.[RcptOrganization],	
	m.[RcptAddressStreet],	
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.[TotalPages] as [PagesShipped],
	m.[ShippingStatus],
	c.Id as [ClientId],
	c.AppCode as [ClientCode],
	c.TimeZone as [ClientTimeZone],
	d.[ShippingServiceType] as [ShippingMethod],
	ch.Title as [Channel],
	ch.Code as [ChannelCode]
	FROM [AM].[MailBatches] b, [Common].[Clients] c,[Common].[SFTChannels] ch,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND ISNULL(m.ShippingStatus, '') = ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, ''))
END



GO
/****** Object:  StoredProcedure [dbo].[uspweb_ErrorReport]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_ErrorReport]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_ErrorReport] AS' 
END
GO
-- =============================================
-- Author:		 Milan Patel
-- Create date:  28-Feb-18
-- Modifiy date: 03-Mar-18
-- Exec [dbo].[uspweb_ErrorReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null,null
-- Description:	 Get Batch by parameters
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_ErrorReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@batchId int,
	@channel varchar(128),
	@shippingStatus varchar(255)
AS
BEGIN

select 
    b.Id as [BatchId],
	b.UniqueId as [BatchUniqueId],
	b.ClientBatchId,
	b.BatchName,
	b.[ReceivedOn],
	m.Id as [MailPieceId],
	m.UniqueId as [MailPieceUniqueId],
	m.[DocumentOrPackageType],
	m.[BusinessFriendlyIdName],
	m.[BusinessFriendlyIdValue],
	m.SLADays,
	m.DueDate,
	m.[FileName],
	m.[RcptFirstName],
	m.[RcptLastName],
	m.[RcptAddressStreet],
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.[ErrorCode],
	m.[ErrorDesc],
	m.ShippingStatus,
	c.Id as [ClientId],
	c.AppCode as [ClientCode],
	c.TimeZone as [ClientTimeZone],
	d.[ShippingServiceType] as [ShippingMethod],
	ch.Title as [Channel],
	ch.Code as [ChannelCode]	
	from [AM].[MailBatches] b, [Common].[Clients] c, [Common].[SFTChannels] ch,[AM].[MailPieces] m
	left outer join [AM].[DocTypes] d on m.DocTypeId=d.Id
	where m.MailBatchId=b.Id and c.Id=b.ClientId and ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND ISNULL(m.ShippingStatus, '') = (ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, '')))
	END



GO
/****** Object:  StoredProcedure [dbo].[uspweb_ErrorReportSLA]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_ErrorReportSLA]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_ErrorReportSLA] AS' 
END
GO
-- =============================================
-- Author:		 Milan Patel
-- Create date:  28-Feb-18
-- Modifiy date: 03-Mar-18
-- Exec [dbo].[uspweb_ErrorReportSLA] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null,null,null
-- Description:	 Get Batch by parameters
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_ErrorReportSLA]
	@clientId int,
	@startDateRec datetime,
	@endDateRec datetime,
	@startDateDue datetime,
	@endDateDue datetime,
	@batchId int,
	@docType varchar(128)
AS
BEGIN

select 
    b.Id as [BatchId],
	b.UniqueId as [BatchUniqueId],
	b.ClientBatchId,
	b.BatchName,
	b.[ReceivedOn],
	m.Id as [MailPieceId],
	m.UniqueId as [MailPieceUniqueId],
	m.[DocumentOrPackageType],
	m.[BusinessFriendlyIdName],
	m.[BusinessFriendlyIdValue],
	m.SLADays,
	m.DueDate,
	m.[FileName],
	m.[RcptFirstName],
	m.[RcptLastName],
	m.[RcptAddressStreet],
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.[ErrorCode],
	m.[ErrorDesc],
	m.ShippingStatus,
	c.Id as [ClientId],
	c.AppCode as [ClientCode],
	c.TimeZone as [ClientTimeZone],
	d.[ShippingServiceType] as [ShippingMethod],
	ch.Title as [Channel],
	ch.Code as [ChannelCode]	
	from [AM].[MailBatches] b, [Common].[Clients] c, [Common].[SFTChannels] ch,[AM].[MailPieces] m
	left outer join [AM].[DocTypes] d on m.DocTypeId=d.Id
	where m.MailBatchId=b.Id and c.Id=b.ClientId and ch.Id=b.SFTChannelId AND m.ClientId=@clientId AND m.SLADays < 0
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDateRec, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDateRec, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.DueDate,0) BETWEEN ISNULL(@startDateDue, ISNULL(m.DueDate, 0)) AND ISNULL(@endDateDue, ISNULL(m.DueDate, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, ''))
	AND m.ShippingStatus IN ('InProgress','Shipped')
	--AND ISNULL(m.ShippingStatus, '') IN (ISNULL('''InProgress'',''Shipped''', ISNULL(m.ShippingStatus, '')))
	END
	


GO
/****** Object:  StoredProcedure [dbo].[uspweb_GetMailPieceByBatchId]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_GetMailPieceByBatchId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_GetMailPieceByBatchId] AS' 
END
GO
-- =============================================
-- Author:		Milan Patel
-- Create date: 03-Mar-18
-- Description:	Get MailPiece by batchid.
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_GetMailPieceByBatchId]
	-- Add the parameters for the stored procedure here
	@batchId int,
	@clientId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select 
m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,
m.DocumentOrPackageType,m.[FileName],m.DueDate,m.TotalPages,m.PrintedOn,m.TrackingNo,
m.[ShippingStatus],
m.[RcptPrefix],
	m.[RcptFirstName],
	m.[RcptMiddleName],
	m.[RcptLastName],
	m.[RcptSuffix],
	m.[RcptOrganization],
	m.[RcptAddrOptionalLine1AboveStreet],
	m.[RcptAddrOptionalLine2AboveStreet],
	m.[RcptAddressStreet],
	m.[RcptAddressAptOrSuiteOrOther],
	m.[RcptAddrOptionalLine1BelowStreet],
	m.[RcptAddrOptionalLine2BelowStreet],
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.ErrorCode,
	m.ErrorDesc,
	c.TimeZone as [ClientTimeZone],
b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,
b.ReceivedOn,
d.[ShippingServiceType] as [ShippingMethod]
from [AM].[MailBatches] b,[Common].[Clients] c,[AM].[MailPieces] m
left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id
where m.MailBatchId=b.Id and b.ClientId=@clientId and m.ClientId=c.Id
and b.Id=@batchId
END




GO
/****** Object:  StoredProcedure [dbo].[uspweb_GetMailPieceByBatchIdAndStatus]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_GetMailPieceByBatchIdAndStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_GetMailPieceByBatchIdAndStatus] AS' 
END
GO
-- =============================================
-- Author:		Milan Patel
-- Create date: 03-Mar-18
-- Description:	Get MailPiece by batchid and status.
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_GetMailPieceByBatchIdAndStatus]
	-- Add the parameters for the stored procedure here
	@batchId int,
	@status varchar(255),
	@clientId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select 
m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,
m.DocumentOrPackageType,m.[FileName],m.DueDate,m.TotalPages,m.PrintedOn,m.TrackingNo,
m.[ShippingStatus],
	m.[RcptFirstName],
	m.[RcptLastName],
	m.[RcptAddressStreet],
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.ErrorCode,
	m.ErrorDesc,
	c.TimeZone as [ClientTimeZone],
b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,
b.ReceivedOn,
d.[ShippingServiceType] as [ShippingMethod]
from [AM].[MailBatches] b,[Common].[Clients] c,[AM].[MailPieces] m
left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id
where m.MailBatchId=b.Id and b.ClientId=@clientId
and m.ShippingStatus=@status and b.Id=@batchId and m.ClientId=c.Id
END




GO
/****** Object:  StoredProcedure [dbo].[uspweb_MailPieceSearch]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_MailPieceSearch]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_MailPieceSearch] AS' 
END
GO
-- =============================================
-- Author:		 Milan Patel
-- Create date:  07-Mar-18
-- Modifiy date: 07-Mar-18
-- Exec: [dbo].[usp_MailPieceSearch] 101,null,null,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null,null
--       [dbo].[usp_MailPieceSearch] 101,null,null,'2018-03-19 04:00:00',null,null,null,null
-- Description:	 Get Batch by parameters
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_MailPieceSearch]

	@clientId int,
	@businessFriendlyIdName varchar(255),
	@businessFriendlyIdValue varchar(255),
	@startDate datetime,
	@endDate datetime,
	@docType varchar(255),
	@shippingStatus varchar(255),
	@channel varchar(128),
	@batchId int
AS
BEGIN

	SELECT 
 	   m.[Id] AS [MailPieceId]
      ,m.[UniqueId] AS [MailPieceUniqueId]
      ,m.[ClientId]
      ,m.[MailBatchId]
      ,m.[PrintBatchId]
      ,m.[BusinessFriendlyIdName]
      ,m.[BusinessFriendlyIdValue]
      ,m.[FileName]
      ,m.[FilePattern]
      ,m.[DocumentOrPackageType]
      ,m.[SLADays]
      ,m.[DueDate]
      ,m.[ShippingStatus]
      ,m.[IsRcptInfoSpecified]
      ,m.[RcptPrefix]
      ,m.[RcptFirstName]
      ,m.[RcptMiddleName]
      ,m.[RcptLastName]
      ,m.[RcptSuffix]
      ,m.[RcptPhone]
      ,m.[RcptOrganization]
      ,m.[CoRcptPrefix]
      ,m.[CoRcptFirstName]
      ,m.[CoRcptMiddleName]
      ,m.[CoRcptLastName]
      ,m.[CoRcptSuffix]
      ,m.[CoRcptPhone]
      ,m.[CoRcptOrganization]
      ,m.[RcptAddrOptionalLine1AboveStreet]
      ,m.[RcptAddrOptionalLine2AboveStreet]
      ,m.[RcptAddressStreet]
      ,m.[RcptAddressAptOrSuiteOrOther]
      ,m.[RcptAddrOptionalLine1BelowStreet]
      ,m.[RcptAddrOptionalLine2BelowStreet]
      ,m.[RcptAddressCity]
      ,m.[RcptAddressState]
      ,m.[RcptAddressZIP]
      ,m.[IsReturnInfoSpecified]
      ,m.[RetRcptPrefix]
      ,m.[RetRcptFirstName]
      ,m.[RetRcptMiddleName]
      ,m.[RetRcptLastName]
      ,m.[RetRcptSuffix]
      ,m.[RetRcptPhone]
      ,m.[RetRcptOrganization]
      ,m.[RetRcptAddrOptionalLine1AboveStreet]
      ,m.[RetRcptAddrOptionalLine2AboveStreet]
      ,m.[RetRcptAddressStreet]
      ,m.[RetRcptAddressAptOrSuiteOrOther]
      ,m.[RetRcptAddrOptionalLine1BelowStreet]
      ,m.[RetRcptAddrOptionalLine2BelowStreet]
      ,m.[RetRcptAddressCity]
      ,m.[RetRcptAddressState]
      ,m.[RetRcptAddressZIP]
      ,m.[TotalPages]
      ,m.[TotalSheets]
      ,m.[ErrorCode]
      ,m.[ErrorDesc]
      ,m.[TrackingNo]
      ,m.[IsPullBackRequested]
      ,m.[PullBackRequestedOn]
      ,m.[IsPulledBack]
      ,m.[PulledBackOn]
      ,m.[PulledBackRequestedBy]
      ,m.[IsPrinted]
      ,m.[PrintedOn]
      ,m.[IsShipped]
      ,m.[ShippedOn]
      ,m.[ShippingCost]
      ,m.[Weight]
      ,m.[IsQCRequested]
      ,m.[IsQCPerformed]
	  ,b.[UniqueId] AS [BatchUniqueId]
	  ,b.[BatchName]
	  ,b.[BatchStatus]
	  ,b.[ReceivedOn]
	  ,b.[SFTChannelId]
	  ,b.[ClientBatchId]
	  ,b.[PrintLocation]
	  ,ch.[Title] AS [Channel]
	  ,ch.Code AS [ChannelCode]
	  ,c.AppCode AS [ClientCode]
	  ,c.TimeZone AS [ClientTimeZone]
	  ,u.UserName AS [PulledBackRequestedByName]
	  ,d.[ShippingServiceType] AS [ShippingMethod]
	 FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	 LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id	 
	 LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	 WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id AND m.ClientId=@clientId
	 AND ISNULL(m.BusinessFriendlyIdName, '') = ISNULL(@businessFriendlyIdName, ISNULL(m.BusinessFriendlyIdName, ''))
	 AND ISNULL(m.BusinessFriendlyIdValue, '') = ISNULL(@businessFriendlyIdValue, ISNULL(m.BusinessFriendlyIdValue, ''))
	 AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, ''))
	 AND ISNULL(m.ShippingStatus, '') = ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, ''))
	 AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	 AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	 AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
END



GO
/****** Object:  StoredProcedure [dbo].[uspweb_MonthlyReconReport]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_MonthlyReconReport]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport] AS' 
END
GO
-- =============================================
-- Author:		 Milan Patel
-- Create date:  26-Feb-18
-- Modifiy date: 05-Mar-18
-- Exec: [dbo].[uspweb_MonthlyReconReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',null,null
-- Description:	 Get Batch by parameters
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128),
	@shippingStatus varchar(255)
AS
select 
 	b.[ReceivedOn],
	b.PrintLocation,
	b.ClientBatchId,
	m.Id as [MailPieceId],
	m.UniqueId as [MailPieceUniqueId],
	m.[DocumentOrPackageType] as [DocumentOrPackageType],
	m.[BusinessFriendlyIdName],
	m.[BusinessFriendlyIdValue],
	m.[FileName],
	m.[PrintedOn],
	m.[ShippedOn] as [MailedOn],
	m.[TrackingNo],	
	m.[RcptFirstName],	
	m.[RcptLastName],
	m.[RcptAddressStreet],
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.[ShippingStatus],
	m.[TotalPages] as [PagesShipped],
	m.SLADays,
	c.Id as [ClientId],
	c.AppCode as [ClientCode],
	c.TimeZone as [ClientTimeZone],
	d.[ShippingServiceType] as [ShippingMethod],
	ch.Title as [Channel],
	ch.Code as [ChannelCode]
	FROM [AM].[MailBatches] b, [Common].[Clients] c,[Common].[SFTChannels] ch,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND ISNULL(m.ShippingStatus, '') = ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, ''))



GO
/****** Object:  StoredProcedure [dbo].[uspweb_SummaryControlReport]    Script Date: 3/28/2018 3:47:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspweb_SummaryControlReport]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[uspweb_SummaryControlReport] AS' 
END
GO
-- =============================================
-- Author:		 Milan Patel
-- Create date:  28-Feb-18
-- Modifiy date: 05-Mar-18
-- Exec [dbo].[uspweb_SummaryControlReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',null
-- Description:	 Get Batch by parameters
-- =============================================
ALTER PROCEDURE [dbo].[uspweb_SummaryControlReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128)
AS
BEGIN
select 
    b.Id as [BatchId],
	b.UniqueId as [BatchUniqueId],
	b.[ReceivedOn],
	b.ClientBatchId,
	b.BatchName,
	b.PrintLocation,
	m.Id as [MailPieceId],
	m.UniqueId as [MailPieceUniqueId],	
	m.[DocumentOrPackageType],
	m.[BusinessFriendlyIdName],
	m.[BusinessFriendlyIdValue],
	m.[FileName],
	m.[PrintedOn],
	m.[ShippedOn] as [MailedOn],
	m.DueDate,
	m.[TrackingNo],
	m.[RcptFirstName],
	m.[RcptLastName],
	m.[RcptAddressStreet],
	m.[RcptAddressCity],
	m.[RcptAddressState],
	m.[RcptAddressZIP],
	m.[ErrorCode],
	m.[ErrorDesc],
	m.[TotalPages] as [PagesShipped],
	c.Id as [ClientId],
	c.AppCode as [ClientCode],
	c.TimeZone as [ClientTimeZone],
	d.[ShippingServiceType] as [ShippingMethod],
	ch.Title as [Channel],
	ch.Code as [ChannelCode]
	FROM [AM].[MailBatches] b, [Common].[Clients] c,[Common].[SFTChannels] ch,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	END



GO