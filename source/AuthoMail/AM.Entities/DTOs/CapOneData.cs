using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class CapOneData
    {
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string BorrowerStreetAddress { get; set; }
        public string BorrowerCity { get; set; }
        public string BorrowerState { get; set; }
        public string BorrowerZip { get; set; }
        public DateTime DocumentDate { get; set; }
        public string PropertyStreetAddress { get; set; }
        public string PropertyCity { get; set; }
        public string PropertyState { get; set; }
        public string PropertyZip { get; set; }
        public string CommunityName { get; set; }
        public string LoanNumber { get; set; }
        public DateTime NoticeDate { get; set; }
        public string ClientCode { get; set; }
        public string CommunityCode { get; set; }
    }

    public class CustomTemplate
    {
        public int ClientId { get; set; }
        public string CustomTemplatePath { get; set; }
        public string CommunityCode { get; set; }
    }
}
