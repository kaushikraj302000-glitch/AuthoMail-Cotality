USE [AuthoMail]
GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateDocTypeDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_UpdateDocTypeDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_UpdateDocTypeDetails]
GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateBusinessUnitDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_UpdateBusinessUnitDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_UpdateBusinessUnitDetails]
GO
/****** Object:  StoredProcedure [dbo].[usp_RevertPipelineStatus]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RevertPipelineStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_RevertPipelineStatus]
GO
/****** Object:  StoredProcedure [dbo].[usp_MoveBatchToHistoryTables]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_MoveBatchToHistoryTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_MoveBatchToHistoryTables]
GO
/****** Object:  StoredProcedure [dbo].[usp_LogBatchTimeLine]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_LogBatchTimeLine]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_LogBatchTimeLine]
GO
/****** Object:  StoredProcedure [dbo].[usp_InsertDocTypeDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_InsertDocTypeDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_InsertDocTypeDetails]
GO
/****** Object:  StoredProcedure [dbo].[usp_InsertBusinessUnitDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_InsertBusinessUnitDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_InsertBusinessUnitDetails]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetTodaysSubmittedBatchesCount]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetTodaysSubmittedBatchesCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetTodaysSubmittedBatchesCount]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetSubmittedBatches]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetSubmittedBatches]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetSubmittedBatches]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetPendingBatches]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetPendingBatches]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetPendingBatches]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetNextBatchIdByJobName]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetNextBatchIdByJobName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetNextBatchIdByJobName]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetNextBatchByJobName]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetNextBatchByJobName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetNextBatchByJobName]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseSchema]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFResponseSchema]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFResponseSchema]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFResponseDataByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFResponseDataByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestSchema]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestSchema]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFRequestSchema]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataForReprintByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestDataForReprintByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataForReprintByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestDataByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetFinalMRDFResponseDataByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetFinalMRDFResponseDataByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetDocTypesByDoctypeId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetDocTypesByDoctypeId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetDocTypesByDoctypeId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetDocTypeDetailsByAppId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetClientAppDetailsById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetClientAppDetailsById]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetBusinessUnits]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBusinessUnits]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetBusinessUnits]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetBusinessUnitById]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBusinessUnitById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetBusinessUnitById]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummarySchema]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummarySchema]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetBatchSummarySchema]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummaryById]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummaryById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetBatchSummaryById]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetAllClientAppDetail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetAllClientAppDetail]
GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteMRDFResponseByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_DeleteMRDFResponseByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_DeleteMRDFResponseByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteMRDFRequestByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_DeleteMRDFRequestByJobId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_DeleteMRDFRequestByJobId]
GO
/****** Object:  StoredProcedure [dbo].[usp_CheckBatchDataForReprint]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_CheckBatchDataForReprint]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_CheckBatchDataForReprint]
GO
/****** Object:  StoredProcedure [dbo].[usp_BatchStatusChart]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_BatchStatusChart]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_BatchStatusChart]
GO
/****** Object:  StoredProcedure [dbo].[usp_ActivateOrInActivateBusinessUnit]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ActivateOrInActivateBusinessUnit]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_ActivateOrInActivateBusinessUnit]
GO
/****** Object:  StoredProcedure [dbo].[usp_ActivateInactivateDocTypes]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ActivateInactivateDocTypes]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_ActivateInactivateDocTypes]
GO
/****** Object:  StoredProcedure [dbo].[usp_ActivateClientApp]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ActivateClientApp]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_ActivateClientApp]
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
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserClaims_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserClaims]'))
ALTER TABLE [Profile].[UserClaims] DROP CONSTRAINT [FK_UserClaims_Users]
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
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_HistoryMRDFResponse_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_MRDFResponse]'))
ALTER TABLE [PnS].[History_MRDFResponse] DROP CONSTRAINT [FK_HistoryMRDFResponse_BatchSummary]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_HistoryMRDFRequest_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_MRDFRequest]'))
ALTER TABLE [PnS].[History_MRDFRequest] DROP CONSTRAINT [FK_HistoryMRDFRequest_BatchSummary]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_History_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_BatchSummary]'))
ALTER TABLE [PnS].[History_BatchSummary] DROP CONSTRAINT [FK_History_BatchSummary_DocTypes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary] DROP CONSTRAINT [FK_BatchSummary_DocTypes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_SFTPChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] DROP CONSTRAINT [FK_Deliverables_SFTPChannel]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_EmailChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] DROP CONSTRAINT [FK_Deliverables_EmailChannel]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Clients_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[Common].[Clients]'))
ALTER TABLE [Common].[Clients] DROP CONSTRAINT [FK_Clients_BusinessUnit]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatch_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatch]'))
ALTER TABLE [AM].[PrintBatch] DROP CONSTRAINT [FK_PrintBatch_Batch]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieceCustomInfo_MailPiece]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]'))
ALTER TABLE [AM].[MailPieceCustomInfo] DROP CONSTRAINT [FK_MailPieceCustomInfo_MailPiece]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPiece_PrintBatch]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPiece]'))
ALTER TABLE [AM].[MailPiece] DROP CONSTRAINT [FK_MailPiece_PrintBatch]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPiece_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPiece]'))
ALTER TABLE [AM].[MailPiece] DROP CONSTRAINT [FK_MailPiece_Batch]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypes_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypes]'))
ALTER TABLE [AM].[DocTypes] DROP CONSTRAINT [FK_DocTypes_Clients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_DocTypeCoverLetters_DocTypes]') AND parent_object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]'))
ALTER TABLE [AM].[DocTypeCoverLetters] DROP CONSTRAINT [FK_DocTypeCoverLetters_DocTypes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_BatchCustomInfo_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[BatchCustomInfo]'))
ALTER TABLE [AM].[BatchCustomInfo] DROP CONSTRAINT [FK_BatchCustomInfo_Batch]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_Batch_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[Batch]'))
ALTER TABLE [AM].[Batch] DROP CONSTRAINT [FK_Batch_Clients]
GO
/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_MRDFResponseData]'))
DROP VIEW [dbo].[view_MRDFResponseData]
GO
/****** Object:  Table [Profile].[Users]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Users]') AND type in (N'U'))
DROP TABLE [Profile].[Users]
GO
/****** Object:  Table [Profile].[UserRoles]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserRoles]') AND type in (N'U'))
DROP TABLE [Profile].[UserRoles]
GO
/****** Object:  Table [Profile].[UserLogins]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserLogins]') AND type in (N'U'))
DROP TABLE [Profile].[UserLogins]
GO
/****** Object:  Table [Profile].[UserClaims]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserClaims]') AND type in (N'U'))
DROP TABLE [Profile].[UserClaims]
GO
/****** Object:  Table [Profile].[Roles]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Roles]') AND type in (N'U'))
DROP TABLE [Profile].[Roles]
GO
/****** Object:  Table [PnS].[Schema_MRDFResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_MRDFResponse]') AND type in (N'U'))
DROP TABLE [PnS].[Schema_MRDFResponse]
GO
/****** Object:  Table [PnS].[Schema_MRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_MRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[Schema_MRDFRequest]
GO
/****** Object:  Table [PnS].[Schema_BatchSummary]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[Schema_BatchSummary]') AND type in (N'U'))
DROP TABLE [PnS].[Schema_BatchSummary]
GO
/****** Object:  Table [PnS].[ReprintMRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[ReprintMRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[ReprintMRDFRequest]
GO
/****** Object:  Table [PnS].[ReprintBatches]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[ReprintBatches]') AND type in (N'U'))
DROP TABLE [PnS].[ReprintBatches]
GO
/****** Object:  Table [PnS].[MRDFResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[MRDFResponse]') AND type in (N'U'))
DROP TABLE [PnS].[MRDFResponse]
GO
/****** Object:  Table [PnS].[MRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[MRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[MRDFRequest]
GO
/****** Object:  Table [PnS].[LogBatchTimeLine]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[LogBatchTimeLine]') AND type in (N'U'))
DROP TABLE [PnS].[LogBatchTimeLine]
GO
/****** Object:  Table [PnS].[History_MRDFResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[History_MRDFResponse]') AND type in (N'U'))
DROP TABLE [PnS].[History_MRDFResponse]
GO
/****** Object:  Table [PnS].[History_MRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[History_MRDFRequest]') AND type in (N'U'))
DROP TABLE [PnS].[History_MRDFRequest]
GO
/****** Object:  Table [PnS].[History_BatchSummary]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[History_BatchSummary]') AND type in (N'U'))
DROP TABLE [PnS].[History_BatchSummary]
GO
/****** Object:  Table [PnS].[BatchSummary]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[BatchSummary]') AND type in (N'U'))
DROP TABLE [PnS].[BatchSummary]
GO
/****** Object:  Table [Common].[SystemSettings]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SystemSettings]') AND type in (N'U'))
DROP TABLE [Common].[SystemSettings]
GO
/****** Object:  Table [Common].[SFTPChannel]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SFTPChannel]') AND type in (N'U'))
DROP TABLE [Common].[SFTPChannel]
GO
/****** Object:  Table [Common].[EmailChannel]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[EmailChannel]') AND type in (N'U'))
DROP TABLE [Common].[EmailChannel]
GO
/****** Object:  Table [Common].[Deliverables]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Deliverables]') AND type in (N'U'))
DROP TABLE [Common].[Deliverables]
GO
/****** Object:  Table [Common].[Clients]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Clients]') AND type in (N'U'))
DROP TABLE [Common].[Clients]
GO
/****** Object:  Table [Common].[BusinessUnit]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[BusinessUnit]') AND type in (N'U'))
DROP TABLE [Common].[BusinessUnit]
GO
/****** Object:  Table [AM].[ShippingAccounts]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[ShippingAccounts]') AND type in (N'U'))
DROP TABLE [AM].[ShippingAccounts]
GO
/****** Object:  Index [CIX_AM_Batch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[AM].[PrintBatch]') AND name = N'CIX_AM_Batch')
DROP INDEX [CIX_AM_Batch] ON [AM].[PrintBatch] WITH ( ONLINE = OFF )
GO
/****** Object:  Table [AM].[PrintBatch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PrintBatch]') AND type in (N'U'))
DROP TABLE [AM].[PrintBatch]
GO
/****** Object:  Table [AM].[PnSResponseDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PnSResponseDetails]') AND type in (N'U'))
DROP TABLE [AM].[PnSResponseDetails]
GO
/****** Object:  Table [AM].[PnSResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PnSResponse]') AND type in (N'U'))
DROP TABLE [AM].[PnSResponse]
GO
/****** Object:  Table [AM].[MailPieceCustomInfo]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]') AND type in (N'U'))
DROP TABLE [AM].[MailPieceCustomInfo]
GO
/****** Object:  Table [AM].[MailPiece]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPiece]') AND type in (N'U'))
DROP TABLE [AM].[MailPiece]
GO
/****** Object:  Table [AM].[LogBatchTimeLine]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[LogBatchTimeLine]') AND type in (N'U'))
DROP TABLE [AM].[LogBatchTimeLine]
GO
/****** Object:  Table [AM].[DocTypes]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[DocTypes]') AND type in (N'U'))
DROP TABLE [AM].[DocTypes]
GO
/****** Object:  Table [AM].[DocTypeCoverLetters]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[DocTypeCoverLetters]') AND type in (N'U'))
DROP TABLE [AM].[DocTypeCoverLetters]
GO
/****** Object:  Table [AM].[BatchCustomInfo]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[BatchCustomInfo]') AND type in (N'U'))
DROP TABLE [AM].[BatchCustomInfo]
GO
/****** Object:  Index [CIX_AM_Batch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[AM].[Batch]') AND name = N'CIX_AM_Batch')
DROP INDEX [CIX_AM_Batch] ON [AM].[Batch] WITH ( ONLINE = OFF )
GO
/****** Object:  Table [AM].[Batch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[Batch]') AND type in (N'U'))
DROP TABLE [AM].[Batch]
GO
/****** Object:  Schema [AM]    Script Date: 2/22/2018 3:20:05 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'AM')
EXEC sys.sp_executesql N'CREATE SCHEMA [AM]'

GO
/****** Object:  Schema [Common]    Script Date: 2/22/2018 3:20:05 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Common')
EXEC sys.sp_executesql N'CREATE SCHEMA [Common]'

GO
/****** Object:  Schema [PnS]    Script Date: 2/22/2018 3:20:05 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'PnS')
EXEC sys.sp_executesql N'CREATE SCHEMA [PnS]'

GO
/****** Object:  Schema [Profile]    Script Date: 2/22/2018 3:20:05 PM ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Profile')
EXEC sys.sp_executesql N'CREATE SCHEMA [Profile]'

GO
/****** Object:  Table [AM].[Batch]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[Batch]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[Batch](
	[Id] [uniqueidentifier] NOT NULL,
	[ClientId] [int] NULL,
	[BatchClustId] [int] IDENTITY(1000,1) NOT NULL,
	[DocumentOrPackageType] [varchar](512) NULL,
	[BatchName] [varchar](512) NULL,
	[PipelineStatus] [varchar](512) NULL,
	[ValidationStatus] [varchar](512) NULL,
	[ValidationErrors] [varchar](max) NULL,
	[BatchStatus] [varchar](512) NULL,
	[IsReceived]  AS (case when [ReceivedOn] IS NULL then (0) else (1) end),
	[ReceivedOn] [datetime] NULL,
	[IsDownloaded]  AS (case when [DownloadedOn] IS NULL then (0) else (1) end),
	[DownloadedOn] [datetime] NULL,
	[IsAcknowledged]  AS (case when [AcknowledgedOn] IS NULL then (0) else (1) end),
	[AcknowledgedOn] [datetime] NULL,
	[IsValidated]  AS (case when [ValidatedOn] IS NULL then (0) else (1) end),
	[ValidatedOn] [datetime] NULL,
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
	[BusinessFriendlyIdName] [varchar](512) NULL,
	[BusinessFriendlyIdValue] [varchar](512) NULL,
	[IsReturnInfoSpecified] [bit] NULL,
	[RetRcptPrefix] [varchar](512) NULL,
	[RetRcptFirstName] [varchar](512) NULL,
	[RetRcptMiddleName] [varchar](512) NULL,
	[RetRcptLastName] [varchar](512) NULL,
	[RetRcptSuffix] [varchar](512) NULL,
	[RetRcptPhone] [varchar](512) NULL,
	[RetRcptOrganization] [varchar](512) NULL,
	[RetRcptAddrOptionalLine1AboveStreet] [varchar](512) NULL,
	[RetRcptAddrOptionalLine2AboveStreet] [varchar](512) NULL,
	[RetRcptAddressStreet] [varchar](512) NULL,
	[RetRcptAddressAptOrSuiteOrOther] [varchar](512) NULL,
	[RetRcptAddrOptionalLine1BelowStreet] [varchar](512) NULL,
	[RetRcptAddrOptionalLine2BelowStreet] [varchar](512) NULL,
	[RetRcptAddressCity] [varchar](512) NULL,
	[RetRcptAddressState] [varchar](512) NULL,
	[RetRcptAddressZIP] [varchar](512) NULL,
	[BatchFilePath] [varchar](1024) NULL,
	[Priority] [varchar](50) NULL,
	[ChannelCode] [int] NULL,
	[TotalLoans] [int] NULL,
 CONSTRAINT [PK_AM_Batch] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Index [CIX_AM_Batch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[AM].[Batch]') AND name = N'CIX_AM_Batch')
CREATE UNIQUE CLUSTERED INDEX [CIX_AM_Batch] ON [AM].[Batch]
(
	[BatchClustId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [AM].[BatchCustomInfo]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[BatchCustomInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[BatchCustomInfo](
	[BatchId] [uniqueidentifier] NOT NULL,
	[Key] [varchar](512) NULL,
	[Value] [varchar](512) NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[DocTypeCoverLetters]    Script Date: 2/22/2018 3:20:05 PM ******/
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
	[CoverLetterTemplatePath] [varchar](512) NULL,
	[Condition] [varchar](512) NULL,
	[CarrierType] [varchar](512) NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[DocTypes]    Script Date: 2/22/2018 3:20:05 PM ******/
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
	[DocTypeCode] [varchar](29) NOT NULL,
	[Description] [varchar](255) NULL,
	[PrintSize] [varchar](50) NOT NULL,
	[ContentScalePercent] [float] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[ShippingCarrier] [varchar](512) NULL,
	[ShippingServiceType] [varchar](512) NULL,
	[ShippingAccount] [varchar](512) NULL,
	[IsReturnInfoSpecified] [bit] NULL,
	[RetRcptPrefix] [varchar](512) NULL,
	[RetRcptFirstName] [varchar](512) NULL,
	[RetRcptMiddleName] [varchar](512) NULL,
	[RetRcptLastName] [varchar](512) NULL,
	[RetRcptSuffix] [varchar](512) NULL,
	[RetRcptPhone] [varchar](512) NULL,
	[RetRcptOrganization] [varchar](512) NULL,
	[RetRcptAddrOptionalLine1AboveStreet] [varchar](512) NULL,
	[RetRcptAddrOptionalLine2AboveStreet] [varchar](512) NULL,
	[RetRcptAddressStreet] [varchar](512) NULL,
	[RetRcptAddressAptOrSuiteOrOther] [varchar](512) NULL,
	[RetRcptAddrOptionalLine1BelowStreet] [varchar](512) NULL,
	[RetRcptAddrOptionalLine2BelowStreet] [varchar](512) NULL,
	[RetRcptAddressCity] [varchar](512) NULL,
	[RetRcptAddressState] [varchar](512) NULL,
	[RetRcptAddressZIP] [varchar](512) NULL,
	[ClientEmails] [varchar](512) NULL,
	[ClientContactNo] [varchar](512) NULL,
	[SupportEmails] [varchar](512) NULL,
	[SupportContactNo] [varchar](512) NULL,
	[ClientId] [int] NOT NULL,
	[QCPercent] [int] NULL,
	[ModifiedOn] [datetime] NULL,
	[CreatedOn] [datetime] NULL,
	[ModifiedBy] [varchar](50) NULL,
	[CreatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_DocTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[LogBatchTimeLine]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[LogBatchTimeLine]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[LogBatchTimeLine](
	[BatchId] [uniqueidentifier] NOT NULL,
	[DownloadStart] [datetime] NULL,
	[DownloadEnd] [datetime] NULL,
	[AcknowledgeStart] [datetime] NULL,
	[AcknowledgeEnd] [datetime] NULL,
	[ValidationStart] [datetime] NULL,
	[ValidationEnd] [datetime] NULL,
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
/****** Object:  Table [AM].[MailPiece]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPiece]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[MailPiece](
	[Id] [uniqueidentifier] NOT NULL,
	[BatchClustId] [int] NULL,
	[PrintBatchClustId] [int] NULL,
	[BusinessFriendlyIdName] [varchar](512) NULL,
	[BusinessFriendlyIdValue] [varchar](512) NULL,
	[FileName] [nvarchar](512) NULL,
	[FilePattern] [nvarchar](512) NULL,
	[IsRcptInfoSpecified] [bit] NULL,
	[RcptPrefix] [nvarchar](50) NULL,
	[RcptFirstName] [nvarchar](50) NULL,
	[RcptMiddleName] [nvarchar](50) NULL,
	[RcptLastName] [nvarchar](50) NULL,
	[RcptSuffix] [nvarchar](50) NULL,
	[RcptPhone] [nvarchar](50) NULL,
	[RcptOrganization] [nvarchar](255) NULL,
	[CoRcptPrefix] [nvarchar](50) NULL,
	[CoRcptFirstName] [nvarchar](50) NULL,
	[CoRcptMiddleName] [nvarchar](50) NULL,
	[CoRcptLastName] [nvarchar](50) NULL,
	[CoRcptSuffix] [nvarchar](50) NULL,
	[CoRcptPhone] [nvarchar](50) NULL,
	[CoRcptOrganization] [nvarchar](255) NULL,
	[RcptAddrOptionalLine1AboveStreet] [varchar](512) NULL,
	[RcptAddrOptionalLine2AboveStreet] [varchar](512) NULL,
	[RcptAddressStreet] [varchar](512) NULL,
	[RcptAddressAptOrSuiteOrOther] [varchar](512) NULL,
	[RcptAddrOptionalLine1BelowStreet] [varchar](512) NULL,
	[RcptAddrOptionalLine2BelowStreet] [varchar](512) NULL,
	[RcptAddressCity] [varchar](512) NULL,
	[RcptAddressState] [varchar](512) NULL,
	[RcptAddressZIP] [varchar](512) NULL,
	[IsReturnInfoSpecified] [bit] NULL,
	[RetRcptPrefix] [varchar](512) NULL,
	[RetRcptFirstName] [varchar](512) NULL,
	[RetRcptMiddleName] [varchar](512) NULL,
	[RetRcptLastName] [varchar](512) NULL,
	[RetRcptSuffix] [varchar](512) NULL,
	[RetRcptPhone] [varchar](512) NULL,
	[RetRcptOrganization] [varchar](512) NULL,
	[RetRcptAddrOptionalLine1AboveStreet] [varchar](512) NULL,
	[RetRcptAddrOptionalLine2AboveStreet] [varchar](512) NULL,
	[RetRcptAddressStreet] [varchar](512) NULL,
	[RetRcptAddressAptOrSuiteOrOther] [varchar](512) NULL,
	[RetRcptAddrOptionalLine1BelowStreet] [varchar](512) NULL,
	[RetRcptAddrOptionalLine2BelowStreet] [varchar](512) NULL,
	[RetRcptAddressCity] [varchar](512) NULL,
	[RetRcptAddressState] [varchar](512) NULL,
	[RetRcptAddressZIP] [varchar](512) NULL,
	[PreppedFilePath] [nvarchar](1000) NULL,
	[TotalPages] [int] NOT NULL,
	[ErrorDesc] [nvarchar](max) NULL,
	[TrackingNo] [nvarchar](25) NULL,
	[IsPullBackRequested]  AS (case when [PullBackRequestedOn] IS NULL then (0) else (1) end),
	[PullBackRequestedOn] [datetime] NULL,
	[IsPulledBack]  AS (case when [PulledBackOn] IS NULL then (0) else (1) end),
	[PulledBackOn] [datetime] NULL,
	[IsShipped]  AS (case when [ShippedOn] IS NULL then (0) else (1) end),
	[ShippedOn] [datetime] NULL,
	[ShippingCost] [decimal](6, 2) NULL,
	[ShippingStatus] [varchar](255) NULL,
	[IsQCRequested] [bit] NULL,
	[IsQCPerformed] [bit] NULL,
	[DocumentOrPackageType] [varchar](512) NULL,
	[DownloadedFilePath] [varchar](512) NULL,
	[ErrorCode] [varchar](50) NULL,
 CONSTRAINT [PK_AM_MailPiece] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[MailPieceCustomInfo]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[MailPieceCustomInfo](
	[MailPieceId] [uniqueidentifier] NOT NULL,
	[Key] [varchar](512) NULL,
	[Value] [varchar](512) NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[PnSResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PnSResponse]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[PnSResponse](
	[PrintBatchId] [uniqueidentifier] NULL,
	[BatchName] [varchar](512) NULL,
	[AppCode] [varchar](512) NULL,
	[PCIBatchName] [varchar](255) NULL,
	[ReceivedOn] [datetime] NULL,
	[BatchStatus] [varchar](512) NULL,
	[Message] [varchar](512) NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[PnSResponseDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PnSResponseDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[PnSResponseDetails](
	[PrintBatchId] [uniqueidentifier] NULL,
	[PDFFileName] [varchar](512) NULL,
	[UniqueIdentifier] [uniqueidentifier] NULL,
	[MailPieceStatus] [varchar](255) NULL,
	[ErrId] [int] NULL,
	[ErrDesc] [varchar](max) NULL,
	[ShippedOn] [datetime] NULL,
	[PostageCost] [decimal](6, 2) NULL,
	[WeightMeasured] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [AM].[PrintBatch]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[PrintBatch]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[PrintBatch](
	[Id] [uniqueidentifier] NOT NULL,
	[PrintBatchClustId] [int] IDENTITY(1,1) NOT NULL,
	[BatchClustId] [int] NULL,
	[OnValidationErrors] [varchar](512) NULL,
	[ValidationStatus] [varchar](50) NULL,
	[ValidationErrors] [varchar](max) NULL,
	[BatchStatus] [varchar](512) NULL,
	[IsSubmittedToShipping]  AS (case when [SubmittedToShippingOn] IS NULL then (0) else (1) end),
	[SubmittedToShippingOn] [datetime] NULL,
	[IsShippingResponseReceived]  AS (case when [ShippingResponseReceivedOn] IS NULL then (0) else (1) end),
	[ShippingResponseReceivedOn] [datetime] NULL,
	[DocTypeCode] [varchar](255) NULL,
 CONSTRAINT [PK_AM_PrintBatch] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Index [CIX_AM_Batch]    Script Date: 2/22/2018 3:20:05 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[AM].[PrintBatch]') AND name = N'CIX_AM_Batch')
CREATE UNIQUE CLUSTERED INDEX [CIX_AM_Batch] ON [AM].[PrintBatch]
(
	[PrintBatchClustId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [AM].[ShippingAccounts]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AM].[ShippingAccounts]') AND type in (N'U'))
BEGIN
CREATE TABLE [AM].[ShippingAccounts](
	[AccountCode] [varchar](255) NULL,
	[WebAuthenticationKey] [nvarchar](50) NULL,
	[WebAuthenticationPassword] [nvarchar](50) NULL,
	[AccountNumber] [nvarchar](50) NULL,
	[MeterNumber] [nvarchar](50) NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[BusinessUnit]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [Common].[Clients]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Clients]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[Clients](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientName] [varchar](50) NOT NULL,
	[AppCode] [varchar](12) NOT NULL,
	[BusinessUnit] [int] NOT NULL,
	[IsClientActive] [bit] NOT NULL,
	[IsPnSActive] [bit] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[SFTProtocol] [varchar](255) NOT NULL,
	[SFTHost] [varchar](255) NOT NULL,
	[SFTUserName] [varchar](255) NOT NULL,
	[SFTPassword] [varchar](512) NOT NULL,
	[SFTADERequestDir] [varchar](512) NOT NULL,
	[SFTADERequestArchiveDir] [varchar](512) NOT NULL,
	[SFTADEResponseDir] [varchar](512) NOT NULL,
	[TimeZone] [varchar](255) NOT NULL,
 CONSTRAINT [PK__ClientApp] PRIMARY KEY CLUSTERED 
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
/****** Object:  Table [Common].[Deliverables]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[Deliverables]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[Deliverables](
	[Id] [int] NOT NULL,
	[ClientId] [int] NULL,
	[ReportName] [varchar](512) NULL,
	[Type] [varchar](50) NULL,
	[ReportType] [varchar](512) NULL,
	[ReportQuery] [varchar](512) NULL,
	[EmailChannelId] [int] NULL,
	[SFTPChannelId] [int] NULL,
	[ScheduleId] [int] NULL,
 CONSTRAINT [PK_Deliverables] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[EmailChannel]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[EmailChannel]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[EmailChannel](
	[Id] [int] NOT NULL,
	[EmailSubject] [varchar](1024) NULL,
	[EmailTo] [varchar](1024) NULL,
	[EmailCc] [varchar](1024) NULL,
 CONSTRAINT [PK_EmailChannel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[SFTPChannel]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SFTPChannel]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[SFTPChannel](
	[Id] [int] NOT NULL,
	[SFTPProtocol] [varchar](255) NULL,
	[SFTPHost] [varchar](255) NULL,
	[SFTPUserName] [varchar](255) NULL,
	[SFTPPassword] [varchar](512) NULL,
	[SFTPErrorReportPath] [varchar](1024) NULL,
	[SFTPDelivaryReportPath] [varchar](1024) NULL,
 CONSTRAINT [PK_SFTPChannel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Common].[SystemSettings]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Common].[SystemSettings]') AND type in (N'U'))
BEGIN
CREATE TABLE [Common].[SystemSettings](
	[Key] [varchar](255) NULL,
	[KeyDesc] [varchar](512) NULL,
	[Value] [varchar](512) NULL,
 CONSTRAINT [UQ_Key] UNIQUE NONCLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[BatchSummary]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[BatchSummary]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[BatchSummary](
	[PrintBatch] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [int] NULL,
	[JobName] [varchar](512) NULL,
	[PrintFileName] [varchar](255) NULL,
	[BatchIdentifier] [uniqueidentifier] NULL,
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
	[DocTypeId] [int] NULL,
 CONSTRAINT [PK_BatchSummary] PRIMARY KEY CLUSTERED 
(
	[PrintBatch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[History_BatchSummary]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[History_BatchSummary]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[History_BatchSummary](
	[PrintBatch] [int] NOT NULL,
	[ClientId] [int] NULL,
	[JobName] [varchar](512) NULL,
	[PrintFileName] [varchar](255) NULL,
	[BatchIdentifier] [uniqueidentifier] NULL,
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
	[DocTypeId] [int] NULL,
 CONSTRAINT [PK_History_BatchSummary] PRIMARY KEY CLUSTERED 
(
	[PrintBatch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[History_MRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[History_MRDFRequest]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[History_MRDFRequest](
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
	[UniqueIdentifier] [nvarchar](255) NULL,
	[PDFFileName] [varchar](255) NULL,
	[ErrDesc] [varchar](max) NULL,
	[MailPieceStatus] [varchar](255) NULL,
 CONSTRAINT [PK_HistoryMRDFRequest] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[History_MRDFResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PnS].[History_MRDFResponse]') AND type in (N'U'))
BEGIN
CREATE TABLE [PnS].[History_MRDFResponse](
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
 CONSTRAINT [PK_HistoryMRDFResponse] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [PnS].[LogBatchTimeLine]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [PnS].[MRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
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
	[UniqueIdentifier] [nvarchar](255) NULL,
	[PDFFileName] [varchar](255) NULL,
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
/****** Object:  Table [PnS].[MRDFResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [PnS].[ReprintBatches]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [PnS].[ReprintMRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [PnS].[Schema_BatchSummary]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [PnS].[Schema_MRDFRequest]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [PnS].[Schema_MRDFResponse]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  Table [Profile].[Roles]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[Roles]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[Roles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[UserClaims]    Script Date: 2/22/2018 3:20:05 PM ******/
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
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[UserLogins]    Script Date: 2/22/2018 3:20:05 PM ******/
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
PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[UserRoles]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profile].[UserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [Profile].[UserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [Profile].[Users]    Script Date: 2/22/2018 3:20:05 PM ******/
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
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[view_MRDFResponseData]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [dbo].[view_MRDFResponseData] AS
SELECT REQ.JobId, CASE WHEN RES.PostageMetered IS NULL THEN 0.00 ELSE RES.PostageMetered END AS PostageCost 
,CASE WHEN RES.DetailedErrorId IS NULL THEN ''0'' ELSE RES.DetailedErrorId END AS ErrId 
,RES.ProcessingTimeStamp AS ShippedOn 
,CASE WHEN RES.WeightMeasured IS NULL THEN 0.00 ELSE RES.WeightMeasured END AS WeightMeasured
,CASE WHEN RES.Disposition IS NULL THEN ''0'' ELSE RES.Disposition END AS Disposition 
,REQ.PieceId,REQ.[UniqueIdentifier]
,REQ.PDFFileName, REQ.ErrDesc, REQ.MailPieceStatus,RES.TotalSheets,REQ.DivertVS3 
FROM [PnS].MRDFRequest REQ
LEFT JOIN [PnS].MRDFResponse RES
ON  REQ.JobId =RES.JobId AND REQ.PieceId =RES.PieceId

' 
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_Batch_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[Batch]'))
ALTER TABLE [AM].[Batch]  WITH CHECK ADD  CONSTRAINT [FK_Batch_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_Batch_Clients]') AND parent_object_id = OBJECT_ID(N'[AM].[Batch]'))
ALTER TABLE [AM].[Batch] CHECK CONSTRAINT [FK_Batch_Clients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_BatchCustomInfo_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[BatchCustomInfo]'))
ALTER TABLE [AM].[BatchCustomInfo]  WITH CHECK ADD  CONSTRAINT [FK_BatchCustomInfo_Batch] FOREIGN KEY([BatchId])
REFERENCES [AM].[Batch] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_BatchCustomInfo_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[BatchCustomInfo]'))
ALTER TABLE [AM].[BatchCustomInfo] CHECK CONSTRAINT [FK_BatchCustomInfo_Batch]
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
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPiece_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPiece]'))
ALTER TABLE [AM].[MailPiece]  WITH CHECK ADD  CONSTRAINT [FK_MailPiece_Batch] FOREIGN KEY([BatchClustId])
REFERENCES [AM].[Batch] ([BatchClustId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPiece_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPiece]'))
ALTER TABLE [AM].[MailPiece] CHECK CONSTRAINT [FK_MailPiece_Batch]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPiece_PrintBatch]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPiece]'))
ALTER TABLE [AM].[MailPiece]  WITH CHECK ADD  CONSTRAINT [FK_MailPiece_PrintBatch] FOREIGN KEY([PrintBatchClustId])
REFERENCES [AM].[PrintBatch] ([PrintBatchClustId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPiece_PrintBatch]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPiece]'))
ALTER TABLE [AM].[MailPiece] CHECK CONSTRAINT [FK_MailPiece_PrintBatch]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieceCustomInfo_MailPiece]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]'))
ALTER TABLE [AM].[MailPieceCustomInfo]  WITH CHECK ADD  CONSTRAINT [FK_MailPieceCustomInfo_MailPiece] FOREIGN KEY([MailPieceId])
REFERENCES [AM].[MailPiece] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_MailPieceCustomInfo_MailPiece]') AND parent_object_id = OBJECT_ID(N'[AM].[MailPieceCustomInfo]'))
ALTER TABLE [AM].[MailPieceCustomInfo] CHECK CONSTRAINT [FK_MailPieceCustomInfo_MailPiece]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatch_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatch]'))
ALTER TABLE [AM].[PrintBatch]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatch_Batch] FOREIGN KEY([BatchClustId])
REFERENCES [AM].[Batch] ([BatchClustId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[AM].[FK_PrintBatch_Batch]') AND parent_object_id = OBJECT_ID(N'[AM].[PrintBatch]'))
ALTER TABLE [AM].[PrintBatch] CHECK CONSTRAINT [FK_PrintBatch_Batch]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Clients_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[Common].[Clients]'))
ALTER TABLE [Common].[Clients]  WITH CHECK ADD  CONSTRAINT [FK_Clients_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [Common].[BusinessUnit] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Clients_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[Common].[Clients]'))
ALTER TABLE [Common].[Clients] CHECK CONSTRAINT [FK_Clients_BusinessUnit]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_EmailChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables]  WITH CHECK ADD  CONSTRAINT [FK_Deliverables_EmailChannel] FOREIGN KEY([EmailChannelId])
REFERENCES [Common].[EmailChannel] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_EmailChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] CHECK CONSTRAINT [FK_Deliverables_EmailChannel]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_SFTPChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables]  WITH CHECK ADD  CONSTRAINT [FK_Deliverables_SFTPChannel] FOREIGN KEY([SFTPChannelId])
REFERENCES [Common].[SFTPChannel] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Common].[FK_Deliverables_SFTPChannel]') AND parent_object_id = OBJECT_ID(N'[Common].[Deliverables]'))
ALTER TABLE [Common].[Deliverables] CHECK CONSTRAINT [FK_Deliverables_SFTPChannel]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary]  WITH CHECK ADD  CONSTRAINT [FK_BatchSummary_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[BatchSummary]'))
ALTER TABLE [PnS].[BatchSummary] CHECK CONSTRAINT [FK_BatchSummary_DocTypes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_History_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_BatchSummary]'))
ALTER TABLE [PnS].[History_BatchSummary]  WITH CHECK ADD  CONSTRAINT [FK_History_BatchSummary_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_History_BatchSummary_DocTypes]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_BatchSummary]'))
ALTER TABLE [PnS].[History_BatchSummary] CHECK CONSTRAINT [FK_History_BatchSummary_DocTypes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_HistoryMRDFRequest_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_MRDFRequest]'))
ALTER TABLE [PnS].[History_MRDFRequest]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMRDFRequest_BatchSummary] FOREIGN KEY([JobId])
REFERENCES [PnS].[BatchSummary] ([PrintBatch])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_HistoryMRDFRequest_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_MRDFRequest]'))
ALTER TABLE [PnS].[History_MRDFRequest] CHECK CONSTRAINT [FK_HistoryMRDFRequest_BatchSummary]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_HistoryMRDFResponse_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_MRDFResponse]'))
ALTER TABLE [PnS].[History_MRDFResponse]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMRDFResponse_BatchSummary] FOREIGN KEY([JobId])
REFERENCES [PnS].[BatchSummary] ([PrintBatch])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[PnS].[FK_HistoryMRDFResponse_BatchSummary]') AND parent_object_id = OBJECT_ID(N'[PnS].[History_MRDFResponse]'))
ALTER TABLE [PnS].[History_MRDFResponse] CHECK CONSTRAINT [FK_HistoryMRDFResponse_BatchSummary]
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
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserClaims_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserClaims]'))
ALTER TABLE [Profile].[UserClaims]  WITH CHECK ADD  CONSTRAINT [FK_UserClaims_Users] FOREIGN KEY([UserId])
REFERENCES [Profile].[Users] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Profile].[FK_UserClaims_Users]') AND parent_object_id = OBJECT_ID(N'[Profile].[UserClaims]'))
ALTER TABLE [Profile].[UserClaims] CHECK CONSTRAINT [FK_UserClaims_Users]
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
/****** Object:  StoredProcedure [dbo].[usp_ActivateClientApp]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ActivateClientApp]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_ActivateClientApp] AS' 
END
GO
-- =============================================
-- Author:		Akshar Vala	
-- Create date: 16 Aug, 2017
-- Description: Activate Client App
-- =============================================
ALTER PROCEDURE [dbo].[usp_ActivateClientApp] 
	-- Add the parameters for the stored procedure here
     @Id int
	 ,@ModifiedOn datetime	
     ,@ModifiedBy nvarchar(255)
AS
BEGIN
	
   UPDATE [Common].[ClientApps]
		SET 
			
			 [IsClientActive] = 1
			,[ModifiedOn] = @ModifiedOn
			,[ModifiedBy] = @ModifiedBy

		WHERE Id = @Id
END




GO
/****** Object:  StoredProcedure [dbo].[usp_ActivateInactivateDocTypes]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ActivateInactivateDocTypes]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_ActivateInactivateDocTypes] AS' 
END
GO
-- =============================================
-- Author:		Akshar 
-- Create date: 14 Aug 2017
-- Description:	Delete DocTypes by id
-- =============================================
ALTER PROCEDURE [dbo].[usp_ActivateInactivateDocTypes]
	-- Add the parameters for the stored procedure here
	@Id int
	,@ModifiedOn datetime
AS
BEGIN
	DECLARE @IsActive bit
	SET @IsActive = (SELECT IsActive FROM [AM].[DocTypes] WHERE Id = @Id)
	
	IF(@IsActive = 1)
	BEGIN
		UPDATE [AM].[DocTypes]
			SET [IsActive] = 0,
				[ModifiedOn] = @ModifiedOn
				WHERE Id = @Id
	END
	IF(@IsActive = 0)
	BEGIN
		UPDATE [AM].[DocTypes]
			SET [IsActive] = 1,
				[ModifiedOn] = @ModifiedOn
				WHERE Id = @Id
	END
END




GO
/****** Object:  StoredProcedure [dbo].[usp_ActivateOrInActivateBusinessUnit]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ActivateOrInActivateBusinessUnit]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_ActivateOrInActivateBusinessUnit] AS' 
END
GO
-- =============================================
-- Author:		Akshar 
-- Create date: 31 Aug 2017
-- Description:	Delete BusinessUnit by id
-- =============================================
ALTER PROCEDURE [dbo].[usp_ActivateOrInActivateBusinessUnit]
	-- Add the parameters for the stored procedure here
	@Id int
AS
BEGIN
	DECLARE @IsActive bit
	SET @IsActive = (SELECT IsActive FROM [Common].[BusinessUnit] WHERE Id = @Id)
	
	IF(@IsActive = 1)
	BEGIN
		UPDATE [Common].[BusinessUnit]
				SET [IsActive] = 0 WHERE Id = @Id
	END
	IF(@IsActive = 0)
	BEGIN
		UPDATE [Common].[BusinessUnit]
				SET [IsActive] = 1 WHERE Id = @Id
	END
END




GO
/****** Object:  StoredProcedure [dbo].[usp_BatchStatusChart]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_BatchStatusChart]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_BatchStatusChart] AS' 
END
GO
ALTER PROCEDURE [dbo].[usp_BatchStatusChart]
 
	@appId int,
	@receivedDate date
AS
BEGIN
	IF(@appId = 0)
	BEGIN
	SELECT B.PrintBatch,l.[BatchId],b.[ClientId],b.[JobName],b.[PipelineStatus],b.[ReceivedOn],l.[DownloadStart],l.[DownloadEnd],l.[PCIBatchPrepStart]
				,l.[PCIBatchPrepEnd],l.[SubmitToPCIStart],l.[SubmitToPCIEnd],l.[MRDFDownloadStart],l.[MRDFDownloadEnd]
				,l.[ReportPrepStart],l.[ReportPrepEnd],l.[ReportBackStart],l.[ReportBackEnd],b.IsReceived,b.IsDownloaded 
		FROM [PnS].[BatchSummary] b 
		LEFT JOIN [PnS].[LogBatchTimeLine] l  ON b.PrintBatch = l.BatchId 
		 Where  b.IsSuspended=0 and CAST( b.ReceivedOn as Date) = CAST(@receivedDate as Date)
		 	--b.PrintBatch=l.BatchId
		 

		 SELECT l.[BatchId],b.[ClientId],b.[JobName],b.[PipelineStatus],b.[SuspendedOn],b.[CancelledOn],l.[DownloadStart]
			,l.[DownloadEnd],l.[PCIBatchPrepStart],l.[PCIBatchPrepEnd],l.[SubmitToPCIStart],l.[SubmitToPCIEnd]
			,l.[MRDFDownloadStart],l.[MRDFDownloadEnd],l.[ReportPrepStart],l.[ReportPrepEnd],l.[ReportBackStart]
			,l.[ReportBackEnd],b.IsReceived,b.IsDownloaded 
		 FROM [PnS].[LogBatchTimeLine] l ,[PnS].[BatchSummary] b
		 Where b.PrintBatch=l.BatchId and (b.IsSuspended=1 or b.IsCancelled=1) 
		-- and  CAST(b.ReceivedOn as Date) = CAST(GETDATE() as Date)

	END
	ELSE
	BEGIN
		SELECT B.PrintBatch,l.[BatchId],b.[ClientId],b.[JobName],b.[PipelineStatus],b.[ReceivedOn],l.[DownloadStart],l.[DownloadEnd],l.					[PCIBatchPrepStart],l.[PCIBatchPrepEnd],l.[SubmitToPCIStart],l.[SubmitToPCIEnd],l.[MRDFDownloadStart],
				l.[MRDFDownloadEnd],l.[ReportPrepStart],l.[ReportPrepEnd],l.[ReportBackStart],
				l.[ReportBackEnd],b.IsReceived,b.IsDownloaded 
		FROM PnS.BatchSummary b 
		LEFT JOIN [Pns].[LogBatchTimeLine] l  
		ON b.PrintBatch = l.BatchId 
		 WHERE b.IsSuspended=0 AND CAST( b.ReceivedOn as Date) = CAST(@receivedDate as Date) AND b.ClientId=@appId
		 
		 SELECT l.[BatchId],b.[ClientId],b.[JobName],b.[PipelineStatus],b.[SuspendedOn],b.[CancelledOn],l.[DownloadStart]
			,l.[DownloadEnd],l.[PCIBatchPrepStart],l.[PCIBatchPrepEnd],l.[SubmitToPCIStart],l.[SubmitToPCIEnd]
			,l.[MRDFDownloadStart],l.[MRDFDownloadEnd],l.[ReportPrepStart],l.[ReportPrepEnd],l.[ReportBackStart]
			,l.[ReportBackEnd],b.IsReceived,b.IsDownloaded 
		 FROM [PnS].[LogBatchTimeLine] l ,[PnS].[BatchSummary] b
		 Where b.PrintBatch=l.BatchId and (b.IsSuspended=1 or b.IsCancelled=1) 
		-- and  CAST(b.ReceivedOn as Date) = CAST(GETDATE() as Date) and b.AppId=@appId
	END
END




GO
/****** Object:  StoredProcedure [dbo].[usp_CheckBatchDataForReprint]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_CheckBatchDataForReprint]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_CheckBatchDataForReprint] AS' 
END
GO
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 31/01/2018
-- Description:	Check and return BatchId if batch is suspended due to Reprint code
-- =============================================
ALTER PROCEDURE [dbo].[usp_CheckBatchDataForReprint] 
 @batchId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @isBatchSuspended INT,@reprintCount INT
	SET @isBatchSuspended=(SELECT COUNT(*) FROM Pns.BatchSummary WHERE PrintBatch=@batchId AND IsSuspended=1 AND IsSubmitted=1)
	
	IF(@isBatchSuspended=1)
	BEGIN
		SET @reprintCount= (SELECT COUNT(*) FROM Pns.MRDFResponse WHERE JobId=@batchId AND RTRIM(LTRIM(Disposition))='7')
		
		IF(@reprintCount>0)
		BEGIN
			SELECT @reprintCount AS RETURNVAL
		END
		ELSE
		BEGIN
			SELECT 0 AS RETURNVAL
		END
	END
	ELSE
	BEGIN
		SELECT 0 AS RETURNVAL
	END
	
END




GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteMRDFRequestByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_DeleteMRDFResponseByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_DeleteMRDFResponseByJobId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_DeleteMRDFResponseByJobId] AS' 
END
GO
ALTER PROCEDURE [dbo].[usp_DeleteMRDFResponseByJobId]
	@jobId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DELETE FROM PnS.MRDFResponse WHERE JobId=@jobId
END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 2/22/2018 3:20:05 PM ******/
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
	C.Id, C.ClientName, C.AppCode, C.BusinessUnit, C.IsClientActive, C.IsPnSActive, C.ModifiedOn, C.ModifiedBy
	, C.CreatedOn, C.CreatedBy, C.SFTProtocol, C.SFTHost, C.SFTUserName, C.SFTPassword, C.SFTADERequestDir
	, C.SFTADERequestArchiveDir
	, C.SFTADEResponseDir, C.TimeZone
	,B.Id As BUId
	,B.CustCode
	,B.[BusinessUnit]
  FROM [Common].[Clients] C INNER JOIN [Common].[BusinessUnit] B ON B.Id = C.BusinessUnit
