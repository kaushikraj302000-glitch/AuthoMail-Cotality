using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IReCalculateDueDateService : IDisposable
    {
        List<Recal> GetDocTypeTest();
        List<Recal> GetCalcListOfPrepStartTime();
        List<Sweep> GetSweepTimings();
        int UpdateDueDateAndEftRecOnEST(int Id, DateTime dueDate, DateTime effectiveReceivedOnEst);
        int UpdateStdPrepStartTime(int id, DateTime stdPrepStartTime);
    }
    public class ReCalculateDueDateService : ServiceBase, IReCalculateDueDateService
    {

        public List<Recal> GetDocTypeTest()
        {
            string q = "select m.Id,m.DocTypeId,m.DueDate,b.ReceivedOn from AM.MailPieces m inner join AM.MailBatches b on m.MailBatchId = b.Id where m.DocTypeId is not null and m.ShippingStatus <> 'Error' and b.clientid = 101 order by m.id";
            return db.Query<Recal>(q).ToList();
        }


        public int UpdateDueDateAndEftRecOnEST(int Id, DateTime dueDate, DateTime effectiveReceivedOnEst)
        {
            string q = "update AM.MailPieces set DueDate=@dueDate,EffectiveReceivedOn=@effectiveReceivedOnEst where Id=@id";
            return db.Execute(q, new { id = Id, dueDate = dueDate, effectiveReceivedOnEst = effectiveReceivedOnEst });
        }

        public List<Recal> GetCalcListOfPrepStartTime()
        {
            string q = "select M.Id,B.ReceivedOn from AM.MailPieces M inner join AM.MailBatches B on M.MailbatchId = B.Id Where B.ReceivedOn > '9/13/2018' AND DueDate IS NOT NULL and M.PrintLocation = 'Fort Mill, SC'";
            return db.Query<Recal>(q).ToList();
        }

        public List<Sweep> GetSweepTimings()
        {
            return db.Query<Sweep>("select * from ufn_GetPCIBundleSweepTimings()").ToList();
        }

        public int UpdateStdPrepStartTime(int id, DateTime stdPrepStartTime)
        {
            string q = "UPDATE AM.MailPieces SET StdPrepStartTime = @stdPrepStartTime FROM AM.MailPieces  WHERE Id = @Id and DueDate IS NOT NULL AND PrintLocation = 'Fort Mill, SC'";
            return db.Execute(q, new { Id = id, stdPrepStartTime = stdPrepStartTime });
        }
    }
    public class Recal
    {
        public int Id { get; set; }
        public int DocTypeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime ReceivedOn { get; set; }
    }


}
