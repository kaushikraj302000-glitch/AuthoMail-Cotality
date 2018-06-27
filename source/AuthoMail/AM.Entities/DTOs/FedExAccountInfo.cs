using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class FedExAccountInfo
    {
        public string AccountCode { get; set; }
        public string WebAuthenticationKey { get; set; }
        public string WebAuthenticationPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
    }

    public class Holiday
    {
        public int Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; }
    }
}
