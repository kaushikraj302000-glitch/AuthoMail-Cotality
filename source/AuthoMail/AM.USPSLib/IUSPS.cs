using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public interface IUSPS
    {
        VerifierAddressResponse VerifyAddress(VerifierAddressRequest request);

        eVSLabelResponse RequesteVSLabel(eVSLabelRequest request);
    }
}
