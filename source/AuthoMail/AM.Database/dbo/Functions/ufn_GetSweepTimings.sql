CREATE FUNCTION [dbo].[ufn_GetSweepTimings] 
( 
 
)
RETURNS @T TABLE
(
 SrNo int,
 AppCode VARCHAR(20),
 VendorCode VARCHAR(20),
 WeekDayType varchar(10),
 SweepType VARCHAR(20),
 SweepTime time 
) 
AS
begin

--NORDIS Sweep TIMINGS

	--ELLIEMAE
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ELLIEMAE','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (2,'ELLIEMAE','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (3,'ELLIEMAE','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (4,'ELLIEMAE','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (5,'ELLIEMAE','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ELLIEMAE','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (2,'ELLIEMAE','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (3,'ELLIEMAE','NORDIS','Saturday','OnSchedule','20:00:00')

	--ALASKA
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'ALASKA','NORDIS','Saturday','OnSchedule','20:00:00')

	--USAA
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'USAA','NORDIS','Saturday','OnSchedule','20:00:00')


	--HSBC
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'HSBC','NORDIS','Saturday','OnSchedule','20:00:00')



	--S3
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'S3','NORDIS','Saturday','OnSchedule','20:00:00')



	--PENFED
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'PENFED','NORDIS','Saturday','OnSchedule','20:00:00')



	--BECU
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BECU','NORDIS','Saturday','OnSchedule','20:00:00')


	--BOTW
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'BOTW','NORDIS','Saturday','OnSchedule','20:00:00')



	--CITIVM
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'CITIVM','NORDIS','Saturday','OnSchedule','20:00:00')



	--RPS
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'RPS','NORDIS','Saturday','OnSchedule','20:00:00')

	--LOAN CARE
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'LOANCARE','NORDIS','Saturday','OnSchedule','20:00:00')

	--MRCOOPER
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Weekday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Weekday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Weekday','OnSchedule','15:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Weekday','OnSchedule','17:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Weekday','OnSchedule','20:00:00')

	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Saturday','OnSchedule','08:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Saturday','OnSchedule','14:00:00')
	insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
	values (1,'MRCOOPER','NORDIS','Saturday','OnSchedule','20:00:00')


-- PCI AND DALLAS Timings
 --ALASKA
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'ALASKA','PCI','Weekday','OnSchedule','19:30:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'ALASKA','PCI','Saturday','OnSchedule','15:00:00')
  insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'ALASKA','PCI','Saturday','OnSchedule','19:30:00')
 
 --USAA
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','CL_DALLAS','Saturday','Continuous','00:00:00')

 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'USAA','PCI','Weekday','OnSchedule','20:00:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'USAA','PCI','Saturday','OnSchedule','20:00:00')
 
 --HSBC
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'HSBC','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'HSBC','CL_DALLAS','Saturday','Continuous','00:00:00')

 --BECU
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BECU','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BECU','CL_DALLAS','Saturday','Continuous','00:00:00')

 --AMTEST
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'AMTEST','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'AMTEST','PCI','Weekday','OnSchedule','20:00:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'AMTEST','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'AMTEST','PCI','Saturday','OnSchedule','20:00:00')

  --AMTEST
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'AMTEST','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'AMTEST','CL_DALLAS','Saturday','Continuous','00:00:00')

 --BOTW
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BOTW','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BOTW','CL_DALLAS','Saturday','Continuous','00:00:00')
 
 --CITI
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'CITIVM','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'CITIVM','PCI','Weekday','OnSchedule','19:30:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'CITIVM','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'CITIVM','PCI','Saturday','OnSchedule','19:30:00')

  --RPS
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'RPS','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'RPS','PCI','Weekday','OnSchedule','19:30:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'RPS','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'RPS','PCI','Saturday','OnSchedule','19:30:00')

 --S3
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'S3','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'S3','CL_DALLAS','Saturday','Continuous','00:00:00')

 --PENFED
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'PENFED','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'PENFED','CL_DALLAS','Saturday','Continuous','00:00:00')

 return
end
