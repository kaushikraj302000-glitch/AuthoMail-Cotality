using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Lib;
using AM.Entities;

namespace AM.Services
{
    public interface ICommonService : IDisposable
    {
        int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp);
        int UpdateBatchStatus(int batchId, string batchStatus);
        int UpdateBatchValidationStatus(int batchId, string validationStatus, string validationErrors);
        int AMLogBatchTimeLine(int batchId, string activityName, DateTime timeStamp);
        int UpdatePrintBatchStatus(int printBatchId, string batchStatus, string statusDescription = null);
        int UpdatePrintBatchPipeLineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp);
        ClientBusinessIdentifier GetBusinessIdNameFromClientId(int clientId);
        ClientBusinessIdentifier CheckBusinessIdNameAlreadyExist(int clientId, string businessIdName);
        int InsertBusinessIdentifierName(int clientId, string businessIdName);
        List<Deliverable> GetDeliverablesByClientId(int clientId);
        Deliverable GetDeliverableByReportType(int clientId, string reportType);
        List<Holiday> GetHolidayList();
        string GetSystemUserId();
        List<DallasBatch> GetDallasBatches(DateTime estToday);
        DateTime GetStandardSubmissionTime(string appCode, string vendorCode, DateTime receivedOn, List<Sweep> sweepTimings);
        List<Sweep> GetSweepTimings();
        List<DocTypePrintTrailMap> GetTrailMapDetailsByDocTypeId(int docTypeId);
        PrintVendor GetPrintVendorDetailsByPrintTrailId(int printTrailId);
    }
    public class CommonService : ServiceBase, ICommonService //It implements Interfaces 
    {

        public int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, {0}On = @timestamp WHERE Id = @batchId".Fill(pipeLineStatus);

            return db.Execute(query, new { pipeLineStatus = pipeLineStatus, timestamp = timestamp, batchId = batchId });
        }

        public int UpdateBatchStatus(int batchId, string batchStatus)
        {
            string query = "UPDATE [AM].[MailBatches] SET BatchStatus = @batchStatus WHERE Id = @batchId";

            return db.Execute(query, new { batchStatus = batchStatus, batchId = batchId });
        }

        public int UpdateBatchValidationStatus(int batchId, string validationStatus, string validationErrors)
        {
            string query = "UPDATE [AM].[MailBatches] SET ValidationStatus = @validationStatus, ValidationErrors=@validationErrors WHERE Id = @batchId";

            return db.Execute(query, new { batchId = batchId, validationStatus = validationStatus, validationErrors = validationErrors });
        }

        public int AMLogBatchTimeLine(int batchId, string activityName, DateTime timeStamp)
        {
            return db.Execute("usp_AMLogBatchTimeLine", new { activityName = activityName, batchId = batchId, timeStamp = timeStamp }, commandType: CommandType.StoredProcedure);
        }

        public int UpdatePrintBatchStatus(int printBatchId, string batchStatus, string statusDescription = null)
        {
            string query = "UPDATE [AM].[PrintBatches] SET BatchStatus = @batchStatus,StatusDescription=@statusDescription WHERE Id = @printBatchId";

            return db.Execute(query, new { batchStatus = batchStatus, printBatchId = printBatchId, statusDescription = statusDescription });
        }

        public int UpdatePrintBatchPipeLineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[PrintBatches] SET {0}On = @timestamp WHERE Id = @printBatchId".Fill(pipeLineStatus);

            return db.Execute(query, new { timestamp = timestamp, printBatchId = printBatchId });
        }

        public ClientBusinessIdentifier GetBusinessIdNameFromClientId(int clientId)
        {
            string query = "SELECT * FROM [AM].[ClientBusinessIdentifiers] WHERE ClientId= @clientId ";

            return db.QuerySingleOrDefault<ClientBusinessIdentifier>(query, new { clientId = clientId });
        }

        public ClientBusinessIdentifier CheckBusinessIdNameAlreadyExist(int clientId, string businessIdName)
        {
            string query = "SELECT * FROM [AM].[ClientBusinessIdentifiers] WHERE ClientId= @clientId AND [BusinessIdName]=@BusinessIdName";

            return db.QuerySingleOrDefault<ClientBusinessIdentifier>(query, new { clientId = clientId, BusinessIdName = businessIdName });
        }

        public int InsertBusinessIdentifierName(int clientId, string businessIdName)
        {
            string query = "INSERT INTO [AM].[ClientBusinessIdentifiers] ( [ClientId], [BusinessIdName] ) VALUES (@ClientId, @BusinessIdName)";

            return db.Execute(query, new { ClientId = clientId, BusinessIdName = businessIdName });
        }

        public List<Deliverable> GetDeliverablesByClientId(int clientId)
        {

            string query = "[dbo].[usp_GetDeliverableByReportType] @ClientId, @ReportType";

            var data = db.Query<Deliverable, EmailChannel, Deliverable>(query,
               (d, e) =>
               {
                   d.EmailChannel = e;
                   return d;
               },
               new
               {
                   ClientId = clientId,
                   ReportType = string.Empty
               }
               );
            return data.ToList();
        }

        public Deliverable GetDeliverableByReportType(int clientId, string reportType)
        {
            string query = "[dbo].[usp_GetDeliverableByReportType] @ClientId, @ReportType";

            var data = db.Query<Deliverable, EmailChannel, Deliverable>(query,
               (d, e) =>
               {
                   d.EmailChannel = e;
                   return d;
               },
               new
               {
                   ClientId = clientId,
                   ReportType = reportType
               }
               );
            return data.SingleOrDefault();
        }

        public List<Holiday> GetHolidayList()
        {
            string query = "SELECT * FROM [Common].[Holidays]";

            return db.Query<Holiday>(query, null).ToList();
        }

        public string GetSystemUserId()
        {
            string q = "select Id from [Profile].[Users] where email = @email";
            return db.QuerySingle<string>(q, new { email = "system@milestonetechnoconsulting.com" });
        }

        public List<DallasBatch> GetDallasBatches(DateTime estToday)
        {
            string query = "Select * from [dbo].[ufn_GetDallasBatches](@EstToday)";

            return db.Query<DallasBatch>(query, new { EstToday = estToday }).ToList();
        }

        public DateTime GetStandardSubmissionTime(string appCode, string vendorCode, DateTime receivedOn, List<Sweep> sweepTimings)
        {
            var revisedReceivedOnEst = receivedOn.UtcToEst();
            var weekDayType = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Saturday ? "Saturday" : "Weekday";
            var continuousSweeps = sweepTimings.Where(s => s.AppCode == appCode && s.VendorCode == vendorCode && s.WeekDayType == weekDayType && s.SweepType == "Continuous").ToList();


            if (continuousSweeps.Count > 0)
                return receivedOn;

            var bundlingSweeps = sweepTimings.Where(s => s.AppCode == appCode && s.VendorCode == vendorCode && s.WeekDayType == weekDayType && s.SweepType == "OnSchedule").OrderBy(t => t.SrNo).ToList();
            if (bundlingSweeps.Count > 0)
            {
                // adjust for sunday
                revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1).Date : revisedReceivedOnEst;

                var applicableSweep = bundlingSweeps.FirstOrDefault(s => s.SweepTime >= revisedReceivedOnEst.TimeOfDay);

                if (applicableSweep == null)
                {
                    revisedReceivedOnEst = revisedReceivedOnEst.AddDays(1);
                    // adjust for sunday
                    revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1).Date : revisedReceivedOnEst;
                    applicableSweep = bundlingSweeps.First();
                }

                var standardSubmissionTime = (revisedReceivedOnEst.Date + applicableSweep.SweepTime).EstToUtc();

                return standardSubmissionTime;
            }
            else
            {
                throw new Exception(string.Format("Sweep timings are not defined for AppCode: {0}, VendorCode: {1}", appCode, vendorCode));
            }
        }

        public List<Sweep> GetSweepTimings()
        {
            return db.Query<Sweep>("SELECT * from ufn_GetSweepTimings()").ToList();
        }

        public List<DocTypePrintTrailMap> GetTrailMapDetailsByDocTypeId(int docTypeId)
        {
            string query = "select * from [Common].[DocTypePrintTrailMaps] where DocTypeId = @docTypeId";
            return db.Query<DocTypePrintTrailMap>(query, new { docTypeId = docTypeId }).ToList();
        }

        public PrintVendor GetPrintVendorDetailsByPrintTrailId(int printTrailId)
        {
            string query = "select pv.* from [Common].[PrintTrails] as pt inner join  [Common].[PrintVendors] as pv on pt.PrintVendorId = pv.Id where pt.Id = @printTrailId";
            return db.QuerySingleOrDefault<PrintVendor>(query, new { printTrailId = printTrailId });
        }
    }
}
