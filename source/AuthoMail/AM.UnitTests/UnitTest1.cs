using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AM.Services;
using System.Collections;
using System.Collections.Generic;
using AM.Entities;
using System.IO;
using AM.Lib;

namespace AM.UnitTests
{
    enum SLATypes
    {
        NA,
        FixedTime,
        Duration
    }

    [TestClass]
    public class UnitTest1
    {
        ISearchService serSvc = new SearchService();
        [TestMethod]
        public void TestMethod1()
        {
            List<TestModel> mp = new List<TestModel>();
            //mp = serSvc.TestMP();

            string slaType = null;
            TimeSpan? cutOff = null;
            TimeSpan? slaTimeSpan = null;
            DateTime dueDate = new DateTime();

            // convert received on from utc to est

            var results = new Dictionary<int, bool>();

            foreach (var item in mp)
            {

                //if (item.Id == 1205)
                //{
                //    string s = "let me Check";
                //}

               item.ReceivedOn = item.ReceivedOn.UtcToEst();

                //item.receivedon = item.receivedon.UtcToEst();

                if (!item.DocTypeId.HasValue)
                {
                    results.Add(item.Id, !item.DueDate.HasValue);
                    continue;
                }
                if (item.DocTypeId.HasValue && item.ErrorCode != "No_Error")
                {
                    results.Add(item.Id, !item.DueDate.HasValue);
                    continue;
                }

                var dow = item.ReceivedOn.DayOfWeek;
                if (dow == DayOfWeek.Saturday)
                {
                    slaType = item.SatSLAType;
                    slaTimeSpan = item.ActualSatTimeSpan; ;

                    if (slaType == SLATypes.NA.ToString())
                    {
                        dow = DayOfWeek.Sunday;
                    }
                }
                if (dow == DayOfWeek.Sunday)
                {
                    slaType = item.SunSLAType;
                    slaTimeSpan = item.ActualSunTimeSpan;

                    if (slaType == SLAType.NA.ToString())
                    {
                        dow = DayOfWeek.Monday;
                    }
                }
                if (dow == DayOfWeek.Monday || dow == DayOfWeek.Tuesday || dow == DayOfWeek.Wednesday || dow == DayOfWeek.Thursday || dow == DayOfWeek.Friday)
                {
                    slaType = item.BizdaySLAType;
                    slaTimeSpan = item.ActualBizdayTimeSpan;
                    cutOff = item.BizCutOffTimeSpan;
                }

                if (slaType == SLAType.FixedTime.ToString())
                {
                    //var effectiveCutOff = item.receivedon.Date.Add(TimeSpan.Parse(cutOff));
                    //var regularDueDate = item.receivedon.Date.Add(TimeSpan.Parse(cutOff));

                    var effectiveCutOff = item.BizCutOffTimeSpan.HasValue ? item.ReceivedOn.Date.Add(item.BizCutOffTimeSpan.Value) : (DateTime?)null;
                    //var regularDueDate = item.receivedon.Date.Add(cutOff.Value);
                    var regularDueDate = item.ReceivedOn.Date.Add(slaTimeSpan.Value);

                    dueDate = item.ReceivedOn > effectiveCutOff ? regularDueDate.AddDays(1) : regularDueDate;
                }
                else
                {
                    dueDate = slaTimeSpan.HasValue ? item.ReceivedOn + slaTimeSpan.Value : item.ReceivedOn;
                }

                var holidayList = serSvc.GetHolidays();

                while (IsHoliday(dueDate, holidayList, item.SatSLAType == SLATypes.NA.ToString(), item.SunSLAType == SLATypes.NA.ToString()))
                    dueDate = dueDate.AddDays(1);

                dueDate = GeneralExtensions.EstToUtc(dueDate);

                results.Add(item.Id, item.DueDate == dueDate);

            }


            // If all duedate are correct then pass otherwise fail
            var failures = results.Where(kp => kp.Value == false).ToList();
            var failureList = String.Join(", ", failures.Select(kp => kp.Key));

            if (failures.Count > 0)
            {
                Assert.Fail("Total {0} mailpieces tested. {1} failed. List - {2}", results.Count, failures.Count, failureList);
            }

            //else
            //    continue;
            // }
        }

        private bool IsHoliday(DateTime dueDate, List<DateTime> holidayList, bool isSatHoliday, bool isSunHoliday)
        {
            if (holidayList.Contains(dueDate.Date))
            {
                return true;
            }

            if (dueDate.DayOfWeek == DayOfWeek.Saturday)
            {
                return isSatHoliday;
            }

            if (dueDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return isSunHoliday;
            }
            return false;
        }
    }
}
