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
        private static DirectoryInfo sourceAMRootDir = new DirectoryInfo(Config.Common_AuthoMailRootFolder);
        private static DirectoryInfo destAMDir = new DirectoryInfo(Config.Common_AMTrash);
        private static DirectoryInfo sourcePnSRootDir = new DirectoryInfo(Config.Core_PnS_SFT_RootDir);
        private static DirectoryInfo destPnSDir = new DirectoryInfo(Config.Common_PnSTrash);
        
        public static void StartPurging()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (IHomeService homeSvc = new HomeService())
            using (IClientService clientSvc = new ClientService())
            {
                var clients = homeSvc.GetAllActiveClient();
                foreach (var client in clients)
                {
                    var clientDetails = clientSvc.GetClientByAppCode(client.AppCode).Result;
                    EnumerateAndDeleteDirs(clientDetails, sourceAMRootDir, destAMDir);
                    EnumerateAndDeleteDirs(clientDetails, sourcePnSRootDir, destPnSDir);
                }
            }
        }

        private static void EnumerateAndDeleteDirs(Client client, DirectoryInfo rootDir, DirectoryInfo destDir)
        {
            int days = client.RetentionPeriod;
            var dir = new DirectoryInfo(Path.Combine(rootDir.FullName, client.AppCode));

            if (dir.Exists)
            {
                if (!Directory.Exists(Path.Combine(destDir.FullName, dir.Name)))
                    Directory.CreateDirectory(Path.Combine(destDir.FullName, dir.Name));                                

                DateTime startDate = Common.Today;
                try
                {
                    do
                    {
                        startDate = startDate.AddDays(-1);
                        days -= IsBusinessDay(startDate, Config.GetHolidaysByClientId(client.Id), false, false) ? 1 : 0;
                    } while (0 != days);

                    var subdirs = dir.EnumerateDirectories().Where(d => d.LastWriteTime < startDate).ToList();

                    foreach (var subdir in subdirs)
                    {
                        if (Directory.Exists(Path.Combine(Path.Combine(destDir.FullName, dir.Name), subdir.Name)))
                            Directory.Delete(Path.Combine(Path.Combine(destDir.FullName, dir.Name), subdir.Name));
                        Directory.Move(subdir.FullName, Path.Combine(Path.Combine(destDir.FullName, dir.Name), subdir.Name));
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while PURGING directories and files: {0} ".Fill(ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
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
            foreach (DirectoryInfo pciDir in destPnSDir.GetDirectories())
            {
                pciDir.Delete(true);
            }
        }

        public static void PurgeDBRecords()
        {
            using (IReCalculateDueDateService reCalculateDueDateService = new ReCalculateDueDateService())
            {
                var recordsToUpdateOnUSPS = reCalculateDueDateService.GetUSPSRecordsToPurge();
                try
                {
                    foreach (var record in recordsToUpdateOnUSPS)
                    {
                        reCalculateDueDateService.UpdateUSPSRecords(record.Id);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while PURGING records from [API].[USPS] Table: {0} ".Fill(ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }

                var recordsToUpdateOnUSPSeVS = reCalculateDueDateService.GetUSPSeVSRecordsToPurge();
                try
                {
                    foreach (var record in recordsToUpdateOnUSPSeVS)
                    {
                        reCalculateDueDateService.UpdateUSPSeVSRecords(record.Id);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while PURGING records from [API].[USPSeVS] Table: {0} ".Fill(ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }
            }
        }
    }
}
