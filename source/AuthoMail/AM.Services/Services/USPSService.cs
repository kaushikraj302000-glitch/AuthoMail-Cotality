using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.USPSLib;

namespace AM.Services
{
    public interface IUSPSService : IDisposable
    {
        VerifierAddressResponse VerifyAddress(string userId, VerifierAddressRequest request);

        eVSLabelResponse RequesteVSLabel(string userId, eVSLabelRequest request);
    }
}
