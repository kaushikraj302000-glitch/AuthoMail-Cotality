using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public interface IUSPS
    {
        [Post("/api/usps/verify")]
        [Headers("Authorization: Bearer")]
        Task<VerifierAddressResponse> VerifyAddress([Body(BodySerializationMethod.Json)] VerifierAddressRequest request);

        [Post("/api/usps/eVS")]
        [Headers("Authorization: Bearer")]
        Task<eVSLabelResponse> RequesteVSLabel([Body(BodySerializationMethod.Json)] eVSLabelRequest request);
    }
}
