TRUNCATE TABLE [Common].[SystemSettings]

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_AuthoMailRootFolder', N'String', N'D:\Production\AuthoMail\AMRoot', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\bin\Debug\AuthoMailRoot', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_CountryCode', N'String', N'US', NULL, 0, N'US', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_CoverTemplateFolder', N'String', N'CoverTemplates', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Core\CoverTemplates', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_FedexServiceAttemptInterval', N'Int32', N'15000', NULL, 0, N'15000', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_NoofTime_AttemptFedexService', N'Int32', N'5', NULL, 0, N'5', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_PnS_SFT_RootDir', N'String', N'\\MTC-sagarg\c$\MSTC\PnS', NULL, 0, N'\\MTC-sagarg\c$\MSTC\PnS', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_DirectReportBackChannelCodes', N'String', N'AMC_Internal,CITIVM_Internal,DALLAS_Internal,RPS_Internal', NULL, 0, N'AMC_Internal,CITIVM_Internal,DALLAS_Internal,RPS_Internal', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_ExpressMailingText', N'String', N'-ExpressMailing', NULL, 0, N'-ExpressMailing', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_AMCBackupReportUploadPath', N'String', N'/BackupReports', NULL, 0, N'/BackupReports', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_PriorityMailingText', N'String', N'-Priority', NULL, 0, N'-Priority', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_PSNoDataEmailSubject', N'String', N'Production Summary', NULL, 0, N'Production Summary', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_DSEmailTemplate', N'String', N'HTMLTemplates\DET.html', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\DET.html', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_PSNoDataEmailTemplate', N'String', N'HTMLTemplates\NoData.html', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\NoData.html', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_Environment', N'String', N'Internal', N'To indicate on which environment job is running.', 0, N'Staging', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_Exception_Email_Cc', N'String', N'nirav.barot@milestonetechnoconsulting.com,milan.patel@milestonetechnoconsulting.com', NULL, 0, N'nirav.barot@milestonetechnoconsulting.com,milan.patel@milestonetechnoconsulting.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_Exception_Email_To', N'String', N'sgolvelkar@corelogic.com,avala@corelogic.com,vmungara@corelogic.com', NULL, 0, N'sgolvelkar@corelogic.com,avala@corelogic.com,vmungara@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_ExceptionEmail_Subject', N'String', N'Unexpected error occurred in AuthoMail', NULL, 0, N'Unexpected error occurred in AuthoMail', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_PSEmailTemplate', N'String', N'HTMLTemplates\PLET.html', NULL, 0, N'D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\PLET.html', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_Email_BodyMsg', N'String', N'***TEST*** PFA pullback request form', N'', 0, N'Please find attached PullBackRequestForm', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_Email_Cc', N'String', N'akshar.vala@milestonetechnoconsulting.com', N'To whom need to send email.', 0, N'avala@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_Email_Subject', N'String', N'TEST ADE "PULL REQUEST"', N'', 0, N'ADE "PULL REQUEST"', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_Email_To', N'String', N'akshar.vala@milestonetechnoconsulting.com', N'To whom need to send email.', 0, N'avala@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_PB_ClientTimeZone', N'String', N'Eastern Standard Time', N'Define client''s local timezone.', 0, N'Eastern Standard Time', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_PB_PullBackFormTemplatePath', N'String', N'~\Template\PullBackForm.pdf', N'Path of pdf template which is used to send when pullback is requested.', 0, N'E:\Git\AuthoMail\source\AuthoMail\AM.Web\Template\PullBackForm.pdf', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_EndCutOffTime', N'String', N'19:45:00', NULL, 0, N'19:45:00', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Ajax_CommonError', N'String', N'Communication error.', NULL, 0, N'Communication error.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_Aborted', N'String', N'Mailpiece is aborted, it is not going to be printed.', N'When mailpiece is aborted by client or for unknown reason.', 0, N'Mailpiece is aborted, it is not going to be printed.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_DataNull', N'String', N'Mailpiece with the specified unique id is yet to be verified, please try again after few minutes.', N'When no data found for specified unique id.', 0, N'Mailpiece with the specified unique id is yet to be verified, please try again after few minutes.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_Empty', N'String', N'At least one mailpiece id must be specified.', N'user have select atleast one mailpiece to request pullback.', 0, N'At least one mailpiece id must be specified.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_Error', N'String', N'Mailpiece has an error, it is not going to be printed. Check error report for details.', N'If mailpiece has some error.', 0, N'Mailpiece has an error, it is not going to be printed. Check error report for details.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_PBReqFormErr', N'String', N'An error occured while generating pullback request form.', N'When error occured while generating pullback form.', 0, N'An error occured while generating pullback request form.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_PullBackAlreadyRequested', N'String', N'Pullback is already requested for this mailpiece.', N'When pullback is already requested by some user.', 0, N'Pullback is already requested for this mailpiece.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_PulledBack', N'String', N'Mailpiece is already pulled back.', N'When mailpiece is pulledback.', 0, N'Mailpiece is already pulled back.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_Shipped', N'String', N'Mailpiece is already shipped, pullback cannot be requested.', N'When mailpiece statis is shipped then it can not be pullback.', 0, N'Mailpiece is already shipped, pullback cannot be requested.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_Success', N'String', N'Pullback request is recorded successfully.', N'If mailpiece has recorded successfully.', 0, N'Pullback request is recorded successfully.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Error', N'String', N'An error occurred while processing your request.', NULL, 0, N'An error occurred while processing your request.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Lockout', N'String', N'This account has been locked out, please try again later.', NULL, 0, N'This account has been locked out, please try again later.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Login_Default', N'String', N'Invalid login attempt.', NULL, 0, N'Invalid login attempt.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Login_EmailConfirm', N'String', N'Please confirm email address.', NULL, 0, N'Please confirm email address.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Login_InvalidUserName', N'String', N'Invalid username.', NULL, 0, N'Invalid username.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Login_Success', N'String', N'Login successful.', NULL, 0, N'Login successful.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_NoErrorDescription', N'String', N'--', NULL, 0, N'No error description.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_NoMailPieceReceivedToday', N'String', N'No batches received today.', NULL, 0, N'No batches received today.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_NoMailPieceSelect', N'String', N'Please select at least one mailpiece.', NULL, 0, N'Please select at least one mailpiece.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_NoRecordFound', N'String', N'No Records', NULL, 0, N'No Records', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_PullBackConfirmation', N'String', N'Click ''Yes'' to confirm your pullback request.', NULL, 0, N'Click ''Yes'' to confirm your pullback request.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_StartCutOffTime', N'String', N'19:45:01', NULL, 0, N'19:45:01', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_NoRecordInTodayAndPendingBatches', N'String', N'No such records found in todays baches or pending batches', NULL, 0, N'No such records found in todays baches or pending batches', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_FedExTestService', N'String', N'https://wsbeta.fedex.com:443/web-services/ship', NULL, 0, N'https://wsbeta.fedex.com:443/web-services/ship', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_FedExProdService', N'String', N'https://gateway.fedex.com:443/web-services', NULL, 0, N'https://gateway.fedex.com:443/web-services', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_AMC_ReportDateFormat', N'String', N'M/d/yyyy h:mm:ss tt', NULL, 0, N'M/d/yyyy h:mm:ss tt', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Monitor_AMCPulledBackDate', N'String', N'1/1/1980 12:00:00 AM', NULL, 0, N'1/1/1980 12:00:00 AM', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_Inactive_Account', N'String', N'Your account is not yet approved. If it is taking longer, you may escalate to rmcfarlane@corelogic.com, brpatel@corelogic.com or mlowenberg@corelogic.com.', NULL, 0, N'Your account is not yet approved. If it is taking longer, you may escalate to rmcfarlane@corelogic.com, brpatel@corelogic.com or mlowenberg@corelogic.com.', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())


INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_PulledBackSuccess', N'String', N'Mailpiece pulled back successfully.', NULL, 0, N'Mailpiece pulled back successfully.', NULL, CAST(N'2018-06-14 07:14:18.343' AS DateTime))

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_Msg_API_PB_BatchNotValidated', N'String', N'The requested mailpiece is yet to be received or validated.', NULL, 0, N'The requested mailpiece is yet to be received or validated.', NULL, CAST(N'2018-06-13 09:12:15.033' AS DateTime))

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_RegistrationDefaultEmails', N'String', 'avala@corelogic.com,sgolvelkar@corelogic.com', 'Notification email for registration', 0, 'avala@corelogic.com,sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_MaxJsonLength', N'Int32', N'16000000', NULL, 0, N'16000000', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), CAST(N'2018-06-29 06:00:10.533' AS DateTime))
	
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_USPSEndPageTemplatePath', N'String', N'USPSBarcodedEndPageTemplate.pdf', NULL, 0, N'USPSBarcodedEndPageTemplate.pdf', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_FedExEndPageTemplatePath', N'String', N'FedExBarcodedEndPageTemplate.pdf', NULL, 0, N'FedExBarcodedEndPageTemplate.pdf', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_FrontPageTemplatePath', N'String', N'BarcodedFrontPageTemplate.pdf', NULL, 0, N'BarcodedFrontPageTemplate.pdf', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_IsDbLoggingEnabled', N'Boolean', N'true', NULL, 0, N'false', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_MilitaryCities', N'String', N'APO,DPO,FPO', NULL, 0, N'APO,DPO,FPO', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_MilitaryStates', N'String', N'AA,AE,AP', NULL, 0, N'AA,AE,AP', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_Territory', N'String', N'AS,FM,GU,MH,MP,PR,PW,VI', NULL, 0, N'AS,FM,GU,MH,MP,PR,PW,VI', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

--INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_URL', N'String', N'http://production.shippingapis.com/ShippingAPI.dll', NULL, 0, N'http://production.shippingapis.com/ShippingAPI.dll', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_UserId', N'String', N'228COREL3398', NULL, 0, N'819MILES3278', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_VerifyAPIName', N'String', N'Verify', NULL, 0, N'Verify', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_AddressCount', N'Int32', N'5', NULL, 0, N'5', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_MainlandStates', N'String', N'AK,AL,AR,AZ,CA,CO,CT,DC,DE,FL,GA,HI,IA,ID,IL,IN,KS,KY,LA,MA,MD,ME,MI,MN,MO,MS,MT,NC,ND,NE,NH,NJ,NM,NV,NY,OH,OK,OR,PA,RI,SC,SD,TN,TX,UT,VA,VT,WA,WI,WV,WY', NULL, 0, N'AK,AL,AR,AZ,CA,CO,CT,DC,DE,FL,GA,HI,IA,ID,IL,IN,KS,KY,LA,MA,MD,ME,MI,MN,MO,MS,MT,NC,ND,NE,NH,NJ,NM,NV,NY,OH,OK,OR,PA,RI,SC,SD,TN,TX,UT,VA,VT,WA,WI,WV,WY', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_eVSAPIName', N'String', N'eVS', NULL, 0, N'eVS', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_URL', N'String', N'https://stg-secure.shippingapis.com/ShippingAPI.dll', NULL, 0, N'https://stg-secure.shippingapis.com/ShippingAPI.dll', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_DD_PrintFormatList', N'String', N'Simplex, Duplex', N'Print format dropdown values', 0, N'Simplex, Duplex', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_DD_PrintSizeList', N'String', N'MixMedia, LETTER-IP, GLT-20-DOP-17.5, LEGAL-IP, WC24#LTR-PERF, LEGAL-PERF-IP', N'Print size dropdown values', 0, N'MixMedia, LETTER-IP, GLT-20-DOP-17.5, LEGAL-IP, WC24#LTR-PERF, LEGAL-PERF-IP', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_DD_ShippingCarrierList', N'String', N'USPS, FedEx, UPS', N'Shipping Carrier dropdown values', 0, N'USPS, FedEx, UPS', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_DD_ShippingServiceTypeList', N'String', N'USPS, FEDEX_2_DAY, FEDEX_EXPRESS_SAVER, FEDEX_GROUND, FIRST_OVERNIGHT, GROUND_HOME_DELIVERY, PRIORITY_OVERNIGHT, STANDARD_OVERNIGHT', N'Shipping Service Type dropdown values', 0, N'USPS, FEDEX_2_DAY, FEDEX_EXPRESS_SAVER, FEDEX_GROUND, FIRST_OVERNIGHT, GROUND_HOME_DELIVERY, PRIORITY_OVERNIGHT, STANDARD_OVERNIGHT', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_DD_ShippingSpecialServiceList', N'String', N'SATURDAY_DELIVERY, SATURDAY_PICKUP', N'Shipping Special Service dropdown values', 0, N'SATURDAY_DELIVERY, SATURDAY_PICKUP', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_USPS_eVSCancelAPIName', N'String', N'eVSCancel', NULL, 0, N'eVSCancel', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ProcessingBufferTime', N'Int32', N'30', NULL, 0, N'30', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_API_AlertElapsedMinute', N'Int32', N'10', NULL, 0, N'10', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_RegistrationBCCEmails', N'String', 'avala@corelogic.com,sgolvelkar@corelogic.com', 'Default BCC emails for getting notification from registration flow', 0, 'avala@corelogic.com,sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ErrorSubscribedEmailSubject', N'String', 'Error Notification For Batch - {0}', 'email subject for error notification', 0, 'Error Notification For Batch - {0}', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ErrorSubscribedEmailCc', N'String', 'avala@corelogic.com,sgolvelkar@corelogic.com', 'cc emails for internal team apart from subscribed email list', 0, 'avala@corelogic.com,sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ErrorSubscribedEmailTemplate', N'String', 'HTMLTemplates\ErrorSubScribed.html', 'template for error notification', 0, 'HTMLTemplates\ErrorSubScribed.html', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_BaseURL', N'String', 'http://10.215.48.138:8080/', '', 0, 'http://10.215.48.138:8080/', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_ClientId', N'String', '3A57E75F-E25A-4077-9B59-C3766489562C', '', 0, '3A57E75F-E25A-4077-9B59-C3766489562C', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_UserName', N'String', 'AksharClientAdmin', '', 0, 'AksharClientAdmin', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_Password', N'String', 'Intellimods123#', '', 0, 'Intellimods123#', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_CreateSigningRoomURL', N'String', 'api/SigningRoomManager/SigningRooms/CreateFromPackage', '', 0, 'api/SigningRoomManager/SigningRooms/CreateFromPackage', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_SendInvitationURL', N'String', 'api/SigningRoomManager/SigningRooms/{0}/Participants/SendInvitations', '', 0, 'api/SigningRoomManager/SigningRooms/{0}/Participants/SendInvitations', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_SigningRoomActivityLogSearchURL', N'String', 'api/Reports/SigningRoomActivityLogSearch', '', 0, 'api/Reports/SigningRoomActivityLogSearch', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())

INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'E_API_SigningRoomSummaryURL', N'String', 'api/SigningRoomManager/SigningRooms/{0}/signingroomsummary', '', 0, 'api/SigningRoomManager/SigningRooms/{0}/signingroomsummary', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())