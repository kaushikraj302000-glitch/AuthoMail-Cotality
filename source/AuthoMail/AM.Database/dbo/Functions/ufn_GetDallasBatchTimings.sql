
	CREATE FUNCTION [dbo].[ufn_GetDallasBatchTimings]
	(
	@EstDate DATE
	)
	RETURNS @BatchTimings TABLE 
	(
		AppCode VARCHAR(255),
		BatchName VARCHAR(255),
		StartTime VARCHAR(255),
		EndTime VARCHAR(255),
		ShippingCarrier VARCHAR(255),
		ShippingServiceType VARCHAR(255)
	)
	AS
	BEGIN
		
		DECLARE @DayofWeek INT = DATEPART(dw,@EstDate)

		IF(@DayofWeek=7)
		BEGIN
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','10AM','00:00:00','11:00:00','USPS','IMB')			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','01PM','11:00:01','20:00:00','USPS','IMB')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','10AM','00:00:00','11:00:00','FedEx','PRIORITY_OVERNIGHT')			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','01PM','11:00:01','14:00:00','FedEx','PRIORITY_OVERNIGHT')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','10AM','00:00:00','11:00:00','USPS','PRIORITY_EXPRESS')			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','01PM','11:00:01','14:00:00','USPS','PRIORITY_EXPRESS')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','10AM','00:00:00','11:00:00','USPS','PRIORITY')		
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','01PM','11:00:01','14:00:00','USPS','PRIORITY')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','10AM','00:00:00','11:00:00','FedEx','PRIORITY_OVERNIGHT')			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','01PM','11:00:01','14:00:00','FedEx','PRIORITY_OVERNIGHT')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','10AM','00:00:00','11:00:00','USPS','PRIORITY_EXPRESS')			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','01PM','11:00:01','14:00:00','USPS','PRIORITY_EXPRESS')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','10AM','00:00:00','11:00:00','USPS','PRIORITY')		
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','01PM','11:00:01','14:00:00','USPS','PRIORITY')
		END
		ELSE
		BEGIN
			-- This is for Adverse Action Document types			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','07PM','15:00:01','20:00:00','USPS','IMB')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','12PM','00:00:00','13:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','05PM','13:00:01','18:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','06PM','18:00:01','19:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','07PM','19:00:01','20:00:00','FedEx','PRIORITY_OVERNIGHT')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','12PM','00:00:00','13:00:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','05PM','13:00:01','18:00:00','USPS','PRIORITY_EXPRESS')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','12PM','00:00:00','13:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('USAA','05PM','13:00:01','18:00:00','USPS','PRIORITY')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('HSBC','12PM','00:00:00','13:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('HSBC','04PM','13:00:01','17:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('HSBC','05PM','17:00:01','18:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('HSBC','06PM','18:00:01','19:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('HSBC','07PM','19:00:01','19:30:00','USPS','IMB')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','12PM','00:00:00','13:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','04PM','13:00:01','17:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','05PM','17:00:01','18:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','06PM','18:00:01','19:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','07PM','19:00:01','20:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','12PM','00:00:00','13:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','04PM','13:00:01','17:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','05PM','17:00:01','18:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','06PM','18:00:01','19:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BOTW','07PM','19:00:01','19:30:00','USPS','IMB')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BECU','12PM','00:00:00','13:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BECU','04PM','13:00:01','17:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BECU','05PM','17:00:01','18:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BECU','06PM','18:00:01','19:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('BECU','07PM','19:00:01','20:00:00','USPS','IMB')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','12PM','00:00:00','13:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','05PM','13:00:01','18:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','06PM','18:00:01','19:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','07PM','19:00:01','20:00:00','FedEx','PRIORITY_OVERNIGHT')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','12PM','00:00:00','13:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','04PM','13:00:01','17:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','05PM','17:00:01','18:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','06PM','18:00:01','19:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','07PM','19:00:01','20:00:00','USPS','IMB')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','12PM','00:00:00','13:00:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','04PM','13:00:01','17:00:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','05PM','17:00:01','18:00:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','06PM','18:00:01','19:00:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','07PM','19:00:01','20:00:00','USPS','PRIORITY_EXPRESS')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','12PM','00:00:00','13:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','04PM','13:00:01','17:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','05PM','17:00:01','18:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','06PM','18:00:01','19:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('AMTEST','07PM','19:00:01','20:00:00','USPS','PRIORITY')

			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('S3','12PM','00:00:00','13:00:00','USPS','IMB')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('S3','04PM','13:00:01','16:30:00','USPS','IMB')
			
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('PENFED','12PM','00:00:00','13:00:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('PENFED','04PM','13:00:01','16:30:00','USPS','PRIORITY')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('PENFED','12PM','00:00:00','13:00:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('PENFED','04PM','13:00:01','16:30:00','USPS','PRIORITY_EXPRESS')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('PENFED','12PM','00:00:00','13:00:00','FedEx','PRIORITY_OVERNIGHT')
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			VALUES ('PENFED','04PM','13:00:01','16:30:00','FedEx','PRIORITY_OVERNIGHT')

		END
		
		RETURN
	END