END


/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 11/16/2017 17:20:19 ******/
SET ANSI_NULLS ON


GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummaryById]    Script Date: 2/22/2018 3:20:05 PM ******/
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
 SuspendedOn, IsCancelled, CancelledOn, JulianDate, Images, DocTypeId
  FROM [PnS].[BatchSummary] WHERE PrintBatch = @batchId
END

/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 11/16/2017 17:20:05 ******/
SET ANSI_NULLS ON


GO
/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummarySchema]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_GetBusinessUnitById]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBusinessUnitById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetBusinessUnitById] AS' 
END
GO
-- =============================================
-- Author:		Akshar Vala
-- Create date: 31 Aug 2017
-- Description:	To get Businessunit by id
-- Test : [sp_GetBusinessUnitById] 1
-- =============================================
ALTER PROCEDURE [dbo].[usp_GetBusinessUnitById] 
	-- Add the parameters for the stored procedure here
	@Id int
AS
BEGIN
	SELECT [Id]
		  ,[BusinessUnit]
		  ,[CustCode]
		  ,[IsActive]
	FROM [Common].[BusinessUnit] WHERE Id = @Id
END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetBusinessUnits]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBusinessUnits]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetBusinessUnits] AS' 
END
GO
-- =============================================
-- Author:		Akshar Vala
-- Create date: 30 Aug 2017
-- Description:	To get all Businessunit names
-- =============================================
ALTER PROCEDURE [dbo].[usp_GetBusinessUnits] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT [Id]
		  ,[BusinessUnit]
		  ,[CustCode]
		  ,[IsActive]
	FROM [Common].[BusinessUnit]
