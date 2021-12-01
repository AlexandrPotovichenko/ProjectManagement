using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Options
{
    public class ClamAVServerOptions
    {
        public const string ClamAVServer = "ClamAVServer";
        public string URL { get; set; } = String.Empty;
        public string Port { get; set; } = String.Empty;
    }
}
