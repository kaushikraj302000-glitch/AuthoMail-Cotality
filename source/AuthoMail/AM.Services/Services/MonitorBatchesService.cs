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
        List<dynamic> GetPendingBatches();
        void SaveEmailSentTimeStamp(int printBatch);
        int CheckPrintBatchIdExist(int printBatch);
        int CheckLastEmailSentTime(int printBatch);
        void UpdateEmailTimeStamp(int printBatch);
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public List<dynamic> GetPendingBatches()
        {
            return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.JobName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn FROM PnS.BatchSummary PB INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE IsSubmitted = 0 AND DATEDIFF(minute, ReceivedOn, GETUTCDATE()) > 15").ToList();
        }

        public void SaveEmailSentTimeStamp(int printBatch)
        {
            db.Execute("INSERT INTO [Common].[MonitorBatches] ([PrintBatch],[Email_TimeStamp]) VALUES (@PrintBatch,@Email_TimeStamp)", new { @PrintBatch = printBatch, @Email_TimeStamp = DateTime.UtcNow });
        }
        public int CheckPrintBatchIdExist(int printBatch)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT Count(PrintBatch) FROM Common.MonitorBatches WHERE PrintBatch = @PrintBatch", new { @PrintBatch = printBatch }).ToString());
        }
        public int CheckLastEmailSentTime(int printBatch)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT DATEDIFF(hour, Email_TimeStamp, GETUTCDATE()) AS EmailTimeStamp FROM Common.MonitorBatches WHERE PrintBatch = @PrintBatch", new { @PrintBatch = printBatch }).ToString());
        }
        public void UpdateEmailTimeStamp(int printBatch)
        {
            db.Execute("UPDATE Common.MonitorBatches SET Email_TimeStamp = GETUTCDATE() WHERE PrintBatch = @PrintBatch", new { @PrintBatch = printBatch });
        }
    }
}
