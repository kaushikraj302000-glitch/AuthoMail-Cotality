using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Services
{
    public class DocumentType
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
    }
    public class ChannelCodeTitle
    {
        public string Code { get; set; }
        public string Title { get; set; }
    }
    public class ClientIdName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
