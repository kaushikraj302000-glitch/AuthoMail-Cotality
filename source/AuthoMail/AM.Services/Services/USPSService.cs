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
        Task<VerifierAddressResponse> VerifyAddressAsync(string userId, VerifierAddressRequest request);

        Task<eVSLabelResponse> RequesteVSLabelAsync(string userId, eVSLabelRequest request);

        Task<eVSCancelLabelResponse> RequesteVSCancelLabelAsync(string userId, eVSCancelLabelRequest request);
    }
}
