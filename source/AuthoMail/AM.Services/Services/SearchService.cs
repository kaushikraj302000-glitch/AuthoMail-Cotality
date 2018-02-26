using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Services
{
    public interface ISearchService
    {
        List<MailPieceService> GetMailPieceByParam(int clientId, string loanNo, DateTime printOn, DateTime shippedOn);
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<MailPieceService> GetMailPieceByParam(int clientId, string loanNo, DateTime printOn, DateTime shippedOn)
        {
            throw new NotImplementedException();
        }
    }
}