END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 2/22/2018 3:20:05 PM ******/
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
      ,[ClientName]
	  ,SFTProtocol
	,SFTHost, 
	SFTUserName, 
	SFTPassword, 
	SFTADERequestDir, 
	SFTADERequestArchiveDir, 
	SFTADEResponseDir, 
	TimeZone
      ,[IsClientActive]
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
/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 2/22/2018 3:20:05 PM ******/
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
 
 SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails],D.[Description] as DocTypeDesc,D.DocTypeCode,D.IsActive,D.ModifiedOn
 ,D.CreatedOn,D.Id AS DocTypeId
 FROM AM.DocTypes D
 INNER JOIN Common.Clients C ON C.Id=D.ClientId
 WHERE C.Id=@clientId
 
END


/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 11/16/2017 17:20:56 ******/
SET ANSI_NULLS ON


GO
/****** Object:  StoredProcedure [dbo].[usp_GetDocTypesByDoctypeId]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetDocTypesByDoctypeId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetDocTypesByDoctypeId] AS' 
END
GO
ALTER PROCEDURE [dbo].[usp_GetDocTypesByDoctypeId]
	-- Add the parameters for the stored procedure here
	@DocTypeId int
AS
BEGIN
	
	SELECT [Id]
		  ,[DocTypeCode]
		  ,[Description]
		  ,[IsActive]
  FROM [AM].[DocTypes] WHERE Id = @DocTypeId

END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetFinalMRDFResponseDataByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
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
 , PieceId, [UniqueIdentifier], PDFFileName, ErrDesc, MailPieceStatus,TotalSheets,DivertVS3 
 FROM view_MRDFResponseData V WHERE V.JobId=@jobId
END


GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
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
	IF((@mailPieceStatus IS NULL) OR (@mailPieceStatus = 'ALL'))
	BEGIN
		SET @mailPieceStatus=''		
	END
	
	SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
	, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
	, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
	, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
	, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
	, Expansion, EndofRecord, [UniqueIdentifier], PDFFileName, ErrDesc, MailPieceStatus 
	FROM [PnS].[MRDFRequest] WHERE JobId = @jobId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus
END


GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataForReprintByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetMRDFRequestDataForReprintByJobId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataForReprintByJobId] AS' 
END
GO
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 31/01/2018
-- Description:	return MRDF request data which is only have disposition code 7 of MRDFResponse
-- =============================================
ALTER PROCEDURE [dbo].[usp_GetMRDFRequestDataForReprintByJobId] 
 @batchId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT ROW_NUMBER() OVER (Order by PieceId ) AS PieceId,JobId,[Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
	, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
	, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
	, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
	, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
	, CONVERT(VARCHAR(4),PieceId) AS Expansion, EndofRecord, [UniqueIdentifier], PDFFileName, ErrDesc, MailPieceStatus 
	FROM [PnS].[MRDFRequest] WHERE JobId = @batchId AND ISNULL(MailPieceStatus,'')='InProgress'
	AND PieceId IN (SELECT PieceId FROM PnS.MRDFResponse WHERE JobId=@batchId AND RTRIM(LTRIM(Disposition))='7')
	
END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestSchema]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByJobId]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseSchema]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_GetNextBatchByJobName]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetNextBatchByJobName]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetNextBatchByJobName] AS' 
END
GO
-- =============================================
-- Author:		Akshar vala
-- Create date: 10 AUg 2017
-- Description:	To Get Next Batch to Process by JOB Name
-- Test : usp_GetNextBatchByJobName 'PnSJobMonitor',''
-- =============================================
ALTER PROCEDURE [dbo].[usp_GetNextBatchByJobName] 
	-- Add the parameters for the stored procedure here
	@jobName varchar(50),
	@operation varchar(50)
AS
BEGIN
	
	IF (@jobName = 'PnSJobMonitor' AND @operation = 'ReportBack')
	BEGIN
		SELECT TOP 1 PrintBatch FROM PnS.BatchSummary WHERE IsReportPrepped = 1 AND IsReportedBack = 0
	END
    ELSE IF(@jobName = 'PnSCoreService')
    BEGIN
		IF(@operation = 'Prep')
		BEGIN
			SELECT TOP 1 PrintBatch AS BatchId FROM PnS.BatchSummary WHERE IsDownloaded = 1 AND IsPrepped = 0 
		END
		ELSE IF(@operation = 'ProcessMRDF')
		BEGIN
			SELECT TOP 1 PrintBatch AS BatchId FROM PnS.BatchSummary WHERE IsMRDFReceived = 1 AND IsReportPrepped = 0
		END
		ELSE
		BEGIN
			SELECT NULL AS BatchId
		END
    END
	ELSE IF(@jobName = 'PCIJobMonitor' AND @operation = 'Submit')
    BEGIN
		SELECT TOP 1 PrintBatch AS BatchId FROM PnS.BatchSummary WHERE IsPrepped = 1 AND IsSubmitted = 0 
    END
	ELSE
    BEGIN
       SELECT NULL AS BatchId
    END

END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetNextBatchIdByJobName]    Script Date: 2/22/2018 3:20:05 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_GetPendingBatches]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetPendingBatches]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetPendingBatches] AS' 
END
GO
ALTER  PROCEDURE [dbo].[usp_GetPendingBatches]
	-- Add the parameters for the stored procedure here
	@currentDate datetime
AS
BEGIN
	SELECT l.[BatchId],b.[ClientId],b.[JobName],b.[IsReceived],b.[IsDownloaded],b.[PipelineStatus],l.[DownloadStart],l.[DownloadEnd],l.[PCIBatchPrepStart],b.[PrintBatch],b.[SuspendedOn],b.[IsCancelled]
				,l.[PCIBatchPrepEnd],l.[SubmitToPCIStart],l.[SubmitToPCIEnd],l.[MRDFDownloadStart],l.[MRDFDownloadEnd]
				,l.[ReportPrepStart],l.[ReportPrepEnd],l.[ReportBackStart],l.[ReportBackEnd] 
		FROM [PnS].[LogBatchTimeLine] l ,[PnS].[BatchSummary] b
		WHERE (CAST(b.ReceivedOn AS Date) < CAST(@currentDate AS Date)) AND b.IsCancelled=1 AND l.BatchId=b.PrintBatch
		-- IN (SELECT MAX				        ([PrintBatch]) AS [PrintBatch] FROM [dbo].[BatchSummary] GROUP BY [JobName])
	
END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch] AS' 
END
GO
ALTER PROCEDURE [dbo].[usp_GetStartDateOfCanceledOrSuspendedBatch] 
	@printBatch int,
	@pipelineStatus varchar(512)
AS
BEGIN
	SELECT 
		CASE 
			WHEN l.DownloadStart IS NULL and l.DownloadEnd IS NULL THEN b.ReceivedOn 
			WHEN l.PCIBatchPrepStart IS NULL and l.PCIBatchPrepEnd IS NULL THEN l.DownloadEnd  
			WHEN l.SubmitToPCIStart IS NULL and l.SubmitToPCIEnd IS NULL THEN l.PCIBatchPrepEnd
			WHEN l.MRDFDownloadStart IS NULL and l.MRDFDownloadEnd IS NULL THEN l.SubmitToPCIEnd
			WHEN l.ReportPrepStart IS NULL and l.ReportPrepEnd IS NULL THEN l.MRDFDownloadEnd
			WHEN l.ReportBackStart IS NULL and l.ReportBackEnd IS NULL THEN l.ReportPrepEnd 
		ELSE
			NULL
		END AS StartDate 
   FROM [PnS].[BatchSummary] b,PnS.LogBatchTimeLine l WHERE l.BatchId=@printBatch AND b.PrintBatch=@printBatch AND PipelineStatus=@pipelineStatus
END




GO
/****** Object:  StoredProcedure [dbo].[usp_GetSubmittedBatches]    Script Date: 2/22/2018 3:20:05 PM ******/
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
WHERE PipelineStatus='Submitted'
UNION
SELECT RB.ParentPrintBatch as PrintBatch,RB.DocTypeCode,C.AppCode,BU.CustCode,RB.PrintBatch as BatchCode 
FROM [PnS].ReprintBatches RB
INNER JOIN [Common].Clients C ON C.Id=RB.AppId
INNER JOIN [Common].BusinessUnit BU ON BU.Id=C.BusinessUnit
WHERE PipelineStatus='Submitted'

