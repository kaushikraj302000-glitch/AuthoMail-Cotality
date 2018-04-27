using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IMonitorBatchesService : IDisposable
    {
        List<dynamic> GetPendingBatchesToSubmit();
        List<dynamic> GetPendingBatchesToReportBack();
        void SaveEmailSentTimeStamp(int printBatch, bool IsSubmitted);
        int CheckPrintBatchIdExist(int printBatch, bool IsSubmitted);
        int CheckLastEmailSentTime(int printBatch, bool IsSubmitted);
        void UpdateEmailTimeStamp(int printBatch, bool IsSubmitted);
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public List<dynamic> GetPendingBatchesToSubmit()
        {
            return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.JobName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn FROM PnS.BatchSummary PB INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE IsSubmitted = 0 AND DATEDIFF(minute, ReceivedOn, GETUTCDATE()) > 15").ToList();
        }

        public List<dynamic> GetPendingBatchesToReportBack()
        {
            return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.JobName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn,PB.SubmittedOn FROM PnS.BatchSummary PB LEFT OUTER JOIN AM.PrintBatches AP ON AP.UniqueId = PB.BatchIdentifier INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE PB.IsSubmitted = 1 AND (PB.IsReportedBack = 0 OR AP.IsShippingResponseReceived = 0) AND DATEDIFF(HOUR, ReceivedOn, GETUTCDATE()) > 5").ToList();
        }

        public void SaveEmailSentTimeStamp(int printBatch, bool IsSubmitted)
        {
            db.Execute("INSERT INTO [Common].[MonitorBatches] ([PrintBatch],[Email_TimeStamp],[IsSubmitted]) VALUES (@PrintBatch,@Email_TimeStamp,@IsSubmitted)", new { @PrintBatch = printBatch, @Email_TimeStamp = DateTime.UtcNow, @IsSubmitted = IsSubmitted });
        }
        public int CheckPrintBatchIdExist(int printBatch, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT Count(PrintBatch) FROM Common.MonitorBatches WHERE PrintBatch = @PrintBatch AND IsSubmitted = @IsSubmitted", new { @PrintBatch = printBatch, @IsSubmitted = IsSubmitted }).ToString());
        }
        public int CheckLastEmailSentTime(int printBatch, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT DATEDIFF(hour, Email_TimeStamp, GETUTCDATE()) AS EmailTimeStamp FROM Common.MonitorBatches WHERE PrintBatch = @PrintBatch AND IsSubmitted = @IsSubmitted", new { @PrintBatch = printBatch, @IsSubmitted = IsSubmitted }).ToString());
        }
        public void UpdateEmailTimeStamp(int printBatch, bool IsSubmitted)
        {
            db.Execute("UPDATE Common.MonitorBatches SET Email_TimeStamp = GETUTCDATE(),IsSubmitted = @IsSubmitted WHERE PrintBatch = @PrintBatch", new { @PrintBatch = printBatch, @IsSubmitted = IsSubmitted });
        }
    }
}
