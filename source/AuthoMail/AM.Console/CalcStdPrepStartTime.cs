using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Console
{
    public class CalcStdPrepStartTime
    {
        public static void Start()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())            
            {
                var calcStdPrepStartTimeList = rSvc.GetCalcListOfPrepStartTime();
                var timings = rSvc.GetSweepTimings();
                foreach (var item in calcStdPrepStartTimeList)
                {
                    DateTime stdPrepStartTime = GetEstimatedSubmittedOn(timings, 0, item.ReceivedOn);
                    if (stdPrepStartTime == null)
                    {
                        
                    }
                    rSvc.UpdateStdPrepStartTime(item.Id, stdPrepStartTime);
                }
            }
        }

        public static DateTime GetEstimatedSubmittedOn(List<Sweep> sweepTimings, int bufferTime,DateTime receivedOn)
        {
            var revisedReceivedOnEst = receivedOn.UtcToEst();
            
            sweepTimings = sweepTimings.OrderBy(t => t.SrNo).ToList();
            if (revisedReceivedOnEst.DayOfWeek == DayOfWeek.Saturday)
            {
                sweepTimings = sweepTimings.Where(s => s.WeekDayType == "Saturday").ToList();
            }
            else
            {
                sweepTimings = sweepTimings.Where(s => s.WeekDayType == "Weekday").ToList();
            }            

            // adjust for sunday
            revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1).Date : revisedReceivedOnEst;

            var applicableSweep = sweepTimings.FirstOrDefault(s => s.SweepTime >= revisedReceivedOnEst.TimeOfDay);

            if (applicableSweep == null)
            {
                revisedReceivedOnEst = revisedReceivedOnEst.AddDays(1);
                // adjust for sunday
                revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1).Date : revisedReceivedOnEst;
                applicableSweep = sweepTimings.First();
            }

            var estimatedSubmittedOn = (revisedReceivedOnEst.Date + applicableSweep.SweepTime.Add(new TimeSpan(0, bufferTime, 0))).EstToUtc();

            return estimatedSubmittedOn;
        }
    }
}