END

GO
/****** Object:  StoredProcedure [dbo].[usp_GetTodaysSubmittedBatchesCount]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetTodaysSubmittedBatchesCount]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_GetTodaysSubmittedBatchesCount] AS' 
END
GO
-- =============================================
-- Author:		Akshar Vala
-- Create date: 26 Oct 17
-- Description:	Get client wise count of today's submitted batches 
-- =============================================
ALTER PROCEDURE [dbo].[usp_GetTodaysSubmittedBatchesCount]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT C.AppCode AS Client,Count(B.ClientId) AS SubmittedBatchesCount 
	FROM PnS.BatchSummary B 
		 INNER JOIN 
			Common.Clients C
				ON B.ClientId = C.Id 
	WHERE CAST(B.SubmittedOn AS DATE) = CAST(GETDATE() AS DATE) GROUP BY C.AppCode

	SELECT COUNT(ClientId) AS TotalSubmittedBatches FROM PnS.BatchSummary 
	WHERE CAST(SubmittedOn AS DATE) = CAST(GETDATE() AS DATE)

END




GO
/****** Object:  StoredProcedure [dbo].[usp_InsertBusinessUnitDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_InsertBusinessUnitDetails]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_InsertBusinessUnitDetails] AS' 
END
GO
-- =============================================
-- Author:		Akshar
-- Create date: 31 Aug 2017
-- Description:	Inserting New BusinessUnit
-- =============================================
ALTER PROCEDURE [dbo].[usp_InsertBusinessUnitDetails]
	-- Add the parameters for the stored procedure here
	@BusinessUnit varchar(10),
	@CustCode varchar(20)
AS
BEGIN
	INSERT INTO [Common].[BusinessUnit]
           ([BusinessUnit]
		   ,[CustCode]
           ,[IsActive])
     VALUES
           (@BusinessUnit
		   ,@CustCode
           ,1)
END




GO
/****** Object:  StoredProcedure [dbo].[usp_InsertDocTypeDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_InsertDocTypeDetails]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_InsertDocTypeDetails] AS' 
END
GO
-- =============================================
-- Author:  Akshar
-- Create date: 14 Nov 2017
-- Description: To Insert Doctype Details
-- =============================================
ALTER PROCEDURE [dbo].[usp_InsertDocTypeDetails]
 -- Add the parameters for the stored procedure here
 @DocTypeCode varchar(29),
 @Description varchar(255),
 @CheckStockName nvarchar(50),
 @ModifiedOn datetime,
 @CreatedOn datetime,
 @ClientId int 
AS
BEGIN
 INSERT INTO [AM].[DocTypes]
           ( [DocTypeCode]
            ,[Description]
			,[ModifiedOn]
			,[CreatedOn]
			,[IsActive]
			,[ClientId]
     )
     VALUES
           ( @DocTypeCode
            ,@Description
			,@ModifiedOn
			,@CreatedOn
			,1
			,@ClientId  
		  
           )
	SELECT SCOPE_IDENTITY();
END




GO
/****** Object:  StoredProcedure [dbo].[usp_LogBatchTimeLine]    Script Date: 2/22/2018 3:20:05 PM ******/
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_MoveBatchToHistoryTables]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_MoveBatchToHistoryTables]  AS' 
END
GO
ALTER PROCEDURE [dbo].[usp_MoveBatchToHistoryTables] 
	-- Add the parameters for the stored procedure here
	@batchId int
