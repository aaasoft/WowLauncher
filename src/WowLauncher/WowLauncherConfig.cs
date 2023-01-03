using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowLauncher
{
    public class WowLauncherConfig
    {
        public string ServerHost { get; set; } = "127.0.0.1";
        public int[] ProxyPorts { get; set; } = new[] { 3724, 8085 };
    }
}
