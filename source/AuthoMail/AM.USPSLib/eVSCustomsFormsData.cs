using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AM.USPSLib
{
    public class eVSCustomsFormsData : IeVSCustomsFormsData
    {
        public List<ItemDetail> ShippingContents { get; set; }
    }
}