AS

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   
    DECLARE @ErrorCode  int  
    DECLARE @ErrorStep  varchar(200)
     SET @ErrorCode = @@ERROR

BEGIN TRY

    BEGIN TRAN
        
		 -- Copy from dbo.BatchSummary to history.BatchSummary
		 
		 SET @ErrorStep = 'Error in Copying from the BatchSummary';
		
		 INSERT INTO [PnS].[History_BatchSummary] 
		 (PrintBatch, ClientId, JobName,PrintFileName, PipelineStatus, ValidationStatus, OnValidationErrors,ValidationErrors, BatchStatus,  ReceivedOn, 
		  DownloadedOn, PreppedOn, SubmittedOn,  MRDFReceivedOn,  ReportPreppedOn, 
		  ReportedBackOn,  SuspendedOn,  CancelledOn, JulianDate, Images,DocTypeId)
		 Select PrintBatch, ClientId, JobName,PrintFileName,  PipelineStatus, ValidationStatus, OnValidationErrors, ValidationErrors, BatchStatus,  ReceivedOn,
		  DownloadedOn,  PreppedOn,  SubmittedOn,  MRDFReceivedOn,  ReportPreppedOn, 
		  ReportedBackOn,  SuspendedOn,  CancelledOn, JulianDate, Images,DocTypeId
		 FROM [PnS].[BatchSummary]
		 WHERE PrintBatch =@batchId

		  
		-- Copy from dbo.MRDFRequest to history.MRDFRequest

        SET @ErrorStep = 'Error in Copying from the MRDFRequest';

        INSERT INTO [PnS].[History_MRDFRequest]
		(JobId,PieceId,[Date],TotalSheets,InputWeight,Feeder3_Sel,Feeder4_Sel,Feeder5_Sel,LowCap_Sel,NoSeal_Sel,Feeder2_Sel,ZipMark_Sel,Address1,Address2,
		Zip5,Zip4,Zip2,DivertVS2,DivertVS3,DivertVS4,ReturnLogoBitMap,ReturnLine1,ReturnLine2,ReturnLine3,ReturnLine4,ReturnLine5,ReturnLine6,GroupNumber,SocialSecNumber,CheckAmount,PassThru,
		Expansion,EndofRecord,UniqueIdentifier,PDFFileName,ErrDesc,MailPieceStatus)
		SELECT JobId,PieceId,[Date],TotalSheets,InputWeight,Feeder3_Sel,Feeder4_Sel,Feeder5_Sel,LowCap_Sel,NoSeal_Sel,Feeder2_Sel,ZipMark_Sel,Address1,Address2,
		Zip5,Zip4,Zip2,DivertVS2,DivertVS3,DivertVS4,ReturnLogoBitMap,ReturnLine1,ReturnLine2,ReturnLine3,ReturnLine4,ReturnLine5,ReturnLine6,GroupNumber,SocialSecNumber,CheckAmount,PassThru,
		Expansion,EndofRecord,UniqueIdentifier,PDFFileName,ErrDesc,MailPieceStatus
		FROM PnS.MRDFRequest
		WHERE JobId=@batchId


         -- Copy from dbo.MRDFResponse to history.MRDFResponse
         
		SET @ErrorStep = 'Error in Copying from the MRDFResponse';
		
        INSERT INTO [PnS].[History_MRDFResponse]
		(JobId,PieceId,[Date],Feeder2_Fed,OperatorId,MachineId,ProcessingTimeStamp,DetailedErrorId,Disposition,Input_Fed,Feeder3_Fed,Feeder4_Fed,Feeder5_Fed,ComputedWeight,MeterDate,StatusSource,
		PostageMetered,WeightMeasured,ExitLocation,PDFFileName,TotalSheets)
		SELECT JobId,PieceId,[Date],Feeder2_Fed,OperatorId,MachineId,ProcessingTimeStamp,DetailedErrorId,Disposition,Input_Fed,Feeder3_Fed,Feeder4_Fed,Feeder5_Fed,ComputedWeight,MeterDate,StatusSource,
		PostageMetered,WeightMeasured,ExitLocation,PDFFileName,TotalSheets
		FROM PnS.MRDFResponse
		WHERE JobId=@batchId
		
         -- delete from dbo.MRDFRequest

        SET @ErrorStep = 'Error in deleting the items from the MRDFRequest'

        DELETE
        FROM PnS.MRDFRequest
        WHERE JobId = @batchId
        
        -- delete from dbo.MRDFResponse 

        SET @ErrorStep = 'Error in deleting the items from the MRDFResponse'

        DELETE
        FROM PnS.MRDFResponse
        WHERE JobId = @batchId
        
        -- delete from dbo.BatchSummary 

        SET @ErrorStep = 'Error in deleting the items from the BatchSummary'

        DELETE
        FROM PnS.BatchSummary
        WHERE PrintBatch = @batchId

    COMMIT TRAN

     SET @ErrorCode  = 0 -- @ErrorCode=0 if success,  <>0 if failure
     SET @ErrorStep='All data was moved over'   
    
        SELECT @ErrorCode As ErrorCode,@ErrorStep As ErrorMsg                       

