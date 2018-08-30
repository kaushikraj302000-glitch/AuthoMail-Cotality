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
        object GetDetDocTypeTest();
    }
    public class ReCalculateDueDateService : ServiceBase, IReCalculateDueDateService
    {

        public object GetDetDocTypeTest()
        {
            string q = "select m.Id,m.DocTypeId,m.DueDate,b.ReceivedOn from AM.MailPieces m inner join AM.MailBatches b on m.MailBatchId = b.Id where m.DocTypeId is not null";
            return db.Query(q);
        }
    }
}
