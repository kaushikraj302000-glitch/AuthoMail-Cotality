
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
 --ALASKA
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'ALASKA','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'ALASKA','PCI','Weekday','OnSchedule','19:45:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'ALASKA','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'ALASKA','PCI','Saturday','OnSchedule','19:45:00')
 
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

 --BOTW
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BOTW','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BOTW','CL_DALLAS','Saturday','Continuous','00:00:00')
 
  --BECU
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BECU','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BECU','CL_DALLAS','Saturday','Continuous','00:00:00')
 
 return
end