END TRY

BEGIN CATCH

   --Get the Error Message for @@Error
    IF @@TRANCOUNT > 0 ROLLBACK
	SET @ErrorCode = ERROR_NUMBER()-- @ErrorCode=0 if success,  <>0 if failure
    SET @ErrorStep=@ErrorStep + ' '
        + cast(ERROR_NUMBER() as varchar(20)) + ' line: '
        + cast(ERROR_LINE() as varchar(20)) + ' ' 
        + ERROR_MESSAGE() + ' > ' 
        + ERROR_PROCEDURE()
    SELECT @ErrorCode As ErrorCode,@ErrorStep As ErrorMsg 

END CATCH
GO
/****** Object:  StoredProcedure [dbo].[usp_RevertPipelineStatus]    Script Date: 2/22/2018 3:20:05 PM ******/
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
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'Received',DownloadedOn = NULL,PreppedOn = NULL,SubmittedOn = NULL,
		 MRDFReceivedOn = NULL,ReportPreppedOn = NULL,ReportedBackOn = NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Prepped')
	BEGIN
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'Downloaded',PreppedOn = NULL,SubmittedOn = NULL,MRDFReceivedOn = NULL,
		 ReportPreppedOn = NULL,ReportedBackOn = NULL,ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL
		 ,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Submitted')
	BEGIN
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'Prepped',SubmittedOn = NULL,MRDFReceivedOn = NULL,ReportPreppedOn = NULL,
		 ReportedBackOn = NULL,ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'MRDFReceived')
	BEGIN
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'Submitted',MRDFReceivedOn = NULL,ReportPreppedOn = NULL
		 ,ReportedBackOn = NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'ReportPrepped')
	BEGIN
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'MRDFReceived',ReportPreppedOn = NULL,ReportedBackOn = NULL
		 ,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'ReportedBack')
	BEGIN
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'ReportPrepped',ReportedBackOn = NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Reprint')
	BEGIN
		 UPDATE PnS.BatchSummary SET PipelineStatus = 'Submitted',MRDFReceivedOn = NULL,ReportPreppedOn = NULL,ReportedBackOn = NULL,
		 ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL
		 WHERE PrintBatch = @batchId
		 DELETE FROM pns.MRDFResponse WHERE Disposition LIKE '%7%'
	END
