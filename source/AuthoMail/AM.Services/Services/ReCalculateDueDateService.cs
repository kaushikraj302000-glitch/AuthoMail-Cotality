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
        List<Recal> GetDocTypeTestWithErrorMP();
        List<Recal> GetCalcListOfMailPieces();
        List<Sweep> GetSweepTimings();
        int UpdateDueDateAndEftRecOnEST(int Id, DateTime dueDate, DateTime effectiveReceivedOnEst);
        int UpdateEftRecOnEST(int Id, DateTime effectiveReceivedOnEst);
        int UpdateSubmissionTime(int id, DateTime stdSubmissionTime, DateTime maxSubmissionTime, string missedSLAReason);
    }
    public class ReCalculateDueDateService : ServiceBase, IReCalculateDueDateService
    {

        public List<Recal> GetDocTypeTest()
        {
            string q = "select m.Id,m.DocTypeId,m.DueDate,b.ReceivedOn from AM.MailPieces m inner join AM.MailBatches b on m.MailBatchId = b.Id where m.DocTypeId is not null and m.ShippingStatus <> 'Error' order by m.id";
            return db.Query<Recal>(q).ToList();
        }
        public List<Recal> GetDocTypeTestWithErrorMP()
        {
            string q = "select m.Id,m.DocTypeId,b.ReceivedOn,m.EffectiveReceivedOnEst from AM.MailPieces m inner join AM.MailBatches b on m.MailBatchId = b.Id where m.DocTypeId is not null and m.ShippingStatus = 'Error' and m.EffectiveReceivedOnEst IS NULL order by m.id";
            return db.Query<Recal>(q).ToList();
        }

        public int UpdateDueDateAndEftRecOnEST(int Id, DateTime dueDate, DateTime effectiveReceivedOnEst)
        {
            string q = "update AM.MailPieces set DueDate=@dueDate,EffectiveReceivedOnEst=@effectiveReceivedOnEst where Id=@id";
            return db.Execute(q, new { id = Id, dueDate = dueDate, effectiveReceivedOnEst = effectiveReceivedOnEst });
        }
        public int UpdateEftRecOnEST(int Id, DateTime effectiveReceivedOnEst)
        {
            string q = "update AM.MailPieces set EffectiveReceivedOnEst=@effectiveReceivedOnEst where Id=@id";
            return db.Execute(q, new { id = Id, effectiveReceivedOnEst = effectiveReceivedOnEst });
        }

        public List<Recal> GetCalcListOfMailPieces()
        {
            string q = "select M.Id,M.UniqueId,B.ReceivedOn,M.ClientId,C.AppCode,PV.VendorCode,M.SLADays,M.DueDate,M.ShippingStatus,M.DocTypeId,M.PrintLocation,M.ShippedOn,M.EffectiveReceivedOnEst,D.BizdayCutOff,D.SatCutOff,M.MissedSLAReason,M.StdSubmissionTime,M.MaxSubmissionTime from AM.MailPieces M inner join AM.MailBatches B on M.MailbatchId = B.Id inner join AM.DocTypes D on M.DocTypeId = D.Id inner join Common.PrintTrails PT on M.PrintTrailId = PT.Id inner join Common.PrintVendors PV on PT.PrintVendorId = PV.Id inner join Common.Clients C on M.ClientId = C.Id Where DueDate IS NOT NULL and M.EffectiveReceivedOnEst IS NOT NULL and M.PrintLocation is not null";
            return db.Query<Recal>(q).ToList();
        }

        public List<Sweep> GetSweepTimings()
        {
            return db.Query<Sweep>("select * from ufn_GetSweepTimings()").ToList();
        }

        public int UpdateSubmissionTime(int id, DateTime stdSubmissionTime, DateTime maxSubmissionTime, string missedSLAReason)
        {
            string q = "UPDATE AM.MailPieces SET StdSubmissionTime = @stdSubmissionTime,MaxSubmissionTime = @maxSubmissionTime,MissedSLAReason = @missedSLAReason FROM AM.MailPieces WHERE Id = @Id";
            return db.Execute(q, new { Id = id, stdSubmissionTime = stdSubmissionTime, maxSubmissionTime = maxSubmissionTime, missedSLAReason = missedSLAReason });
        }

    }
    public class Recal
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int DocTypeId { get; set; }
        public string AppCode { get; set; }
        public string VendorCode { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string ShippingStatus { get; set; }
        public string PrintLocation { get; set; }
        public DateTime? ShippedOn { get; set; }
        public DateTime? EffectiveReceivedOnEst { get; set; }
        public string BizdayCutOff { get; set; }
        public string SatCutOff { get; set; }
        public string MissedSLAReason { get; set; }
        public DateTime? StdSubmissionTime { get; set; }
        public DateTime? MaxSubmissionTime { get; set; }
    }

}
