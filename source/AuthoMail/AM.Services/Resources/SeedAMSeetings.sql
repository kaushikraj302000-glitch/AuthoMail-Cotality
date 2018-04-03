TRUNCATE TABLE [Common].[SystemSettings]

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'AMCDirPath', N'String', N'\\MTC-sagarg\c$\MSTC\AM\AMC', NULL, 0, N'\\MTC-sagarg\c$\MSTC\AM\AMC', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'API_Email_Cc', N'String', N'avala@corelogic.com', N'To whom need to send email.', 0, N'avala@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'API_Email_To', N'String', N'avala@corelogic.com', N'To whom need to send email.', 0, N'avala@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'API_PB_ClientTimeZone', N'String', N'Eastern Standard Time', N'Define client''s local timezone.', 0, N'Eastern Standard Time', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'API_PB_PullBackFormTemplatePath', N'String', N'E:\Git\AuthoMail\source\AuthoMail\AM.Web\Template\PullBackForm.pdf', N'Path of pdf template which is used to send when pullback is requested.', 0, N'E:\Git\AuthoMail\source\AuthoMail\AM.Web\Template\PullBackForm.pdf', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'AuthoMailRootFolder', N'String', N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\bin\Debug\AuthoMailRoot', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\bin\Debug\AuthoMailRoot', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'CountryCode', N'String', N'US', NULL, 0, N'US', (select Id from [Profile].Users where UserName = 'system@mtc.com'), CAST(N'2018-04-03 06:18:52.750' AS DateTime))

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'CoverTemplateFolder', N'String', N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Core\CoverTemplates', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Core\CoverTemplates', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'CustomerTransactionId', N'String', N'***Express Domestic Ship v7 Request using VC#***', NULL, 0, N'***Express Domestic Ship v7 Request using VC#***', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'DS_Email_Cc', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'DS_Email_To', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'DSEmail_Subject', N'String', N'TEST USAA Download Summary', NULL, 0, N'TEST USAA Download Summary', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'DSEmailTemplate', N'String', N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\DET.html', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\DET.html', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'EndCutOffTime', N'String', N'19:45:00', NULL, 0, N'19:45:00', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Environment', N'String', N'Staging', N'To indicate on which environment job is running.', 0, N'Staging', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Exception_Email_Cc', N'String', N'nirav.barot@milestonetechnoconsulting.com,milan.patel@milestonetechnoconsulting.com', NULL, 0, N'nirav.barot@milestonetechnoconsulting.com,milan.patel@milestonetechnoconsulting.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Exception_Email_To', N'String', N'sgolvelkar@corelogic.com,avala@corelogic.com,vmungara@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com,avala@corelogic.com,vmungara@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'ExceptionEmail_Subject', N'String', N'Unexpected error occurred in AuthoMail', NULL, 0, N'Unexpected error occurred in AuthoMail', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'FedexServiceAttemptInterval', N'Int32', N'15000', NULL, 0, N'15000', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_Aborted', N'String', N'Mailpiece is aborted, it is not going to be printed.', N'When mailpiece is aborted by client or for unknown reason.', 0, N'Mailpiece is aborted, it is not going to be printed.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_BatchStatusSubmitted', N'String', N'Batch is being parsed, please try again after few minutes.', N'If batchstatus is submitted then only mailpiece can be pullback.', 0, N'Batch is being parsed, please try again after few minutes.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_DataNull', N'String', N'Mailpiece with the specified unique id is not yet received.', N'When no data found for specified unique id.', 0, N'Mailpiece with the specified unique id is not yet received.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_Empty', N'String', N'At least one mailpiece id must be specified.', N'user have select atleast one mailpiece to request pullback.', 0, N'At least one mailpiece id must be specified.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_Error', N'String', N'Mailpiece has an error, it is not going to be printed. Check error report for details.', N'If mailpiece has some error.', 0, N'Mailpiece has an error, it is not going to be printed. Check error report for details.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_PBReqFormErr', N'String', N'An error occured while generating pullback request form.', N'When error occured while generating pullback form.', 0, N'An error occured while generating pullback request form.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_PullBackAlreadyRequested', N'String', N'Pullback is already requested for this mailpiece.', N'When pullback is already requested by some user.', 0, N'Pullback is already requested for this mailpiece.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_PulledBack', N'String', N'Mailpiece is already pulled back.', N'When mailpiece is pulledback.', 0, N'Mailpiece is already pulled back.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_Shipped', N'String', N'Mailpiece is already shipped, pullback cannot be requested.', N'When mailpiece statis is shipped then it can not be pullback.', 0, N'Mailpiece is already shipped, pullback cannot be requested.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_API_PB_Success', N'String', N'Pullback request is recorded successfully.', N'If mailpiece has recorded successfully.', 0, N'Pullback request is recorded successfully.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_Ajax_CommonError', N'String', N'Communication error. Please check your internet connection and try again.', NULL, 0, N'Communication error. Please check your internet connection and try again.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'MSG_WEB_Error', N'String', N'An error occurred while processing your request.', NULL, 0, N'An error occurred while processing your request.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'MSG_WEB_Lockout', N'String', N'This account has been locked out, please try again later.', NULL, 0, N'This account has been locked out, please try again later.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_Login_Default', N'String', N'Invalid login attempt.', NULL, 0, N'Invalid login attempt.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_Login_EmailConfirm', N'String', N'Please confirm email address.', NULL, 0, N'Please confirm email address.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_Login_InvalidUserName', N'String', N'Invalid username.', NULL, 0, N'Invalid username.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_Login_Success', N'String', N'Login successful.', NULL, 0, N'Login successful.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_NoErrorDescription', N'String', N'No error description.', NULL, 0, N'No error description.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_NoMailPieceReceivedToday', N'String', N'No batches received today.', NULL, 0, N'No batches received today.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_NoMailPieceSelect', N'String', N'Please select at least one mailpiece.', NULL, 0, N'Please select at least one mailpiece.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_NoRecordFound', N'String', N'No Records', NULL, 0, N'No Records', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Msg_Web_PullBackConfirmation', N'String', N'Click ''Yes'' to confirm your pullback request.', NULL, 0, N'Click ''Yes'' to confirm your pullback request.', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'NoBatch_Email_Cc', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'NoBatch_Email_To', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'NoBatchReceivedNotification_Subject', N'String', N'No USAA batch is available', NULL, 0, N'No USAA batch is available', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'NoofTime_AttemptFedexService', N'Int32', N'5', NULL, 0, N'5', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnS_Email_Cc', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnS_Email_To', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnS_SFT_HostURL', N'String', N'\\MTC-sagarg\c$\MSTC\PnS', NULL, 0, N'\\MTC-sagarg\c$\MSTC\PnS', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnS_SFT_Password', N'String', N'', NULL, 0, N'', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnS_SFT_RootDir', N'String', N'\\MTC-sagarg\c$\MSTC\PnS', NULL, 0, N'\\MTC-sagarg\c$\MSTC\PnS', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnS_SFT_UserName', N'String', N'user', NULL, 0, N'user', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PnSEmail_Subject', N'String', N'TEST USAA Status Update', NULL, 0, N'TEST USAA Status Update', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PS_Email_Cc', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PS_Email_To', N'String', N'sgolvelkar@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PSEmail_Subject', N'String', N'TEST USAA Production Summary', NULL, 0, N'TEST USAA Production Summary', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'PSEmailTemplate', N'String', N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\PLET.html', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\PLET.html', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'ReportSFTUploadingChannelCode', N'String', N'USAA_CL', NULL, 0, N'USAA_CL', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'ReportSFTUploadingPathAMC', N'String', N'/Unknown/USAA/Output', NULL, 0, N'/Unknown/USAA/Output', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'StartCutOffTime', N'String', N'19:45:01', NULL, 0, N'19:45:01', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'USAACustIdentifier', N'String', N'166', NULL, 0, N'166', (select Id from [Profile].Users where UserName = 'system@mtc.com'), GETUTCDATE())