END




GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateBusinessUnitDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_UpdateBusinessUnitDetails]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_UpdateBusinessUnitDetails] AS' 
END
GO
-- =============================================
-- Author:		Akshar
-- Create date: 31 Aug 2017
-- Description:	Updating Existing BusinessUnit
-- =============================================
ALTER PROCEDURE [dbo].[usp_UpdateBusinessUnitDetails]
	-- Add the parameters for the stored procedure here
	@Id int,
	@BusinessUnit varchar(10),
	@CustCode varchar(20)
AS
BEGIN
	UPDATE [Common].[BusinessUnit]
	SET [BusinessUnit] = @BusinessUnit,
		[CustCode] = @CustCode
	WHERE Id = @Id
END




GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateDocTypeDetails]    Script Date: 2/22/2018 3:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_UpdateDocTypeDetails]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_UpdateDocTypeDetails] AS' 
END
GO
-- =============================================
-- Author:		Akshar
-- Create date: 15 Nov 2017
-- Description:	Update Doc type details
-- =============================================
ALTER PROCEDURE [dbo].[usp_UpdateDocTypeDetails]
	-- Add the parameters for the stored procedure here
	@DocTypeId int,
	@DocTypeCode varchar(29),
	@Description varchar(255),
	@ModifiedOn datetime
AS
BEGIN
	UPDATE [AM].[DocTypes]
	SET [DocTypeCode] = @DocTypeCode
      ,[Description] = @Description
      ,[ModifiedOn] = @ModifiedOn
      
	WHERE Id = @DocTypeId
END




GO
