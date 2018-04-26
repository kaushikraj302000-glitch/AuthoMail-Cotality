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
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public List<dynamic> GetPendingBatches()
        {
            return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.JobName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn FROM PnS.BatchSummary PB INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE IsSubmitted = 0 AND DATEDIFF(minute, ReceivedOn, GETUTCDATE()) > 15").ToList();
        }
    }
}
