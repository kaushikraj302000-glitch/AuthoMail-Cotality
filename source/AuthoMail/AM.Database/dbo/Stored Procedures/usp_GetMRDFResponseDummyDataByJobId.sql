

				CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDummyDataByJobId]
				@jobId int
				AS
				BEGIN
					-- SET NOCOUNT ON added to prevent extra result sets from
					-- interfering with SELECT statements.
					SET NOCOUNT ON;

					SELECT Req.JobId, Req.PieceId, Req.[Date], Req.TotalSheets, Req.InputWeight, Req.Feeder3_Sel, Req.Feeder4_Sel
						, Req.Feeder5_Sel, Req.LowCap_Sel, Req.NoSeal_Sel, Req.Feeder2_Sel, Req.ZipMark_Sel, Req.Address1, Req.Address2
						, Req.Zip5, Req.Zip4, Req.Zip2, Req.DivertVS2, Req.DivertVS3
						, Req.DivertVS4, Req.ReturnLogoBitMap, Req.ReturnLine1, Req.ReturnLine2, Req.ReturnLine3, Req.ReturnLine4
						, Req.ReturnLine5, Req.ReturnLine6, Req.GroupNumber, Req.SocialSecNumber, Req.CheckAmount, Req.PassThru
						, Req.Expansion, Req.EndofRecord, Req.UniqueId, Req.PDFFileName, Req.ErrId, Req.ErrDesc, Req.MailPieceStatus, Req.IsPDFReceived 
						, Req.Feeder2_Sel as Feeder2_Fed
						,NULL AS PackageTrackingNumber
						,'RichardP' as OperatorId,'PCI09   ' as MachineId
						,(CONVERT(VARCHAR(8), [dbo].[ufn_UTCToEST](GETUTCDATE()), 112)+REPLACE(CONVERT(VARCHAR(8), [dbo].[ufn_UTCToEST](GETUTCDATE()), 108),':','')) as ProcessingTimeStamp
						,'4691 ' as DetailedErrorId,'00' as Input_Fed,'0' as Feeder3_Fed,'0' as Feeder4_Fed,'0' as Feeder5_Fed
						,CASE WHEN P.IsPullBackRequested=1 AND P.PulledBackOn IS NULL THEN '4 ' WHEN P.IsQCRequested=1 THEN '8 ' ELSE '5 ' END as Disposition
						,'0.00 ' as ComputedWeight, '00000000' as MeterDate,'Mailrun_Files            ' as StatusSource
						,'0.0000' as PostageMetered,'000 ' as WeightMeasured,'Mailrun_Files            ' as ExitLocation
						FROM [PnS].[MRDFRequest] Req
						INNER JOIN [AM].[MailPieces] P ON P.UniqueId=Req.UniqueId
						WHERE Req.JobId = @jobId AND Req.MailPieceStatus='InProgress'
				END
