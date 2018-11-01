using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public class Client
    {
        public int Id { get; set; }
        public string AppCode { get; set; }
        public string Name { get; set; }
        public bool IsPnSActive { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int BUId { get; set; }
        public string BusinessUnit { get; set; }
        public string CustCode { get; set; }
    }
}
