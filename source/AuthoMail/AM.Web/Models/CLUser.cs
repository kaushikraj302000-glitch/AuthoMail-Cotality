using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class CLUser
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public string emailAddress { get; set; }
        public bool enabled { get; set; }
        public string phoneNumber { get; set; }
        public int createdDate { get; set; }
        public string identityProvider { get; set; }
        public string applicationCode { get; set; }
    }
}