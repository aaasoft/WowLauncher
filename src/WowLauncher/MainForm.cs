using System.Diagnostics;
using System.Security.AccessControl;
using WowLauncher.Utils;

namespace WowLauncher
{
    public partial class MainForm : Form
    {
        public const string LOCAL_LOOPBACK_IPADDRESS = "127.0.0.1";
        private WowLauncherConfig config;
        private int maxLogLinesCount = 1000;
        private Queue<string> logQueue = new Queue<string>();
        private List<Portal> portalList = new List<Portal>();
        private string gameExeFile = "Wow.exe";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            config = ConfigFileUtils.Load<WowLauncherConfig>();
            if (config == null)
                config = new WowLauncherConfig();
        }


        private void pushLog(string message)
        {
            this.Invoke(new Action(() =>
            {
                logQueue.Enqueue($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {message}");
                while (logQueue.Count > maxLogLinesCount)
                    logQueue.Dequeue();
                txtLogs.Lines = logQueue.ToArray();
                txtLogs.Select(txtLogs.TextLength, 0);
                txtLogs.ScrollToCaret();
            }));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (portalList.Count == 0)
            {
                pushLog("Starting portal...");
                txtServerHost.Enabled = false;
                btnStart.Text = "Stop";
                config.ServerHost = txtServerHost.Text.Trim();
                ConfigFileUtils.Save(config);
                foreach (var port in config.ProxyPorts)
                {
                    var portal = new Portal(new PortalConfig()
                    {
                        LocalIPAddress = LOCAL_LOOPBACK_IPADDRESS,
                        LocalListenPort = port,
                        RemoteHost = config.ServerHost,
                        RemotePort = port,
                        Logger = pushLog
                    });
                    portalList.Add(portal);
                    portal.Start();
                }
                pushLog("Portal started");

                if (File.Exists(gameExeFile))
                {
                    pushLog("Starting game...");
                    var process = Process.Start(gameExeFile);
                    pushLog($"Game started,process id: {process.Id}");
                }
            }
            else
            {
                pushLog("Stoping portal...");
                txtServerHost.Enabled = true;
                btnStart.Text = "Start";
                foreach (var portal in portalList)
                    portal.Stop();
                portalList.Clear();
                pushLog("Portal stoped");
            }
        }
    }
}