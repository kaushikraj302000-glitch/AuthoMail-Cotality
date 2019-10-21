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
        int UpdateEDeliveryPipelineStatus(int batchId, string pipeLineStatus, DateTime timestamp);
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
        DateTime GetMaximumSubmissionTime(DocType docType, DateTime effectiveReceivedOnEst);
        int GetPrintTrail(DocType docType, DateTime estEffectiveReceivedOn);
        DateTime GetDueDate(DocType docType, DateTime estEffectiveReceivedOn);
        DateTime GetEffectiveEstReceivedOn(DocType docType, DateTime utcReceivedOn);
        DateTime GetNextWorkingDay(DateTime estDateTime, List<Holiday> HolidayList, bool isSatWorkDay, bool isSunWorkDay);
        int InsertErrorNotifications(int mailBatchId);
        string GetErrorNotificationEmailSubscriberList(int clientId);
        void GetUpdatedDueDateAndPrintLocation(DateTime receivedOn, MailPieceDTO mailPiece, DocType docType, string appCode);
        int GetReportingSequence();
    }
    public class CommonService : ServiceBase, ICommonService //It implements Interfaces 
    {

        public int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            string query;
            if (pipeLineStatus == BatchPipelineStatus.SubmittedToEDelivery)//System will move pipelinestatus to SubmittedToEDelivery to exclude it from regular print and ship process.
            {
                query = "UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, ValidatedOn = @timestamp WHERE Id = @batchId";
            }
            else
                query = "UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, {0}On = @timestamp WHERE Id = @batchId".Fill(pipeLineStatus);

            return db.Execute(query, new { pipeLineStatus = pipeLineStatus, timestamp = timestamp, batchId = batchId });
        }

        public int UpdateEDeliveryPipelineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = @pipelineStatus, {0}On = @timestamp WHERE Id = @batchId".Fill(pipeLineStatus);

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
            string query = "select pv.*,pt.PrintLocation from [Common].[PrintTrails] as pt inner join  [Common].[PrintVendors] as pv on pt.PrintVendorId = pv.Id where pt.Id = @printTrailId";
            return db.QuerySingleOrDefault<PrintVendor>(query, new { printTrailId = printTrailId });
        }

        public DateTime GetMaximumSubmissionTime(DocType docType, DateTime effectiveReceivedOnEst)
        {
            DayOfWeek dayOfWeek = effectiveReceivedOnEst.DayOfWeek;

            string cutOffTime = string.Empty;

            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    cutOffTime = docType.SatCutOff;
                    break;
                case DayOfWeek.Sunday:
                    cutOffTime = docType.SunCutOff;
                    break;
                default:
                    cutOffTime = docType.BizdayCutOff;
                    break;
            }

            var effectiveReceivedOnEstDate = effectiveReceivedOnEst.Date;
            var maxSubmissionDateTime = effectiveReceivedOnEstDate + TimeSpan.Parse(cutOffTime);

            return maxSubmissionDateTime.EstToUtc();
        }

        public int GetPrintTrail(DocType docType, DateTime estEffectiveReceivedOn)
        {
            DocTypePrintTrailMap docTypePrintTrail;

            using (ICommonService commonSvc = new CommonService())
            {
                List<DocTypePrintTrailMap> docTypePrintTrailMaps = commonSvc.GetTrailMapDetailsByDocTypeId(docType.Id);
                var dayofWeek = estEffectiveReceivedOn.DayOfWeek;

                if (dayofWeek == DayOfWeek.Saturday)
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.SatStartTime && estEffectiveReceivedOn.TimeOfDay <= c.SatEndTime);
                }
                else if (dayofWeek == DayOfWeek.Sunday)
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.SunStartTime && estEffectiveReceivedOn.TimeOfDay <= c.SunEndTime);
                }
                else
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.BizDayStartTime && estEffectiveReceivedOn.TimeOfDay <= c.BizDayEndTime);
                }
            }

            return docTypePrintTrail.PrintTrailId;
        }

        public DateTime GetDueDate(DocType docType, DateTime estEffectiveReceivedOn)
        {
            bool isSatWorkDay = docType.SatSLAType == SLAType.NA ? false : true;
            bool isSunWorkDay = docType.SunSLAType == SLAType.NA ? false : true;

            var estEffectiveReceivedOnDate = estEffectiveReceivedOn.Date;

            DayOfWeek dayOfWeek = estEffectiveReceivedOnDate.DayOfWeek;

            string slaTimeSpan = string.Empty;
            string slaType = string.Empty;

            DateTime dueDate = estEffectiveReceivedOn;

            if (dayOfWeek == DayOfWeek.Saturday)
            {
                slaTimeSpan = docType.SatTimeSpan;
                slaType = docType.SatSLAType;
            }

            if (dayOfWeek == DayOfWeek.Sunday)
            {
                slaTimeSpan = docType.SunTimeSpan;
                slaType = docType.SunSLAType;
            }

            if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
            {
                slaTimeSpan = docType.BizdayTimeSpan;
                slaType = docType.BizdaySLAType;
            }

            TimeSpan timeSpan = TimeSpan.Parse(slaTimeSpan);
            if (slaType == SLAType.FixedTime)
            {
                //Here we are taking only time from full time span which includes dates
                dueDate = estEffectiveReceivedOnDate + TimeSpan.Parse(string.Format("{0:hh\\:mm\\:ss}", timeSpan));

                for (int i = 0; i < timeSpan.Days; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
                }
            }
            else
            {
                dueDate = estEffectiveReceivedOn + TimeSpan.Parse(string.Format("{0:hh\\:mm\\:ss}", timeSpan));
                for (int i = 0; i < timeSpan.Days; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
                }
            }


            return dueDate.EstToUtc();
        }

        public DateTime GetEffectiveEstReceivedOn(DocType docType, DateTime utcReceivedOn)
        {
            var estReceivedOn = utcReceivedOn.UtcToEst();
            var estReceivedOnDate = estReceivedOn.Date;
            var effectiveEstReceivedOn = estReceivedOn;

            bool isSatWorkDay = docType.SatSLAType == SLAType.NA ? false : true;
            bool isSunWorkDay = docType.SunSLAType == SLAType.NA ? false : true;

            if (IsBusinessDay(estReceivedOn, Config.Common_Holidays, isSatWorkDay, isSunWorkDay))
            {
                DayOfWeek dayOfWeek = estReceivedOn.DayOfWeek;

                string slaCutOff = string.Empty;
                string slaType = string.Empty;

                if (dayOfWeek == DayOfWeek.Saturday)
                {
                    slaCutOff = docType.SatCutOff;
                    slaType = docType.SatSLAType;
                }

                if (dayOfWeek == DayOfWeek.Sunday)
                {
                    slaCutOff = docType.SunCutOff;
                    slaType = docType.SunSLAType;
                }

                if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
                {
                    slaCutOff = docType.BizdayCutOff;
                    slaType = docType.BizdaySLAType;
                }

                if (slaType == SLAType.FixedTime)
                {
                    var estSLACutOffDateTime = (estReceivedOnDate + TimeSpan.Parse(slaCutOff));

                    if (estReceivedOn > estSLACutOffDateTime)
                    {
                        effectiveEstReceivedOn = GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
                    }
                }
                else
                {
                    effectiveEstReceivedOn = estReceivedOn;
                }
            }
            else
            {
                effectiveEstReceivedOn = GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
            }

            return effectiveEstReceivedOn;
        }

        public bool IsBusinessDay(DateTime dueDate, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(dueDate.Date))
                return false;

            if (dueDate.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (dueDate.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }

        public DateTime GetNextWorkingDay(DateTime estDateTime, List<Holiday> HolidayList, bool isSatWorkDay, bool isSunWorkDay)
        {
            do
            {
                estDateTime = estDateTime.AddDays(1);
            }
            while (!IsBusinessDay(estDateTime, HolidayList, isSatWorkDay, isSunWorkDay));

            return estDateTime;
        }

        public int InsertErrorNotifications(int mailBatchId)
        {
            return db.Execute("usp_InsertErrorNotificationData @mailBatchId", new { mailBatchId = mailBatchId });
        }

        public string GetErrorNotificationEmailSubscriberList(int clientId)
        {
            return String.Join(",", db.Query<string>("SELECT U.Email FROM [AM].[AlertSubscribers] A INNER JOIN [Profile].[Users] U ON A.UserId=U.Id WHERE U.ClientId=@ClientId", new { ClientId = clientId }));
        }

        public void GetUpdatedDueDateAndPrintLocation(DateTime receivedOn, MailPieceDTO mailPiece, DocType docType, string appCode)
        {
            mailPiece.DocTypeId = docType.Id;
            mailPiece.EffectiveReceivedOnEst = GetEffectiveEstReceivedOn(docType, receivedOn);
            //Calculate Due Date for Mail Pieces doc type wise
            mailPiece.DueDate = GetDueDate(docType, mailPiece.EffectiveReceivedOnEst.Value);
            //Set print location where mail piece will go for print and ship
            mailPiece.PrintTrailId = GetPrintTrail(docType, mailPiece.EffectiveReceivedOnEst.Value);
            var printVendorDetails = GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value);
            mailPiece.PrintLocation = printVendorDetails.PrintLocation;
            mailPiece.StdSubmissionTime = GetStandardSubmissionTime(appCode, printVendorDetails.VendorCode, receivedOn, GetSweepTimings());
            mailPiece.MaxSubmissionTime = GetMaximumSubmissionTime(docType, mailPiece.EffectiveReceivedOnEst.Value);

        }

        public int GetReportingSequence()
        {
            string query = "SELECT NEXT VALUE FOR dbo.ReportingSequence";
            return db.ExecuteScalar<int>(query);
        }
    }
}
