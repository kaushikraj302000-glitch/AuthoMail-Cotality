using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Services
{
    public interface IPnSAPIService : IDisposable
    {
        Task<bool> AddPackage(string id, DateTime receivedOn);
        Task UpdatePackageStatus(PerfLog perfLog);
    }

    public class PnSAPIService : ServiceBase, IPnSAPIService
    {
        public PnSAPIService() : base("AMAPI") { }

        public async Task<bool> AddPackage(string id, DateTime receivedOn)
        {
            await db.OpenAsync();

            using (var t = db.BeginTransaction())
            {
                // check if the package already received

                // if yes, return false and log request

                // if no, add package and return true

                t.Commit();
            }

            db.Close();

            return true;
        }

        public Task UpdatePackageStatus(PerfLog perfLog)
        {
            throw new NotImplementedException();
        }
    }
}
