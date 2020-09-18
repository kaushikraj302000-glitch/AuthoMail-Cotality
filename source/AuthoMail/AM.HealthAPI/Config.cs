using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AM.HealthAPI
{
    public class Config
    {
        //public Config(string env, IConfiguration configuration)
        //{
        //    ConnectionString = configuration[$"{env}:{nameof(ConnectionString)}"];
        //    AMRootPath = configuration[$"{env}:{nameof(AMRootPath)}"];
        //    AccessCheckPath = configuration[$"{env}:{nameof(AccessCheckPath)}"];
        //}
        public string ConnectionString { get; set; }
        public string AMRootPath { get; set; }
        public long MinFreeMB { get; set; }
        public string AccessCheckPath { get; set; }
    }
}
