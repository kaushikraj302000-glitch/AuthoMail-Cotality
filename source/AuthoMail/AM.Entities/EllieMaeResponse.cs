using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class EllieMaeResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public Error Errors { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
    }
}
