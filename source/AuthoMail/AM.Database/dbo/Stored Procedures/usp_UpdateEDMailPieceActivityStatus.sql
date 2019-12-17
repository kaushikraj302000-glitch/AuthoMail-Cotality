-- =============================================
-- Author:		Akshar vala
-- Create date: 21 oct 2019
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_UpdateEDMailPieceActivityStatus]
	-- Add the parameters for the stored procedure here
	@Id int,
	@ConsentAcceptedOn datetime,
	@ConsentDeclinedOn datetime,
	@DocReviewedOn datetime,
	@DocDownloadedOn datetime,
	@ExpiryDate Datetime,
	@IpAddress varchar(50)
AS
BEGIN
	DECLARE @IsConsentAccepted bit
	DECLARE @IsConsentDeclined bit
	DECLARE @IsDocDownloaded bit


	UPDATE [AM].[EDeliveryMailPieces] 
	SET [ConsentAcceptedOn] = @ConsentAcceptedOn, [ConsentDeclinedOn] = @ConsentDeclinedOn, [DocReviewedOn] = @DocReviewedOn, [DocDownloadedOn] = @DocDownloadedOn, DownloadedIPAddress = @IpAddress WHERE Id=@Id

	SELECT @IsConsentAccepted = IsConsentAccepted, @IsConsentDeclined = IsConsentDeclined,@IsDocDownloaded = [IsDocDownloaded],@ConsentAcceptedOn = ConsentAcceptedOn,@DocDownloadedOn = DocDownloadedOn FROM [AM].[EDeliveryMailPieces] WHERE Id = @id

	IF(@IsConsentDeclined=1)
	BEGIN
		UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'Declined' WHERE Id=@Id
	END

	IF(@IsConsentAccepted = 1 AND @IsDocDownloaded = 1 AND @ConsentAcceptedOn <= @ExpiryDate AND @DocDownloadedOn <= @ExpiryDate)
	BEGIN
		UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'eDelivered',eDeliveredOn = @DocDownloadedOn WHERE Id=@Id
	END

	IF(GETUTCDATE() > @ExpiryDate)
	BEGIN
		IF(@IsConsentAccepted = 0 OR @IsDocDownloaded = 0)
		BEGIN
			UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'Expired' WHERE Id=@Id
		END
	END

END
