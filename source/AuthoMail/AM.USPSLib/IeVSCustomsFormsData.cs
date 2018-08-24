using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    //Todo take it from below
    //Source: 2.1.3 eVS Customs Forms and Data Requirements 
    public interface IeVSCustomsFormsData
    {
        List<ItemDetail> ShippingContents { get; set; }
    }
}
