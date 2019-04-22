using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    public class Purging
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //Testing Paths
        //private static DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"E:\Delete\AMRoot");
        //private static DirectoryInfo destAMDir = new DirectoryInfo(@"E:\Delete\AMTrash");
        //private static DirectoryInfo sourcePnSRootDir = new DirectoryInfo(@"E:\Delete\PCIRoot");
        //private static DirectoryInfo destPCIDir = new DirectoryInfo(@"E:\Delete\PCITrash");

        //PROD paths
        private static DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"\\10.212.24.57\d$\Production\AuthoMail\AMRoot");
        private static DirectoryInfo destAMDir = new DirectoryInfo(@"\\10.212.24.57\D$\MTC\Delete\AM");
        private static DirectoryInfo sourcePnSRootDir = new DirectoryInfo(@"\\10.212.24.57\d$\MTC\PnSService\PCIRoot");
        private static DirectoryInfo destPCIDir = new DirectoryInfo(@"\\10.212.24.57\D$\MTC\Delete\PCI");

        //Staging Paths
        //private static DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail\Monitor\AuthoMailRoot");
        //private static DirectoryInfo destAMDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\Delete\AM");
        //private static DirectoryInfo sourcePnSRootDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail\PnSService\PCIRoot");
        //private static DirectoryInfo destPCIDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\Delete\PCI");
        public static void StartPurging()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            {
                var amDirs = sourceAMRootDir.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in amDirs)
                {
                    if (!Directory.Exists(Path.Combine(destAMDir.FullName, dir.Name)))
                        Directory.CreateDirectory(Path.Combine(destAMDir.FullName, dir.Name));
                    int days = 0;
                    if (dir.Name == "ALASKA")
                        days = 30;
                    else if (dir.Name == "BOTW")
                        days = 30;
                    else if (dir.Name == "CITIVM")
                        days = 30;
                    else if (dir.Name == "HSBC")
                        days = 30;
                    else if (dir.Name == "RPS")
                        days = 60;
                    else if (dir.Name == "USAA")
                        days = 21;
                    else
                        days = 30;
                    DateTime startDate = Common.Today;
                    try
                    {
                        do
                        {
                            startDate = startDate.AddDays(-1);
                            days -= IsBusinessDay(startDate, Config.Common_Holidays, false, false) ? 1 : 0;
                        } while (0 != days);


                        var subdirs = dir.EnumerateDirectories().Where(d => d.LastWriteTime < startDate).ToList();

                        foreach (var subdir in subdirs)
                        {
                            if (Directory.Exists(Path.Combine(Path.Combine(destAMDir.FullName, dir.Name), subdir.Name)))
                                Directory.Delete(Path.Combine(Path.Combine(destAMDir.FullName, dir.Name), subdir.Name));
                            Directory.Move(subdir.FullName, Path.Combine(Path.Combine(destAMDir.FullName, dir.Name), subdir.Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while PURGING AM ROOT files: {0} ".Fill(ex.Message);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                    }
                }

                var pnsDirs = sourcePnSRootDir.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in pnsDirs)
                {
                    if (!Directory.Exists(Path.Combine(destPCIDir.FullName, dir.Name)))
                        Directory.CreateDirectory(Path.Combine(destPCIDir.FullName, dir.Name));

                    int days = 0;
                    if (dir.Name == "ALASKA")
                        days = 30;
                    else if (dir.Name == "BOTW")
                        days = 30;
                    else if (dir.Name == "CITIVM")
                        days = 30;
                    else if (dir.Name == "HSBC")
                        days = 30;
                    else if (dir.Name == "RPS")
                        days = 60;
                    else if (dir.Name == "USAA")
                        days = 21;
                    else
                        days = 30;
                    DateTime startDate = Common.Today;
                    try
                    {
                        do
                        {
                            startDate = startDate.AddDays(-1);
                            days -= IsBusinessDay(startDate, Config.Common_Holidays, false, false) ? 1 : 0;
                        } while (0 != days);

                        var subdirs = dir.EnumerateDirectories().Where(d => d.LastWriteTime < startDate).ToList();

                        foreach (var subdir in subdirs)
                        {
                            if (Directory.Exists(Path.Combine(Path.Combine(destPCIDir.FullName, dir.Name), subdir.Name)))
                                Directory.Delete(Path.Combine(Path.Combine(destPCIDir.FullName, dir.Name), subdir.Name));
                            Directory.Move(subdir.FullName, Path.Combine(Path.Combine(destPCIDir.FullName, dir.Name), subdir.Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while PURGING PCI Root files: {0} ".Fill(ex.Message);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                    }
                }
            }
        }
       
        private static bool IsBusinessDay(DateTime date, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(date.Date))
                return false;

            if (date.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (date.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }

        public static void DeleteFiles()
        {
            foreach (DirectoryInfo amDir in destAMDir.GetDirectories())
            {
                amDir.Delete(true);
            }
            foreach (DirectoryInfo pciDir in destPCIDir.GetDirectories())
            {
                pciDir.Delete(true);
            }
        }
    }
}
