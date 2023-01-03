using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowLauncher
{
    public class PortalConfig
    {
        public string LocalIPAddress { get; set; }
        public int LocalListenPort { get; set; }
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public Action<string> Logger { get; set; }
    }
}
