using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services;
using AM.Lib;

namespace AM.Services
{
    public interface ISupportReportService : IDisposable
    {
        List<NotReportedBackMailPieces> GetNotReportedBackMailPieces(int clientId, string printLocation, DateTime date);
    }
    public class SupportReportService : ServiceBase, ISupportReportService
    {
        public List<NotReportedBackMailPieces> GetNotReportedBackMailPieces(int clientId, string printLocation, DateTime date)
        {
            date = clientId == 104 ? date.Date + new TimeSpan(20, 00, 00) : date.Date + new TimeSpan(20, 15, 00);
            date = date.EstToUtc();
            string query = "select bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode as ClientCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType,Count(m.Id) as TotalPackages,Sum(m.TotalPages) as TotalPages from PnS.MRDFRequest rq inner join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch inner join Common.Clients c on bs.ClientId=c.Id inner join AM.DocTypes d on bs.DocTypeId=d.Id inner join AM.MailPieces m on rq.UniqueId=m.UniqueId where c.Id=@clientId and bs.PrintLocation=ISNULL(@printLocation,bs.PrintLocation) and bs.PipelineStatus not in ('ReportedBack','Cancelled') and m.SubmittedOn <= @date group by bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType";
            return db.Query<NotReportedBackMailPieces>(query,
                new
                {
                    ClientId = clientId,
                    PrintLocation = printLocation,
                    date = date
                }).ToList();
        }
    }
}
