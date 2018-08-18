using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IUSPSService : IDisposable
    {
        List<AddressResponse> VerifyAddress(string userId, List<AddressRequest> requests);
        AddressResponse VerifyAddress(string userId, AddressRequest request);
    }
}
