CREATE PROCEDURE [dbo].[usp_InsertTrackingDump]
	@TrackingDump AS [dbo].TrackingDump READONLY
AS
BEGIN

	SET NOCOUNT ON

	INSERT INTO TrackingDump(IsReturnTracking,TrackingNo,ShippingCarrier,ShipmentStatus,ShipmentStatusUpdatedOn,DeliveryDate,DeliveryStatus,MailPieceId)
	SELECT IsReturnTracking,TrackingNo,ShippingCarrier,ShipmentStatus,ShipmentStatusUpdatedOn,DeliveryDate,DeliveryStatus
	,CASE WHEN IsReturnTracking=0 THEN (SELECT TOP 1 Id FROM [AM].[MailPieces] WHERE [TrackingNo] = TrackingNo ORDER BY EffectiveReceivedOnEst DESC) ELSE (SELECT TOP 1 Id FROM [AM].[MailPieces] WHERE [ReturnTrackingNo] = TrackingNo ORDER BY EffectiveReceivedOnEst DESC) END
	FROM @TrackingDump

	SELECT @@ROWCOUNT

END