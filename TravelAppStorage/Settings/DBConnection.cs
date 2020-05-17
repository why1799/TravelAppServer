using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelAppStorage.Settings
{
    public class DBConnection
    {
        public string StringDBConnection { get; set; }
        public string DataBaseName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
